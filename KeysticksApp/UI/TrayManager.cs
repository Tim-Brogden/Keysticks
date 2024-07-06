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
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using Keysticks.Core;
using Keysticks.Config;
using Keysticks.Event;
using Keysticks.Sys;

namespace Keysticks.UI
{
    /// <summary>
    /// Manages the system tray icon and the application's main menu
    /// </summary>
    public class TrayManager : IDisposable, ITrayManager
    {
        // Fields
        private AppConfig _appConfig;
        private IMainWindow _parentWindow;
        private System.ComponentModel.IContainer _components;
        private NotifyIcon _notifyIcon;
        private bool _profileEdited = false;
        private ProfileDesignerWindow _designerDialog;
        private AppConfigWindow _appConfigDialog;
        private OpenProfileWindow _profileBrowserDialog;
        private LogWindow _logDialog;
        private IKeyboardContext _inputContext;
        private ContextMenuStrip _menuStrip;
        private ToolStripMenuItem _chooseProfileMenuItem;
        private ToolStripMenuItem _chooseTemplateMenuItem;
        private ToolStripMenuItem _openProfilesFolderMenuItem;
        private ToolStripMenuItem _saveProfileMenuItem;
        private ToolStripMenuItem _saveAsMenuItem;
        private ToolStripMenuItem _newProfileMenuItem;
        private ToolStripMenuItem _newTemplateMenuItem;
        private ToolStripMenuItem _editProfileMenuItem;
        private ToolStripMenuItem _viewControlsMenuItem;
        private ToolStripMenuItem _viewHelpFileMenuItem;
        private ToolStripMenuItem _vieLogMenuItem;
        private ToolStripMenuItem _programOptionsMenuItem;
        //private ToolStripMenuItem _programUpdatesMenuItem;
        private ToolStripMenuItem _helpAboutMenuItem;
        private ToolStripSeparator _recentFilesSeparator;
        private ToolStripMenuItem _recentFilesMenuItem;
        private ToolStripMenuItem _exitProgramMenuItem;

