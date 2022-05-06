// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.MVVMBase;

namespace Aml.Editor.Plugin.WithToolbar
{
    internal class LogViewModel : ViewModelBase
    {
        private int _added;

        public int Added
        {
            get { return _added; }
            set => Set(ref _added, value);
        }

        private string _type;

        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }


        private int _deleted;

        public int Deleted
        {
            get { return _deleted; }
            set => Set(ref _deleted, value);
        }


    }
}
