// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using System.ComponentModel;

namespace Aml.Editor.Plugin.Window
{
    public partial class PluginView : INotifyPropertyChanged
    {
        #region Constructors

        public PluginView()
        {
            DataContext = this;
            InitializeComponent();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private double _zoomFactor=1;

        public double ZoomFactor
        {
            get { return _zoomFactor; }
            set
            {
                _zoomFactor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomFactor)));
            }
        }

        #endregion Constructors
    }
}