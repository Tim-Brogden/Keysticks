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
using System.Windows.Media;

namespace Keysticks.Core
{
    /// <summary>
    /// Application-wide constants
    /// </summary>
    public class Constants
    {
        // Update these two when the file or application version changes
        // Both values should be parseable as floats
        public const string FileVersionString = "2.1";
        public const string AppVersionString = "2.14";

        public const string ReleaseDateYear = "2024";
        public const string AuthorName = "Tim Brogden";
        public const string Manufacturer = "Keysticks.net";
        public const string ProductName = "Keysticks";

        public const string ProductLogoFileName = "KeysticksLogo.ico";
        public const string KLogoFileName = "KLogo.ico";
        public const string HelpFileName = "Keysticks.chm";
        public const string AppWebsiteWithScheme = "https://keysticks.net";
        public const string AppWebsiteWithoutScheme = "Keysticks.net";
        public const string AppUpdateMSIFilename = "KeysticksSetup.msi";
        public const string AppUpdaterProgramName = "KeysticksUpdater";
        public const string DownloadPageURL = "https://keysticks.net/download/";
        public const string NewUserURL = "https://keysticks.net/register/";
        public const string LocalWebServiceURL = "http://localhost/keysticks/webservices/service.php";
        public const string WebServiceURL = "https://keysticks.net/webservices/service.php";
        public const string MessageLogFileName = "Message Log.txt";
        public const string ProfileListCacheFileName = "Profiles Cache.dat";
        public const string RecommendedProfileName = "P01 - Standard keyboard";
        public const string ProfileFileExtension = ".keyx";
        public const string TemplateFileExtension = ".keyt";
        public const string PackageFileExtension = ".atp";
        public const string ConfigFileName = "config.xml";
        public const string InstallDirectoriesFileName = "installdirs.txt";
        public const string QuickEditTemplatesName = "templates";
        public const string ControlSetTemplatesName = "Control set templates";
        public const string DefaultControllerBackgroundUri = "pack://application:,,,/Keysticks;component/Images/Xbox360Controller.png";
        
        // General IDs and limits
        public const int LastUsedID = -4;
        public const int NextID = -3;
        public const int PreviousID = -2;
        public const int DefaultID = -1;
        public const int NoneID = 0;
        public const int ID1 = 1;
        public const int ID2 = 2;
        public const int ID3 = 3;
        public const int ID4 = 4;
        public const int Index0 = 0;
        public const int Index1 = 1;
        public const int Index2 = 2;
        public const int Index3 = 3;
        public const int MaxPlayers = 4;
        public const int MaxInputs = 4;
        public const int MaxControls = 255;
        public const int LoggingLevelNone = 0;
        public const int LoggingLevelErrors = 1;
        public const int LoggingLevelInfo = 2;
        public const int LoggingLevelDebug = 3;

        // Encryption
        public const string ProfileSeed = "ktszRokpVh2vLbErTlgNVSSmE0jdLc9dlhfLMYCrYj4ESDtLJSV1obHkURWdal/pUPw=";
        public const string PasswordSeed = "eBHau6P6rJXthhaSU9Rm0ZQ7lkOTVqaCQa2Sx1/i1osFD4NsWesqCT1uIUpx5yZwnA8=";

        // Keyboard cell names - these should match EKeyboardKey
        public static readonly string[] KeyboardCellNames = new string[]
        {
            "None",
            "Escape", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "Insert", "Print screen", "Delete",
            "Backtick", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "Minus sign", "Equals sign", "Backspace", "Home", 
            "Tab", "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "Left bracket", "Right bracket", "Hash", "Page up",
            "Caps lock", "A", "S", "D", "F", "G", "H", "J", "K", "L", "Semi-colon", "Apostrophe", "Return", "Page down",
            "Shift", "Backslash", "Z", "X", "C", "V", "B", "N", "M", "Comma", "Full stop", "Slash", "Right Shift", "Up", "End",
            "Ctrl", "Windows", "Alt", "Spacebar", "Right Alt", "Right Windows", "Apps", "Right Ctrl", "Left", "Down", "Right"
        };

