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
using System.Windows.Input;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.UI;

namespace Keysticks
{
    /// <summary>
    /// Profile preview window
    /// </summary>
    public partial class ProfilePreviewWindow : Window
    {
        // Fields
        private bool _isLoaded;
        private IWindowParent _parentWindow;
        private Profile _profile;
        private EProfileStatus _profileStatus = EProfileStatus.Local;
                
        // Events
        public event RoutedEventHandler DownloadClicked;
        public event RoutedEventHandler ShowInMyProfileClicked;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProfilePreviewWindow(IWindowParent parentWindow)
        {
            _parentWindow = parentWindow;

            InitializeComponent();
        }

        /// <summary>
        /// Set the config
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            ProfileViewer.SetAppConfig(appConfig);
        }

        /// <summary>
        /// Window loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            _isLoaded = true;

            RefreshPreview();
        }

        /// <summary>
        /// Handle window closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _parentWindow.ChildWindowClosing(this);
        }

        /// <summary>
        /// Set the profile to display
        /// </summary>
        /// <param name="profile"></param>
        public void SetProfile(Profile profile, EProfileStatus profileStatus)
        {
            _profile = profile;
            _profileStatus = profileStatus;

            if (_isLoaded)
            {
                RefreshPreview();
            }
        }

        /// <summary>
        /// Handle change of input language
        /// </summary>
        public void KeyboardLayoutChanged(KxKeyboardChangeEventArgs args)
        {
            if (_profile != null)
            {
                // Update profile
                _profile.Initialise(args);

                // Refresh display
                RefreshPreview();
            }
        }
        
        /// <summary>
        /// Allow the user to close by pressing Esc
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Refresh the preview
        /// </summary>
        private void RefreshPreview()
        {
            try
            {
                // Show the initial state
                if (_profile != null)
                {
                    ProfileNameTextBlock.Text = _profile.Name;

                    // Populate controls
                    ProfileViewer.SetProfile(_profile);
                    ProfileViewer.Visibility = Visibility.Visible;

                    if (_profileStatus == EProfileStatus.Local)
                    {
                        // Local profile
                        DownloadButton.Visibility = Visibility.Collapsed;
                        ShowInMyProfilesButton.Visibility = Visibility.Collapsed;
                        InfoMessage.Text = Properties.Resources.Info_ProfileIsLocal;
                    }
                    else
                    {
                        // Online profile
                        if (_profileStatus == EProfileStatus.OnlineNotDownloaded)
                        {
                            ShowInMyProfilesButton.Visibility = Visibility.Collapsed;
                            DownloadButton.Visibility = Visibility.Visible;
                            InfoMessage.Text = Properties.Resources.Info_OnlineProfileNotYetDownloaded;
                        }
                        else
                        {
                            DownloadButton.Visibility = Visibility.Collapsed;
                            ShowInMyProfilesButton.Visibility = Visibility.Visible;
                            InfoMessage.Text = Properties.Resources.Info_OnlineProfileAlreadyDownloaded;
                        }
                    }
                }
                else
                {
                    ProfileNameTextBlock.Text = Properties.Resources.String_NoProfileSelected;

                    DownloadButton.Visibility = Visibility.Collapsed;
                    ShowInMyProfilesButton.Visibility = Visibility.Collapsed;
                    InfoMessage.Text = "";

                    ProfileViewer.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_RefreshProfilePreview, ex);
            }
        }

        /// <summary>
        /// Download clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (DownloadClicked != null)
            {
                DownloadClicked(sender, e);
            }
        }

        /// <summary>
        /// Show in My Profiles clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowInMyProfilesButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShowInMyProfileClicked != null)
            {
                ShowInMyProfileClicked(sender, e);
            }
        }

        /// <summary>
        /// Handle an error
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleError(object sender, KxErrorRoutedEventArgs e)
        {
            ReportError(e.Error.Message, e.Error.InnerException);
        }

        /// <summary>
        /// Handle an error from a child control
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        private void ReportError(string message, Exception ex)
        {
            if (_parentWindow is IErrorHandler)
            {
                ((IErrorHandler)_parentWindow).HandleError(message, ex);
            }
        }

        /// <summary>
        /// Clear any error message
        /// </summary>
        private void ClearError()
        {
            if (_parentWindow is IErrorHandler)
            {
                ((IErrorHandler)_parentWindow).ClearError();
            }
        }

    }
}
