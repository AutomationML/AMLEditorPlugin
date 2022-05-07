// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.Plugin.Sandbox.ViewModels;
using System.Linq;
using System.Windows;

namespace Aml.Editor.Plugin.Sandbox
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            PluginViewModel.Folder = e.Args.FirstOrDefault() ??
                Sandbox.Properties.Settings.Default.PluginFolder;
        }
    }
}