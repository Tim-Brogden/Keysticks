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
using System.Windows.Forms;
using Keysticks.Core;
using Keysticks.Actions;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for editing move the pointer actions
    /// </summary>
    public partial class MoveThePointerControl : System.Windows.Controls.UserControl
    {
        // Fields
        private bool _isLoaded = false;
        private MoveThePointerAction _currentAction = new MoveThePointerAction();
        private EActionType _actionType = EActionType.MoveThePointer;

        // Properties
        public EActionType ActionType { get { return _actionType; } set { _actionType = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public MoveThePointerControl()
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
            _isLoaded = true;
            RefreshDisplay();
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is MoveThePointerAction)
            {
                _currentAction = (MoveThePointerAction)action;
                RefreshDisplay();
            }
        }

        /// <summary>
        /// Redisplay the action
        /// </summary>
        private void RefreshDisplay()
        {
            if (_isLoaded && _currentAction != null)
            {
                if (_currentAction.AbsoluteMove)
                {
                    this.AbsoluteRadioButton.IsChecked = true;
                }
                else
                {
                    this.RelativeRadioButton.IsChecked = true;
                }
                if (_currentAction.PercentOrPixels)
                {
                    this.PercentRadioButton.IsChecked = true;
                }
                else
                {
                    this.PixelsRadioButton.IsChecked = true;
                }
                if (_currentAction.RelativeToWindow)
                {
                    this.CurrentWindowRadioButton.IsChecked = true;
                }
                else
                {
                    this.DesktopRadioButton.IsChecked = true;
                }
                this.XAmountSlider.Value = _currentAction.X;
                this.YAmountSlider.Value = _currentAction.Y;

                ConfigureControls();
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = new MoveThePointerAction();
            _currentAction.AbsoluteMove = this.AbsoluteRadioButton.IsChecked == true;
            _currentAction.PercentOrPixels = this.PercentRadioButton.IsChecked == true;
            _currentAction.RelativeToWindow = this.CurrentWindowRadioButton.IsChecked == true;
            _currentAction.X = XAmountSlider.Value;
            _currentAction.Y = YAmountSlider.Value;

            return _currentAction;
        }

        private void MoveTypeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                ConfigureControls();
            }
        }

        private void UnitsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                ConfigureControls();
            }
        }

        /// <summary>
        /// Configure the controls according to the current selections
        /// </summary>
        private void ConfigureControls()
        {
            bool isAbsolute = this.AbsoluteRadioButton.IsChecked == true;
            bool isPercent = this.PercentRadioButton.IsChecked == true;

            double xMax = isPercent ? 100.0 : SystemInformation.VirtualScreen.Width;
            this.XUnitsLabel.Text = isPercent ? "%" : "px";
            this.XAmountSlider.Minimum = isAbsolute ? 0 : -xMax;
            this.XAmountSlider.Maximum = xMax;
            this.XAmountSlider.DecimalPlaces = isPercent ? 2 : 0;

            double yMax = isPercent ? 100.0 : SystemInformation.VirtualScreen.Height;
            this.YUnitsLabel.Text = isPercent ? "%" : "px";
            this.YAmountSlider.Minimum = isAbsolute ? 0 : -yMax;
            this.YAmountSlider.Maximum = yMax;
            this.YAmountSlider.DecimalPlaces = isPercent ? 2 : 0;

            this.WithRespectToPanel.IsEnabled = isAbsolute || isPercent;
        }
    }
}
