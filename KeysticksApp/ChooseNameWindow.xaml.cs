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
using System.Windows;
using System.Windows.Input;
using Keysticks.Core;

namespace Keysticks
{
    /// <summary>
    /// Name chooser window
    /// </summary>
    public partial class ChooseNameWindow : Window
    {
        // Fields
        private string _itemTypeName;
        private NamedItemList _itemList;
        private string _selectedName;

        // Properties
        public string SelectedName { get { return _selectedName; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentWindow"></param>
        /// <param name="profile"></param>
        public ChooseNameWindow(Window parentWindow, string itemTypeName, string suggestedName, NamedItemList itemList)
        {
            _itemTypeName = itemTypeName;
            _selectedName = suggestedName;
            _itemList = itemList;

            InitializeComponent();

            Owner = parentWindow;
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.CreateItemNameLabel.Text = string.Format(Properties.Resources.String_NewXName, _itemTypeName.ToLower());
            this.CreateItemNameTextBox.Text = _selectedName;
            this.CreateItemNameTextBox.Focus();
            this.CreateItemNameTextBox.SelectAll();
        }
                
        /// <summary>
        /// OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Check the name is valid
            ErrorMessage.Clear();
            string newItemName = this.CreateItemNameTextBox.Text;
            if (newItemName == "")
            {
                this.ErrorMessage.Show(Properties.Resources.E_EnterAName, null);
            }
            else if (_itemList.GetItemByName(newItemName) != null)
            {
                this.ErrorMessage.Show(Properties.Resources.E_NameInUse, null);
            }
            else
            {
                _selectedName = newItemName;
                this.DialogResult = true;
                this.Close();
            }

            e.Handled = true;
        }

        /// <summary>
        /// Cancelled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Clear();
            this.Close();

            e.Handled = true;
        }

        /// <summary>
        /// Text changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateItemNameTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // Clear any error
            if (this.CreateItemNameTextBox.Text != "")
            {
                ErrorMessage.Clear();
            }

            e.Handled = true;
        }        
    }
}

