// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.Plugin.Contracts;
using Aml.Engine.CAEX;
using Aml.Toolkit.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;

namespace Aml.Editor.Plugin.Sandbox.ViewModels
{
    public class AMLLibraryViewModel : AMLTreeViewModel
    {
        #region fields
        private string _contentId = null;
        #endregion fields

        protected override void SelectedElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.SelectedElementsChanged(sender, e);

            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                var node = e.NewItems.OfType<AMLNodeViewModel>().FirstOrDefault();
                MainModel.PropagateSelectionEventToPlugins(node?.CAEXObject);

                if (node?.CAEXObject is IObjectWithAttributes at &&
                    this != ActiveDocumentViewModel.AttributeTree)
                {
                    ActiveDocumentViewModel.AttributeTree.ClearAll();
                    ActiveDocumentViewModel.AttributeTree.SetRoot(node.CAEXNode);
                    ActiveDocumentViewModel.AttributeTree.MainModel = MainModel;
                    ActiveDocumentViewModel.AttributeTree.Document = Document;
                }
            }
        }

        public new bool IsActive
        {
            get => base.IsActive;
            set
            {
                if ( base.IsActive != value )
                { 
                    base.IsActive = value;
                    if (value )
                    {
                        PropagateActivationToPlugin (DisplayName);
                        if (MainModel.ActiveDocument != null)
                            MainModel.ActiveDocument.ActiveHierarchy = this;
                    }
                }
            }
        }

        private void PropagateActivationToPlugin(string displayName)
        {
            if (MainModel == null)
            {
                return;
            }
            foreach (var plugin in MainModel.Plugins)
            {
                if (plugin.Plugin.IsActive && plugin.Plugin is INotifyViewActivation notify)
                {
                    plugin.Activate(displayName);
                }
            }
        }

        public string ContentId => "Aml.Editor." + DisplayName;

        public AMLLibraryViewModel(XElement rootNode, HashSet<string> caexTagNames) :
            base(rootNode, caexTagNames)
        {
        }

        public string DisplayName { get; set; }

        internal MainViewModel MainModel { get; set; }

        internal ActiveDocumentViewModel Document { get; set; }
    }
}
