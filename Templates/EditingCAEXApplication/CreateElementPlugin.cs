// *********************************************************************** 
// Assembly :
// CreateElementPlugin Author : Josef Prinz Created : 12-08-2014
// 
// Last Modified By : Josef Prinz Last Modified On : 28-02-2018 
// ***********************************************************************
// <copyright file="CreateElementPlugin.cs" company="AutomationML e.V.">
//     Copyright (c) inpro. All rights reserved.
// </copyright>
// <summary>
// </summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Aml.Editor.Plugin.Contracts;
using Aml.Engine.CAEX;

/// <summary>
/// The AutomationML.Plugin.Examples namespace.
/// </summary>
namespace Aml.Editor.Plugin
{
    /// <summary>
    /// The Class CreateElementPlugin is an example for a PlugIn, which has it's own User Interface
    /// and which is an editing PlugIn. The Export Attribute of this class enables the AutomationML
    /// Editor to load the PlugIn with the <a
    /// href="http://msdn.microsoft.com/en-us/library/dd460648%28v=vs.110%29.aspx">Microsoft Managed
    /// Extensibility Framework</a>.    /// 
    /// Whenever an Editing PlugIn is activated, the AutomationML Editor will block all User
    /// Interactions until the PlugIn is terminated.    /// 
    /// The UI is started in its own UI Thread. The Synchronization of Method Calls between the
    /// AMLEditors Thread and the UI Thread is managed via a synchronization Context. The Context is
    /// needed for sending events back to the AMLEditor from the PlugIn UI
    /// </summary>
    [Export(typeof(IAMLEditorPlugin))]
    public class CreateElementPlugin : Base.PluginBase
    {
        /// <summary>
        /// <see cref="AboutCommand"/>
        /// </summary>
        private RelayCommand<object> aboutCommand;

        /// <summary>
        /// The UI for PlugIn Interaction.
        /// </summary>
        private CreateElementUI ui;

        /// <summary>
        /// The view model for the PlugIn
        /// </summary>
        private CreateElementViewModel viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateElementPlugin"/> class.
        /// </summary>
        public CreateElementPlugin()
        {
            // Add the About Command (recommended to exist in any PlugIn)
            Commands.Add(new PluginCommand()
            {
                CommandName = "About",
                Command = AboutCommand
            });

            this.DisplayName = "InternalElement Generator"; 
        }

      
        /// <summary>
        /// The AboutCommand - Command
        /// </summary>
        /// <value>The about command.</value>
        public System.Windows.Input.ICommand AboutCommand
        {
            get
            {
                return this.aboutCommand
                ??
                (this.aboutCommand = new RelayCommand<object>(this.AboutCommandExecute, this.AboutCommandCanExecute));
            }
        }
              

