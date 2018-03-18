using Aml.Editor.Plugin.Contracts;
using Aml.Editor.Plugin.Sandbox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Aml.Editor
{
    /// <summary>
    /// This class is the original plugin loader class, used by the AutomationML editor. The class has been transfered to this sandbox project to be able to test, if a plugin can be loaded.
    /// </summary>
    public class PluginLoader
    {
        #region Private Fields

        /// <summary>
        /// The _container
        /// </summary>
        private CompositionContainer _container;

        /// <summary>
        /// The current
        /// </summary>
        private Brush _current;

        /// <summary>
        /// The main
        /// </summary>
        private MainWindow _main;

        /// <summary>
        /// The _plugin catalog
        /// </summary>
        private DirectoryCatalog _pluginCatalog;

        /// <summary>
        /// The screen
        /// </summary>
        private SplashScreen _screen;

        private bool _updateMessage;

        #endregion Private Fields

        #region Internal Events

        internal event EventHandler ShutDownRequest;

        #endregion Internal Events

        #region Private Properties

        /// <summary>
        /// Gets or sets the plugins.
        /// </summary>
        /// <value>The plugins.</value>
        [ImportMany(typeof(IAMLEditorViewCollection), AllowRecomposition = true)]
        private List<Lazy<IAMLEditorViewCollection>> MultipleViewPlugins
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the plugins.
        /// </summary>
        /// <value>The plugins.</value>
        [ImportMany(typeof(IAMLEditorPlugin), AllowRecomposition = true)]
        private List<Lazy<IAMLEditorPlugin>> Plugins
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the plugins.
        /// </summary>
        /// <value>The plugins.</value>
        [ImportMany(typeof(IAMLEditorView), AllowRecomposition = true)]
        private List<Lazy<IAMLEditorView>> UIPlugins
        {
            get;
            set;
        }

        #endregion Private Properties

        #region Public Methods

        public void DoSomething(MainWindow main)
        {
            this._main = main;
            if (main != null)
            {
                main.ContentRendered += MainContentRendered;
                main.Show();
            }
        }

        public void DoWorkInShadowCopiedDomain()
        {
            try
            {
                string pluginsPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Plugins");

                _pluginCatalog = new DirectoryCatalog(pluginsPath);
                var catalog = new AggregateCatalog(_pluginCatalog);

                // Create the CompositionContainer with the parts in the catalog
                _container = new CompositionContainer(catalog);
                _container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                MessageBox.Show(compositionException.ToString(), "Plugin Loader", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (DirectoryNotFoundException directoryException)
            {
                MessageBox.Show(directoryException.ToString(), "Plugin Loader", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ReflectionTypeLoadException loaderException)
            {
                if (loaderException.LoaderExceptions != null && loaderException.LoaderExceptions.Length > 0)
                    MessageBox.Show(loaderException.LoaderExceptions[0].ToString(), "Plugin Loader", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show(loaderException.ToString(), "Plugin Loader", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Recompose()
        {
            _pluginCatalog.Refresh();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Instantiates the loaded PlugIns.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        private void InstantiatePlugins(object sender, DoWorkEventArgs e)
        {
            _main.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                if (this.Plugins != null)
                {
                    foreach (var plugin in this.Plugins)
                        Plugin.Sandbox.ViewModel.MainViewModel.Instance.Plugins.Add(plugin.Value);
                }

                if (this.UIPlugins != null)
                {
                    foreach (var plugin in this.UIPlugins)
                        Plugin.Sandbox.ViewModel.MainViewModel.Instance.Plugins.Add(plugin.Value);
                }

                if (this.MultipleViewPlugins != null)
                {
                    foreach (var plugin in this.MultipleViewPlugins)
                        Plugin.Sandbox.ViewModel.MainViewModel.Instance.Plugins.Add(plugin.Value);
                }

                Plugin.Sandbox.ViewModel.MainViewModel.Instance.HasPlugins = Plugin.Sandbox.ViewModel.MainViewModel.Instance.Plugins.Count > 0;
            }));
        }

        /// <summary>
        /// Handles the ContentRendered event of the main control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MainContentRendered(object sender, EventArgs e)
        {
            // start a background worker to instantiate the loaded plugins
            BackgroundWorker instantiatePluginsWorker = new BackgroundWorker();
            instantiatePluginsWorker.DoWork += InstantiatePlugins;
            instantiatePluginsWorker.RunWorkerAsync();

            _updateMessage = false;
        }

        #endregion Private Methods
    }
}