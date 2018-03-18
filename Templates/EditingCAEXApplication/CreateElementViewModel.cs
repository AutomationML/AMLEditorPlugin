// *********************************************************************** Assembly :
// CreateElementPlugin Author : Josef Prinz Created : 01-20-2015
// 
// Last Modified By : Josef Prinz Last Modified On : 01-20-2015 ***********************************************************************
// <copyright file="CreateElementViewModel.cs" company="AutomationML e.V.">
//     Copyright (c) AutomationML e.V.. All rights reserved.
// </copyright>
// <summary>
// </summary>
// ***********************************************************************
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;
using Aml.Editor.Plugin.Contracts;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;

/// <summary>
/// The Aml.Editor.Plugin namespace.
/// </summary>
namespace Aml.Editor.Plugin
{
    /// <summary>
    /// Class CreateElementViewModel. This ViewModel is the DataModel for the Plugin UI <see cref="CreateElementUI"/>
    /// </summary>
    public class CreateElementViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// <see cref="AmlFilePath"/>
        /// </summary>
        private string amlFilePath;

        /// <summary>
        /// <see cref="CreateCommand"/>
        /// </summary>
        private RelayCommand<object> createCommand;

        /// <summary>
        /// <see cref="ElementCount"/>
        /// </summary>
        private int elementCount;

        /// <summary>
        /// <see cref="InstanceHierarchyCollection"/>
        /// </summary>
        private ObservableCollection<CAEXObject> instanceHierarchies;

        /// <summary>
        /// <see cref="InternalElementCollection"/>
        /// </summary>
        private ObservableCollection<ElementViewModel> internalElements;

        /// <summary>
        /// <see cref="SaveCommand"/>
        /// </summary>
        private RelayCommand<object> saveCommand;

        /// <summary>
        /// <see cref="TotalElementCount"/>
        /// </summary>
        private int totalElementCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateElementViewModel"/> class.
        /// </summary>
        public CreateElementViewModel()
        {
            this.InstanceHierarchyCollection = new ObservableCollection<CAEXObject>();
            this.InternalElementCollection = new ObservableCollection<ElementViewModel>();

            InstanceHierarchies.CurrentChanged += InstanceHierarchies_CurrentChanged;
        }

        /// <summary>
        /// Gets and sets the AmlFilePath and loads the AMLDocument as a CAEXDocument
        /// </summary>
        /// <value>The aml file path.</value>
        public string AmlFilePath
        {
            get
            {
                return amlFilePath;
            }
            set
            {
                if (amlFilePath != value)
                {
                    amlFilePath = value;
                    OnPropertyChanged("AmlFilePath");

                    // load the CAEX Document
                    if (File.Exists(amlFilePath))
                    {
                        document = CAEXDocument.LoadFromFile(amlFilePath);

                        // load the Instance Hierarchy Collection
                        foreach (CAEXObject ih in document.CAEXFile.InstanceHierarchy)
                        {
                            this.InstanceHierarchyCollection.Add(ih);
                        }
                    }

                    InstanceHierarchies.MoveCurrentToFirst();
                    OnPropertyChanged("InstanceHierarchies");
                }
            }
        }

        /// <summary>
        /// The CreateCommand - Command
        /// </summary>
        /// <value>The create command.</value>
        public System.Windows.Input.ICommand CreateCommand
        {
            get
            {
                return this.createCommand
                ??
                (this.createCommand = new RelayCommand<object>(this.CreateCommandExecute, this.CreateCommandCanExecute));
            }
        }

        /// <summary>
        /// Gets and sets the ElementCount
        /// </summary>
        /// <value>The element count.</value>
        public int ElementCount
        {
            get
            {
                return elementCount;
            }
            set
            {
                if (elementCount != value)
                {
                    elementCount = value; OnPropertyChanged("ElementCount");
                }
            }
        }

        /// <summary>
        /// Gets the instance hierarchies.
        /// </summary>
        /// <value>The instance hierarchies.</value>
        public ICollectionView InstanceHierarchies
        {
            get { return CollectionViewSource.GetDefaultView(this.InstanceHierarchyCollection); }
        }

        /// <summary>
        /// Gets and sets the InstanceHierarchies
        /// </summary>
        /// <value>The instance hierarchy collection.</value>
        public ObservableCollection<CAEXObject> InstanceHierarchyCollection
        {
            get
            {
                return instanceHierarchies;
            }
            set
            {
                if (instanceHierarchies != value)
                {
                    instanceHierarchies = value; OnPropertyChanged("InstanceHierarchyCollection");
                }
            }
        }

        /// <summary>
        /// Gets and sets the InternalElements
        /// </summary>
        /// <value>The internal element collection.</value>
        public ObservableCollection<ElementViewModel> InternalElementCollection
        {
            get
            {
                return internalElements;
            }
            set
            {
                if (internalElements != value)
                {
                    internalElements = value; OnPropertyChanged("InternalElementCollection");
                }
            }
        }

        /// <summary>
        /// Gets the internal elements.
        /// </summary>
        /// <value>The internal elements.</value>
        public ICollectionView InternalElements
        {
            get
            {
                return CollectionViewSource.GetDefaultView(this.InternalElementCollection);
            }
        }

