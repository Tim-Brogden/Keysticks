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
using Keysticks.Core;

namespace Keysticks
{
    /// <summary>
    /// Custom message box
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        // Configuration
        private string _caption;
        private string _message;
        private MessageBoxButton _buttons = MessageBoxButton.OKCancel;
        private bool _firstButtonIsDefault = true;
        private bool _showDontAskOption;
        private bool _isModal = true;
        private Window _parentWindow;

        // State
        private bool _dontAskAgain = false;
        private MessageBoxResult _mbResult = MessageBoxResult.Cancel;

        // Properties
        public bool DontAskAgain { get { return _dontAskAgain; } }
        public MessageBoxResult Result { get { return _mbResult; } }
        public bool IsModal { set { _isModal = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="caption"></param>
        /// <remarks>If opening modally, set parentWindow to a loaded window. If opening modelessly, set IsModal = false before opening.</remarks>
        public CustomMessageBox(Window parentWindow, string message, string caption, MessageBoxButton buttons, bool firstButtonIsDefault, bool showDontAskOption)
        {
            _parentWindow = parentWindow;
            _message = message;
            _caption = caption;
            _buttons = buttons;
            _firstButtonIsDefault = firstButtonIsDefault;
            _showDontAskOption = showDontAskOption;

            InitializeComponent();

            if (parentWindow != null && parentWindow.IsLoaded)
            {
                Owner = parentWindow;
            }
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = _caption;
            this.CustomMessageTextBox.Text = _message;

            this.DontAskAgainCheckBox.Visibility = _showDontAskOption ? Visibility.Visible : Visibility.Collapsed;
            switch (_buttons)
            {
                case MessageBoxButton.OK:
                    this.OKButton.Visibility = Visibility.Visible;
                    this.InformationIcon.Visibility = Visibility.Visible; 
                    this.QuestionIcon.Visibility = Visibility.Collapsed;
                    this.OKButton.Focus();
                    break;
                case MessageBoxButton.OKCancel:
                    this.OKButton.Visibility = Visibility.Visible;
                    this.CancelButton.Visibility = Visibility.Visible; 
                    this.InformationIcon.Visibility = Visibility.Collapsed; 
                    this.QuestionIcon.Visibility = Visibility.Visible;
                    if (_firstButtonIsDefault)
                    {
                        this.OKButton.Focus();
                    }
                    else
                    {
                        this.CancelButton.Focus();
                    }
                    break;
                case MessageBoxButton.YesNo:
                    this.YesButton.Visibility = Visibility.Visible;
                    this.NoButton.Visibility = Visibility.Visible;
                    this.InformationIcon.Visibility = Visibility.Collapsed; 
                    this.QuestionIcon.Visibility = Visibility.Visible; 
                    if (_firstButtonIsDefault)
                    {
                        this.YesButton.Focus();
                    }
                    else
                    {
                        this.NoButton.Focus();
                    }
                    break;
                case MessageBoxButton.YesNoCancel:
                    this.YesButton.Visibility = Visibility.Visible;
                    this.NoButton.Visibility = Visibility.Visible;
                    this.CancelButton.Visibility = Visibility.Visible; 
                    this.InformationIcon.Visibility = Visibility.Collapsed; 
                    this.QuestionIcon.Visibility = Visibility.Visible; 
                    if (_firstButtonIsDefault)
                    {
                        this.YesButton.Focus();
                    }
                    else
                    {
                        this.NoButton.Focus();
                    }
                    break;
            }

            this.ButtonsPanel.HorizontalAlignment = _showDontAskOption ? HorizontalAlignment.Right : HorizontalAlignment.Center;
        }

        /// <summary>
        /// Yes clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            _dontAskAgain = this.DontAskAgainCheckBox.IsChecked == true;
            _mbResult = MessageBoxResult.Yes;
            if (_isModal)
            {
                DialogResult = true;
            }
            this.Close();
        }

        /// <summary>
        /// No clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            _dontAskAgain = this.DontAskAgainCheckBox.IsChecked == true;
            _mbResult = MessageBoxResult.No;
            if (_isModal)
            {
                DialogResult = false;
            }
            this.Close();
        }

        /// <summary>
        /// OK clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            _dontAskAgain = this.DontAskAgainCheckBox.IsChecked == true;
            _mbResult = MessageBoxResult.OK;
            if (_isModal)
            {
                DialogResult = true;
            }
            this.Close();
        }

        /// <summary>
        /// Cancel clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _dontAskAgain = this.DontAskAgainCheckBox.IsChecked == true;
            _mbResult = MessageBoxResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Window closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isModal && _parentWindow is IWindowParent)
            {
                ((IWindowParent)_parentWindow).ChildWindowClosing(this);
            }
        }
    }
}
