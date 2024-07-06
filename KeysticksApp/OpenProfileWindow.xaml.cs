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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.IO;
using System.Xml;
using System.Globalization;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.UI;
using Keysticks.WebService;

namespace Keysticks
{
    /// <summary>
    /// Profile Browser window
    /// </summary>
    public partial class OpenProfileWindow : Window, IWindowParent, IErrorHandler
    {
        // Configuration
        private ITrayManager _parent;
        private AppConfig _appConfig;
        private string _profilesDir;
        private string _fileExtension;

        // Bindings
        private ProfileDisplayList _localProfilesList = new ProfileDisplayList();
        private ProfileDisplayList _onlineProfilesList = new ProfileDisplayList();

        // State
        private bool _delayedShowRequired = false;
        private string _currentFilePath;

        // Static variables (avoid having to log in each time window is opened)
        private static string _username;
        private static string _password;
        private static bool _isLoggedIn;
        private static bool _isAdmin;

        // Misc
        private ProfilePreviewWindow _previewWindow;
        private DispatcherTimer _timer;
        private WebServiceUtils _wsUtils;

        // Commands
        public static RoutedCommand RenameCommand = new RoutedCommand();

        // Properties
        public ProfileDisplayList LocalProfilesList { get { return _localProfilesList; } }
        public ProfileDisplayList OnlineProfilesList { get { return _onlineProfilesList; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="profilesDir"></param>
        /// <param name="fileExtension"></param>
        /// <param name="appConfig"></param>
        public OpenProfileWindow(ITrayManager parent, string profilesDir, string fileExtension)
        {
            _parent = parent;
            _profilesDir = profilesDir;
            _fileExtension = fileExtension;

            InitializeComponent();

            DownloadAreaWelcomeTextBlock.Text =
                string.Format(Properties.Resources.Info_DownloadArea, Environment.NewLine, Constants.AppWebsiteWithoutScheme);

            LoginInfoMessage.Text = string.Format(Properties.Resources.Info_LoginForm, Constants.AppWebsiteWithoutScheme);  

            _wsUtils = new WebServiceUtils();
            _wsUtils.OnResponse += new WebServiceEventHandler(OnWebServiceResponse);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += HandleTimeout;

            // Set bindings
            this.MyProfilesTable.ItemsSource = _localProfilesList;
            this.OnlineProfilesTable.ItemsSource = _onlineProfilesList;
        }

        /// <summary>
        /// Set the config
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            _appConfig = appConfig;
            if (_previewWindow != null)
            {
                _previewWindow.SetAppConfig(_appConfig);
                RefreshProfilePreview();
            }
        }

        /// <summary>
        /// Window loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Initialise username and password if user ticked Remember Me last time
                bool rememberMe = _appConfig.GetBoolVal(Constants.ConfigDownloadAreaRememberMe, Constants.DefaultDownloadAreaRememberMe);
                this.RememberMeCheckBox.IsChecked = rememberMe;
                if (rememberMe)
                {
                    LoadDownloadAreaCredentials();
                }
                if (!string.IsNullOrEmpty(_username))
                {
                    UsernameTextBox.Text = _username;
                }
                if (!string.IsNullOrEmpty(_password))
                {
                    PasswordMaskBox.Password = _password;
                }

                // Populate files list
                RefreshLocalProfilesList();

                // Read cached online profiles list
                InitialiseOnlineProfilesList();

                // Select a profile
                if (_onlineProfilesList.Count != 0)
                {
                    this.OnlineProfilesTable.SelectedIndex = 0;                    
                }

                // Select a profile
                if (_localProfilesList.Count != 0)
                {
                    this.MyProfilesTable.SelectedIndex = 0;

                    // Focus first row
                    MyProfilesTable.Focus();
                    DataGridCellInfo cellInfo = new DataGridCellInfo(this.MyProfilesTable.Items[0], MyProfilesTable.Columns[0]);
                    MyProfilesTable.CurrentCell = cellInfo;
                }
                else
                {
                    InfoMessage.Show(Properties.Resources.Info_NoProfiles);
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_LoadWindow, ex);
            }
        }

        /// <summary>
        /// Get the user's credentials from the app config
        /// </summary>
        private void LoadDownloadAreaCredentials()
        {
            int seedHint = _appConfig.GetIntVal(Constants.ConfigDownloadAreaSeedHint, 0);
            string usernameSec = _appConfig.GetStringVal(Constants.ConfigDownloadAreaUsernameSec, null);
            string passwordSec = _appConfig.GetStringVal(Constants.ConfigDownloadAreaPasswordSec, null);
            if (usernameSec != null && passwordSec != null)
            {
                // Get the seed from the hint
                Sys.IdentityManager idManager = new Sys.IdentityManager();
                string seed = idManager.GetIdentity(seedHint);
                if (!string.IsNullOrEmpty(seed))
                {
                    // Decrypt the stored username and password
                    SymmetricCrypto configCrypto = new SymmetricCrypto(seed);
                    _username = configCrypto.DecryptString(usernameSec);
                    _password = configCrypto.DecryptString(passwordSec);
                }
            }
        }

        /// <summary>
        /// Save the user's credentials to the app config
        /// </summary>
        private void SaveDownloadAreaCredentials()
        {
            string usernameSec = null;
            string passwordSec = null;
            int seedHint = 0;

            if (_username != null && _password != null)
            {
                Sys.IdentityManager idManager = new Sys.IdentityManager();
                string seed = idManager.GetAnIdentity();
                if (!string.IsNullOrEmpty(seed))
                {
                    seedHint = seed.GetHashCode();

                    SymmetricCrypto configCrypto = new SymmetricCrypto(seed);
                    usernameSec = configCrypto.EncryptString(_username);
                    passwordSec = configCrypto.EncryptString(_password);
                }
            }

            _appConfig.SetStringVal(Constants.ConfigDownloadAreaUsernameSec, usernameSec);
            _appConfig.SetStringVal(Constants.ConfigDownloadAreaPasswordSec, passwordSec);
            _appConfig.SetIntVal(Constants.ConfigDownloadAreaSeedHint, seedHint);
        }

