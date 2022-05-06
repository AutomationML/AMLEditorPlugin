// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.Plugin.Contracts;
using Aml.Editor.Plugin.Contracts.Commanding;
using Aml.Editor.Plugin.Sandbox.ViewModels;
using Aml.Editor.PlugInManager.Loader;
using Aml.Engine.CAEX.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;

namespace Aml.Editor.Plugin.Sandbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MainViewModel _mainModel;

        public MainWindow()
        {
            InitializeComponent();
            _mainModel = new MainViewModel();
            DataContext = _mainModel;

            Loaded += MainWindowLoaded;
        }


        private void PlugInSelectionHandler(object sender, SelectionEventArgs e)
        {
            if (e.SelectedElement.CAEXDocument() == _mainModel.ActiveDocument.Document)
            {
                _mainModel.Select(e.SelectedElement, true);
            }
        }

        /// <summary>
        /// When the main window is loaded, the plugins are loaded with the plugin loader.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            var pluginLoader = new PluginLoader();
            _mainModel.Plugins.CollectionChanged += PluginsCollectionChanged;
            pluginLoader.LoadPlugIns(PluginViewModel.Folder, _mainModel);
        }

        /// <summary>
        /// Activation handler for PlugIns, defining multiple views
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="editorView">The editor view.</param>
        private void MultipleView_ViewAdded(object sender, IAMLEditorView editorView)
        {
            var plugin = _mainModel.Plugins.FirstOrDefault(p => p.Plugin.PackageName == editorView.PackageName);
            
            if (plugin== null)
            {
                return;
            }
            plugin.AddView (editorView);
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
                if (view is ISupportsThemes theming)
                {
                    _mainModel.ChangeThemeForPlugIn ( _mainModel.CurrentTheme, theming);
                }

                if (view is IAMLEditorViewCollection multipleView)
                {
                    multipleView.ViewAdded += MultipleView_ViewAdded;
                }
                if (view is IEditorCommanding commanding)
                {
                    commanding.EditorCommand = ExecuteEditorCommandInvokedFromPlugIn;
                }
                if (view is IToolBarIntegration toolBarIntegration)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                    {
                        AddToolBar(toolBarIntegration);
                    }));
                }

                if (view is IAMLEditorView editorView)
                {
                    var plugin = _mainModel.Plugins.FirstOrDefault(p => p.Plugin.PackageName == editorView.PackageName);

                    switch (editorView.InitialDockPosition)
                    {
                        case DockPositionEnum.DockContent:
                        case DockPositionEnum.DockContentMaximized:
                            if (!MainViewModel.Documents.Contains(plugin))
                            {
                                MainViewModel.Documents.Add(plugin);
                            }
                            break;

                        default:
                            if (!MainViewModel.Properties.Contains(plugin))
                            {
                                MainViewModel.Properties.Add(plugin);
                            }
                            break;
                    }

                    plugin.IsVisible = true;
                }

                if (_mainModel.ActiveDocument?.Document != null)
                {
                    //if (_viewModel.DocumentChanged && !view.IsReadonly)
                    //{
                    //    SaveCurrentDocument();
                    //}

                    if (File.Exists(_mainModel.ActiveDocument.FilePath))
                        view.PublishAutomationMLFileAndObject(_mainModel.ActiveDocument.FilePath, _mainModel.CurrentSelectedObject);
                }
            }
        }

        private void AddToolBar(IToolBarIntegration toolBarIntegration)
        {
            ToolBar tb = new() { Name = MainViewModel.PluginName(toolBarIntegration.DisplayName), HorizontalAlignment = HorizontalAlignment.Left };

            tb.ToolTip = toolBarIntegration.DisplayName;
            foreach (var command in toolBarIntegration.ToolBarCommands.OfType<PlugInCommand>())
            {
                if (command.IsCheckable)
                {
                    var button = new ToggleButton
                    {
                        ToolTip = command.CommandToolTip,
                        Content = new Image { Source = command.CommandIcon }
                    };
                    Binding bd = new("Command") { Source = command };
                    button.SetBinding(ToggleButton.CommandProperty, bd);

                    Binding bd2 = new("IsChecked") { Source = command };
                    button.SetBinding(ToggleButton.IsCheckedProperty, bd2);
                    ToolTipService.SetShowOnDisabled(button, true);
                    tb.Items.Add(button);
                }
                else
                {
                    var button = new Button
                    {
                        ToolTip = command.CommandToolTip,
                        Content = new Image { Source = command.CommandIcon }
                    };
                    Binding bd = new("Command") { Source = command };
                    button.SetBinding(Button.CommandProperty, bd);
                    ToolTipService.SetShowOnDisabled(button, true);
                    tb.Items.Add(button);
                }
            }
            ToolBarTray.ToolBars.Add(tb);
        }

        private bool ExecuteEditorCommandInvokedFromPlugIn(IAMLEditorPlugin source, AMLEditorCommandEnum command, EditorCommandArguments args)
        {
            bool success = false;
            args.Cancelled = false;

            Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    switch (command)
                    {
                        case AMLEditorCommandEnum.CloseFileCommand:
                            MessageBoxResult result = MessageBox.Show("Close File", "AMLEditor Command Execution", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                            success = result == MessageBoxResult.Yes;
                            args.Cancelled = result == MessageBoxResult.Cancel;

                            if (success)
                            {
                                _mainModel.CloseCommand.Execute(null);
                            }

                            break;

                        case AMLEditorCommandEnum.GetCAEXFileCommand:
                            if (_mainModel.ActiveDocument.Document != null && args is GetCAEXFileCommandArguments cfarg)
                            {
                                cfarg.CaexFile = _mainModel.ActiveDocument.Document.CAEXFile;
                            }
                            break;

                        case AMLEditorCommandEnum.NewFileCommand:
                            result = MessageBox.Show("New File", "AMLEditor Command Execution", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                            success = result == MessageBoxResult.Yes;
                            args.Cancelled = result == MessageBoxResult.Cancel;

                            if (success)
                            {
                                _mainModel.CloseCommand.Execute(null);
                                _mainModel.NewCommand.Execute(null);
                            }

                            break;

                        case AMLEditorCommandEnum.OpenFileCommand:
                            result = MessageBox.Show("Open File", "AMLEditor Command Execution", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                            success = result == MessageBoxResult.Yes;
                            args.Cancelled = result == MessageBoxResult.Cancel;

                            if (success && args is OpenFileCommandArguments ofarg)
                            {
                                _mainModel.CloseCommand.Execute(null);
                                _mainModel.OpenDocument(ofarg.FilePath);
                            }
                            break;

                        case AMLEditorCommandEnum.ImportLibrariesCommand:
                            result = MessageBox.Show("Import from File not implemented in SandBox", "AMLEditor Command Execution", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                            success = result == MessageBoxResult.Yes;
                            args.Cancelled = result == MessageBoxResult.Cancel;

                            break;
                    }
                }
                catch (Exception exp)
                {
                    args.Error = exp;
                }
            }));

            return success;
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
                foreach (PluginViewModel plugin in e.NewItems)
                {
                    plugin.Plugin.PluginActivated += PluginActivated;
                    plugin.Plugin.PluginTerminated += PluginTerminated;
                    if (plugin.Plugin.IsAutoActive)
                    {
                        PluginActivated(plugin, EventArgs.Empty);
                    }
                }
            }
            _mainModel.HasPlugins = _mainModel.Plugins?.Count > 0;
        }

        private void PluginTerminated(object sender, EventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            {
                try
                {
                    if (sender is IToolBarIntegration toolBarIntegration)
                    {
                        ToolBar tb = ToolBarTray.ToolBars.FirstOrDefault(tb => tb.Name == MainViewModel.PluginName(toolBarIntegration.DisplayName));

                        if (tb != null)
                        {
                            ToolBarTray.ToolBars.Remove(tb);
                        }
                    }

                    if (sender is IAMLEditorPlugin plugin)
                    {
                        var viewModel = _mainModel.Plugins.FirstOrDefault(p => p.Plugin == plugin);
                        if (viewModel != null)
                        {
                            viewModel.Terminate();
                        }
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
    }
}