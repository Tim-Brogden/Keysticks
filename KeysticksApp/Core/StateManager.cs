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
using System.Threading;
using System.Windows;
using Keysticks.Config;
using Keysticks.Event;
using Keysticks.Input;
using Keysticks.Sources;
using Keysticks.Sys;
using System.Diagnostics;

namespace Keysticks.Core
{
    /// <summary>
    /// Monitors the state of the input sources and certain system variables (e.g. active process, keyboard layout) 
    /// and triggers the corresponding actions to be performed
    /// </summary>
    public class StateManager : IStateManager
    {
        // Fields
        private ThreadManager _parent;        
        private TimeSpan _systemPollingInterval = new TimeSpan(Constants.DefaultSystemPollingIntervalMS * TimeSpan.TicksPerMillisecond);
        private Profile _profile;
        private AppConfig _appConfig = new AppConfig();
        //private bool _isDiagnosticsEnabled = false;
        private bool _isConnected = false;
        private bool _isProcessMonitoringReqd = false;
        private IntPtr _fgWindowHandle = IntPtr.Zero;
        private WINDOWINFO _windowInfo = new WINDOWINFO(true);
        private Rect _currentWindowRect = new Rect();

        private int _inputPollingIntervalMS = Constants.DefaultInputPollingIntervalMS;
        private IntPtr _currentKeyboardLayout = IntPtr.Zero;
        private DateTime _lastSystemUpdateTime = DateTime.UtcNow;
        private List<ActionList> _ongoingActionLists;
        private int _loggingLevel = Constants.DefaultMessageLoggingLevel;
        private KeyPressManager _keyStateManager;
        private MouseManager _mouseStateManager;
        private WordPredictionManager _wordPredictionManager;
        private InputManager _inputManager;
        private CellUtils _cellManager;
        private IKeyboardContext _keyboardContext;

