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
using System.Windows.Threading;
using System.IO;
using System.Collections.ObjectModel;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.UI;

namespace Keysticks
{
    /// <summary>
    /// Program options window
    /// </summary>
    public partial class AppConfigWindow : Window
    {
        // Fields
        private bool _isLoaded;
        private readonly ITrayManager _parent;
        private readonly AppConfig _originalAppConfig;
        private readonly AppConfig _appConfig;
        private bool _refreshingDisplay = false;
        private bool _configChanged = false;
        private readonly DispatcherTimer _timer;
        private const int _timerIntervalMS = 500;
        private NamedItemList _playerColoursList;
        private ObservableCollection<CommandRuleItem> _commandRulesList;
        private NamedItemList _installedLanguagesList;
        private readonly StringUtils _utils = new StringUtils();

        /// <summary>
        /// Constructor
        /// </summary>
        public AppConfigWindow(ITrayManager parent, AppConfig appConfig)
        {
            _parent = parent;
            _originalAppConfig = appConfig;
            _appConfig = new AppConfig(_originalAppConfig);

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(_timerIntervalMS)
            };
            _timer.Tick += HandleTimeout;

            InitializeComponent();
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _isLoaded = true;
                _configChanged = false;

                // Bind languages combo box
                BindLanguagesComboBox();

                // Bind player colours
                _playerColoursList = new NamedItemList();
                PlayerColoursTable.ItemsSource = _playerColoursList;

                // Bind command rules
                _commandRulesList = new ObservableCollection<CommandRuleItem>();
                CommandRulesTable.ItemsSource = _commandRulesList;

                // Bind installed languages list
                _installedLanguagesList = new NamedItemList();
                LanguagesTable.ItemsSource = _installedLanguagesList;

                // Display config
                DisplayAppConfig(_appConfig);

