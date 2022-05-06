// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.Plugin.Sandbox.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Aml.Editor.Plugin.Sandbox.Converter
{
    class PanesTemplateSelector : DataTemplateSelector
    {
        public PanesTemplateSelector()
        {
        }

        public DataTemplate LibraryViewTemplate
        {
            get;
            set;
        }

        public DataTemplate PlugInViewTemplate
        {
            get;
            set;
        }


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                AMLLibraryViewModel => LibraryViewTemplate,
                PluginViewModel => PlugInViewTemplate,
                _ => base.SelectTemplate(item, container),
            };
        }
    }
}
