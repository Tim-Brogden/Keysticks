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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Sources;

namespace Keysticks
{
    /// <summary>
    /// Create template window
    /// </summary>
    public partial class CreateTemplateWindow : Window
    {
        // Fields
        private StringUtils _utils = new StringUtils();
        private BaseSource _source;
        private NamedItemList _controlSetsList = new NamedItemList();
        private NamedItemList _templateGroupList = new NamedItemList();
        private NamedItemList _gridTypeList = new NamedItemList();
        private NamedItemList _controlTypeList = new NamedItemList();
        private NamedItemList _navigationDisplayList = new NamedItemList();
        private NamedItemList _selectionDisplayList = new NamedItemList();
        private NamedItemList _restrictionsList = new NamedItemList();
        private List<GeneralisedControl> _navigationControlsList;
        private List<GeneralisedControl> _selectionControlsList;
        private string _selectedModeName;
        private int _selectedInsertAfterID;
        private ETemplateGroup _selectedTemplateGroup;
        private EGridType _selectedGridType;
        private ControlsDefinition _selectedControls;

        // Properties
        public string ModeName { get { return _selectedModeName; } }
        public int InsertAfterID { get { return _selectedInsertAfterID; } }
        public ETemplateGroup TemplateGroup { get { return _selectedTemplateGroup; } }
        public EGridType GridType { get { return _selectedGridType; } }
        public ControlsDefinition TemplateControls { get { return _selectedControls; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentWindow"></param>
        /// <param name="profile"></param>
        public CreateTemplateWindow(Window parentWindow, BaseSource source, int insertAfterID)
        {
            _source = source;
            _selectedInsertAfterID = insertAfterID;

            InitializeComponent();

            Owner = parentWindow;

            PopulateControlSetsList();
            PopulateTemplateGroupList();
            PopulateGridTypeList();
            PopulateControlTypesList();
            PopulateRestrictionsList();
        }

        /// <summary>
        /// Populate list of control sets
        /// </summary>
        private void PopulateControlSetsList()
        {
            _controlSetsList.Clear();
            _controlSetsList.Add(new NamedItem(Constants.DefaultID, Properties.Resources.String_AtTheBeginning));
            foreach (NamedItem item in _source.StateTree.SubValues)
            {
                if (item.ID > 0)
                {
                    _controlSetsList.Add(item);
                }
            }
            this.InsertPosCombo.ItemsSource = _controlSetsList;
        }
        
        /// <summary>
        /// Populate template group combo
        /// </summary>
        private void PopulateTemplateGroupList()
        {
            _templateGroupList.Clear();
            foreach (ETemplateGroup group in Enum.GetValues(typeof(ETemplateGroup)))
            {
                _templateGroupList.Add(new NamedItem((int)group, _utils.TemplateGroupToString(group)));
            }
        }

        /// <summary>
        /// Populate grid types combo
        /// </summary>
        private void PopulateGridTypeList()
        {
            _gridTypeList.Clear();
            foreach (EGridType gridType in Enum.GetValues(typeof(EGridType)))
            {
                _gridTypeList.Add(new NamedItem((int)gridType, _utils.GridTypeToString(gridType)));
            }
        }

        /// <summary>
        /// Populate navigation/selection type list
        /// </summary>
        private void PopulateControlTypesList()
        {
            _controlTypeList.Clear();
            foreach (EDirectionMode directionality in Enum.GetValues(typeof(EDirectionMode)))
            {
                _controlTypeList.Add(new NamedItem((int)directionality, _utils.DirectionModeToString(directionality)));
            }
        }

        /// <summary>
        /// Populate restrictions combo items
        /// </summary>
        private void PopulateRestrictionsList()
        {
            _restrictionsList.Clear();
            foreach (EControlRestrictions restriction in Enum.GetValues(typeof(EControlRestrictions)))
            {
                _restrictionsList.Add(new NamedItem((int)restriction, _utils.RestrictionsToString(restriction)));
            }
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
                // Navigation / selection menus
                this.NavigationControlCombo.ItemsSource = _navigationDisplayList;
                this.SelectionControlCombo.ItemsSource = _selectionDisplayList;
                this.GroupCombo.ItemsSource = _templateGroupList;
                this.GridTypeCombo.ItemsSource = _gridTypeList;
                this.NavigationTypeCombo.ItemsSource = _controlTypeList;
                this.SelectionTypeCombo.ItemsSource = _controlTypeList;
                this.RestrictionsCombo.ItemsSource = _restrictionsList;

                // Select items
                this.InsertPosCombo.SelectedValue = _selectedInsertAfterID;
                this.GroupCombo.SelectedIndex = 0;
                this.RestrictionsCombo.SelectedIndex = 0;
                this.GridTypeCombo.SelectedIndex = 0;
                this.SelectionTypeCombo.SelectedIndex = 0;
                this.NavigationTypeCombo.SelectedIndex = 0;

                this.CreateItemNameTextBox.Text = _source.StateTree.SubValues.GetFirstUnusedName(Properties.Resources.String_ControlSet, false, true, _source.StateTree.NumModes);
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
                bool isValid = true;
                string newItemName = this.CreateItemNameTextBox.Text;
                NamedItem insertAfterItem = (NamedItem)this.InsertPosCombo.SelectedItem;
                NamedItem templateGroupItem = (NamedItem)this.GroupCombo.SelectedItem;
                NamedItem gridTypeItem = (NamedItem)this.GridTypeCombo.SelectedItem;
                int navigationControlIndex = this.NavigationControlCombo.SelectedIndex;
                int selectionControlIndex = this.SelectionControlCombo.SelectedIndex;
                NamedItem restrictionsItem = (NamedItem)this.RestrictionsCombo.SelectedItem;
                if (newItemName == "")
                {
                    this.ErrorMessage.Show(Properties.Resources.E_EnterAName, null);
                    isValid = false;
                }
                else if (_source.StateTree.SubValues.GetItemByName(newItemName) != null)
                {
                    this.ErrorMessage.Show(Properties.Resources.E_NameInUse, null);
                    isValid = false;
                }
                else if (insertAfterItem == null)
                {
                    this.ErrorMessage.Show(string.Format(Properties.Resources.E_SelectPosition, Properties.Resources.String_template), null);
                    isValid = false;
                }
                else if (templateGroupItem == null)
                {
                    this.ErrorMessage.Show(Properties.Resources.E_SelectTemplateGroup, null);
                    isValid = false;
                }
                else if (gridTypeItem == null)
                {
                    this.ErrorMessage.Show(Properties.Resources.E_SelectGridType, null);
                    isValid = false;
                }
                else if (navigationControlIndex == -1 ||
                        navigationControlIndex >= _navigationControlsList.Count ||
                        selectionControlIndex == -1 ||
                        selectionControlIndex >= _selectionControlsList.Count ||
                        restrictionsItem == null)
                {
                    this.ErrorMessage.Show(Properties.Resources.E_SelectNavAndSelect, null);
                    isValid = false;
                }

                if (isValid)
                {
                    _selectedModeName = newItemName;
                    _selectedInsertAfterID = insertAfterItem.ID;
                    _selectedTemplateGroup = (ETemplateGroup)templateGroupItem.ID;
                    _selectedGridType = (EGridType)gridTypeItem.ID;
                    GeneralisedControl navigationControl = _navigationControlsList[navigationControlIndex];
                    GeneralisedControl selectionControl = _selectionControlsList[selectionControlIndex];
                    _selectedControls = new ControlsDefinition(navigationControl,
                                                                selectionControl,
                                                                (EControlRestrictions)restrictionsItem.ID);
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
            // Clear any error
            ErrorMessage.Clear();

            e.Handled = true;
        }

        /// <summary>
        /// Navigation type changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigationTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NamedItem navigationTypeItem = (NamedItem)this.NavigationTypeCombo.SelectedItem;
            if (navigationTypeItem != null)
            {
                EDirectionMode directionality = (EDirectionMode)navigationTypeItem.ID;
                _navigationControlsList = _source.Utils.GetControlsWithDirectionType(directionality, null);

                _navigationDisplayList.Clear();
                foreach (GeneralisedControl genControl in _navigationControlsList)
                {
                    string name = _source.Utils.GetGeneralControlName(genControl);
                    _navigationDisplayList.Add(new NamedItem(genControl.ToID(), name));
                }
                this.NavigationControlCombo.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Selection type changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectionTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NamedItem selectionTypeItem = (NamedItem)this.SelectionTypeCombo.SelectedItem;
            if (selectionTypeItem != null)
            {
                EDirectionMode directionality = (EDirectionMode)selectionTypeItem.ID;
                _selectionControlsList = _source.Utils.GetControlsWithDirectionType(directionality, null);

                _selectionDisplayList.Clear();
                foreach (GeneralisedControl genControl in _selectionControlsList)
                {
                    string name = _source.Utils.GetGeneralControlName(genControl);
                    _selectionDisplayList.Add(new NamedItem(genControl.ToID(), name));
                }
                this.SelectionControlCombo.SelectedIndex = 0;
            }
        }
    }
}

