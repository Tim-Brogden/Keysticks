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
using System.Windows.Input;
using Keysticks.Core;
using Keysticks.Config;
using Keysticks.Controls;
using Keysticks.Event;
using Keysticks.Actions;
using Keysticks.Sources;

namespace Keysticks
{
    /// <summary>
    /// Edit actions window
    /// </summary>
    public partial class EditActionWindow : Window
    {
        // Fields
        private bool _isLoaded = false;
        private IProfileEditorWindow _parentWindow;
        private StringUtils _utils = new StringUtils();
        private FrameworkElement _visibleActionGrid;
        private BaseSource _source;
        private StateVector _currentSituation;
        private KxControlEventArgs _currentInputControl;
        private EEventReason _currentReason = EEventReason.None;
        private AppConfig _appConfig;
        private Dictionary<EEventReason, RadioButton> _eventReasonButtons;
        private Dictionary<EPressType, RadioButton> _pressTypeButtons;
        private NamedItemList _actionListItems = new NamedItemList();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="action">Can be null</param>
        /// <param name="inputControl"></param>
        public EditActionWindow(IProfileEditorWindow parentWindow)
        {
            _parentWindow = parentWindow;

            InitializeComponent();

            // Create a dictionary of event reason radio buttons
            _eventReasonButtons = new Dictionary<EEventReason, RadioButton>();
            _eventReasonButtons[EEventReason.Directed] = this.ReasonDirected;
            _eventReasonButtons[EEventReason.Undirected] = this.ReasonUndirected;
            _eventReasonButtons[EEventReason.Moved] = this.ReasonMoved;
            _eventReasonButtons[EEventReason.Pressed] = this.ReasonPressed;
            _eventReasonButtons[EEventReason.Released] = this.ReasonReleased;
            _eventReasonButtons[EEventReason.Activated] = this.ReasonActivated;
            //_eventReasonButtons[EEventReason.Deactivated] = this.ReasonDeactivated;

            // Create a dictionary of press type radio buttons
            _pressTypeButtons = new Dictionary<EPressType, RadioButton>();
            _pressTypeButtons[EPressType.Any] = this.PressTypeAny;
            _pressTypeButtons[EPressType.Short] = this.PressTypeShort;
            _pressTypeButtons[EPressType.Long] = this.PressTypeLong;
            _pressTypeButtons[EPressType.AutoRepeat] = this.PressTypeAutoRepeat;

            // Bind controls with data that isn't control or situation dependent
            StartProgramDetails.ParentWindow = this;
        }

        /// <summary>
        /// Set the config
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            _appConfig = appConfig;

            LoadProfileDetails.SetAppConfig(appConfig);
        }

        /// <summary>
        /// Set the source
        /// </summary>
        /// <param name="source"></param>
        public void SetSource(BaseSource source)
        {
            _source = source;
            BaseKeyActionDetails.SetSource(source);

            RefreshDisplay();
        }

        /// <summary>
        /// Set the state to edit
        /// </summary>
        /// <param name="situation"></param>
        /// <param name="inputControl"></param>
        public void SetData(StateVector situation, KxControlEventArgs inputControl)
        {
            _currentSituation = situation;
            _currentInputControl = inputControl;

            RefreshDisplay();
        }

        /// <summary>
        /// Loaded event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;

            // Bind action list
            this.ActionsList.ItemsSource = _actionListItems;

