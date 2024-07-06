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
using System.Collections.Generic;
using Keysticks.Actions;
using Keysticks.Controls;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Sources;

namespace Keysticks.Config
{
    /// <summary>
    /// Utility class for editing a profile (add / replace / copy / delete / import actions)
    /// </summary>
    public class ProfileActionEditor
    {
        private BaseSource _source;
        private int _maxActionListLength = Constants.DefaultMaxActionListLength;
        private StringUtils _utils = new StringUtils();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="profile"></param>
        public ProfileActionEditor(BaseSource source)
        {
            _source = source;
        }

        /// <summary>
        /// Handle change of app config
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            _maxActionListLength = appConfig.GetIntVal(Constants.ConfigMaxActionListLength, Constants.DefaultMaxActionListLength);
        }

        /// <summary>
        /// Add an action to a profile
        /// </summary>
        /// <param name="action"></param>
        /// <param name="situation"></param>
        /// <param name="inputControl"></param>
        /// <param name="reason"></param>
        public void AddAction(BaseAction action, StateVector situation, KxControlEventArgs inputControl, EEventReason reason)
        {
            ActionSet actionSet = _source.Actions.GetActionsForInputControl(situation, inputControl, false);
            if (actionSet == null)
            {
                actionSet = new ActionSet();
                actionSet.LogicalState = new StateVector(situation);
                actionSet.EventArgs = new KxControlEventArgs(inputControl);
                _source.Actions.AddActionSet(actionSet);
            }
            ActionList actionList = actionSet.GetActions(reason);
            if (actionList == null)
            {
                actionList = new ActionList(reason);
                actionSet.SetActions(reason, actionList);
            }

            actionList.Add(action);
            actionSet.Updated();
            _source.Actions.ActionSetUpdated(actionSet);
            _source.Actions.Validate();
        }

        /// <summary>
        /// Check whether an action can be added to an action set
        /// </summary>
        /// <param name="action">Not null</param>
        /// <param name="actionSet">Not null</param>
        /// <param name="reason"></param>
        /// <param name="appConfig"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool CanAddAction(BaseAction newAction, StateVector situation, KxControlEventArgs inputControl, EEventReason reason, AppConfig appConfig, out string errorMessage)
        {
            bool isValid = true;
            errorMessage = "";

            ActionSet actionSet = _source.Actions.GetActionsForInputControl(situation, inputControl, false);
            if (actionSet != null)
            {
                ActionList actionListToChange = actionSet.GetActions(reason);
                if (actionListToChange != null)
                {
                    if (actionListToChange.Count >= _maxActionListLength)
                    {
                        errorMessage = Properties.Resources.E_MaxNumberOfActions;
                        isValid = false;
                    }
                    else if (actionListToChange.Count != 0 &&
                            (reason == EEventReason.DirectionRepeated || reason == EEventReason.PressRepeated))
                    {
                        // Only allow one action if the reason is auto-repeat and the action is a change situation action
                        bool actionListChangesControlSet = newAction.ActionType == EActionType.ChangeControlSet ||
                                                            newAction.ActionType == EActionType.NavigateCells ||
                                                            actionListToChange.HasActionsOfType(EActionType.ChangeControlSet) ||
                                                            actionListToChange.HasActionsOfType(EActionType.NavigateCells);
                        if (actionListChangesControlSet)
                        {
                            errorMessage = Properties.Resources.E_ChangeControlSetWithAutoRepeat;
                            isValid = false;
                        }
                    }
                }

                if (isValid)
                {
                    // Check delete / backspace actions

                    // Get a table of actions by type, and add the new action
                    List<BaseAction> allActionsList = actionSet.GetAllActions();

                    // Add the new action
                    allActionsList.Add(newAction);

                    bool hasDeleteOrBackspace = false;
                    bool hasOtherRelevantActions = false;
                    foreach (BaseAction action in allActionsList)
                    {
                        if (action is BaseKeyAction)
                        {
                            BaseKeyAction bka = (BaseKeyAction)action;
                            VirtualKeyData vk = bka.KeyData;
                            if (vk != null)
                            {
                                if (vk.KeyCode == System.Windows.Forms.Keys.Delete ||
                                    vk.KeyCode == System.Windows.Forms.Keys.Back)
                                {
                                    hasDeleteOrBackspace = true;
                                }
                                else
                                {
                                    hasOtherRelevantActions = true;
                                }
                            }
                        }
                        else if (action.ActionType == EActionType.StartProgram)
                        {
                            hasOtherRelevantActions = true;
                        }
                    }

                    // Don't allow delete or backspace with other non-situation actions
                    if (hasDeleteOrBackspace && hasOtherRelevantActions)
                    {
                        errorMessage = Properties.Resources.E_DeleteOrBackspaceCombo;
                        isValid = false;
                    }
                }
            }       

            return isValid;
        }

