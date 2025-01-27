// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.Plugin.Sandbox.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Aml.Editor.Plugin.Sandbox.Converter
{
    internal class PanesStyleSelector : StyleSelector
    {
        public Style LibraryStyle
        {
            get;
            set;
        }

        public Style PluginStyle
        {
            get;
            set;
        }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            return item switch
            {
                AMLLibraryViewModel => LibraryStyle,
                PluginViewModel => PluginStyle,
                _ => base.SelectStyle(item, container),
            };
        }
    }
}