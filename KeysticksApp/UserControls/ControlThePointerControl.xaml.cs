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

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for editing Control the pointer actions
    /// </summary>
    public partial class ControlThePointerControl : UserControl
    {
        // Fields
        private ControlThePointerAction _currentAction = new ControlThePointerAction();
        private EActionType _actionType = EActionType.ControlThePointer;

        // Properties
        public EActionType ActionType { get { return _actionType; } set { _actionType = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public ControlThePointerControl()
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
            RefreshDisplay();
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is ControlThePointerAction)
            {
                _currentAction = (ControlThePointerAction)action;
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
                if (_currentAction.IsPositionMode)
                {
                    PositionModeRadioButton.IsChecked = true;
                }
                else
                {
                    SpeedModeRadioButton.IsChecked = true;
                }
                if (_currentAction.IsAbsolutePosition)
                {
                    AbsoluteModeRadioButton.IsChecked = true;
                }
                else
                {
                    RelativeModeRadioButton.IsChecked = true;
                }
                FixedSpeedCheckBox.IsChecked = _currentAction.IsFixedSpeed;
                SpeedMultiplierSlider.Value = _currentAction.SpeedMultiplier;
                RadiusSlider.Value = _currentAction.CircleRadiusPercent;
                XOnlyCheckBox.IsChecked = _currentAction.AxisCombo == EAxisCombination.XOnly;
                YOnlyCheckBox.IsChecked = _currentAction.AxisCombo == EAxisCombination.YOnly;
                InvertXCheckBox.IsChecked = _currentAction.InvertX;
                InvertYCheckBox.IsChecked = _currentAction.InvertY;
            }
        }
        
        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = new ControlThePointerAction();
            _currentAction.IsPositionMode = (PositionModeRadioButton.IsChecked == true);
            _currentAction.IsAbsolutePosition = (AbsoluteModeRadioButton.IsChecked == true);
            _currentAction.IsFixedSpeed = (FixedSpeedCheckBox.IsChecked == true);
            _currentAction.SpeedMultiplier = SpeedMultiplierSlider.Value;
            _currentAction.CircleRadiusPercent = RadiusSlider.Value;
            if (XOnlyCheckBox.IsChecked == true)
            {
                _currentAction.AxisCombo = EAxisCombination.XOnly;
            }
            else if (YOnlyCheckBox.IsChecked == true)
            {
                _currentAction.AxisCombo = EAxisCombination.YOnly;
            }
            else
            {
                _currentAction.AxisCombo = EAxisCombination.Both;
            }
            _currentAction.InvertX = InvertXCheckBox.IsChecked == true;
            _currentAction.InvertY = InvertYCheckBox.IsChecked == true;

            return _currentAction;
        }

        /// <summary>
        /// Handle change to speed or position
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpeedOrPositionButton_Checked(object sender, RoutedEventArgs e)
        {
            bool isSpeedMode = (SpeedModeRadioButton.IsChecked == true);
            this.SpeedPanel.Visibility = isSpeedMode ? Visibility.Visible : Visibility.Hidden;
            this.PositionPanel.Visibility = !isSpeedMode ? Visibility.Visible : Visibility.Hidden;

            ValidateAxisOptions();
        }

        /// <summary>
        /// Handle change to absolute or relative
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AbsoluteOrRelativeButton_Checked(object sender, RoutedEventArgs e)
        {
            this.RadiusPanel.IsEnabled = (this.RelativeModeRadioButton.IsChecked == true);

            ValidateAxisOptions();
        }

        /// <summary>
        /// X only option changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XOnlyCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (XOnlyCheckBox.IsChecked == true)
            {
                YOnlyCheckBox.IsChecked = false;
                InvertYCheckBox.IsChecked = false;
            }
            YAxisPanel.IsEnabled = XOnlyCheckBox.IsChecked != true;
        }

        /// <summary>
        /// Y only option changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YOnlyCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (YOnlyCheckBox.IsChecked == true)
            {
                XOnlyCheckBox.IsChecked = false;
                InvertXCheckBox.IsChecked = false;
            }
            XAxisPanel.IsEnabled = YOnlyCheckBox.IsChecked != true;
        }

        /// <summary>
        /// Prevent the user from selecting either single axis option when in absolute position mode
        /// </summary>
        private void ValidateAxisOptions()
        {
            bool forceBothAxes = PositionModeRadioButton.IsChecked == true && AbsoluteModeRadioButton.IsChecked == true;
            if (forceBothAxes)
            {
                XOnlyCheckBox.IsChecked = false;
                YOnlyCheckBox.IsChecked = false;
            }
            XOnlyCheckBox.IsEnabled = !forceBothAxes;
            YOnlyCheckBox.IsEnabled = !forceBothAxes;
        }
    }
}
