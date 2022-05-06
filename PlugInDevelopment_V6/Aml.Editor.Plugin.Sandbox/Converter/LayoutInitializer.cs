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
    class LayoutInitializer : ILayoutUpdateStrategy
    {
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

            else if (anchorableToShow.Content is PluginViewModel pluginModel &&
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

        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {

        }

        private void MaximizeLayout (LayoutRoot layout)
        {
            var documentPanels = layout.Descendents().OfType<LayoutPanel>();
            var horizontalPanel = documentPanels.First();
            var verticalPanel = documentPanels.Last();

            var documents = verticalPanel.Descendents().OfType<LayoutDocument>().ToList();
            while (verticalPanel.ChildrenCount > 0)
            {
                verticalPanel.Children.RemoveAt(0);
            }

            var topVertical = AddLayoutItem(verticalPanel, () => new LayoutDocumentPane()) as LayoutDocumentPane;

            foreach (var doc in documents)
            {
                topVertical.Children.Add(doc);
            }
        }


        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
        {
            if (MainViewModel.IsDocument(anchorableToShow.Content))
            {
                var documentPanels = layout.Descendents().OfType<LayoutPanel>();
                var horizontalPanel = documentPanels.First();
                var verticalPanel = documentPanels.Last();

                var topVertical = (verticalPanel.ChildrenCount > 0)
                    ? verticalPanel.Children[0] as LayoutDocumentPaneGroup
                    : AddLayoutItem(verticalPanel, () => new LayoutDocumentPaneGroup()) as LayoutDocumentPaneGroup;

                var bottomVertical = (verticalPanel.ChildrenCount > 1)
                    ? verticalPanel.Children[1] as LayoutDocumentPaneGroup
                    : AddLayoutItem(verticalPanel, () => new LayoutDocumentPaneGroup()) as LayoutDocumentPaneGroup;

                if (anchorableToShow.Content == ActiveDocumentViewModel.InstanceHierarchy)
                {
                    AddDocument(topVertical, anchorableToShow, 0);
                    return true;
                }
                if (anchorableToShow.Content == ActiveDocumentViewModel.SystemUnitClassLib)
                {
                    AddDocument(topVertical, anchorableToShow, 1);
                    return true;
                }
                if (anchorableToShow.Content == ActiveDocumentViewModel.RoleClassLib)
                {
                    AddDocument(bottomVertical, anchorableToShow, 0);
                    return true;
                }
                if (anchorableToShow.Content == ActiveDocumentViewModel.InterfaceClassLib)
                {
                    AddDocument(bottomVertical, anchorableToShow, 1);
                    return true;
                }
                if (anchorableToShow.Content == ActiveDocumentViewModel.AttributeTypeLib)
                {
                    AddDocument(bottomVertical, anchorableToShow, 2);
                    return true;
                }


                if (anchorableToShow.Content is PluginViewModel p
                    && p.IsContentPlugin)
                {
                    var view = p.Plugin as IAMLEditorView;
                    if (view.InitialDockPosition == DockPositionEnum.DockContentMaximized)                   
                    {
                        MaximizeLayout (layout);
                    }
                    p.IsVisible = true;
                    var pane = horizontalPanel.ChildrenCount > 1
                        ? horizontalPanel.Children[1] as LayoutDocumentPane
                        : AddLayoutItem(horizontalPanel, () => new LayoutDocumentPane()) as LayoutDocumentPane;
                    pane.Children.Add(anchorableToShow);
                    return true;
                }
            }

            return false;
        }


        private void AddProperty(LayoutAnchorSide side, LayoutAnchorable content)
        {
            var group = (side.ChildrenCount > 0)
                    ? side.Children[0]
                    : AddLayoutItem(side, () => new LayoutAnchorGroup()) as LayoutAnchorGroup;
            group.Children.Add(content);
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


        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
        {
        }
    }
}
