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
using System.Windows.Controls;
using Keysticks.Actions;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Sources;
using Keysticks.UI;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for editing keystroke actions
    /// </summary>
    public partial class BaseKeyControl : UserControl
    {
        // Fields
        private NamedItemList _keyListItems = new NamedItemList();
        private BaseKeyAction _currentAction = new TypeKeyAction();
        private EActionType _actionType = EActionType.TypeKey;

        // Properties
        public EActionType ActionType 
        { 
            get { return _actionType; } 
            set 
            {
                if (_actionType != value)
                {
                    _actionType = value; 
                    DisplayActionType();
                }
            } 
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseKeyControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set the source
        /// </summary>
        /// <param name="context"></param>
        public void SetSource(BaseSource source)
        {
            GUIUtils.PopulateDisplayableListWithKeys(_keyListItems, source.Profile.KeyboardContext);            
        }

        /// <summary>
        /// Control loaded event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.KeyboardKeyCombo.ItemsSource = _keyListItems;
            RefreshDisplay();
        }

        /// <summary>
        /// Handle change of input language
        /// </summary>
        public void KeyboardLayoutChanged(KxKeyboardChangeEventArgs args)
        {
            GUIUtils.PopulateDisplayableListWithKeys(_keyListItems, args);
            RefreshDisplay();
        }
        
        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is BaseKeyAction)
            {
                _currentAction = (BaseKeyAction)action;
                RefreshDisplay();
            }
        }
                
        /// <summary>
        /// Redisplay the action
        /// </summary>
        private void RefreshDisplay()
        {
            if (IsLoaded)
            {
                DisplayActionType();
                if (_currentAction != null)
                {
                    this.KeyboardKeyCombo.SelectedValue = _currentAction.UniqueKeyID;
                    this.IsVirtualCheck.IsChecked = _currentAction.IsVirtualKey;
                    if (_currentAction is TypeKeyAction)
                    {
                        TypeKeyAction tka = (TypeKeyAction)_currentAction;
                        this.AltCheck.IsChecked = tka.IsAltModifierSet;
                        this.ControlCheck.IsChecked = tka.IsControlModifierSet;
                        this.ShiftCheck.IsChecked = tka.IsShiftModifierSet;
                        this.WinCheck.IsChecked = tka.IsWinModifierSet;
                    }
                }
            }     
        }

        /// <summary>
        /// Show the correct information according to the action type
        /// </summary>
        private void DisplayActionType()
        {
            if (IsLoaded)
            {
                // Set text caption
                switch (_actionType)
                {
                    case EActionType.TypeKey:
                        CaptionTextBlock.Text = Properties.Resources.KeyAction_KeyToTypeLabel;
                        ModifiersPanel.Visibility = Visibility.Visible;
                        break;
                    case EActionType.PressDownKey:
                        CaptionTextBlock.Text = Properties.Resources.KeyAction_KeyToPressDownLabel;
                        ModifiersPanel.Visibility = Visibility.Hidden;
                        break;
                    case EActionType.ReleaseKey:
                        CaptionTextBlock.Text = Properties.Resources.KeyAction_KeyToReleaseLabel;
                        ModifiersPanel.Visibility = Visibility.Hidden;
                        break;
                    case EActionType.ToggleKey:
                        CaptionTextBlock.Text = Properties.Resources.KeyAction_KeyToToggleLabel;
                        ModifiersPanel.Visibility = Visibility.Hidden;
                        break;
                }               
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = null;

            NamedItem keyboardKey = (NamedItem)this.KeyboardKeyCombo.SelectedItem;
            if (keyboardKey != null)
            {
                switch (_actionType)
                {
                    case EActionType.TypeKey:
                        TypeKeyAction tka = new TypeKeyAction();
                        tka.IsAltModifierSet = (this.AltCheck.IsChecked == true);
                        tka.IsControlModifierSet = (this.ControlCheck.IsChecked == true);
                        tka.IsShiftModifierSet = (this.ShiftCheck.IsChecked == true);
                        tka.IsWinModifierSet = (this.WinCheck.IsChecked == true);
                        _currentAction = tka;
                        break;
                    case EActionType.PressDownKey:
                        _currentAction = new PressDownKeyAction();
                        break;
                    case EActionType.ReleaseKey:
                        _currentAction = new ReleaseKeyAction();
                        break;
                    case EActionType.ToggleKey:
                        _currentAction = new ToggleKeyAction();
                        break;
                }
                if (_currentAction != null)
                {
                    _currentAction.UniqueKeyID = keyboardKey.ID;
                    _currentAction.IsVirtualKey = (this.IsVirtualCheck.IsChecked == true);
                }
            }

            return _currentAction;
        }

    }
}