        /// <summary>
        /// Read the online profiles from the cache file and check the statuses are correct
        /// </summary>
        /// <remarks>
        /// Requires the local profiles list to have been initialised
        /// </remarks>
        private void InitialiseOnlineProfilesList()
        {
            // Read cached profiles
            string filePath = Path.Combine(AppConfig.LocalAppDataDir, Constants.ProfileListCacheFileName);
            _onlineProfilesList.Clear();
            _onlineProfilesList.FromFile(filePath);

            // Check statuses of downloaded profiles
            foreach (ProfileDisplayItem displayItem in _onlineProfilesList)
            {
                if (displayItem.Status != EProfileStatus.OnlineNotDownloaded &&
                    _localProfilesList.GetItemByName(displayItem.Name) == null)
                {
                    // Local copy has been deleted or renamed, so set the online status back to 'not downloaded'
                    displayItem.Status = EProfileStatus.OnlineNotDownloaded;
                }
            }
        }

        /// <summary>
        /// Handle change of input language
        /// </summary>
        public void KeyboardLayoutChanged(KxKeyboardChangeEventArgs args)
        {
            if (_previewWindow != null)
            {
                _previewWindow.KeyboardLayoutChanged(args);
            }
        }

        /// <summary>
        /// Handle child window closing
        /// </summary>
        /// <param name="window"></param>
        public void ChildWindowClosing(Window window)
        {
            ClearError();
            _previewWindow = null;
        }

        /// <summary>
        /// Refresh the list of local profiles to choose from
        /// </summary>
        private void RefreshLocalProfilesList()
        {
            _localProfilesList.Clear();

            // Get profiles directory
            DirectoryInfo dirInfo = new DirectoryInfo(_profilesDir);
            if (dirInfo.Exists)
            {
                // Find profiles in directory
                FileInfo[] fileList = dirInfo.GetFiles("*" + _fileExtension);

                // Get list of file names
                for (int i = 0; i < fileList.Length; i++)
                {
                    FileInfo fi = fileList[i];
                    string fileName = fi.Name;
                    if (fileName.EndsWith(_fileExtension))
                    {
                        fileName = fileName.Substring(0, fileName.Length - _fileExtension.Length);
                    }
                    ProfileDisplayItem displayItem = new ProfileDisplayItem(fileName, fi.LastWriteTimeUtc);
                    _localProfilesList.Add(displayItem);
                }
            }
            else
            {
                HandleError(string.Format(Properties.Resources.E_DirectoryNotFound, _profilesDir), null);
            }
        }

        /// <summary>
        /// Profile selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyProfilesTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check we're on the right tab
            if (ProfilesTabControl.SelectedIndex == 0)
            {
                // Use a timer to avoid hammering the file system / UI if the user whizzes down the profiles list
                if (!_timer.IsEnabled)
                {
                    ShowLocalProfile();
                    _timer.Start();
                }
                else
                {
                    _delayedShowRequired = true;
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle timeout which signifies that the selected profile should be displayed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HandleTimeout(object sender, EventArgs args)
        {
            if (_delayedShowRequired)
            {
                ShowLocalProfile();
                _delayedShowRequired = false;
            }
            _timer.Stop();
        }

        /// <summary>
        /// Display the selected profile
        /// </summary>
        private void ShowLocalProfile()
        {
            try
            {
                if (ProfilesTabControl.SelectedIndex == 0)
                {
                    ErrorMessage.Clear();
                    _currentFilePath = null;

                    ProfileDisplayItem displayItem = (ProfileDisplayItem)this.MyProfilesTable.SelectedItem;
                    if (displayItem != null)
                    {
                        // Get profile path
                        _currentFilePath = Path.Combine(_profilesDir, displayItem.Name + _fileExtension);

                        // Enable buttons
                        PreviewButton.IsEnabled = true;
                        ActionsButton.IsEnabled = true;
                        LoadButton.IsEnabled = true;
                        ProfileDisplayItem onlineItem = null;
                        if (displayItem.ID != 0)
                        {
                            onlineItem = (ProfileDisplayItem)_onlineProfilesList.GetItemByID(displayItem.ID);
                            if (onlineItem != null && onlineItem.Name != displayItem.Name)
                            {
                                onlineItem = null;
                            }
                        }
                        ShowInOtherTabButton.Visibility = (onlineItem != null) ? Visibility.Visible : Visibility.Hidden;

                        // Display details
                        EAnnotationImage icon = GUIUtils.GetIconForProfileStatus(EProfileStatus.Local);
                        ProfileIcon.Source = GUIUtils.FindIcon(icon);
                        ProfileIcon.ToolTip = GUIUtils.GetCaptionForProfileStatus(EProfileStatus.Local);
                        ProfileIcon.Visibility = Visibility.Visible;
                        ProfileNameTextBlock.Text = displayItem.Name;
                        VersionWarningTextBlock.Text = "";
                    }
                    else
                    {
                        ProfileIcon.Visibility = Visibility.Collapsed;
                        ProfileNameTextBlock.Text = "";
                        VersionWarningTextBlock.Text = "";
                        ShowInOtherTabButton.Visibility = Visibility.Hidden;

                        PreviewButton.IsEnabled = false;
                        ActionsButton.IsEnabled = false;
                        LoadButton.IsEnabled = false;
                    }

                    // Refresh preview window if open
                    RefreshProfilePreview();
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_ShowProfile, ex);
            }
        }

        /// <summary>
        /// Refresh the preview window
        /// </summary>
        private void RefreshProfilePreview()
        {
            if (_previewWindow != null)
            {
                bool shown = false;
                try
                {
                    if (ProfilesTabControl.SelectedIndex == 0)
                    {
                        ProfileDisplayItem displayItem = (ProfileDisplayItem)MyProfilesTable.SelectedItem;
                        if (displayItem != null && _currentFilePath != null)
                        {
                            Profile profile = new Profile();
                            profile.FromFile(_currentFilePath);
                            profile.Name = displayItem.Name;
                            profile.Initialise(_parent.InputContext);
                            profile.SetAppConfig(_appConfig);
                            _previewWindow.SetProfile(profile, EProfileStatus.Local);
                            shown = true;
                        }
                        else
                        {
                            _previewWindow.SetProfile(null, EProfileStatus.None);
                        }
                    }
                    else
                    {
                        ProfileDisplayItem displayItem = (ProfileDisplayItem)OnlineProfilesTable.SelectedItem;
                        if (displayItem != null && DownloadPanel.Visibility == Visibility.Visible)
                        {
                            Profile profile = new Profile(displayItem.MetaData);
                            profile.Name = displayItem.Name;
                            profile.Initialise(_parent.InputContext);
                            profile.SetAppConfig(_appConfig);
                            _previewWindow.SetProfile(profile, displayItem.Status);
                            shown = true;
                        }
                        else
                        {
                            _previewWindow.SetProfile(null, EProfileStatus.None);
                        }
                    }
                }
                catch (Exception ex)
                {
                    HandleError(Properties.Resources.E_UpdateProfilePreview, ex);
                }

                if (!shown)
                {
                    try
                    {
                        _previewWindow.SetProfile(null, EProfileStatus.None);
                    }
                    catch (Exception ex)
                    {
                        HandleError(Properties.Resources.E_ClearProfilePreview, ex);
                    }
                }
            }
        }

        /// <summary>
        /// Load the selected profile and close the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentFilePath != null && 
                    _parent.LoadProfile(_currentFilePath, _currentFilePath.EndsWith(Constants.TemplateFileExtension), true))
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_LoadProfile, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Cancel clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

            e.Handled = true;
        }

