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
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Keysticks.Actions;
using Keysticks.Core;
using Keysticks.Config;
using Keysticks.Event;
using Keysticks.Sources;
using Keysticks.Sys;
using Keysticks.UI;
using Keysticks.UserControls;
using Keysticks.WebService;

namespace Keysticks
{
    /// <summary>
    /// Main window (invisible)
    /// </summary>
    public partial class MainWindow : Window, IMainWindow
    {
        // Threading
        private double _uiPollingIntervalMS = Constants.DefaultUIPollingIntervalMS;
        private DispatcherTimer _timer;
        private ThreadManager _manager = new ThreadManager();
        private MessageLogger _messageLogger = new MessageLogger();
        private WebServiceUtils _wsUtils;

        // Inter-process comms
        private HwndSource _hwndSource;
        private HwndSourceHook _hwndSourceHook;
        private string _interProcessMessage;

        // Configuration
        private string _startUpProfile;
        private AppConfig _appConfig;
        private Profile _profile;

        // State
        private bool _isExiting = false;
        private IKeyboardContext _outputContext;
        private StartProgramAction _pendingStartProgramAction;

        // Windows
        private TrayManager _trayManager;
        private HelpAboutWindow _helpAboutWindow;
        private HoldStateWindow _holdStateWindow;
        private Dictionary<int, ControllerWindow> _controllerWindows = new Dictionary<int, ControllerWindow>();
        private Dictionary<int, GridsWindow> _keyboardWindows = new Dictionary<int,GridsWindow>();
        private CustomMessageBox _confirmCommandMessageBox;
        private CustomMessageBox _commandBlockedMessageBox;

        // Utils
        private StringUtils _utils = new StringUtils();

        // Properties
        public IThreadManager ThreadManager { get { return _manager; } }
        public IKeyboardContext OutputContext { get { return _outputContext; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            // Get the command line options
            string[] cmdArgs = Environment.GetCommandLineArgs();
            for (int i = 1; i < cmdArgs.Length; i++)
            {
                string arg = cmdArgs[i];
                if (arg.StartsWith("/") || arg.StartsWith("-"))
                {
                    // Program option
                    string option = arg.Substring(1).ToLower();
                    if (option == "kxadmin")
                    {
                        AppConfig.IsAdminMode = true;
                    }
                    else if (option == "kxlocal")
                    {
                        AppConfig.IsLocalMode = true;
                    }
                    else if (option == "kxcatchall")
                    {
                        AppConfig.IsCatchAll = true;
                    }
                }
                else
                {
                    // Profile file to load
                    _startUpProfile = arg;
                }
            }

            Keysticks.App app = (Keysticks.App)App.Current;
            if (!app.IsFirstInstance)
            {
                // If another instance is already running, send it the profile file path to load
                SendStringToRunningInstance(_startUpProfile);

                this.Close();
                return;
            }

            InitializeComponent();

            // Register global error handler if required
            if (AppConfig.IsCatchAll)
            {
                System.AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledExcept‌​ion);
            }

            // Set this instance as the error logger
            ErrorMessageControl.ErrorHandler = this;

            // Create timer
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(_uiPollingIntervalMS);
            _timer.Tick += UpdateGUI;
        }

