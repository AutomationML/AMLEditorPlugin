// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.Plugin.Contracts;
using Aml.Editor.Plugin.MultiView.ViewModels;
using Aml.Editor.Plugin.WPFBase;
using Aml.Engine.CAEX;
using Aml.Toolkit.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aml.Editor.Plugin.MultiView
{
    public partial class PluginView : PluginViewBase, INotifyViewActivation
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

        private void PluginView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is AMLTreeViewModel model)
            {
                DisplayName = ActiveDocumentViewModel.DisplayName(model);
            }
        }

        public void Activate(string viewName)
        {
            //no action
        }

        internal void ChangeTheme(string theme)
        {
            Plugin.Contract.Theming.ThemeManager.ChangeTheme(this.Resources, theme);
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