        /// <summary>
        /// Remove an action from a profile
        /// </summary>
        /// <param name="actionIndex"></param>
        /// <param name="situation"></param>
        /// <param name="inputControl"></param>
        /// <param name="reason"></param>
        public bool DeleteAction(int actionIndex, StateVector situation, KxControlEventArgs inputControl, EEventReason reason)
        {
            bool deleted = false;
            ActionSet actionSet = _source.Actions.GetActionsForInputControl(situation, inputControl, false);
            if (actionSet != null)
            {
                ActionList actionList = actionSet.GetActions(reason);
                if (actionIndex > -1 && actionList != null && actionIndex < actionList.Count)
                {
                    // Remove action from profile
                    actionList.RemoveAt(actionIndex);
                    deleted = true;

                    if (actionList.Count == 0)
                    {
                        // Remove action list
                        actionSet.SetActions(reason, null);

                        // Delete action set if it's empty
                        if (actionSet.IsEmpty())
                        {
                            _source.Actions.RemoveActionSet(actionSet);
                        }
                    }
                    actionSet.Updated();
                    _source.Actions.ActionSetUpdated(actionSet);
                    _source.Actions.Validate();
                }
            }

            return deleted;
        }

        /// <summary>
        /// Replace an action in the profile
        /// </summary>
        /// <param name="actionIndex"></param>
        /// <param name="action"></param>
        /// <param name="situation"></param>
        /// <param name="inputControl"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public void ReplaceAction(int actionIndex, BaseAction action, StateVector situation, KxControlEventArgs inputControl, EEventReason reason)
        {
            ActionSet actionSet = _source.Actions.GetActionsForInputControl(situation, inputControl, false);
            if (actionSet != null)
            {
                ActionList actionList = actionSet.GetActions(reason);
                if (actionIndex > -1 && actionList != null && actionIndex < actionList.Count)
                {
                    actionList[actionIndex] = action;
                    actionSet.Updated();
                    _source.Actions.ActionSetUpdated(actionSet);
                    _source.Actions.Validate();
                }
            }
        }

