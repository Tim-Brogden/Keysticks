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

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for editing Wait actions
    /// </summary>
    public partial class WaitControl : UserControl
    {
        // Fields
        private WaitAction _currentAction = new WaitAction();

        /// <summary>
        /// Constructor
        /// </summary>
        public WaitControl()
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
            if (action != null && action is WaitAction)
            {
                _currentAction = (WaitAction)action;
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
                this.DurationSlider.Value = _currentAction.WaitTimeMS * 0.001;
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = new WaitAction();
            _currentAction.WaitTimeMS = (int)(this.DurationSlider.Value * 1000.0);

            return _currentAction;
        }

    }
}
