// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.MVVMBase;
using Aml.Editor.Plugin.Contracts;
using Aml.Engine.CAEX;
using Aml.Toolkit.ViewModel;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Aml.Engine.CAEX.CAEX_CLASSModel_TagNames;

namespace Aml.Editor.Plugin.Sandbox.ViewModels
{
    internal class ActiveDocumentViewModel : ViewModelBase
    {
        #region Fields

        /// <summary>
        /// The libraries which are available to be edited
        /// </summary>
        public static readonly AMLLibraryViewModel[] Libraries = new AMLLibraryViewModel[6];

        private const short AR = 5;
        private const short AT = 4;
        private const short IC = 3;
        private const short IH = 0;
        private const short RC = 2;
        private const short SC = 1;
        private static bool inUse = false;

        /// <summary>
        /// <see cref="ActiveHierarchy"/>
        /// </summary>
        private AMLLibraryViewModel _activeHierarchy;

        private CAEXDocument _document;
        private string _filePath;

        #endregion Fields

        static ActiveDocumentViewModel()
        {
            InitLibraries();
        }

        #region Properties

        /// <summary>
        /// Gets the Attribute Tree, defined for the attributes of the selected CAEX object.
        /// </summary>
        /// <value>The attribute type library.</value>
        public static AMLLibraryViewModel AttributeTree
        {
            get => Libraries[AR] as AMLLibraryViewModel;
            set => Libraries[AR] = value;
        }

        /// <summary>
        /// Gets the LibrarViewModel, defined for all CAEX attribute type libraries.
        /// </summary>
        /// <value>The attribute type library.</value>
        public static AMLLibraryViewModel AttributeTypeLib => Libraries[AT];

        /// <summary>
        /// Gets the LibrarViewModel, defined for all CAEX Instance Hierachies.
        /// </summary>
        public static AMLLibraryViewModel InstanceHierarchy => Libraries[IH];

        /// <summary>
        /// Gets the LibrarViewModel, defined for all CAEX InterfaceClass Libraries.
        /// </summary>
        public static AMLLibraryViewModel InterfaceClassLib => Libraries[IC];

        /// <summary>
        /// Gets the LibrarViewModel, defined for all CAEX RoleClass Libraries.
        /// </summary>
        public static AMLLibraryViewModel RoleClassLib => Libraries[RC];

        /// <summary>
        /// Gets the LibrarViewModel, defined for all CAEX SystemUnitClass Libraries.
        /// </summary>
        public static AMLLibraryViewModel SystemUnitClassLib => Libraries[SC];

        /// <summary>
        /// Gets and sets the ActiveHierarchy
        /// </summary>
        public AMLLibraryViewModel ActiveHierarchy
        {
            get => _activeHierarchy;
            set
            {
                if (inUse || _activeHierarchy == value)
                {
                    return;
                }

                // prevent recursive call
                inUse = true;
                if (value != AttributeTree)
                {
                    for (var i = 0; i < AR; i++)
                    {
                        if (Libraries[i] != value)
                        {
                            Libraries[i].IsActive = false;
                            Libraries[i].RaiseNotifySelection = false;
                            Libraries[i].ClearSelections();
                            Libraries[i].RaiseNotifySelection = true;
                        }
                    }
                }

                if (Set(ref _activeHierarchy, value) && _activeHierarchy != null)
                {
                    _activeHierarchy.IsActive = true;
                }
                inUse = false;
            }
        }

        internal static void Activate(string displayName)
        {
            for (int i=0; i < Libraries.Length; i++)
            {
                if (Libraries[i].DisplayName==displayName)
                {
                    Libraries[i].IsActive=true;
                }
                else
                {
                    Libraries[i].IsActive = false;
                }
            }
        }