        // Action strip grid (Cells named A0 - A15)
        public const int ActionStripMaxCells = 16;
        public const int ActionStripDefaultNumCells = 10;

        // Square grid
        public const int TopLeftCellID = 100;
        public const int TopCentreCellID = 101;
        public const int TopRightCellID = 102;
        public const int CentreLeftCellID = 103;
        public const int CentreCellID = 104;
        public const int CentreRightCellID = 105;
        public const int BottomLeftCellID = 106;
        public const int BottomCentreCellID = 107;
        public const int BottomRightCellID = 108;
        public static readonly int[] SquareGridCellIDs = new int[] { TopLeftCellID, TopCentreCellID, TopRightCellID,
                                                                    CentreLeftCellID, CentreCellID, CentreRightCellID,
                                                                    BottomLeftCellID, BottomCentreCellID, BottomRightCellID};
        public static readonly string[] SquareGridPanelNames = new string[] { "TopLeftGrid", "TopCentreGrid", "TopRightGrid",
                                                                    "CentreLeftGrid", "CentreGrid", "CentreRightGrid",
                                                                    "BottomLeftGrid", "BottomCentreGrid", "BottomRightGrid"};
        public static readonly ELRUDState[] SquareGridDirections = new ELRUDState[] { ELRUDState.UpLeft, ELRUDState.Up, ELRUDState.UpRight,
                                                                                    ELRUDState.Left, ELRUDState.Centre, ELRUDState.Right, 
                                                                                    ELRUDState.DownLeft, ELRUDState.Down, ELRUDState.DownRight};

