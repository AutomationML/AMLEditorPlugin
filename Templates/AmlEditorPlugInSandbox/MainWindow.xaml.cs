using Aml.Editor.Plugin.Contracts;
using Aml.Editor.Plugin.Sandbox.ViewModel;
using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock.Layout;

namespace Aml.Editor.Plugin.Sandbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Public Constructors

        public MainWindow()
        {
            InitializeComponent();

            // bind this view to the data
            DataContext = MainViewModel.Instance;
            Loaded += MainWindowLoaded;
        }

        #endregion Public Constructors

        #region Private Methods

        private LayoutAnchorable DockView(IAMLEditorView view)
        {
            var plugin = new LayoutAnchorable()
            {
                Content = view,
                CanClose = view.CanClose
            };

            switch (view.InitialDockPosition)
            {
                case DockPositionEnum.DockContent:
                    DocumentPane.Children.Add(plugin);
                    break;

                case DockPositionEnum.DockLeft:
                    LeftTabSide.Children.Add(plugin);
                    break;

                case DockPositionEnum.DockRight:
                    RightTabSide.Children.Add(plugin);
                    break;

                case DockPositionEnum.DockBottom:
                    BottomTabSide.Children.Add(plugin);
                    break;

                case DockPositionEnum.DockTop:
                    TopTabSide.Children.Add(plugin);
                    break;

                case DockPositionEnum.Floating:
                    FloatingPane.Children.Add(plugin);
                    break;
            }

            plugin.Title = view.DisplayName;

            if (MainViewModel.Instance.Plugins.Contains(view))
                plugin.IsActive = true;

            if (plugin.CanClose)
            {
                plugin.Closed += PluginClosed;
            }

            return plugin;
        }

        /// <summary>
        /// When the main window is loaded, the plugins are loaded with the plugin loader.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            var pluginLoader = new PluginLoader();
            MainViewModel.Instance.Plugins.CollectionChanged += PluginsCollectionChanged;
            pluginLoader.DoWorkInShadowCopiedDomain();
            pluginLoader.DoSomething(this);
        }

        /// <summary>
        /// Activation handler for PlugIns, defining multiple views
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="editorView">The editor view.</param>
        private void MultipleView_ViewAdded(object sender, IAMLEditorView editorView)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                var pluginWindow = DockView(editorView);
                if (pluginWindow != null && pluginWindow.Content is IAMLEditorPlugin plugin)
                {
                    plugin.PluginTerminated += PluginTerminated;
                }
            }));
        }

        /// <summary>
        /// Activation handler for a PlugIn
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PluginActivated(object sender, EventArgs e)
        {
            if (sender is IAMLEditorPlugin)
            {
                var view = sender as IAMLEditorPlugin;

                if (view is IAMLEditorViewCollection multipleView)
                {
                    multipleView.ViewAdded += MultipleView_ViewAdded;
                }

                if (view is IAMLEditorView editorView)
                {
                    DockView(editorView);
                }

                if (MainViewModel.Instance.Document != null)
                {
                    //if (_viewModel.DocumentChanged && !view.IsReadonly)
                    //{
                    //    SaveCurrentDocument();
                    //}

                    if (File.Exists(MainViewModel.Instance.FilePath))
                        view.PublishAutomationMLFileAndObject(MainViewModel.Instance.FilePath, MainViewModel.Instance.CurrentSelectedObject);
                }
            }
        }

        /// <summary>
        /// Handles the Closed event of the plugIn control.
        /// </summary>
        private void PluginClosed(object sender, EventArgs e)
        {
            if (sender is LayoutAnchorable pluginWindow && pluginWindow.Content is IAMLEditorPlugin plugin)
            {
                plugin.ExecuteCommand(PluginCommandsEnum.Terminate, MainViewModel.Instance.FilePath);
            }
        }

        /// <summary>
        /// This Handler is used to show the loaded plugins as dockable views in the Editors docking manager.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void PluginsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (IAMLEditorPlugin plugin in e.NewItems)
                {
                    plugin.PluginActivated += PluginActivated;
                    plugin.PluginTerminated += PluginTerminated;
                    if (plugin.IsAutoActive)
                    {
                        PluginActivated(plugin, EventArgs.Empty);
                    }
                }
            }
            MainViewModel.Instance.HasPlugins = MainViewModel.Instance.Plugins != null && MainViewModel.Instance.Plugins.Count > 0;
        }

        private void PluginTerminated(object sender, EventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
              {
                  try
                  {
                      if (sender is IAMLEditorPlugin)
                      {
                          foreach (var source in this.DockingManager.AnchorablesSource)
                          {
                          }
                          //var pluginWindow = this.DockingManager.AnchorablesSource .GetPanes(PaneNavigationOrder.ActivationOrder).OfType<PluginWindow>().Where(p => p.Plugin == view).FirstOrDefault();
                          //if (pluginWindow != null)
                          //{
                          //    pluginWindow.ExecuteCommand(ContentPaneCommands.Close);
                          //}

                          //if (!view.IsReadonly && !view.IsActive)
                          //{
                          //    // don't enable the pane, if any of the not read only plugIns is still active
                          //    if (_viewModel.Plugins.Any(p => p.IsActive && !p.IsReadonly))
                          //    {
                          //        return;
                          //    }

                          //    _viewModel.IsReadonly = false;
                          //    this.AMLEditorApplication_Activated(this, EventArgs.Empty);
                          //}
                      }

                      if (sender is IAMLEditorViewCollection multiViewPlugin)
                      {
                          foreach (var pview in multiViewPlugin)
                          {
                              //var pluginWindow = this.DockingManager.GetPanes(PaneNavigationOrder.ActivationOrder).OfType<PluginWindow>().Where(p => p.Plugin == pview).FirstOrDefault();
                              //if (pluginWindow != null)
                              //{
                              //    pluginWindow.ExecuteCommand(ContentPaneCommands.Close);
                              //}
                          }
                      }
                  }
                  catch
                  { }

              }));
        }

        #endregion Private Methods
    }
}