        public CAEXDocument Document
        {
            get { return _document; }
            set
            {
                if (Set(ref _document, value))
                {
                    if (_document != null)
                    {
                        LoadLibraries();
                    }
                    PropagateFileOpenEventToPlugins(_document?.FilePath, true);
                }
            }
        }

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (Set(ref _filePath, value))
                {
                }
            }
        }

        public MainViewModel MainModel { get; internal set; }

        internal void Close()
        {
            for (var i = 0; i < Libraries.Length; i++)
            {
                Libraries[i].ClearAll();
            }
            Document.Unload();
            Document = null;
        }

        #endregion Properties

        #region Methods

        internal async Task SaveDocument()
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                await SaveDocumentAs();
            }
            else
            {
                await Document.SaveToFileAsync(FilePath, true, new());
            }
        }

        /// <summary>
        /// Propagates the file open event to PlugIns. This method is called by the AutomationML editor if an AML file is opened.
        /// </summary>
        /// <param name="amlFilePath">The AML file path.</param>
        internal void PropagateFileOpenEventToPlugins(string amlFilePath, bool loaded)
        {
            foreach (var plugin in MainModel.Plugins)
            {
                if (plugin.Plugin.IsActive)
                { 
                    if (loaded)
                    { 
                        plugin.Plugin.PublishAutomationMLFileAndObject(amlFilePath,null);
                        if (plugin.Plugin is INotifyAMLDocumentLoad iload && Document != null)
                        {
                            iload.DocumentLoaded(Document);
                        }
                    }

                    plugin.Plugin.ChangeAMLFilePath(amlFilePath);
                }
            }
        }


        internal async Task SaveDocumentAs()
        {            
            var dlg = new SaveFileDialog 
            {
                Filter = "AutomationML Files (.aml)|*.aml", 
            };

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                FilePath = dlg.FileName;

                await Document.SaveToFileAsync(FilePath, true, new());
                PropagateFileOpenEventToPlugins (FilePath, false); 
            }
        }

        internal void SetActiveHierarchy(string arguments)
        {
            switch (arguments)
            {
                case INSTANCEHIERARCHY_STRING:
                    ActiveHierarchy = InstanceHierarchy;
                    break;

                case SYSTEMUNITCLASSLIB_STRING:
                    ActiveHierarchy = SystemUnitClassLib;
                    break;

                case ROLECLASSLIB_STRING:
                    ActiveHierarchy = RoleClassLib;
                    break;

                case INTERFACECLASSLIB_STRING:
                    ActiveHierarchy = InterfaceClassLib;
                    break;

                case ATTRIBUTETYPELIB_STRING:
                    ActiveHierarchy = AttributeTypeLib;
                    break;
            }
        }

        private static void InitLibraries()
        {
            HashSet<string> ihTemplate = new(AMLTreeViewTemplate.CompleteInstanceHierarchyTree);
            _ = ihTemplate.Remove(INTERNALLINK_STRING);

            HashSet<string> sLibTemplate = new(AMLTreeViewTemplate.CompleteSystemUnitClassLibTree);
            _ = sLibTemplate.Remove(INTERNALLINK_STRING);

            Libraries[IH] = (new AMLLibraryViewModel(null, ihTemplate)
            {
                DisplayName = INSTANCEHIERARCHY_STRING
            });
            Libraries[IC] = (new AMLLibraryViewModel(null, AMLTreeViewTemplate.InterfaceClassLibTree)
            {
                DisplayName = INTERFACECLASSLIB_STRING
            });
            Libraries[SC] = (new AMLLibraryViewModel(null, sLibTemplate)
            {
                DisplayName = SYSTEMUNITCLASSLIB_STRING
            });
            Libraries[RC] = (new AMLLibraryViewModel(null, AMLTreeViewTemplate.ExtendedRoleClassLibTree)
            {
                DisplayName = ROLECLASSLIB_STRING
            });
            Libraries[AT] = (new AMLLibraryViewModel(null, AMLTreeViewTemplate.AttributeTypeLibTree)
            {
                DisplayName = ATTRIBUTETYPELIB_STRING
            });

            Libraries[AR] = (new AMLLibraryViewModel(null, AMLTreeViewTemplate.AttributeTypeLibTree)
            {
                DisplayName = ATTRIBUTE_STRING
            });

        }

        private static AMLLibraryViewModel LibraryViewModel(CAEXObject lib) => lib switch
        {
            InstanceHierarchyType when InstanceHierarchy != null => InstanceHierarchy,
            RoleClassLibType when RoleClassLib != null => RoleClassLib,
            SystemUnitClassLibType when SystemUnitClassLib != null => SystemUnitClassLib,
            InterfaceClassLibType when InterfaceClassLib != null => InterfaceClassLib,
            AttributeTypeLibType when AttributeTypeLib != null => AttributeTypeLib,
            _ => null
        };

        private void LoadLibraries()
        {
            for (var i = 0; i < Libraries.Length - 1; i++)
            {
                Libraries[i].SetRoot(Document?.CAEXFile.Node);
                Libraries[i].MainModel = MainModel;
                Libraries[i].Document = this;
            }
        }

        #endregion Methods
    }
}