        // Properties
        public bool ProfileEdited { get { return _profileEdited; } set { _profileEdited = value; } }
        public IKeyboardContext InputContext { get { return _inputContext; } }
        public IThreadManager ThreadManager { get { return _parentWindow.ThreadManager; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent"></param>
        public TrayManager(IMainWindow parent, AppConfig appConfig, KxKeyboardChangeEventArgs inputContext)
        {
            _parentWindow = parent;
            _appConfig = appConfig;
            _inputContext = inputContext;

            // Create the system tray icon
            _components = new System.ComponentModel.Container();
            _notifyIcon = new NotifyIcon(_components);
            string iconPath = Path.Combine(AppConfig.AppProjectDir, Constants.KLogoFileName);
            _notifyIcon.Icon = new System.Drawing.Icon(iconPath);
            SetNotifyIconText(Constants.ProductName);   // Default tool tip text
            _notifyIcon.Visible = true;

            // Handle the click event
            _notifyIcon.MouseUp += NotifyIcon_MouseUp;

            // Create the context menu
            CreateContextMenu();

            // Show or hide "Install updates" menu item
            //InitialiseProgramUpdateOption();
        }

        /// <summary>
        /// Create the context menu
        /// </summary>
        private void CreateContextMenu()
        {
            _menuStrip = new ContextMenuStrip();
            _menuStrip.Name = "KxContextMenu";

            // Choose a profile
            _chooseProfileMenuItem = new ToolStripMenuItem();
            _chooseProfileMenuItem.Text = Properties.Resources.Menu_ChooseAProfile + "...";
            _chooseProfileMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            _chooseProfileMenuItem.ToolTipText = Properties.Resources.Menu_ChooseAProfileToolTip;
            _chooseProfileMenuItem.Image = GUIUtils.ConvertImage(GUIUtils.FindIcon(EAnnotationImage.OpenFile));
            _chooseProfileMenuItem.Click += FileOpen_Click;
            _menuStrip.Items.Add(_chooseProfileMenuItem);

            // Choose a template
            _chooseTemplateMenuItem = new ToolStripMenuItem();
            _chooseTemplateMenuItem.Text = Properties.Resources.Menu_ChooseATemplate + "...";
            _chooseTemplateMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            _chooseTemplateMenuItem.ToolTipText = Properties.Resources.Menu_ChooseATemplateToolTip;
            _chooseTemplateMenuItem.Click += FileOpenTemplate_Click;
            _menuStrip.Items.Add(_chooseTemplateMenuItem);

            // Open profiles folder
            _openProfilesFolderMenuItem = new ToolStripMenuItem();
            _openProfilesFolderMenuItem.Text = Properties.Resources.Menu_OpenProfilesFolder + "...";
            _openProfilesFolderMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            _openProfilesFolderMenuItem.ToolTipText = Properties.Resources.Menu_OpenProfilesFolderToolTip;
            _openProfilesFolderMenuItem.Image = GUIUtils.ConvertImage(GUIUtils.FindIcon(EAnnotationImage.OpenFolder));
            _openProfilesFolderMenuItem.Click += ProfilesFolder_Click;
            _menuStrip.Items.Add(_openProfilesFolderMenuItem);

            // Save profile
            _saveProfileMenuItem = new ToolStripMenuItem();
            _saveProfileMenuItem.Text = Properties.Resources.Menu_SaveThisProfile;
            _saveProfileMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            _saveProfileMenuItem.ToolTipText = Properties.Resources.Menu_SaveThisProfileToolTip;
            _saveProfileMenuItem.Image = GUIUtils.ConvertImage(GUIUtils.FindIcon(EAnnotationImage.SaveFile));
            _saveProfileMenuItem.Click += FileSave_Click;
            _menuStrip.Items.Add(_saveProfileMenuItem);

            // Save as
            _saveAsMenuItem = new ToolStripMenuItem();
            _saveAsMenuItem.Text = Properties.Resources.Menu_SaveAs + "...";
            _saveAsMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            _saveAsMenuItem.ToolTipText = Properties.Resources.Menu_SaveAsToolTip;
            _saveAsMenuItem.Click += FileSaveAs_Click;
            _menuStrip.Items.Add(_saveAsMenuItem);

            // Separator
            ToolStripSeparator separator = new ToolStripSeparator();
            _menuStrip.Items.Add(separator);

            // New profile
            _newProfileMenuItem = new ToolStripMenuItem();
            _newProfileMenuItem.Text = Properties.Resources.Menu_CreateNewProfile + "...";
            _newProfileMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            _newProfileMenuItem.ToolTipText = Properties.Resources.Menu_CreateNewProfileToolTip;
            _newProfileMenuItem.Image = GUIUtils.ConvertImage(GUIUtils.FindIcon(EAnnotationImage.NewProfile));
            _newProfileMenuItem.Click += FileNew_Click;
            _menuStrip.Items.Add(_newProfileMenuItem);

            // New template
            _newTemplateMenuItem = new ToolStripMenuItem();
            _newTemplateMenuItem.Text = Properties.Resources.Menu_CreateNewTemplate + "...";
            _newTemplateMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            _newTemplateMenuItem.ToolTipText = Properties.Resources.Menu_CreateNewTemplateToolTip;
            _newTemplateMenuItem.Click += FileNewTemplate_Click;
            _menuStrip.Items.Add(_newTemplateMenuItem);

            // Edit profile
            _editProfileMenuItem = new ToolStripMenuItem();
            _editProfileMenuItem.Text = Properties.Resources.Menu_EditProfile + "...";
            _editProfileMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            _editProfileMenuItem.ToolTipText = Properties.Resources.Menu_EditProfileToolTip;
            _editProfileMenuItem.Image = GUIUtils.ConvertImage(GUIUtils.FindIcon(EAnnotationImage.EditFile));
            _editProfileMenuItem.Click += EditProfile_Click;
            _menuStrip.Items.Add(_editProfileMenuItem);

            // Separator
            separator = new ToolStripSeparator();
            _menuStrip.Items.Add(separator);

            // View controls
            _viewControlsMenuItem = new ToolStripMenuItem();
            _viewControlsMenuItem.Text = Properties.Resources.Menu_ViewControls + "...";
            _viewControlsMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            _viewControlsMenuItem.ToolTipText = Properties.Resources.Menu_ViewControlsToolTip;
            _viewControlsMenuItem.Image = GUIUtils.ConvertImage(GUIUtils.FindIcon(EAnnotationImage.Controller));
            _viewControlsMenuItem.Click += ViewControls_Click;
            _menuStrip.Items.Add(_viewControlsMenuItem);

            // View help file
            _viewHelpFileMenuItem = new ToolStripMenuItem();
            _viewHelpFileMenuItem.Text = Properties.Resources.Menu_ViewHelp + "...";
            _viewHelpFileMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            _viewHelpFileMenuItem.ToolTipText = Properties.Resources.Menu_ViewHelpToolTip;
            _viewHelpFileMenuItem.Image = GUIUtils.ConvertImage(GUIUtils.FindIcon(EAnnotationImage.Help));
            _viewHelpFileMenuItem.Click += ViewHelp_Click;
            _menuStrip.Items.Add(_viewHelpFileMenuItem);

            // View message log
            _vieLogMenuItem = new ToolStripMenuItem();
            _vieLogMenuItem.Text = Properties.Resources.Menu_MessageLog + "...";
            _vieLogMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            _vieLogMenuItem.ToolTipText = Properties.Resources.Menu_MessageLogToolTip;
            _vieLogMenuItem.Image = GUIUtils.ConvertImage(GUIUtils.FindIcon(EAnnotationImage.ViewText));
            _vieLogMenuItem.Click += ViewLogFile_Click;
            _menuStrip.Items.Add(_vieLogMenuItem);

            // Separator
            separator = new ToolStripSeparator();
            _menuStrip.Items.Add(separator);

            // Program options
            _programOptionsMenuItem = new ToolStripMenuItem();
            _programOptionsMenuItem.Text = Properties.Resources.Menu_ProgramOptions + "...";
            _programOptionsMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            _programOptionsMenuItem.ToolTipText = Properties.Resources.Menu_ProgramOptions;
            _programOptionsMenuItem.Image = GUIUtils.ConvertImage(GUIUtils.FindIcon(EAnnotationImage.Settings));
            _programOptionsMenuItem.Click += ToolsOptions_Click;
            _menuStrip.Items.Add(_programOptionsMenuItem);

            // Program updates
            //_programUpdatesMenuItem = new ToolStripMenuItem();
            //_programUpdatesMenuItem.Text = Properties.Resources.Menu_ProgramUpdates + "...";
            //_programUpdatesMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //_programUpdatesMenuItem.ToolTipText = Properties.Resources.Menu_ProgramUpdates;
            //_programUpdatesMenuItem.Image = GUIUtils.ConvertImage(GUIUtils.FindIcon(EAnnotationImage.ProgramUpdates));
            //_programUpdatesMenuItem.Click += ProgramUpdate_Click;
            //_menuStrip.Items.Add(_programUpdatesMenuItem);

            // Separator
            separator = new ToolStripSeparator();
            _menuStrip.Items.Add(separator);

            // About
            _helpAboutMenuItem = new ToolStripMenuItem();
            _helpAboutMenuItem.Text = Properties.Resources.Menu_About + "...";
            _helpAboutMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            _helpAboutMenuItem.ToolTipText = Properties.Resources.Menu_AboutToolTip;
            _helpAboutMenuItem.Image = GUIUtils.ConvertImage(GUIUtils.FindIcon(EAnnotationImage.Information));
            _helpAboutMenuItem.Click += HelpAbout_Click;
            _menuStrip.Items.Add(_helpAboutMenuItem);

            _recentFilesSeparator = new ToolStripSeparator();
            _menuStrip.Items.Add(_recentFilesSeparator);

            _recentFilesMenuItem = new ToolStripMenuItem();
            _recentFilesMenuItem.Text = Properties.Resources.Menu_RecentProfiles;
            _recentFilesMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            _menuStrip.Items.Add(_recentFilesMenuItem);

            separator = new ToolStripSeparator();
            _menuStrip.Items.Add(separator);

            _exitProgramMenuItem = new ToolStripMenuItem();
            _exitProgramMenuItem.Text = Properties.Resources.Menu_Exit;
            _exitProgramMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            _exitProgramMenuItem.ToolTipText = Properties.Resources.Menu_ExitToolTip;
            _exitProgramMenuItem.Image = GUIUtils.ConvertImage(GUIUtils.FindIcon(EAnnotationImage.Exit));
            _exitProgramMenuItem.Click += FileClose_Click;
            _menuStrip.Items.Add(_exitProgramMenuItem);

            // Hide / disable items as required
            _saveProfileMenuItem.Enabled = false;
            //_programUpdatesMenuItem.Visible = false;
            _recentFilesSeparator.Visible = false;
            _recentFilesMenuItem.Visible = false;
            if (!AppConfig.IsAdminMode)
            {
                _newTemplateMenuItem.Visible = false;
                _chooseTemplateMenuItem.Visible = false;
            }

            // Assign context menu
            _notifyIcon.ContextMenuStrip = _menuStrip;
        }

        /// <summary>
        /// Set the notify icon tooltip text       
        /// </summary>
        /// <remarks>Max text length supported by notify icon is 63 characters</remarks>
        /// <param name="text"></param>
        private void SetNotifyIconText(string text)
        {
            string shortText = text.Length < 64 ? text : text.Substring(0, 60) + "...";
            _notifyIcon.Text = shortText;
        }

        /// <summary>
        /// Open the context menu
        /// </summary>
        public void ShowContextMenu()
        {
            System.Drawing.Point position = System.Windows.Forms.Cursor.Position;
            _menuStrip.Show(position, ToolStripDropDownDirection.AboveLeft);
        }

        /// <summary>
        /// Handle click on system tray icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyIcon_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowContextMenu();
            }
        }

