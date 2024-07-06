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
using Keysticks.Config;
using Keysticks.Controls;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Sources;
using Keysticks.UI;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Summary panel which shows a description of the actions for the currently selected control
    /// and provides editing buttons
    /// </summary>
    public partial class ActionsSummaryControl : KxUserControl, ISourceViewerControl
    {
        // Fields
        private bool _isLoaded;
        private AppConfig _appConfig;
        private BaseSource _source;
        private StateVector _selectedState;
        private KxControlEventArgs _selectedControl;
        private StringUtils _utils = new StringUtils();

        /// <summary>
        /// Constructor
        /// </summary>
        public ActionsSummaryControl()
            :base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set the application configuration
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        /// <summary>
        /// Set the profile
        /// </summary>
        /// <param name="profile"></param>
        public void SetSource(BaseSource source)
        {
            _source = source;

            _selectedState = null;
            _selectedControl = null;
        }

        /// <summary>
        /// Set the situation to display
        /// </summary>
        /// <param name="situation"></param>
        public void SetCurrentSituation(StateVector situation)
        {            
            _selectedState = situation;

            RefreshDisplay();
        }

        /// <summary>
        /// Set the selected control
        /// </summary>
        /// <param name="control"></param>
        public void SetCurrentControl(KxControlEventArgs control)
        {
            _selectedControl = control;

            RefreshDisplay();
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KxUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;
            RefreshDisplay();
        }
        
        /// <summary>
        /// Edit the actions shown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditButton_Click(object sender, RoutedEventArgs e)
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
                    ReportError(Properties.Resources.E_EditActions, ex);
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Quick edit button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuickEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedState != null && _selectedControl != null)
            {
                try
                {
                    // Tell profile designer to show context menu
                    RaiseEvent(new RoutedEventArgs(KxQuickEditActionsEvent));
                }
                catch (Exception ex)
                {
                    ReportError(Properties.Resources.E_OpenQuickEdit, ex);
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Override the actions shown that are from a less-specific situation (i.e. default)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OverrideButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedState != null)
            {
                try
                {
                    // Edit the actions for the selected state, overriding the inherited actions 
                    RaiseEvent(new KxStateRoutedEventArgs(KxEditActionsEvent, _selectedState));
                }
                catch (Exception ex)
                {
                    ReportError(Properties.Resources.E_OverrideActions, ex);
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Switch to the situation from which actions are inherited for the current control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InheritedActionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedState != null && _selectedControl != null)
            {
                try
                {
                    // Get action set for this state and control
                    ActionSet actionSet = _source.Actions.GetActionsForInputControl(_selectedState, _selectedControl, true);
                    if (actionSet != null)
                    {
                        RaiseEvent(new KxStateRoutedEventArgs(KxStateChangedEvent, actionSet.LogicalState));
                    }
                }
                catch (Exception ex)
                {
                    ReportError(Properties.Resources.E_ViewInheritedActions, ex);
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Refresh the actions list
        /// </summary>
        public void RefreshDisplay()
        {
            ActionSet actionSet = null;
            if (_isLoaded && _source != null && _selectedState != null && _selectedControl != null)
            {
                // Get the input control
                BaseControl control = _source.GetVirtualControl(_selectedControl);                

                string controlName;
                if (control != null)
                {
                    // Add direction qualifier if reqd
                    ELRUDState direction = _selectedControl.LRUDState;
                    if (direction != ELRUDState.None)
                    {
                        controlName = string.Format("{0} ({1})", control.Name, _utils.DirectionToString(direction));
                    }
                    else
                    {
                        controlName = control.Name;
                    }
                }
                else
                {
                    controlName = Properties.Resources.String_NotKnownAbbrev;
                }

                // Set the group box header
                string header = string.Format("{0} {1}", Properties.Resources.String_ActionsForLabel, controlName);
                this.ActionsGroupBox.Header = header;

                // Get actions for this control
                actionSet = _source.Actions.GetActionsForInputControl(_selectedState, _selectedControl, true);

                this.EditButton.IsEnabled = true;
                this.QuickEditButton.IsEnabled = true;
            }
            else
            {
                // Set the group box header
                this.ActionsGroupBox.Header = string.Format("{0} {1}", Properties.Resources.String_ActionsForLabel, Properties.Resources.String_NoControlSelected);
                this.EditButton.IsEnabled = false;
                this.QuickEditButton.IsEnabled = false;
            }

            // Display summary and update action editor if required
            if (actionSet != null)
            {
                this.ActionsSummaryText.Text = actionSet.Info.Description;
            }
            else
            {
                string summary;
                if (_selectedControl == null)
                {
                    summary = Properties.Resources.Info_SelectAControl;
                }
                else if (IsControlReservedForKeyboardSelection(_selectedState, _selectedControl))
                {
                    summary = Properties.Resources.Info_PerformsActionsForCell;
                }
                else
                {
                    summary = Properties.Resources.Info_NoActionsAssigned;
                }
                this.ActionsSummaryText.Text = summary;
            }

            // Handle actions inherited from less-specific situation
            if (actionSet != null && !actionSet.LogicalState.IsSameAs(_selectedState))
            {
                this.InheritedActionsButton.Content = _source.Utils.GetAbsoluteSituationName(actionSet.LogicalState);
                this.InheritedActionsPanel.Visibility = System.Windows.Visibility.Visible;
                this.OverrideButton.IsEnabled = true;
            }
            else
            {
                this.InheritedActionsPanel.Visibility = System.Windows.Visibility.Hidden;
                this.OverrideButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// See whether the control is used for selecting keyboard actions
        /// </summary>
        /// <param name="inputControl"></param>
        /// <returns></returns>
        private bool IsControlReservedForKeyboardSelection(StateVector situation, KxControlEventArgs inputControl)
        {
            bool isSelectionControl = false;
            if (/*inputControl is KxControllerEventArgs &&*/
                situation != null &&
                situation.CellID == Constants.DefaultID)
            {
                AxisValue modeItem = _source.Utils.GetModeItem(situation);
                if (modeItem != null && modeItem.GridType != EGridType.None && modeItem.Controls != null)
                {
                    GeneralisedControl selectionControl = modeItem.Controls.SelectionControl;
                    if (_source.Utils.IsGeneralTypeOfControl(inputControl, selectionControl))
                    {
                        isSelectionControl = true;
                    }
                }
            }

            return isSelectionControl;
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
