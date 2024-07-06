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
    /// Control for editing the hold and auto-repeat timings of a virtual control
    /// </summary>
    public partial class SetDwellAndAutorepeatControl : UserControl
    {
        // Fields
        private SetDwellAndAutorepeatAction _currentAction = new SetDwellAndAutorepeatAction();        

        /// <summary>
        /// Constructor
        /// </summary>
        public SetDwellAndAutorepeatControl()
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
            if (action != null && action is SetDwellAndAutorepeatAction)
            {
                _currentAction = (SetDwellAndAutorepeatAction)action;
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
                // Settings
                this.DwellTimeSlider.Value = _currentAction.DwellTimeMS * 0.001;
                this.RepeatIntervalSlider.Value = _currentAction.AutoRepeatInterval * 0.001;
            }
        }
        
        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = new SetDwellAndAutorepeatAction();

            // Settings
            _currentAction.DwellTimeMS = (int)(1000.0 * this.DwellTimeSlider.Value);
            _currentAction.AutoRepeatInterval = (int)(1000.0 * this.RepeatIntervalSlider.Value);

            return _currentAction;
        }

    }
}
