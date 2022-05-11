using Aml.Editor.API;
using Aml.Engine.CAEX.Extensions;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace Aml.Editor.Plugin.Sandbox.ViewModels
{
    internal class CommandExecution
    {
        #region Private Fields
        private static CommandExecution Instance;
        private AMLEditor _amlEditor;
        #endregion Private Fields

        #region Private Constructors

        private CommandExecution()
        {
            _amlEditor = new AMLEditor();
            AMLEditor.CommandInvocation += AMLEditor_CommandInvocation;
        }

        #endregion Private Constructors

        #region Internal Methods

        internal static void InitCommandExecution()
        {
            Instance = new CommandExecution();
        }

        #endregion Internal Methods

        #region Private Methods

        private void AMLEditor_CommandInvocation(object sender, Aml.Editor.API.Commanding.CommandExecutionEventArgs args)
        {
            switch (args)
            {
                case AMLEditorCommandExecutedEventArgs ed:
                    Task.Run (()=> ExecuteEditorCommandAsync(ed));
                    break;

                default:
                    break;
            }
        }

        private async Task ExecuteEditorCommandAsync(AMLEditorCommandExecutedEventArgs ed)
        {
            switch (ed.Command)
            {
                case AMLEditorCommandType.OpenDocument:
                    await MainViewModel.Instance.View?.Dispatcher.BeginInvoke(new Action( ()=> OpenDocument(ed)));
                    break;

                case AMLEditorCommandType.CloseDocument:
                    await MainViewModel.Instance.View?.Dispatcher.BeginInvoke(new Action(() => CloseDocument(ed)));
                    break;


                case AMLEditorCommandType.SelectByPath:
                    await MainViewModel.Instance.View?.Dispatcher.BeginInvoke(new Action(() => SelectByPath (ed)));
                    break;


                case AMLEditorCommandType.SelectByID:
                    await MainViewModel.Instance.View?.Dispatcher.BeginInvoke(new Action( () => SelectByID(ed)));
                    break;


                case AMLEditorCommandType.ExpandByPath:
                    await MainViewModel.Instance.View?.Dispatcher.BeginInvoke (new Action(()=> ExpandByPath(ed)));
                    break;


                case AMLEditorCommandType.ExpandByID:
                    await MainViewModel.Instance.View?.Dispatcher.BeginInvoke(new Action(() => ExpandByID(ed)));
                    break;

                default:
                    break;
            }
            AMLEditor.AMLApplication.EndExecution(ed);
        }

        private void CloseDocument(AMLEditorCommandExecutedEventArgs cmd)
        {
            if (MainViewModel.Instance.CloseCommand.CanExecute(cmd.Document))
            { 
                MainViewModel.Instance.CloseCommand.Execute(cmd.Document);
            }
        }

        internal void ExpandByPath(AMLEditorCommandExecutedEventArgs cmd)
        {
            if (cmd.CommandArgument is string path && !string.IsNullOrEmpty(path))
            {
                var caexObject = MainViewModel.Instance.ActiveDocument.Document.FindByPath(path);
                if (caexObject != null)
                {
                    MainViewModel.Instance.ActiveDocument.Expand(caexObject);
                }
            }
        }

        internal void SelectByPath(AMLEditorCommandExecutedEventArgs cmd)
        {
            if (cmd.CommandArgument is string path && !string.IsNullOrEmpty(path))
            {
                var caexObject = MainViewModel.Instance.ActiveDocument.Document.FindByPath(path);
                if (caexObject != null)
                {
                    _ = MainViewModel.Instance.ActiveDocument.Select(caexObject);
                }
            }
        }


        internal void SelectByID(AMLEditorCommandExecutedEventArgs cmd)
        {
            if (cmd.CommandArgument is string path && !string.IsNullOrEmpty(path))
            {
                var caexObject = MainViewModel.Instance.ActiveDocument.Document.FindByID(path);
                if (caexObject != null)
                {
                    _ = MainViewModel.Instance.ActiveDocument.Select(caexObject);
                }
            }
        }

        internal void ExpandByID(AMLEditorCommandExecutedEventArgs cmd)
        {
            if (cmd.CommandArgument is string path && !string.IsNullOrEmpty(path))
            {
                var caexObject = MainViewModel.Instance.ActiveDocument.Document.FindByID(path);
                if (caexObject != null)
                {
                    MainViewModel.Instance.ActiveDocument.Expand(caexObject);
                }
            }
        }


        private void OpenDocument(AMLEditorCommandExecutedEventArgs args)
        {
            if (AMLEditor.AMLApplication.CancelExecution(
                new AMLEditorCommandExecutingEventArgs(args.Command, args.CommandArgument, args.Document)))
            {
                return;
            }

            var filePath = args.CommandArgument.ToString();
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return;
            }
           
            MainViewModel.Instance.OpenDocument(filePath);           
            args.Document = new AMLDocument(filePath);
        }

        #endregion Private Methods
    }
}