        /*
        /// <summary>
        /// Show or hide "Install updates" option, and delete msi after upgrade           
        /// </summary>
        private void InitialiseProgramUpdateOption()
        {
            try
            {
                bool canUpdate = false;
                float availableVersion = _appConfig.GetFloatVal(Constants.ConfigProgramUpdateAvailableVersion, -1f);
                if (availableVersion > 0f)
                {
                    string msiFilePath = Path.Combine(AppConfig.CommonAppDataDir, Constants.AppUpdateMSIFilename);
                    FileInfo fi = new FileInfo(msiFilePath);

                    // Check that a newer version is available
                    float appVersion;
                    if (float.TryParse(Constants.AppVersionString, NumberStyles.Number, CultureInfo.InvariantCulture, out appVersion) &&
                        appVersion < availableVersion)
                    {
                        //if (fi.Exists)
                        //{
                        //    // Check that the file size of the MSI is as expected (as a basic security check)
                        //    int fileSize = _appConfig.GetIntVal(Constants.ConfigProgramUpdateFileSize, 0);
                        //    if ((int)fi.Length == fileSize)
                        //    {
                        canUpdate = true;
                        //    }
                        //}
                    }
                    else
                    {
                        // Already up to date
                        _appConfig.SetStringVal(Constants.ConfigProgramUpdateAvailableVersion, null);
                        _appConfig.SetStringVal(Constants.ConfigProgramUpdateShowEULA, null);
                        _appConfig.SetStringVal(Constants.ConfigProgramUpdateReleaseNotes, null);
                        _appConfig.SetStringVal(Constants.ConfigProgramUpdateFileSize, null);
                    }

                    if (!canUpdate && fi.Exists)
                    {
                        // Delete msi file
                        File.Delete(fi.FullName);
                    }

                    // Delete updater program as long as it's not running                    
                    string updaterFilePath = Path.Combine(AppConfig.CommonAppDataDir, Constants.AppUpdaterProgramName + ".exe");
                    FileInfo exeFile = new FileInfo(updaterFilePath);
                    if (exeFile.Exists && !ProcessManager.IsRunning(Constants.AppUpdaterProgramName))
                    {
                        File.Delete(exeFile.FullName);
                    }
                }

                _programUpdatesMenuItem.Visible = canUpdate;
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_InitInstallUpdates, ex);
            }
        }
        */

        /// <summary>
        /// File New
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileNew_Click(object sender, EventArgs e)
        {
            try
            {
                DoFileNew(false);
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_CreateNewProfile, ex);
            }
        }

