// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.MVVMBase;
using Aml.Engine.CAEX;
using Aml.Toolkit.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Aml.Engine.CAEX.CAEX_CLASSModel_TagNames;

namespace Aml.Editor.Plugin.MultiView.ViewModels
{
    internal class ActiveDocumentViewModel : ViewModelBase
    {

        #region Fields

        /// <summary>
        /// The libraries which are available to be edited
        /// </summary>
        public static readonly AMLTreeViewModel[] Libraries = new AMLTreeViewModel[5];
        
        private const short AT = 4;
        private const short IC = 3;
        private const short IH = 0;
        private const short RC = 2;
        private const short SC = 1;

        private AMLTreeViewModel? _activeHierarchy;

        private CAEXDocument? _document;

        private string? _filePath;

        #endregion Fields

        #region Constructors

        static ActiveDocumentViewModel()
        {
            InitLibraries();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the LibrarViewModel, defined for all CAEX attribute type libraries.
        /// </summary>
        /// <value>The attribute type library.</value>
        public static AMLTreeViewModel AttributeTypeLib => Libraries[AT];

        /// <summary>
        /// Gets the LibrarViewModel, defined for all CAEX Instance Hierachies.
        /// </summary>
        public static AMLTreeViewModel InstanceHierarchy => Libraries[IH];

        /// <summary>
        /// Gets the LibrarViewModel, defined for all CAEX InterfaceClass Libraries.
        /// </summary>
        public static AMLTreeViewModel InterfaceClassLib => Libraries[IC];

        /// <summary>
        /// Gets the LibrarViewModel, defined for all CAEX RoleClass Libraries.
        /// </summary>
        public static AMLTreeViewModel RoleClassLib => Libraries[RC];

        /// <summary>
        /// Gets the LibrarViewModel, defined for all CAEX SystemUnitClass Libraries.
        /// </summary>
        public static AMLTreeViewModel SystemUnitClassLib => Libraries[SC];

        /// <summary>
        /// Gets and sets the ActiveHierarchy
        /// </summary>
        public AMLTreeViewModel ActiveHierarchy
        {
            get => _activeHierarchy;
            set
            {
                if (_activeHierarchy == value)
                {
                    return;
                }

                for (var i = 0; i < Libraries.Length; i++)
                {
                    if (Libraries[i] != value)
                    {
                        Libraries[i].IsActive = false;
                        Libraries[i].RaiseNotifySelection = false;
                        Libraries[i].ClearSelections();
                        Libraries[i].RaiseNotifySelection = true;
                    }
                }

                if (Set(ref _activeHierarchy, value) && _activeHierarchy != null)
                {
                    _activeHierarchy.IsActive = true;
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

        #endregion Properties

        #region Methods

        internal static string DisplayName(AMLTreeViewModel model)
        {
            if (model.Equals(InstanceHierarchy))
                return INSTANCEHIERARCHY_STRING;
            if (model.Equals(SystemUnitClassLib))
                return SYSTEMUNITCLASSLIB_STRING;
            if (model.Equals(InterfaceClassLib))
                return INTERFACECLASSLIB_STRING;
            if (model.Equals(AttributeTypeLib))
                return ATTRIBUTETYPELIB_STRING;
            if (model.Equals(RoleClassLib))
                return ROLECLASSLIB_STRING;
            return "";
        }

        internal void Close()
        {
            for (var i = 0; i < Libraries.Length; i++)
            {
                Libraries[i].ClearAll();
            }
            Document.Unload();
            Document = null;
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

        internal void Unload()
        {
            foreach (var lib in Libraries)
            {
                lib.ClearAll();
            }
            Document.Unload();
        }
        private static void InitLibraries()
        {
            HashSet<string> ihTemplate = new(AMLTreeViewTemplate.CompleteInstanceHierarchyTree);
            _ = ihTemplate.Remove(INTERNALLINK_STRING);

            HashSet<string> sLibTemplate = new(AMLTreeViewTemplate.CompleteSystemUnitClassLibTree);
            _ = sLibTemplate.Remove(INTERNALLINK_STRING);

            Libraries[IH] = (new AMLTreeViewModel(null, ihTemplate));
            Libraries[IC] = (new AMLTreeViewModel(null, AMLTreeViewTemplate.InterfaceClassLibTree));
            Libraries[SC] = (new AMLTreeViewModel(null, sLibTemplate));
            Libraries[RC] = (new AMLTreeViewModel(null, AMLTreeViewTemplate.ExtendedRoleClassLibTree));
            Libraries[AT] = (new AMLTreeViewModel(null, AMLTreeViewTemplate.AttributeTypeLibTree));
        }

        private void LoadLibraries()
        {
            for (var i = 0; i < Libraries.Length; i++)
            {
                Libraries[i].SetRoot(Document?.CAEXFile.Node);
            }
        }

        #endregion Methods

    }
}