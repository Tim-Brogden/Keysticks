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
using Keysticks.Actions;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for editing mouse button actions
    /// </summary>
    public partial class MouseButtonActionControl : UserControl
    {
        // Fields
        private NamedItemList _mouseButtonListItems = new NamedItemList();
        private MouseButtonAction _currentAction = new MouseButtonAction();
        private EActionType _actionType = EActionType.ClickMouseButton;

        // Properties
        public EActionType ActionType { get { return _actionType; } set { _actionType = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public MouseButtonActionControl()
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
            StringUtils utils = new StringUtils();
            foreach (EMouseState mouseButton in Enum.GetValues(typeof(EMouseState)))
            {
                if (mouseButton != EMouseState.None)
                {
                    _mouseButtonListItems.Add(new NamedItem((int)mouseButton, utils.MouseButtonToString(mouseButton)));
                }
            }
            this.MouseButtonCombo.ItemsSource = _mouseButtonListItems;

            RefreshDisplay();
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is MouseButtonAction)
            {
                _currentAction = (MouseButtonAction)action;
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
                this.MouseButtonCombo.SelectedValue = (int)_currentAction.MouseButton;
            }
        }
        
        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = null;

            NamedItem selectedItem = (NamedItem)this.MouseButtonCombo.SelectedItem;
            if (selectedItem != null)
            {
                _currentAction = new MouseButtonAction();
                _currentAction.MouseButton = (EMouseState)selectedItem.ID;
                switch (_actionType)
                {
                    case EActionType.ClickMouseButton:
                        _currentAction.MouseButtonActionType = EMouseButtonActionType.Click;
                        break;
                    case EActionType.DoubleClickMouseButton:
                        _currentAction.MouseButtonActionType = EMouseButtonActionType.DoubleClick;
                        break;
                    case EActionType.PressDownMouseButton:
                        _currentAction.MouseButtonActionType = EMouseButtonActionType.PressDown;
                        break;
                    case EActionType.ReleaseMouseButton:
                        _currentAction.MouseButtonActionType = EMouseButtonActionType.Release;
                        break;
                    case EActionType.ToggleMouseButton:
                        _currentAction.MouseButtonActionType = EMouseButtonActionType.Toggle;
                        break;
                }
            }

            return _currentAction;
        }

    }
}
