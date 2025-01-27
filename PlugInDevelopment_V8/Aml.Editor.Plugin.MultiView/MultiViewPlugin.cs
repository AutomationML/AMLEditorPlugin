// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.Plugin.Contracts;
using Aml.Editor.Plugin.MultiView.ViewModels;
using Aml.Editor.Plugin.WPFBase;
using Aml.Engine.CAEX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Aml.Editor.Plugin.MultiView
{
    /// <summary>
    /// An example that shows how a plugin can integrate multiple views
    /// <see cref="IAMLEditorViewCollection"/> into the editor.
    /// </summary>   
    public partial class MultiViewPlugin : PluginViewBase, ISupportsThemes, IAMLEditorViewCollection, INotifyViewActivation,
        INotifyAMLDocumentLoad
    {
        private readonly MultiViewModel _viewModel;
        private readonly List<IAMLEditorSubView> _views;
        private ApplicationTheme _currentTheme;

        public MultiViewPlugin()
        {
            InitializeComponent();
            _viewModel = new MultiViewModel();
            _views = new List<IAMLEditorSubView>();
            DisplayName = "Multiple Views";

            IsReactive = true;
            PaneImage = new BitmapImage(
                new Uri("pack://application:,,,/Aml.Editor.Plugin.MultiView;component/Plugin.png"));
        }

        protected override void TerminateCommandExecute(object arg)
        {
            base.TerminateCommandExecute(arg);
            Clear();
        }

        public override DockPositionEnum InitialDockPosition => DockPositionEnum.DockBottom;

        public override bool CanClose => true;

        public override bool IsReadonly => false;

        public override string PackageName => "Aml.Editor.Plugin.MultiView";

        public int Count => _views.Count;

        public bool IsReadOnly => IsReadonly;

        public IAMLEditorSubView this[int index] { get => _views[index]; set => _views[index] = value; }

        public event EventHandler<IAMLEditorView> ViewAdded;

        // not used
        public event EventHandler<CAEXDocument> IsDocumentLoaded;

        public event ViewActivatedEventHandler ViewActivated;

        /// <summary>
        /// The file is reloaded into an AutomationML document which represents a duplicate of the
        /// open document of the editor.
        /// </summary>
        /// <param name="amlFilePath"></param>
        /// <param name="selectedObject"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void PublishAutomationMLFileAndObject(string amlFilePath, CAEXBasicObject selectedObject)
        {
            if (!string.IsNullOrEmpty(amlFilePath) && File.Exists(amlFilePath))
            {
                base.PublishAutomationMLFileAndObject(amlFilePath, selectedObject);
                LoadFile(amlFilePath);
            }
        }

        public override void ChangeAMLFilePath(string amlFilePath)
        {
            // due to a save as file operation when not loaded before
            if (Count == 0 && !string.IsNullOrEmpty(amlFilePath) && File.Exists(amlFilePath))
            {
                LoadFile(amlFilePath);
            }
        }

        private void LoadFile(string amlFilePath)
        {
            _viewModel.LoadDocument(amlFilePath);
            if (Count == 0)
            {
                foreach (var view in from lib in ActiveDocumentViewModel.Libraries
                                     select new PluginView() { DataContext = lib })
                {
                    Add(view);
                    ViewAdded?.Invoke(this, view);

                    view.OnThemeChanged(_currentTheme);
                }
            }
        }

        public void Activate(string viewName)
        {
           foreach (var view in this)
           {
                // local views are defined with identical display names than editor views
                // the parallel view should be activated by the editor
                if (view.DisplayName == viewName)
                {
                    ViewActivated?.Invoke(view, new ViewActivationEventArgs("") );
                }
           }
        }

        public void ApplicationClose()
        {
            // no action
        }

        public void DocumentLoaded(CAEXDocument document)
        {
            // no action required
        }

        public void DocumentUnLoaded()
        {
            // clear all content
            _viewModel.ActiveDocument = null;
        }

        public int IndexOf(IAMLEditorSubView item) => _views.IndexOf(item);

        public void Insert(int index, IAMLEditorSubView item) => _views.Insert(index, item);

        public void RemoveAt(int index) => _views.RemoveAt(index);

        public void Add(IAMLEditorSubView item) => _views.Add(item);

        public void Clear()
        { 
            _views.Clear();
            _viewModel.ActiveDocument = null;
        }

        public bool Contains(IAMLEditorSubView item) => _views.Contains(item);

        public void CopyTo(IAMLEditorSubView[] array, int arrayIndex) => _views.CopyTo(array, arrayIndex);

        public bool Remove(IAMLEditorSubView item) => _views.Remove(item);

        public IEnumerator<IAMLEditorSubView> GetEnumerator() => _views.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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

                _currentTheme = theme;
                // change the theme for the used mahapps.metro ui elements
                ControlzEx.Theming.ThemeManager.Current.ChangeTheme(this, this.Resources, applicationTheme);

                // propagate theme change to dependent views
                foreach (var view in this.OfType<PluginView>())
                {
                    view.OnThemeChanged(theme);
                }
            }
        }
    }
}