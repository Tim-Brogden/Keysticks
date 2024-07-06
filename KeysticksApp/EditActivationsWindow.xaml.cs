/******************************************************************************
 *
 * Copyright 2019 Tim Brogden
 * All rights reserved. This program and the accompanying materials   
 * are made available under the terms of the Eclipse Public License v1.0  
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *           
 * Contributors: 
 * Tim Brogden - Keysticks application and installer
 *
 *****************************************************************************/
using Keysticks.Actions;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Sources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Keysticks
{
    /// <summary>
    /// Interaction logic for EditActivationsWindow.xaml
    /// </summary>
    public partial class EditActivationsWindow : Window
    {
        // Fields
        private BaseSource _source;
        private StateVector _selectedState;
        private NamedItemList _activationsList;

        // Properties
        public NamedItemList ActivationList { get { return _activationsList; } }

        public EditActivationsWindow(BaseSource source, StateVector selectedState)
        {
            _source = source;
            _selectedState = selectedState;

            InitializeComponent();
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string stateTitle = _source.Utils.GetModeOrPageName(_selectedState);
            string intro = string.Format(Properties.Resources.String_ActivateXControls, stateTitle);
            IntroLabel.Text = intro;

            _activationsList = _source.AutoActivations.GetActivations(_selectedState);
            ActivationsTable.ItemsSource = _activationsList;
        }

        /// <summary>
        /// Cancel clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            e.Handled = true;
        }

        /// <summary>
        /// OK clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
            e.Handled = true;
        }

        /// <summary>
        /// Return whether Delete is allowed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ActivationsTable.SelectedItem != null;
            e.Handled = true;
        }

        /// <summary>
        /// User deleted table row
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteCommand_Executed(object sender, RoutedEventArgs e)
        {
            DeleteSelectedItem();
            e.Handled = true;
        }

        /// <summary>
        /// Delete button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedItem();
            e.Handled = true;
        }

        /// <summary>
        /// Delete the selected activation rule
        /// </summary>
        private void DeleteSelectedItem()
        {
            int selectedIndex = ActivationsTable.SelectedIndex;
            if (selectedIndex > -1 && selectedIndex < _activationsList.Count)
            {
                _activationsList.RemoveAt(selectedIndex);
            }
        }

        /// <summary>
        /// Add button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateWindowAction action = (ActivateWindowAction)ActivationSettings.GetCurrentAction();
            if (action != null)
            {
                int id = _source.AutoActivations.GetFirstUnusedID(_source.AutoActivations.Count + 1);
                AutoActivation activation = new AutoActivation(id, _selectedState, action.ProgramName, action.WindowTitle, action.MatchType);
                _activationsList.Add(activation);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Table selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActivationsTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteButton.IsEnabled = ActivationsTable.SelectedIndex != -1;

            e.Handled = true;
        }
    }
}
