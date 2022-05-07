// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.
using Aml.Editor.Plugin.Contracts;
using Aml.Engine.CAEX;
using Aml.Engine.Services;
using Aml.Engine.Services.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace Aml.Editor.Plugin.WithToolbar
{
    internal static class EditorCommands
    {
        private static PluginCommand _addCommand;
        private static PluginCommand _delCommand;
        private static PluginCommand _undoCommand;
        private static CAEXBasicObject? _caexBasicObject;
        private static CAEXDocument? _document;

        private static UndoRedoService _undoRedoService;
        private static IUniqueName _uniqueNameService;

        internal static CAEXBasicObject? SelectedObject
        {
            get => _caexBasicObject;
            set
            {
                _caexBasicObject = value;
                if (_caexBasicObject != null)
                    _document = _caexBasicObject.CAEXDocument;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        internal static void ConfigureCommands(List<PluginCommand> commands,
            ImageSource addIcon, ImageSource delIcon, ImageSource undoIcon)
        {
            _addCommand = new PlugInCommand
            {
                Command = new RelayCommand<object>(AddExecute, AddCanExecute),
                CommandName = "Add",
                IsCheckable = false,
                IsChecked = false,
                CommandIcon = addIcon,
                CommandToolTip = "Add an AutomationML object to the selected object"
            };

            _delCommand = new PlugInCommand
            {
                Command = new RelayCommand<object>(DeleteExecute, DeleteCanExecute),
                CommandName = "Delete",
                IsCheckable = false,
                IsChecked = false,
                CommandIcon = delIcon,
                CommandToolTip = "Delete the selected object"
            };

            _undoCommand = new PlugInCommand
            {
                Command = new RelayCommand<object>(UndoExecute, UndoCanExecute),
                CommandName = "Undo",
                IsCheckable = false,
                IsChecked = false,
                CommandIcon = undoIcon,
                CommandToolTip = "Undo last command"
            };

            commands.Add(_addCommand);
            commands.Add(_delCommand);
            commands.Add(_undoCommand);

            _undoRedoService = UndoRedoService.Register();
            _uniqueNameService = UniqueNameService.Register();
        }

        private static void DeleteExecute(object obj)
        {
            LogWithDelete(SelectedObject);
            SelectedObject?.Remove();

            SelectedObject = null;
            CommandManager.InvalidateRequerySuggested();
        }

        private static bool DeleteCanExecute(object arg) => SelectedObject != null;

        private static void UndoExecute(object obj)
        {
            _undoRedoService.Undo(_document);
            SelectedObject = null;
            CommandManager.InvalidateRequerySuggested();
        }

        private static bool UndoCanExecute(object arg) => _document != null && _undoRedoService.CanUndo(_document);

        public static ObservableCollection<LogViewModel> LogBook { get; } = new();

        private static void LogWithAdd(CAEXBasicObject? caex)
        {
            if (caex == null)
            {
                return;
            }

            var log = LogBook.FirstOrDefault(l => l.Type == caex.TagName);
            if (log == null)
            {
                log = new()
                {
                    Type = caex.TagName
                };

                LogBook.Add(log);
            }
            log.Added++;
        }

        private static void LogWithDelete(CAEXBasicObject? caex)
        {
            if (caex == null)
            {
                return;
            }
            var log = LogBook.FirstOrDefault(l => l.Type == caex.TagName);
            if (log == null)
            {
                log = new()
                {
                    Type = caex.TagName
                };
                LogBook.Add(log);
            }
            log.Deleted++;
        }

        private static void AddExecute(object obj)
        {
            CAEXBasicObject? added = null;

            switch (_caexBasicObject)
            {
                case InternalElementType ie:
                    added = ie.InternalElement.Append();
                    break;

                case RoleFamilyType rc:
                    added = rc.RoleClass.Append();
                    break;

                case SystemUnitFamilyType sc:
                    added = sc.InternalElement.Append();
                    break;

                case InterfaceFamilyType sc:
                    added = sc.InterfaceClass.Append();
                    break;

                case AttributeType at:
                    added = at.Attribute.Append();
                    break;

                case AttributeFamilyType at:
                    added = at.Attribute.Append();
                    break;
            }
            LogWithAdd(added);
            CommandManager.InvalidateRequerySuggested();
        }

        private static bool AddCanExecute(object arg) => SelectedObject != null;
    }
}