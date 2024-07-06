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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Keysticks.Core;
using Keysticks.Actions;
using Keysticks.Input;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for editing word prediction actions
    /// </summary>
    public partial class WordPredictionControl : UserControl
    {
        // Fields
        private WordPredictionAction _currentAction = new WordPredictionAction();

        /// <summary>
        /// Constructor
        /// </summary>
        public WordPredictionControl()
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
            if (action != null && action is WordPredictionAction)
            {
                _currentAction = (WordPredictionAction)action;
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
                switch (_currentAction.PredictionEventType)
                {
                    case EWordPredictionEventType.PreviousSuggestion:
                        this.PreviousRadioButton.IsChecked = true; break;
                    case EWordPredictionEventType.NextSuggestion:
                        this.NextRadioButton.IsChecked = true; break;
                    case EWordPredictionEventType.InsertSuggestion:
                        this.InsertRadioButton.IsChecked = true; break;
                    case EWordPredictionEventType.CancelSuggestions:
                        this.CancelRadioButton.IsChecked = true; break;
                }
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = new WordPredictionAction();
            if (this.PreviousRadioButton.IsChecked == true)
            {
                _currentAction.PredictionEventType = EWordPredictionEventType.PreviousSuggestion;
            }
            else if (this.NextRadioButton.IsChecked == true)
            {
                _currentAction.PredictionEventType = EWordPredictionEventType.NextSuggestion;
            }
            else if (this.InsertRadioButton.IsChecked == true)
            {
                _currentAction.PredictionEventType = EWordPredictionEventType.InsertSuggestion;
            }
            else if (this.CancelRadioButton.IsChecked == true)
            {
                _currentAction.PredictionEventType = EWordPredictionEventType.CancelSuggestions;
            }

            return _currentAction;
        }

    }
}