        /// <summary>
        /// The SaveCommand - Command
        /// </summary>
        /// <value>The save command.</value>
        public System.Windows.Input.ICommand SaveCommand
        {
            get
            {
                return this.saveCommand
                ??
                (this.saveCommand = new RelayCommand<object>(this.SaveCommandExecute, this.SaveCommandCanExecute));
            }
        }

        /// <summary>
        /// Gets and sets the TotalElementCount
        /// </summary>
        /// <value>The total element count.</value>
        public int TotalElementCount
        {
            get
            {
                return totalElementCount;
            }
            set
            {
                if (totalElementCount != value)
                {
                    totalElementCount = value; OnPropertyChanged("TotalElementCount");
                }
            }
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        internal void Clear()
        {
            this.amlFilePath = "";
            this.document = null;
            this.InstanceHierarchyCollection.Clear();
            this.InternalElementCollection.Clear();
            this.TotalElementCount = 0;
        }

        /// <summary>
        /// Test, if the <see cref="CreateCommand"/> can execute.
        /// </summary>
        /// <param name="parameter">unused parameter.</param>
        /// <returns>true, if command can execute</returns>
        private bool CreateCommandCanExecute(object parameter)
        {
            return this.InstanceHierarchyCollection.Count > 0 && (this.ElementCount > 0) &&
                   this.InstanceHierarchies.CurrentItem != null;
        }

        /// <summary>
        /// The <see cref="CreateCommand"/> Execution Action. Ne Child Elements are added to the
        /// Selected Internal Element. If no Internal Element is selected, the Child
        /// InternalElements are added to the selected InstanceHierarchy.
        /// </summary>
        /// <param name="parameter">unused parameter.</param>
        private void CreateCommandExecute(object parameter)
        {
            if (this.ElementCount > 0)
            {
                var ie = this.InternalElements.CurrentItem as ElementViewModel;

                if (ie != null)
                {
                    int index = this.InternalElementCollection.IndexOf(ie);

                    InternalElementType parent = ie.CAEX as InternalElementType;
                    if (parent != null)
                    {
                        for (int i = 0; i < ElementCount; i++)
                        {
                            var ie_new = parent.New_InternalElement("InternalElement " + (i + 1));
                            var element_new = new ElementViewModel();
                            element_new.Init(ie_new, ie.Level + 1);

                            this.InternalElementCollection.Insert(++index, element_new);
                        }

                        this.TotalElementCount += ElementCount;

                        OnPropertyChanged("InternalElements");
                    }
                }
                else
                {
                    var ih = this.InstanceHierarchies.CurrentItem as InstanceHierarchyType;

                    if (ih != null)
                    {
                        for (int i = 0; i < ElementCount; i++)
                        {
                            var ie_new = ih.New_InternalElement("InternalElement " + (i + 1));
                            var element_new = new ElementViewModel();
                            element_new.Init(ie_new, 1);

                            this.InternalElementCollection.Insert(0, element_new);
                        }

                        this.TotalElementCount += ElementCount;

                        OnPropertyChanged("InternalElements");
                    }
                }
            }
        }

        /// <summary>
        /// The currently selected Element in the InstanceHierarchy Collection chnaged. The
        /// InternalElement Collection will be updated
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">     
        /// The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        private void InstanceHierarchies_CurrentChanged(object sender, System.EventArgs e)
        {
            var ih = this.InstanceHierarchies.CurrentItem as InstanceHierarchyType;

            if (ih != null)
            {
                ShowInternalElements(ih);
            }

            InternalElements.Refresh();
            OnPropertyChanged("InternalElements");
        }

        /// <summary>
        /// Test, if a CAEX Document is loaded and the <see cref="SaveCommand"/> can execute.
        /// </summary>
        /// <param name="parameter">unused parameter.</param>
        /// <returns>true, if command can execute</returns>
        private bool SaveCommandCanExecute(object parameter)
        {
            return this.document != null;
        }

        /// <summary>
        /// The <see cref="SaveCommand"/> Execution Action.
        /// </summary>
        /// <param name="parameter">unused parameter.</param>
        private void SaveCommandExecute(object parameter)
        {
            if (this.document != null)
            {
                this.document.SaveToFile(this.AmlFilePath, false);
            }
        }

        /// <summary>
        /// Shows the internal elements of an InternalElement
        /// </summary>
        /// <param name="ie">   The ie.</param>
        /// <param name="level">The level.</param>
        private void ShowInternalElements(InternalElementType ie, int level)
        {
            var element = new ElementViewModel();
            element.Init(ie, level);

            this.InternalElementCollection.Add(element);
            this.TotalElementCount++;

            foreach (InternalElementType cie in ie.InternalElement)
            {
                // incr. the Hierarchy Level
                ShowInternalElements(cie, level + 1);
            }
        }

        /// <summary>
        /// Shows the internal elements of the Instance Hierarchy (level 0)
        /// </summary>
        /// <param name="ih">The ih.</param>
        private void ShowInternalElements(InstanceHierarchyType ih)
        {
            this.TotalElementCount = 0;
            this.InternalElementCollection.Clear();

            foreach (InternalElementType ie in ih.InternalElement)
            {
                ShowInternalElements(ie, 0);
            }
            OnPropertyChanged("InternalElements");
        }

        #region INotifyPropertyChanged Member

        /// <summary>
        /// The document
        /// </summary>
        private CAEXDocument document;

        /// <summary>
        /// Tritt ein, wenn sich ein Eigenschaftswert ändert.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged Member
    }
}