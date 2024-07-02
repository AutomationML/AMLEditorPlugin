// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.Plugin.Contracts;
using Aml.Editor.Plugin.Sandbox.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace Aml.Editor.PlugInManager.Loader
{
    /// <summary>
    ///     Loads plug-ins from assembly path
    /// </summary>
    public class PluginLoader
    {
        #region Private Fields

        /// <summary>
        ///     The _container
        /// </summary>
        private CompositionContainer _container;

#pragma warning disable CS0169 // The field 'PluginLoader._dispatcher' is never used
        private readonly Dispatcher _dispatcher;
#pragma warning restore CS0169 // The field 'PluginLoader._dispatcher' is never used

        #endregion Private Fields

        #region Private Properties

        /// <summary>
        ///     Gets or sets the plugIns.
        /// </summary>
        /// <value>The plugIns.</value>
        [ImportMany(typeof(IAMLEditorViewCollection), AllowRecomposition = true)]
        private List<Lazy<IAMLEditorViewCollection, IAMLEditorPluginMetadata>> MultipleViewPlugins { get; set; }

        /// <summary>
        ///     Gets or sets the plugIns.
        /// </summary>
        /// <value>The plugIns.</value>
        [ImportMany(typeof(IAMLEditorPlugin), AllowRecomposition = true)]
        private List<Lazy<IAMLEditorPlugin, IAMLEditorPluginMetadata>> Plugins { get; set; }

        /// <summary>
        ///     Gets or sets the plugIns.
        /// </summary>
        /// <value>The plugIns.</value>
        [ImportMany(typeof(IAMLEditorView), AllowRecomposition = true)]
        private List<Lazy<IAMLEditorView, IAMLEditorPluginMetadata>> UIPlugins { get; set; }

        #endregion Private Properties

        #region Public Methods

        /// <summary>
        ///     Instantiates the loaded PlugIns.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal void InstantiatePlugins(MainViewModel mainModel)
        {
            try
            {
                string uniqueName;
                if (Plugins != null)
                {
                    foreach (Lazy<IAMLEditorPlugin, IAMLEditorPluginMetadata> plugin in Plugins)
                    {
                        uniqueName = MainViewModel.PluginName(plugin.Value.DisplayName);
                        mainModel.Plugins.Add(new PluginViewModel(plugin.Value, plugin.Metadata));
                    }
                }

                if (UIPlugins != null)
                {
                    foreach (Lazy<IAMLEditorView, IAMLEditorPluginMetadata> plugin in UIPlugins)
                    {
                        uniqueName = MainViewModel.PluginName(plugin.Value.DisplayName);
                        mainModel.Plugins.Add(new PluginViewModel(plugin.Value, plugin.Metadata));
                    }
                }

                if (MultipleViewPlugins != null)
                {
                    foreach (Lazy<IAMLEditorViewCollection, IAMLEditorPluginMetadata> plugin in MultipleViewPlugins)
                    {
                        uniqueName = MainViewModel.PluginName(plugin.Value.DisplayName);
                        mainModel.Plugins.Add(new PluginViewModel(plugin.Value, plugin.Metadata));
                    }
                }
                mainModel.HasPlugins = mainModel.Plugins.Count > 0;
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"PlugIn instantiation failed! \n {ex}");
            }
        }

        internal void LoadPlugIns(string folder, MainViewModel mainModel)
        {
            string pluginFolder = folder;

            if (!Directory.Exists(pluginFolder))
            {
                return;
            }

            try
            {
                SafeDirectoryCatalog pluginsCatalog;
                using (pluginsCatalog = new SafeDirectoryCatalog(pluginFolder))
                {
                    //Create the CompositionContainer with the parts in the catalog
                    _container = new CompositionContainer(pluginsCatalog);
                    _container.ComposeParts(this);
                    InstantiatePlugins(mainModel);
                }
            }
            catch (CompositionException compositionException)
            {
                _ = MessageBox.Show(Application.Current.MainWindow ?? throw new InvalidOperationException(),
                    compositionException.ToString(),
                    "PlugIn Loader", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (DirectoryNotFoundException directoryException)
            {
                _ = MessageBox.Show(Application.Current.MainWindow ?? throw new InvalidOperationException(),
                    directoryException.ToString(),
                    "PlugIn Loader", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ReflectionTypeLoadException loaderException)
            {
                if (loaderException.LoaderExceptions != null && loaderException.LoaderExceptions.Length > 0)
                {
                    _ = MessageBox.Show(Application.Current.MainWindow ?? throw new InvalidOperationException(),
                        loaderException.LoaderExceptions[0].ToString(),
                        "PlugIn Loader", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    _ = MessageBox.Show(Application.Current.MainWindow ?? throw new InvalidOperationException(),
                        loaderException.ToString(),
                        "PlugIn Loader", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (TypeLoadException exc)
            {
                _ = MessageBox.Show(Application.Current.MainWindow ?? throw new InvalidOperationException(),
                    exc.ToString(),
                    "PlugIn Loader", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
            }
        }

        #endregion Public Methods
    }
}