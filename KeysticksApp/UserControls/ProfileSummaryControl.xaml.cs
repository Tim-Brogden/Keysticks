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
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Keysticks.Config;
using Keysticks.Core;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Show summary meta data for the profile
    /// </summary>
    public partial class ProfileSummaryControl : KxUserControl
    {
        // Fields
        private bool _isLoaded = false;
        private Profile _profile;

        // Dependency properties
        public bool IsDesignMode
        {
            get { return (bool)GetValue(IsDesignModeProperty); }
            set { SetValue(IsDesignModeProperty, value); }
        }
        private static readonly DependencyProperty IsDesignModeProperty =
            DependencyProperty.Register(
            "IsDesignMode",
            typeof(bool),
            typeof(ProfileSummaryControl),
            new FrameworkPropertyMetadata(false)
        );
        
        // Routed events
        public static readonly RoutedEvent KxMetaDataEditedEvent = EventManager.RegisterRoutedEvent(
            "MetaDataEdited", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ProfileSummaryControl));
        public event RoutedEventHandler MetaDataEdited
        {
            add { AddHandler(KxMetaDataEditedEvent, value); }
            remove { RemoveHandler(KxMetaDataEditedEvent, value); }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public ProfileSummaryControl()
            : base()
        {
            InitializeComponent();
        }
       
        /// <summary>
        /// Set app config
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            // Do nothing
        }
     
        /// <summary>
        /// Set the source
        /// </summary>
        /// <param name="profile"></param>
        public void SetProfile(Profile profile)
        {
            _profile = profile;            

            RefreshDisplay();            
        }

        /// <summary>
        /// Refresh the annotations
        /// </summary>
        public void RefreshDisplay()
        {
            if (_isLoaded && _profile != null)
            {
                // User-defined data
                string appName = _profile.MetaData.GetStringVal(EMetaDataItem.TargetApp.ToString(), "");
                string appURL = _profile.MetaData.GetStringVal(EMetaDataItem.TargetAppURL.ToString(), "");
                if (IsDesignMode)
                {
                    TargetAppNameTextBox.Text = appName;
                    TargetAppURLTextBox.Text = appURL;
                }
                else
                {
                    TargetAppNameTextBlock.Text = appName;
                    TargetAppURLTextBlock.Text = appURL;

                    // Set the hyperlink
                    Uri uri = StringToUri(appURL);
                    if (uri != null)
                    {
                        TargetAppURLHyperlink.NavigateUri = uri;
                        TargetAppURLHyperlink.ToolTip = string.Format(Properties.Resources.String_GoToX, uri.AbsoluteUri);
                    }
                    else
                    {
                        TargetAppURLHyperlink.NavigateUri = null;
                        TargetAppURLHyperlink.ToolTip = Properties.Resources.String_InvalidLink;
                    }
                }
                ProfileNotesTextBox.Text = _profile.MetaData.GetStringVal(EMetaDataItem.Notes.ToString(), "");

                // Calculated data
                int numPlayers = _profile.MetaData.GetIntVal(EMetaDataItem.NumPlayers.ToString(), 1);
                string keyboardsSummary = _profile.MetaData.GetStringVal(EMetaDataItem.KeyboardTypes.ToString(), Properties.Resources.String_None);
                string programActionsSummary = _profile.MetaData.GetStringVal(EMetaDataItem.ProgramActions.ToString(), Properties.Resources.String_None);
                string activationsSummary = _profile.MetaData.GetStringVal(EMetaDataItem.AutoActivations.ToString(), Properties.Resources.String_None);
                NumPlayersTextBlock.Text = numPlayers.ToString(System.Globalization.CultureInfo.InvariantCulture);
                KeyboardTypesTextBlock.Text = keyboardsSummary;
                KeyboardTypesTextBlock.ToolTip = keyboardsSummary;
                ProgramActionsTextBlock.Text = programActionsSummary;
                ProgramActionsTextBlock.ToolTip = programActionsSummary;
                AutoActivationTextBlock.Text = activationsSummary;
                AutoActivationTextBlock.ToolTip = activationsSummary;
            }
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;
                
                // Allow editing if design mode
                if (IsDesignMode)
                {
                    TargetAppNameTextBlock.Visibility = Visibility.Collapsed;
                    TargetAppURLContainer.Visibility = Visibility.Collapsed;
                    TargetAppNameTextBox.Visibility = Visibility.Visible;
                    TargetAppURLTextBox.Visibility = Visibility.Visible;
                    ProfileNotesTextBox.IsReadOnly = false;
                    TargetAppNameTextBox.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(TargetAppNameTextBox_LostKeyboardFocus);
                    TargetAppURLTextBox.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(TargetAppURLTextBox_LostKeyboardFocus);
                    ProfileNotesTextBox.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(ProfileNotesTextBox_LostKeyboardFocus);
                }
                else
                {
                    TargetAppNameTextBox.Visibility = Visibility.Collapsed;
                    TargetAppURLTextBox.Visibility = Visibility.Collapsed;
                    TargetAppNameTextBlock.Visibility = Visibility.Visible;
                    TargetAppURLContainer.Visibility = Visibility.Visible;
                    ProfileNotesTextBox.IsReadOnly = true;
                }
                RefreshDisplay();
            }
        }

         /// <summary>
        /// Target app name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TargetAppNameTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string appName = TargetAppNameTextBox.Text;
            if (appName != _profile.MetaData.GetStringVal(EMetaDataItem.TargetApp.ToString(), ""))
            {
                _profile.MetaData.SetStringVal(EMetaDataItem.TargetApp.ToString(), appName);
                _profile.IsModified = true;
                RaiseEvent(new RoutedEventArgs(KxMetaDataEditedEvent));
            }
            e.Handled = true;
        }

        /// <summary>
        /// Target app URL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TargetAppURLTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string appURL = TargetAppURLTextBox.Text;
            if (appURL != _profile.MetaData.GetStringVal(EMetaDataItem.TargetAppURL.ToString(), ""))
            {
                _profile.MetaData.SetStringVal(EMetaDataItem.TargetAppURL.ToString(), appURL);
                _profile.IsModified = true;
                RaiseEvent(new RoutedEventArgs(KxMetaDataEditedEvent));
            }
            e.Handled = true;
        }


        /// <summary>
        /// Profile notes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ProfileNotesTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string notes = ProfileNotesTextBox.Text;
            if (notes != _profile.MetaData.GetStringVal(EMetaDataItem.Notes.ToString(), ""))
            {
                _profile.MetaData.SetStringVal(EMetaDataItem.Notes.ToString(), notes);
                _profile.IsModified = true;
                RaiseEvent(new RoutedEventArgs(KxMetaDataEditedEvent));
            }
            e.Handled = true;
        }

        /// <summary>
        /// Hyperlink clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            if (e.Uri != null)
            {
                OpenURI(e.Uri.AbsoluteUri);
            }
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
        /// Validate http address and create uri
        /// </summary>
        /// <param name="appURL"></param>
        /// <returns></returns>
        private Uri StringToUri(string url)
        {
            Uri uri = null;
            if (!string.IsNullOrWhiteSpace(url))
            {
                if (!url.StartsWith("http"))
                {
                    url = "http://" + url;
                }

                Regex regex = new Regex(Constants.ValidHttpURIRegex);
                if (regex.IsMatch(url))
                {
                    Uri.TryCreate(url, UriKind.Absolute, out uri);
                }
            }

            return uri;
        }
    }
}
