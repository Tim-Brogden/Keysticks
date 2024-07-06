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

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for displaying error messages
    /// </summary>
    public partial class ErrorMessageControl : UserControl
    {
        private string _message;
        private Exception _exception;
        private static IErrorHandler _errorHandler;

        // Properties
        public static IErrorHandler ErrorHandler { set { _errorHandler = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public ErrorMessageControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Show error message
        /// </summary>
        /// <param name="message"></param>
        public void Show(string message, Exception ex)
        {
            _message = message;
            _exception = ex;
            if (IsLoaded)
            {
                Updated();
            }

            // Log the error if there's an exception
            if (ex != null && _errorHandler != null)
            {
                _errorHandler.HandleError(message, ex);
            }
        }

        /// <summary>
        /// Clear current error message
        /// </summary>
        public void Clear()
        {
            if (_message != null)
            {
                _message = null;
                _exception = null;
                if (_errorHandler != null)
                {
                    _errorHandler.ClearError();
                }
                if (IsLoaded)
                {
                    Updated();
                }
            }
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Updated();
        }

        /// <summary>
        /// Refresh
        /// </summary>
        private void Updated()
        {
            if (_message != null)
            {
                this.MessageTextBlock.Text = _message;
                if (_exception != null)
                {
                    Grid.SetColumnSpan(MessageTextBlock, 1);
                    this.DetailsButton.Visibility = Visibility.Visible;
                }
                else
                {
                    this.DetailsButton.Visibility = Visibility.Hidden;
                    Grid.SetColumnSpan(MessageTextBlock, 2);
                }
                this.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        /// <summary>
        /// Show error details
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_message != null && _exception != null)
            {
                string details = string.Format("{0}{1}{2}:{1}{3}", _message, Environment.NewLine, Properties.Resources.String_Details, _exception.Message);
                Window parentWindow = Window.GetWindow(this);
                CustomMessageBox messageBox = new CustomMessageBox(parentWindow, details, Properties.Resources.String_ErrorDetails, MessageBoxButton.OK, true, false);
                if (parentWindow == null)
                {
                    messageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
                messageBox.ShowDialog();
            }
        }
    }
}
