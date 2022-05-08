// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.Plugin.Contracts;
using Aml.Editor.Plugin.Sandbox.ViewModels;
using AvalonDock.Layout;
using System;
using System.Linq;

namespace Aml.Editor.Plugin.Sandbox.Converter
{
    internal class LayoutInitializer : ILayoutUpdateStrategy
    {
        #region Methods

        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {
           
        }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
        {
        }

        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            anchorableToShow.AutoHideWidth = 256;
            anchorableToShow.AutoHideHeight = 128;
            anchorableToShow.CanShowOnHover = false;

            if (anchorableToShow.Content == ActiveDocumentViewModel.AttributeTree)
            {
                anchorableToShow.CanHide = false;
                AddProperty(layout.RightSide, anchorableToShow);
                return true;
            }
            else
            {
                return DockPlugin(layout, anchorableToShow);
            }
        }

        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
        {
            if (MainViewModel.IsDocument(anchorableToShow.Content))
            {
                GetPanels(layout, out var mainPanel, out var maximizePanel, out var amlPanel);
                GetDocumentGroups(amlPanel, out var topPanel, out var bottomPanel);

                if (anchorableToShow.Content == ActiveDocumentViewModel.InstanceHierarchy)
                {
                    AddDocument(topPanel, anchorableToShow, 0);
                    return true;
                }
                if (anchorableToShow.Content == ActiveDocumentViewModel.SystemUnitClassLib)
                {
                    AddDocument(topPanel, anchorableToShow, 1);
                    return true;
                }
                if (anchorableToShow.Content == ActiveDocumentViewModel.RoleClassLib)
                {
                    AddDocument(bottomPanel, anchorableToShow, 0);
                    return true;
                }
                if (anchorableToShow.Content == ActiveDocumentViewModel.InterfaceClassLib)
                {
                    AddDocument(bottomPanel, anchorableToShow, 1);
                    return true;
                }
                if (anchorableToShow.Content == ActiveDocumentViewModel.AttributeTypeLib)
                {
                    AddDocument(bottomPanel, anchorableToShow, 2);
                    return true;
                }

                if (anchorableToShow.Content is PluginViewModel p
                    && p.IsContentPlugin)
                {
                    var view = p.Plugin as IAMLEditorView;
                    LayoutDocumentPane documentPane;

                    if (view.InitialDockPosition == DockPositionEnum.DockContentMaximized)
                    {
                        MaximizeLayout(layout);
                        documentPane = PluginDocumentPane(maximizePanel);
                    }
                    else
                    {
                        documentPane = PluginDocumentPane(mainPanel);
                    }
                    p.IsVisible = true;
                    documentPane.Children.Add(anchorableToShow);
                    return true;
                }
            }

            return false;
        }

        internal void MaximizeLayout(LayoutRoot layout)
        {
            GetPanels(layout, out var mainPanel, out var maximizePanel, out var amlPanel);

            if (!amlPanel.Children.Any(p => p is LayoutDocumentPaneGroup g && g.ChildrenCount > 0))
            {
                return;
            }

            var amlDocuments = layout.Descendents().OfType<LayoutDocument>().
                Where(l => l.Content is AMLLibraryViewModel).ToList();

            var amlProperties = layout.Descendents().OfType<LayoutAnchorable>().
               Where(l => l.Content is AMLLibraryViewModel).ToList();

            var pluggedDocuments = layout.Descendents().OfType<LayoutDocument>().
                Where(l => l.Content is PluginViewModel).ToList();

            var pluggedProperties = layout.Descendents().OfType<LayoutAnchorable>().
                Where(l => l.Content is PluginViewModel).ToList();

            // remove everything from the aml Panel
            amlPanel.Children.Clear();

            var amlDocumentPane = AddLayoutItem(amlPanel, () => new LayoutDocumentPane()) as LayoutDocumentPane;
            foreach (var doc in amlDocuments)
            {
                doc.Parent?.RemoveChild(doc);
                amlDocumentPane.Children.Add(doc);
            }

            foreach (var anchorableToShow in amlProperties)
            {
                anchorableToShow.Parent?.RemoveChild(anchorableToShow);
                AddProperty(layout.RightSide, anchorableToShow);
            }

            foreach (var anchorableToShow in pluggedProperties)
            {
                anchorableToShow.Parent?.RemoveChild(anchorableToShow);
                DockPlugin(layout, anchorableToShow);
            }

            if (pluggedDocuments.Count > 0)
            {
                var pluggDocumentPane = PluginDocumentPane(maximizePanel);
                foreach (var doc in pluggedDocuments)
                {
                    doc.Parent?.RemoveChild(doc);
                    pluggDocumentPane.Children.Add(doc);
                }
            }
        }

        private bool DockPlugin(LayoutRoot layout, LayoutAnchorable anchorableToShow)
        {
            if (anchorableToShow.Content is PluginViewModel pluginModel &&
               !pluginModel.IsContentPlugin)
            {
                var view = pluginModel.Plugin as IAMLEditorView;
                anchorableToShow.CanHide = view.CanClose;
                pluginModel.IsVisible = true;

                switch (view.InitialDockPosition)
                {
                    case DockPositionEnum.DockLeft:
                        AddProperty(layout.LeftSide, anchorableToShow);
                        return true;

                    case DockPositionEnum.DockRight:
                        AddProperty(layout.RightSide, anchorableToShow);
                        return true;

                    case DockPositionEnum.DockBottom:
                        AddProperty(layout.BottomSide, anchorableToShow);
                        return true;

                    case DockPositionEnum.DockTop:
                        AddProperty(layout.TopSide, anchorableToShow);
                        return true;

                        //case DockPositionEnum.Floating:
                        //    FloatingPane.Children.Add((LayoutAnchorable)plugin);
                        //    break;
                }
            }
            return false;
        }

