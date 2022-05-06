// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.MVVMBase;
using Aml.Editor.Plugin.Contract.Commanding;
using Aml.Editor.Plugin.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aml.Editor.Plugin.Sandbox.ViewModels
{
    internal class PluginViewModel : ViewModelBase
    {
        public IAMLEditorPluginMetadata Metadata { get; }

        public PluginViewModel(IAMLEditorPlugin plugin, IAMLEditorPluginMetadata metadata)
        {
            Plugin = plugin;
            Metadata = metadata;
            ContentId = plugin.PackageName;
        }

        private bool _isVisible = true;

        private SimpleCommand<object> _closeCommand = null;


        public SimpleCommand<object> CloseCommand => _closeCommand ??= new SimpleCommand<object>(o => true, (o) => IsVisible = false);


        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (Set(ref _isVisible, value))
                {
                    if (Plugin.IsActive && !_isVisible)
                    {
                        Terminate();
                        Plugin.TerminatePlugin.Command.Execute(Plugin);
                    }
                }
            }
        }


        public IAMLEditorPlugin Plugin { get; }

        public static string Folder { get; internal set; }

        internal void Terminate()
        {
            _isVisible = false;
            if (MainViewModel.Documents.Contains(this))
            {
                MainViewModel.Documents.Remove(this);
            }
            else if (MainViewModel.Properties.Contains(this))
            {
                MainViewModel.Properties.Remove(this);
            }

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    child.Terminate();
                }
            }
        }

        internal bool IsContentPlugin => ( Plugin is IAMLEditorView view ) 
            && ( view.InitialDockPosition == DockPositionEnum.DockContent ||
                view.InitialDockPosition == DockPositionEnum.DockContentMaximized ); 

        internal List<PluginViewModel> Children { get; private set;}

        internal void Activate(string displayName)
        {
            if (displayName == Plugin.DisplayName)
            {
                IsActive = true;
            }
            foreach (var child in Children)
            {
                if (child.Plugin.DisplayName == displayName)
                {
                    child.IsActive = true;
                }
            }
        }

        public string ContentId
        {
            get;
            set;
        }

        internal void AddView(IAMLEditorView editorView)
        {
            if (Children == null)
            {
                Children = new List<PluginViewModel>();
            }
            if (Children.Any(p=>p.Plugin==editorView))
            {
                return;
            }
            var plugin = new PluginViewModel(editorView, null);
            plugin.IsVisible = true;
            plugin.ContentId = $"{ContentId}.{editorView.DisplayName}";
            Children.Add(plugin);

            if (plugin.IsContentPlugin)
            {
                MainViewModel.Documents.Add(plugin);
            }
            else
            {
                MainViewModel.Properties.Add(plugin);
            }
        }

        private bool _isActive;

        public bool IsActive
        {
            get => _isActive; 
            set
            { 
                if ( Set(ref _isActive, value) )
                {
                    if (value && Plugin is INotifyViewActivation)
                    {
                        ActiveDocumentViewModel.Activate(Plugin.DisplayName);
                    }
                }
            }
        }

    }
}
