// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.MVVMBase;
using Aml.Engine.CAEX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aml.Editor.Plugin.MultiView.ViewModels
{
    internal class MultiViewModel: ViewModelBase
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
                if (_activeDocument != null)
                {
                    _activeDocument.Unload();
                }
                _activeDocument = value;
            }
        }

        internal void LoadDocument (string filePath)
        {
            ActiveDocument = new ()
            {
                FilePath = filePath,
                Document = CAEXDocument.LoadFromFile(filePath),
            };
        }

    }
}
