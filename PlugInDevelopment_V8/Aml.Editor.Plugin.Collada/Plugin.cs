using Aml.Editor.Plugin.Base;
using Aml.Editor.Plugin.Collada.ViewModels;
using Aml.Editor.Plugin.Contract.Theming;
using Aml.Editor.Plugin.Contracts;
using Aml.Editor.Plugin.Contracts.Commanding;
using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Aml.Engine.AmlObjects.AutomationMLContainer;

namespace Aml.Editor.Plugin.Window
{
    /// <summary>
    /// This plugin defines its own mainw window. It supports a zooming UI.
    /// </summary>   
    public class Plugin : PluginBase, ISupportsUIZoom, ISupportsThemes, IAMLEditorExternalsPlugin
    {
        private PluginView _mainWindow;
        private ApplicationTheme _currentThem;
        private double _zoom = 1.0;
        private string _last;

        public Plugin()
        {
            DisplayName = "COLLADA Viewer";
        }

        protected override void ActivateCommandExecute(object parameter)
        {
            base.ActivateCommandExecute(parameter);
            _mainWindow = new PluginView(_zoom);
            _mainWindow.Closed += OnMainWindowClosed;
            _mainWindow.Show();
            OnThemeChanged (_currentThem);  
        }

        protected override void TerminateCommandExecute(object parameter)
        {
            base.TerminateCommandExecute(parameter);
            _last  =null;
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

        public bool IsExternalsViewer => true;

        public string MIMEType => RelationshipType.Collada.MimeType;

        public override void ChangeAMLFilePath(string amlFilePath)
        {
            _last = null;
        }

        public override void ChangeSelectedObject(CAEXBasicObject selectedObject)
        {
        }

        public override void PublishAutomationMLFileAndObject(string amlFilePath, CAEXBasicObject selectedObject)
        {
            _last = null;
        }

        public void OnUIZoomChanged(double zoomFactor)
        {
            if (_mainWindow?.DataContext is PluginViewModel vm)
            {
                vm.ZoomFactor = zoomFactor;
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
            if (_last == filePath)
            {
                return;
            }
            _last = filePath;
            if (_mainWindow?.DataContext is PluginViewModel vm)
            {
                vm.ImportGeometry(filePath, _mainWindow.view3D);
            }
        }

        public void ViewExternal(RefURIAttributeType attribute, Stream memoryStream)
        {
            // only supported, when used with AML Editor
        }
    }
}
