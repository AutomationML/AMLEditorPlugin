// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.Plugin.Contracts;
using Aml.Editor.Plugin.MultiView.ViewModels;
using Aml.Editor.Plugin.WPFBase;
using Aml.Toolkit.ViewModel;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Aml.Editor.Plugin.MultiView
{
    public partial class PluginView : PluginViewBase, INotifyViewActivation, ISupportsThemes
    {
        #region Constructors

        public PluginView()
        {
            InitializeComponent();
            DataContextChanged += PluginView_DataContextChanged;
            IsReactive = true;

            PaneImage = new BitmapImage(
                new Uri("pack://application:,,,/Aml.Editor.Plugin.MultiView;component/Plugin.png"));
        }

        public event ViewActivatedEventHandler ViewActivated;

        private void PluginView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is AMLTreeViewModel model)
            {
                DisplayName = ActiveDocumentViewModel.DisplayName(model);
            }
        }

        public void Activate(string viewName)
        {
            if (viewName == DisplayName)
            {
                ViewActivated?.Invoke(this, new ViewActivationEventArgs(this.DisplayName));
            }
        }

        public void OnThemeChanged(ApplicationTheme theme)
        {
            var applicationTheme = ControlzEx.Theming.ThemeManager.Current.DetectTheme(Application.Current);
            if (applicationTheme != null)
            {
                Plugin.Contract.Theming.ThemeManager.ChangeTheme(this.Resources, applicationTheme.BaseColorScheme);
                ControlzEx.Theming.ThemeManager.Current.ChangeTheme(this, this.Resources, applicationTheme);
            }
        }

        #endregion Constructors

        #region Properties

        public override DockPositionEnum InitialDockPosition => DockPositionEnum.DockContentMaximized;

        public override bool CanClose => false;

        /// <summary>
        /// Use the same package name as the parent plugin
        /// </summary>
        public override string PackageName => "Aml.Editor.Plugin.MultiView";

        #endregion Properties
    }
}