// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.Plugin.Contracts;
using Aml.Editor.Plugin.WPFBase;
using Aml.Engine.CAEX;
using Aml.Toolkit.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Aml.Editor.Plugin.Theming
{
    /// <summary>
    /// An example that shows how the change of the light-dark display mode can
    /// be transferred from the editor to the plugin. The plugin itself must be
    /// able to rearrange its UI elements, for example by using a UI library like
    /// mahapps.metro. If UI elements of the <see cref="N:Aml.Toolkit"/> and
    /// <see cref="N:Aml.Skins"/>are used, the method
    /// <see cref="Plugin.Contract.Theming.ThemeManager.ChangeTheme"/>
    /// must be called for the update.
    /// <seealso cref="ISupportsThemes"/>.
    /// <see cref="OnThemeChanged(ApplicationTheme)"/>
    /// </summary>    
    public partial class PluginView : PluginViewBase, ISupportsThemes
    {
        #region Constructors

        public PluginView()
        {
            InitializeComponent();
            DisplayName = "Theming";
            IsReactive = true;

            //iconPacks.  iconPacks:PackIconZondicons
            //     Width = "22"
            //    Height = "22"
            //    Margin = "4"
            //    HorizontalAlignment = "Center"
            //    VerticalAlignment = "Center"
            //    Foreground = "DarkOrange"
            //    Kind = "Plugin" />

            PaneImage = new BitmapImage(
                new Uri("pack://application:,,,/Aml.Editor.Plugin.Theming;component/Plugin.png"));
        }

        #endregion Constructors

        #region Properties

        public override DockPositionEnum InitialDockPosition => DockPositionEnum.DockContent;

        public override bool CanClose => true;

        /// <summary>
        /// It is recommended to give all plugins a package name with the prefix Aml.Editor.Plugin.
        /// The package name is used as the ID of the plugin and should be unique.
        /// </summary>
        public override string PackageName => "Aml.Editor.Plugin.Theming";

        #endregion Properties

        #region Methods

        public override void ChangeSelectedObject(CAEXBasicObject selectedObject)
        {
            base.ChangeSelectedObject(selectedObject);
            if (selectedObject == null)
            {
                return;
            }
            ShowTreeView(selectedObject);
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

        private void ShowTreeView(CAEXBasicObject caex)
        {
            HashSet<string> amlTreeTemplate = new(
                AMLTreeViewTemplate.CompleteInstanceHierarchyTree.Concat(
                AMLTreeViewTemplate.CompleteSystemUnitClassLibTree.Concat(
                AMLTreeViewTemplate.InterfaceClassLibTree.Concat(
                AMLTreeViewTemplate.ExtendedRoleClassLibTree.Concat(
                AMLTreeViewTemplate.AttributeTypeLibTree
                )))));

            AMLTree.TreeViewModel = new AMLTreeViewModel(caex.Node, amlTreeTemplate);
            AMLTree.TreeViewModel.ExpandAllCommand.Execute(AMLTree.TreeViewModel.Root);
        }

        #endregion Methods
    }
}