            RefreshDisplay();
        }

        /// <summary>
        /// Handle change of input language
        /// </summary>
        public void KeyboardLayoutChanged(KxKeyboardChangeEventArgs args)
        {
            BaseKeyActionDetails.KeyboardLayoutChanged(args);            

            RefreshDisplay();
        }
        
        /// <summary>
        /// Refresh the display when the state or input control is changed
        /// </summary>
        private void RefreshDisplay()
        {
            if (!_isLoaded || _source == null)
            {
                return;
            }

            try
            {
                ErrorMessage.Clear();

                // Situation name
                if (_currentSituation != null)
                {
                    string situationName = _source.Utils.GetAbsoluteSituationName(_currentSituation);
                    this.SituationTextBlock.Text = situationName;
                    this.SituationTextBlock.ToolTip = situationName;    // To help it's too long to display fully
                }
                else
                {
                    this.SituationTextBlock.Text = Properties.Resources.String_NoControlSetSelected;
                    this.SituationTextBlock.ToolTip = null;
                }

                // Get the current control
                BaseControl control = null;
                if (_currentInputControl != null)
                {
                    control = _source.GetVirtualControl(_currentInputControl);
                }

                // Input name
                if (control != null)
                {
                    this.ControlTextBlock.Text = control.Name;
                }
                else
                {
                    this.ControlTextBlock.Text = Properties.Resources.String_NoControlSelected;
                }

                // Bind child controls
                this.ChangeSituationDetails.SetSourceAndSituation(_source, _currentSituation);
                this.NavigateCellsDetails.SetInputControl(_currentInputControl);
                this.SetDirectionModeDetails.SetInputControl(_currentInputControl);

                // Event reasons
                RefreshEventReasons();

                // If current reason doesn't have actions, but another does, select it
                TryToSelectAReasonWithActions();

                // Press types
                RefreshPressTypes();

                // Actions
                RefreshActionsList();

                // Alternative text
                RefreshDisplayOptions();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_RefreshDisplay, ex);
            }
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
        /// Selected reason changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReasonButtons_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                // Set the reason for the input event
                _currentReason = GetSelectedEventReason();

                RefreshPressTypes();
                RefreshActionsList();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_ReasonChange, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Selected press type changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PressTypeButtons_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                // Set the reason for the input event
                _currentReason = GetSelectedEventReason();

                RefreshActionsList();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_PressLengthChange, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Display the list of actions for the selected logical state
        /// </summary>
        private void RefreshActionsList()
        {
            // Bind action types, remembering the current selection
            NamedItem selectedActionType = (NamedItem)this.ActionTypeCombo.SelectedItem;
            NamedItemList validActionTypesList = null;
            if (_currentSituation != null && _currentInputControl != null && _source != null)
            {
                // Get the valid action types
                validActionTypesList = new NamedItemList(); 
                foreach (EActionType actionType in Enum.GetValues(typeof(EActionType)))
                {
                    if (_source.Utils.IsActionTypeValid(_currentSituation, _currentInputControl, _currentReason, actionType))
                    {
                        validActionTypesList.Add(new NamedItem((int)actionType, _utils.ActionTypeToString(actionType)));
                    }
                }
            }
            this.ActionTypeCombo.ItemsSource = validActionTypesList;

            // Clear existing actions
            _actionListItems.Clear();

            if (validActionTypesList != null)
            {
                // Get action list for this event
                ActionList actionList = null;
                ActionSet actionSet = _source.Actions.GetActionsForInputControl(_currentSituation, _currentInputControl, false);
                if (actionSet != null)
                {
                    actionList = actionSet.GetActions(_currentReason);
                }

                if (actionList != null && actionList.Count > 0)
                {
                    // Action list exists

                    // Add actions to display list
                    foreach (BaseAction action in actionList)
                    {
                        _actionListItems.Add(new NamedItem(0, action.Name));
                    }

                    // Select the first action
                    this.ActionsList.SelectedIndex = 0;
                }
                else
                {
                    // Select the same action type if possible, or else the first one
                    if (selectedActionType != null && validActionTypesList.GetItemByID(selectedActionType.ID) != null)
                    {
                        this.ActionTypeCombo.SelectedValue = selectedActionType.ID;
                    }
                    else if (validActionTypesList.Count != 0)
                    {
                        this.ActionTypeCombo.SelectedIndex = 0;
                    }
                }
            }

            ShowOrHideMoveUpDownButtons();
        }

        /// <summary>
        /// Show the display text/icon for the action set
        /// </summary>
        private void RefreshDisplayOptions()
        {
            List<EAnnotationImage> iconList = null;
            string displayText = "";            
            EAnnotationImage displayIcon = EAnnotationImage.None;
            if (_currentSituation != null && _currentInputControl != null && _source != null)
            {
                // Get action list for this event
                ActionSet actionSet = _source.Actions.GetActionsForInputControl(_currentSituation, _currentInputControl, false);
                if (actionSet != null)
                {
                    displayText = actionSet.Info.TinyDescription;
                    displayIcon = actionSet.Info.IconRef;

                    iconList = new List<EAnnotationImage>();
                    bool isSituationChangeOnly = true;
                    foreach (ActionList actionList in actionSet.ActionLists)
                    {
                        foreach (BaseAction action in actionList)
                        {
                            EAnnotationImage icon = action.IconRef;
                            if (icon != EAnnotationImage.None && !iconList.Contains(icon))
                            {
                                iconList.Add(icon);
                            }

                            if (action.ActionType != EActionType.ChangeControlSet &&
                                action.ActionType != EActionType.NavigateCells)
                            {
                                isSituationChangeOnly = false;                                                 
                            }
                        }
                    }

                    // If the action set only changes situation, and other icons are allowed, also allow the "do nothing icon"
                    if (isSituationChangeOnly && iconList.Count != 0)
                    {
                        iconList.Add(EAnnotationImage.DontShow);
                    }                    
                }
            }

            this.DisplayText.Text = displayText;
            this.DisplayText.IsEnabled = (_currentInputControl != null && _currentInputControl.SettingType == EControlSetting.None);

            this.DisplayIconPicker.SetIconList(iconList);
            if (iconList != null && iconList.Count != 0)
            {
                this.DisplayIconPicker.SelectedIcon = displayIcon;
                this.DisplayIconPanel.IsEnabled = true;
            }
            else
            {
                this.DisplayIconPanel.IsEnabled = false;
            }
        }

        /// <summary>
        /// Show / hide / enable / disable the action moving buttons
        /// </summary>
        private void ShowOrHideMoveUpDownButtons()
        {
            if (this.ActionsList.Items.Count > 1)
            {
                // Show move up / down buttons
                this.MoveUpButton.Visibility = Visibility.Visible;
                this.MoveDownButton.Visibility = Visibility.Visible;
                Grid.SetColumnSpan(this.ActionsList, 1);
            }
            else
            {
                // Hide move up / down buttons
                this.MoveUpButton.Visibility = Visibility.Collapsed;
                this.MoveDownButton.Visibility = Visibility.Collapsed;
                Grid.SetColumnSpan(this.ActionsList, 2);
            }
        }

        /// <summary>
        /// Handle selection of action in list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                // Get action list for this event
                ActionList actionList = null;
                if (_currentSituation != null && _currentInputControl != null && _source != null)
                {
                    ActionSet actionSet = _source.Actions.GetActionsForInputControl(_currentSituation, _currentInputControl, false);
                    if (actionSet != null)
                    {
                        actionList = actionSet.GetActions(_currentReason);
                    }
                }

                int selectedIndex = this.ActionsList.SelectedIndex;
                if (selectedIndex > -1 && actionList != null && selectedIndex < actionList.Count)
                {
                    // Set current action and display
                    DisplayAction(actionList[selectedIndex]);

                    // Enable buttons
                    this.DeleteActionButton.IsEnabled = true;
                    this.UpdateActionButton.IsEnabled = true;
                    this.MoveUpButton.IsEnabled = (selectedIndex > 0);
                    this.MoveDownButton.IsEnabled = (selectedIndex < actionList.Count - 1);
                }
                else
                {
                    // Disable buttons
                    this.DeleteActionButton.IsEnabled = false;
                    this.UpdateActionButton.IsEnabled = false;
                    this.MoveUpButton.IsEnabled = false;
                    this.MoveDownButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_DisplayAction, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Display an action's settings
        /// </summary>
        private void DisplayAction(BaseAction action)
        {
            // Display settings for current action
            switch (action.ActionType)
            {
                case EActionType.TypeKey:
                case EActionType.PressDownKey:
                case EActionType.ReleaseKey:
                case EActionType.ToggleKey:
                    this.BaseKeyActionDetails.ActionType = action.ActionType;
                    this.BaseKeyActionDetails.SetCurrentAction(action);
                    break;
                case EActionType.TypeText:
                    this.TypeTextDetails.SetCurrentAction(action);
                    break;                
                case EActionType.ClickMouseButton:
                case EActionType.DoubleClickMouseButton:
                case EActionType.PressDownMouseButton:
                case EActionType.ReleaseMouseButton:
                case EActionType.ToggleMouseButton:
                    this.MouseButtonActionDetails.ActionType = action.ActionType;
                    this.MouseButtonActionDetails.SetCurrentAction(action);
                    break;
                case EActionType.MouseWheelUp:
                case EActionType.MouseWheelDown:
                    // Nothing to do
                    break;
                case EActionType.ControlThePointer:
                    this.ControlThePointerDetails.SetCurrentAction(action);
                    break;
                case EActionType.MoveThePointer:
                    this.MoveThePointerDetails.SetCurrentAction(action);
                    break;
                case EActionType.ChangeControlSet:
                    this.ChangeSituationDetails.SetCurrentAction(action);
                    break;
                case EActionType.NavigateCells:
                    this.NavigateCellsDetails.SetCurrentAction(action);
                    break;
                case EActionType.LoadProfile:
                    this.LoadProfileDetails.SetCurrentAction(action);
                    break;
                case EActionType.StartProgram:
                    this.StartProgramDetails.SetCurrentAction(action);
                    break;
                case EActionType.ActivateWindow:
                    this.ActivateWindowDetails.SetCurrentAction(action);
                    break;
                case EActionType.MaximiseWindow:
                case EActionType.MinimiseWindow:
                case EActionType.ToggleControlsWindow:
                    // Nothing to do
                    break;
                case EActionType.Wait:
                    this.WaitDetails.SetCurrentAction(action);
                    break;
                case EActionType.WordPrediction:
                    this.WordPredictionDetails.SetCurrentAction(action);
                    break;
                case EActionType.SetDirectionMode:
                    this.SetDirectionModeDetails.SetCurrentAction(action);
                    break;
                case EActionType.SetDwellAndAutorepeat:
                    this.SetDwellAndAutorepeatDetails.SetCurrentAction(action);
                    break;
                case EActionType.DoNothing:
                    // Nothing to do
                    break;
            }

            // Select action type
            this.ActionTypeCombo.SelectedValue = (int)action.ActionType;
        }

        /// <summary>
        /// Move up button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                // Get action list for this event
                if (_source != null)
                {
                    ActionSet actionSet = _source.Actions.GetActionsForInputControl(_currentSituation, _currentInputControl, false);
                    if (actionSet != null)
                    {
                        ActionList actionList = actionSet.GetActions(_currentReason);
                        int selectedIndex = this.ActionsList.SelectedIndex;
                        if (actionList != null && selectedIndex > -1 && selectedIndex < actionList.Count)
                        {
                            // Move action
                            BaseAction action = actionList[selectedIndex - 1];
                            actionList.RemoveAt(selectedIndex - 1);
                            actionList.Insert(selectedIndex, action);

                            // Move display item
                            NamedItem item = _actionListItems[selectedIndex - 1];
                            _actionListItems.RemoveAt(selectedIndex - 1);
                            _actionListItems.Insert(selectedIndex, item);

                            selectedIndex--;
                            this.MoveUpButton.IsEnabled = (selectedIndex > 0);
                            this.MoveDownButton.IsEnabled = (selectedIndex < actionList.Count - 1);

                            // Report the change
                            actionSet.Updated();
                            _source.Actions.ActionSetUpdated(actionSet);

                            RefreshDisplayOptions();
                            _parentWindow.ActionsEdited();
                        }
                    }
                }                
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_ReorderItems, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Move down button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                // Get action list for this event
                if (_source != null)
                {
                    ActionSet actionSet = _source.Actions.GetActionsForInputControl(_currentSituation, _currentInputControl, false);
                    if (actionSet != null)
                    {
                        ActionList actionList = actionSet.GetActions(_currentReason);
                        int selectedIndex = this.ActionsList.SelectedIndex;
                        if (actionList != null && selectedIndex > -1 && selectedIndex < actionList.Count - 1)
                        {
                            // Move action
                            BaseAction action = actionList[selectedIndex + 1];
                            actionList.RemoveAt(selectedIndex + 1);
                            actionList.Insert(selectedIndex, action);

                            // Move display item
                            NamedItem item = _actionListItems[selectedIndex + 1];
                            _actionListItems.RemoveAt(selectedIndex + 1);
                            _actionListItems.Insert(selectedIndex, item);

                            selectedIndex++;
                            this.MoveUpButton.IsEnabled = (selectedIndex > 0);
                            this.MoveDownButton.IsEnabled = (selectedIndex < actionList.Count - 1);

                            // Report the change
                            actionSet.Updated();
                            _source.Actions.ActionSetUpdated(actionSet);

                            RefreshDisplayOptions();
                            _parentWindow.ActionsEdited();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_ReorderItems, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// New type of action selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActionTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                NamedItem selectedItem = (NamedItem)this.ActionTypeCombo.SelectedItem;
                if (selectedItem != null)
                {
                    EActionType actionType = (EActionType)selectedItem.ID;
                    switch (actionType)
                    {
                        case EActionType.TypeKey:
                        case EActionType.PressDownKey:
                        case EActionType.ReleaseKey:
                        case EActionType.ToggleKey:
                            this.BaseKeyActionDetails.ActionType = actionType;
                            ShowActionGrid(this.BaseKeyActionDetails);
                            break;
                        case EActionType.TypeText:
                            ShowActionGrid(this.TypeTextDetails);
                            break;
                        case EActionType.ClickMouseButton:
                        case EActionType.DoubleClickMouseButton:
                        case EActionType.PressDownMouseButton:
                        case EActionType.ReleaseMouseButton:
                        case EActionType.ToggleMouseButton:
                            this.MouseButtonActionDetails.ActionType = actionType;
                            ShowActionGrid(this.MouseButtonActionDetails);
                            break;
                        case EActionType.MouseWheelUp:
                        case EActionType.MouseWheelDown:
                            ShowActionGrid(null);
                            break;
                        case EActionType.ControlThePointer:
                            ShowActionGrid(this.ControlThePointerDetails);
                            break;
                        case EActionType.MoveThePointer:
                            ShowActionGrid(this.MoveThePointerDetails);
                            break;
                        case EActionType.ChangeControlSet:
                            ShowActionGrid(this.ChangeSituationDetails);
                            break;
                        case EActionType.NavigateCells:
                            ShowActionGrid(this.NavigateCellsDetails);
                            break;
                        case EActionType.LoadProfile:
                            ShowActionGrid(this.LoadProfileDetails);
                            break;
                        case EActionType.StartProgram:
                            ShowActionGrid(this.StartProgramDetails);
                            break;
                        case EActionType.ActivateWindow:
                            ShowActionGrid(this.ActivateWindowDetails);
                            break;
                        case EActionType.MaximiseWindow:
                        case EActionType.MinimiseWindow:
                        case EActionType.ToggleControlsWindow:
                            ShowActionGrid(null);
                            break;
                        case EActionType.Wait:
                            ShowActionGrid(this.WaitDetails);
                            break;
                        case EActionType.WordPrediction:
                            ShowActionGrid(this.WordPredictionDetails);
                            break;
                        case EActionType.SetDirectionMode:
                            ShowActionGrid(this.SetDirectionModeDetails);
                            break;
                        case EActionType.SetDwellAndAutorepeat:
                            ShowActionGrid(this.SetDwellAndAutorepeatDetails);
                            break;
                        case EActionType.DoNothing:
                            ShowActionGrid(null);
                            break;
                    }

                    this.NewActionButton.IsEnabled = true;
                }
                else
                {
                    ShowActionGrid(null);
                    this.NewActionButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_ActionTypeChange, ex);
            }

            e.Handled = true;
        }        

        /// <summary>
        /// New clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                if (_source != null)
                {
                    // Add a new action
                    BaseAction newAction = CreateAction();
                    if (newAction != null)
                    {
                        // Check if adding this action is allowed regardless
                        bool canAdd = !_appConfig.GetBoolVal(Constants.ConfigDisallowDangerousControls, Constants.DefaultDisallowDangerousControls);
                        if (!canAdd)
                        {
                            // Check if the action is potentially harmful
                            string errorMessage;
                            canAdd = _source.ActionEditor.CanAddAction(newAction,
                                                            _currentSituation,
                                                            _currentInputControl,
                                                            _currentReason,
                                                            _appConfig,
                                                            out errorMessage);
                            if (!canAdd && errorMessage != "")
                            {
                                // Allow the user to turn off the security feature
                                string message = errorMessage + " " + string.Format(Properties.Resources.Q_DisableSecurityFeatureMessage, Environment.NewLine);
                                CustomMessageBox messageBox = new CustomMessageBox(this, message, Properties.Resources.Q_DisableSecurityFeature, MessageBoxButton.YesNoCancel, false, false);
                                messageBox.ShowDialog();
                                if (messageBox.Result == MessageBoxResult.Yes)
                                {
                                    canAdd = true;
                                    _appConfig.SetBoolVal(Constants.ConfigDisallowDangerousControls, false);
                                }
                            }
                        }
                        if (canAdd)
                        {
                            // Add action to profile
                            _source.ActionEditor.AddAction(newAction, _currentSituation, _currentInputControl, _currentReason);

                            // Update display
                            _actionListItems.Add(new NamedItem(0, newAction.Name));
                            this.ActionsList.SelectedIndex = _actionListItems.Count - 1;

                            this.DeleteActionButton.IsEnabled = true;

                            // Make the reason button bold when the first action is added
                            if (_actionListItems.Count == 1)
                            {
                                RefreshEventReasons();
                                RefreshPressTypes();
                            }

                            ShowOrHideMoveUpDownButtons();
                            RefreshDisplayOptions();

                            // Tell the parent that the profile has been updated
                            _parentWindow.ActionsEdited();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_AddAction, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Delete clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                if (_source != null)
                {
                    // Try to delete the selected action
                    int selectedIndex = this.ActionsList.SelectedIndex;
                    if (_source.ActionEditor.DeleteAction(selectedIndex, _currentSituation, _currentInputControl, _currentReason))
                    {
                        // Remove from display
                        _actionListItems.RemoveAt(selectedIndex);

                        // Select item
                        if (selectedIndex < _actionListItems.Count)
                        {
                            this.ActionsList.SelectedIndex = selectedIndex;
                        }
                        else if (selectedIndex > 0)
                        {
                            this.ActionsList.SelectedIndex = selectedIndex - 1;
                        }
                        else
                        {
                            // Deleted last action, so unbold radio buttons
                            RefreshEventReasons();
                            RefreshPressTypes();
                        }

                        ShowOrHideMoveUpDownButtons();
                        RefreshDisplayOptions();

                        // Tell the parent that the profile has been updated
                        _parentWindow.ActionsEdited();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_DeleteAction, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Update clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                if (_source != null)
                {
                    // Update the selected action
                    int selectedIndex = this.ActionsList.SelectedIndex;
                    BaseAction updatedAction = CreateAction();
                    if (updatedAction != null)
                    {
                        // Check if replacing this action is allowed regardless
                        bool canReplace = !_appConfig.GetBoolVal(Constants.ConfigDisallowDangerousControls, Constants.DefaultDisallowDangerousControls);
                        if (!canReplace)
                        {
                            // Check if the action is potentially harmful
                            string errorMessage;
                            canReplace = _source.ActionEditor.CanReplaceAction(selectedIndex, updatedAction, _currentSituation, _currentInputControl, _currentReason, out errorMessage);
                            if (!canReplace && errorMessage != "")
                            {
                                // Allow the user to turn off the security feature
                                string message = errorMessage + " " + string.Format(Properties.Resources.Q_DisableSecurityFeatureMessage, Environment.NewLine);
                                CustomMessageBox messageBox = new CustomMessageBox(this, message, Properties.Resources.Q_DisableSecurityFeature, MessageBoxButton.YesNoCancel, false, false);
                                messageBox.ShowDialog();
                                if (messageBox.Result == MessageBoxResult.Yes)
                                {
                                    canReplace = true;
                                    _appConfig.SetBoolVal(Constants.ConfigDisallowDangerousControls, false);
                                }
                            }
                        }

                        if (canReplace)
                        {
                            // Replace action
                            _source.ActionEditor.ReplaceAction(selectedIndex, updatedAction, _currentSituation, _currentInputControl, _currentReason);
                            _actionListItems[selectedIndex].Name = updatedAction.Name;

                            RefreshDisplayOptions();

                            // Tell the parent that the profile has been updated
                            _parentWindow.ActionsEdited();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_UpdateAction, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Create an action according to the UI selections
        /// </summary>
        /// <returns></returns>
        private BaseAction CreateAction()
        {
            BaseAction action = null;

            NamedItem selectedItem = (NamedItem)this.ActionTypeCombo.SelectedItem;
            if (selectedItem != null)
            {
                EActionType actionType = (EActionType)selectedItem.ID;
                switch (actionType)
                {
                    case EActionType.TypeKey:
                    case EActionType.PressDownKey:
                    case EActionType.ReleaseKey:
                    case EActionType.ToggleKey:
                        action = this.BaseKeyActionDetails.GetCurrentAction();
                        break;
                    case EActionType.TypeText:
                        action = this.TypeTextDetails.GetCurrentAction();
                        break;
                    case EActionType.ClickMouseButton:
                    case EActionType.DoubleClickMouseButton:
                    case EActionType.PressDownMouseButton:
                    case EActionType.ReleaseMouseButton:
                    case EActionType.ToggleMouseButton:
                        action = this.MouseButtonActionDetails.GetCurrentAction();
                        break;
                    case EActionType.MouseWheelUp:
                        action = new MouseWheelAction();
                        ((MouseWheelAction)action).IsUpDirection = true;
                        break;
                    case EActionType.MouseWheelDown:
                        action = new MouseWheelAction();
                        ((MouseWheelAction)action).IsUpDirection = false;
                        break;
                    case EActionType.ControlThePointer:
                        action = this.ControlThePointerDetails.GetCurrentAction();
                        break;
                    case EActionType.MoveThePointer:
                        action = this.MoveThePointerDetails.GetCurrentAction();
                        break;
                    case EActionType.ChangeControlSet:
                        action = this.ChangeSituationDetails.GetCurrentAction();
                        break;
                    case EActionType.NavigateCells:
                        action = this.NavigateCellsDetails.GetCurrentAction();
                        break;
                    case EActionType.LoadProfile:
                        action = this.LoadProfileDetails.GetCurrentAction();
                        break;
                    case EActionType.StartProgram:
                        action = this.StartProgramDetails.GetCurrentAction();
                        break;
                    case EActionType.ActivateWindow:
                        action = this.ActivateWindowDetails.GetCurrentAction();
                        break;
                    case EActionType.MaximiseWindow:
                        action = new ShowCurrentWindowAction();
                        ((ShowCurrentWindowAction)action).MaximiseOrMinimise = true;
                        break;
                    case EActionType.MinimiseWindow:
                        action = new ShowCurrentWindowAction();
                        ((ShowCurrentWindowAction)action).MaximiseOrMinimise = false;
                        break;
                    case EActionType.ToggleControlsWindow:
                        action = new ToggleControlsWindowAction();
                        break;
                    case EActionType.Wait:
                        action = this.WaitDetails.GetCurrentAction();
                        break;
                    case EActionType.WordPrediction:
                        action = this.WordPredictionDetails.GetCurrentAction();
                        break;
                    case EActionType.SetDirectionMode:
                        action = this.SetDirectionModeDetails.GetCurrentAction();
                        break;
                    case EActionType.SetDwellAndAutorepeat:
                        action = this.SetDwellAndAutorepeatDetails.GetCurrentAction();
                        break;
                    case EActionType.DoNothing:
                        action = new DoNothingAction();
                        break;
                }
            }

            // Initialise the action's internal data
            if (action != null)
            {
                action.Initialise(_source.Profile.KeyboardContext);
            }

            return action;
        }

        /// <summary>
        /// Make the specified action grid visible and hide the others
        /// </summary>
        /// <param name="grid"></param>
        private void ShowActionGrid(FrameworkElement gridToShow)
        {
            if (_visibleActionGrid != null)
            {
                _visibleActionGrid.Visibility = Visibility.Hidden;
            }
            if (gridToShow != null)
            {
                gridToShow.Visibility = Visibility.Visible;
            }
            _visibleActionGrid = gridToShow;
        }

        /// <summary>
        /// Refresh the event reason radio buttons when the input source or control changes
        /// </summary>
        private void RefreshEventReasons()
        {
            ActionSet actionSet = null;
            List<EEventReason> supportedReasons = null;
            if (_currentSituation != null && _currentInputControl != null && _source != null)
            {
                // Get action set for this event
                actionSet = _source.Actions.GetActionsForInputControl(_currentSituation, _currentInputControl, false);

                // Get supported reasons
                BaseControl control = _source.GetVirtualControl(_currentInputControl);
                if (control != null)
                {                    
                    ELRUDState direction = _currentInputControl.LRUDState;
                    this.ReasonDirected.Content = _utils.GetEventDescription(EEventReason.Directed, direction);
                    this.ReasonUndirected.Content = _utils.GetEventDescription(EEventReason.Undirected, direction);
                    supportedReasons = control.GetSupportedEventReasons(_currentInputControl/*, directionMode*/);
                }
            }

            // Enable / disable radio buttons
            Dictionary<EEventReason, RadioButton>.Enumerator eRadio = _eventReasonButtons.GetEnumerator();
            while (eRadio.MoveNext())
            {
                EEventReason reason = eRadio.Current.Key;
                RadioButton button = eRadio.Current.Value;

                bool show = false;
                bool highlight = false;
                if (supportedReasons != null)
                {
                    switch (reason)
                    {
                        case EEventReason.Activated:
                        //case EEventReason.Deactivated:
                        //case EEventReason.Dwelled:
                        case EEventReason.Moved:
                        case EEventReason.Released:
                        case EEventReason.Undirected:
                            show = supportedReasons.Contains(reason);
                            highlight = (actionSet != null) && actionSet.HasActionsForReason(reason);
                            break;
                        case EEventReason.Directed:
                            show = (supportedReasons.Contains(EEventReason.Directed) ||
                                    supportedReasons.Contains(EEventReason.DirectedLong) ||
                                    supportedReasons.Contains(EEventReason.DirectedShort) ||
                                    supportedReasons.Contains(EEventReason.DirectionRepeated));
                            if (actionSet != null)
                            {
                                highlight = actionSet.HasActionsForReason(EEventReason.Directed) ||
                                    actionSet.HasActionsForReason(EEventReason.DirectedLong) ||
                                    actionSet.HasActionsForReason(EEventReason.DirectedShort) ||
                                    actionSet.HasActionsForReason(EEventReason.DirectionRepeated);
                            }
                            break;
                        case EEventReason.Pressed:
                            show = supportedReasons != null &&
                                    (supportedReasons.Contains(EEventReason.Pressed) ||
                                    supportedReasons.Contains(EEventReason.PressedLong) ||
                                    supportedReasons.Contains(EEventReason.PressedShort) ||
                                    supportedReasons.Contains(EEventReason.PressRepeated));
                            if (actionSet != null)
                            {
                                highlight = actionSet.HasActionsForReason(EEventReason.Pressed) ||
                                    actionSet.HasActionsForReason(EEventReason.PressedLong) ||
                                    actionSet.HasActionsForReason(EEventReason.PressedShort) ||
                                    actionSet.HasActionsForReason(EEventReason.PressRepeated);
                            }
                            break;
                    }
                }

                button.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                if (!show && button.IsChecked == true)
                {
                    // If the currently checked button is now disabled, uncheck it
                    _currentReason = EEventReason.None;
                    button.IsChecked = false;
                }

                HighlightRadioButton(button, highlight);
            }

            // If no reason is selected, select the first reason that is supported
            if (_currentReason == EEventReason.None &&
                supportedReasons != null &&
                supportedReasons.Count > 0)
            {
                SetSelectedEventReason(supportedReasons[0]);
            }
        }

        /// <summary>
        /// Enable or disable press type buttons            
        /// </summary>
        private void RefreshPressTypes()
        {
            bool showPressType = false;
            if (this.ReasonPressed.IsChecked == true || this.ReasonDirected.IsChecked == true)
            {
                ActionSet actionSet = null;
                List<EEventReason> supportedReasons = null;
                if (_currentSituation != null && _currentInputControl != null && _source != null)
                {
                    // Get action set for this event
                    actionSet = _source.Actions.GetActionsForInputControl(_currentSituation, _currentInputControl, false);

                    // Get supported reasons
                    BaseControl control = _source.GetVirtualControl(_currentInputControl);
                    if (control != null)
                    {
                        supportedReasons = control.GetSupportedEventReasons(_currentInputControl);
                    }
                }

                if (supportedReasons != null)
                {
                    if (this.ReasonPressed.IsChecked == true)
                    {
                        showPressType = supportedReasons.Contains(EEventReason.PressedLong) ||
                                                supportedReasons.Contains(EEventReason.PressedShort) ||
                                                supportedReasons.Contains(EEventReason.PressRepeated);
                        if (showPressType)
                        {
                            HighlightRadioButton(this.PressTypeAny, (actionSet != null) && actionSet.HasActionsForReason(EEventReason.Pressed));
                            HighlightRadioButton(this.PressTypeLong, (actionSet != null) && actionSet.HasActionsForReason(EEventReason.PressedLong));
                            HighlightRadioButton(this.PressTypeShort, (actionSet != null) && actionSet.HasActionsForReason(EEventReason.PressedShort));
                            HighlightRadioButton(this.PressTypeAutoRepeat, (actionSet != null) && actionSet.HasActionsForReason(EEventReason.PressRepeated));
                        }
                    }
                    else if (this.ReasonDirected.IsChecked == true)
                    {
                        showPressType = supportedReasons.Contains(EEventReason.DirectedLong) ||
                                                supportedReasons.Contains(EEventReason.DirectedShort) ||
                                                supportedReasons.Contains(EEventReason.DirectionRepeated);
                        if (showPressType)
                        {
                            HighlightRadioButton(this.PressTypeAny, (actionSet != null) && actionSet.HasActionsForReason(EEventReason.Directed));
                            HighlightRadioButton(this.PressTypeLong, (actionSet != null) && actionSet.HasActionsForReason(EEventReason.DirectedLong));
                            HighlightRadioButton(this.PressTypeShort, (actionSet != null) && actionSet.HasActionsForReason(EEventReason.DirectedShort));
                            HighlightRadioButton(this.PressTypeAutoRepeat, (actionSet != null) && actionSet.HasActionsForReason(EEventReason.DirectionRepeated));
                        }
                    }
                }
            }

            if (showPressType)
            {
                this.PressTypeTextBlock.Visibility = Visibility.Visible;
                this.PressTypePanel.Visibility = Visibility.Visible;
            }
            else
            {
                PressTypeAny.IsChecked = true;
                this.PressTypeTextBlock.Visibility = Visibility.Hidden;
                this.PressTypePanel.Visibility = Visibility.Hidden;
            }            
        }

        private void HighlightRadioButton(RadioButton button, bool highlight)
        {
            button.FontWeight = highlight ? FontWeights.Bold : FontWeights.Normal;
        }

        private void TryToSelectAReasonWithActions()
        {
            if (_currentSituation != null && _currentInputControl != null && _source != null)
            {
                // Get action set for this event
                ActionSet actionSet = _source.Actions.GetActionsForInputControl(_currentSituation, _currentInputControl, false);
                if (actionSet != null)
                {
                    // See if current reason has actions
                    bool currentReasonHasActions = actionSet.HasActionsForReason(_currentReason);
                    if (!currentReasonHasActions)
                    {
                        BaseControl control = _source.GetVirtualControl(_currentInputControl);
                        if (control != null)
                        {
                            List<EEventReason> supportedReasons = control.GetSupportedEventReasons(_currentInputControl);
                            foreach (EEventReason reason in supportedReasons)
                            {
                                if (actionSet.HasActionsForReason(reason))
                                {
                                    SetSelectedEventReason(reason);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Select event reason and press type radio buttons
        /// </summary>
        /// <param name="reason"></param>
        private void SetSelectedEventReason(EEventReason reason)
        {
            switch (reason)
            {
                case EEventReason.Activated:
                    this.ReasonActivated.IsChecked = true;
                    break;
                //case EEventReason.Deactivated:
                //    this.ReasonDeactivated.IsChecked = true;
                //    break;
                case EEventReason.Directed:
                    this.ReasonDirected.IsChecked = true;
                    this.PressTypeAny.IsChecked = true;
                    break;
                case EEventReason.DirectedLong:
                    this.ReasonDirected.IsChecked = true;
                    this.PressTypeLong.IsChecked = true;
                    break;
                case EEventReason.DirectedShort:
                    this.ReasonDirected.IsChecked = true;
                    this.PressTypeShort.IsChecked = true;
                    break;
                case EEventReason.DirectionRepeated:
                    this.ReasonDirected.IsChecked = true;
                    this.PressTypeAutoRepeat.IsChecked = true;
                    break;
                case EEventReason.Moved:
                    this.ReasonMoved.IsChecked = true;
                    break;
                case EEventReason.Pressed:
                    this.ReasonPressed.IsChecked = true;
                    this.PressTypeAny.IsChecked = true;
                    break;
                case EEventReason.PressedLong:
                    this.ReasonPressed.IsChecked = true;
                    this.PressTypeLong.IsChecked = true;
                    break;
                case EEventReason.PressedShort:
                    this.ReasonPressed.IsChecked = true;
                    this.PressTypeShort.IsChecked = true;
                    break;
                case EEventReason.PressRepeated:
                    this.ReasonPressed.IsChecked = true;
                    this.PressTypeAutoRepeat.IsChecked = true;
                    break;
                case EEventReason.Released:
                    this.ReasonReleased.IsChecked = true;
                    break;
                case EEventReason.Undirected:
                    this.ReasonUndirected.IsChecked = true;
                    break;
            }

            _currentReason = reason;
        }

        /// <summary>
        /// Get the selected event reason radio button
        /// </summary>
        /// <returns></returns>
        private EEventReason GetSelectedEventReason()
        {
            // Get the primary reason, ignoring press type
            EEventReason reason = EEventReason.None;
            Dictionary<EEventReason, RadioButton>.Enumerator eReason = _eventReasonButtons.GetEnumerator();
            while (eReason.MoveNext())
            {
                if (eReason.Current.Value.IsChecked == true)
                {
                    reason = eReason.Current.Key;
                    break;
                }
            }

            // For pressed and directed buttons, adjust according to the selected press type
            if (reason == EEventReason.Directed || reason == EEventReason.Pressed)
            {
                EPressType pressType = EPressType.Any;
                Dictionary<EPressType, RadioButton>.Enumerator ePressType = _pressTypeButtons.GetEnumerator();
                while (ePressType.MoveNext())
                {
                    if (ePressType.Current.Value.IsChecked == true)
                    {
                        pressType = ePressType.Current.Key;
                        break;
                    }
                }

                switch (pressType)
                {
                    case EPressType.Short:
                        reason = (reason == EEventReason.Directed) ? EEventReason.DirectedShort : EEventReason.PressedShort;
                        break;
                    case EPressType.Long:
                        reason = (reason == EEventReason.Directed) ? EEventReason.DirectedLong : EEventReason.PressedLong;
                        break;
                    case EPressType.AutoRepeat:
                        reason = (reason == EEventReason.Directed) ? EEventReason.DirectionRepeated : EEventReason.PressRepeated;
                        break;
                }
            }

            return reason;
        }

        /// <summary>
        /// Allow the user to customise the tiny description of the action set
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayText_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_currentSituation != null && _currentInputControl != null && _source != null)
            {
                try
                {
                    ErrorMessage.Clear();

                    // Get action list for this event
                    ActionSet actionSet = _source.Actions.GetActionsForInputControl(_currentSituation, _currentInputControl, false);
                    if (actionSet != null)
                    {
                        // Update the display text for the action set
                        string alternativeText = this.DisplayText.Text;
                        if (alternativeText == actionSet.Info.TinyDescription ||
                            (alternativeText == "" && !this.DisplayIconPanel.IsEnabled))
                        {
                            alternativeText = null;
                        }
                        if (alternativeText != actionSet.AlternativeText)
                        {
                            actionSet.AlternativeText = alternativeText;
                            actionSet.Updated();
                            _source.Actions.ActionSetUpdated(actionSet);

                            _parentWindow.ActionsEdited();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_ParseDisplayText, ex);
                }
            }
        }

        /// <summary>
        /// Allow user to choose the display icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayIconPicker_SelectionChanged(object sender, EventArgs e)
        {
            if (_currentSituation != null && _currentInputControl != null && _source != null)
            {
                try
                {
                    ErrorMessage.Clear();

                    // Get action list for this event
                    ActionSet actionSet = _source.Actions.GetActionsForInputControl(_currentSituation, _currentInputControl, false);
                    if (actionSet != null)
                    {
                        // Update the display icon for the action set
                        EAnnotationImage alternativeIcon = this.DisplayIconPicker.SelectedIcon;
                        if (alternativeIcon == actionSet.Info.IconRef)
                        {
                            alternativeIcon = EAnnotationImage.None;
                        }
                        if (alternativeIcon != actionSet.AlternativeIcon)
                        {
                            actionSet.AlternativeIcon = alternativeIcon;
                            actionSet.Updated();
                            _source.Actions.ActionSetUpdated(actionSet);

                            _parentWindow.ActionsEdited();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_SelectionChange, ex);
                }
            }
        }
    }
}