        // Properties
        public Profile CurrentProfile { get { return _profile; } }
        public AppConfig AppConfig { get { return _appConfig; } }
        public IThreadManager ThreadManager { get { return _parent; } }
        public KeyPressManager KeyStateManager { get { return _keyStateManager; } }
        public MouseManager MouseStateManager { get { return _mouseStateManager; } }
        public WordPredictionManager WordPredictionManager { get { return _wordPredictionManager; } }
        public CellUtils CellManager { get { return _cellManager; } }
        public IKeyboardContext KeyboardContext { get { return _keyboardContext; } }
        public int PollingIntervalMS { get { return _inputPollingIntervalMS; } }
        public Rect CurrentWindowRect { get { return _currentWindowRect; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public StateManager(ThreadManager parent)
        {
            _parent = parent;

            try
            {                
                // Initialise keyboard layout
                IntPtr dwhkl = WindowsAPI.GetKeyboardLayout(0);
                KeyUtils.InitialiseKeyboardLayout(dwhkl);
                _keyboardContext = new KxKeyboardChangeEventArgs(dwhkl);

                _ongoingActionLists = new List<ActionList>();
                _keyStateManager = new KeyPressManager(this);
                _mouseStateManager = new MouseManager(this);
                _wordPredictionManager = new WordPredictionManager(this);
                _cellManager = new CellUtils();
                _inputManager = new InputManager();
                _inputManager.SetThreadManager(_parent);
            }
            catch (Exception ex)
            {
                _parent.SubmitUIEvent(new KxErrorMessageEventArgs(Properties.Resources.E_InitStateManager, ex));
            }
        }

        /// <summary>
        /// Handle change of app config
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            try
            {
                // Store
                _appConfig = appConfig;

                // Update state manager
                int pollingIntervalMS = appConfig.GetIntVal(Constants.ConfigInputPollingIntervalMS, Constants.DefaultInputPollingIntervalMS);
                if (pollingIntervalMS >= Constants.MinInputPollingIntervalMS)
                {
                    _inputPollingIntervalMS = pollingIntervalMS;
                }
                _loggingLevel = _appConfig.GetIntVal(Constants.ConfigMessageLoggingLevel, Constants.DefaultMessageLoggingLevel);
                _keyStateManager.UseScanCodes = appConfig.GetBoolVal(Constants.ConfigUseScanCodes, Constants.DefaultUseScanCodes);
                _keyStateManager.KeyStrokeLengthTicks = TimeSpan.TicksPerMillisecond * appConfig.GetIntVal(Constants.ConfigKeyStrokeLengthMS, Constants.DefaultKeyStrokeLengthMS);
                _keyStateManager.DisallowShiftDelete = appConfig.GetBoolVal(Constants.ConfigDisallowShiftDelete, Constants.DefaultDisallowShiftDelete);
                _mouseStateManager.ClickLengthTicks = TimeSpan.TicksPerMillisecond * appConfig.GetIntVal(Constants.ConfigMouseClickLengthMS, Constants.DefaultMouseClickLengthMS);
                _mouseStateManager.PointerSpeed = appConfig.GetFloatVal(Constants.ConfigMousePointerSpeed, Constants.DefaultMousePointerSpeed);
                _mouseStateManager.PointerAcceleration = appConfig.GetFloatVal(Constants.ConfigMousePointerAcceleration, Constants.DefaultMousePointerAcceleration);

                // Update inputs
                if (_profile != null)
                {
                    foreach (BaseSource inputSource in _profile.VirtualSources)
                    {
                        inputSource.SetAppConfig(_appConfig);
                    }
                }
            }
            catch (Exception ex)
            {
                _parent.SubmitUIEvent(new KxErrorMessageEventArgs(Properties.Resources.E_ConfigureStateManager, ex));
            }
        }

        /// <summary>
        /// Configure the sets of actions to perform when different events occur in each of the logical states
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="logicalState"></param>
        public void SetProfile(Profile profile)
        {
            try
            {
                _parent.SubmitUIEvent(new KxLogMessageEventArgs(Constants.LoggingLevelDebug, "StateManager SetProfile"));

                if (_profile != null)
                {
                    // Clear the state
                    RestoreNeutralSystemState();
                }

                // Set the current profile
                _profile = profile;
                _profile.Initialise(_keyboardContext);
                _profile.SetAppConfig(_appConfig);

                // See if we need to monitor the active process
                _fgWindowHandle = IntPtr.Zero;
                _isProcessMonitoringReqd = _profile.HasAutoActivations() || _profile.HasActionsOfType(EActionType.MoveThePointer);

                // Set the app config and state
                foreach (BaseSource inputSource in _profile.VirtualSources)
                {
                    inputSource.SetAppConfig(_appConfig);
                    inputSource.SetStateManager(this);
                    inputSource.SetCurrentState(inputSource.StateTree.GetInitialState());
                }

                // Connect the input mappings
                _inputManager.SetProfile(profile);
                _inputManager.RefreshConnectedDeviceList(false);
                _inputManager.BindProfile();
            }
            catch (Exception ex)
            {
                _parent.SubmitUIEvent(new KxErrorMessageEventArgs(Properties.Resources.E_SetProfileStateManager, ex));
            }
        }
        
        /// <summary>
        /// Update the state of the input sources
        /// </summary>
        public void Run()
        {
            // Initialise configuration
            _parent.ReceiveStateManagerConfig(this);

            // Read and ignore any events before the thread was started
            List<KxEventArgs> eventsReceived = _parent.ReceiveStateEvents(); 
            while (_parent.ContinueStateManager)
            {
                // Continue any existing actions
                if (_ongoingActionLists.Count != 0)
                {
                    ContinueOngoingActions();
                }

                // Handle any events submitted from other threads
                eventsReceived = _parent.ReceiveStateEvents();
                if (eventsReceived != null)
                {
                    foreach (KxEventArgs args in eventsReceived)
                    {
                        switch (args.EventType)
                        {
                            case EEventType.RepeatKey:
                                HandleRepeatKeyEvent((KxRepeatKeyEventArgs)args); break;
                            case EEventType.Text:
                                HandleTextEvent((KxTextEventArgs)args); break;
                            case EEventType.WordPrediction:
                                _wordPredictionManager.HandlePredictionEvent((KxPredictionEventArgs)args); break;
                            case EEventType.StateChange:
                                HandleStateChangeEvent((KxStateChangeEventArgs)args); break;
                        }
                    }
                }

                // Read the device states
                bool connected = _inputManager.UpdateState();                

                // Check for new events to raise
                foreach (BaseSource source in _profile.VirtualSources)
                {
                    if (source.UpdateState())
                    {
                        source.RaiseEvents(this);
                    }
                    else
                    {
                        connected = false;                        
                    }
                }

                // Rebind inputs when connection first lost
                if (!connected && _isConnected)
                {
                    _parent.SubmitUIEvent(new KxLogMessageEventArgs(Constants.LoggingLevelInfo, Properties.Resources.Info_ConnectionLost));
                    //Trace.WriteLine("Connection to input lost");
                    if (_inputManager.RefreshConnectedDeviceList(false))
                    {
                        // Device list changed - rebind
                        _inputManager.BindProfile();
                    }
                }
                _isConnected = connected;

                // Perform system updates (less frequently)
                if (DateTime.UtcNow - _lastSystemUpdateTime > _systemPollingInterval)
                {
                    // Check for config updates
                    _parent.ReceiveStateManagerConfig(this);

                    // Monitor keyboard layout change
                    UpdateKeyboardLayout();

                    // Check active window
                    if (_isProcessMonitoringReqd)
                    {
                        MonitorActiveWindow();
                    }

                    // Regularly try to rebind inputs while disconnected
                    if (!_isConnected)
                    {
                        if (_inputManager.RefreshConnectedDeviceList(false))
                        {
                            // Device list changed - rebind
                            _inputManager.BindProfile();
                        }
                    }

                    _lastSystemUpdateTime = DateTime.UtcNow;
                }

                Thread.Sleep(_inputPollingIntervalMS);
            }

            // Clean up before exiting
            EndRun();
        }

        /// <summary>
        /// Check for a change of keyboard layout
        /// </summary>
        private void UpdateKeyboardLayout()
        {
            // Check for change of input language
            IntPtr foreWindow = WindowsAPI.GetForegroundWindow();
            uint foreThreadID = 0u;
            if (foreWindow != IntPtr.Zero)
            {
                foreThreadID = WindowsAPI.GetWindowThreadProcessId(foreWindow, IntPtr.Zero);
                if (foreThreadID != 0u)
                {
                    IntPtr hkl = WindowsAPI.GetKeyboardLayout(foreThreadID);
                    if (hkl != _currentKeyboardLayout)
                    {
                        if (_currentKeyboardLayout != IntPtr.Zero)
                        {
                            HandleKeyboardChangeEvent(new KxKeyboardChangeEventArgs(hkl));
                        }
                        _currentKeyboardLayout = hkl;
                    }
                }
            }
        }

        /// <summary>
        /// Monitor the active window / process
        /// </summary>
        private void MonitorActiveWindow()
        {
            // Get the current window
            IntPtr handle = WindowsAPI.GetForegroundWindow();
            if (handle != _fgWindowHandle && handle != IntPtr.Zero)
            {
                // Store the window handle
                _fgWindowHandle = handle;

                // Get window size and position
                if (WindowsAPI.GetWindowInfo(handle, ref _windowInfo))
                {
                    _currentWindowRect = new Rect(new Point(_windowInfo.rcWindow.Left, _windowInfo.rcWindow.Top),
                                                    new Point(_windowInfo.rcWindow.Right, _windowInfo.rcWindow.Bottom));
                }

                // Get window's process ID
                int processID;
                WindowsAPI.GetWindowThreadProcessId(handle, out processID);

                // Current process change - get exe name
                Process process = Process.GetProcessById(processID);
                string title = WindowsAPI.GetTitleBarText(handle);
                if (process != null && title != null)
                {
                    string processName = process.ProcessName.ToLower();

                    _parent.SubmitUIEvent(new KxLogMessageEventArgs(Constants.LoggingLevelDebug, string.Format("Process {0} window {1}", processName, title)));
                    title = title.ToLower();
                    foreach (BaseSource source in _profile.VirtualSources)
                    {
                        source.SetCurrentWindow(processName, title);
                    }
                }                
            }
        }

        /// <summary>
        /// Process a key press or release event
        /// </summary>
        /// <param name="feetUpKeyEventArgs"></param>
        public void HandleKeyEvent(KxKeyEventArgs args)
        {
            if (_loggingLevel >= Constants.LoggingLevelInfo)
            {
                _parent.SubmitUIEvent(new KxKeyEventArgs(args));
            }

            _keyStateManager.UpdateKeyState(args);                        
            _wordPredictionManager.HandleKeyEvent(args);            
        }

        /// <summary>
        /// Process a repeat key press event
        /// </summary>
        /// <param name="args"></param>
        private void HandleRepeatKeyEvent(KxRepeatKeyEventArgs args)
        {
            if (_loggingLevel >= Constants.LoggingLevelInfo)
            {
                _parent.SubmitUIEvent(new KxRepeatKeyEventArgs(args));
            }

            _keyStateManager.RepeatVirtualKey(args.Key, (uint)args.Count);
            _wordPredictionManager.HandleRepeatKeyEvent(args);
        }

        /// <summary>
        /// Process a text input event
        /// </summary>
        /// <param name="args"></param>
        public void HandleTextEvent(KxTextEventArgs args)
        {
            if (_loggingLevel >= Constants.LoggingLevelInfo)
            {
                _parent.SubmitUIEvent(new KxTextEventArgs(args));
            }

            _keyStateManager.TypeString(args.Text);
            _wordPredictionManager.HandleTextEvent(args);
        }

        /// <summary>
        /// Process a state change event sent by the UI thread
        /// </summary>
        /// <param name="args"></param>
        private void HandleStateChangeEvent(KxStateChangeEventArgs args)
        {
            SetCurrentState(args.PlayerID, args.LogicalState);
        }

        /// <summary>
        /// Process a change of input language
        /// </summary>
        public void HandleKeyboardChangeEvent(KxKeyboardChangeEventArgs args)
        {
            // Update keyboard layouts
            KeyUtils.InitialiseKeyboardLayout(args.KeyboardHKL);

            // Update state manager data
            _keyboardContext = args;
            _keyStateManager.KeyboardLayoutChanged(args);
            _wordPredictionManager.KeyboardLayoutChanged(args);
            if (_profile != null)
            {
                _profile.Initialise(args);
            }

            // Pass on to UI
            _parent.SubmitUIEvent(new KxKeyboardChangeEventArgs(args));
        }

        /// <summary>
        /// Clean up before exiting
        /// </summary>
        private void EndRun()
        {
            RestoreNeutralSystemState();

            if (_inputManager != null)
            {
                _inputManager.Dispose();
            }
        }

        /// <summary>
        /// Add ongoing actions
        /// </summary>
        /// <param name="actionList"></param>
        public void AddOngoingActions(ActionList actionList)
        {
            _ongoingActionLists.Add(actionList);
        }

        /// <summary>
        /// Continue actions which persist over more than one update cycle
        /// </summary>
        private void ContinueOngoingActions()
        {
            // Actions
            int i = 0;
            while (i < _ongoingActionLists.Count)
            {
                ActionList actionList = _ongoingActionLists[i];
                if (actionList.IsOngoing)
                {
                    actionList.Continue(this);
                    i++;
                }
                else
                {
                    actionList.CurrentEvent.InUse = false;  // Free up event object
                    _ongoingActionLists.RemoveAt(i);
                }
            }
        }        

        /// <summary>
        /// Update the current logical state of the application
        /// </summary>
        /// <param name="relativeState"></param>
        public void SetCurrentState(int playerID, StateVector relativeState)
        {
            BaseSource source = _profile.GetSource(playerID);
            if (source != null)
            {
                source.SetCurrentState(relativeState);
            }
        }
        
        /// <summary>
        /// Stop any ongoing actions and return the system to a neutral state
        /// </summary>
        private void RestoreNeutralSystemState()
        {
            // Clear ongoing actions
            _ongoingActionLists.Clear();

            // Reset the keyboard state
            _keyStateManager.ReleaseAll();

            // Reset the mouse state
            _mouseStateManager.ReleaseAll();
        }

    }
}