        // App config settings
        public const string ConfigUserCurrentProfileDirectory = "user_current_profile_directory";
        public const string ConfigUserLastUpdateCheckDate = "user_last_update_check_date";
        public const string ConfigUserLastUsedProfile = "user_last_profile";
        public const string ConfigUserRecentProfilesList = "user_recent_profiles_list";
        public const string ConfigAutoConfirmApplyTemplates = "auto_confirm_apply_templates";
        public const string ConfigAutoConfirmClearActions = "auto_confirm_clear_actions";
        public const string ConfigAutoConfirmDeleteKeyboard = "auto_confirm_delete_keyboard";
        public const string ConfigAutoConfirmDeleteControlSet = "auto_confirm_delete_control_set";
        public const string ConfigAutoConfirmDeletePage = "auto_confirm_delete_page";
        public const string ConfigAutoConfirmDeleteVirtualControl = "auto_confirm_delete_virtual_control";
        public const string ConfigAutoConfirmDeletePlayer = "auto_confirm_delete_player";
        //public const string ConfigAutoConfirmActionListLengthChange = "auto_confirm_action_list_length_change";
        public const string ConfigStartWhenWindowsStarts = "start_when_windows_starts";
        //public const string ConfigLastNewsUpdateTime = "last_news_update_time";
        //public const string ConfigProgramUpdateAvailableVersion = "program_update_available_version";
        //public const string ConfigProgramUpdateShowEULA = "program_update_show_eula";
        //public const string ConfigProgramUpdateReleaseNotes = "program_update_release_notes";
        public const string ConfigProfilesListLastUpdated = "profiles_list_last_updated";
        public const string ConfigLastProgramUpdateID = "last_program_update_id";
        public const string ConfigProgramUpdateFileSize = "program_update_file_size";
        public const string ConfigAutoLoadLastProfile = "auto_load_last_profile";
        public const string ConfigShowControlsWindow = "show_controls_window";
        public const string ConfigKeepControlsWindowOnTop = "keep_controls_window_on_top";
        public const string ConfigAllowBackgroundRunning = "allow_background_running";
        public const string ConfigMessageLoggingLevel = "logging_level";
        public const string OldConfigShowHeldKeysAndMouseButtons = "show_held_keys_and_mouse_buttons";
        public const string ConfigShowHeldModifierKeys = "show_held_modifier_keys";
        public const string ConfigShowHeldMouseButtons = "show_held_mouse_buttons";
        public const string ConfigAnimateControls = "animate_controls";
        public const string ConfigShowTrayNotifications = "show_tray_notifications";
        public const string ConfigCheckForProgramUpdates = "check_for_program_updates";
        public const string ConfigExpandPagesInSituationTree = "expand_pages_in_situation_tree";
        public const string ConfigCellColour = "cell_colour";
        public const string ConfigAlternateCellColour = "alternate_cell_colour";
        public const string ConfigHighlightColour = "highlight_colour";
        public const string ConfigSelectionColour = "selection_colour";
        public const string ConfigAnimationColour = "animation_colour";
        public const string OldConfigWindowOpacityPercent = "window_opacity_percent";
        public const string ConfigCurrentControlsOpacityPercent = "current_controls_opacity_percent";
        public const string ConfigInteractiveControlsOpacityPercent = "interactive_controls_opacity_percent";
        public const string ConfigZoomFactorPercent = "zoom_factor_percent";
        public const string ConfigCompactControlsWindow = "compact_controls_window";
        public const string ConfigDefaultControllerBackgroundType = "default_controller_background_type";
        public const string ConfigDefaultControllerBackgroundColour = "default_controller_background_colour";
        public const string ConfigDefaultControllerBackgroundUri = "default_controller_background_uri";
        public const string ConfigShowInteractiveControlsTitleBar = "show_interactive_controls_title_bar";
        public const string ConfigShowInteractiveControlsFooter = "show_interactive_controls_footer";
        public const string ConfigMousePointerSpeed = "mouse_pointer_speed";
        public const string ConfigMousePointerAcceleration = "mouse_pointer_acceleration";        
        public const string ConfigUseDefaultSensitivity = "use_default_sensitivity";
        public const string ConfigTriggerDeadZoneFraction = "trigger_dead_zone_fraction";
        public const string ConfigThumbstickDeadZoneFraction = "thumbstick_dead_zone_fraction";
        //public const string ConfigHoldTimeMS = "default_hold_time_ms";
        //public const string ConfigAutoRepeatIntervalMS = "default_auto_repeat_interval_ms";
        public const string ConfigEnableWordPrediction = "word_prediction_enabled";
        public const string ConfigAutoInsertSpaces = "word_prediction_auto_insert_spaces";
        public const string ConfigLearnNewWords = "word_prediction_learn_new_words";
        public const string ConfigWordPredictionInstalledLanguages = "word_prediction_installed_languages";
        public const string OldConfigWordPredictionServerPort = "word_prediction_server_port";
        public const string ConfigInputPollingIntervalMS = "input_polling_interval_ms";
        public const string ConfigUIPollingIntervalMS = "ui_polling_interval_ms";
        public const string OldConfigAutoDetectWhichController = "auto_detect_which_controller";
        public const string OldConfigSpecificControllerNumber = "specific_controller_number";
        public const string ConfigKeyStrokeLengthMS = "key_stroke_length_ms";
        public const string OldConfigUseDirectXKeyPresses = "use_directx_key_strokes";
        public const string ConfigUseScanCodes = "use_scan_codes";
        public const string ConfigMouseClickLengthMS = "mouse_click_length_ms";
        public const string ConfigMaxActionListLength = "max_action_list_length";
        public const string ConfigDisallowShiftDelete = "disallow_shift_delete";
        public const string ConfigDisallowDangerousControls = "disallow_dangerous_controls";
        public const string ConfigAllowedCommandsList = "allowed_commands_list";
        public const string ConfigDisallowedCommandsList = "disallowed_commands_list";
        public const string ConfigAskMeCommandsList = "ask_me_commands_list";
        public const string ConfigDownloadAreaEnabled = "download_area_enabled";
        public const string ConfigDownloadAreaRememberMe = "download_area_remember_me";
        public const string OldConfigDownloadAreaUsername = "download_area_username";
        public const string ConfigDownloadAreaUsernameSec = "download_area_username_sec";
        public const string OldConfigDownloadAreaPasswordEnc = "download_area_password_enc";
        public const string ConfigDownloadAreaPasswordSec = "download_area_password_sec";
        public const string ConfigDownloadAreaSeedHint = "download_area_seed_hint";