        internal void RestoreLayout(LayoutRoot layout)
        {
            GetPanels(layout, out var mainPanel, out var maximizePanel, out var amlPanel);

            if (amlPanel.Children.Any(p => p is LayoutDocumentPaneGroup g && g.ChildrenCount > 0))
            {
                return;
            }

            var amlProperties = layout.Descendents().OfType<LayoutAnchorable>().
                Where(l => l.Content is AMLLibraryViewModel).ToList();

            var pluggedProperties = layout.Descendents().OfType<LayoutAnchorable>().
               Where(l => l.Content is PluginViewModel).ToList();

            var amlDocuments = layout.Descendents().OfType<LayoutDocument>().
                Where(l => l.Content is AMLLibraryViewModel).ToList();

            var pluggedDocuments = layout.Descendents().OfType<LayoutDocument>().
                Where(l => l.Content is PluginViewModel).ToList();

            // remove everything from the aml Panel
            amlPanel.Children.Clear();

            GetDocumentGroups(amlPanel, out var topPanel, out var bottomPanel);
            foreach (var anchorableToShow in amlDocuments)
            {
                if (anchorableToShow.Content == ActiveDocumentViewModel.InstanceHierarchy)
                {
                    AddDocument(topPanel, anchorableToShow, 0);
                }
                else if (anchorableToShow.Content == ActiveDocumentViewModel.SystemUnitClassLib)
                {
                    AddDocument(topPanel, anchorableToShow, 1);
                }
                else if (anchorableToShow.Content == ActiveDocumentViewModel.RoleClassLib)
                {
                    AddDocument(bottomPanel, anchorableToShow, 0);
                }
                else if (anchorableToShow.Content == ActiveDocumentViewModel.InterfaceClassLib)
                {
                    AddDocument(bottomPanel, anchorableToShow, 1);
                }
                else if (anchorableToShow.Content == ActiveDocumentViewModel.AttributeTypeLib)
                {
                    AddDocument(bottomPanel, anchorableToShow, 2);
                }
            }

            foreach (var anchorableToShow in amlProperties)
            {
                anchorableToShow.Parent?.RemoveChild(anchorableToShow);
                AddProperty(layout.RightSide, anchorableToShow);
            }

            foreach (var anchorableToShow in pluggedProperties)
            {
                anchorableToShow.Parent?.RemoveChild(anchorableToShow);
                DockPlugin(layout, anchorableToShow);
            }

            if (pluggedDocuments.Count > 0)
            {
                var pluggDocumentPane = PluginDocumentPane(maximizePanel);
                foreach (var doc in pluggedDocuments)
                {
                    pluggDocumentPane.Children.Add(doc);
                }
            }
        }

        private void AddDocument(LayoutDocumentPaneGroup pane, LayoutDocument content, int childIndex)
        {
            var panel = (pane.ChildrenCount > childIndex)
                                ? pane.Children[childIndex] as LayoutDocumentPane
                                : AddLayoutItem(pane, () => new LayoutDocumentPane()) as LayoutDocumentPane;
            
            panel.Children.Add(content);
        }

        private T AddLayoutItem<T>(LayoutGroup<T> paneGroup, Func<T> newItem) where T : class, ILayoutElement
        {
            var documentPane = newItem();
            paneGroup.Children.Add(documentPane);
            return documentPane;
        }

        private void AddProperty(LayoutAnchorSide side, LayoutAnchorable content)
        {
            var group = (side.ChildrenCount > 0)
                    ? side.Children[0]
                    : AddLayoutItem(side, () => new LayoutAnchorGroup());
            group.Children.Add(content);
        }

        private void GetDocumentGroups(LayoutPanel amlPanel, out LayoutDocumentPaneGroup topPanel,
            out LayoutDocumentPaneGroup bottomPanel)
        {
            topPanel = (amlPanel.ChildrenCount > 0)
                ? amlPanel.Children[0] as LayoutDocumentPaneGroup
                : null;
            topPanel ??= AddLayoutItem(amlPanel, () => new LayoutDocumentPaneGroup()) as LayoutDocumentPaneGroup;

            bottomPanel = (amlPanel.ChildrenCount > 1)
                ? amlPanel.Children[1] as LayoutDocumentPaneGroup
                : null;
            bottomPanel ??= AddLayoutItem(amlPanel, () => new LayoutDocumentPaneGroup()) as LayoutDocumentPaneGroup;
        }

        private void GetPanels(LayoutRoot layout, out LayoutPanel mainDocumentPanel, out LayoutPanel maximizePanel, out LayoutPanel amlDocumentPanel)
        {
            mainDocumentPanel = layout.Children.First() as LayoutPanel;
            amlDocumentPanel = mainDocumentPanel.Descendents().OfType<LayoutPanel>().FirstOrDefault
                (p => p.Orientation == System.Windows.Controls.Orientation.Vertical);
            maximizePanel = mainDocumentPanel;

            if (amlDocumentPanel == null)
            {
                amlDocumentPanel = mainDocumentPanel.Descendents().OfType<LayoutPanel>().First();
            }   
        }


        private LayoutDocumentPane PluginDocumentPane(LayoutPanel layoutPanel)
        {
            var documentPanes = layoutPanel.Descendents().OfType<LayoutDocumentPane>();
            foreach (var documentPane in documentPanes)
            {
                if (!documentPane.Children.Any(p => p.Content is AMLLibraryViewModel))
                {
                    return documentPane;
                }
            }
            return AddLayoutItem(layoutPanel, () => new LayoutDocumentPane()) as LayoutDocumentPane;
        }

        #endregion Methods
    }
}