        /// <summary>
        /// File New template
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileNewTemplate_Click(object sender, EventArgs e)
        {
            try
            {
                DoFileNew(true);
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_CreateNewTemplate, ex);
            }
        }

        /// <summary>
        /// Create a new profile / template and open the designer window
        /// </summary>
        /// <param name="isTemplate"></param>
        private void DoFileNew(bool isTemplate)
        {
            // Check that the editor window isn't open
            if (_designerDialog != null)
            {
                HandleError(Properties.Resources.Info_ProfileDesignerOpen, new KxException(Properties.Resources.E_ProfileDesignerOpen));
                return;
            }

            // Check if the current profile needs saving
            MessageBoxResult result = SaveProfileIfRequired(true);
            if (result == MessageBoxResult.Cancel)
            {
                return;
            }

            // Create a blank profile if one isn't already loaded
            ProfileBuilder builder = new ProfileBuilder();
            Profile profile = builder.CreateDefaultProfile(isTemplate, Properties.Resources.String_DefaultControlSetName);
            profile.Initialise(_inputContext);
            profile.SetAppConfig(_appConfig);
            profile.IsModified = true;

            // Clear the last loaded profile path
            _appConfig.SetStringVal(Constants.ConfigUserLastUsedProfile, null);

            // Open the designer
            OpenProfileDesigner(profile);
        }

        /// <summary>
        /// File Open
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void FileOpen_Click(object sender, EventArgs e)
        {
            try
            {
                if (_profileBrowserDialog == null)
                {
                    //EnableMenu(false);

                    // Determine the user's profile directory
                    string defaultProfilesDir = Path.Combine(AppConfig.LocalAppDataDir, "Profiles");
                    string profilesDir = _appConfig.GetStringVal(Constants.ConfigUserCurrentProfileDirectory, defaultProfilesDir);

                    _profileBrowserDialog = new OpenProfileWindow(this, profilesDir, Constants.ProfileFileExtension);
                    _profileBrowserDialog.SetAppConfig(_appConfig);
                    _profileBrowserDialog.Left = 0;
                    _profileBrowserDialog.Top = 0;
                    _profileBrowserDialog.Show();
                }
                else if (_profileBrowserDialog.IsLoaded)
                {
                    _profileBrowserDialog.Activate();
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_OpenProfile, ex);
            }
        }

        /// <summary>
        /// File Open
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileOpenTemplate_Click(object sender, EventArgs e)
        {
            try
            {
                if (_profileBrowserDialog == null)
                {
                    //EnableMenu(false);

                    string templatesDir = Path.Combine(AppConfig.CommonAppDataDir, "Templates");

                    _profileBrowserDialog = new OpenProfileWindow(this, templatesDir, Constants.TemplateFileExtension);
                    _profileBrowserDialog.SetAppConfig(_appConfig);
                    _profileBrowserDialog.Left = 0;
                    _profileBrowserDialog.Top = 0;
                    _profileBrowserDialog.Show();
                }
                else if (_profileBrowserDialog.IsLoaded)
                {
                    _profileBrowserDialog.Activate();
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_OpenTemplate, ex);
            }
        }

        /// <summary>
        /// Open the Profiles folder in Explorer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProfilesFolder_Click(object sender, EventArgs e)
        {
            try
            {
                AppConfig appConfig = _appConfig;
                string defaultProfilesDir = Path.Combine(AppConfig.LocalAppDataDir, "Profiles");
                string profilesDir = appConfig.GetStringVal(Constants.ConfigUserCurrentProfileDirectory, defaultProfilesDir);
                if (Directory.Exists(profilesDir))
                {
                    ProcessManager.Start(profilesDir);
                }
                else
                {
                    HandleError(string.Format(Properties.Resources.E_DirectoryNotFound, profilesDir), null);
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_OpenProfilesFolder, ex);
            }
        }

        /// <summary>
        /// File Save
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileSave_Click(object sender, EventArgs e)
        {
            try
            {
                SaveProfileIfRequired(false);
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_SaveProfile, ex);
            }
        }

        /// <summary>
        /// Save a profile to a file
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="filePath"></param>
        private void SaveProfile(Profile profile, string filePath)
        {
            // Save to disk
            profile.ToFile(filePath);

            // Disable the save button
            _profileEdited = false;
            _saveProfileMenuItem.Enabled = false;

            // Show a temporary message            
            string message = Properties.Resources.String_Saved + " " + profile.Name;
            ShowTemporaryMessage(message, "", ToolTipIcon.Info);

            // Show the profile name in tooltips
            ShowProfileName(profile);
        }

        /// <summary>
        /// Show the name of the currently loaded profile
        /// </summary>
        /// <param name="profile"></param>
        private void ShowProfileName(Profile profile)
        {
            // Set tray icon tooltip
            SetNotifyIconText(string.Format("{0} - {1}", Constants.ProductName, profile.Name));

            // Set controller window tooltip(s)
            for (int i = Constants.ID1; i <= Constants.ID4; i++)
            {
                ControllerWindow window = _parentWindow.GetControllerWindow(i);
                bool windowVisible = window != null && window.IsVisible;
                if (windowVisible)
                {
                    string tooltip = i > Constants.ID1 ? string.Format("{0} {1} - {2}", Properties.Resources.String_Player, i, profile.Name) : profile.Name;
                    window.SetTitleBarToolTip(tooltip);
                }
            }
        }

        /// <summary>
        /// File Save As
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileSaveAs_Click(object sender, EventArgs e)
        {
            PerformSaveAs();
        }

        /// <summary>
        /// Perform Save As
        /// </summary>
        private void PerformSaveAs()
        {
            try
            {
                //EnableMenu(false);

                AppConfig appConfig = _appConfig;
                Profile profile = _parentWindow.GetProfile();

                string directory;
                string fileExtension;
                string filterString;
                string dialogTitle;
                if (profile.IsTemplate)
                {
                    fileExtension = Constants.TemplateFileExtension;
                    filterString = string.Format("{0} {1}|*{2}", Constants.ProductName, Properties.Resources.String_templates, fileExtension);
                    directory = Path.Combine(AppConfig.CommonAppDataDir, "Templates");
                    dialogTitle = Properties.Resources.String_SaveTemplate;
                }
                else
                {
                    fileExtension = Constants.ProfileFileExtension;
                    filterString = string.Format("{0} {1}|*{2}", Constants.ProductName, Properties.Resources.String_profiles, fileExtension);
                    string defaultProfilesDir = Path.Combine(AppConfig.LocalAppDataDir, "Profiles");
                    directory = appConfig.GetStringVal(Constants.ConfigUserCurrentProfileDirectory, defaultProfilesDir);
                    dialogTitle = Properties.Resources.String_SaveProfile;
                }

                // Resolve relative paths
                try
                {
                    DirectoryInfo di = new DirectoryInfo(directory);
                    directory = di.FullName;
                }
                catch (Exception)
                {
                }

                // Open the save dialog
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.AddExtension = true;
                dialog.CheckPathExists = true;
                dialog.CreatePrompt = false;
                dialog.DefaultExt = fileExtension;
                dialog.FileName = profile.Name;
                dialog.Filter = filterString;
                dialog.InitialDirectory = directory;
                dialog.OverwritePrompt = true;
                dialog.RestoreDirectory = true;
                dialog.Title = dialogTitle;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string filePath = dialog.FileName;

                    // Update the profile name
                    FileInfo fi = new FileInfo(filePath);
                    string name = fi.Name;
                    if (name.EndsWith(fileExtension))
                    {
                        name = name.Substring(0, name.Length - fileExtension.Length);
                    }
                    profile.Name = name;

                    // Save the profile
                    SaveProfile(profile, filePath);

                    // Store last used profile in app settings
                    appConfig.SetStringVal(Constants.ConfigUserLastUsedProfile, filePath);

                    // Refresh the recent profiles list
                    UpdateRecentProfiles(filePath);
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_SaveProfile, ex);
            }
        }

        /// <summary>
        /// Load a config profile from a file
        /// </summary>
        /// <param name="filePath">Null means load last used profile, empty string means load new blank profile</param>
        /// <returns>Whether loaded</returns>
        public bool LoadProfile(string filePath, bool isTemplate, bool checkForSave)
        {
            bool loaded = false;

            // Check that the editor window isn't open
            if (_designerDialog != null)
            {
                HandleError(Properties.Resources.Info_ProfileDesignerOpen, new KxException(Properties.Resources.E_ProfileDesignerOpen));
                return false;
            }

            if (checkForSave)
            {
                MessageBoxResult result = SaveProfileIfRequired(true);
                if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }

            AppConfig appConfig = _appConfig;

            bool errorOccurred = false;
            Profile profile = null;
            try
            {
                if (filePath == null)
                {
                    // Load from most recent location if required
                    bool autoLoadLastProfile = appConfig.GetBoolVal(Constants.ConfigAutoLoadLastProfile, Constants.DefaultAutoLoadLastProfile);
                    if (autoLoadLastProfile)
                    {
                        filePath = appConfig.GetStringVal(Constants.ConfigUserLastUsedProfile, null);
                        if (filePath == null)
                        {
                            // If it's the first use of the program, load some sample controls                        
                            string defaultProfilesDir = Path.Combine(AppConfig.LocalAppDataDir, "Profiles");
                            string profilesDir = _appConfig.GetStringVal(Constants.ConfigUserCurrentProfileDirectory, defaultProfilesDir);
                            filePath = Path.Combine(profilesDir, Constants.RecommendedProfileName + Constants.ProfileFileExtension);
                        }
                    }
                }

                // Try to load the profile
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    FileInfo fi = new FileInfo(filePath);

                    // Load from file
                    profile = new Profile();
                    profile.FromFile(fi.FullName);
                    profile.Initialise(_inputContext);
                    profile.SetAppConfig(_appConfig);

                    // Check that the type of profile matches what we expected
                    if (profile.IsTemplate != isTemplate)
                    {
                        HandleError(Properties.Resources.E_IncorrectFileFormat, new KxException(Properties.Resources.E_IncorrectFileFormatMessage));
                        profile = null;
                        errorOccurred = true;
                    }

                    if (profile != null)
                    {
                        // Set profile name
                        string fileExtension = profile.IsTemplate ? Constants.TemplateFileExtension : Constants.ProfileFileExtension;
                        string name = fi.Name;
                        if (name.EndsWith(fileExtension))
                        {
                            name = name.Substring(0, name.Length - fileExtension.Length);
                        }
                        profile.Name = name;

                        // Update last used profile
                        appConfig.SetStringVal(Constants.ConfigUserLastUsedProfile, fi.FullName);

                        // Update the recent profiles list
                        UpdateRecentProfiles(fi.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_ParseProfile, ex);
                profile = null;
                errorOccurred = true;
            }

            try
            {
                // If no profile was loaded, create a blank profile
                if (profile == null)
                {
                    // Create a blank profile if one isn't already loaded
                    ProfileBuilder builder = new ProfileBuilder();
                    profile = builder.CreateDefaultProfile(isTemplate, Properties.Resources.String_DefaultControlSetName);
                    profile.Initialise(_inputContext);
                    profile.SetAppConfig(_appConfig);

                    // Clear the last loaded profile
                    appConfig.SetStringVal(Constants.ConfigUserLastUsedProfile, null);

                    // If it's the first load, initialise the recent profile menu
                    if (_parentWindow.GetProfile() == null)
                    {
                        UpdateRecentProfiles(null);
                    }
                }

                // Apply the profile
                if (ApplyProfile(profile))
                {
                    loaded = true;

                    _profileEdited = false;
                    _saveProfileMenuItem.Enabled = false;

                    // Show a message
                    if (!errorOccurred)
                    {
                        string message = string.Format(Properties.Resources.String_LoadedX, profile.Name);
                        ShowTemporaryMessage(message, "", ToolTipIcon.Info);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_LoadProfile, ex);
            }

            return loaded;
        }

        /// <summary>
        /// Apply program options
        /// </summary>
        /// <param name="appConfig"></param>
        public void ApplyAppConfig(AppConfig appConfig)
        {
            _appConfig = appConfig;

            _parentWindow.SetAppConfig(appConfig);
            if (_designerDialog != null)
            {
                _designerDialog.SetAppConfig(appConfig);
            }
            if (_profileBrowserDialog != null)
            {
                _profileBrowserDialog.SetAppConfig(appConfig);
            }
            if (_logDialog != null)
            {
                _logDialog.SetAppConfig(appConfig);
            }
        }

        /// <summary>
        /// Apply a profile
        /// </summary>
        /// <param name="profile"></param>
        public bool ApplyProfile(Profile profile)
        {
            bool success = _parentWindow.ApplyProfile(profile);
            if (success)
            {
                _profileEdited = true;
                ShowProfileName(profile);

                _saveProfileMenuItem.Enabled = true;
            }

            return success;
        }

        /// <summary>
        /// Handle input language change
        /// </summary>
        public void KeyboardLayoutChanged(KxKeyboardChangeEventArgs args)
        {
            // See if the keyboard was changed with a Keysticks window active
            IntPtr keyboardLayout = WindowsAPI.GetKeyboardLayout(0);
            if (keyboardLayout != _inputContext.KeyboardHKL)
            {
                _inputContext = new KxKeyboardChangeEventArgs(keyboardLayout);

                // Update child dialogs
                if (_designerDialog != null)
                {
                    _designerDialog.KeyboardLayoutChanged(args);
                }

                if (_profileBrowserDialog != null)
                {
                    _profileBrowserDialog.KeyboardLayoutChanged(args);
                }
            }
        }

        /// <summary>
        /// Handle closing of child window
        /// </summary>
        /// <param name="window"></param>
        public void ChildWindowClosing(Window childWindow)
        {
            if (childWindow == _designerDialog)
            {
                _designerDialog = null;
            }
            else if (childWindow == _appConfigDialog)
            {
                _appConfigDialog = null;
            }
            else if (childWindow == _profileBrowserDialog)
            {
                _profileBrowserDialog = null;
            }
            else if (childWindow == _logDialog)
            {
                _logDialog = null;
            }

            ClearError();
        }

        /// <summary>
        /// File Close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileClose_Click(object sender, EventArgs e)
        {
            PromptToSaveAndExit();
        }

        /// <summary>
        /// Exit unless the user cancels
        /// </summary>
        /// <returns></returns>
        public bool PromptToSaveAndExit()
        {
            bool doExit = false;

            // Save profile if needed
            MessageBoxResult result = MessageBoxResult.None;
            try
            {
                result = SaveProfileIfRequired(true);
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_SaveCheck, ex);
            }

            // Close the application unless the user cancelled
            if (result != MessageBoxResult.Cancel)
            {
                _parentWindow.Close();
                doExit = true;
            }

            return doExit;
        }

        /// <summary>
        /// Save profile if user has changed it and wants to save
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private MessageBoxResult SaveProfileIfRequired(bool promptFirst)
        {
            // Ask the user whether they wish to save if appropriate
            MessageBoxResult result = MessageBoxResult.None;
            if (_profileEdited)
            {
                if (promptFirst)
                {
                    CustomMessageBox messageBox = new CustomMessageBox(null, Properties.Resources.Q_SaveProfileMessage, Properties.Resources.Q_SaveProfile, MessageBoxButton.YesNoCancel, true, false);
                    messageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    messageBox.ShowDialog();
                    result = messageBox.Result;
                }
                else
                {
                    result = MessageBoxResult.Yes;
                }

                if (result == MessageBoxResult.Yes)
                {
                    // Get the currently loaded profile path
                    AppConfig appConfig = _appConfig;
                    string filePath = appConfig.GetStringVal(Constants.ConfigUserLastUsedProfile, null);

                    // Save the profile
                    if (filePath != null)
                    {
                        FileInfo fi = new FileInfo(filePath);
                        if (fi.Exists && !fi.IsReadOnly)
                        {
                            Profile profile = _parentWindow.GetProfile();
                            SaveProfile(profile, filePath);
                        }
                        else
                        {
                            // Missing or read only
                            PerformSaveAs();
                        }
                    }
                    else
                    {
                        // No existing profile, so launch save dialog
                        PerformSaveAs();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Choose actions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void EditProfile_Click(object sender, EventArgs e)
        {
            try
            {
                if (_designerDialog != null)
                {
                    // Editor is already open
                    _designerDialog.Activate();
                    if (_designerDialog.WindowState == WindowState.Minimized)
                    {
                        _designerDialog.WindowState = WindowState.Normal;
                    }
                }
                else
                {
                    // Create a copy of the current profile to edit
                    Profile profile = _parentWindow.GetProfile();
                    if (profile != null)
                    {
                        profile = new Profile(profile);
                        profile.Initialise(_inputContext);
                        profile.SetAppConfig(_appConfig);
                        profile.IsModified = false;

                        // Open the designer window
                        OpenProfileDesigner(profile);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_OpenDesigner, ex);
            }
        }

        /// <summary>
        /// Open the Profile Designer window
        /// </summary>
        /// <param name="profile"></param>
        private void OpenProfileDesigner(Profile profile)
        {
            // Create edit dialog if reqd
            if (_designerDialog == null)
            {
                _designerDialog = new ProfileDesignerWindow(this);
            }

            _designerDialog.SetAppConfig(_appConfig);
            _designerDialog.SetProfile(profile);

            // Launch the edit dialog if not already launched
            if (!_designerDialog.IsLoaded)
            {
                // Show the dialog
                _designerDialog.Show();
            }
            else
            {
                _designerDialog.Activate();
            }
        }

        /// <summary>
        /// Show the controller windows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewControls_Click(object sender, EventArgs e)
        {
            try
            {
                _parentWindow.ShowControllerWindows();
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_ShowControllerWindow, ex);
            }
        }

        /// <summary>
        /// Show the help file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ViewHelp_Click(object sender, EventArgs e)
        {
            try
            {
                string helpFilePath = Path.Combine(AppConfig.ProgramRootDir, "Help", Constants.HelpFileName);
                ProcessManager.Start(helpFilePath);
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_OpenHelp, ex);
            }
        }

        /// <summary>
        /// Show the message logger
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ViewLogFile_Click(object sender, EventArgs e)
        {
            try
            {
                if (_logDialog != null)
                {
                    _logDialog.Activate();
                    if (_logDialog.WindowState == WindowState.Minimized)
                    {
                        _logDialog.WindowState = WindowState.Normal;
                    }
                }
                else
                {
                    _logDialog = new LogWindow(this, _parentWindow.GetMessageLogger());
                    _logDialog.SetAppConfig(_appConfig);
                    _logDialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    _logDialog.Show();
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_OpenLog, ex);
            }
        }

        /// <summary>
        /// Program options menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ToolsOptions_Click(object sender, EventArgs e)
        {
            try
            {
                if (_appConfigDialog == null)
                {
                    _appConfigDialog = new AppConfigWindow(this, _appConfig);
                    _appConfigDialog.Show();
                }
                else if (_appConfigDialog.IsLoaded)
                {
                    _appConfigDialog.Activate();
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_OpenProgramOptions, ex);
            }
        }

        /// <summary>
        /// Show the about window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HelpAbout_Click(object sender, EventArgs e)
        {
            try
            {
                _parentWindow.ShowHelpAboutWindow();
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_OpenAboutWindow, ex);
            }
        }

        /// <summary>
        /// Install program updates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void ProgramUpdate_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        PromptUserToUpdateProgram();
        //    }
        //    catch (Exception ex)
        //    {
        //        HandleError(Properties.Resources.E_ProgramUpdate, ex);
        //    }
        //}

        /// <summary>
        /// Show a program update, e.g. new version announcement.
        /// </summary>
        public void ShowProgramUpdate(string title, string description, string url)
        {
            // Get the details of the new release
            //string appVersion = _appConfig.GetStringVal(Constants.ConfigProgramUpdateAvailableVersion, "0.0");
            //string releaseNotes = _appConfig.GetStringVal(Constants.ConfigProgramUpdateReleaseNotes, "");

            //string caption = string.Format(Properties.Resources.Q_GoToX, Constants.AppWebsiteWithoutScheme);
            StringBuilder sb = new StringBuilder();
            //sb.AppendLine(string.Format(Properties.Resources.String_NewVersionAvailable, Constants.ProductName, appVersion));
            //sb.AppendLine(releaseNotes);
            sb.AppendLine(description);
            sb.AppendLine(Properties.Resources.Q_GoToWebsite);
            CustomMessageBox messageBox = new CustomMessageBox(null, sb.ToString(), title, MessageBoxButton.YesNoCancel, false, false);
            messageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            messageBox.ShowDialog();
            if (messageBox.Result == MessageBoxResult.Yes && url.StartsWith("http"))
            {
                string uri = Uri.EscapeUriString(url);
                ProcessManager.Start(uri);
            }
            /*
            // See if the MSI was downloaded
            string filePath = Path.Combine(AppConfig.CommonAppDataDir, Constants.AppUpdateMSIFilename);
            if (File.Exists(filePath))
            {
                // Make sure update option is visible in program menu
                ProgramUpdate.Visibility = Visibility.Visible;

                // Prompt the user
                string caption = string.Format(Properties.Resources.Q_InstallUpdates, Constants.ProductName);
                sb.AppendLine();
                sb.AppendLine(Properties.Resources.Q_InstallUpdatesMessage);
                CustomMessageBox messageBox = new CustomMessageBox(null, sb.ToString(), caption, MessageBoxButton.YesNoCancel, true, false);
                messageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                messageBox.ShowDialog();
                if (messageBox.Result == MessageBoxResult.Yes)
                {
                    // Check that the updater isn't already running
                    if (!ProcessManager.IsRunning(Constants.AppUpdaterProgramName))
                    {
                        // Start software update

                        // First copy the updater to a different location to its install location
                        // so that the installer doesn't try to overwrite it while it's running
                        string sourceExePath = Path.Combine(AppConfig.ProgramRootDir, Constants.AppUpdaterProgramName + ".exe");
                        string destExePath = Path.Combine(AppConfig.CommonAppDataDir, Constants.AppUpdaterProgramName + ".exe");
                        FileInfo sourceFile = new FileInfo(sourceExePath);
                        FileInfo destFile = new FileInfo(destExePath);
                        if (sourceFile.Exists && sourceExePath != destExePath)
                        {
                            File.Copy(sourceFile.FullName, destFile.FullName, true);

                            // Start the software updater in its new location, passing the install dir as a command line arg
                            DirectoryInfo installDir = new DirectoryInfo(AppConfig.ProgramRootDir);
                            string args = string.Format("/targetdir \"{0}\" /showeula {1}", installDir.FullName, showEULA);

                            StartProgramAction action = new StartProgramAction();
                            action.ProgramFolder = AppConfig.CommonAppDataDir;
                            action.ProgramName = Constants.AppUpdaterProgramName;
                            action.ProgramArgs = args;
                            ProcessManager.Start(action);
                        }
                    }
                    else
                    {
                        HandleError(Properties.Resources.Info_UpdaterAlreadyRunning, new KxException(string.Format(Properties.Resources.E_UpdaterAlreadyRunning, Constants.ProductName)));
                    }
                }
            }
            else
            {                
            // No MSI was downloaded so just inform the user that a new version is available
            sb.AppendLine(string.Format(Properties.Resources.Info_VisitWebsiteToUpdate, Constants.AppWebsiteWithHttp));
            ShowTemporaryMessage(sb.ToString(), sb.ToString(), ToolTipIcon.Info);
        }*/
        }

        /// <summary>
        /// Window closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Dispose()
        {
            try
            {
                // Close child windows if required
                if (_designerDialog != null)
                {
                    _designerDialog.Close();
                }
                if (_appConfigDialog != null)
                {
                    _appConfigDialog.Close();
                }
                if (_profileBrowserDialog != null)
                {
                    _profileBrowserDialog.Close();
                }
                if (_logDialog != null)
                {
                    _logDialog.Close();
                }

                if (_notifyIcon != null)
                {
                    _notifyIcon.Dispose();
                    _notifyIcon = null;
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_CloseTrayManager, ex);
            }
        }

        /// <summary>
        /// Handle renamed profile
        /// </summary>
        /// <param name="fromFile"></param>
        /// <param name="toFile"></param>
        public void ProfileRenamed(FileInfo fromFile, FileInfo toFile)
        {
            AppConfig appConfig = _appConfig;

            // Update last loaded profile
            if (fromFile.FullName == appConfig.GetStringVal(Constants.ConfigUserLastUsedProfile, null))
            {
                appConfig.SetStringVal(Constants.ConfigUserLastUsedProfile, toFile.FullName);

                // Update current profile's name
                Profile profile = _parentWindow.GetProfile();
                if (profile != null && !profile.IsTemplate)
                {
                    profile.Name = toFile.Name.Replace(Constants.ProfileFileExtension, "");
                }
            }

            // Update recent profiles list
            string recentProfiles = appConfig.GetStringVal(Constants.ConfigUserRecentProfilesList, "");
            if (recentProfiles.Contains(fromFile.FullName))
            {
                recentProfiles = recentProfiles.Replace(fromFile.FullName, toFile.FullName);
                appConfig.SetStringVal(Constants.ConfigUserRecentProfilesList, recentProfiles);

                // Update the recent profiles list
                UpdateRecentProfiles(null);
            }
        }

        /// <summary>
        /// Update the list of recent profiles
        /// </summary>
        /// <param name="loadedProfile"></param>
        private void UpdateRecentProfiles(string loadedProfile)
        {
            AppConfig appConfig = _appConfig;

            // Add new profile to the recent profiles list
            List<string> recentProfilesList = new List<string>();
            int count = 0;
            if (!string.IsNullOrEmpty(loadedProfile))
            {
                recentProfilesList.Add(loadedProfile);
                count++;
            }
            string recentProfilesStr = appConfig.GetStringVal(Constants.ConfigUserRecentProfilesList, "");
            string[] recentProfilesArray = recentProfilesStr.Split(',');
            foreach (string recentProfile in recentProfilesArray)
            {
                if (recentProfile != "" && !recentProfilesList.Contains(recentProfile))
                {
                    recentProfilesList.Add(recentProfile);
                    if (++count == Constants.MaxRecentProfiles)
                    {
                        break;
                    }
                }
            }

            // If a new profile has been added to the list, save it back to the app config
            if (!string.IsNullOrEmpty(loadedProfile))
            {
                // Convert to string
                StringBuilder sb = new StringBuilder();
                bool first = true;
                foreach (string path in recentProfilesList)
                {
                    if (!first)
                    {
                        sb.Append(',');
                    }
                    sb.Append(path);
                    first = false;
                }

                // Update config
                appConfig.SetStringVal(Constants.ConfigUserRecentProfilesList, sb.ToString());
            }

            // Refresh recent profiles menu
            RefreshRecentProfilesMenu(recentProfilesList);
        }

        /// <summary>
        /// Update the recent profiles list
        /// </summary>
        /// <param name="recentProfilesList"></param>
        private void RefreshRecentProfilesMenu(List<string> recentProfilesList)
        {
            int menuItemCount = 0;
            foreach (string filePath in recentProfilesList)
            {
                try
                {
                    // Check that the file exists and get the file name
                    FileInfo fi = new FileInfo(filePath);
                    if (fi.Exists)
                    {
                        // Get the display name
                        string fileName = fi.Name;
                        if (fileName.EndsWith(Constants.ProfileFileExtension))
                        {
                            fileName = fileName.Substring(0, fileName.Length - Constants.ProfileFileExtension.Length);
                        }
                        else if (fileName.EndsWith(Constants.TemplateFileExtension))
                        {
                            fileName = fileName.Substring(0, fileName.Length - Constants.TemplateFileExtension.Length);
                        }

                        // See if a menu item exists
                        ToolStripMenuItem menuItem;
                        if (menuItemCount < _recentFilesMenuItem.DropDownItems.Count)
                        {
                            menuItem = (ToolStripMenuItem)_recentFilesMenuItem.DropDownItems[menuItemCount];
                        }
                        else
                        {
                            menuItem = new ToolStripMenuItem();
                            menuItem.Click += this.LoadRecentProfile_Click;
                            _recentFilesMenuItem.DropDownItems.Add(menuItem);
                        }

                        menuItem.Tag = fi.FullName;
                        menuItem.ToolTipText = fi.FullName;
                        menuItem.Text = fileName;

                        menuItemCount++;
                    }
                }
                catch (Exception)
                {
                    // Ignore errors
                }
            }

            // Remove any surplus menu items
            while (menuItemCount < _recentFilesMenuItem.DropDownItems.Count)
            {
                _recentFilesMenuItem.DropDownItems.RemoveAt(menuItemCount);
            }

            _recentFilesMenuItem.Visible = menuItemCount != 0;
            _recentFilesSeparator.Visible = menuItemCount != 0;
        }

        /// <summary>
        /// Load a recent profile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadRecentProfile_Click(object sender, EventArgs e)
        {
            try
            {
                if (sender is ToolStripMenuItem)
                {
                    ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
                    if (menuItem.Tag is string)
                    {
                        string filePath = (string)menuItem.Tag;
                        LoadProfile(filePath, filePath.EndsWith(Constants.TemplateFileExtension), true);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_LoadProfile, ex);
            }
        }

        /// <summary>
        /// Show title bar or notification area message
        /// </summary>
        /// <param name="message"></param>
        public void ShowTemporaryMessage(string message, string caption, ToolTipIcon icon)
        {
            // If the main controller window is open, set a temporary title bar message
            ControllerWindow window = _parentWindow.GetControllerWindow(Constants.ID1);
            bool windowVisible = window != null && window.IsVisible;
            if (windowVisible)
            {
                window.SetTitleBarText(message, true);
                if (icon == ToolTipIcon.Warning ||
                    icon == ToolTipIcon.Error)
                {
                    window.ShowWarningIcon();
                }
            }
            else
            {
                // If running in the background, show tray notifications (if enabled)
                bool showTrayNotifications = (_appConfig != null) ? _appConfig.GetBoolVal(Constants.ConfigShowTrayNotifications, Constants.DefaultShowTrayNotifications) : Constants.DefaultShowTrayNotifications;
                if (showTrayNotifications)
                {
                    string text = message;
                    if (!string.IsNullOrEmpty(caption))
                    {
                        text += Environment.NewLine + caption;
                    }
                    _notifyIcon.ShowBalloonTip(Constants.TitleBarAnimationDuration, Constants.ProductName, text, icon);
                }
            }
        }

        /// <summary>
        /// Handle an error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void HandleError(string message, Exception ex)
        {
            _parentWindow.HandleError(message, ex);
        }

        /// <summary>
        /// Clear errors
        /// </summary>
        public void ClearError()
        {
            // If the main controller window is open, clear the warning icon
            ControllerWindow window = _parentWindow.GetControllerWindow(Constants.ID1);
            if (window != null)
            {
                window.ClearError();
            }
        }

    }
}