        // Default config values
        public const bool DefaultAutoConfirmApplyTemplates = false;
        public const bool DefaultAutoConfirmClearActions = false;
        public const bool DefaultAutoConfirmDeleteKeyboard = false;
        public const bool DefaultAutoConfirmDeleteControlSet = false;
        public const bool DefaultAutoConfirmDeletePage = false;
        //public const bool DefaultAutoConfirmActionListLengthChange = false;
        public const bool DefaultAutoConfirmDeleteVirtualControl = false;
        public const bool DefaultAutoConfirmDeletePlayer = false;
        public const bool DefaultStartWhenWindowsStarts = false;
        public const bool DefaultAutoLoadLastProfile = true;
        public const bool DefaultShowControlsWindow = true;
        public const bool DefaultKeepControlsWindowOnTop = true;
        public const bool DefaultAllowBackgroundRunning = true;
        public const int DefaultMessageLoggingLevel = Constants.LoggingLevelErrors;
        public const bool DefaultShowHeldModifierKeys = true;
        public const bool DefaultShowHeldMouseButtons = true;
        public const bool DefaultAnimateControls = true;
        public const bool DefaultShowTrayNotifications = true;
        public const bool DefaultCheckForProgramUpdates = true;
        public const bool DefaultExpandPagesInSituationTree = false;
        public static Color DefaultCellColour = Colors.LightSkyBlue;
        public static Color DefaultAlternateCellColour = Colors.DodgerBlue;
        public static Color DefaultHighlightColour = Colors.Yellow;
        public static Color DefaultSelectionColour = Colors.LightSalmon;
        public static PlayerColourScheme DefaultPlayer1Colours = new PlayerColourScheme("LightSkyBlue", "DodgerBlue", "Yellow", "LightSalmon");
        public static PlayerColourScheme DefaultPlayer2Colours = new PlayerColourScheme("LightGreen", "MediumSeaGreen", "Yellow", "LightSalmon");
        public static PlayerColourScheme DefaultPlayer3Colours = new PlayerColourScheme("Gold", "Orange", "PaleGreen", "LightSalmon");
        public static PlayerColourScheme DefaultPlayer4Colours = new PlayerColourScheme("Pink", "Orchid", "PaleGreen", "LightSalmon");
        public const string DefaultControllerBackgroundColourName = "LightGray";
        public const int DefaultWindowOpacityPercent = 90;
        public const int DefaultZoomFactorPercent = 100;
        public const bool DefaultCompactControlsWindow = true;
        public const bool DefaultShowInteractiveControlsTitleBar = true;
        public const bool DefaultShowInteractiveControlsFooter = true;
        public const float DefaultMousePointerSpeed = 1.0f;
        public const float DefaultMousePointerAcceleration = 10.0f;
        public const int DefaultMousePointerCircleSizePercent = 10;
        public const bool DefaultUseDefaultSensitivity = true;
        public const float DefaultTriggerDeadZoneFraction = 0.1f;
        public const float DefaultStickDeadZoneFraction = 0.25f;
        public const int DefaultHoldTimeMS = 500;
        public const int DefaultAutoRepeatIntervalMS = 330;
        public const bool DefaultEnableWordPrediction = true;
        public const bool DefaultAutoInsertSpaces = true;
        public const bool DefaultLearnNewWords = false;
        public const string DefaultWordPredictionInstalledLanguages = "enggb,True";
        public const int MinInputPollingIntervalMS = 10;
        public const int DefaultUIPollingIntervalMS = 50;
        public const int DefaultInputPollingIntervalMS = 20;
        public const int DefaultWordPredictionPollingIntervalMS = 250;
        public const int DefaultSystemPollingIntervalMS = 500;
        public const int WaitForExitMS = 1500;
        //public const bool DefaultAutoDetectWhichController = true;
        public const int DefaultSpecificControllerNumber = 1;
        public const int DefaultKeyStrokeLengthMS = 100;
        public const int DefaultMouseClickLengthMS = 100;
        public const int DefaultMaxActionListLength = 3;
        public const bool DefaultUseScanCodes = true;
        public const bool DefaultDownloadAreaEnabled = false;
        public const bool DefaultDownloadAreaRememberMe = true;
        public const bool DefaultDisallowShiftDelete = true;
        public const bool DefaultDisallowDangerousControls = true;

