// ***********************************************************************
// Assembly         : CreateElementPlugin
// Author           : Josef Prinz
// Created          : 01-20-2015
//
// Last Modified By : Josef Prinz
// Last Modified On : 01-20-2015
// ***********************************************************************
// <copyright file="ElementViewModel.cs" company="AutomationML e.V.">
//     Copyright (c) AutomationML e.V. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.ComponentModel;
using Aml.Engine.CAEX;

/// <summary>
/// The Aml.Editor.Plugin namespace.
/// </summary>
namespace Aml.Editor.Plugin
{
    /// <summary>
    /// Class ElementViewModel is used to represent an editable Element of the CAEX Document
    /// </summary>
    public class ElementViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// <see cref="Name" />
        /// </summary>
        private string name;

        /// <summary>
        /// Gets or sets the caex Object.
        /// </summary>
        /// <value>The caex.</value>
        public CAEXObject CAEX { get; set; }

        /// <summary>
        /// Gets or sets the Level in the Element Hierarchy.
        /// </summary>
        /// <value>The level.</value>
        public int Level { get; set; }

        /// <summary>
        /// Gets and sets the Name which is used in the View
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (name != value)
                {
                    name = value; OnPropertyChanged("Name");
                }
            }
        }

        /// <summary>
        /// Initializes the Element with the specified caex - Object and Hierarchy-Level
        /// </summary>
        /// <param name="caex">The caex.</param>
        /// <param name="hierarchyLevel">The level.</param>
        public void Init(CAEXObject caex, int hierarchyLevel)
        {
            CAEX = caex;
            Level = hierarchyLevel;

            string tabs = new String('\t', hierarchyLevel);

            Name = tabs + caex.Name;
        }

        /// <summary>
        /// ToString used in the View to Display the Element
        /// </summary>
        /// <returns>System.String.</returns>
        public override string ToString()
        {
            return Name;
        }

        #region INotifyPropertyChanged Member

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