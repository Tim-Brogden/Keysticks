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

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for displaying an informational message
    /// </summary>
    public partial class InfoMessageControl : UserControl
    {
        private bool _isLoaded = false;
        private string _text = "";

        public string Text { get { return _text; } set { _text = value; Updated(); } }

        /// <summary>
        /// Constructor
        /// </summary>
        public InfoMessageControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;
            Updated();
        }

        /// <summary>
        /// Show error message
        /// </summary>
        /// <param name="message"></param>
        public void Show(string message)
        {
            Text = message;
            this.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Clear current error message
        /// </summary>
        public void Clear()
        {
            this.Visibility = System.Windows.Visibility.Hidden;
        }
        
        /// <summary>
        /// Refresh
        /// </summary>
        private void Updated()
        {
            if (_isLoaded)
            {
                this.MessageTextBlock.Text = _text;
            }
        }
        
    }
}
