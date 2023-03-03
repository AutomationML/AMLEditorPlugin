// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.MVVMBase;
using Aml.Editor.Plugin.Contract.Commanding;
using Aml.Editor.Plugin.Contracts;
using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Skins;
using ControlzEx.Theming;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static Aml.Engine.AmlObjects.AutomationMLContainer;

namespace Aml.Editor.Plugin.Sandbox.ViewModels
{
    /// <summary>
    /// The Aml.Editor.Plugin.Sandbox serves as a testbed for the development of AutomationML editor PlugIns.
    /// This Software mimics the behavior of the AutomationML editor loading and activating PlugIns. After successfully testing a PlugIn,
    /// the compiled DLLs can be moved from the Plugins folder of this solution to the installed AutomationML editor Plugins folder.
    /// Please check the content of the Plugins folder for any duplicates. If duplicates exist rename your assemblies.
    /// </summary>
    internal class MainViewModel : ViewModelBase
    {
        #region Fields

        private ActiveDocumentViewModel _activeDocument;
        private SimpleCommand<object> _closeCommand = null;
        private SimpleCommand<object> _loadCommand = null;
        private SimpleCommand<object> _newCommand = null;
        private SimpleCommand<object> _saveAsCommand = null;
        private SimpleCommand<object> _saveCommand = null;
        private SimpleCommand<object> _exitCommand = null;

        private string _selectedTheme = "Light";

        private double _zoom = 1.0;

        #endregion Fields

        public event EventHandler PluginFolderChanged;

        public MainViewModel()
        {
            // populate document collection
            for (int i = 0; i < 5; i++)
            {
                Documents.Add(ActiveDocumentViewModel.Libraries[i]);
                ActiveDocumentViewModel.Libraries[i].MainModel = this;
            }
            Properties.Add(ActiveDocumentViewModel.AttributeTree);
            SetTheme();

            Instance = this;
        }

        public static MainViewModel Instance { get; set; }

        private void SetTheme()
        {
            SelectedTheme = Sandbox.Properties.Settings.Default.Theme;
        }

        #region Properties

        /// <summary>
        /// The document collection presented in the main view
        /// </summary>
        public static ObservableCollection<ViewModelBase> Documents { get; } = new();

        /// <summary>
        /// The properties collection presented in the main view
        /// </summary>
        public static ObservableCollection<ViewModelBase> Properties { get; } = new();

        public ActiveDocumentViewModel ActiveDocument
        {
            get => _activeDocument;
            private set => Set(ref _activeDocument, value);
        }

        public SimpleCommand<object> SettingsCommand => _settingsCommand ??= new SimpleCommand<object>(o => true, DoSettings);

        private void DoSettings(object obj)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                PluginViewModel.Folder = dialog.SelectedPath;
                Sandbox.Properties.Settings.Default.PluginFolder = PluginViewModel.Folder;
                Sandbox.Properties.Settings.Default.Save();

                PluginFolderChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public SimpleCommand<object> CloseCommand => _closeCommand ??= new SimpleCommand<object>(o => ActiveDocument != null, DoClose);

        public SimpleCommand<object> ExitCommand => _exitCommand ??= new SimpleCommand<object>(o => true, DoExit);

        public SimpleCommand<object> LoadCommand => _loadCommand ??= new SimpleCommand<object>(o => true, async x => { await DoLoad(); });

        public SimpleCommand<object> NewCommand => _newCommand ??= new SimpleCommand<object>(o => true, DoNew);

        public SimpleCommand<object> SaveAsCommand => _saveAsCommand ??= new SimpleCommand<object>(o => ActiveDocument != null, async x => { await ActiveDocument.SaveDocumentAs(); });

        public SimpleCommand<object> SaveCommand => _saveCommand ??= new SimpleCommand<object>(o => ActiveDocument != null, async x => { await ActiveDocument.SaveDocument(); });

        public AvalonDock.Themes.Theme SelectedAvalonTheme { get; private set; }

        /// <summary>
        ///    Gets a value indicating whether this instance has plugins.
        /// </summary>
        /// <value><c>true</c> if this instance has plugins; otherwise, <c>false</c>.</value>
        public bool HasPlugins
        {
            get => _hasPlugins;
            set => Set(ref _hasPlugins, value);
        }

        public string SelectedTheme
        {
            get { return _selectedTheme; }
            set
            {
                _selectedTheme = value;
                SwitchExtendedTheme();
                RaisePropertyChanged(nameof(SelectedTheme));
                RaisePropertyChanged(nameof(SelectedAvalonTheme));
            }
        }

