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
using Keysticks.Core;

namespace Keysticks
{
    /// <summary>
    /// Window for reordering items in a list
    /// </summary>
    public partial class ReorderItemsWindow : Window
    {
        // Fields
        private NamedItemList _displayableItemList = new NamedItemList();
        private string _itemTypeName;
        private NamedItemList _itemList;

        // Properties
        public NamedItemList ItemList { get { return _itemList; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentWindow"></param>
        /// <param name="profile"></param>
        public ReorderItemsWindow(Window parentWindow, string itemTypeName, NamedItemList itemList)
        {
            _itemTypeName = itemTypeName;
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
            try
            {
                // Title
                this.Title = Properties.Resources.Reorder_Title;

                // Bind list
                _displayableItemList.Clear();
                foreach (NamedItem item in _itemList)
                {
                    if (item.ID > 0)
                    {
                        _displayableItemList.Add(item);
                    }
                }
                this.ItemsListBox.ItemsSource = _displayableItemList;

                // Select first item
                if (_displayableItemList.Count > 0)
                {
                    this.ItemsListBox.SelectedIndex = 0;
                }
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

                // Repopulate the list in the new order
                // Make sure we keep any default or special items at the beginning that we didn't allow reordering for
                int i = 0;
                while (i < _itemList.Count)
                {
                    NamedItem item = _itemList[i];
                    if (item.ID > 0)
                    {
                        _itemList.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
                foreach (NamedItem item in _displayableItemList)
                {
                    _itemList.Add(item);
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_ReorderItems, ex);
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
        /// Move up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int selectedIndex = this.ItemsListBox.SelectedIndex;
                if (selectedIndex > 0 && selectedIndex < _displayableItemList.Count)
                {
                    NamedItem itemToMove = _displayableItemList[selectedIndex - 1];
                    _displayableItemList.RemoveAt(selectedIndex - 1);
                    _displayableItemList.Insert(selectedIndex, itemToMove);

                    EnableOrDisableButtons();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_ReorderItems, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Move down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int selectedIndex = this.ItemsListBox.SelectedIndex;
                if (selectedIndex > -1 && selectedIndex < _displayableItemList.Count - 1)
                {
                    NamedItem itemToMove = _displayableItemList[selectedIndex + 1];
                    _displayableItemList.RemoveAt(selectedIndex + 1);
                    _displayableItemList.Insert(selectedIndex, itemToMove);

                    EnableOrDisableButtons();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_ReorderItems, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableOrDisableButtons();

            e.Handled = true;
        }

        /// <summary>
        /// Enable or disable up / down buttons
        /// </summary>
        private void EnableOrDisableButtons()
        {
            int selectedIndex = this.ItemsListBox.SelectedIndex;

            this.MoveUpButton.IsEnabled = (selectedIndex > 0);
            this.MoveDownButton.IsEnabled = (selectedIndex > -1) && (selectedIndex < _displayableItemList.Count - 1);
        }
    }
}

