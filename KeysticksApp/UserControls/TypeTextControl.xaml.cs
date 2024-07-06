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
    /// Control for editing Type text actions
    /// </summary>
    public partial class TypeTextControl : UserControl
    {
        // Fields
        private TypeTextAction _currentAction = new TypeTextAction();

        /// <summary>
        /// Constructor
        /// </summary>
        public TypeTextControl()
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
            if (action != null && action is TypeTextAction)
            {
                _currentAction = (TypeTextAction)action;
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
                this.TextToTypeTextBox.Text = _currentAction.TextToType;
            }
        }
        
        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = null;

            if (this.TextToTypeTextBox.Text != "" && this.TextToTypeTextBox.Text != Properties.Resources.String_EnterTextHint)
            {
                _currentAction = new TypeTextAction();
                _currentAction.TextToType = this.TextToTypeTextBox.Text;
            }

            return _currentAction;
        }

    }
}
