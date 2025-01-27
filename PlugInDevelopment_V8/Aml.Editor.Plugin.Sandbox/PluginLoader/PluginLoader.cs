// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.Plugin.Contracts;
using Aml.Editor.Plugin.Sandbox.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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


#pragma warning disable CS0169 // The field 'PluginLoader._dispatcher' is never used
        private readonly Dispatcher _dispatcher;
#pragma warning restore CS0169 // The field 'PluginLoader._dispatcher' is never used

        #endregion Private Fields

        #region Private Properties


        private List<IAMLEditorViewCollection> MultipleViewPlugins { get; set; } = [];

       
        private List<IAMLEditorPlugin> Plugins { get; set; } = [];


        private List<IAMLEditorView> UIPlugins { get; set; } = [];

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
                foreach (var file in Directory.EnumerateFiles(pluginFolder, "*.nupkg", SearchOption.AllDirectories))
                {
                    var directoryPath = Path.GetDirectoryName(file);
                    var fileName = Path.GetFileNameWithoutExtension(file).Split('.');
                    var pluginName = fileName.Take(fileName.Length - 3).Aggregate((a, b) => a + "." + b);

                    var pluginFile = Directory.EnumerateFiles(directoryPath, $"{pluginName}.dll", SearchOption.AllDirectories).FirstOrDefault();

                    var plugin = new PluginViewModel(pluginFile);
                    AssemblyLoader.PluginLoader.LoadPlugin(plugin, mainModel);

                    if (plugin.Plugin == null)
                    {
                        MessageBox.Show($"{pluginFile} not loaded");
                    }
                    else
                    {
                        mainModel.Plugins.Add(plugin);
                    }
                }

                AppDomain.CurrentDomain.AssemblyResolve += AssemblyLoader.PluginLoader.ResolveAssembly;
            }
           
            catch (DirectoryNotFoundException directoryException)
            {
                _ = MessageBox.Show(Application.Current.MainWindow ?? throw new InvalidOperationException(),
                    directoryException.ToString(),
                    "PlugIn Loader", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion Public Methods
    }
}