                // Select a language
                this.LanguageComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_ShowProgramOptions, ex);
            }
        }

        /// <summary>
        /// Bind the combo box for selecting languages to download
        /// </summary>
        private void BindLanguagesComboBox()
        {
            NamedItemList languagesList = new NamedItemList();
            foreach (ELanguageCode eLang in Enum.GetValues(typeof(ELanguageCode)))
            {
                // Exclude the default preinstalled language which can't be uninstalled
                if (eLang != Constants.DefaultInstalledPredictionLanguage)
                {
                    NamedItem langItem = new NamedItem((int)eLang, _utils.LanguageCodeToString(eLang));
                    languagesList.Add(langItem);
                }
            }
            this.LanguageComboBox.ItemsSource = languagesList;
        }
      
        /// <summary>
        /// Display the configuration
        /// </summary>
        /// <param name="appConfig"></param>
        private void DisplayAppConfig(AppConfig appConfig)
        {
            // Prevent preview updates which refreshing display
            _refreshingDisplay = true;

            // Messages tab
            this.ConfirmApplyTemplatesCheckbox.IsChecked = !appConfig.GetBoolVal(Constants.ConfigAutoConfirmApplyTemplates, Constants.DefaultAutoConfirmApplyTemplates);
            this.ConfirmClearActionsCheckbox.IsChecked = !appConfig.GetBoolVal(Constants.ConfigAutoConfirmClearActions, Constants.DefaultAutoConfirmClearActions);
            //this.ConfirmChangeActionListLengthCheckbox.IsChecked = !appConfig.GetBoolVal(Constants.ConfigAutoConfirmActionListLengthChange, Constants.DefaultAutoConfirmActionListLengthChange);
            this.ConfirmDeleteKeyboardCheckbox.IsChecked = !appConfig.GetBoolVal(Constants.ConfigAutoConfirmDeleteKeyboard, Constants.DefaultAutoConfirmDeleteKeyboard);
            this.ConfirmDeleteControlSetsCheckbox.IsChecked = !appConfig.GetBoolVal(Constants.ConfigAutoConfirmDeleteControlSet, Constants.DefaultAutoConfirmDeleteControlSet);
            this.ConfirmDeletePagesCheckbox.IsChecked = !appConfig.GetBoolVal(Constants.ConfigAutoConfirmDeletePage, Constants.DefaultAutoConfirmDeletePage);
            this.ConfirmDeleteVirtualControlsCheckbox.IsChecked = !appConfig.GetBoolVal(Constants.ConfigAutoConfirmDeleteVirtualControl, Constants.DefaultAutoConfirmDeleteVirtualControl);
            this.ConfirmDeletePlayerCheckbox.IsChecked = !appConfig.GetBoolVal(Constants.ConfigAutoConfirmDeletePlayer, Constants.DefaultAutoConfirmDeletePlayer);

            // General tab
            this.StartWhenWindowsStartsCheckbox.IsChecked = appConfig.GetBoolVal(Constants.ConfigStartWhenWindowsStarts, Constants.DefaultStartWhenWindowsStarts);
            this.AutoLoadCheckbox.IsChecked = appConfig.GetBoolVal(Constants.ConfigAutoLoadLastProfile, Constants.DefaultAutoLoadLastProfile);
            this.ShowControlsWindowCheckbox.IsChecked = appConfig.GetBoolVal(Constants.ConfigShowControlsWindow, Constants.DefaultShowControlsWindow);
            this.AllowBackgroundRunningCheckbox.IsChecked = appConfig.GetBoolVal(Constants.ConfigAllowBackgroundRunning, Constants.DefaultAllowBackgroundRunning);
            this.ShowHeldModifierKeysCheckbox.IsChecked = appConfig.GetBoolVal(Constants.ConfigShowHeldModifierKeys, Constants.DefaultShowHeldModifierKeys);
            this.ShowHeldMouseButtonsCheckbox.IsChecked = appConfig.GetBoolVal(Constants.ConfigShowHeldMouseButtons, Constants.DefaultShowHeldMouseButtons);
            this.ShowTrayNotificationsCheckbox.IsChecked = appConfig.GetBoolVal(Constants.ConfigShowTrayNotifications, Constants.DefaultShowTrayNotifications);
            this.CheckForUpdatesCheckbox.IsChecked = appConfig.GetBoolVal(Constants.ConfigCheckForProgramUpdates, Constants.DefaultCheckForProgramUpdates);

            // Colours tab
            this.CurrentControlsOpacitySlider.Value = appConfig.GetIntVal(Constants.ConfigCurrentControlsOpacityPercent, Constants.DefaultWindowOpacityPercent);
            this.InteractiveControlsOpacitySlider.Value = appConfig.GetIntVal(Constants.ConfigInteractiveControlsOpacityPercent, Constants.DefaultWindowOpacityPercent);

            ColourScheme colourScheme = appConfig.ColourScheme;
            PlayerColourScheme player1Colours = colourScheme.GetPlayerColours(Constants.ID1);
            PlayerColourScheme player2Colours = colourScheme.GetPlayerColours(Constants.ID2);
            PlayerColourScheme player3Colours = colourScheme.GetPlayerColours(Constants.ID3);
            PlayerColourScheme player4Colours = colourScheme.GetPlayerColours(Constants.ID4);

            _playerColoursList.Clear();
            PlayersColourItem colourItem = new PlayersColourItem(Constants.ID1, Properties.Resources.String_KeyboardBackground + " (" + Properties.Resources.String_primary + ")")
            {
                Player1Colour = player1Colours.CellColour,
                Player2Colour = player2Colours.CellColour,
                Player3Colour = player3Colours.CellColour,
                Player4Colour = player4Colours.CellColour
            };
            _playerColoursList.Add(colourItem);
            colourItem = new PlayersColourItem(Constants.ID2, Properties.Resources.String_KeyboardBackground + " (" + Properties.Resources.String_secondary + ")")
            {
                Player1Colour = player1Colours.AlternateCellColour,
                Player2Colour = player2Colours.AlternateCellColour,
                Player3Colour = player3Colours.AlternateCellColour,
                Player4Colour = player4Colours.AlternateCellColour
            };
            _playerColoursList.Add(colourItem);
            colourItem = new PlayersColourItem(Constants.ID3, Properties.Resources.String_Highlights)
            {
                Player1Colour = player1Colours.HighlightColour,
                Player2Colour = player2Colours.HighlightColour,
                Player3Colour = player3Colours.HighlightColour,
                Player4Colour = player4Colours.HighlightColour
            };
            _playerColoursList.Add(colourItem);
            colourItem = new PlayersColourItem(Constants.ID4, Properties.Resources.String_Selections)
            {
                Player1Colour = player1Colours.SelectionColour,
                Player2Colour = player2Colours.SelectionColour,
                Player3Colour = player3Colours.SelectionColour,
                Player4Colour = player4Colours.SelectionColour
            };
            _playerColoursList.Add(colourItem);

            // Style tab
            this.ZoomFactorSlider.Maximum = (int)(0.25 * SystemParameters.VirtualScreenWidth);     // 100% * ScreenWidth / 400 (=default controller window width)
            this.ZoomFactorSlider.Value = appConfig.GetIntVal(Constants.ConfigZoomFactorPercent, Constants.DefaultZoomFactorPercent);
            this.CompactControlsWindowCheckbox.IsChecked = appConfig.GetBoolVal(Constants.ConfigCompactControlsWindow, Constants.DefaultCompactControlsWindow);
            this.KeepControlsWindowOnTopCheckbox.IsChecked = appConfig.GetBoolVal(Constants.ConfigKeepControlsWindowOnTop, Constants.DefaultKeepControlsWindowOnTop);
            this.AnimateControlsCheckbox.IsChecked = appConfig.GetBoolVal(Constants.ConfigAnimateControls, Constants.DefaultAnimateControls);
            this.ShowTitleBarCheckbox.IsChecked = appConfig.GetBoolVal(Constants.ConfigShowInteractiveControlsTitleBar, Constants.DefaultShowInteractiveControlsTitleBar);
            this.ShowFooterCheckbox.IsChecked = appConfig.GetBoolVal(Constants.ConfigShowInteractiveControlsFooter, Constants.DefaultShowInteractiveControlsFooter);

            // Input tab
            // Note: Override checkbox has opposite value to stored 'use sensitivity' setting
            this.OverrideDeadZonesCheckbox.IsChecked = !appConfig.GetBoolVal(Constants.ConfigUseDefaultSensitivity, Constants.DefaultUseDefaultSensitivity);
            this.TriggerDeadZoneSlider.Value = appConfig.GetFloatVal(Constants.ConfigTriggerDeadZoneFraction, Constants.DefaultTriggerDeadZoneFraction);
            this.ThumbstickDeadZoneSlider.Value = appConfig.GetFloatVal(Constants.ConfigThumbstickDeadZoneFraction, Constants.DefaultStickDeadZoneFraction);

            // Output tab
            this.MousePointerSpeedSlider.Value = appConfig.GetFloatVal(Constants.ConfigMousePointerSpeed, Constants.DefaultMousePointerSpeed);
            this.MousePointerAccelerationSlider.Value = appConfig.GetFloatVal(Constants.ConfigMousePointerAcceleration, Constants.DefaultMousePointerAcceleration);
            this.DefaultMouseClickTimeSlider.Value = 0.001 * appConfig.GetIntVal(Constants.ConfigMouseClickLengthMS, Constants.DefaultMouseClickLengthMS);
            this.DefaultKeyStrokeTimeSlider.Value = 0.001 * appConfig.GetIntVal(Constants.ConfigKeyStrokeLengthMS, Constants.DefaultKeyStrokeLengthMS);
            if (appConfig.GetBoolVal(Constants.ConfigUseScanCodes, Constants.DefaultUseScanCodes))
            {
                this.ScanCodesRadioButton.IsChecked = true;
            }
            else
            {
                this.VirtualKeysRadioButton.IsChecked = true;
            }
            
            // Word prediction tab
            this.EnableWordPredictionCheckbox.IsChecked = appConfig.GetBoolVal(Constants.ConfigEnableWordPrediction, Constants.DefaultEnableWordPrediction);
            this.AutoInsertSpaceCheckbox.IsChecked = appConfig.GetBoolVal(Constants.ConfigAutoInsertSpaces, Constants.DefaultAutoInsertSpaces);
            this.LearnNewWordsCheckbox.IsChecked = appConfig.GetBoolVal(Constants.ConfigLearnNewWords, Constants.DefaultLearnNewWords);
            DisplayInstalledLanguages(appConfig);

            // Folders tab
            string defaultProfilesDir = Path.Combine(AppConfig.LocalAppDataDir, "Profiles");
            string profilesDir = appConfig.GetStringVal(Constants.ConfigUserCurrentProfileDirectory, defaultProfilesDir);
            DirectoryInfo di = new DirectoryInfo(profilesDir);
            this.ProfilesFolderTextBox.Text = di.FullName;
            this.ProfilesFolderTextBox.ToolTip = di.FullName;

            // Security tab
            this.MaxActionListLengthSlider.Value = appConfig.GetIntVal(Constants.ConfigMaxActionListLength, Constants.DefaultMaxActionListLength);
            this.DisallowShiftDeleteCheckBox.IsChecked = appConfig.GetBoolVal(Constants.ConfigDisallowShiftDelete, Constants.DefaultDisallowShiftDelete);
            this.DisallowDangerousControlsCheckBox.IsChecked = appConfig.GetBoolVal(Constants.ConfigDisallowDangerousControls, Constants.DefaultDisallowDangerousControls);
            CommandRuleManager ruleManager = new CommandRuleManager();
            ruleManager.FromConfig(appConfig);
            _commandRulesList.Clear();
            foreach (CommandRuleItem rule in ruleManager.Rules)
            {
                _commandRulesList.Add(rule);
            }
            
            _refreshingDisplay = false;
        }

        /// <summary>
        /// Display the list of installed languages and whether or not they are active
        /// </summary>
        /// <param name="appConfig"></param>
        private void DisplayInstalledLanguages(AppConfig appConfig)
        {
            string installedLanguagesStr = appConfig.GetStringVal(Constants.ConfigWordPredictionInstalledLanguages, Constants.DefaultWordPredictionInstalledLanguages);
            if (installedLanguagesStr != Constants.DefaultWordPredictionInstalledLanguages)
            {
                // Read which languages are installed from app config and whether they are active
                NamedItemList langList = _utils.LanguageListFromString(installedLanguagesStr);
                _installedLanguagesList.Clear();
                foreach (OptionalNamedItem langItem in langList)
                {
                    _installedLanguagesList.Add(langItem);
                }
            }
            else
            {
                // Display based upon package files
                PopulateLanguageListFromPackageFiles();
            }
        }

        /// <summary>
        /// If the language config isn't present in the app config, assume it is the default language plus the packages found
        /// </summary>
        private void PopulateLanguageListFromPackageFiles()
        {
            _installedLanguagesList.Clear();

            // Add the default installed language
            OptionalNamedItem langItem = new OptionalNamedItem((int)Constants.DefaultInstalledPredictionLanguage, _utils.LanguageCodeToString(Constants.DefaultInstalledPredictionLanguage), true);     // Active
            _installedLanguagesList.Add(langItem);

            // Add the packages
            string packagesFolderPath = Path.Combine(AppConfig.CommonAppDataDir, "base", "Packages");
            DirectoryInfo di = new DirectoryInfo(packagesFolderPath);
            if (di.Exists)
            {
                FileInfo[] packageFiles = di.GetFiles("*" + Constants.PackageFileExtension);
                foreach (FileInfo packageFile in packageFiles)
                {
                    string langCodeName = packageFile.Name.Replace(Constants.PackageFileExtension, "");
                    if (Enum.TryParse<ELanguageCode>(langCodeName, out ELanguageCode langCode) &&
                        langCode != Constants.DefaultInstalledPredictionLanguage)
                    {
                        langItem = new OptionalNamedItem((int)langCode, _utils.LanguageCodeToString(langCode), false);  // Inactive
                        _installedLanguagesList.Add(langItem);
                    }
                }
            }
        }

        /// <summary>
        /// OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                // Get the profiles folder
                string profilesDir = this.ProfilesFolderTextBox.Text;
                if (!Directory.Exists(profilesDir))
                {
                    CustomMessageBox messageBox = new CustomMessageBox(this,
                                                                        Properties.Resources.Q_CreateProfilesFolderMessage,
                                                                        Properties.Resources.Q_CreateProfilesFolder,
                                                                        MessageBoxButton.YesNoCancel,
                                                                        true,
                                                                        false);
                    messageBox.ShowDialog();
                    if (messageBox.Result == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                    else if (messageBox.Result == MessageBoxResult.Yes)
                    {
                        Directory.CreateDirectory(profilesDir);
                    }
                }

                // Apply the display config options
                ApplyCurrentConfig();

                // Save app config and indicate it's up to date
                _appConfig.Save();
                _configChanged = false;

                this.Close();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_UpdateProgramOptions, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Reset config to program defaults
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RestoreDefaultsButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Clear();
            CustomMessageBox messageBox = new CustomMessageBox(this, Properties.Resources.Q_ResetProgramOptionsMessage, Properties.Resources.Q_ResetProgramOptions, MessageBoxButton.OKCancel, true, false);
            if (messageBox.ShowDialog() == true)
            {
                try
                {
                    // Display default config
                    DisplayAppConfig(new AppConfig());

                    // Apply
                    ScheduleAsyncConfigApply();
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_ResetProgramOptions, ex);
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Clear();
            this.Close();

            e.Handled = true;
        }

        /// <summary>
        /// Window closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Stop any async update
            _timer.IsEnabled = false;

            // Restore the app config if the user changed it and then cancelled
            if (_configChanged)
            {
                _parent.ApplyAppConfig(_originalAppConfig);
            }

            // Tell the parent we're closing
            _parent.ChildWindowClosing(this);
        }

        /// <summary>
        /// Selected available language changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();
                NamedItem langItem = (NamedItem)this.LanguageComboBox.SelectedItem;
                if (langItem != null)
                {
                    int langID = langItem.ID;
                    bool isInstalled = (_installedLanguagesList.GetItemByID(langID) != null);
                    AddLanguageButton.IsEnabled = !isInstalled;
                }
                else
                {
                    AddLanguageButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_SelectionChange, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Add button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddLanguageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the language to add
                ErrorMessage.Clear();
                NamedItem selectedItem = (NamedItem)this.LanguageComboBox.SelectedItem;
                if (selectedItem != null)
                {
                    // Confirm with user
                    string message = string.Format(Properties.Resources.Q_AddLanguageMessage, selectedItem.Name);
                    CustomMessageBox messageBox = new CustomMessageBox(this, message, Properties.Resources.Q_AddLanguage, MessageBoxButton.OKCancel, true, false);
                    messageBox.ShowDialog();
                    if (messageBox.Result == MessageBoxResult.OK)
                    {
                        OptionalNamedItem langItem = new OptionalNamedItem(selectedItem.ID, selectedItem.Name, true);
                        _installedLanguagesList.Insert(0, langItem);
                        AddLanguageButton.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_AddLanguage, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Move dictionary up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();
                int selectedIndex = this.LanguagesTable.SelectedIndex;
                if (selectedIndex > 0 && selectedIndex < _installedLanguagesList.Count)
                {
                    NamedItem previousItem = _installedLanguagesList[selectedIndex - 1];
                    _installedLanguagesList.RemoveAt(selectedIndex - 1);
                    _installedLanguagesList.Insert(selectedIndex, previousItem);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_ReorderItems, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Move dictionary down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();
                int selectedIndex = this.LanguagesTable.SelectedIndex;
                if (selectedIndex > -1 && selectedIndex < _installedLanguagesList.Count - 1)
                {
                    NamedItem nextItem = _installedLanguagesList[selectedIndex + 1];
                    _installedLanguagesList.RemoveAt(selectedIndex + 1);
                    _installedLanguagesList.Insert(selectedIndex, nextItem);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_ReorderItems, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Update the config from the UI
        /// </summary>
        private void UpdateConfig()
        {
            // General tab
            _appConfig.SetBoolVal(Constants.ConfigStartWhenWindowsStarts, this.StartWhenWindowsStartsCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigAutoLoadLastProfile, this.AutoLoadCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigShowControlsWindow, this.ShowControlsWindowCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigAllowBackgroundRunning, this.AllowBackgroundRunningCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigShowHeldModifierKeys, this.ShowHeldModifierKeysCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigShowHeldMouseButtons, this.ShowHeldMouseButtonsCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigShowTrayNotifications, this.ShowTrayNotificationsCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigCheckForProgramUpdates, this.CheckForUpdatesCheckbox.IsChecked == true);

            // Colours tab
            _appConfig.SetIntVal(Constants.ConfigCurrentControlsOpacityPercent, (int)this.CurrentControlsOpacitySlider.Value);
            _appConfig.SetIntVal(Constants.ConfigInteractiveControlsOpacityPercent, (int)this.InteractiveControlsOpacitySlider.Value);
            PlayersColourItem colourItem = (PlayersColourItem)_playerColoursList.GetItemByID(Constants.ID1);
            _appConfig.SetPlayerVal(Constants.ID1, Constants.ConfigCellColour, colourItem.Player1Colour);
            _appConfig.SetPlayerVal(Constants.ID2, Constants.ConfigCellColour, colourItem.Player2Colour);
            _appConfig.SetPlayerVal(Constants.ID3, Constants.ConfigCellColour, colourItem.Player3Colour);
            _appConfig.SetPlayerVal(Constants.ID4, Constants.ConfigCellColour, colourItem.Player4Colour);
            colourItem = (PlayersColourItem)_playerColoursList.GetItemByID(Constants.ID2);
            _appConfig.SetPlayerVal(Constants.ID1, Constants.ConfigAlternateCellColour, colourItem.Player1Colour);
            _appConfig.SetPlayerVal(Constants.ID2, Constants.ConfigAlternateCellColour, colourItem.Player2Colour);
            _appConfig.SetPlayerVal(Constants.ID3, Constants.ConfigAlternateCellColour, colourItem.Player3Colour);
            _appConfig.SetPlayerVal(Constants.ID4, Constants.ConfigAlternateCellColour, colourItem.Player4Colour);
            colourItem = (PlayersColourItem)_playerColoursList.GetItemByID(Constants.ID3);
            _appConfig.SetPlayerVal(Constants.ID1, Constants.ConfigHighlightColour, colourItem.Player1Colour);
            _appConfig.SetPlayerVal(Constants.ID2, Constants.ConfigHighlightColour, colourItem.Player2Colour);
            _appConfig.SetPlayerVal(Constants.ID3, Constants.ConfigHighlightColour, colourItem.Player3Colour);
            _appConfig.SetPlayerVal(Constants.ID4, Constants.ConfigHighlightColour, colourItem.Player4Colour);
            colourItem = (PlayersColourItem)_playerColoursList.GetItemByID(Constants.ID4);
            _appConfig.SetPlayerVal(Constants.ID1, Constants.ConfigSelectionColour, colourItem.Player1Colour);
            _appConfig.SetPlayerVal(Constants.ID2, Constants.ConfigSelectionColour, colourItem.Player2Colour);
            _appConfig.SetPlayerVal(Constants.ID3, Constants.ConfigSelectionColour, colourItem.Player3Colour);
            _appConfig.SetPlayerVal(Constants.ID4, Constants.ConfigSelectionColour, colourItem.Player4Colour);
            _appConfig.ClearCache();

            // Style tab
            _appConfig.SetIntVal(Constants.ConfigZoomFactorPercent, (int)this.ZoomFactorSlider.Value);
            _appConfig.SetBoolVal(Constants.ConfigCompactControlsWindow, this.CompactControlsWindowCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigKeepControlsWindowOnTop, this.KeepControlsWindowOnTopCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigAnimateControls, this.AnimateControlsCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigShowInteractiveControlsTitleBar, this.ShowTitleBarCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigShowInteractiveControlsFooter, this.ShowFooterCheckbox.IsChecked == true);

            // Input tab            
            // Note: Override checkbox has opposite value to stored 'use sensitivity' setting
            _appConfig.SetBoolVal(Constants.ConfigUseDefaultSensitivity, this.OverrideDeadZonesCheckbox.IsChecked != true);
            _appConfig.SetFloatVal(Constants.ConfigTriggerDeadZoneFraction, (float)this.TriggerDeadZoneSlider.Value);
            _appConfig.SetFloatVal(Constants.ConfigThumbstickDeadZoneFraction, (float)this.ThumbstickDeadZoneSlider.Value);            

            // Output tab
            _appConfig.SetFloatVal(Constants.ConfigMousePointerSpeed, (float)this.MousePointerSpeedSlider.Value);
            _appConfig.SetFloatVal(Constants.ConfigMousePointerAcceleration, (float)this.MousePointerAccelerationSlider.Value);
            _appConfig.SetIntVal(Constants.ConfigMouseClickLengthMS, (int)(this.DefaultMouseClickTimeSlider.Value * 1000));
            _appConfig.SetIntVal(Constants.ConfigKeyStrokeLengthMS, (int)(this.DefaultKeyStrokeTimeSlider.Value * 1000));
            _appConfig.SetBoolVal(Constants.ConfigUseScanCodes, this.ScanCodesRadioButton.IsChecked == true);

            // Word prediction tab
            _appConfig.SetBoolVal(Constants.ConfigEnableWordPrediction, this.EnableWordPredictionCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigAutoInsertSpaces, this.AutoInsertSpaceCheckbox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigLearnNewWords, this.LearnNewWordsCheckbox.IsChecked == true);
            string languagesList = _utils.LanguageListToString(_installedLanguagesList);
            _appConfig.SetStringVal(Constants.ConfigWordPredictionInstalledLanguages, languagesList);

            // Messages tab
            _appConfig.SetBoolVal(Constants.ConfigAutoConfirmApplyTemplates, this.ConfirmApplyTemplatesCheckbox.IsChecked == false);
            _appConfig.SetBoolVal(Constants.ConfigAutoConfirmClearActions, this.ConfirmClearActionsCheckbox.IsChecked == false);
            _appConfig.SetBoolVal(Constants.ConfigAutoConfirmDeleteKeyboard, this.ConfirmDeleteKeyboardCheckbox.IsChecked == false);
            _appConfig.SetBoolVal(Constants.ConfigAutoConfirmDeleteControlSet, this.ConfirmDeleteControlSetsCheckbox.IsChecked == false);
            _appConfig.SetBoolVal(Constants.ConfigAutoConfirmDeletePage, this.ConfirmDeletePagesCheckbox.IsChecked == false);
            _appConfig.SetBoolVal(Constants.ConfigAutoConfirmDeleteVirtualControl, this.ConfirmDeleteVirtualControlsCheckbox.IsChecked == false);
            _appConfig.SetBoolVal(Constants.ConfigAutoConfirmDeletePlayer, this.ConfirmDeletePlayerCheckbox.IsChecked == false);

            // Folders tab
            DirectoryInfo di = new DirectoryInfo(this.ProfilesFolderTextBox.Text);
            _appConfig.SetStringVal(Constants.ConfigUserCurrentProfileDirectory, di.FullName);

            // Security tab
            _appConfig.SetIntVal(Constants.ConfigMaxActionListLength, (int)this.MaxActionListLengthSlider.Value);
            _appConfig.SetBoolVal(Constants.ConfigDisallowShiftDelete, this.DisallowShiftDeleteCheckBox.IsChecked == true);
            _appConfig.SetBoolVal(Constants.ConfigDisallowDangerousControls, this.DisallowDangerousControlsCheckBox.IsChecked == true);
            CommandRuleManager ruleManager = new CommandRuleManager(_commandRulesList);
            ruleManager.ToConfig(_appConfig);
        }

        /// <summary>
        /// Handle change of checkbox / radio button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void PreviewableControl_Changed(object sender, RoutedEventArgs args)
        {
            ScheduleAsyncConfigApply();
            args.Handled = true;
        }

        /// <summary>
        /// Handle slider change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void PreviewableControl_ValueChanged(object sender, KxDoubleValRoutedEventArgs args)
        {
            ScheduleAsyncConfigApply();
            args.Handled = true;
        }

        /// <summary>
        /// Handle combo box change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewableControl_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ScheduleAsyncConfigApply();
            e.Handled = true;
        }

        /// <summary>
        /// Schedule an async config apply
        /// </summary>
        private void ScheduleAsyncConfigApply()
        {
            if (_isLoaded && !_refreshingDisplay && !_timer.IsEnabled)
            {
                _timer.Start();
            }
        }

        /// <summary>
        /// Perform a GUI update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HandleTimeout(object sender, EventArgs args)
        {
            // Stop the timer
            _timer.Stop();

            ApplyCurrentConfig();            
        }

        /// <summary>
        /// Apply the selected config options
        /// </summary>
        private void ApplyCurrentConfig()
        {
            // Retrieve current config from UI
            UpdateConfig();

            // Apply
            AppConfig appConfig = new AppConfig(_appConfig);
            _parent.ApplyAppConfig(appConfig);
            _configChanged = true;
        }

        /// <summary>
        /// Let the user choose a Profiles folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();
                DirectoryInfo di = new DirectoryInfo(ProfilesFolderTextBox.Text);
                System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (di.Exists)
                {
                    dialog.SelectedPath = di.FullName;
                }
                dialog.Description = string.Format(Properties.Resources.Info_ChooseProfilesFolder, Constants.ProductName);
                dialog.ShowNewFolderButton = true;
                if (System.Windows.Forms.DialogResult.OK == dialog.ShowDialog())
                {
                    ProfilesFolderTextBox.Text = dialog.SelectedPath;
                    ProfilesFolderTextBox.ToolTip = dialog.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_ChangeProfilesFolder, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Disable word prediction options if word prediction is not enabled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnableWordPredictionCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Clear();
            bool enable = (this.EnableWordPredictionCheckbox.IsChecked == true);
            this.AutoInsertSpaceCheckbox.IsEnabled = enable;
            this.LearnNewWordsCheckbox.IsEnabled = enable;
            this.LanguagesTable.IsEnabled = enable;
            this.UpDownButtonsPanel.IsEnabled = enable;
            this.DownloadLanguagesGroupBox.IsEnabled = enable;

            e.Handled = true;
        }

        /// <summary>
        /// Disable dead zone sliders if default sensitivity is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UseDefaultSensitivityCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Clear();
            bool enable = (this.OverrideDeadZonesCheckbox.IsChecked == true);
            this.TriggerDeadZoneLabel.IsEnabled = enable;
            this.TriggerDeadZoneSlider.IsEnabled = enable;
            this.ThumbstickDeadZoneLabel.IsEnabled = enable;
            this.ThumbstickDeadZoneSlider.IsEnabled = enable;

            ScheduleAsyncConfigApply();

            e.Handled = true;
        }

        /// <summary>
        /// Disable the maximum action list length setting if the user has disabled the dangerous controls check
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisallowDangerousControlsCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Clear();
            bool enable = (this.DisallowDangerousControlsCheckBox.IsChecked == true);
            this.MaxActionListLengthSlider.IsEnabled = enable;

            ScheduleAsyncConfigApply();

            e.Handled = true;
        }

    }

}
