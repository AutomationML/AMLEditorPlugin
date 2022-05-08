using Aml.Editor.Plugin.Base;
using Aml.Editor.Plugin.Contract.Theming;
using Aml.Editor.Plugin.Contracts;
using Aml.Editor.Plugin.Contracts.Commanding;
using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Aml.Editor.Plugin.Window
{
    /// <summary>
    /// This plugin defines its own mainw window. It supports a zooming UI.
    /// </summary>
    [ExportMetadata("Author", "Josef Prinz")]
    [ExportMetadata("Owner", "AutomationML")]
    [ExportMetadata("DisplayName", "COLLADA Viewer")]
    [ExportMetadata("Description",
        "This plugin does not provide a dockable view but implements its own main window to visualize collada models .")]
    [Export(typeof(IAMLEditorPlugin))]
    public class Plugin : PluginBase, ISupportsUIZoom, ISupportsThemes, IEditorCommanding, IAMLEditorExternalsPlugin
    {
        private PluginView _mainWindow;
        private ApplicationTheme _currentThem;
        private double _zoom = 1.0;

        public Plugin()
        {
            DisplayName = "COLLADA Viewer";
        }

        protected override void ActivateCommandExecute(object parameter)
        {
            base.ActivateCommandExecute(parameter);
            _mainWindow = new PluginView();
            _mainWindow.ZoomFactor = _zoom;
            _mainWindow.Closed += OnMainWindowClosed;
            _mainWindow.Show();
            OnThemeChanged (_currentThem);  
        }

        protected override void TerminateCommandExecute(object parameter)
        {
            base.TerminateCommandExecute(parameter);
            if (parameter is bool isClosing && isClosing)
            {
                return;
            }
            _mainWindow?.Close();
        }

        private void OnMainWindowClosed(object sender, EventArgs e)
        {
            if (IsActive)
            {
                TerminateCommandExecute (true);
            }
        }

        public override bool IsReactive => true;

        public override bool IsReadonly => false;

        public override string PackageName => "Aml.Editor.Plugin.Window";

        public CommandExecution EditorCommand { get; set; }

        public bool IsExternalsViewer => throw new NotImplementedException();

        public string MIMEType => throw new NotImplementedException();

        public override void ChangeAMLFilePath(string amlFilePath)
        {
        }

        public override void ChangeSelectedObject(CAEXBasicObject selectedObject)
        {
        }

        public override void PublishAutomationMLFileAndObject(string amlFilePath, CAEXBasicObject selectedObject)
        {
        }

        public void OnUIZoomChanged(double zoomFactor)
        {
            if (_mainWindow != null)
            {
                _mainWindow.ZoomFactor = zoomFactor;
            }
            _zoom = zoomFactor;
        }

        public void OnThemeChanged(ApplicationTheme theme)
        {
            if (_mainWindow == null)
            {
                return;
            }
            _currentThem = theme;
            // detect, which theme is currently used by mahapps.metro
            var applicationTheme = ControlzEx.Theming.ThemeManager.Current.DetectTheme(Application.Current);
            if (applicationTheme != null)
            {
                // change the theme for used toolkit elements. This method must be called in all xaml views
                // where aml.toolkit or aml.skins resources are used.
                ThemeManager.ChangeTheme(_mainWindow.Resources,
                    applicationTheme.BaseColorScheme);

                // change the theme for the used mahapps.metro ui elements
                ControlzEx.Theming.ThemeManager.Current.ChangeTheme(this, _mainWindow.Resources, applicationTheme);
            }
        }

        public void ViewExternal(RefURIAttributeType attribute, string filePath)
        {
            throw new NotImplementedException();
        }

        public void ViewExternal(RefURIAttributeType attribute, Stream memoryStream)
        {
            throw new NotImplementedException();
        }
    }
}
