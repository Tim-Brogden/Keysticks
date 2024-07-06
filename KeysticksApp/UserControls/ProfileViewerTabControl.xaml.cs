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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Keysticks.Actions;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Input;
using Keysticks.Sources;
using Keysticks.UI;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Tab control for displaying and editing a profile
    /// </summary>
    public partial class ProfileViewerTabControl : KxUserControl, IProfileDesignerControl
    {
        // Fields
        private bool _isLoaded = false;
        private bool _showOnlyActionTabs = false;
        private bool _showAutoActivations = true;
        private IProfileEditorWindow _editorWindow;
        private IThreadManager _threadManager;
        private EditBackgroundWindow _editBackgroundWindow;
        private ProfileSummariser _profileSummariser;
        private AppConfig _appConfig;
        private Profile _profile;
        private BaseSource _source;
        private StateVector _selectedState;
        private KxControlEventArgs _selectedControl;
        private List<ISourceViewerControl> _sourceViewerControls;
        private IActionViewerControl _visibleActionViewerControl;
        private IActionViewerControl _requiredKeyboardControl;
        private InputManager _inputManager;
        private NamedItemList _availableInputs;
        private DispatcherTimer _timer;
        private const int _timerIntervalMS = Constants.DefaultUIPollingIntervalMS;
        private long _idleIntervalTicks = TimeSpan.TicksPerSecond;
        private long _lastRefreshTicks = 0L;
        private bool _isBound;

        // Properties
        public StateVector SelectedState { get { return _selectedState; } }
        public bool ShowOnlyActionTabs { get { return _showOnlyActionTabs; } set { _showOnlyActionTabs = value; } }
        public bool ShowAutoActivations { get { return _showAutoActivations; } set { _showAutoActivations = value; } }

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
            typeof(ProfileViewerTabControl),
            new FrameworkPropertyMetadata(false/*, new PropertyChangedCallback(OnDependencyPropertyChanged)*/) 
        );

        public bool ShowUnusedControls
        {
            get { return (bool)GetValue(ShowUnusedControlsProperty); }
            set { SetValue(ShowUnusedControlsProperty, value); }
        }
        private static readonly DependencyProperty ShowUnusedControlsProperty =
            DependencyProperty.Register(
            "ShowUnusedControls",
            typeof(bool),
            typeof(ProfileViewerTabControl),
            new FrameworkPropertyMetadata(false/*, new PropertyChangedCallback(OnDependencyPropertyChanged)*/) 
        );

        // Events
        public static readonly RoutedEvent KxPlayerChangedEvent = EventManager.RegisterRoutedEvent(
            "PlayerChanged", RoutingStrategy.Bubble, typeof(KxIntValRoutedEventHandler), typeof(ProfileViewerTabControl));
        public event KxIntValRoutedEventHandler PlayerChanged
        {
            add { AddHandler(KxPlayerChangedEvent, value); }
            remove { RemoveHandler(KxPlayerChangedEvent, value); }
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ProfileViewerTabControl()
            : base()
        {
            InitializeComponent();

            _inputManager = new InputManager();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(_timerIntervalMS);
            _timer.Tick += HandleTimeout;

            _sourceViewerControls = new List<ISourceViewerControl> {
                                                                    SituationTree,
                                                                    InputsTree,
                                                                    ControllerLayoutControl,
                                                                    InputMappingsControl,
                                                                    ControllerViewControl,
                                                                    SettingsViewControl,
                                                                    KeyboardViewControl,
                                                                    ActionStripViewControl,
                                                                    SquareGridViewControl,
                                                                    NoKeyboardViewControl,
                                                                    ActionsSummaryViewControl,
                                                                    AutoActivationSettings };
            foreach (FrameworkElement viewerControl in _sourceViewerControls)
            {
                viewerControl.DataContext = this;
            }
            SummaryViewControl.DataContext = this;
        }

        /// <summary>
        /// Set the profile editor window to report to
        /// </summary>
        /// <param name="profileEditor"></param>
        public void SetProfileEditor(IProfileEditorWindow editorWindow)
        {
            _editorWindow = editorWindow;

            ControllerLayoutControl.SetProfileEditor(editorWindow);
            EditSituationsMenu.SetProfileEditor(editorWindow);
            EditActionsMenu.SetProfileEditor(editorWindow);
            NoKeyboardViewControl.SetProfileEditor(editorWindow);

            _threadManager = null;
            if (_editorWindow != null)
            {
                _threadManager = _editorWindow.GetTrayManager().ThreadManager;
            }
            _inputManager.SetThreadManager(_threadManager);
        }

        /// <summary>
        /// Get the profile editor window
        /// </summary>
        /// <returns></returns>
        public IProfileEditorWindow GetEditorWindow()
        {
            return _editorWindow;
        }

        /// <summary>
        /// Set the application configuration
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            _appConfig = appConfig;

            _inputManager.SetAppConfig(appConfig);
            EditSituationsMenu.SetAppConfig(appConfig);
            EditActionsMenu.SetAppConfig(appConfig);
            SummaryViewControl.SetAppConfig(appConfig);
            foreach (ISourceViewerControl viewerControl in _sourceViewerControls)
            {
                viewerControl.SetAppConfig(appConfig);
            }            
        }

        /// <summary>
        /// Set the profile to edit
        /// </summary>
        /// <param name="profile"></param>
        public void SetProfile(Profile profile)
        {
            // Store the profile to edit
            _profile = profile;
            _profileSummariser = new ProfileSummariser(profile);

            _inputManager.SetProfile(_profile);

            if (_isLoaded)
            {
                InitialiseDisplay();
            }
            SummaryViewControl.SetProfile(profile);

            // Select first source if possible
            BaseSource source = null;
            if (profile.VirtualSources.Count != 0)
            {
                source = (BaseSource)profile.VirtualSources[0];
            }
            SetSource(source);
        }

        /// <summary>
        /// Set the current player
        /// </summary>
        /// <param name="playerID"></param>
        private void SetSource(BaseSource source)
        {
            _source = source;
            if (source != null)
            {
                // Set source in child controls
                foreach (ISourceViewerControl viewer in _sourceViewerControls)
                {
                    viewer.SetSource(source);
                }
                if (_editBackgroundWindow != null)
                {
                    _editBackgroundWindow.SetSource(source);
                }
                EditSituationsMenu.SetSource(source);
                EditActionsMenu.SetSource(source);

                if (_isLoaded)
                {
                    _selectedState = null;  // Force refresh
                    SetCurrentSituation(source.StateTree.GetInitialState());
                    RefreshDisplay();
                }

                // Raise event
                RaiseEvent(new KxIntValRoutedEventArgs(KxPlayerChangedEvent, source.ID));
            }
        }

        /// <summary>
        /// Set the selected tab
        /// </summary>
        /// <param name="index"></param>
        public void SetCurrentTab(EProfileViewerTab eTab)
        {
            if (_isLoaded)
            {
                switch (eTab)
                {
                    case EProfileViewerTab.Controls:
                        ControlsRadioButton.IsChecked = true; break;
                    case EProfileViewerTab.Keyboard:
                        KeyboardRadioButton.IsChecked = true; break;
                    case EProfileViewerTab.Settings:
                        SettingsRadioButton.IsChecked = true; break;
                    case EProfileViewerTab.Inputs:
                        InputsRadioButton.IsChecked = true; break;
                    case EProfileViewerTab.Summary:
                        SummaryRadioButton.IsChecked = true; break;
                }
            }
        }

        /// <summary>
        /// Set the current situation
        /// </summary>
        public void SetCurrentSituation(StateVector situation)
        {
            bool modeOrPageChanged = (_selectedState == null) ||
                                             (_selectedState.ModeID != situation.ModeID) ||
                                             (_selectedState.PageID != situation.PageID);
            _selectedState = situation;

            if (_isLoaded && _selectedState != null)
            {
                if (modeOrPageChanged)
                {
                    ShowOrHideKeyboard(_selectedState);
                    SituationTree.SetState(_selectedState);
                    AutoActivationSettings.SetState(_selectedState);
                }
                SituationNameTextBlock.Text = string.Format("{0} {1}: {2}", Properties.Resources.String_Player, _source.ID, _source.Utils.GetAbsoluteSituationName(_selectedState));
                if (_visibleActionViewerControl != null)
                {
                    _visibleActionViewerControl.SetCurrentSituation(_selectedState);
                }
                if (IsDesignMode)
                {
                    ActionsSummaryViewControl.SetCurrentSituation(_selectedState);
                }

                // Raise event
                RaiseEvent(new KxStateRoutedEventArgs(KxStateChangedEvent, _selectedState));
            }
        }

        /// <summary>
        /// Set the current input event
        /// </summary>
        /// <param name="inputControl"></param>
        private void SetCurrentInputEvent(KxControlEventArgs inputControl)
        {
            _selectedControl = inputControl;

            if (IsDesignMode)
            {
                ActionsSummaryViewControl.SetCurrentControl(inputControl);
            }
            RaiseEvent(new KxInputControlRoutedEventArgs(KxInputControlChangedEvent, inputControl));
        }

        /// <summary>
        /// Initialise display
        /// </summary>
        private void InitialiseDisplay()
        {
            if (_profile.VirtualSources.Count != 0)
            {
                ControlsRadioButton.Visibility = Visibility.Visible;
                KeyboardRadioButton.Visibility = Visibility.Visible;
                SettingsRadioButton.Visibility = Visibility.Visible;
                if (_showOnlyActionTabs)
                {
                    InputsRadioButton.Visibility = Visibility.Collapsed;
                    SummaryRadioButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    InputsRadioButton.Visibility = Visibility.Visible;
                    SummaryRadioButton.Visibility = Visibility.Visible;
                }
                AutoActivationSettings.Visibility = _showAutoActivations ? Visibility.Visible : Visibility.Collapsed;

                // Select controls tab if no tab is selected
                if (_visibleActionViewerControl == null)
                {
                    ControlsRadioButton.IsChecked = true;
                }
            }
            else
            {
                // Select summary tab
                SummaryRadioButton.IsChecked = true;

                ControlsRadioButton.Visibility = Visibility.Collapsed;
                KeyboardRadioButton.Visibility = Visibility.Collapsed;
                SettingsRadioButton.Visibility = Visibility.Collapsed;
                InputsRadioButton.Visibility = Visibility.Collapsed;
            }

            ShowOrHidePlayerControls();            
        }

        /// <summary>
        /// Refresh the control
        /// </summary>
        public void RefreshDisplay()
        {
            // Show or hide tabs
            if (_isLoaded && _profile != null)
            {
                RefreshThisControl();
                if (_profile.VirtualSources.Count != 0)
                {
                    // Refresh the child controls
                    SituationTree.RefreshDisplay();
                    AutoActivationSettings.RefreshDisplay();
                    //InputsTree.RefreshDisplay();
                    //ControllerLayoutControl.RefreshDisplay();
                    //InputMappingsControl.RefreshDisplay();
                    RefreshActionViewers();
                }
            }
        }

        /// <summary>
        /// Refresh only this control, not child controls
        /// </summary>
        public void RefreshThisControl()
        {
            if (_source != null)
            {
                // Inputs caption
                InputsCaptionTextBlock.Text = string.Format("{0} {1}: {2}", 
                    Properties.Resources.String_Player, _source.ID, Properties.Resources.String_InputsAndDesign);

                // Refresh action set name caption
                string text;
                if (_selectedState != null)
                {
                    text = string.Format("{0} {1}: {2}", Properties.Resources.String_Player, _source.ID, _source.Utils.GetAbsoluteSituationName(_selectedState));
                }
                else
                {
                    text = string.Format("{0} {1}", Properties.Resources.String_Player, _source.ID);
                }
                SituationNameTextBlock.Text = text;
            }
        }

        /// <summary>
        /// Refresh action viewer controls
        /// </summary>
        public void RefreshActionViewers()
        {
            ControllerViewControl.RefreshDisplay();
            SettingsViewControl.RefreshDisplay();
            KeyboardViewControl.RefreshDisplay();
            ActionStripViewControl.RefreshDisplay();
            SquareGridViewControl.RefreshDisplay();
            ActionsSummaryViewControl.RefreshDisplay();
            SummaryViewControl.RefreshDisplay();            
        }

        /// <summary>
        /// Refresh the backgrounds of the controller viewers
        /// </summary>
        public void RefreshBackground()
        {
            ControllerLayoutControl.RefreshLayout();
            ControllerViewControl.RefreshLayout();
        }

        /// <summary>
        /// Handle input language change
        /// </summary>
        public void KeyboardLayoutChanged(KxKeyboardChangeEventArgs args)
        {
            EditActionsMenu.KeyboardLayoutChanged(args);
        }        

        /// <summary>
        /// Control loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;
               
                // Configure according to design mode
                if (IsDesignMode)
                {
                    SituationTree.StateEdit += Control_EditSituation;
                    SituationTree.StateRename += Control_RenameSituation;
                    SituationTree.StateDelete += Control_DeleteSituation;
                }
                ActionsSummaryViewControl.Visibility = IsDesignMode ? Visibility.Visible : Visibility.Collapsed;
                Grid.SetRowSpan(ActionViewerControlsGrid, IsDesignMode ? 1 : 2);

                // Initialise display
                if (_profile != null)
                {
                    InitialiseDisplay();
                    RefreshThisControl();
                }

                // Set the initial state
                if (_source != null)
                {
                    StateVector situation = _selectedState;
                    if (situation == null)
                    {
                        situation = _source.StateTree.GetInitialState();
                    }
                    _selectedState = null;                  // Force refresh
                    SetCurrentSituation(situation);
                }
            }
        }

        /// <summary>
        /// Control unloaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_editBackgroundWindow != null)
            {
                _editBackgroundWindow.Close();
                _editBackgroundWindow = null;
            }
            _timer.IsEnabled = false;
            _inputManager.Dispose();            
        }

        /// <summary>
        /// Handle child window closing
        /// </summary>
        /// <param name="window"></param>
        public void ChildWindowClosing(Window window)
        {
            if (window == _editBackgroundWindow)
            {
                _editBackgroundWindow = null;
            }
        }

        /// <summary>
        /// Tab changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isLoaded)
                {
                    // Determine the active controls
                    IActionViewerControl actionViewerControl = null;
                    bool isActionsTab = false;
                    RadioButton radioButton = (RadioButton)sender;
                    if (radioButton == ControlsRadioButton)
                    {
                        actionViewerControl = ControllerViewControl;
                        isActionsTab = true;                        
                    }
                    else if (radioButton == KeyboardRadioButton)
                    {
                        actionViewerControl = _requiredKeyboardControl;
                        isActionsTab = true;
                    }
                    else if (radioButton == SettingsRadioButton)
                    {
                        actionViewerControl = SettingsViewControl;
                        isActionsTab = true;
                    }

                    // Show or hide grids
                    if (isActionsTab)
                    {
                        InputsViewerGrid.Visibility = Visibility.Collapsed;
                        SummaryViewerGrid.Visibility = Visibility.Collapsed;
                        ActionViewerGrid.Visibility = Visibility.Visible;

                        ShowActionsView(actionViewerControl);
                        if (_visibleActionViewerControl != null && _selectedState != null)
                        {
                            _visibleActionViewerControl.SetCurrentSituation(_selectedState);
                        }
                    }
                    else if (radioButton == InputsRadioButton)
                    {
                        ActionViewerGrid.Visibility = Visibility.Collapsed;
                        SummaryViewerGrid.Visibility = Visibility.Collapsed;
                        InputsViewerGrid.Visibility = Visibility.Visible;
                    }
                    else if (radioButton == SummaryRadioButton)
                    {
                        ActionViewerGrid.Visibility = Visibility.Collapsed;
                        InputsViewerGrid.Visibility = Visibility.Collapsed;
                        SummaryViewerGrid.Visibility = Visibility.Visible;
                    }

                    // Enable or disable action editor window
                    if (_editorWindow != null)
                    {
                        _editorWindow.EnableActionEditor(isActionsTab);
                    }

                    // Monitor inputs when on inputs tab
                    _timer.IsEnabled = IsDesignMode && (radioButton == InputsRadioButton);
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_SwitchTabs, ex);
            }
            e.Handled = true;
        }

        /// <summary>
        /// Poll the inputs on timeout (design mode only)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HandleTimeout(object sender, EventArgs args)
        {
            try
            {                
                // Poll the inputs
                bool connected = _inputManager.UpdateState();
                _source.UpdateState();
                if (_source.IsStateChanged)
                {
                    InputsTree.RefreshAnimations();
                    ControllerLayoutControl.RefreshAnimations();
                }

                long ticks = DateTime.UtcNow.Ticks;
                if ((!connected || !_isBound) &&
                    ticks - _lastRefreshTicks > _idleIntervalTicks)
                {
                    _lastRefreshTicks = ticks;

                    // Regularly rebind inputs if a required device is not connected
                    if (_inputManager.RefreshConnectedDeviceList(false))
                    {
                        _isBound = _inputManager.BindProfile();
                    }                    
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Change player
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayerRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isLoaded)
                {
                    RadioButton button = (RadioButton)sender;
                    string playerStr = (string)button.Tag;
                    int playerID;
                    switch (playerStr)
                    {
                        case "1":
                        default:
                            playerID = Constants.ID1; break;
                        case "2":
                            playerID = Constants.ID2; break;
                        case "3":
                            playerID = Constants.ID3; break;
                        case "4":
                            playerID = Constants.ID4; break;
                    }

                    BaseSource source = _profile.GetSource(playerID);
                    if (source != null)
                    {
                        SetSource(source);
                    }
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_SelectPlayer, ex);
            }
            e.Handled = true;
        }

        /// <summary>
        /// Add a player
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProfileBuilder builder = new ProfileBuilder();

                SendDebugMessage("ProfileViewerTabControl AddPlayer");

                // Add source for new player
                int id = _profile.VirtualSources.GetFirstUnusedID(Constants.ID1);
                BaseSource controller = builder.CreateDefaultVirtualController(id, Properties.Resources.String_DefaultControlSetName);
                _profile.AddSource(controller);

                // Refresh the profile summary
                _profileSummariser.UpdateMetaData();
                
                // Update button visibility
                ShowOrHidePlayerControls();

                // Bind profile to devices
                _inputManager.RefreshConnectedDeviceList(false);
                _isBound = _inputManager.BindProfile();

                // Select the new player
                switch (id)
                {
                    case 2:
                        Player2RadioButton.IsChecked = true; break;
                    case 3:
                        Player3RadioButton.IsChecked = true; break;
                    case 4:
                        Player4RadioButton.IsChecked = true; break;
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_AddPlayer, ex);
            }
            e.Handled = true;
        }
        
        /// <summary>
        /// Remove the last player
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeletePlayerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Disallow deletion unless 2 or more players
                int index = _profile.VirtualSources.Count - 1;
                if (index > 0)
                {
                    // See if we're deleting the selected player
                    BaseSource source = (BaseSource)_profile.VirtualSources[index];
                    bool deletingSelectedPlayer = false;
                    switch (source.ID)
                    {
                        case Constants.ID2:
                            deletingSelectedPlayer = (Player2RadioButton.IsChecked == true); break;
                        case Constants.ID3:
                            deletingSelectedPlayer = (Player3RadioButton.IsChecked == true); break;
                        case Constants.ID4:
                            deletingSelectedPlayer = (Player4RadioButton.IsChecked == true); break;
                    }

                    // Confirm with user if required
                    bool confirmed = _appConfig.GetBoolVal(Constants.ConfigAutoConfirmDeletePlayer, Constants.DefaultAutoConfirmDeletePlayer);
                    if (!confirmed)
                    {
                        string message = string.Format(Properties.Resources.Q_RemovePlayerMessage, source.ID);
                        CustomMessageBox messageBox = new CustomMessageBox((Window)_editorWindow, message, Properties.Resources.Q_RemovePlayer, MessageBoxButton.OKCancel, true, true);
                        if (messageBox.ShowDialog() == true)
                        {
                            confirmed = true;
                            if (messageBox.DontAskAgain)
                            {
                                // User clicked OK and chose not to be asked again
                                _appConfig.SetBoolVal(Constants.ConfigAutoConfirmDeletePlayer, true);
                            }
                        }
                    }

                    if (confirmed)
                    {
                        // Remove last player                    
                        _profile.RemoveSource(source);

                        // Refresh the profile summary
                        _profileSummariser.UpdateMetaData();
                        SummaryViewControl.RefreshDisplay();

                        // Update button visibility
                        ShowOrHidePlayerControls();

                        // Select first player if selected player is being deleted
                        if (deletingSelectedPlayer)
                        {
                            Player1RadioButton.IsChecked = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_RemovePlayer, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Show or hide player buttons
        /// </summary>
        private void ShowOrHidePlayerControls()
        {
            // Show the player radio buttons in design mode or for multi-player profiles
            PlayerPanel.Visibility = (IsDesignMode || _profile.VirtualSources.Count > 1) ? Visibility.Visible : Visibility.Collapsed;

            // Show correct buttons
            AddPlayerButton.Visibility = (IsDesignMode && _profile.VirtualSources.Count < Constants.MaxPlayers) ? Visibility.Visible : Visibility.Collapsed;
            DeletePlayerButton.Visibility = (IsDesignMode && _profile.VirtualSources.Count > 1) ? Visibility.Visible : Visibility.Collapsed;
            Player2RadioButton.Visibility = _profile.VirtualSources.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            Player3RadioButton.Visibility = _profile.VirtualSources.Count > 2 ? Visibility.Visible : Visibility.Collapsed;
            Player4RadioButton.Visibility = _profile.VirtualSources.Count > 3 ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Handle change of selected state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Control_StateChanged(object sender, KxStateRoutedEventArgs e)
        {
            try
            {
                StateVector newSituation = e.State;

                SetCurrentSituation(newSituation);
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_SelectControlSet, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle change of selected control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Control_InputEventChanged(object sender, KxInputControlRoutedEventArgs e)
        {
            try
            {
                SetCurrentInputEvent(e.Control);
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_SelectControl, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Show the context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Control_EditSituation(object sender, KxStateRoutedEventArgs e)
        {
            try
            {
                StateVector state = e.State;

                // Open context menu
                EditSituationsMenu.SetState(state);
                EditSituationsMenu.ShowContextMenu((UIElement)sender);
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_OpenEditSituationsMenu, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Rename situation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Control_RenameSituation(object sender, KxStateRoutedEventArgs e)
        {
            try
            {
                StateVector state = e.State;

                // Let the editor control handle the command
                if (state.PageID != Constants.DefaultID)
                {
                    EditSituationsMenu.SetState(state);
                    EditSituationsMenu.RenamePage();
                }
                else if (state.ModeID != Constants.DefaultID)
                {
                    EditSituationsMenu.SetState(state);
                    EditSituationsMenu.RenameMode();
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_RenameControlSetOrPage, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Delete situation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Control_DeleteSituation(object sender, KxStateRoutedEventArgs e)
        {
            try
            {
                StateVector state = e.State;

                // Let the editor control handle the command
                if (state.PageID != Constants.DefaultID)
                {
                    EditSituationsMenu.SetState(state);
                    EditSituationsMenu.DeletePage();
                }
                else if (state.ModeID != Constants.DefaultID)
                {
                    EditSituationsMenu.SetState(state);
                    EditSituationsMenu.DeleteMode();
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_DeleteControlSetOrPage, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle situation edit in viewer control e.g. adding keyboard, deleting action strip cells
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Control_SituationsEdited(object sender, KxStateRoutedEventArgs e)
        {
            try
            {
                OnSituationsEdited(e.State);
                
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_EditControlSets, ex);
            }

            e.Handled = true;
        }

        private void OnSituationsEdited(StateVector stateToSelect)
        {
            // Refresh the profile summary
            _profileSummariser.UpdateMetaData();

            // Refresh the control set viewer
            RefreshDisplay();

            // Select specified state if applicable
            if (stateToSelect != null)
            {
                _selectedState = null;  // Force refresh
                SetCurrentSituation(stateToSelect);
            }
        }

        /// <summary>
        /// Auto-activation changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_ActivationChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                // Refresh the profile summary
                _profileSummariser.UpdateMetaData();
                SummaryViewControl.RefreshDisplay();
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_ActivationChange, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Quick edit actions (using context menu)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_QuickEditActions(object sender, RoutedEventArgs e)
        {            
            if (_selectedState != null && _selectedControl != null)
            {
                try
                {
                    EditActionsMenu.SetState(_selectedState);
                    EditActionsMenu.SetControl(_selectedControl);
                    EditActionsMenu.ShowContextMenu((UIElement)sender);
                }
                catch (Exception ex)
                {
                    ReportError(Properties.Resources.E_OpenQuickEdit, ex);
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle moving / editing / deleting actions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Control_ActionsEdited(object sender, RoutedEventArgs e)
        {
            try
            {
                // Refresh the profile summary
                _profileSummariser.UpdateMetaData();

                // Refresh UI
                RefreshActionViewers();

                // Refresh action editor
                if (_editorWindow != null)
                {
                    _editorWindow.RefreshActionEditor();
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_EditActions, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Get the available controllers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_GetInputsList(object sender, RoutedEventArgs e)
        {
            if (_profile != null)
            {
                try
                {
                    SendDebugMessage("ProfileViewerTabControl GetInputsList");
                    
                    // Bind all devices to get the list of available devices
                    _inputManager.RefreshConnectedDeviceList(true);
                    _availableInputs = _inputManager.GetAvailableInputs();
                    InputsTree.SetAvailableInputs(_availableInputs);

                    // Retain only required devices
                    _inputManager.RefreshConnectedDeviceList(false);
                    _isBound = _inputManager.BindProfile();
                }
                catch (Exception ex)
                {
                    ReportError(Properties.Resources.E_GetInputsList, ex);
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle change of input in the inputs list control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_InputsEdited(object sender, RoutedEventArgs e)
        {
            try
            {
                SendDebugMessage("ProfileViewerTabControl InputsEdited");

                // Bind profile to devices
                _inputManager.RefreshConnectedDeviceList(false);
                _isBound = _inputManager.BindProfile();

                ControllerLayoutControl.RefreshDisplay();
                InputMappingsControl.RefreshDisplay();
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_SelectInput, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle change of dead zone in the inputs list control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_DeadZoneEdited(object sender, RoutedEventArgs e)
        {
            try
            {
                SendDebugMessage("ProfileViewerTabControl DeadZoneEdited");

                // Rebind to apply new dead zone
                _isBound = _inputManager.BindProfile();
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_ChangeDeadZone, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle virtual control move by dragging
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_LayoutEdited(object sender, RoutedEventArgs e)
        {
            try
            {
                ControllerViewControl.RefreshLayout();
                if (_editBackgroundWindow != null)
                {
                    _editBackgroundWindow.RefreshDisplay();
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_RefreshControllerLayout, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle virtual control added or deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_ControlsEdited(object sender, RoutedEventArgs e)
        {
            try
            {
                SendDebugMessage("ProfileViewerTabControl ControlsEdited");

                // Rebind profile to inputs
                _isBound = _inputManager.BindProfile();

                ControllerViewControl.InitialiseDisplay();
                SettingsViewControl.CreateDisplay();
                ActionsSummaryViewControl.RefreshDisplay();
                SituationTree.RefreshDisplay();
                if (_editBackgroundWindow != null)
                {
                    _editBackgroundWindow.RefreshDisplay();
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_EditControllerDesign, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle virtual control renamed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_ControlRenamed(object sender, RoutedEventArgs e)
        {
            try
            {
                SettingsViewControl.CreateDisplay();
                ActionsSummaryViewControl.RefreshDisplay();
                if (_editorWindow != null)
                {
                    _editorWindow.RefreshActionEditor();
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_RenameControl, ex);
            }
            e.Handled = true;
        }

        /// <summary>
        /// Handle input mapping edit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_MappingEdited(object sender, RoutedEventArgs e)
        {
            try
            {
                SendDebugMessage("ProfileViewerTabControl MappingEdited");

                // Rebind profile to inputs
                _isBound = _inputManager.BindProfile();

                ControllerLayoutControl.RefreshDisplay();
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_EditInputMapping, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Return whether Delete is allowed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearActions_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsDesignMode && _selectedState != null && _selectedControl != null;
            e.Handled = true;
        }

        /// <summary>
        /// Handle delete command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ClearActions_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            try
            {
                EditActionsMenu.SetState(_selectedState);
                EditActionsMenu.SetControl(_selectedControl);
                EditActionsMenu.ClearActions();
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_ClearActions, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle change of selected annotation in inputs tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_LayoutControlClicked(object sender, KxInputControlRoutedEventArgs e)
        {
            KxControlEventArgs ev = e.Control;

            InputMappingsControl.SetCurrentInputEvent(ev);
        }

        /// <summary>
        /// Open controller background editor window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_EditBackgroundClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_editBackgroundWindow == null)
                {
                    _editBackgroundWindow = new EditBackgroundWindow(this);
                }
                _editBackgroundWindow.SetAppConfig(_appConfig);
                _editBackgroundWindow.SetSource(_source);
                _editBackgroundWindow.Show();

                // Position alongside this window
                Window parentWindow = (Window)_editorWindow;
                _editBackgroundWindow.Top = 0.0;
                _editBackgroundWindow.Left = Math.Min(parentWindow.Left + parentWindow.Width + 10, SystemParameters.VirtualScreenWidth - _editBackgroundWindow.Width);
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_OpenBackgroundEditor, ex);
            }
        }

        /// <summary>
        /// If the mode of the selected situation has an associated grid
        /// show the template for that grid. Otherwise, show the standard controller view.
        /// </summary>
        private void ShowOrHideKeyboard(StateVector situation)
        {
            EGridType requiredGridType = EGridType.None;
            _requiredKeyboardControl = NoKeyboardViewControl;
            if (situation != null)
            {
                int modeID = situation.ModeID;
                AxisValue modeItem = _source.StateTree.GetMode(modeID);
                if (modeItem != null)
                {
                    // Window associated so show
                    requiredGridType = modeItem.GridType;
                    switch (requiredGridType)
                    {
                        case EGridType.Keyboard:
                            _requiredKeyboardControl = KeyboardViewControl;
                            break;
                        case EGridType.ActionStrip:
                            _requiredKeyboardControl = ActionStripViewControl;
                            break;
                        case EGridType.Square4x4:
                        case EGridType.Square8x4:
                            _requiredKeyboardControl = SquareGridViewControl;
                            break;
                        // case ... Other types of grid here
                    }
                }
            }

            if (KeyboardRadioButton.IsChecked == true)
            {
                if (requiredGridType != EGridType.None)
                {
                    // Show this keyboard
                    ShowActionsView(_requiredKeyboardControl);
                }
                else
                {
                    // Switch back to the control tab if no keyboard is to be shown
                    ControlsRadioButton.IsChecked = true;
                }
            }
            else if (ControlsRadioButton.IsChecked == true &&
                    requiredGridType != EGridType.None &&
                    situation.PageID != Constants.DefaultID)
            {
                // Switch from Controls to Keyboard tab if it is visible and a non-default page is selected
                KeyboardRadioButton.IsChecked = true;
            }            
        }

        /// <summary>
        /// Show the required actions view (controls / keyboard / settings)
        /// </summary>
        /// <param name="actionViewerControl"></param>
        private void ShowActionsView(IActionViewerControl actionViewerControl)
        {
            if (actionViewerControl != _visibleActionViewerControl)
            {
                // Hide the current viewer control
                if (_visibleActionViewerControl != null)
                {
                    ((UIElement)_visibleActionViewerControl).Visibility = Visibility.Collapsed;
                }
                _visibleActionViewerControl = actionViewerControl;

                // Show or hide option to remove keyboard controls
                bool canRemoveKeyboard = IsDesignMode && (actionViewerControl == KeyboardViewControl || actionViewerControl == SquareGridViewControl || actionViewerControl == ActionStripViewControl);
                RemoveKeyboardButton.Visibility = canRemoveKeyboard ? Visibility.Visible : Visibility.Collapsed;

                // Show the new action viewer control
                KxControlEventArgs inputControl = null;
                if (_visibleActionViewerControl != null)
                {
                    ((UIElement)_visibleActionViewerControl).Visibility = Visibility.Visible;

                    // Preserve the control selection
                    inputControl = _visibleActionViewerControl.CurrentInputEvent;
                }

                // Set the selected control
                SetCurrentInputEvent(inputControl);
            }
        }

        /// <summary>
        /// Report an error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        private void ReportError(string message, Exception ex)
        {
            KxErrorRoutedEventArgs args = new KxErrorRoutedEventArgs(KxErrorEvent, new KxException(message, ex));
            RaiseEvent(args);
        }

        /// <summary>
        /// Send a debug message to the message logger
        /// </summary>
        /// <param name="text"></param>
        /// <param name="details"></param>
        private void SendDebugMessage(string text, string details = "")
        {
            if (_threadManager != null)
            {
                _threadManager.SubmitUIEvent(new KxLogMessageEventArgs(Constants.LoggingLevelDebug, text, details));
            }
        }

        /// <summary>
        /// Remove keyboard controls from the current control set
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveKeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int modeID = _selectedState.ModeID;
                AxisValue modeItem = _source.StateTree.GetMode(modeID);

                // Confirm with user if required
                bool confirmed = _appConfig.GetBoolVal(Constants.ConfigAutoConfirmDeleteKeyboard, Constants.DefaultAutoConfirmDeleteKeyboard);
                if (!confirmed)
                {
                    string message = string.Format(Properties.Resources.Q_RemoveKeyboardMessage, modeItem.Name);
                    CustomMessageBox messageBox = new CustomMessageBox((Window)_editorWindow, message, Properties.Resources.Q_RemoveKeyboard, MessageBoxButton.OKCancel, true, true);
                    if (messageBox.ShowDialog() == true)
                    {
                        confirmed = true;
                        if (messageBox.DontAskAgain)
                        {
                            // User clicked OK and chose not to be asked again
                            _appConfig.SetBoolVal(Constants.ConfigAutoConfirmDeleteKeyboard, true);
                        }
                    }
                }

                if (confirmed)
                {
                    // Remove any control set pages and their actions
                    modeItem.SubValues.Clear();

                    // Remove actions navigation and selection actions
                    ActionMappingTable modeActions = _source.Actions.GetActionsForState(modeItem.Situation, false);
                    Dictionary<int, ActionSet>.Enumerator eActionSets = modeActions.GetEnumerator();
                    while (eActionSets.MoveNext())
                    {
                        ActionSet actionSet = eActionSets.Current.Value;
                        if (_source.Utils.IsGeneralTypeOfControl(actionSet.EventArgs, modeItem.Controls.SelectionControl))
                        {
                            _source.Actions.RemoveActionSet(actionSet);
                        }
                        else if (_source.Utils.IsGeneralTypeOfControl(actionSet.EventArgs, modeItem.Controls.NavigationControl) &&
                            actionSet.EventArgs.SettingType != EControlSetting.None)
                        {
                            // Only remove timing and directionality settings for the navigation control
                            // because the user may have drag-dropped the navigate cell actions to another control.
                            // Instead, allow RemoveKeyboardSpecificActions() to remove the navigate cell actions.
                            _source.Actions.RemoveActionSet(actionSet);
                        }
                        else
                        {
                            RemoveKeyboardSpecificActions(actionSet);
                        }
                    }

                    // Remove keyboard grid and controls
                    modeItem.Grid = null;
                    modeItem.Controls = null;

                    // Remove all other keyboard actions
                    _source.Actions.Validate();

                    // Refresh UI
                    OnSituationsEdited(modeItem.Situation);
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_RemoveKeyboardControls, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Remove keyboard specific actions from an action set.
        /// Intended to be called for control sets which do not have any pages.
        /// </summary>
        /// <param name="actionSet"></param>
        private void RemoveKeyboardSpecificActions(ActionSet actionSet)
        {
            foreach (ActionList actionList in actionSet.ActionLists)
            {
                int i = 0;
                while (i < actionList.Count)
                {
                    BaseAction action = actionList[i];

                    bool remove = false;
                    if (action is WordPredictionAction)
                    {
                        remove = true;
                    }
                    else if (action is NavigateCellsAction)
                    {
                        remove = true;
                    }
                    else if (action is ChangeSituationAction)
                    {
                        ChangeSituationAction csa = (ChangeSituationAction)action;
                        remove = csa.NewSituation.ModeID == Constants.NoneID ||
                                        csa.NewSituation.ModeID == actionSet.LogicalState.ModeID;
                    }                    

                    if (remove)
                    {
                        actionList.RemoveAt(i);
                        actionSet.Updated();
                        continue;
                    }

                    i++;
                }
            }
        }
    }  
}