        // Other defaults
        public const EDirectionMode DefaultDirectionMode = EDirectionMode.EightWay;
        public const int DefaultHoldStateDelayMS = 400;
        public const int MaxRecentProfiles = 4;
        public const int TitleBarAnimationDuration = 2000;
        public const int DefaultControllerWidth = 400;
        public const int DefaultControllerHeight = 300;
        public const int DefaultAnnotationWidth = 35;
        public const int DefaultAnnotationHeight = 21;
        public const int DefaultControllerTitleBarX = 85;
        public const int DefaultControllerTitleBarY = 22;
        public const int DefaultControllerTitleBarWidth = 232;
        public const int DefaultControllerTitleBarHeight = 25;
        public const int DefaultControllerMenuBarX = 142;
        public const int DefaultControllerMenuBarY = 49;
        public const int DefaultControllerMenuBarWidth = 125;
        public const int DefaultControllerMenuBarHeight = 25;
        public const int DefaultCompactControllerPadding = 5;

        // Word prediction constants
        public const ELanguageCode DefaultInstalledPredictionLanguage = ELanguageCode.enggb;
        public const int REQUEST_RESET_INPUT = 10;
        public const int REQUEST_INSERT_STRING = 11;
        public const int REQUEST_MOVE_CURSOR = 12;
        public const int REQUEST_REMOVE_CHARS = 13;
        public const int REQUEST_INSERT_SUGGESTION = 14;
        public const int REQUEST_CONFIGURE_LEARNING = 15;
        public const int REQUEST_SET_CURSOR = 16;
        public const int REQUEST_GET_SUGGESTIONS = 17;
        public const int REQUEST_INSTALL_PACKAGES = 18;
        public const int REQUEST_UNINSTALL_PACKAGES = 19;
        public const int REQUEST_SET_ACTIVE_DICTIONARIES = 20;
        public const int RESPONSE_ERROR_UNRECOGNISED_MSG_TYPE = 200;
        public const int RESPONSE_ERROR_BUFFER_OVERFLOW = 201;
        public const int RESPONSE_ERROR_RESET = 210;
        public const int RESPONSE_ERROR_INSERT_STRING = 211;
        public const int RESPONSE_ERROR_MOVE_CURSOR = 212;
        public const int RESPONSE_ERROR_REMOVE_CHARS = 213;
        public const int RESPONSE_ERROR_INSERT_SUGGESTION = 214;
        public const int RESPONSE_ERROR_CONFIGURE_LEARNING = 215;
        public const int RESPONSE_ERROR_SET_CURSOR = 216;
        public const int RESPONSE_ERROR_GET_SUGGESTIONS = 217;
        public const int RESPONSE_ERROR_INSTALL_PACKAGES = 218;
        public const int RESPONSE_ERROR_UNINSTALL_PACKAGES = 219;
        public const int RESPONSE_ERROR_SET_ACTIVE_DICTIONARIES = 220;

        // UI settings
        public const int MaxTinyDescriptionLen = 16;

        // Regexes
        //public const string ValidProgramFilesLocationRegex = @"^['""]?[a-z]:[\\/]program files([a-z0-9_:\\/()'"" +-]+[.]?)+$";
        public const string ValidHttpURIRegex = @"^['""]?http[.a-z0-9_:/'"" %?=&!$@#+-]+$";
    }
}
