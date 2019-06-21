using Aml.Editor.Plugin.Contracts;
using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Aml.Toolkit.ViewModel;
using GalaSoft.MvvmLight;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using static Aml.Engine.AmlObjects.AutomationMLContainer;

namespace Aml.Editor.Plugin.Sandbox.ViewModel
{
    /// <summary>
    /// The Aml.Editor.Plugin.Sandbox serves as a testbed for the development of AutomationML editor PlugIns.
    /// This Software mimics the behavior of the AutomationML editor loading and activating PlugIns. After successfully testing a PlugIn,
    /// the compiled DLLs can be moved from the Plugins folder of this solution to the installed AutomationML editor Plugins folder.
    /// Please check the content of the Plugins folder for any duplicates. If duplicates exist rename your assemblies.
    /// </summary>
    /// <seealso cref="GalaSoft.MvvmLight.ViewModelBase" />
    public class MainViewModel : ViewModelBase
    {
        #region Private Fields

        /// <summary>
        ///  <see cref="AMLDocumentTreeViewModel"/>
        /// </summary>
        private AMLTreeViewModel _aMLDocumentTreeViewModel;

        /// <summary>
        ///  <see cref="HasPlugins"/>
        /// </summary>
        private bool _hasPlugins;

        /// <summary>
        ///  <see cref="OpenFileCommand"/>
        /// </summary>
        private RelayCommand<object> _OpenFileCommand;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes the singleton instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        static MainViewModel()
        {
            Instance = new MainViewModel();
        }

        #endregion Public Constructors

        #region Private Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="MainViewModel"/> class from being created.
        /// </summary>
        private MainViewModel()
        {
            Plugins = new ObservableCollection<IAMLEditorPlugin>();

            GenerateSomeAutomationMLTestData("My test hierarchy");
            BuildTreeViewModel();
        }

        #endregion Private Constructors

        #region Public Properties

        /// <summary>
        /// Gets the singleton instance of the view model
        /// </summary>
        public static MainViewModel Instance { get; private set; }

        internal void Select(ICAEXWrapper caexObject, bool activate)
        { 
            var lib = caexObject.Library();
            if (lib is InstanceHierarchyType)
            {
                AMLDocumentTreeViewModel?.SelectCaexNode(caexObject.Node, true, true);
                if (activate)
                    AMLDocumentTreeViewModel.RaisePropertyChanged("Activate");
            }
        }



        internal static string PluginName(string name)
        {
            var varName = name.Replace(' ', '_');
            varName = varName.Replace('-', '_');

            return varName;
        }



        /// <summary>
        ///  Gets and sets the AMLDocumentTreeViewModel which holds the data for the AML document tree view
        /// </summary>
        public AMLTreeViewModel AMLDocumentTreeViewModel
        {
            get
            {
                return _aMLDocumentTreeViewModel;
            }
            set
            {
                if (_aMLDocumentTreeViewModel != value)
                {
                    _aMLDocumentTreeViewModel = value;
                    RaisePropertyChanged(() => AMLDocumentTreeViewModel);

                    // we need a handler to recognize a selection in the tree view. Every selection can be propagated to every plugIn.
                    if (AMLDocumentTreeViewModel != null)
                        AMLDocumentTreeViewModel.SelectedElements.CollectionChanged += SelectedElementsCollectionChanged;
                }
            }
        }

        /// <summary>
        /// Gets the current selected object.
        /// </summary>
        public CAEXBasicObject CurrentSelectedObject { get; private set; }

        public CAEXDocument Document { get; private set; }

        /// <summary>
        /// Gets the file path of the AML document.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        ///    Gets a value indicating whether this instance has plugins.
        /// </summary>
        /// <value><c>true</c> if this instance has plugins; otherwise, <c>false</c>.</value>
        public bool HasPlugins
        {
            get
            {
                return _hasPlugins;
            }
            set
            {
                if (_hasPlugins != value)
                {
                    _hasPlugins = value;
                    RaisePropertyChanged("HasPlugins");
                }
            }
        }

        /// <summary>
        ///  The OpenFileCommand - Command
        /// </summary>
        public System.Windows.Input.ICommand OpenFileCommand
        {
            get
            {
                return this._OpenFileCommand
                ??
                (this._OpenFileCommand = new RelayCommand<object>(this.OpenFileCommandExecute, this.OpenFileCommandCanExecute));
            }
        }

