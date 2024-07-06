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
using System.IO;
using Keysticks.Config;
using Keysticks.Core;

namespace Keysticks
{
    /// <summary>
    /// About window
    /// </summary>
    public partial class HelpAboutWindow : Window
    {
        // Fields
        private IMainWindow _parentWindow;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentWindow"></param>
        public HelpAboutWindow(IMainWindow parentWindow)
        {
            _parentWindow = parentWindow;

            InitializeComponent();
        }

        /// <summary>
        /// Window loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Display app details
            this.TitleTextBlock.Text = Properties.Resources.String_About + " " + Constants.ProductName + "...";
            this.CopyrightTextBlock.Text = string.Format("{0} \u00A9 {1} {2}", Properties.Resources.String_Copyright, Constants.ReleaseDateYear, Constants.AuthorName);
            this.VersionTextBlock.Text = string.Format("{0} v{1}", Constants.ProductName, Constants.AppVersionString);
            this.WebsiteHyperlink.NavigateUri = new Uri(Constants.AppWebsiteWithScheme);
            this.WebsiteHyperlink.ToolTip = string.Format(Properties.Resources.String_GoToX, Constants.AppWebsiteWithScheme);
            this.WebsiteTextBlock.Text = Constants.AppWebsiteWithoutScheme;
            
            // Focus the OK button
            OKButton.Focus();

            e.Handled = true;
        }

        /// <summary>
        /// Window closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _parentWindow.ChildWindowClosing(this);
        }

        /// <summary>
        /// Close window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            e.Handled = true;
        }
        
        /// <summary>
        /// Hyperlink clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            OpenURI(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        /// <summary>
        /// Open a web page
        /// </summary>
        /// <param name="uri"></param>
        private void OpenURI(string uri)
        {
            try
            {
                uri = Uri.EscapeUriString(uri);
                ProcessManager.Start(uri);
            }
            catch (Exception)
            {
            }
        }                

        /// <summary>
        /// Help button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string helpFilePath = Path.Combine(AppConfig.ProgramRootDir, "Help", Constants.HelpFileName);
                ProcessManager.Start(helpFilePath);
            }
            catch (Exception)
            {
            }
            e.Handled = true;
        }
        
    }
}
