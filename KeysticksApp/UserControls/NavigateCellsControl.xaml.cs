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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Keysticks.Actions;
using Keysticks.Core;
using Keysticks.Event;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for editing keyboard cell navigation actions
    /// </summary>
    public partial class NavigateCellsControl : UserControl
    {
        // Fields
        private NavigateCellsAction _currentAction = new NavigateCellsAction();
        private NamedItemList _directionList = new NamedItemList();

        /// <summary>
        /// Constructor
        /// </summary>
        public NavigateCellsControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Control loaded event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
           PopulateDirectionCombo();
           RefreshDisplay();
        }

        /// <summary>
        /// Set the control for which we are editing
        /// </summary>
        /// <param name="args"></param>
        public void SetInputControl(KxControlEventArgs args)
        {
            if (IsLoaded && _currentAction != null && args != null)
            {
                // Try to select a sensible direction to match the input control
                ELRUDState direction = ELRUDState.None;
                if (args.LRUDState != ELRUDState.None && args.LRUDState != ELRUDState.Centre)
                {
                    switch (args.LRUDState)
                    {
                        case ELRUDState.Left:
                        case ELRUDState.Right:
                        case ELRUDState.Up:
                        case ELRUDState.Down:
                            direction = args.LRUDState;
                            break;
                    }
                }
                else
                {
                    switch (args.ControlID)
                    {
                        case (int)EButtonState.X:
                            direction = ELRUDState.Left; break;
                        case (int)EButtonState.B:
                            direction = ELRUDState.Right; break;
                        case (int)EButtonState.Y:
                            direction = ELRUDState.Up; break;
                        case (int)EButtonState.A:
                            direction = ELRUDState.Down; break;
                    }
                }

                if (direction != ELRUDState.None)
                {
                    this.DirectionCombo.SelectedValue = (int)direction;
                }
            }
        }

        /// <summary>
        /// Add the directions to the combo box
        /// </summary>
        private void PopulateDirectionCombo()
        {
            StringUtils utils = new StringUtils();
            List<ELRUDState> directions = new List<ELRUDState>() { 
                ELRUDState.Left, ELRUDState.Right, ELRUDState.Up, ELRUDState.Down };

            _directionList.Clear();
            foreach (ELRUDState direction in directions)
            {
                _directionList.Add(new NamedItem((int)direction, utils.DirectionToString(direction)));
            }
            this.DirectionCombo.ItemsSource = _directionList;            
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is NavigateCellsAction)
            {
                _currentAction = (NavigateCellsAction)action;
                RefreshDisplay();
            }
        }

        /// <summary>
        /// Redisplay the action
        /// </summary>
        private void RefreshDisplay()
        {
            if (IsLoaded && _currentAction != null)
            {
                this.DirectionCombo.SelectedValue = (int)_currentAction.Direction;
                this.WrapAroundCheckBox.IsChecked = _currentAction.WrapAround;
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = null;

            NamedItem directionItem = (NamedItem)this.DirectionCombo.SelectedItem;
            if (directionItem != null)
            {
                _currentAction = new NavigateCellsAction();
                _currentAction.Direction = (ELRUDState)directionItem.ID;
                _currentAction.WrapAround = this.WrapAroundCheckBox.IsChecked == true;
            }

            return _currentAction;
        }

    }
}
