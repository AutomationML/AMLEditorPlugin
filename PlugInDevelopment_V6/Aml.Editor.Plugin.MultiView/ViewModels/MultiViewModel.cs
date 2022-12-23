// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.MVVMBase;
using Aml.Engine.CAEX;

namespace Aml.Editor.Plugin.MultiView.ViewModels
{
    internal class MultiViewModel : ViewModelBase
    {
        private ActiveDocumentViewModel _activeDocument;

        public ActiveDocumentViewModel ActiveDocument
        {
            get
            {
                return _activeDocument;
            }
            set
            {
                _activeDocument?.Unload();
                _activeDocument = value;
                RaisePropertyChanged(nameof(ActiveDocument));
            }
        }

        internal void LoadDocument(string filePath)
        {
            ActiveDocument = new()
            {
                FilePath = filePath,
                Document = CAEXDocument.LoadFromFile(filePath),
            };
        }
    }
}