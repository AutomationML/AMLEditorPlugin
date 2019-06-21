using Aml.Editor.Plugin.Contracts;
using Aml.Engine.CAEX;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Aml.Editor.PlugIn.WithToolBar
{
    /// <summary>
    /// Interaktionslogik für PlugIn.xaml
    /// </summary>
    [Export(typeof(IAMLEditorView))]
    public partial class PlugIn : UserControl, IAMLEditorView, IToolBarIntegration
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


            Counter.Text = "0";

            // Add the StartCommand (should exist in any PlugIn)
            Commands.Add(ActivatePlugin);

            // Add the Stop Command (should exist in any PlugIn)
            Commands.Add(TerminatePlugin);

            // ToolBar Integration
            ToolBarCommands = new List<PluginCommand>();

            CountAddCommand = new PluginCommand
            {
                Command = new RelayCommand<object>(this.CountAddExecute, this.CountExecuteCanExecute),
                CommandName = "Add",
                IsCheckable = false,
                IsChecked = false,
                CommandIcon = new BitmapImage(new Uri("pack://application:,,,/PluginWithToolBar;component/Resource/appbar.add.png")),
                CommandToolTip = "Add 1"
            };

            CountSubCommand = new PluginCommand
            {
                Command = new RelayCommand<object>(this.CountSubExecute, this.CountExecuteCanExecute),
                CommandName = "Sub",
                IsCheckable = false,
                IsChecked = false,
                CommandIcon = new BitmapImage(new Uri("pack://application:,,,/PluginWithToolBar;component/Resource/appbar.minus.png")),
                CommandToolTip = "Sub 1"
            };

            ToolBarCommands.Add(CountAddCommand);
            ToolBarCommands.Add(CountSubCommand);
        }

        #endregion Public Constructors

        #region Public Events

        public event EventHandler PluginActivated;

        public event EventHandler PluginTerminated;

        #endregion Public Events

        #region Public Properties

        public PluginCommand ActivatePlugin { get; private set; }

        public bool CanClose => true;

        public List<PluginCommand> Commands { get; private set; }

        public PluginCommand CountAddCommand { get; }

        public PluginCommand CountSubCommand { get; }

        public string DisplayName => "WithToolBar";

        public DockPositionEnum InitialDockPosition => DockPositionEnum.DockLeft;

        public bool IsActive { get; private set; }

        public bool IsAutoActive { get; set; }

        public bool IsReactive => true;

        public bool IsReadonly => true;

        public string PackageName => "";

        public BitmapImage PaneImage => null;

        public PluginCommand TerminatePlugin { get; private set; }

        public List<PluginCommand> ToolBarCommands { get; private set; }

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

        #region Private Methods

        private void CountAddExecute(object obj)
        {
            Counter.Text = (int.Parse(Counter.Text) + 1).ToString();
        }

        private bool CountExecuteCanExecute(object obj) => true;

        private void CountSubExecute(object obj)
        {
            Counter.Text = (int.Parse(Counter.Text) - 1).ToString();
        }

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

        #endregion Private Methods
    }
}