        /// <summary>
        ///    Gets or sets the plugIns collection. This collection is updated by the Editor when PlugIns are recognized.
        /// </summary>
        /// <value>The plugIns.</value>
        public ObservableCollection<IAMLEditorPlugin> Plugins
        {
            get;
            set;
        }

        #endregion Public Properties

        #region Internal Methods

        /// <summary>
        /// Utility method to get the Document URI.
        /// </summary>
        internal static Uri DocumentUri(CAEXDocument document)
        {
            var documentPath = document.CAEXFile.GetFileNamePath();
            var documentDirectory = Path.GetDirectoryName(documentPath);

            Uri.TryCreate(documentDirectory, UriKind.Absolute, out var documentBaseURI);

            if (documentBaseURI == null)
            {
                MessageBox.Show(string.Format("Could not create a Document URI {0}", documentDirectory));
            }
            return documentBaseURI;
        }

        internal void Close()
        {
            AMLDocumentTreeViewModel?.ClearAll();
            Document = null;
        }

        internal void New()
        {
            AMLDocumentTreeViewModel?.ClearAll();
            Document = null;

            GenerateSomeAutomationMLTestData("My new hierarchy");
            BuildTreeViewModel();
        }



        /// <summary>
        /// Utility method to get the Files path from an URI attribute value which can be relative to the document URI
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="documentBaseURI">The document base URI.</param>
        /// <returns></returns>
        internal static string FilePathFromValue(string path, Uri documentBaseURI)
        {
            if (Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out var fileUri))
            {
                // use the relative path
                if (!fileUri.IsAbsoluteUri)
                {
                    Uri.TryCreate(Path.GetFullPath(string.Concat(documentBaseURI.LocalPath, fileUri.OriginalString)), UriKind.RelativeOrAbsolute, out fileUri);
                }

                if (fileUri.IsAbsoluteUri && fileUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (String.IsNullOrEmpty(fileUri.LocalPath))
                        return string.Empty;

                    return File.Exists(fileUri.LocalPath) ? fileUri.LocalPath : string.Empty;
                }
            }
            return string.Empty;
        }


        /// <summary>
        /// Propagates the file open event to PlugIns. This method is called by the AutomationML editor if an AML file is opened.
        /// </summary>
        /// <param name="amlFilePath">The AML file path.</param>
        internal void PropagateFileOpenEventToPlugins(string amlFilePath)
        {
            foreach (var plugin in Plugins)
            {
                if (plugin.IsReactive && plugin.IsActive)
                {
                    plugin.ChangeAMLFilePath(amlFilePath);
                }
            }
        }

        internal void Open(string filePath)
        {
            AMLDocumentTreeViewModel?.ClearAll();
            Document = null;

            FilePath = filePath;
            Document = CAEXDocument.LoadFromFile(filePath);
            AMLDocumentTreeViewModel = new AMLTreeViewModel(Document.CAEXFile.Node, AMLTreeViewTemplate.CompleteInstanceHierarchyTree);
            PropagateFileOpenEventToPlugins(FilePath);
        }

        /// <summary>
        /// Propagates the selection event to plugIns. This method is called, whenever a tree node is selected by the user.
        /// This Sandbox implementation doesn't support AutomationML container as the Editor does. The related parts are
        /// commented in this method.
        /// </summary>
        /// <param name="caex">The CAEX.</param>
        internal void PropagateSelectionEventToPlugins(CAEXWrapper caex)
        {
            if (caex is CAEXBasicObject)
            {
                foreach (var plugin in Plugins)
                {
                    if (plugin.IsReactive && plugin.IsActive)
                    {
                        // specific for PlugIns dedicated to view content of external data references
                        if (plugin is IAMLEditorExternalsPlugin pextern)
                        {
                            if (caex is InterfaceClassType et)
                            {
                                ObjectWithAMLAttributes aml = et;
                                if (aml.RefURIAttribute != null)
                                {
                                    if (!string.IsNullOrEmpty(pextern.MIMEType))
                                    {
                                        if ((pextern.MIMEType == RelationshipType.PLCopenXml.MimeType && et.IsPLCopenXMLInterface()) ||
                                            (pextern.MIMEType == RelationshipType.Collada.MimeType && et.IsCOLLADAInterface()))
                                        {
                                            // No Support for AutomationML container
                                            //if (!IsContainer)
                                            pextern.ViewExternal(aml.RefURIAttribute, FilePathFromValue(aml.RefURIAttribute.FilePath, DocumentUri(Document)));
                                            //else if (AMLContainerEditor.AMLContainerParts.TryGetValue(aml.RefURIAttribute.FilePath, out var editor))
                                            //    pextern.ViewExternal(aml.RefURIAttribute, editor.PackagePart?.GetStream());
                                        }
                                    }
                                    else
                                    {
                                        // No Support for AutomationML container
                                        //if (!IsContainer)
                                        pextern.ViewExternal(aml.RefURIAttribute, FilePathFromValue(aml.RefURIAttribute.FilePath, DocumentUri(Document)));
                                        //else if (AMLContainerEditor.AMLContainerParts.TryGetValue(aml.RefURIAttribute.FilePath, out var editor))
                                        //    pextern.ViewExternal(aml.RefURIAttribute, editor.PackagePart?.GetStream());
                                    }
                                }
                            }
                        }

                        // for any other plugIn
                        else
                        {
                            plugin.ChangeSelectedObject(caex as CAEXBasicObject);
                        }
                    }
                }
            }
        }