        internal void Select(ICAEXWrapper selectedElement, bool v)
        {
            throw new NotImplementedException();
        }

        public List<string> Themes { get; set; } = new()
        {
            "Dark",
            "Light"
        };

        public double ZoomFactor
        {
            get => _zoom;
            set 
            {
                if ( Set(ref _zoom, value) )
                {
                    PropagateZoomToPlugins();
                }
            }
        }


        internal void PropagateZoomToPlugins()
        {
            foreach (var plugin in Plugins.Where(p => p.Plugin is ISupportsUIZoom))
            {
                (plugin.Plugin as ISupportsUIZoom).OnUIZoomChanged(ZoomFactor);
            }
        }

        #endregion Properties

        #region Methods

        private void DoClose(object args)
        {
            if (ActiveDocument != null)
            {
                ActiveDocument?.Close();
                ActiveDocument = null;
                PropagateUnloadEventToPlugins();
            }
        }

        private void DoExit(object args)
        {
            DoClose(args);
            PropagateCloseApplicationEventToPlugins();
            Application.Current.Shutdown();
        }

        private void PropagateCloseApplicationEventToPlugins()
        {
            foreach (var plugin in Plugins.Where(p => p.Plugin is INotifyAMLDocumentLoad))
            {
                (plugin.Plugin as INotifyAMLDocumentLoad).ApplicationClose();
            }
        }

        private void PropagateUnloadEventToPlugins()
        {
            foreach (var plugin in Plugins.Where(p => p.Plugin is INotifyAMLDocumentLoad))
            {
                (plugin.Plugin as INotifyAMLDocumentLoad).DocumentUnLoaded();
            }
        }

        private async Task DoLoad()
        {
            DoClose(null);
            var dlg = new OpenFileDialog
            {
                Filter = "AutomationML Files (.aml)|*.aml",
                FilterIndex = 1
            };
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                ActiveDocument = new()
                {
                    FilePath = dlg.FileName,
                    MainModel = this,
                    Document = await CAEXDocument.LoadFromFileAsync(dlg.FileName, new()),
                };
            }
        }

        internal static bool IsDocument(object content)
        {
            return Documents.Contains(content);
        }

        private void DoNew(object args)
        {
            DoClose(args);

            var document = CAEXDocument.New_CAEXDocument();
            document.AddAutomationMLBaseRoleClassLib();
            document.AddAutomationMLBaseAttributeTypeLib();
            document.AddAutomationMLInterfaceClassLib();
            ActiveDocument = new()
            {
                MainModel = this,
                Document = document
            };
        }

        /// <summary>
        ///  <see cref="HasPlugins"/>
        /// </summary>
        private bool _hasPlugins;

        private SimpleCommand<object> _settingsCommand;

        internal static string PluginName(string name)
        {
            var varName = name.Replace(' ', '_');
            varName = varName.Replace('-', '_');
            varName = varName.Replace('.', '_');

            return varName;
        }

        //public IAMLEditorView ViewPlugin (string id) =>
        //    Plugins.FirstOrDefault(p=>p.Plugin is IAMLEditorView && p.Plugin.PackageName == id) as IAMLEditorView;

        public ObservableCollection<PluginViewModel> Plugins { get; } = new();

        public CAEXBasicObject CurrentSelectedObject { get; internal set; }

        private void SwitchExtendedTheme()
        {
            var avalonTheme = Application.Current.Resources.MergedDictionaries.First(x => x.Source.OriginalString.Contains("AvalonDock"));
            switch (_selectedTheme)
            {
                case "Dark":
                    avalonTheme.Source = new Uri("pack://application:,,,/AvalonDock.Themes.VS2013;component/DarkBrushs.xaml");
                    ThemeManager.Current.ChangeThemeBaseColor(Application.Current, "Dark");
                    SelectedAvalonTheme = new AvalonDock.Themes.Vs2013DarkTheme();
                    AMLApp.ApplySkin(App.Current, SkinSource.DarkSkin);
                    ChangeThemeForPlugIn(ApplicationTheme.RoyalDark);
                    break;

                case "Light":
                    avalonTheme.Source = new Uri("pack://application:,,,/AvalonDock.Themes.VS2013;component/LightBrushs.xaml");
                    ThemeManager.Current.ChangeThemeBaseColor(Application.Current, "Light");
                    SelectedAvalonTheme = new AvalonDock.Themes.Vs2013LightTheme();
                    AMLApp.ApplySkin(App.Current, SkinSource.LightSkin);
                    ChangeThemeForPlugIn(ApplicationTheme.RoyalLight);
                    break;

                default:
                    break;
            }
            Sandbox.Properties.Settings.Default.Theme = SelectedTheme;
            Sandbox.Properties.Settings.Default.Save();
        }

