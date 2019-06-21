using Aml.Editor.Plugin.Contracts;
using Aml.Editor.Plugin.Contracts.Commanding;
using Aml.Engine.CAEX;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Aml.Editor.PlugIn.CallingCommands
{
    /// <summary>
    /// Interaktionslogik für PlugIn.xaml
    /// </summary>
    [Export(typeof(IAMLEditorView))]
    public partial class PlugIn : UserControl, IAMLEditorView, IEditorCommanding, INotifyPropertyChanged
    {
        #region Public Constructors

        public PlugIn()
        {
            // Defines the Command list, which will contain user commands, which a user can select
            // via the PlugIn Menu.
            Commands = new List<PluginCommand>();

            DataContext = this;

            // Every PlugIn needs at least an Activation command, which will be called by a user to activate the PlugIn.
            ActivatePlugin = new PluginCommand()
            {
                Command = new RelayCommand<object>(this.StartCommandExecute,
                    this.StartCommandCanExecute),
                CommandName = "Start",
                CommandToolTip = "Start the PlugIn"
            };

            // Every PlugIn should provide a Termination command, which will be called when the PlugIn window is closed by the user. This can only
            // occur, if the PlugIn view is embedded in a docking window by the Editor.
            TerminatePlugin = new PluginCommand()
            {
                Command = new RelayCommand<object>(this.StopCommandExecute, this.StopCommandCanExecute),
                CommandName = "Stop",
                CommandToolTip = "Stop the PlugIn"
            };

            InitializeComponent();


            // Add the StartCommand (should exist in any PlugIn)
            Commands.Add(ActivatePlugin);

            // Add the Stop Command (should exist in any PlugIn)
            Commands.Add(TerminatePlugin);

        }

        #endregion Public Constructors

        #region Public Events

        public event EventHandler PluginActivated;

        public event EventHandler PluginTerminated;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        public PluginCommand ActivatePlugin { get; private set; }
        public bool CanClose => true;

        public List<PluginCommand> Commands { get; private set; }

        public string DisplayName => "CommandingPlugIn";
        public CommandExecution EditorCommand { get; set; }
        public DockPositionEnum InitialDockPosition => DockPositionEnum.DockLeft;

        public bool IsActive { get; private set; }
        
        public bool IsReactive => true;
        public bool IsReadonly => true;
        public string PackageName => "";
        public BitmapImage PaneImage => null;
        public PluginCommand TerminatePlugin { get; private set; }

        bool IAMLEditorPlugin.IsAutoActive { get; set; }

        #endregion Public Properties

        #region Public Methods

        public void ChangeAMLFilePath(string amlFilePath)
        {
            ;
        }

        public void ChangeSelectedObject(CAEXBasicObject selectedObject)
        {
            ;
        }

        public void ExecuteCommand(PluginCommandsEnum command, string amlFilePath)
        {
            switch (command)
            {
                case PluginCommandsEnum.Terminate:
                    StopCommandExecute(null);
                    break;
            }
        }

        public void PublishAutomationMLFileAndObject(string amlFilePath, CAEXBasicObject selectedObject)
        {
            ;
        }

        #endregion Public Methods


        /// <summary>
        /// Test, if the <see cref="StartCommand"/> can execute. The <see cref="IsActive"/> Property
        /// should be false prior to Activation.
        /// </summary>
        /// <param name="parameter">unused</param>
        /// <returns>true, if command can execute</returns>
        private bool StartCommandCanExecute(object parameter)
        {
            return !this.IsActive;
        }

        /// <summary>
        /// The <see cref="StartCommand"/> s execution Action. The <see cref="PluginActivated"/>
        /// event is raised and the <see cref="IsActive"/> Property is set to true.
        /// </summary>
        /// <param name="parameter">unused</param>
        private void StartCommandExecute(object parameter)
        {
            this.IsActive = true;
            PluginActivated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Test, if the <see cref="StopCommand"/> can execute.
        /// </summary>
        /// <param name="parameter">unused</param>
        /// <returns>true, if command can execute</returns>
        private bool StopCommandCanExecute(object parameter)
        {
            return this.IsActive;
        }

        /// <summary>
        /// The <see cref="StopCommand"/> Execution Action sets the <see cref="IsActive"/> Property
        /// to false. The <see cref="PluginTerminated"/> event will be raised.
        /// </summary>
        /// <param name="parameter">unused</param>
        private void StopCommandExecute(object parameter)
        {
            this.IsActive = false;
            PluginTerminated?.Invoke(this, EventArgs.Empty);
        }


        public string Infos { get; set; }

        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if ((bool)ofd.ShowDialog())
            {
                if (this.OpenCAEXFile(ofd.FileName, out var args))
                    Infos = "open o.k.";
                else if (args.Cancelled)
                    Infos = "open cancelled";
                else
                    Infos = "open error";

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Infos"));
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.CloseCAEXFile(out var args))
                Infos = "close o.k.";
            else if (args.Cancelled)
                Infos = "close cancelled";
            else
                Infos = "close error";

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Infos"));
        }

        private void NewBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.NewCAEXFile(out var args))
                Infos = "new o.k.";
            else if (args.Cancelled)
                Infos = "new cancelled";
            else
                Infos = "new error";

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Infos"));
        }

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if ((bool)ofd.ShowDialog())
            {
                if (this.ImportLibraries(ofd.FileName, new List<string>(), false, out var args))
                    Infos = "import o.k.";
                else if (args.Cancelled)
                    Infos = "import cancelled";
                else
                    Infos = "import error";

                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs("Infos"));
            }
        }

        private void ImportBtnSilent_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if ((bool)ofd.ShowDialog())
            {
                CAEXDocument doc = CAEXDocument.LoadFromFile(ofd.FileName);
                doc.CAEXFile.Select(c => c.Name).ToList();

                if (this.ImportLibraries(ofd.FileName, doc.CAEXFile.Select(c => c.Name).ToList(), true, out var args))
                    Infos = "import o.k.";
                else if (args.Cancelled)
                    Infos = "import cancelled";
                else
                    Infos = "import error";

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Infos"));
            }

        }

    }
}