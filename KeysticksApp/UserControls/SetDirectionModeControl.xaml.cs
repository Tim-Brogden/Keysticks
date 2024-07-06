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
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Actions;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for editing the behaviour of a directional control (e.g. thumbstick or DPad)
    /// </summary>
    public partial class SetDirectionModeControl : UserControl
    {
        // Fields
        private KxControlEventArgs _inputControl;
        private SetDirectionModeAction _currentAction = new SetDirectionModeAction();

        /// <summary>
        /// Constructor
        /// </summary>
        public SetDirectionModeControl()
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
        /// Set the control for which we are editing
        /// </summary>
        /// <param name="args"></param>
        public void SetInputControl(KxControlEventArgs args)
        {
            _inputControl = args;

            RefreshDisplay();
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is SetDirectionModeAction)
            {
                _currentAction = (SetDirectionModeAction)action;
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
                // Only enable continuous for thumbs
                this.ContinuousButton.IsEnabled = (_inputControl != null) && (_inputControl.ControlType == EVirtualControlType.Stick);

                // Direction mode
                switch (_currentAction.DirectionMode)
                {
                    case EDirectionMode.Continuous:
                        this.ContinuousButton.IsChecked = true; break;
                    case EDirectionMode.FourWay:
                        this.FourWayButton.IsChecked = true; break;
                    case EDirectionMode.EightWay:
                        this.EightWayButton.IsChecked = true; break;
                    default:
                        this.AxisStyleButton.IsChecked = true; break;
                }
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = new SetDirectionModeAction();

            // Direction mode
            if (this.ContinuousButton.IsChecked == true)
            {
                _currentAction.DirectionMode = EDirectionMode.Continuous;
            }
            else if (this.FourWayButton.IsChecked == true)
            {
                _currentAction.DirectionMode = EDirectionMode.FourWay;
            }
            else if (this.EightWayButton.IsChecked == true)
            {
                _currentAction.DirectionMode = EDirectionMode.EightWay;
            }
            else
            {
                _currentAction.DirectionMode = EDirectionMode.AxisStyle;
            }            

            return _currentAction;
        }
    }
}