        internal ApplicationTheme CurrentTheme => (SelectedTheme == "Dark")
            ? ApplicationTheme.RoyalDark
            : ApplicationTheme.RoyalLight;

        public MainWindow View { get; internal set; }

        internal void ChangeThemeForPlugIn(ApplicationTheme name, ISupportsThemes view = null)
        {
            if (view != null)
            {
                view.OnThemeChanged(name);
                return;
            }
            foreach (var plugin in Plugins.Select(p => p.Plugin))
            {
                if (plugin is not ISupportsThemes theming)
                {
                    continue;
                }
                theming.OnThemeChanged(name);
            }
        }

        /// <summary>
        /// Propagates the selection event to plugIns.
        /// </summary>
        /// <param name="caex">The CAEX.</param>
        internal void PropagateSelectionEventToPlugins(CAEXWrapper caex)
        {
            if (caex is not CAEXBasicObject)
            {
                return;
            }

            foreach (PluginViewModel plugin in Plugins)
            {
                if (!plugin.Plugin.IsReactive || !plugin.Plugin.IsActive)
                {
                    continue;
                }

                plugin.Plugin.ChangeSelectedObject(caex as CAEXBasicObject);

                if (plugin.Plugin is not IAMLEditorExternalsPlugin pextern)
                {
                    continue;
                }

                if (caex is not InterfaceClassType et)
                {
                    continue;
                }

                ObjectWithAMLAttributes aml = et;
                if (aml.RefURIAttribute == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(pextern.MIMEType))
                {
                    var mimeTypes = pextern.MIMEType.Split(';');

                    // is vali
                    bool isValid = (mimeTypes.Contains(RelationshipType.PLCopenXml.MimeType) && et.IsPLCopenXMLInterface())
                                   || (mimeTypes.Contains(RelationshipType.Collada.MimeType) && et.IsCOLLADAInterface());

                    // check for other mime types
                    if (!isValid)
                    {
                        var mimeTypeAt = aml.Attribute.FirstOrDefault(a => a.IsMIMEType());
                        isValid = (mimeTypeAt != null && mimeTypes.Contains(mimeTypeAt.Value));
                    }

                    if (!isValid)
                    {
                        continue;
                    }
                    
                }                 
                pextern.ViewExternal(aml.RefURIAttribute, FilePathFromValue(
                    aml.RefURIAttribute.FilePath,
                    DocumentUri(ActiveDocument.Document)));
            }
        }

        private static string FilePathFromValue(string path, Uri documentBaseURI)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            path = path.TrimStart('/');

            if (!Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out Uri fileUri))
            {
                return string.Empty;
            }

            // use the relative path
            if (!fileUri.IsAbsoluteUri && documentBaseURI != null &&
                !Uri.TryCreate(Path.GetFullPath(Path.Combine(documentBaseURI.LocalPath, path)),
                    UriKind.RelativeOrAbsolute, out fileUri))
            {
                return string.Empty;
            }

            if (!fileUri.IsAbsoluteUri || !fileUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(fileUri.LocalPath))
            {
                return string.Empty;
            }

            return File.Exists(fileUri.LocalPath) ? fileUri.LocalPath : string.Empty;
        }
        private static Uri DocumentUri(CAEXDocument document)
        {
            string documentPath = document.FilePath;
            if (string.IsNullOrEmpty(documentPath))
            {
                return null;
            }

            string documentDirectory = Path.GetDirectoryName(documentPath);
            if (string.IsNullOrEmpty(documentDirectory))
            {
                return null;
            }

            _ = Uri.TryCreate(documentDirectory, UriKind.Absolute, out Uri documentBaseURI);

            if (documentBaseURI == null)
            {
                _ = MessageBox.Show($"Could not create a Document URI {documentDirectory}");
            }

            return documentBaseURI;
        }

        internal void OpenDocument(string filePath)
        {
            ActiveDocument = new()
            {
                FilePath = filePath,
                MainModel = this,
                Document = CAEXDocument.LoadFromFile(filePath),
            };
        }

        internal void OpenDocument(CAEXDocument document) {
            ActiveDocument = new() {
                FilePath = document.CAEXFile.FileName,
                MainModel = this,
                Document = document,
            };
        }

        #endregion Methods
    }
}