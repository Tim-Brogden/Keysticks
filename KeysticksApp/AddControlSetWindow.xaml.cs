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
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Keysticks.Core;

namespace Keysticks
{
    /// <summary>
    /// Add control set window
    /// </summary>
    public partial class AddControlSetWindow : Window
    {
        // Fields
        private string _itemTypeName;
        private NamedItemList _displayableItems;
        private string _itemName;
        private int _insertAfterID;

        // Properties
        public string ItemName { get { return _itemName; } }
        public int InsertAfterID { get { return _insertAfterID; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentWindow"></param>
        /// <param name="profile"></param>
        public AddControlSetWindow(Window parentWindow, string itemTypeName, NamedItemList itemList, int insertAfterID)
        {
            _itemTypeName = itemTypeName;
            _insertAfterID = insertAfterID;

            InitializeComponent();

            Owner = parentWindow;

            _displayableItems = new NamedItemList();
            _displayableItems.Add(new NamedItem(Constants.DefaultID, Properties.Resources.String_AtTheBeginning));
            foreach (NamedItem item in itemList)
            {
                if (item.ID > 0)
                {
                    _displayableItems.Add(item);
                }
            }
            this.InsertPosCombo.ItemsSource = _displayableItems;
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Title and label
                this.Title = string.Format(Properties.Resources.String_AddANewX, _itemTypeName.ToLower());
                this.CreateItemNameLabel.Text = string.Format(Properties.Resources.String_XName, _itemTypeName);

                // Select insert position
                this.InsertPosCombo.SelectedValue = _insertAfterID;

                this.CreateItemNameTextBox.Text = _displayableItems.GetFirstUnusedName(_itemTypeName, false, true, Math.Max(_displayableItems.Count, 1));
                this.CreateItemNameTextBox.Focus();
                this.CreateItemNameTextBox.SelectAll();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_LoadWindow, ex);
            }
        }
                
        /// <summary>
        /// OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                // Check the name is valid
                string newItemName = this.CreateItemNameTextBox.Text;
                NamedItem insertAfterItem = (NamedItem)this.InsertPosCombo.SelectedItem;
                if (newItemName == "")
                {
                    this.ErrorMessage.Show(Properties.Resources.E_EnterAName, null);
                }
                else if (_displayableItems.GetItemByName(newItemName) != null)
                {
                    this.ErrorMessage.Show(Properties.Resources.E_NameInUse, null);
                }
                else if (_displayableItems.Count != 0 && insertAfterItem == null)
                {
                    string message = string.Format(Properties.Resources.E_SelectPosition, _itemTypeName.ToLower());
                    this.ErrorMessage.Show(message, null);
                }
                else
                {
                    _itemName = newItemName;
                    _insertAfterID = insertAfterItem != null ? insertAfterItem.ID : Constants.DefaultID;
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_InvalidDetails, ex);
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
            if (CreateItemNameTextBox.Text != "")
            {
                // Clear any error
                ErrorMessage.Clear();
            }

            e.Handled = true;
        }

        /// <summary>
        /// Insertion position changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InsertPosCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.InsertPosCombo.SelectedItem != null)
            {
                // Clear any error
                ErrorMessage.Clear();
            }

            e.Handled = true;
        }
    }
}