        /// <summary>
        /// Handle window closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save the profiles list if it has changed
            if (_onlineProfilesList.Changed)
            {
                try
                {
                    string filePath = Path.Combine(AppConfig.LocalAppDataDir, Constants.ProfileListCacheFileName);
                    _onlineProfilesList.ToFile(filePath);
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            // Close child windows
            if (_previewWindow != null)
            {
                _previewWindow.Close();
            }

            // Tell parent window
            _parent.ChildWindowClosing(this);            
        }

        /// <summary>
        /// Show the preview for the selected profile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InfoMessage.Clear();
                ClearError();

                if (_previewWindow == null)
                {
                    // Open preview window alongside this window
                    _previewWindow = new ProfilePreviewWindow(this);
                    _previewWindow.SetAppConfig(_appConfig);
                    _previewWindow.DownloadClicked += this.DownloadButton_Click;
                    _previewWindow.ShowInMyProfileClicked += this.ShowInOtherTabButton_Click;
                    _previewWindow.Top = this.Top;
                    _previewWindow.Left = Math.Min(this.Left + this.Width + 10, SystemParameters.VirtualScreenWidth - _previewWindow.Width);
                    _previewWindow.Show();

                    RefreshProfilePreview();
                }
                else if (_previewWindow.IsLoaded)
                {
                    _previewWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_OpenProfilePreview, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Open the edit menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActionsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Decide which menu to open
                ContextMenu menu;
                if (ProfilesTabControl.SelectedIndex == 0)
                {
                    menu = this.Resources["LocalProfilesContextMenu"] as ContextMenu;
                    if (menu != null && ShowOrHideLocalContextMenuItems())
                    {
                        menu.PlacementTarget = ActionsButton;
                        menu.IsOpen = true;
                    }
                }
                else
                {
                    menu = this.Resources["OnlineProfilesContextMenu"] as ContextMenu;
                    if (menu != null && ShowOrHideOnlineContextMenuItems())
                    {
                        menu.PlacementTarget = ActionsButton;
                        menu.IsOpen = true;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_OpenActionMenu, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Download the selected profile 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InfoMessage.Clear();
                ClearError();

                // Get the profile to download
                ProfileDisplayItem profileItem = (ProfileDisplayItem)OnlineProfilesTable.SelectedItem;
                if (profileItem != null)
                {
                    // Check whether the user already has this profile
                    // Confirm overwrite if file already exists
                    bool canCreate = true;
                    ProfileDisplayItem localItem = (ProfileDisplayItem)_localProfilesList.GetItemByName(profileItem.Name);
                    if (localItem != null)
                    {
                        CustomMessageBox messageBox = new CustomMessageBox(this, Properties.Resources.Q_DownloadProfileMessage, Properties.Resources.Q_DownloadProfile, MessageBoxButton.OKCancel, true, false);
                        messageBox.ShowDialog();
                        canCreate = messageBox.Result == MessageBoxResult.OK;
                    }

                    if (canCreate)
                    {
                        // Specify which profile to download
                        // Send both the profile ID and name so that the web service can return the name without reading the meta data
                        WebServiceMessageData request = new WebServiceMessageData(EMessageType.GetProfileData);
                        request.SetMetaVal(EMetaDataItem.ID.ToString(), profileItem.ID.ToString());
                        request.SetMetaVal(EMetaDataItem.Name.ToString(), profileItem.Name);

                        // Download profile
                        ProgressIcon.Visibility = Visibility.Visible;
                        _wsUtils.StartWebServiceRequest(request);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_StartProfileDownload, ex);
            }

            e.Handled = true;
        }       

        /// <summary>
        /// Show the selected profile in the other tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowInOtherTabButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InfoMessage.Clear();
                ClearError();

                if (ProfilesTabControl.SelectedIndex == 0)
                {
                    // Find selected local profile
                    ProfileDisplayItem displayItem = (ProfileDisplayItem)MyProfilesTable.SelectedItem;
                    if (displayItem != null)
                    {
                        displayItem = (ProfileDisplayItem)_onlineProfilesList.GetItemByName(displayItem.Name);
                        if (displayItem != null)
                        {
                            OnlineProfilesTable.SelectedItem = displayItem;
                            OnlineProfilesTable.ScrollIntoView(displayItem);

                            // Switch to list view if login panel is showing
                            if (DownloadPanel.Visibility != Visibility.Visible)
                            {
                                ShowDownloadAreaControls(false, false);
                            }
                            this.ProfilesTabControl.SelectedIndex = 1;
                        }
                    }
                }
                else
                {
                    // Find selected online profile
                    ProfileDisplayItem displayItem = (ProfileDisplayItem)OnlineProfilesTable.SelectedItem;
                    if (displayItem != null)
                    {
                        // Select local profile and switch tabs
                        displayItem = (ProfileDisplayItem)_localProfilesList.GetItemByName(displayItem.Name);
                        if (displayItem != null)
                        {
                            MyProfilesTable.SelectedItem = displayItem;
                            MyProfilesTable.ScrollIntoView(displayItem);
                            ProfilesTabControl.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_ShowProfile, ex);
            }
        }

        /// <summary>
        /// Handle opening of context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocalProfilesContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            bool show = ShowOrHideLocalContextMenuItems();
            if (!show)
            {
                // Suppress context menu
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handle opening of context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnlineProfilesContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            bool show = ShowOrHideOnlineContextMenuItems();
            if (!show)
            {
                // Suppress context menu
                e.Handled = true;
            }
        }

        /// <summary>
        /// Show or hide local context menu items
        /// </summary>
        /// <returns>Whether or not to show the menu</returns>
        private bool ShowOrHideLocalContextMenuItems()
        {
            ContextMenu menu = this.Resources["LocalProfilesContextMenu"] as ContextMenu;
            if (menu == null)
            {
                return false;
            }

            bool show = MyProfilesTable.SelectedIndex != -1;

            MenuItem shareMenuItem = (MenuItem)LogicalTreeHelper.FindLogicalNode(menu, "ShareMenuItem");
            shareMenuItem.Visibility = (show && _isLoggedIn) ? Visibility.Visible : Visibility.Collapsed;            

            return show;
        }

        /// <summary>
        /// Show or hide online profiles context menu items
        /// </summary>
        /// <returns>Whether or not to show the menu</returns>
        private bool ShowOrHideOnlineContextMenuItems()
        {
            ContextMenu menu = this.Resources["OnlineProfilesContextMenu"] as ContextMenu;
            if (menu == null)
            {
                return false;
            }

            MenuItem removeMenuItem = (MenuItem)LogicalTreeHelper.FindLogicalNode(menu, "RemoveMenuItem");

            bool show = true;
            ProfileDisplayItem displayItem = (ProfileDisplayItem)OnlineProfilesTable.SelectedItem;
            if (displayItem != null)
            {
                bool canRemove = _isLoggedIn && (_username == displayItem.AddedBy);
                canRemove |= _isAdmin;
                removeMenuItem.Visibility = canRemove ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                show = false;
            }

            return show;
        }

        /// <summary>
        /// Load the selected profile and close the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShareMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InfoMessage.Clear();
                ClearError();

                ProfileDisplayItem displayItem = (ProfileDisplayItem)MyProfilesTable.SelectedItem;
                if (displayItem != null && _currentFilePath != null && _isLoggedIn)
                {
                    // See if profile exists in the Download Area already
                    ProfileDisplayItem existingItem = null;
                    int profileID = displayItem.ID;
                    if (profileID > 0)
                    {
                        existingItem = (ProfileDisplayItem)_onlineProfilesList.GetItemByID(profileID);                        
                    }

                    string message;
                    string caption;
                    if (existingItem != null)
                    {
                        if (existingItem.Name != displayItem.Name)
                        {
                            // User is uploading their own version of a profile
                            message = string.Format(Properties.Resources.Q_ShareProfileAgainMessage, existingItem.ProfileRef);
                            caption = Properties.Resources.Q_ShareProfile;
                            profileID = 0;
                        }
                        else if (_username != existingItem.AddedBy)
                        {
                            // User is uploading their own version of a profile
                            message = string.Format(Properties.Resources.Q_ShareNewVersionMessage, existingItem.ProfileRef);
                            caption = Properties.Resources.Q_ShareNewVersion;
                            profileID = 0;
                        }
                        else
                        {
                            // User is updating their own profile
                            message = string.Format(Properties.Resources.Q_UpdateProfileMessage, existingItem.Name);
                            caption = Properties.Resources.Q_UpdateProfile;
                        }
                    }
                    else
                    {
                        message = string.Format(Properties.Resources.Q_ShareProfileMessage, displayItem.Name);
                        caption = Properties.Resources.Q_ShareProfile;
                    }

                    // Confirm with user
                    CustomMessageBox messageBox = new CustomMessageBox(this, message, caption, MessageBoxButton.OKCancel, true, false);
                    messageBox.ShowDialog();
                    if (messageBox.Result == MessageBoxResult.OK)
                    {
                        string content = File.ReadAllText(_currentFilePath);
                        
                        // Load the profile to check it's valid
                        Profile profile = new Profile();
                        profile.FromString(content);
                        profile.Initialise(_parent.InputContext);
                        profile.SetAppConfig(_appConfig);

                        // Create a web service request
                        WebServiceMessageData request = new WebServiceMessageData(EMessageType.SubmitProfile);
                        request.SetMetaVal(EMetaDataItem.Username.ToString(), _username);
                        request.SetMetaVal(EMetaDataItem.Password.ToString(), _password);
                        request.SetMetaVal(EMetaDataItem.ID.ToString(), profileID.ToString());
                        request.SetMetaVal(EMetaDataItem.ShortName.ToString(), displayItem.ShortName);
                        request.SetMetaVal(EMetaDataItem.Name.ToString(), displayItem.Name);
                        request.SetMetaVal(EMetaDataItem.FileVersion.ToString(), Constants.FileVersionString);
                        request.SetMetaVal(EMetaDataItem.AppVersion.ToString(), Constants.AppVersionString);
                        Dictionary<string, string>.Enumerator eDict = profile.MetaData.GetEnumerator();
                        while (eDict.MoveNext())
                        {
                            request.SetMetaVal(eDict.Current.Key, eDict.Current.Value);
                        }
                        request.Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(content));

                        ProgressIcon.Visibility = Visibility.Visible;
                        _wsUtils.StartWebServiceRequest(request);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_ShareProfile, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Delete profile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteCommand_Executed(object sender, RoutedEventArgs e)
        {
            InfoMessage.Clear();
            ClearError();

            if (ProfilesTabControl.SelectedIndex == 0)
            {
                // My profiles tab
                ProfileDisplayItem displayItem = (ProfileDisplayItem)this.MyProfilesTable.SelectedItem;
                if (displayItem != null)
                {
                    // Confirm
                    string message = string.Format(Properties.Resources.Q_DeleteProfileMessage, displayItem.Name);
                    CustomMessageBox messageBox = new CustomMessageBox(this, message, Properties.Resources.Q_DeleteProfile, MessageBoxButton.OKCancel, true, false);
                    messageBox.ShowDialog();
                    if (messageBox.Result == MessageBoxResult.OK)
                    {
                        try
                        {
                            // Delete local profile
                            int selectedIndex = this.MyProfilesTable.SelectedIndex;
                            string filePath = Path.Combine(_profilesDir, displayItem.Name + _fileExtension);
                            File.Delete(filePath);

                            // Reset download status if profile was previously downloaded
                            int id = displayItem.ID;
                            if (id != 0)
                            {
                                ProfileDisplayItem onlineItem = (ProfileDisplayItem)_onlineProfilesList.GetItemByID(id);
                                if (onlineItem != null && onlineItem.Name == displayItem.Name)
                                {
                                    onlineItem.Status = EProfileStatus.OnlineNotDownloaded;
                                }
                            }

                            // Refresh My Profiles view
                            RefreshLocalProfilesList();
                            if (selectedIndex < this.MyProfilesTable.Items.Count)
                            {
                                this.MyProfilesTable.SelectedIndex = selectedIndex;
                            }
                            else if (selectedIndex > 0)
                            {
                                this.MyProfilesTable.SelectedIndex = selectedIndex - 1;
                            }
                        }
                        catch (Exception ex)
                        {
                            HandleError(Properties.Resources.E_DeleteProfile, ex);
                        }
                    }
                }
            }
            else
            {
                // Handle deleting online profile
                ProfileDisplayItem displayItem = (ProfileDisplayItem)OnlineProfilesTable.SelectedItem;
                if (displayItem != null)
                {
                    bool canRemove = _isLoggedIn && (_username == displayItem.AddedBy);
                    canRemove |= _isAdmin;
                    if (canRemove)
                    {
                        // Confirm
                        string message = string.Format(Properties.Resources.Q_RemoveProfileMessage, displayItem.Name);
                        CustomMessageBox messageBox = new CustomMessageBox(this, message, Properties.Resources.Q_RemoveProfile, MessageBoxButton.OKCancel, true, false);
                        messageBox.ShowDialog();
                        if (messageBox.Result == MessageBoxResult.OK)
                        {
                            try
                            {
                                // Specify which profile to delete
                                WebServiceMessageData request = new WebServiceMessageData(EMessageType.DeleteProfile);
                                request.SetMetaVal(EMetaDataItem.Username.ToString(), _username);
                                request.SetMetaVal(EMetaDataItem.Password.ToString(), _password);
                                request.SetMetaVal(EMetaDataItem.ID.ToString(), displayItem.ID.ToString());
                                request.SetMetaVal(EMetaDataItem.Name.ToString(), displayItem.Name);

                                // Delete profile
                                ProgressIcon.Visibility = Visibility.Visible;
                                _wsUtils.StartWebServiceRequest(request);
                            }
                            catch (Exception ex)
                            {
                                HandleError(Properties.Resources.E_RemoveRemoteProfile, ex);
                            }
                        }
                    }
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Return whether Delete is allowed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ProfilesTabControl.SelectedIndex == 0)
            {
                e.CanExecute = this.MyProfilesTable.SelectedItem != null;
            }
            else
            {
                e.CanExecute = false;
                ProfileDisplayItem displayItem = (ProfileDisplayItem)OnlineProfilesTable.SelectedItem;
                if (displayItem != null)
                {
                    bool canRemove = _isLoggedIn && (_username == displayItem.AddedBy);
                    canRemove |= _isAdmin;
                    if (canRemove)
                    {
                        e.CanExecute = true;
                    }
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Rename profile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RenameCommand_Executed(object sender, RoutedEventArgs e)
        {
            InfoMessage.Clear();
            ClearError();

            ProfileDisplayItem displayItem = (ProfileDisplayItem)this.MyProfilesTable.SelectedItem;
            if (displayItem != null)
            {
                ChooseNameWindow dialog = new ChooseNameWindow(this, "profile", displayItem.Name, _localProfilesList);
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        // Move file
                        FileInfo sourceFile = new FileInfo(Path.Combine(_profilesDir, displayItem.Name + _fileExtension));
                        FileInfo destFile = new FileInfo(Path.Combine(_profilesDir, dialog.SelectedName + _fileExtension));
                        File.Move(sourceFile.FullName, destFile.FullName);

                        // Update parent UI
                        _parent.ProfileRenamed(sourceFile, destFile);

                        // Reset download status if profile was previously downloaded
                        int id = displayItem.ID;
                        if (id != 0)
                        {
                            ProfileDisplayItem onlineItem = (ProfileDisplayItem)_onlineProfilesList.GetItemByID(id);
                            if (onlineItem != null && onlineItem.Name == displayItem.Name)
                            {
                                onlineItem.Status = EProfileStatus.OnlineNotDownloaded;
                            }
                        }

                        // Refresh list
                        RefreshLocalProfilesList();

                        // Select renamed profile
                        displayItem = (ProfileDisplayItem)_localProfilesList.GetItemByName(dialog.SelectedName);
                        if (displayItem != null)
                        {
                            MyProfilesTable.SelectedItem = displayItem;
                            MyProfilesTable.ScrollIntoView(displayItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleError(Properties.Resources.E_RenameProfile, ex);
                    }
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Decide whether rename can execute
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RenameCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (ProfilesTabControl.SelectedIndex == 0 && MyProfilesTable.SelectedItem != null);
            e.Handled = true;
        }

        /// <summary>
        /// Copy profile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            InfoMessage.Clear();
            ClearError();

            ProfileDisplayItem displayItem = (ProfileDisplayItem)this.MyProfilesTable.SelectedItem;
            if (displayItem != null)
            {
                ChooseNameWindow dialog = new ChooseNameWindow(this, "profile", displayItem.Name, _localProfilesList);
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        // Copy the profile
                        string sourceFilePath = Path.Combine(_profilesDir, displayItem.Name + _fileExtension);
                        string destFilePath = Path.Combine(_profilesDir, dialog.SelectedName + _fileExtension);
                        File.Copy(sourceFilePath, destFilePath);

                        // Refresh list
                        RefreshLocalProfilesList();

                        // Try to select the new file
                        NamedItem profileCopy = _localProfilesList.GetItemByName(dialog.SelectedName);
                        if (profileCopy != null)
                        {
                            MyProfilesTable.SelectedItem = profileCopy;
                            MyProfilesTable.ScrollIntoView(profileCopy);
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleError(Properties.Resources.E_CopyProfile, ex);
                    }
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle change of tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProfilesTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InfoMessage.Clear();
            ClearError();

            if (ProfilesTabControl.SelectedIndex == 0)
            {
                // My profiles tab
                ShowMyProfilesControls();
            }
            else
            {
                // Download area tab
                if (_appConfig.GetBoolVal(Constants.ConfigDownloadAreaEnabled, Constants.DefaultDownloadAreaEnabled))
                {
                    // Show correct download area controls
                    ShowDownloadAreaControls(false, LoginPanel.Visibility == Visibility.Visible);

                    // Downloads enabled - initialise list if required                    
                    try
                    {
                        RequestOnlineProfilesUpdate();
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage.Show(Properties.Resources.E_InitRemoteProfilesList, ex);
                    }
                }
                else
                {
                    ShowDownloadAreaControls(true, false);
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Show either the login panel or the downloads panel
        /// </summary>
        /// <param name="showLoginPanel"></param>
        private void ShowDownloadAreaControls(bool showWelcomePanel, bool showLoginPanel)
        {
            // Show or hide controls
            this.LoadButton.IsEnabled = false;
            this.ShowInOtherTabButton.Visibility = Visibility.Hidden;
            this.ShowInOtherTabButton.Content = Properties.Resources.String_ShowInMyProfiles;

            // Show either welcome, login or downloads panel
            if (showWelcomePanel)
            {
                // Show correct panel
                this.WelcomePanel.Visibility = Visibility.Visible;
                this.LoginPanel.Visibility = Visibility.Hidden;
                this.DownloadPanel.Visibility = Visibility.Hidden;

                this.LoginStatusTextBlock.Text = "";
                this.GoToLoginButton.Visibility = Visibility.Collapsed;
                this.LogoutButton.Visibility = Visibility.Collapsed;
                this.BackButton.Visibility = Visibility.Collapsed;
            }
            else if (showLoginPanel)
            {
                // Show correct panel
                this.WelcomePanel.Visibility = Visibility.Hidden;
                this.LoginPanel.Visibility = Visibility.Visible;
                this.DownloadPanel.Visibility = Visibility.Hidden;

                this.LoginStatusTextBlock.Text = Properties.Resources.Info_CreateAccount;
                this.GoToLoginButton.Visibility = Visibility.Collapsed;
                this.LogoutButton.Visibility = Visibility.Collapsed;
                this.BackButton.Visibility = Visibility.Visible;
            }
            else
            {
                // Show correct panel
                this.WelcomePanel.Visibility = Visibility.Hidden;
                this.LoginPanel.Visibility = Visibility.Hidden;
                this.DownloadPanel.Visibility = Visibility.Visible;

                if (_isLoggedIn)
                {
                    this.LoginStatusTextBlock.Text = Properties.Resources.String_Welcome + " " + _username;
                    this.GoToLoginButton.Visibility = Visibility.Collapsed;
                    this.LogoutButton.Visibility = Visibility.Visible;
                }
                else
                {
                    this.LoginStatusTextBlock.Text = Properties.Resources.Info_LogInToShare;
                    this.GoToLoginButton.Visibility = Visibility.Visible;
                    this.LogoutButton.Visibility = Visibility.Collapsed;
                }
                this.BackButton.Visibility = Visibility.Collapsed;
            }

            ShowOnlineProfile();
        }

        /// <summary>
        /// Show the correct My Profiles controls
        /// </summary>
        private void ShowMyProfilesControls()
        {
            // Show / hide buttons
            this.DownloadButton.IsEnabled = false;
            this.ShowInOtherTabButton.Visibility = Visibility.Hidden;
            this.ShowInOtherTabButton.Content = Properties.Resources.String_ShowInDownloadArea;

            ShowLocalProfile();
        }

        /// <summary>
        /// Populate online profiles list
        /// </summary>
        private void RequestOnlineProfilesUpdate()
        {
            DateTime timeNow = DateTime.UtcNow;
            DateTime lastUpdateCheck = _appConfig.GetDateVal(Constants.ConfigProfilesListLastUpdated, DateTime.MinValue);
            double minutesSinceLastCheck = (timeNow - lastUpdateCheck).TotalMinutes;
            if (minutesSinceLastCheck > 5.0)
            {
                // Store date of last update
                _appConfig.SetDateVal(Constants.ConfigProfilesListLastUpdated, timeNow);

                // Download profiles list
                WebServiceMessageData request = new WebServiceMessageData(EMessageType.GetProfilesList);

                // Perform request(s)
                ProgressIcon.Visibility = Visibility.Visible;
                _wsUtils.StartWebServiceRequest(request);
            }
        }

        /// <summary>
        /// Web service response handler
        /// </summary>
        /// <param name="response"></param>
        private void OnWebServiceResponse(WebServiceMessageData response)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(new WebServiceEventHandler(HandleWebServiceResponse), response);
            }
            else
            {
                HandleWebServiceResponse(response);
            }
        }

        /// <summary>
        /// Handle web service response
        /// </summary>
        /// <param name="response"></param>
        private void HandleWebServiceResponse(WebServiceMessageData response)
        {
            ProgressIcon.Visibility = Visibility.Collapsed;
            
            if (response != null)
            {
                switch (response.MessageType)
                {
                    case EMessageType.ProfilesList:
                        HandleProfileListResponse(response);
                        break;
                    case EMessageType.ProfileData:
                        HandleProfileDataResponse(response);
                        break;
                    case EMessageType.ProfileSubmitted:
                        HandleProfileSubmitted(response);
                        break;
                    case EMessageType.ProfileDeleted:
                        HandleProfileDeleted(response);
                        break;
                    case EMessageType.LoginSuccess:
                    case EMessageType.LoginError:
                        HandleLoginResponse(response);
                        break;
                    case EMessageType.WebServiceError:
                        HandleError(response.Content, null);
                        break;
                }
            }
        }


        /// <summary>
        /// Receive profile list from web service
        /// </summary>
        /// <param name="response"></param>
        private void HandleProfileListResponse(WebServiceMessageData response)
        {
            try
            {
                // Parse XML data
                XmlDocument doc = new XmlDocument();
                string xml = Encoding.UTF8.GetString(Convert.FromBase64String(response.Content));
                doc.LoadXml(xml);
                XmlNodeList rows = doc.SelectNodes("/profiles/profile");
                Dictionary<int, bool> profileIDs = new Dictionary<int, bool>();
                foreach (XmlElement row in rows) 
                {
                    // Parse profile meta data
                    MetaDataTable profileItem = new MetaDataTable();
                    foreach (XmlAttribute attr in row.Attributes)
                    {
                        profileItem.SetStringVal(attr.Name, attr.Value);
                    }

                    ProfileDisplayItem displayItem = new ProfileDisplayItem(profileItem);
                    profileIDs[displayItem.ID] = true;

                    // See if the user already has a profile with the same name
                    bool hasLocalProfile = (_localProfilesList.GetItemByName(displayItem.Name) != null);

                    ProfileDisplayItem existingItem = (ProfileDisplayItem)_onlineProfilesList.GetItemByID(displayItem.ID);
                    if (existingItem != null)
                    {
                        // If the user has this profile, change the status
                        if (hasLocalProfile && displayItem.LastModifiedDate > existingItem.LastDownloadModifiedDate)
                        {
                            existingItem.Status = EProfileStatus.OnlinePreviouslyDownloaded;
                        }
                        existingItem.UpdateMetaData(profileItem);
                    }
                    else
                    {
                        displayItem.Status = hasLocalProfile ? EProfileStatus.OnlineDownloaded : EProfileStatus.OnlineNotDownloaded;
                        _onlineProfilesList.OrderedInsert(displayItem);
                    }
                    _onlineProfilesList.Changed = true;                    
                }

                // Remove any online profiles that have been deleted
                int i = 0;
                while (i < _onlineProfilesList.Count)
                {
                    NamedItem item = _onlineProfilesList[i];
                    if (!profileIDs.ContainsKey(item.ID))
                    {
                        // Remove item
                        _onlineProfilesList.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }

                if (OnlineProfilesTable.SelectedIndex == -1)
                {
                    // Select first item if none is selected
                    if (_onlineProfilesList.Count != 0)
                    {
                        OnlineProfilesTable.SelectedIndex = 0;
                    }
                }
                else
                {
                    // Refresh the selected item
                    ShowOnlineProfile();
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_ReceiveRemoteProfilesList, ex);
            }
        }

        /// <summary>
        /// Receive profile data from web service
        /// </summary>
        /// <param name="response"></param>
        private void HandleProfileDataResponse(WebServiceMessageData response)
        {
            try
            {
                // Get profile data
                int id = int.Parse(response.GetMetaVal(EMetaDataItem.ID.ToString(), "0"));
                string name = response.GetMetaVal(EMetaDataItem.Name.ToString());
                string content = Encoding.UTF8.GetString(Convert.FromBase64String(response.Content));

                // Create profile
                Profile profile = new Profile();
                profile.FromString(content);
                profile.Name = name;
                profile.Initialise(_parent.InputContext);
                profile.SetAppConfig(_appConfig);

                // Save profile
                string filePath = Path.Combine(_profilesDir, name + _fileExtension);
                profile.ToFile(filePath);                    

                // Update My Profiles
                ProfileDisplayItem displayItem = (ProfileDisplayItem)_localProfilesList.GetItemByName(name);
                if (displayItem == null)
                {
                    // Refresh list if it's a new download
                    RefreshLocalProfilesList();

                    displayItem = (ProfileDisplayItem)_localProfilesList.GetItemByName(name);
                }

                // Select downloaded profile
                if (displayItem != null)
                {
                    MyProfilesTable.SelectedItem = displayItem;
                    MyProfilesTable.ScrollIntoView(displayItem);
                }

                // Update the status and download time of the online profile item
                displayItem = (ProfileDisplayItem)_onlineProfilesList.GetItemByID(id);
                if (displayItem != null)
                {
                    displayItem.Status = EProfileStatus.OnlineDownloaded;
                    displayItem.LastDownloadModifiedDate = displayItem.LastModifiedDate;
                        
                    // Increment Downloads stat
                    displayItem.Downloads++;
                    _onlineProfilesList.Changed = true;
                        
                    // Redisplay
                    ShowOnlineProfile();
                }

                // Show message
                InfoMessage.Show(Properties.Resources.Info_ProfileDownloadSuccess);                
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_OnProfileDownload, ex);
            }
        }

        /// <summary>
        /// Handle profile submitted response
        /// </summary>
        /// <param name="response"></param>
        private void HandleProfileSubmitted(WebServiceMessageData response)
        {
            try
            {
                // Get profile meta data from response
                MetaDataTable profileItem = new MetaDataTable();
                Dictionary<string, string>.Enumerator eDict = response.Metadata.GetEnumerator();
                while (eDict.MoveNext())
                {
                    profileItem.SetStringVal(eDict.Current.Key, eDict.Current.Value);
                }

                // Get the name of the profile submitted, then remove from meta data so it won't be stored
                string originalName = response.GetMetaVal(EMetaDataItem.Name.ToString());
                profileItem.SetStringVal(EMetaDataItem.Name.ToString(), null);

                // Create a new display item
                ProfileDisplayItem displayItem = new ProfileDisplayItem(profileItem);
                displayItem.Status = EProfileStatus.OnlineDownloaded;
                displayItem.LastDownloadModifiedDate = displayItem.LastModifiedDate;

                // Update online profiles list
                ProfileDisplayItem existingItem = (ProfileDisplayItem)_onlineProfilesList.GetItemByName(displayItem.Name);
                if (existingItem != null)
                {
                    existingItem.Status = EProfileStatus.OnlineDownloaded;
                    existingItem.LastDownloadModifiedDate = displayItem.LastModifiedDate;
                    existingItem.UpdateMetaData(profileItem);
                }
                else
                {
                    _onlineProfilesList.OrderedInsert(displayItem);
                }
                _onlineProfilesList.Changed = true;

                // Select the online profile
                OnlineProfilesTable.SelectedValue = displayItem.ID;

                // Rename the file if it's different
                if (originalName != displayItem.Name)
                {
                    FileInfo sourceFile = new FileInfo(Path.Combine(_profilesDir, originalName + _fileExtension));
                    FileInfo destFile = new FileInfo(Path.Combine(_profilesDir, displayItem.Name + _fileExtension));
                    File.Move(sourceFile.FullName, destFile.FullName);

                    // Update parent UI
                    _parent.ProfileRenamed(sourceFile, destFile);

                    // Update local list
                    RefreshLocalProfilesList();

                    // Select local file if possible
                    displayItem = (ProfileDisplayItem)_localProfilesList.GetItemByName(displayItem.Name);
                    if (displayItem != null)
                    {
                        MyProfilesTable.SelectedItem = displayItem;
                        MyProfilesTable.ScrollIntoView(displayItem);
                    }

                    // Show message
                    InfoMessage.Show(string.Format(Properties.Resources.Info_ProfileShareAndRenameSuccess, displayItem.ProfileRef));
                }
                else
                {
                    // Refresh the current item
                    ShowLocalProfile();

                    // Show message
                    InfoMessage.Show(Properties.Resources.Info_ProfileShareSuccess);                
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_OnSubmitProfile, ex);
            }
        }

        /// <summary>
        /// Handle profile deleted response
        /// </summary>
        /// <param name="response"></param>
        private void HandleProfileDeleted(WebServiceMessageData response)
        {
            try
            {
                string name = response.GetMetaVal(EMetaDataItem.Name.ToString());
                int selectedIndex = OnlineProfilesTable.SelectedIndex;
                ProfileDisplayItem existingItem = (ProfileDisplayItem)_onlineProfilesList.GetItemByName(name);
                if (existingItem != null)
                {
                    // Remove from list
                    _onlineProfilesList.Remove(existingItem);
                    _onlineProfilesList.Changed = true;

                    // If selected item was deleted, try to select same position
                    if (OnlineProfilesTable.SelectedIndex == -1 && _onlineProfilesList.Count != 0)
                    {
                        OnlineProfilesTable.SelectedIndex = Math.Min(_onlineProfilesList.Count - 1, selectedIndex);
                    }
                }                
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_DeleteProfile, ex);
            }
        }

        /// <summary>
        /// Handle login result
        /// </summary>
        /// <param name="response"></param>
        private void HandleLoginResponse(WebServiceMessageData response)
        {
            try
            {
                _isLoggedIn = false;
                _isAdmin = false;
                if (response.MessageType == EMessageType.LoginSuccess)                
                {
                    // Store username to display (as user may have logged in using email address)
                    _username = response.GetMetaVal(EMetaDataItem.Username.ToString());
                    _isAdmin = response.GetMetaVal(EMetaDataItem.IsAdmin.ToString()) == "1";
                    _isLoggedIn = true;

                    // Remember username and password if remember me was ticked
                    if (_appConfig.GetBoolVal(Constants.ConfigDownloadAreaRememberMe, Constants.DefaultDownloadAreaRememberMe))
                    {
                        SaveDownloadAreaCredentials();
                    }

                    // Show online profiles list
                    ShowDownloadAreaControls(false, false);
                }
                else
                {
                    _username = null;
                    _password = null;
                    SaveDownloadAreaCredentials();

                    // Refresh the controls, but stay on the same view
                    ShowDownloadAreaControls(false, this.LoginPanel.Visibility == Visibility.Visible);

                    HandleError(Properties.Resources.E_BadCredentials, null);
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_LoginResponse, ex);
            }
        }

        /// <summary>
        /// Online profile selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnlineProfilesTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check we're on the right tab
            if (ProfilesTabControl.SelectedIndex == 1)
            {
                ShowOnlineProfile();
            }

            e.Handled = true;
        }

        /// <summary>
        /// Display the selected online profile
        /// </summary>
        private void ShowOnlineProfile()
        {
            try
            {
                if (ProfilesTabControl.SelectedIndex == 1)
                {
                    ErrorMessage.Clear();

                    ProfileDisplayItem profileItem = (ProfileDisplayItem)this.OnlineProfilesTable.SelectedItem;
                    if (profileItem != null &&
                        DownloadPanel.Visibility == Visibility.Visible)
                    {
                        // Display details
                        EAnnotationImage icon = GUIUtils.GetIconForProfileStatus(profileItem.Status);
                        ProfileIcon.Source = GUIUtils.FindIcon(icon);
                        ProfileIcon.ToolTip = GUIUtils.GetCaptionForProfileStatus(profileItem.Status);
                        ProfileIcon.Visibility = Visibility.Visible;
                        ProfileNameTextBlock.Text = profileItem.Name;

                        // Display version warning if necessary                        
                        string reqdVersionStr = profileItem.MetaData.GetStringVal(EMetaDataItem.FileVersion.ToString(), Constants.FileVersionString);
                        float reqdVersion;
                        float fileVersion;
                        if (float.TryParse(reqdVersionStr, NumberStyles.Number, CultureInfo.InvariantCulture, out reqdVersion) &&
                            float.TryParse(Constants.FileVersionString, NumberStyles.Number, CultureInfo.InvariantCulture, out fileVersion) &&
                            reqdVersion > fileVersion)
                        {
                            VersionWarningTextBlock.Text = string.Format("({0} v{1} {2})", Constants.ProductName, reqdVersionStr, Properties.Resources.String_or_later);
                        }
                        else
                        {
                            VersionWarningTextBlock.Text = "";
                        }

                        // Enable / disable buttons
                        PreviewButton.IsEnabled = true;
                        ActionsButton.IsEnabled = true;
                        DownloadButton.IsEnabled = true;
                        ShowInOtherTabButton.Visibility = profileItem.Status == EProfileStatus.OnlineNotDownloaded ? Visibility.Hidden : Visibility.Visible;
                    }
                    else
                    {
                        // Disable buttons
                        PreviewButton.IsEnabled = false;
                        ActionsButton.IsEnabled = false;
                        DownloadButton.IsEnabled = false;
                        ShowInOtherTabButton.Visibility = Visibility.Hidden;

                        // Clear details
                        ProfileIcon.Visibility = Visibility.Collapsed;
                        ProfileNameTextBlock.Text = "";
                        VersionWarningTextBlock.Text = "";
                    }

                    RefreshProfilePreview();
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_ShowProfile, ex);
            }
        }

        /// <summary>
        /// Handle an error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void HandleError(string message, Exception ex)
        {
            ProgressIcon.Visibility = Visibility.Collapsed;
            InfoMessage.Clear();
            ErrorMessage.Show(message, ex);
        }

        /// <summary>
        /// Clear any error message
        /// </summary>
        public void ClearError()
        {
            ErrorMessage.Clear();
        }

        /// <summary>
        /// Enter Download Area
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnterDownloadAreaButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InfoMessage.Clear();
                ClearError();

                // Store that download area is enabled
                _appConfig.SetBoolVal(Constants.ConfigDownloadAreaEnabled, true);

                // Show downloads panel
                ShowDownloadAreaControls(false, false);

                // Populate the profiles list
                RequestOnlineProfilesUpdate();
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_OnEnterDownloadArea, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Check the user's login credentials
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InfoMessage.Clear();
                ClearError();

                if (UsernameTextBox.Text != "" && PasswordMaskBox.Password != "")
                {
                    _appConfig.SetBoolVal(Constants.ConfigDownloadAreaRememberMe, this.RememberMeCheckBox.IsChecked == true);

                    _username = this.UsernameTextBox.Text;
                    _password = PasswordMaskBox.Password;

                    WebServiceMessageData request = new WebServiceMessageData(EMessageType.CheckForumUser);
                    request.SetMetaVal(EMetaDataItem.Username.ToString(), _username);
                    request.SetMetaVal(EMetaDataItem.Password.ToString(), _password);
                    ProgressIcon.Visibility = Visibility.Visible;
                    _wsUtils.StartWebServiceRequest(request);
                }
                else
                {
                    HandleError(Properties.Resources.E_EnterCredentials, null);
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_ValidateCredentials, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Cancel login form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InfoMessage.Clear();
                ClearError();

                ShowDownloadAreaControls(false, false);
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_ShowOnlineProfiles, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Go to new user webpage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewUserButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InfoMessage.Clear();
                ClearError();

                ProcessManager.Start(Constants.NewUserURL);
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_OpenRegistrationPage, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Go to login form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InfoMessage.Clear();
                ClearError();

                ShowDownloadAreaControls(false, true);
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_ShowLoginForm, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Log out of Download Area
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InfoMessage.Clear();
                ClearError();

                _isLoggedIn = false;
                _username = null;
                _password = null;
                SaveDownloadAreaCredentials();

                // Redisplay current view
                ShowDownloadAreaControls(false, this.LoginPanel.Visibility == Visibility.Visible);
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_OnLogOut, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Open the help file
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
