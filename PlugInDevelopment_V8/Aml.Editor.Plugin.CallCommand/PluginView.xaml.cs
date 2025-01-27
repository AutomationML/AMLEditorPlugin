// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.

using Aml.Editor.Plugin.Contracts;
using Aml.Editor.Plugin.Contracts.Commanding;
using Aml.Editor.Plugin.WPFBase;
using Aml.Engine.CAEX;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Aml.Editor.Plugin.CallCommand
{
    /// <summary>
    /// An example that shows how a plugin can call commands of the editor.
    /// <seealso cref="IEditorCommanding"/>.
    /// </summary>
    /// <remarks>Not all commands, supported by the editor are supported by the sandbox.</remarks>    
    public partial class PluginView : PluginViewBase, IEditorCommanding, ISupportsThemes
    {
        #region Constructors

        public PluginView()
        {
            InitializeComponent();
            DisplayName = "Call Commands";
            IsReactive = true;

            PaneImage = new BitmapImage(
                new Uri("pack://application:,,,/Aml.Editor.Plugin.CallCommand;component/Plugin.png"));
        }

        #endregion Constructors

        #region Properties

        public override bool CanClose => true;
        public CommandExecution EditorCommand { get; set; }
        public override DockPositionEnum InitialDockPosition => DockPositionEnum.DockLeft;

        /// <summary>
        /// It is recommended to give all plugins a package name with the prefix Aml.Editor.Plugin.
        /// The package name is used as the ID of the plugin and should be unique.
        /// </summary>
        public override string PackageName => "Aml.Editor.Plugin.CallCommand";

        #endregion Properties

        #region Methods

        public override void ChangeSelectedObject(CAEXBasicObject selectedObject)
        {
            base.ChangeSelectedObject(selectedObject);
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

        private void CaptureBtn_Click(object sender, RoutedEventArgs e)
        {
            this.CaptureCommand(out _);
            Info.Text = "not supported";
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.CloseCAEXFile(out var args))
                Info.Text = "close o.k.";
            else if (args.Cancelled)
                Info.Text = "close cancelled";
            else
                Info.Text = "close error";
        }

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            this.ImportLibraries("", new List<string>(), false, out _);
            Info.Text = "not supported";
        }

        private void NewBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.NewCAEXFile(out var args))
                Info.Text = "new o.k.";
            else if (args.Cancelled)
                Info.Text = "new cancelled";
            else
                Info.Text = "new error";
        }

        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "AutomationML Files (.aml)|*.aml",
                FilterIndex = 1
            };
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                if (this.OpenCAEXFile(dlg.FileName, out var args))
                    Info.Text = "open o.k.";
                else if (args.Cancelled)
                    Info.Text = "open cancelled";
                else
                    Info.Text = "open error";
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.SaveCAEXFile(out var args))
                Info.Text = "save o.k.";
            else if (args.Cancelled)
                Info.Text = "save cancelled";
            else
                Info.Text = "save error";
        }

        #endregion Methods
    }
}