        /// <summary>
        /// Check whether an action can be updated
        /// </summary>
        /// <param name="actionIndex"></param>
        /// <param name="action"></param>
        /// <param name="situation"></param>
        /// <param name="inputControl"></param>
        /// <param name="reason"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool CanReplaceAction(int actionIndex, BaseAction updatedAction, StateVector situation, KxControlEventArgs inputControl, EEventReason reason, out string errorMessage)
        {
            bool isValid = true;
            errorMessage = ""; 
            
            ActionList actionList;
            ActionSet actionSet = _source.Actions.GetActionsForInputControl(situation, inputControl, false);
            if (actionSet != null &&
                (actionList = actionSet.GetActions(reason)) != null &&
                actionList.Count > actionIndex)
            {
                if (actionList.Count > 1 &&
                    (reason == EEventReason.DirectionRepeated || reason == EEventReason.PressRepeated))
                {
                    // Only allow one action if the reason is auto-repeat and the action is a change situation action
                    bool actionChangesControlSet = updatedAction.ActionType == EActionType.ChangeControlSet ||
                                                        updatedAction.ActionType == EActionType.NavigateCells;
                    if (actionChangesControlSet)
                    {
                        errorMessage = Properties.Resources.E_ChangeControlSetWithAutoRepeat;
                        isValid = false;
                    }
                }

                if (isValid)
                {
                    // Check delete / backspace actions

                    // Get a table of actions by type, and add the new action
                    List<BaseAction> allActionsList = actionSet.GetAllActions();

                    // Replace current action in this list
                    BaseAction actionToReplace = actionList[actionIndex];
                    allActionsList.Remove(actionToReplace);
                    allActionsList.Add(updatedAction);

                    bool hasDeleteOrBackspace = false;
                    bool hasOtherRelevantActions = false;
                    foreach (BaseAction action in allActionsList)
                    {
                        if (action is BaseKeyAction)
                        {
                            BaseKeyAction bka = (BaseKeyAction)action;
                            VirtualKeyData vk = bka.KeyData;
                            if (vk != null)
                            {
                                if (vk.KeyCode == System.Windows.Forms.Keys.Delete ||
                                    vk.KeyCode == System.Windows.Forms.Keys.Back)
                                {
                                    hasDeleteOrBackspace = true;
                                }
                                else
                                {
                                    hasOtherRelevantActions = true;
                                }
                            }
                        }
                        else if (action.ActionType == EActionType.StartProgram)
                        {
                            hasOtherRelevantActions = true;
                        }
                    }

                    // Don't allow delete or backspace with other non-situation actions
                    if (hasDeleteOrBackspace && hasOtherRelevantActions)
                    {
                        errorMessage = Properties.Resources.E_DeleteOrBackspaceCombo;
                        isValid = false;
                    }
                }
            }
            else
            {
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Copy actions within one situation to another
        /// </summary>
        /// <param name="fromSituation"></param>
        /// <param name="toSituation"></param>
        public void CopyActions(BaseSource fromSource, StateVector fromSituation, StateVector toSituation)
        {
            List<ActionSet> relevantActionSets = fromSource.Actions.GetActionsWithinState(fromSituation);
            foreach (ActionSet actionSet in relevantActionSets)
            {
                StateVector targetSituation = new StateVector(toSituation);
                for (int i = 1; i < 3; i++)   // Preserve page and cell when importing (but not mode)
                {
                    if (targetSituation.GetAxisValue(i) == Constants.DefaultID)
                    {
                        targetSituation.SetAxisValue(i, actionSet.LogicalState.GetAxisValue(i));
                    }
                }

                ActionSet actionSetCopy = new ActionSet(actionSet);
                actionSetCopy.LogicalState = targetSituation;
                actionSetCopy.Initialise(_source.Profile.KeyboardContext);

                _source.Actions.AddActionSet(actionSetCopy);
            }
        }

        /// <summary>
        /// Copies just the actions for the root "All" control set (not recursive)
        /// </summary>
        /// <param name="fromSource"></param>
        public void CopyRootActions(BaseSource fromSource)
        {
            StateVector rootState = StateVector.GetRootSituation();
            ActionMappingTable rootActions = fromSource.Actions.GetActionsForState(rootState, false);
            Dictionary<int, ActionSet>.Enumerator eActionSets = rootActions.GetEnumerator();
            while (eActionSets.MoveNext())
            {
                ActionSet actionSetCopy = new ActionSet(eActionSets.Current.Value);
                actionSetCopy.LogicalState = rootState;
                actionSetCopy.Initialise(_source.Profile.KeyboardContext);

                _source.Actions.AddActionSet(actionSetCopy);
            }
        }
        
        /// <summary>
        /// Import actions from one mode to another, translating the bindings accordingly
        /// </summary>
        /// <param name="fromProfile"></param>
        /// <param name="fromMode"></param>
        /// <param name="toMode"></param>
        public void ImportActions(BaseSource fromSource, 
                                    StateVector fromSituation,
                                    ControlsDefinition fromControls, 
                                    StateVector toSituation,
                                    ControlsDefinition toControls)
        {
            // Import the action sets within the source situation
            List<ActionSet> relevantActionSets = fromSource.Actions.GetActionsWithinState(fromSituation);
            foreach (ActionSet actionSet in relevantActionSets)
            {                
                ImportActionSet(actionSet, fromSituation, fromControls, toSituation, toControls);
            }
            _source.Validate();
        }

        /// <summary>
        /// Import an action set
        /// </summary>
        /// <remarks>The caller should call _profile.ActionsChanged() after calling this method</remarks>
        /// <param name="actionSet"></param>
        /// <param name="fromSituation"></param>
        /// <param name="fromControls"></param>
        /// <param name="toSituation"></param>
        /// <param name="toControls"></param>
        private void ImportActionSet(ActionSet actionSet, 
                                        StateVector fromSituation,
                                        ControlsDefinition fromControls,
                                        StateVector toSituation,
                                        ControlsDefinition toControls)
        {
            StateVector targetSituation = new StateVector(toSituation);
            for (int i = 1; i < 3; i++)   // Preserve page and cell when importing (but not mode)
            {
                if (targetSituation.GetAxisValue(i) == Constants.DefaultID)
                {
                    targetSituation.SetAxisValue(i, actionSet.LogicalState.GetAxisValue(i));
                }                
            }

            // Create a copy of the actions to import
            ActionSet actionSetCopy = new ActionSet(actionSet);
            actionSetCopy.LogicalState = targetSituation;
            actionSetCopy.Initialise(_source.Profile.KeyboardContext);

            // Translate action set reasons and internal settings
            bool converted = false;
            if (fromControls != null &&
                toControls != null)
            {
                int fromSelectionControlID = fromControls.SelectionControl.ToID();
                if (fromSelectionControlID != 0)
                {
                    /*
                    bool interpretDirections = (fromControls.SelectionControl.DirectionMode != EDirectionMode.None) && 
                                                (fromControls.SelectionControl.DirectionMode != EDirectionMode.NonDirectional);
                    EDirectionMode targetDirectionality = _profileUtils.GetActiveDirectionMode(targetSituation, toControls.SelectionControl.SpecificControl, interpretDirections);
                    GeneralisedControl targetControl = new GeneralisedControl(targetDirectionality, toControls.SelectionControl.SpecificControl);
                    converted = ConvertActionSet(actionSetCopy, fromControls.SelectionControl, targetSituation, targetControl);*/
                    converted = ConvertActionSet(actionSetCopy, fromControls.SelectionControl, targetSituation, toControls.SelectionControl);
                }

                int fromNavigationControlID = fromControls.NavigationControl.ToID();
                if (!converted && fromNavigationControlID != 0)
                {
                    /*
                    bool interpretDirections = (fromControls.NavigationControl.DirectionMode != EDirectionMode.None) &&
                                                (fromControls.NavigationControl.DirectionMode != EDirectionMode.NonDirectional);
                    EDirectionMode targetDirectionality = _profileUtils.GetActiveDirectionMode(targetSituation, toControls.NavigationControl.SpecificControl, interpretDirections);
                    GeneralisedControl targetControl = new GeneralisedControl(targetDirectionality, toControls.NavigationControl.SpecificControl);
                    ConvertActionSet(actionSetCopy, fromControls.NavigationControl, targetSituation, targetControl);*/
                    converted = ConvertActionSet(actionSetCopy, fromControls.NavigationControl, targetSituation, toControls.NavigationControl);
                }
            }

            if (converted)
            {
                // Remove any existing actions
                ActionSet existingActionSet = _source.Actions.GetActionsForInputControl(targetSituation, actionSetCopy.EventArgs, false);
                if (existingActionSet != null)
                {
                    _source.Actions.RemoveActionSet(existingActionSet);
                }

                // Initialise and add the action set
                _source.Actions.AddActionSet(actionSetCopy);
            }
        }

        /// <summary>
        /// See whether an action set can be assigned to a particular input in a given situation
        /// </summary>
        /// <param name="actionSet"></param>
        /// <param name="fromGenControl"></param>
        /// <param name="toSituation"></param>
        /// <param name="toGenControl"></param>
        /// <returns></returns>
        public bool CanConvertActionSet(ActionSet actionSet, GeneralisedControl fromGenControl, StateVector toSituation, GeneralisedControl toGenControl)
        {
            bool isValid = false;
            if (_source.Utils.IsGeneralTypeOfControl(actionSet.EventArgs, fromGenControl))
            {
                isValid = true;

                // Translate input control
                ELRUDState fromDirection = _source.Utils.GetInputControlDirection(actionSet.EventArgs, fromGenControl);
                KxControlEventArgs toControl = _source.Utils.GetSpecificInputControl(toGenControl, actionSet.EventArgs.SettingType, fromDirection);
                if (toControl != null)
                {
                    BaseControl control = _source.GetVirtualControl(toControl);
                    if (control != null)
                    {
                        // See if the action set is OK without any conversion
                        foreach (ActionList actionList in actionSet.ActionLists)
                        {
                            EEventReason reason = actionList.Reason;
                            if (control.IsReasonSupported(toControl, /*toGenControl.DirectionMode,*/ reason))
                            {
                                foreach (BaseAction action in actionList)
                                {
                                    if (!_source.Utils.IsActionTypeValid(toSituation, toControl, reason, action.ActionType))
                                    {
                                        isValid = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                isValid = false;
                            }

                            if (!isValid)
                            {
                                break;
                            }
                        }

                        if (!isValid)
                        {
                            // See if the action list is OK if all the action lists are converted
                            isValid = true;
                            foreach (ActionList actionList in actionSet.ActionLists)
                            {
                                EEventReason reason = actionList.Reason;
                                EEventReason alternativeReason = GetAlternativeReason(reason);
                                if (control.IsReasonSupported(toControl, /*toGenControl.DirectionMode,*/ alternativeReason))
                                {
                                    foreach (BaseAction action in actionList)
                                    {
                                        if (!_source.Utils.IsActionTypeValid(toSituation, toControl, alternativeReason, action.ActionType))
                                        {
                                            isValid = false;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    isValid = false;
                                }

                                if (!isValid)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }                
            }            

            return isValid;
        }

        /// <summary>
        /// Convert an action set from one input to another
        /// </summary>
        /// <param name="actionSet"></param>
        /// <param name="fromControl"></param>
        /// <param name="toControl"></param>
        public bool ConvertActionSet(ActionSet actionSet, GeneralisedControl fromGenControl, StateVector toSituation, GeneralisedControl toGenControl)
        {
            // Translate triggering control
            bool success = false;
            if (CanConvertActionSet(actionSet, fromGenControl, toSituation, toGenControl))
            {
                // Translate input control
                ELRUDState fromDirection = _source.Utils.GetInputControlDirection(actionSet.EventArgs, fromGenControl);
                KxControlEventArgs toControl = _source.Utils.GetSpecificInputControl(toGenControl, actionSet.EventArgs.SettingType, fromDirection);

                if (toControl != null)
                {
                    BaseControl control = _source.GetVirtualControl(toControl);
                    if (control != null)
                    {
                        // Get the direction mode of the control in the target situation
                        //EDirectionMode directionMode = _source.Utils.GetActiveDirectionMode(toSituation, toControl);

                        // See if the action set is OK without any conversion
                        bool needToConvert = false;
                        foreach (ActionList actionList in actionSet.ActionLists)
                        {
                            EEventReason reason = actionList.Reason;
                            if (control.IsReasonSupported(toControl, /*toGenControl.DirectionMode,*/ reason))
                            {
                                foreach (BaseAction action in actionList)
                                {
                                    if (!_source.Utils.IsActionTypeValid(toSituation, toControl, reason, action.ActionType))
                                    {
                                        needToConvert = true;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                needToConvert = true;
                            }

                            if (needToConvert)
                            {
                                break;
                            }
                        }

                        // See whether we need to translate between pressed and directed
                        if (needToConvert)
                        {
                            // Get a list of reasons in use
                            List<EEventReason> reasons = new List<EEventReason>();
                            foreach (ActionList actionList in actionSet.ActionLists)
                            {
                                reasons.Add(actionList.Reason);
                            }

                            // Convert action lists for each reason in use
                            foreach (EEventReason reason in reasons)
                            {
                                ActionList actionList = actionSet.GetActions(reason);
                                if (actionList != null)
                                {
                                    EEventReason alternativeReason = GetAlternativeReason(reason);
                                    if (alternativeReason != reason)
                                    {
                                        // Convert to new reason
                                        actionSet.SetActions(reason, null);
                                        actionList.Reason = alternativeReason;
                                        actionSet.SetActions(alternativeReason, actionList);
                                    }
                                }
                            }
                        }

                        // Convert the action set
                        KxControlEventArgs args = new KxControlEventArgs(toControl);
                        actionSet.EventArgs = args;
                        actionSet.Updated();
                        success = true;
                    }
                }
            }            

            return success;
        }        
            
        /// <summary>
        /// Convert between directed and pressed where valid
        /// </summary>
        /// <param name="inputControl"></param>
        /// <param name="reason"></param>
        /// <param name="alternativeReason"></param>
        /// <returns></returns>
        private EEventReason GetAlternativeReason(EEventReason reason)
        {
            EEventReason alternativeReason = reason;
            switch (reason)
            {
                case EEventReason.Pressed:
                    alternativeReason = EEventReason.Directed; break;
                case EEventReason.PressedLong:
                    alternativeReason = EEventReason.DirectedLong; break;
                case EEventReason.PressedShort:
                    alternativeReason = EEventReason.DirectedShort; break;
                case EEventReason.PressRepeated:
                    alternativeReason = EEventReason.DirectionRepeated; break;
                case EEventReason.Released:
                    alternativeReason = EEventReason.Undirected; break;
                case EEventReason.Directed:
                    alternativeReason = EEventReason.Pressed; break;
                case EEventReason.DirectedLong:
                    alternativeReason = EEventReason.PressedLong; break;
                case EEventReason.DirectedShort:
                    alternativeReason = EEventReason.PressedShort; break;
                case EEventReason.DirectionRepeated:
                    alternativeReason = EEventReason.PressRepeated; break;
                case EEventReason.Undirected:
                    alternativeReason = EEventReason.Released; break;                
            }

            return alternativeReason;
        }
    }
}