        #endregion Internal Methods

        #region Private Methods

        /// <summary>
        /// Builds the TreeView model for the generated test data.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void BuildTreeViewModel()
        {
            // use the InstanceHierarchy template for the created tree view because our document contains an IH only.
            AMLDocumentTreeViewModel = new AMLTreeViewModel(Document.CAEXFile.Node, AMLTreeViewTemplate.CompleteInstanceHierarchyTree);

            // expands the first level
            AMLDocumentTreeViewModel.Root.Children[0].IsExpanded = true;
        }

        /// <summary>
        /// Generates some automationML test data to be viewed in the tree
        /// </summary>
        private void GenerateSomeAutomationMLTestData(string file)
        {
            // we want unique names for the created elements
            Engine.Services.UniqueNameService.Register();

            Document = CAEXDocument.New_CAEXDocument();
            var ih = Document.CAEXFile.InstanceHierarchy.Append(file);
            var slib = Document.CAEXFile.SystemUnitClassLib.Append("SLib");
            var rand = new Random(DateTime.Now.Millisecond);
            for (int i=0; i< 10; i++)
            {
                var s = slib.SystemUnitClass.Append();
                for (int j = 0; j < rand.Next(5); j++)
                    s.InternalElement.Append();
                for (int j = 0; j < rand.Next(3); j++)
                    s.ExternalInterface.Append();
            }

            for (int i = 0; i < 15; i++)
            {
                ih.InternalElement.Insert(slib.SystemUnitClass[rand.Next(0, 9)].CreateClassInstance(), false);
                ih.InternalElement.Last.Name = "IE of " + ih.InternalElement.Last.Name;
            }

            // create a temp file for this document
            FilePath = Path.GetTempFileName();
            Document.SaveToFile(FilePath, false);
        }

        /// <summary>
        ///  Test, if the <see cref="OpenFileCommand"/> can execute.
        /// </summary>
        private bool OpenFileCommandCanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        ///  The <see cref="OpenFileCommand"/> Execution Action.
        /// </summary>
        private void OpenFileCommandExecute(object parameter)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Open AML File",
                DefaultExt = ".aml",
                Filter = "AML Files (*.aml)|*.aml"
            };

            var result = ofd.ShowDialog();
            if (result.HasValue && (bool)result)
            {
                FilePath = ofd.FileName;
                Document = CAEXDocument.LoadFromFile(ofd.FileName);
                AMLDocumentTreeViewModel = new AMLTreeViewModel(Document.CAEXFile.Node, AMLTreeViewTemplate.CompleteInstanceHierarchyTree);
                PropagateFileOpenEventToPlugins(FilePath);
            }
        }

        /// <summary>
        /// Handler for the SelectedElements collection changed event which will propagate the selection event to every PlugIn.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void SelectedElementsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                AMLNodeViewModel node = e.NewItems.OfType<AMLNodeViewModel>().FirstOrDefault();
                if (node != null)
                {
                    PropagateSelectionEventToPlugins(node.CAEXObject);
                    CurrentSelectedObject = node.CAEXObject as CAEXBasicObject;
                }
            }
        }

        #endregion Private Methods
    }
}