        /// <summary>
        /// Gets a value indicating whether this instance is reactive. Reactive PlugIn will be
        /// notified, when the actual CAEX-Object changes (Selection of the Tree view Item) <see
        /// cref="ChangeAMLFilePath"/> and <see cref="ChangeSelectedObject"/>.
        /// </summary>
        /// <value><c>true</c> if this instance is reactive; otherwise, <c>false</c>.</value>
        public override bool IsReactive
        {
            // this one is not reactive
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read only. No CAEX Objects should be
        /// modified by the PlugIn, when set to true. If a PlugIn is Read only, the AmlEditor is
        /// still enabled, when the PlugIn is Active. If a PlugIn is not read only the Editor is
        /// disabled during activation. Please note, that the Editor will get disabled, if only one
        /// of the currently activated PlugIns of the Editor is not read only.
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        public override bool IsReadonly
        {
            // this one is not read only, it can change the AML Document
            get { return false; }
        }

        public override string PackageName => "";


        /// <summary>
        /// Changes the AML file path. This method is called for a reactive PlugIn <see
        /// cref="IsReactive"/> only. Those PlugIns will be informed, when the loaded AutomationML
        /// Document in the AMLEditor changes. This can only happen, if the plugIn is read only and
        /// the AMLEditor is not disabled. The AMLEditor will be disabled for active PlugIns, which
        /// are not read only.
        /// </summary>
        /// <param name="amlFilePath">The AML file path.</param>
        public override void ChangeAMLFilePath(string amlFilePath)
        {
            // the plugIn is neither reactive not read only. Nothing has to be done here
            ;
        }

        /// <summary>
        /// Changes the selected object. The Host Application will call this method when the PlugIn
        /// <see cref="IsReactive"/> is set to true and the Current Selection changes in the Host Application.
        /// </summary>
        /// <param name="selectedObject">The selected object.</param>
        public override void ChangeSelectedObject(CAEXBasicObject selectedObject)
        {
            // the plugIn is neither reactive not read only. Nothing has to be done here
            ;
        }

       
        /// <summary>
        /// This Method is called on activation of a PlugIn. The AutomationML Editor 'publishes' its
        /// current state to the plugIn, that is the Path of the loaded AutomationML Document and
        /// the currently selected AutomationML Object'. Please note, that the objects may be empty
        /// or null.
        /// </summary>
        /// <param name="amlFilePath">   The AML file path, may be empty.</param>
        /// <param name="selectedObject">The selected object, may be null.</param>
        public override void PublishAutomationMLFileAndObject(string amlFilePath, CAEXBasicObject selectedObject)
        {
            // inform the View Model to load the document the View Model belongs to a different UI
            // thread, we need the dispatcher to send the change to the UI
            this.ui.Dispatcher.Invoke(DispatcherPriority.Normal,
                new ThreadStart(() => { this.viewModel.AmlFilePath = amlFilePath; }));
        }

        /// <summary>
        /// Test, if the <see cref="AboutCommand"/> can execute.
        /// </summary>
        /// <param name="parameter">unused.</param>
        /// <returns>true, if command can execute</returns>
        private bool AboutCommandCanExecute(object parameter)
        {
            // Execution is always possible, also for inactive plugIns
            return true;
        }

        /// <summary>
        /// The <see cref="AboutCommand"/> Execution Action.
        /// </summary>
        /// <param name="parameter">unused.</param>
        private void AboutCommandExecute(object parameter)
        {
            var dialog = new About();
            dialog.ShowDialog();
        }

                /// <summary>
        /// The <see cref="StartCommand"/> Execution Action. A new Dispatcher Tread will be created
        /// for the UI-Window. A Synchronization Context is needed to send events back to the AMLEditor
        /// </summary>
        /// <param name="parameter">unused parameter.</param>
        protected override void ActivateCommandExecute(object parameter)
        {
            this.IsActive = true;

            // get the current Synchronization Context (this is the AMLEditors Dispatcher Thread of the Main Window)
            var syncContext = SynchronizationContext.Current;

            if (syncContext != null)
            {
                // Create a thread. The new Thread is the owner of all data objects
                Thread newWindowThread = new Thread(new ThreadStart(() =>
                {
                    // create the viewModel for the UI
                    this.viewModel = new CreateElementViewModel();

                    // Create a new context for the UI Thread, and install it:
                    SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(
                            Dispatcher.CurrentDispatcher));

                    // create the UI
                    this.ui = new CreateElementUI();                   

                    // set the Data Context to the View Model
                    this.ui.DataContext = this.viewModel;

                    // close event needs to be caught
                    this.ui.Closed += (s, e) =>
                    {
                        this.IsActive = false;
                        this.viewModel.SaveCommand.Execute(null);

                        // post the Terminated Event on the Synchronization Context, so that the AMLEditor gets informed
                        syncContext.Post(o =>  this.RaisePluginTerminated (), this);

                        // Shut Down the Dispatcher Thread
                        Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                    };

                    // Showing the UI
                    this.ui.Show();

                    // Notify the Host Application, post the Activation Event on the Synchronization Context
                    syncContext.Post(o =>  this.RaisePluginActivated(), this);

                    // Start the Dispatcher Processing after the Activation Event was raised
                    System.Windows.Threading.Dispatcher.Run();

                    // Extra Code here will be executed only, when the Dispatcher has been terminated

                    // .... 
                }));

                // Set the apartment state
                newWindowThread.SetApartmentState(ApartmentState.STA);
                
                // Make the thread a background thread (not required)
                newWindowThread.IsBackground = true;
                
                // Start the thread
                newWindowThread.Start();
            }
            else
            {
                MessageBox.Show("Couldn't activate the PlugIn UI Thread! No current Synchronization Context exists!");
            }

        }

         
       
        /// <summary>
        /// The <see cref="StopCommand"/> Execution Action on the Dispatcher Thread of the UI
        /// </summary>
        /// <param name="parameter">unused parameter.</param>
        protected override void TerminateCommandExecute(object parameter)
        {
            // we need the dispatcher again, to send the close command to the UI
            this.ui.Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(ui.Close));
        }

       
    }
}