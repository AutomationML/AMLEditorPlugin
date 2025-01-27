// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.Plugin.Contracts;
using Aml.Editor.Plugin.WPFBase;
using Aml.Engine.CAEX;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aml.Editor.Plugin.WithToolbar
{
    /// <summary>
    /// An example that shows how plugin methods are bound to buttons added to the editors main
    /// toolbar.
    /// <seealso cref="IToolBarIntegration"/>.
    /// </summary>
    public partial class PluginView : PluginViewBase, ISupportsThemes, IToolBarIntegration
    {
        #region Constructors

        public PluginView()
        {
            InitializeComponent();
            DisplayName = "Toolbar Integration";
            IsReactive = true;

            ToolBarCommands = new List<PluginCommand>();
            EditorCommands.ConfigureCommands(ToolBarCommands,
                (ImageSource)TryFindResource("AddItemIcon"),
                (ImageSource)TryFindResource("DeleteItemIcon"),
                (ImageSource)TryFindResource("RedoIcon")
                );

            PaneImage = new BitmapImage(
                new Uri("pack://application:,,,/Aml.Editor.Plugin.WithToolbar;component/Plugin.png"));
        }

        #endregion Constructors

        #region Properties

        public override DockPositionEnum InitialDockPosition => DockPositionEnum.DockBottom;

        public override bool CanClose => true;

        /// <summary>
        /// It is recommended to give all plugins a package name with the prefix Aml.Editor.Plugin.
        /// The package name is used as the ID of the plugin and should be unique.
        /// </summary>
        public override string PackageName => "Aml.Editor.Plugin.WithToolbar";

        public List<PluginCommand> ToolBarCommands { get; }

        #endregion Properties

        #region Methods

        public override void ChangeSelectedObject(CAEXBasicObject selectedObject)
        {
            base.ChangeSelectedObject(selectedObject);
            EditorCommands.SelectedObject = selectedObject;
        }

        /// <summary>
        /// The method is always called when the display mode is changed
        /// in the editor and when the plugin is activated.
        /// </summary>
        /// <param name="theme"></param>
        public void OnThemeChanged(ApplicationTheme theme)
        {
            // detect, which theme is currently used by mahapps.metro
            var applicationTheme = ControlzEx.Theming.ThemeManager.Current.DetectTheme(Application.Current);
            if (applicationTheme != null)
            {
                // change the theme for used toolkit elements. This method must be called in all xaml views
                // where aml.toolkit or aml.skins resources are used.
                Plugin.Contract.Theming.ThemeManager.ChangeTheme(this.Resources,
                    applicationTheme.BaseColorScheme);

                // change the theme for the used mahapps.metro ui elements
                ControlzEx.Theming.ThemeManager.Current.ChangeTheme(this, this.Resources, applicationTheme);
            }
        }

        #endregion Methods
    }
}