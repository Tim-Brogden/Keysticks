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
using Keysticks.Config;
using Keysticks.Controls;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Sources;
using Keysticks.UI;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Context menu for editing actions
    /// </summary>
    public partial class EditActionsContextMenu : KxUserControl
    {
        // Fields
        private IProfileEditorWindow _editorWindow;
        private AppConfig _appConfig;
        private Profile _templatesProfile;
        private BaseSource _source;
        private StateVector _selectedState;
        private KxControlEventArgs _selectedControl;
        private StringUtils _utils = new StringUtils();

        /// <summary>
        /// Constructor
        /// </summary>
        public EditActionsContextMenu()
            : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set the app configuration
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        /// <summary>
        /// Set the profile to edit
        /// </summary>
        /// <param name="profile"></param>
        public void SetSource(BaseSource source)
        {
            _source = source;
        }

        /// <summary>
        /// Set the profile editor window to report to
        /// </summary>
        /// <param name="profileEditor"></param>
        public void SetProfileEditor(IProfileEditorWindow editorWindow)
        {
            _editorWindow = editorWindow;

            // Load templates
            string filePath = System.IO.Path.Combine(AppConfig.CommonAppDataDir, "Templates", Constants.QuickEditTemplatesName + Constants.TemplateFileExtension);
            _templatesProfile = editorWindow.LoadTemplates(filePath);

            // Create the context menu
            CreateQuickEditMenu();
        }

        /// <summary>
        /// Set the situation to edit
        /// </summary>
        /// <param name="situation"></param>
        public void SetState(StateVector state)
        {
            _selectedState = state;
        }

        /// <summary>
        /// Set the control to edit
        /// </summary>
        /// <param name="control"></param>
        public void SetControl(KxControlEventArgs control)
        {
            _selectedControl = control;
        }

        /// <summary>
        /// Handle input language change
        /// </summary>
        public void KeyboardLayoutChanged(KxKeyboardChangeEventArgs args)
        {
            if (_templatesProfile != null)
            {
                _templatesProfile.Initialise(args);

                // Update mode names according to current input language
                _templatesProfile.SetKeyboardSpecificControlSetNames(args);

                // Create the context menu
                CreateQuickEditMenu();
            }
        }

        /// <summary>
        /// Menu opening
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ShowContextMenu(UIElement placementControl)
        {
            if (_selectedState != null && _selectedControl != null)
            {
                // Get the direction modes and types of template supported by this control
                List<ETemplateGroup> groupsToInclude = new List<ETemplateGroup>();
                if (_selectedControl.SettingType == EControlSetting.DirectionMode)
                {
                    groupsToInclude.Add(ETemplateGroup.DirectionMode);
                }
                else if (_selectedControl.SettingType == EControlSetting.DwellAndRepeat)
                {
                    groupsToInclude.Add(ETemplateGroup.Timing);
                }
                else
                {
                    foreach (ETemplateGroup group in Enum.GetValues(typeof(ETemplateGroup)))
                    {
                        if (group != ETemplateGroup.None &&
                            group != ETemplateGroup.ControlSets &&
                            group != ETemplateGroup.Timing)
                        {
                            groupsToInclude.Add(group);
                        }
                    }
                }

                AxisValue modeItem = (AxisValue)_source.Utils.GetModeItem(_selectedState);

                // Get which type of templates to allow
                // If a non-directional keyboard cell selected, only allow non-directional templates
                EDirectionMode supportedDirections = EDirectionMode.NonDirectional;
                if (_selectedState.CellID == Constants.DefaultID ||
                    (modeItem.GridType != EGridType.ActionStrip && modeItem.GridType != EGridType.Keyboard))
                {
                    supportedDirections = _source.Utils.GetSupportedDirectionTypes(_selectedControl);
                }

                // Show or hide items
                foreach (MenuItem menuItem in TheContextMenu.Items)
                {
                    ShowOrHideMenuItem(menuItem, groupsToInclude, _selectedControl, supportedDirections);
                }

                // Always show Edit actions option (penultimate item)
                MenuItem editItem = (MenuItem)TheContextMenu.Items[TheContextMenu.Items.Count - 2];
                editItem.Visibility = Visibility.Visible;

                // If the control has actions or settings assigned to this situation, show the "Clear" option (last item)
                MenuItem clearItem = (MenuItem)TheContextMenu.Items[TheContextMenu.Items.Count - 1];
                ActionSet actionSet = _source.Actions.GetActionsForInputControl(_selectedState, _selectedControl, true);
                if (actionSet != null)
                {
                    clearItem.Visibility = Visibility.Visible;
                    editItem.Header = Properties.Resources.String_EditActions + "...";
                }
                else
                {
                    clearItem.Visibility = Visibility.Collapsed;
                    editItem.Header = Properties.Resources.String_CustomActions + "...";
                }

                TheContextMenu.PlacementTarget = (UIElement)placementControl;
                TheContextMenu.IsOpen = true;
            }
        }

        /// <summary>
        /// Show or hide a quick edit menu item
        /// </summary>
        /// <param name="menuItem"></param>
        /// <param name="groupsToInclude"></param>
        /// <param name="inputControl"></param>
        /// <param name="supportedDirections"></param>
        /// <returns></returns>
        private bool ShowOrHideMenuItem(MenuItem menuItem,
                                            List<ETemplateGroup> groupsToInclude,
                                            KxControlEventArgs inputControl,
                                            EDirectionMode supportedDirections)
        {
            bool isVisible = false;
            if (menuItem.HasItems)
            {
                // Sub menu
                foreach (MenuItem childItem in menuItem.Items)
                {
                    isVisible |= ShowOrHideMenuItem(childItem, groupsToInclude, inputControl, supportedDirections);
                }
            }
            else
            {
                // Leaf node (template)
                if (menuItem.Tag is AxisValue)
                {
                    isVisible = IsTemplateApplicable((AxisValue)menuItem.Tag, groupsToInclude, inputControl, supportedDirections);
                }
            }

            // Show if any sub items are visible
            menuItem.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;

            return isVisible;
        }

        /// <summary>
        /// Decide whether or not to show a template menu item
        /// </summary>
        /// <param name="templateMode"></param>
        /// <returns></returns>
        private bool IsTemplateApplicable(AxisValue templateMode, List<ETemplateGroup> groupsToInclude, KxControlEventArgs inputControl, EDirectionMode supportedDirections)
        {
            bool isValid = false;
            if (groupsToInclude.Contains(templateMode.TemplateGroup))
            {
                // Template is for navigation or selection but not both
                bool isNavigationTemplate = templateMode.Controls.NavigationControl.DirectionMode != EDirectionMode.None;
                GeneralisedControl fromControl = isNavigationTemplate ? templateMode.Controls.NavigationControl : templateMode.Controls.SelectionControl;
                if ((fromControl.DirectionMode & supportedDirections) != 0)
                {
                    isValid = true;
                    
                    // Assume templates are defined for player 1 only
                    GeneralisedControl toControl = new GeneralisedControl(fromControl.DirectionMode, inputControl);
                    BaseSource templateSource = _templatesProfile.GetSource(Constants.ID1);
                    if (templateSource != null)
                    {
                        ActionMappingTable actionMapping = templateSource.Actions.GetActionsForState(templateMode.Situation, false);
                        Dictionary<int, ActionSet>.Enumerator eActionSet = actionMapping.GetEnumerator();
                        while (eActionSet.MoveNext())
                        {
                            if (!_source.ActionEditor.CanConvertActionSet(eActionSet.Current.Value,
                                                                    fromControl,
                                                                    _selectedState,
                                                                    toControl))
                            {
                                isValid = false;
                                break;
                            }
                        }
                    }
                }
            }

            return isValid;
        }

        /// <summary>
        /// Create the quick edit context menu
        /// </summary>
        /// <returns></returns>
        private void CreateQuickEditMenu()
        {
            // Close / clear the menu if necessary
            if (TheContextMenu.IsOpen)
            {
                TheContextMenu.IsOpen = false;
            }
            TheContextMenu.Items.Clear();

            if (_templatesProfile != null)
            {
                // Get a table of templates
                // Assume templates are defined for player 1 only
                Dictionary<ETemplateGroup, List<AxisValue>> templateGroups = new Dictionary<ETemplateGroup, List<AxisValue>>();
                BaseSource templateSource = _templatesProfile.GetSource(Constants.ID1);
                if (templateSource != null)
                {
                    foreach (AxisValue templateMode in templateSource.StateTree.SubValues)
                    {
                        if (templateMode.Controls != null)
                        {
                            bool isNavigationTemplate = templateMode.Controls.NavigationControl.DirectionMode != EDirectionMode.None;
                            bool isSelectionTemplate = templateMode.Controls.SelectionControl.DirectionMode != EDirectionMode.None;
                            if (isNavigationTemplate != isSelectionTemplate)
                            {
                                List<AxisValue> templateList;
                                if (templateGroups.ContainsKey(templateMode.TemplateGroup))
                                {
                                    templateList = templateGroups[templateMode.TemplateGroup];
                                }
                                else
                                {
                                    templateList = new List<AxisValue>();
                                    templateGroups[templateMode.TemplateGroup] = templateList;
                                }

                                templateList.Add(templateMode);
                            }
                        }
                    }
                }

                // Create a context menu with the allowed templates
                MenuItem holdSubMenu = null;
                MenuItem autorepeatSubMenu = null;
                MenuItem typingSubMenu = null;
                //MenuItem virtualSubMenu = null;
                foreach (ETemplateGroup group in Enum.GetValues(typeof(ETemplateGroup)))
                {
                    if (templateGroups.ContainsKey(group) &&
                        group != ETemplateGroup.None &&
                        group != ETemplateGroup.ControlSets)
                    {
                        // Create the template group
                        MenuItem groupMenuItem = new MenuItem();
                        groupMenuItem.Header = _utils.TemplateGroupToString(group);
                        System.Windows.Media.Imaging.BitmapImage bitmap = GUIUtils.FindIcon(GUIUtils.GetIconForTemplateGroup(group));
                        if (bitmap != null)
                        {
                            groupMenuItem.Icon = new System.Windows.Controls.Image { Source = bitmap };
                        }

                        // Decide whether to add it to the main menu or to a submenu
                        ItemCollection parentCollection;
                        switch (group)
                        {
                            case ETemplateGroup.HoldLetterKey:
                            case ETemplateGroup.HoldNumberKey:
                            case ETemplateGroup.HoldSymbolKey:
                            case ETemplateGroup.HoldArrowKey:
                            case ETemplateGroup.HoldFunctionKey:
                            case ETemplateGroup.HoldNumpadKey:
                            case ETemplateGroup.HoldOtherKey:
                                {
                                    if (holdSubMenu == null)
                                    {
                                        holdSubMenu = new MenuItem();
                                        holdSubMenu.Header = Properties.Resources.String_HoldKey;
                                        System.Windows.Media.Imaging.BitmapImage icon = GUIUtils.FindIcon(EAnnotationImage.HoldLetterKey);
                                        if (icon != null)
                                        {
                                            holdSubMenu.Icon = new System.Windows.Controls.Image { Source = icon };
                                        }
                                        TheContextMenu.Items.Add(holdSubMenu);
                                    }
                                    parentCollection = holdSubMenu.Items;
                                }
                                break;
                            case ETemplateGroup.TypeLetterKey:
                            case ETemplateGroup.TypeShiftedLetterKey:
                            case ETemplateGroup.TypeNumberKey:
                            case ETemplateGroup.TypeShiftedNumberKey:
                            case ETemplateGroup.TypeSymbolKey:
                            case ETemplateGroup.TypeShiftedSymbolKey:
                            case ETemplateGroup.TypeArrowKey:
                            case ETemplateGroup.TypeFunctionKey:
                            case ETemplateGroup.TypeNumpadKey:
                            case ETemplateGroup.TypeOtherKey:
                                {
                                    if (typingSubMenu == null)
                                    {
                                        typingSubMenu = new MenuItem();
                                        typingSubMenu.Header = Properties.Resources.String_TypeKey;
                                        System.Windows.Media.Imaging.BitmapImage icon = GUIUtils.FindIcon(EAnnotationImage.TypeShiftedLetterKey);
                                        if (icon != null)
                                        {
                                            typingSubMenu.Icon = new System.Windows.Controls.Image { Source = icon };
                                        }
                                        TheContextMenu.Items.Add(typingSubMenu);
                                    }
                                    parentCollection = typingSubMenu.Items;
                                }
                                break;
                            case ETemplateGroup.AutorepeatLetterKey:
                            case ETemplateGroup.AutorepeatNumberKey:
                            case ETemplateGroup.AutorepeatSymbolKey:
                            case ETemplateGroup.AutorepeatArrowKey:
                            case ETemplateGroup.AutorepeatFunctionKey:
                            case ETemplateGroup.AutorepeatNumpadKey:
                            case ETemplateGroup.AutorepeatOtherKey:
                                {
                                    if (autorepeatSubMenu == null)
                                    {
                                        autorepeatSubMenu = new MenuItem();
                                        autorepeatSubMenu.Header = Properties.Resources.String_AutoRepeatKey;
                                        System.Windows.Media.Imaging.BitmapImage icon = GUIUtils.FindIcon(EAnnotationImage.AutorepeatLetterKey);
                                        if (icon != null)
                                        {
                                            autorepeatSubMenu.Icon = new System.Windows.Controls.Image { Source = icon };
                                        }
                                        TheContextMenu.Items.Add(autorepeatSubMenu);
                                    }
                                    parentCollection = autorepeatSubMenu.Items;
                                }
                                break;
                            default:
                                parentCollection = TheContextMenu.Items;
                                break;
                        }

                        // Add the group to the menu
                        parentCollection.Add(groupMenuItem);

                        // Add the templates to the group
                        List<AxisValue> templateList = templateGroups[group];
                        foreach (AxisValue templateMode in templateList)
                        {
                            MenuItem menuItem = new MenuItem();
                            string header = templateMode.Name;
                            // Fudge for "Type key" template names
                            if (header.EndsWith(" key"))
                            {
                                header = header.Substring(0, header.Length - 4);
                            }
                            menuItem.Header = header;
                            //menuItem.Icon = new System.Windows.Controls.Image { Source = bulletBitmap };
                            menuItem.Tag = templateMode;
                            menuItem.Click += this.QuickEditMenuItem_Click;
                            groupMenuItem.Items.Add(menuItem);
                        }
                    }
                }
            }

            // Add a "Edit actions" option
            // Note: Other code in this module relies on this being the penultimate item in the menu
            MenuItem editItem = new MenuItem();
            editItem.Header = Properties.Resources.String_EditActions + "...";
            editItem.Click += this.EditActions_Click;
            TheContextMenu.Items.Add(editItem);

            // Add a "Clear" option
            // Note: Other code in this module relies on this being the last item in the menu
            MenuItem clearItem = new MenuItem();
            clearItem.Header = Properties.Resources.String_ClearActions;
            clearItem.InputGestureText = "Del";
            clearItem.Click += this.ClearActions_Click;
            TheContextMenu.Items.Add(clearItem);
        }

        /// <summary>
        /// Handle click on Custom Edit in context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditActions_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedState != null && _selectedControl != null)
            {
                try
                {
                    // Get action set for this state and control
                    ActionSet actionSet = _source.Actions.GetActionsForInputControl(_selectedState, _selectedControl, true);
                    StateVector stateToEdit;
                    if (actionSet != null)
                    {
                        // Let the user edit the displayed actions, which may be inherited from a higher situation
                        stateToEdit = actionSet.LogicalState;
                    }
                    else
                    {
                        // Let the user add new actions for the selected state
                        stateToEdit = _selectedState;
                    }
                    RaiseEvent(new KxStateRoutedEventArgs(KxEditActionsEvent, stateToEdit));
                }
                catch (Exception ex)
                {
                    ReportError(Properties.Resources.E_EditCustomActions, ex);
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle click on item in Quick Edit context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuickEditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem menuItem = (MenuItem)sender;
                AxisValue templateMode = (AxisValue)menuItem.Tag;

                // Get the direction mode of the template we're applying
                // The template is expected to be either a navigation or selection template, not both
                EDirectionMode directionMode = EDirectionMode.None;
                if (templateMode.Controls != null)
                {
                    directionMode = templateMode.Controls.NavigationControl.DirectionMode;
                    if (directionMode == EDirectionMode.None)
                    {
                        directionMode = templateMode.Controls.SelectionControl.DirectionMode;
                    }
                }

                // Get the control to import actions for                
                GeneralisedControl genControl = new GeneralisedControl(directionMode, _selectedControl);
                BaseControl control = _source.GetVirtualControl(_selectedControl);

                // Confirm if required
                bool confirmed = _appConfig.GetBoolVal(Constants.ConfigAutoConfirmApplyTemplates, Constants.DefaultAutoConfirmApplyTemplates);
                if (!confirmed)
                {
                    string message = string.Format(Properties.Resources.Q_ApplyTemplateMessage,
                                                    templateMode.Name,
                                                    control != null ? control.Name : Properties.Resources.String_NotKnownAbbrev, // genControl.ToString(),
                                                    _source.Utils.GetAbsoluteSituationName(_selectedState));

                    CustomMessageBox messageBox = new CustomMessageBox((Window)_editorWindow, message, Properties.Resources.Q_ApplyTemplate, MessageBoxButton.OKCancel, true, true);
                    if (messageBox.ShowDialog() == true)
                    {
                        confirmed = true;
                        if (messageBox.DontAskAgain)
                        {
                            // User clicked OK and chose not to be asked again
                            _appConfig.SetBoolVal(Constants.ConfigAutoConfirmApplyTemplates, true);
                        }
                    }
                }

                if (confirmed)
                {
                    // Import template actions
                    // Assume templates are defined for player 1 only
                    BaseSource templateSource = _templatesProfile.GetSource(Constants.ID1);
                    if (templateSource != null)
                    {
                        ControlsDefinition toControls = new ControlsDefinition(genControl, genControl, EControlRestrictions.None);
                        _source.ActionEditor.ImportActions(templateSource,
                                                    templateMode.Situation,
                                                    templateMode.Controls,
                                                    _selectedState,
                                                    toControls);                        

                        // Update UI
                        RoutedEventArgs args = new RoutedEventArgs(KxUserControl.KxActionsEditedEvent);
                        RaiseEvent(args);
                    }
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_ApplyQuickEdit, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle click on item in Quick Edit context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearActions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearActions();
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_ClearActions, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Clear actions from the selected control
        /// </summary>
        public void ClearActions()
        {
            ActionSet actionSet = null;
            BaseControl control = null;
            if (_selectedState != null && _selectedControl != null)
            {
                // Get the actions to remove
                actionSet = _source.Actions.GetActionsForInputControl(_selectedState, _selectedControl, true);
                control = _source.GetVirtualControl(_selectedControl);
            }

            if (actionSet != null)
            {                
                // Confirm if required
                bool confirmed = _appConfig.GetBoolVal(Constants.ConfigAutoConfirmClearActions, Constants.DefaultAutoConfirmClearActions);
                if (!confirmed)
                {
                    string message;
                    string caption;
                    if (_selectedControl.SettingType == EControlSetting.None)
                    {
                        message = string.Format(Properties.Resources.Q_ClearActionsMessage,
                                                    control != null ? control.Name : Properties.Resources.String_NotKnownAbbrev,
                                                    _source.Utils.GetAbsoluteSituationName(actionSet.LogicalState));
                        caption = Properties.Resources.Q_ClearActions;
                    }
                    else
                    {
                        message = string.Format(Properties.Resources.Q_ClearSettingMessage,
                                                    control != null ? control.Name : Properties.Resources.String_NotKnownAbbrev,
                                                    _source.Utils.GetAbsoluteSituationName(actionSet.LogicalState));
                        caption = Properties.Resources.Q_ClearSetting;
                    }
                    CustomMessageBox messageBox = new CustomMessageBox((Window)_editorWindow, message, caption, MessageBoxButton.OKCancel, true, true);
                    if (messageBox.ShowDialog() == true)
                    {
                        confirmed = true;
                        if (messageBox.DontAskAgain)
                        {
                            _appConfig.SetBoolVal(Constants.ConfigAutoConfirmClearActions, true);
                        }
                    }
                }

                if (confirmed)
                {
                    // Remove the actions
                    _source.Actions.RemoveActionSet(actionSet);
                    _source.Actions.Validate();

                    // Update UI
                    RoutedEventArgs args = new RoutedEventArgs(KxUserControl.KxActionsEditedEvent);
                    RaiseEvent(args);
                }
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
    }
}