        /// <summary>
        /// Send a string to another running instance of the program
        /// </summary>
        /// <param name="message"></param>
        private void SendStringToRunningInstance(string message)
        {
            try
            {
                // Get the handle of the other instance's main window
                _interProcessMessage = message != null ? message : "";
                Process currentProcess = Process.GetCurrentProcess();
                if (currentProcess != null)
                {
                    Process[] appInstances = Process.GetProcessesByName(Constants.ProductName);
                    if (appInstances != null)
                    {
                        CallBackPtr callBack = new CallBackPtr(this.EnumRunningInstanceWindowsCallback);
                        foreach (Process process in appInstances)
                        {
                            if (process.Id != currentProcess.Id)
                            {
                                foreach (ProcessThread thread in process.Threads)
                                {
                                    WindowsAPI.EnumThreadWindows((uint)thread.Id, callBack, IntPtr.Zero);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        /// <summary>
        /// Callback for EnumThreadWindows call
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private bool EnumRunningInstanceWindowsCallback(IntPtr hWnd, IntPtr lParam)
        {
            bool continueEnumerating = true;
            try
            {
                // See if it's the main window
                string title = WindowsAPI.GetTitleBarText(hWnd);
                if (!string.IsNullOrEmpty(title) && title == Constants.ProductName)
                {
                    // Convert to byte array
                    byte[] bytes = Encoding.UTF8.GetBytes(_interProcessMessage);

                    // Allocate a memory address for our byte array
                    IntPtr ptrData = Marshal.AllocCoTaskMem(bytes.Length);
                    Marshal.Copy(bytes, 0, ptrData, bytes.Length);

                    // Create Windows msg
                    WindowsAPI.COPYDATASTRUCT cds;
                    cds.dwData = IntPtr.Zero;
                    cds.cbData = bytes.Length;
                    cds.lpData = ptrData;
                    IntPtr iPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(cds));
                    Marshal.StructureToPtr(cds, iPtr, true);

                    // Send
                    WindowsAPI.SendMessage(hWnd, WindowsAPI.WM_COPYDATA, IntPtr.Zero, iPtr);

                    // Free memory
                    Marshal.FreeCoTaskMem(ptrData);
                    Marshal.FreeCoTaskMem(iPtr);

                    continueEnumerating = false;
                }
            }
            catch (Exception)
            {
                // Ignore errors
            }

            return continueEnumerating;
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
                this.Title = Constants.ProductName; // Don't change, used by software updater
                this.Visibility = System.Windows.Visibility.Hidden;

                // Load the app config
                _appConfig = new AppConfig();
                _appConfig.Load();

                // Get the current keyboard layout
                IntPtr dwhkl = WindowsAPI.GetKeyboardLayout(0);
                KeyUtils.InitialiseKeyboardLayout(dwhkl);
                KxKeyboardChangeEventArgs inputContext = new KxKeyboardChangeEventArgs(dwhkl);
                _outputContext = new KxKeyboardChangeEventArgs(dwhkl);

                // Create tray icon
                _trayManager = new TrayManager(this, _appConfig, inputContext);

                // Create web service manager
                _wsUtils = new WebServiceUtils();
                _wsUtils.OnResponse += new WebServiceEventHandler(OnWebServiceResponse);
                
                // Deploy the sample profiles to the user's profiles folder if not already there
                CopySampleProfiles();

                // Start updating the GUI
                _timer.IsEnabled = true;

                // Configure the thread manager
                _manager.SetAppConfig(_appConfig);

                // Register Windows message handler
                _hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
                _hwndSourceHook = new HwndSourceHook(WndProc);
                _hwndSource.AddHook(_hwndSourceHook);

                // Create the modifiers window
                _holdStateWindow = new HoldStateWindow(this);
                _holdStateWindow.SetAppConfig(_appConfig);

                // Load start up profile, or if not specified, the last used profile (if any), or sample controls
                _trayManager.LoadProfile(_startUpProfile, false, false);
                
                // Check for program updates occasionally if enabled
                if (_appConfig.GetBoolVal(Constants.ConfigCheckForProgramUpdates, Constants.DefaultCheckForProgramUpdates))
                {
                    DateTime timeNow = DateTime.UtcNow;
                    DateTime lastUpdateCheck = _appConfig.GetDateVal(Constants.ConfigUserLastUpdateCheckDate, DateTime.MinValue);
                    double daysSinceLastCheck = (timeNow - lastUpdateCheck).TotalDays;
                    if (daysSinceLastCheck > 1.0)
                    {
                        // Download any program updates
                        string lastProgramUpdateID = _appConfig.GetStringVal(Constants.ConfigLastProgramUpdateID, "0");
                        //string lastProgramUpdateVersion = _appConfig.GetStringVal(Constants.ConfigProgramUpdateAvailableVersion, Constants.AppVersionString);
                        //bool downloadUpdates = _appConfig.GetBoolVal(Constants.ConfigDownloadUpdatesAutomatically, Constants.DefaultDownloadUpdatesAutomatically);
                        //if (downloadUpdates)
                        //{
                        //    string msiFilePath = Path.Combine(AppConfig.CommonAppDataDir, Constants.AppUpdateMSIFilename);
                        //    if (!File.Exists(msiFilePath))
                        //    {
                        //        lastProgramUpdateVersion = Constants.AppVersionString;
                        //    }
                        //}

                        WebServiceMessageData request = new WebServiceMessageData(EMessageType.GetProgramUpdates);
                        request.SetMetaVal(EMetaDataItem.ID.ToString(), lastProgramUpdateID);
                        request.SetMetaVal(EMetaDataItem.AppVersion.ToString(), Constants.AppVersionString);
                        _wsUtils.StartWebServiceRequest(request);

                        // Store date of last update check
                        _appConfig.SetDateVal(Constants.ConfigUserLastUpdateCheckDate, timeNow);
                    }
                }                
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_InitApplication, ex);
            }
        }

        /// <summary>
        /// Show the About window
        /// </summary>
        public void ShowHelpAboutWindow()
        {
            if (_helpAboutWindow == null)
            {
                _helpAboutWindow = new HelpAboutWindow(this);
                _helpAboutWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                _helpAboutWindow.Show();
            }
            else if (_helpAboutWindow.IsLoaded)
            {
                _helpAboutWindow.Activate();
            }
        }

        /// <summary>
        /// Receive web service response
        /// </summary>
        /// <param name="userLicence"></param>
        private void OnWebServiceResponse(WebServiceMessageData responseData)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(new WebServiceEventHandler(HandleWebServiceResponse), responseData);
            }
            else
            {
                HandleWebServiceResponse(responseData);
            }
        }

        /// <summary>
        /// Handle web service response
        /// </summary>
        /// <param name="responseData"></param>
        private void HandleWebServiceResponse(WebServiceMessageData responseData)
        {
            if (responseData != null)
            {
                if (responseData.MessageType == EMessageType.ProgramUpdates)
                {
                    HandleDownloadedProgramUpdates(responseData);
                }
                else if (responseData.MessageType == EMessageType.WordPredictionLanguagePack)
                {
                    HandleDownloadedPredictionPackage(responseData);
                }
                else if (responseData.MessageType == EMessageType.WebServiceError)
                {
                    // Ignore
                }
            }
        }        

        /// <summary>
        /// Process downloaded program update
        /// </summary>
        private void HandleDownloadedProgramUpdates(WebServiceMessageData responseData)
        {
            try
            {
                int lastUpdateID = int.Parse(responseData.GetMetaVal(EMetaDataItem.ID.ToString(), "0"));
                if (lastUpdateID > 0)
                {
                    // Store the version we downloaded and whether it requires licence acceptance
                    _appConfig.SetStringVal(Constants.ConfigLastProgramUpdateID, lastUpdateID.ToString());

                    string title = responseData.GetMetaVal(EMetaDataItem.Title.ToString());
                    string description = responseData.GetMetaVal(EMetaDataItem.Description.ToString());
                    string url = responseData.GetMetaVal(EMetaDataItem.Url.ToString());

                    //if (!string.IsNullOrEmpty(responseData.Content))
                    //{
                    //    // Save the msi file
                    //    string filePath = Path.Combine(AppConfig.CommonAppDataDir, Constants.AppUpdateMSIFilename);
                    //    byte[] msiData = Convert.FromBase64String(responseData.Content);
                    //    File.WriteAllBytes(filePath, msiData);

                    //    // Store the file size
                    //    FileInfo fi = new FileInfo(filePath);
                    //    _appConfig.SetIntVal(Constants.ConfigProgramUpdateFileSize, (int)fi.Length);

                    //    Trace.WriteLine("Downloaded MSI");
                    //}

                    // Offer to upgrade
                    _trayManager.ShowProgramUpdate(title, description, url);
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_ReceiveUpdates, ex);
            }            
        }

        /// <summary>
        /// Process downloaded prediction package file
        /// </summary>
        /// <param name="responseData"></param>
        private void HandleDownloadedPredictionPackage(WebServiceMessageData responseData)
        {
            try
            {
                string fileName = responseData.GetMetaVal(EMetaDataItem.Name.ToString());
                int index = int.Parse(responseData.GetMetaVal(EMetaDataItem.Index.ToString(), "0"));
                int count = int.Parse(responseData.GetMetaVal(EMetaDataItem.Count.ToString(), "0"));
                if (fileName != "")
                {
                    // Save the package file
                    string filePath = Path.Combine(AppConfig.CommonAppDataDir, "base", "Packages", fileName);
                    byte[] fileData = Convert.FromBase64String(responseData.Content);
                    File.WriteAllBytes(filePath, fileData);

                    // If it's the last package being downloaded, tell the prediction engine to install the new packages
                    if (index >= count - 1)
                    {
                        _manager.SubmitPredictionEvent(new KxGenericEventArgs(EEventType.LanguagePackages));
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_ReceiveLanguageData, ex);
            }        
        }

        /// <summary>
        /// Deploy the sample profiles to the user's default Profiles folder when it's a newer app version
        /// </summary>
        private void CopySampleProfiles()
        {
            try
            {
                string sourceProfilesDir = Path.Combine(AppConfig.CommonAppDataDir, "Profiles");
                string destProfilesDir = Path.Combine(AppConfig.LocalAppDataDir, "Profiles");

                // Check we're not running in dev env
                if (!sourceProfilesDir.Equals(destProfilesDir))
                {
                    // Read list of profile directories
                    Dictionary<string, string> userProfileDirs = new Dictionary<string, string>();
                    string installLogFilePath = Path.Combine(AppConfig.CommonAppDataDir, Constants.InstallDirectoriesFileName);
                    if (File.Exists(installLogFilePath))
                    {
                        string[] lines = File.ReadAllLines(installLogFilePath, Encoding.UTF8);
                        foreach (string line in lines)
                        {
                            string[] tokens = line.Split(',');
                            if (tokens.Length > 0)
                            {
                                string dir = tokens[0];
                                string deployVersion = tokens.Length > 1 ? tokens[1] : "1";
                                userProfileDirs[dir] = deployVersion;
                            }
                        }
                    }

                    // See if we have deployed to this directory or not for this program version
                    if (!userProfileDirs.ContainsKey(destProfilesDir) || userProfileDirs[destProfilesDir] != Constants.AppVersionString)
                    {
                        // Record that we deployed to this directory (or tried to) in the install dirs file
                        userProfileDirs[destProfilesDir] = Constants.AppVersionString;
                        StringBuilder sb = new StringBuilder();
                        Dictionary<string, string>.Enumerator eDir = userProfileDirs.GetEnumerator();
                        while (eDir.MoveNext())
                        {
                            sb.Append(eDir.Current.Key);
                            sb.Append(',');
                            sb.AppendLine(eDir.Current.Value);
                        }
                        File.WriteAllText(installLogFilePath, sb.ToString(), Encoding.UTF8);

                        // Create the profiles dir if it doesn't exist
                        DirectoryInfo destDirInfo = new DirectoryInfo(destProfilesDir);
                        if (!destDirInfo.Exists)
                        {
                            destDirInfo.Create();
                        }

                        DirectoryInfo sourceDirInfo = new DirectoryInfo(sourceProfilesDir);
                        if (sourceDirInfo.Exists)
                        {
                            FileInfo[] sourceFiles = sourceDirInfo.GetFiles("*" + Constants.ProfileFileExtension);

                            // Deploy missing or newer sample profiles
                            foreach (FileInfo sourceFile in sourceFiles)
                            {
                                string destFilePath = Path.Combine(destProfilesDir, sourceFile.Name);
                                FileInfo destFile = new FileInfo(destFilePath);
                                if (!destFile.Exists || sourceFile.LastWriteTime > destFile.LastWriteTime)
                                {
                                    File.Copy(sourceFile.FullName, destFilePath, true);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_CopySampleProfiles, ex);
            }
        }

        /// <summary>
        /// Handle certain Windows messages
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WindowsAPI.WM_COPYDATA)
            {
                try
                {
                    if (_trayManager != null)
                    {
                        // msg.LParam contains a pointer to the COPYDATASTRUCT struct
                        WindowsAPI.COPYDATASTRUCT cds =
                            (WindowsAPI.COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(WindowsAPI.COPYDATASTRUCT));

                        // Check data received is sensible
                        if (cds.cbData < 10000 && cds.lpData != IntPtr.Zero)
                        {
                            // Create a byte array to hold the data
                            byte[] bytes = new byte[cds.cbData];

                            // Make a copy of the original data referenced by the COPYDATASTRUCT struct
                            Marshal.Copy(cds.lpData, bytes, 0, cds.cbData);

                            // Deserialize the data back into a string
                            string filePath = Encoding.UTF8.GetString(bytes);
                            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                            {
                                // Load the profile file provided
                                if (!_trayManager.ProfileEdited)
                                {
                                    _trayManager.LoadProfile(filePath, false, false);
                                }
                                else
                                {
                                    HandleError(Properties.Resources.E_ChangeProfileUnsavedChanges, null);
                                }
                            }
                            else
                            {
                                // Show a message to indicate already running
                                string message = string.Format(Properties.Resources.E_ProgramAlreadyRunning, Constants.ProductName);
                                string caption = "";
                                if (_profile != null)
                                {
                                    caption = string.Format(Properties.Resources.String_Profile + ": {0}", _profile.Name);
                                }
                                _trayManager.ShowTemporaryMessage(message, caption, System.Windows.Forms.ToolTipIcon.Info);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore
                }

                handled = true;
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Window closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _isExiting = true;
                CloseChildWindows();
                if (_timer != null)
                {
                    _timer.IsEnabled = false;
                }
                StopMonitoringSources();
                if (_appConfig != null && _appConfig.IsModified)
                {
                    // Save config changes e.g. window positions
                    _appConfig.Save();
                }
                if (_hwndSource != null && _hwndSourceHook != null)
                {
                    _hwndSource.RemoveHook(_hwndSourceHook);
                }
                if (_trayManager != null)
                {
                    _trayManager.Dispose();
                    _trayManager = null;
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_ExitProgram, ex);
            }
        }
       
        /// <summary>
        /// Close any child windows
        /// </summary>
        private void CloseChildWindows()
        {
            if (_holdStateWindow != null)
            {
                _holdStateWindow.Close();
            }

            if (_helpAboutWindow != null)
            {
                _helpAboutWindow.Close();
            }

            for (int id = Constants.ID1; id <= Constants.ID4; id++)
            {
                Window window = GetControllerWindow(id);
                if (window != null)
                {
                    window.Close();
                }
                GridsWindow keyboard = GetKeyboardWindow(id);
                if (keyboard != null)
                {
                    keyboard.Close();
                }
            }

            if (_confirmCommandMessageBox != null)
            {
                _confirmCommandMessageBox.Close();
            }

            if (_commandBlockedMessageBox != null)
            {
                _commandBlockedMessageBox.Close();
            }
        }

        /// <summary>
        /// Get the tray manager
        /// </summary>
        /// <returns></returns>
        public ITrayManager GetTrayManager()
        {
            return _trayManager;
        }

        /// <summary>
        /// Get the message logger
        /// </summary>
        /// <returns></returns>
        public MessageLogger GetMessageLogger()
        {
            return _messageLogger;
        }

        /// <summary>
        /// Get the current profile
        /// </summary>
        /// <returns></returns>
        public Profile GetProfile()
        {
            return _profile;
        }

        /// <summary>
        /// Apply a new profile, either after loading from file, or after editing
        /// </summary>
        /// <param name="profile"></param>
        public bool ApplyProfile(Profile profile)
        {
            bool success = true;
            try
            {                
                // Store the profile
                _profile = profile;

                // Initialise the profile
                profile.Initialise(_outputContext);
                profile.SetAppConfig(_appConfig);

                // Validate
                profile.Validate();
                bool disallowDangerousControls = _appConfig.GetBoolVal(Constants.ConfigDisallowDangerousControls, Constants.DefaultDisallowDangerousControls);
                if (disallowDangerousControls)
                {
                    profile.ValidateSecurity();
                }

                // Close windows whose sources have been deleted
                for (int id = Constants.ID1; id <= Constants.ID4; id++)
                {
                    BaseSource source = profile.GetSource(id);
                    if (source == null)
                    {
                        ControllerWindow window = GetControllerWindow(id);
                        if (window != null)
                        {
                            window.Close();
                        }
                        GridsWindow keyboard = GetKeyboardWindow(id);
                        if (keyboard != null)
                        {
                            keyboard.Close();
                        }
                    }
                }

                // Create / update controller and keyboard windows
                foreach (BaseSource source in profile.VirtualSources)
                {
                    // Controller windows
                    ControllerWindow window = GetControllerWindow(source.ID);
                    if (window != null)
                    {
                        // Update window
                        window.SetSource(source);
                    }
                    else
                    {
                        // Create window for player
                        window = new ControllerWindow(this);
                        window.SetAppConfig(_appConfig);
                        window.SetSource(source);
                        _controllerWindows[source.ID] = window;

                        // Show the controls window on first creation if reqd
                        if (_appConfig.GetBoolVal(Constants.ConfigShowControlsWindow, Constants.DefaultShowControlsWindow))
                        {
                            ShowControllerWindow(source.ID);
                        }
                    }

                    // Keyboard windows
                    GridsWindow keyboard = GetKeyboardWindow(source.ID);
                    if (keyboard != null)
                    {
                        // Update window
                        keyboard.SetSource(source);
                    }
                    else
                    {
                        // Create window for player
                        keyboard = new GridsWindow(this);
                        keyboard.SetAppConfig(_appConfig);
                        keyboard.SetSource(source);
                        _keyboardWindows[source.ID] = keyboard;
                    }
                }

                // Configure threads
                _manager.SetProfile(_profile);                
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_ApplyProfile, ex);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Show the controller window for each player
        /// </summary>
        public void ShowControllerWindows()
        {
            if (_profile != null)
            {
                foreach (BaseSource source in _profile.VirtualSources)
                {
                    ShowControllerWindow(source.ID);
                }
            }            
        }

        /// <summary>
        /// Show the controls window
        /// </summary>
        public void ShowControllerWindow(int playerID)
        {
            ControllerWindow window = GetControllerWindow(playerID);
            if (window != null)
            {
                if (!window.IsLoaded)
                {
                    // Show window
                    window.Show();
                }
                else
                {
                    // Unhide and restore if necessary
                    window.Visibility = Visibility.Visible;
                    window.WindowState = System.Windows.WindowState.Normal;                    
                    
                    // Activate
                    if (!window.IsActive)
                    {
                        // Not minimised, but inactive
                        window.Activate();
                    }
                }
            }                                
        }

        /// <summary>
        /// Return the controller window for a player
        /// </summary>
        /// <returns></returns>
        public ControllerWindow GetControllerWindow(int playerID)
        {
            ControllerWindow window = null;
            if (_controllerWindows.ContainsKey(playerID))
            {
                window = _controllerWindows[playerID];
            }

            return window;
        }

        /// <summary>
        /// Return the controller window for a player
        /// </summary>
        /// <returns></returns>
        private GridsWindow GetKeyboardWindow(int playerID)
        {
            GridsWindow window = null;
            if (_keyboardWindows.ContainsKey(playerID))
            {
                window = _keyboardWindows[playerID];
            }

            return window;
        }

        /// <summary>
        /// Handle child window closing
        /// </summary>
        /// <param name="window"></param>
        public void ChildWindowClosing(Window window)
        {
            if (window == _holdStateWindow)
            {
                _holdStateWindow = null;
            }
            else if (window == _confirmCommandMessageBox)
            {
                if (!_isExiting)
                {
                    HandleStartProgramConfirmation(_confirmCommandMessageBox);
                }
                _confirmCommandMessageBox = null;
            }
            else if (window == _commandBlockedMessageBox)
            {
                _commandBlockedMessageBox = null;
            }
            else if (window is GridsWindow)
            {
                GridsWindow keyboardWindow = (GridsWindow)window;
                if (_keyboardWindows.ContainsKey(keyboardWindow.Source.ID))
                {
                    _keyboardWindows.Remove(keyboardWindow.Source.ID);
                }
            }
            else if (window is ControllerWindow)
            {
                ControllerWindow controllerWindow = (ControllerWindow)window;
                if (_controllerWindows.ContainsKey(controllerWindow.Source.ID))
                {
                    _controllerWindows.Remove(controllerWindow.Source.ID);
                }
            }
            else if (window == _helpAboutWindow)
            {
                _helpAboutWindow = null;                
            }

            ClearError();
        }

        /// <summary>
        /// Perform a pending Start Program action
        /// </summary>
        /// <param name="action"></param>
        private void HandleStartProgramConfirmation(CustomMessageBox messageBox)
        {
            string thisCommand = "";
            try
            {
                if (_pendingStartProgramAction != null)
                {
                    StartProgramAction action = _pendingStartProgramAction;
                    thisCommand = action.GetCommandLine();

                    // Handle "Don't ask again"
                    if (messageBox.DontAskAgain && messageBox.Result != MessageBoxResult.Cancel)
                    {
                        CommandRuleManager ruleManager = new CommandRuleManager();
                        ruleManager.FromConfig(_appConfig);
                        ECommandAction actionType = messageBox.Result == MessageBoxResult.Yes ? ECommandAction.Run : ECommandAction.DontRun;
                        CommandRuleItem rule = ruleManager.FindRule(thisCommand);
                        if (rule == null)
                        {
                            // Create new rule
                            rule = new CommandRuleItem(thisCommand, actionType);
                            ruleManager.Rules.Add(rule);
                        }
                        else
                        {
                            // Change rule's action type
                            rule.ActionType = actionType;
                        }
                        ruleManager.ToConfig(_appConfig);
                    }

                    if (messageBox.Result == MessageBoxResult.Yes)
                    {                        
                        // Start the program
                        if (!action.CheckIfRunning || !ProcessManager.IsRunning(action.ProgramName))
                        {
                            ProcessManager.Start(action);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_RunCommand + " " + thisCommand, ex);
            }
        }

        /// <summary>
        /// Apply a new app config
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            try
            {
                // Apply to active profile
                if (_profile != null)
                {
                    _profile.SetAppConfig(appConfig);
                }

                // Apply Start when Windows Starts option
                bool startWhenWindowsStarts = appConfig.GetBoolVal(Constants.ConfigStartWhenWindowsStarts, Constants.DefaultStartWhenWindowsStarts);
                if (startWhenWindowsStarts != _appConfig.GetBoolVal(Constants.ConfigStartWhenWindowsStarts, Constants.DefaultStartWhenWindowsStarts))
                {
                    // Option changed
                    ShortCutManager shortCutManager = new ShortCutManager();
                    try
                    {
                        if (startWhenWindowsStarts)
                        {
                            // Enable
                            shortCutManager.CreateStartupShortCut();
                        }
                        else
                        {
                            // Disable
                            shortCutManager.RemoveStartupShortCut();
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleError(ex.Message, ex.InnerException);
                    }
                }

                // Update message logging level
                _messageLogger.LoggingLevel = appConfig.GetIntVal(Constants.ConfigMessageLoggingLevel, Constants.DefaultMessageLoggingLevel);

                // Update polling interval
                _uiPollingIntervalMS = appConfig.GetIntVal(Constants.ConfigUIPollingIntervalMS, Constants.DefaultUIPollingIntervalMS);
                _timer.Interval = TimeSpan.FromMilliseconds(_uiPollingIntervalMS);

                // Add word prediction languages
                string installedLangList = appConfig.GetStringVal(Constants.ConfigWordPredictionInstalledLanguages, Constants.DefaultWordPredictionInstalledLanguages);
                if (installedLangList != _appConfig.GetStringVal(Constants.ConfigWordPredictionInstalledLanguages, Constants.DefaultWordPredictionInstalledLanguages))
                {
                    AddWordPredictionLanguages(installedLangList);
                }

                // Apply preferences to windows
                foreach (ControllerWindow window in _controllerWindows.Values)
                {
                    window.SetAppConfig(appConfig);
                }
                foreach (GridsWindow window in _keyboardWindows.Values)
                {
                    window.SetAppConfig(appConfig);
                }
                if (_holdStateWindow != null)
                {
                    _holdStateWindow.SetAppConfig(appConfig);
                }

                // Configure other threads
                _manager.SetAppConfig(appConfig);

                // Store config
                _appConfig = appConfig;
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_SetAppConfig, ex);
            }
        }

        /// <summary>
        /// Download any word prediction packages that the user has added
        /// </summary>
        /// <param name="installedLangList"></param>
        private void AddWordPredictionLanguages(string installedLangList)
        {
            NamedItemList langList = _utils.LanguageListFromString(installedLangList);
            string packagesFolderPath = Path.Combine(AppConfig.CommonAppDataDir, "base", "Packages");

            //bool packagesDeleted = false;
            // Delete any language packages that are no longer required
            //DirectoryInfo di = new DirectoryInfo(packagesFolderPath);
            //if (di.Exists)
            //{
            //    // Loop through package files
            //    FileInfo[] packageFiles = di.GetFiles("*" + Constants.PackageFileExtension);
            //    foreach (FileInfo packageFile in packageFiles)
            //    {
            //        string langCodeName = packageFile.Name.Replace(Constants.PackageFileExtension, "");
            //        ELanguageCode langCode;
            //        if (Enum.TryParse<ELanguageCode>(langCodeName, out langCode))
            //        {
            //            // See if the language is required
            //            if (langList.GetItemByID((int)langCode) == null)
            //            {
            //                // Remove package file
            //                packageFile.Delete();
            //                packagesDeleted = true;
            //            }
            //        }
            //    }
            //}

            // Determine whether any new languages are required
            List<string> newPackageFileNames = new List<string>();
            foreach (NamedItem langItem in langList)
            {
                // Don't download the default installed language
                ELanguageCode langCode = (ELanguageCode)langItem.ID;
                if (langCode != Constants.DefaultInstalledPredictionLanguage)
                {
                    string packageFileName = langCode.ToString() + Constants.PackageFileExtension;
                    string packageFilePath = Path.Combine(packagesFolderPath, packageFileName);
                    if (!File.Exists(packageFilePath))
                    {
                        // Need to download package
                        newPackageFileNames.Add(packageFileName);
                    }
                }
            }

            // Download any new packages required
            if (newPackageFileNames.Count != 0)
            {
                WebServiceMessageData[] requests = new WebServiceMessageData[newPackageFileNames.Count];
                for (int i = 0; i < requests.Length; i++)
                {
                    WebServiceMessageData request = new WebServiceMessageData(EMessageType.GetWordPredictionLanguagePack);
                    // Include index and count so that we'll know which response is the last one
                    request.SetMetaVal(EMetaDataItem.Name.ToString(), newPackageFileNames[i]);
                    request.SetMetaVal(EMetaDataItem.Index.ToString(), i.ToString());
                    request.SetMetaVal(EMetaDataItem.Count.ToString(), requests.Length.ToString());
                    requests[i] = request;
                }
                _wsUtils.StartWebServiceRequests(requests);
            }
            //else if (packagesDeleted)
            //{
            //    // If packages are being deleted but not added, update the prediction engine now
            //    _manager.SubmitPredictionEvent(new KxEventArgs(EEventType.LanguagePackages));
            //}
        }

        /// <summary>
        /// Stop monitoring input sources
        /// </summary>
        private void StopMonitoringSources()
        {
            // Stop the thread manager
            _manager.StopThreads();

            // Wait for thread manager to stop
            Thread.Sleep(Constants.WaitForExitMS);
        }

        /// <summary>
        /// Perform a GUI update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void UpdateGUI(Object sender, EventArgs args)
        {
            List<KxEventArgs> eventsToHandle = _manager.ReceiveUIEvents();
            if (eventsToHandle != null)
            {
                foreach (KxEventArgs report in eventsToHandle)
                {
                    HandleReportedEvent(report);
                }
            }
        }

        /// <summary>
        /// Handle a reported event 
        /// </summary>
        /// <param name="ev"></param>
        private void HandleReportedEvent(KxEventArgs args)
        {
            // Log event if required
            if (_messageLogger.LoggingLevel >= Constants.LoggingLevelInfo &&
                args.EventType != EEventType.LogMessage &&
                args.EventType != EEventType.ErrorMessage)    // Errors are logged separately
            {
                MessageItem item = new MessageItem();
                item.Time = DateTime.UtcNow;
                item.Type = _utils.EventTypeToString(args.EventType);
                item.Text = args.ToString();
                _messageLogger.LogMessage(item, false);
            }

            switch (args.EventType)
            {
                case EEventType.Source:
                    HandleControllerEvent((KxSourceEventArgs)args);
                    break;
                case EEventType.StateChange:
                    HandleStateChangeEvent((KxStateChangeEventArgs)args);
                    break;
                case EEventType.WordPrediction:
                    HandlePredictionEvent((KxPredictionEventArgs)args);
                    break;
                case EEventType.MouseButtonState:
                    HandleMouseButtonStateEvent((KxMouseButtonStateEventArgs)args);
                    break;
                case EEventType.KeyboardState:
                    HandleKeyboardStateEvent((KxKeyboardStateEventArgs)args);
                    break;
                case EEventType.LogMessage:
                    HandleLogMessageEvent((KxLogMessageEventArgs)args);
                    break;
                case EEventType.ErrorMessage:
                    {
                        KxErrorMessageEventArgs eArgs = (KxErrorMessageEventArgs)args;
                        HandleError(eArgs.Message, eArgs.Ex);
                    }
                    break;
                case EEventType.StartProgram:
                    HandleStartProgramEvent((KxStartProgramEventArgs)args);
                    break;
                case EEventType.LoadProfile:
                    HandleLoadProfileEvent((KxLoadProfileEventArgs)args);
                    break;
                case EEventType.KeyboardLayoutChange:
                    HandleKeyboardChangeEvent((KxKeyboardChangeEventArgs)args);
                    break;
                case EEventType.ToggleControls:
                    HandleToggleControlsEvent((KxToggleControlsEventArgs)args);
                    break;
            }
        }

        /// <summary>
        /// Handle a controller event
        /// </summary>
        /// <param name="args"></param>
        private void HandleControllerEvent(KxSourceEventArgs args)
        {
            ControllerWindow window = GetControllerWindow(args.SourceID);
            if (window != null)
            {
                window.HandleControllerEvent(args);
            }
            GridsWindow keyboard = GetKeyboardWindow(args.SourceID);
            if (keyboard != null)
            {
                keyboard.HandleControllerEvent(args);
            }
        }

        /// <summary>
        /// Handle a state change event
        /// </summary>
        /// <param name="args"></param>
        private void HandleStateChangeEvent(KxStateChangeEventArgs args)
        {
            ControllerWindow window = GetControllerWindow(args.PlayerID);
            if (window != null)
            {
                window.HandleStateChangeEvent(args);
            }
            GridsWindow keyboard = GetKeyboardWindow(args.PlayerID);
            if (keyboard == null)
            {
                BaseSource source = _profile.GetSource(args.PlayerID);
                if (source != null)
                {
                    // Create window for player
                    keyboard = new GridsWindow(this);
                    keyboard.SetAppConfig(_appConfig);
                    keyboard.SetSource(source);
                    _keyboardWindows[args.PlayerID] = keyboard;
                }
            }
            if (keyboard != null)
            {
                keyboard.HandleStateChangeEvent(args);
            }
        }

        /// <summary>
        /// Handle a word prediction event
        /// </summary>
        /// <param name="args"></param>
        private void HandlePredictionEvent(KxPredictionEventArgs args)
        {
            foreach (GridsWindow keyboard in _keyboardWindows.Values)
            {
                keyboard.HandlePredictionEvent(args);                
            }
        }

        /// <summary>
        /// Handle a mouse button state event
        /// </summary>
        /// <param name="args"></param>
        private void HandleMouseButtonStateEvent(KxMouseButtonStateEventArgs args)
        {
            if (_holdStateWindow != null)
            {
                _holdStateWindow.HandleMouseButtonStateEvent(args);
            }
        }

        /// <summary>
        /// Handle a keyboard state event
        /// </summary>
        /// <param name="args"></param>
        private void HandleKeyboardStateEvent(KxKeyboardStateEventArgs args)
        {
            if (_holdStateWindow != null)
            {
                _holdStateWindow.HandleKeyboardStateEvent(args);
            }
        }

        /// <summary>
        /// Handle a log message event
        /// </summary>
        /// <param name="args"></param>
        private void HandleLogMessageEvent(KxLogMessageEventArgs args)
        {
            if (_messageLogger.LoggingLevel >= args.LoggingLevel)
            {
                string msgType = null;
                if (args.LoggingLevel == Constants.LoggingLevelInfo)
                {
                    msgType = Properties.Resources.String_Info;
                }
                else if (args.LoggingLevel == Constants.LoggingLevelDebug)
                {
                    msgType = Properties.Resources.String_Debug;
                }

                if (msgType != null)
                {
                    MessageItem item = new MessageItem(DateTime.UtcNow, msgType, args.Text, args.Details);
                    _messageLogger.LogMessage(item, false);
                }
            }
        }

        /// <summary>
        /// Handle a request to start a program
        /// </summary>
        /// <param name="args"></param>
        private void HandleStartProgramEvent(KxStartProgramEventArgs args)
        {
            string thisCommand = "";
            try
            {
                StartProgramAction action = args.Action;

                // Check if the program is already running if required, and not already scheduled to occur
                if (_confirmCommandMessageBox == null && _commandBlockedMessageBox == null)
                {
                    // Get the command to execute
                    thisCommand = action.GetCommandLine();

                    // Check whether the program is allowed
                    CommandRuleManager ruleManager = new CommandRuleManager();
                    ruleManager.FromConfig(_appConfig);
                    CommandRuleItem matchingRule = ruleManager.ApplyRules(thisCommand);
                    ECommandAction actionType = (matchingRule != null) ? matchingRule.ActionType : ECommandAction.AskMe;

                    bool canStart = false;
                    if (actionType != ECommandAction.DontRun &&
                        (!action.CheckIfRunning || !ProcessManager.IsRunning(action.ProgramName)))
                    {
                        canStart = true;
                    }

                    switch (actionType)
                    {
                        case ECommandAction.Run:
                            if (canStart)
                            {
                                // Start the program now
                                ProcessManager.Start(action);
                            }
                            break;
                        case ECommandAction.DontRun:
                            {
                                // Add a disallow rule to the config if not already present, so that the user can easily change it
                                if (matchingRule != null && !matchingRule.Command.Equals(thisCommand))
                                {
                                    ruleManager.Rules.Add(new CommandRuleItem(thisCommand, ECommandAction.DontRun));
                                    ruleManager.ToConfig(_appConfig);
                                }

                                string message = string.Format(Properties.Resources.Info_CommandDisallowedMessage,
                                                            Constants.ProductName, Environment.NewLine, thisCommand);
                                _commandBlockedMessageBox = new CustomMessageBox(this, message, Properties.Resources.Info_CommandDisallowed, MessageBoxButton.OK, true, false);
                                _commandBlockedMessageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                                _commandBlockedMessageBox.IsModal = false;
                                _commandBlockedMessageBox.Show();
                                break;
                            }
                        case ECommandAction.AskMe:
                        default:
                            {
                                if (canStart)
                                {
                                    // Ask the user for confirmation, without blocking this thread
                                    _pendingStartProgramAction = action;
                                    string message = string.Format(Properties.Resources.Q_RunCommandMessage,
                                                                    Constants.ProductName, Environment.NewLine, thisCommand);
                                    _confirmCommandMessageBox = new CustomMessageBox(this, message, Properties.Resources.Q_RunCommand, MessageBoxButton.YesNoCancel, true, true);
                                    _confirmCommandMessageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                                    _confirmCommandMessageBox.IsModal = false;
                                    _confirmCommandMessageBox.Show();
                                }
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_RunCommand + " " + thisCommand, ex);
            }
        }

        /// <summary>
        /// Handle a request to load a profile
        /// Null profile name means load last used, empty profile name means load a blank profile
        /// </summary>
        /// <param name="args"></param>
        private void HandleLoadProfileEvent(KxLoadProfileEventArgs args)
        {
            try
            {
                // Disallow when current profile is unsaved
                if (!_trayManager.ProfileEdited)
                {
                    // Get the file name of the profile to load
                    string filePath = args.ProfileName;
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        // Add extension if required
                        if (!filePath.EndsWith(Constants.ProfileFileExtension))
                        {
                            filePath += Constants.ProfileFileExtension;
                        }

                        // Get the profiles directory
                        string defaultProfilesDir = Path.Combine(AppConfig.LocalAppDataDir, "Profiles");
                        string profilesDir = _appConfig.GetStringVal(Constants.ConfigUserCurrentProfileDirectory, defaultProfilesDir);

                        // Prepend the directory
                        filePath = Path.Combine(profilesDir, filePath);
                    }

                    _trayManager.LoadProfile(filePath, false, false);
                }
                else
                {
                    HandleError(Properties.Resources.E_ChangeProfileUnsavedChanges, null);
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_LoadProfileAction, ex);
            }
        }

        /// <summary>
        /// Handle change of output language
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyboardChangeEvent(KxKeyboardChangeEventArgs args)
        {
            try
            {
                // Update the output context
                _outputContext = args;

                // Update profile
                if (_profile != null)
                {
                    _profile.Initialise(args);
                }

                // Update controller and keyboard windows
                foreach (ControllerWindow window in _controllerWindows.Values)
                {
                    window.KeyboardLayoutChanged(args);
                }
                foreach (GridsWindow window in _keyboardWindows.Values)
                {
                    window.KeyboardLayoutChanged(args);
                }

                // Update tray manager
                _trayManager.KeyboardLayoutChanged(args);                
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_KeyboardLayoutChange, ex);
            }
        }

        /// <summary>
        /// Handle toggle controls event
        /// </summary>
        /// <param name="args"></param>
        private void HandleToggleControlsEvent(KxToggleControlsEventArgs args)
        {
            try
            {
                ControllerWindow window = GetControllerWindow(args.PlayerID);
                if (window != null)
                {
                    if (window.IsLoaded && window.Visibility == Visibility.Visible)
                    {
                        window.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        ShowControllerWindow(args.PlayerID);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_ShowOrHideControls, ex);
            }
        }        

        /// <summary>
        /// Catch all error handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CurrentDomain_UnhandledExcept‌​ion(object sender, UnhandledExceptionEventArgs args)
        {
            Exception ex = (Exception)args.ExceptionObject;
            HandleError(Properties.Resources.E_UnhandledError, ex);
        }

        /// <summary>
        /// Handle an error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void HandleError(string message, Exception ex)
        {
            // Log error if there's an exception to log or if tray notifications are disabled
            if (ex != null)
            {
                LogError(message, ex);
            }
            
            if (_trayManager != null)
            {
                _trayManager.ShowTemporaryMessage(message, "", System.Windows.Forms.ToolTipIcon.Warning);
            }
        }

        /// <summary>
        /// Clear any error message
        /// </summary>
        public void ClearError()
        {
            // Update the context menu text
            if (_trayManager != null)
            {
                _trayManager.ClearError();
            }            
        }

        /// <summary>
        /// Log an error to file and update the UI
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void LogError(string message, Exception ex)
        {
            if (_messageLogger.LoggingLevel >= Constants.LoggingLevelErrors)
            {
                // Log to file
                MessageItem errorItem = new MessageItem(message, ex);
                _messageLogger.LogMessage(errorItem, true);
            }
        }
    }
}
