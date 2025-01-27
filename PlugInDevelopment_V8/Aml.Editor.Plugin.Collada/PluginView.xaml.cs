// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.API;
using Aml.Editor.Plugin.Collada.ViewModels;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Aml.Editor.Plugin.Window
{
    public partial class PluginView
    {
        #region Constructors

        public PluginView(double zoom)
        {
            DataContext = new PluginViewModel { ZoomFactor = zoom };
            InitializeComponent();
        }

        #endregion Constructors

        private void LoadBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var appFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var amlFile = Path.Combine(appFolder, "Robots.aml");

            Parallel.Invoke(
                () =>
                {
                    var stream = this.GetType().Assembly.GetManifestResourceStream("Aml.Editor.Plugin.Collada.Resources.Robots.aml");

                    using var amlFileStream = File.Create(amlFile);
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(amlFileStream);
                    amlFileStream.Close();
                },
                () =>
                {
                    var stream = this.GetType().Assembly.GetManifestResourceStream("Aml.Editor.Plugin.Collada.Resources.kr360.dae");
                    using var daeFileStream = File.Create(Path.Combine(appFolder, "kr360.dae"));
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(daeFileStream);
                    daeFileStream.Close();
                },
                () =>
                {
                    var stream = this.GetType().Assembly.GetManifestResourceStream("Aml.Editor.Plugin.Collada.Resources.youbot.dae");
                    using var daeFileStream = File.Create(Path.Combine(appFolder, "youbot.dae"));
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(daeFileStream);
                    daeFileStream.Close();
                });

            // the editor api can be used from any class defined in the plugin dll
            AMLEditor.AMLApplication.CommandExecuted += AMLApplication_CommandExecuted;
            AMLEditor.AMLApplication.OpenAMLDocument(amlFile);
        }

        private void AMLApplication_CommandExecuted(object sender, AMLEditorCommandExecutedEventArgs e)
        {
            switch (e.Command)
            {
                case AMLEditorCommandType.OpenDocument:

                    //AMLEditor.AMLApplication.SelectObjectById("7f5e6211-0fd8-473a-821b-efcadf07061b");
                    AMLEditor.AMLApplication.SelectObjectById("a429a490-c4c0-4835-bd03-e6f3f66ed4e0");
                    AMLEditor.AMLApplication.ExpandObjectByPath("RobotLib");
                    break;
            }
        }
    }
}