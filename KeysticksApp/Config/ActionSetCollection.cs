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
using System.Windows.Forms;
using System.Xml;
using Keysticks.Actions;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Controls;
using Keysticks.Sources;

namespace Keysticks.Config
{
    public class ActionSetCollection
    {
        // Fields
        private BaseSource _parent;
        private List<ActionSet> _actionSets = new List<ActionSet>();

        // State
        private int _maxActionListLength = Constants.DefaultMaxActionListLength;
        private Dictionary<int, List<ActionSet>> _actionsForInputControl;
        private Dictionary<int, ActionMappingTable> _actionMappingsCache = new Dictionary<int, ActionMappingTable>();
        private Dictionary<long, ActionSet> _actionSetCache = new Dictionary<long, ActionSet>();
        private Dictionary<int, bool> _hasPredictionCache = new Dictionary<int, bool>();

        // Properties
        private bool IsModified { set { _parent.IsModified = value; } }
        private Dictionary<int, List<ActionSet>> ActionsForInputControl
        {
            get
            {
                if (_actionsForInputControl == null)
                {
                    CategoriseActionSets();
                }
                return _actionsForInputControl;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent"></param>
        public ActionSetCollection(BaseSource parent)
        {
            _parent = parent;
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
        /// Handle keyboard layout change
        /// </summary>
        public void Initialise(IKeyboardContext context)
        {            
            foreach (ActionSet actionSet in _actionSets)
            {
                // Trigger a refresh of the action set descriptions
                actionSet.Initialise(context);
            }
        }

        /// <summary>
        /// Get the action sets which apply to a state and its substates
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public List<ActionSet> GetActionsWithinState(StateVector state)
        {
            List<ActionSet> actionSetsWithinState = new List<ActionSet>();
            foreach (ActionSet actionSet in _actionSets)
            {
                if (state.Contains(actionSet.LogicalState))
                {
                    actionSetsWithinState.Add(actionSet);
                }
            }

            return actionSetsWithinState;
        }

        /// <summary>
        /// Return the actions for the specified logical state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionMappingTable GetActionsForState(StateVector state, bool includeDefaults)
        {
            ActionMappingTable actionMappings;
            int id = state.ID | (includeDefaults ? (1 << 31) : 0);
            if (_actionMappingsCache.ContainsKey(id))
            {
                actionMappings = _actionMappingsCache[id];
            }
            else
            {
                actionMappings = new ActionMappingTable(new StateVector(state));

                // Add the actions for this state
                Dictionary<int, List<ActionSet>>.Enumerator eEnum = ActionsForInputControl.GetEnumerator();
                while (eEnum.MoveNext())
                {
                    // See which actions to perform for this event type in this state, if any
                    int controlID = eEnum.Current.Key;
                    List<ActionSet> actionSets = eEnum.Current.Value;
                    foreach (ActionSet actionSet in actionSets)
                    {
                        // Check for exact match on player and state
                        if (actionSet.LogicalState.IsSameAs(state))
                        {
                            actionMappings.SetActions(controlID, actionSet);
                            break;
                        }
                    }
                }

                // If defaults are required also, include actions for parent states
                if (includeDefaults)
                {
                    List<StateVector> parentStates = state.GetParentStates();
                    for (int i = 0; i < parentStates.Count; i++)
                    {
                        // In the case where a state has more than one parent (i.e. page and cell have values), 
                        // only recurse upwards for the first parent table, to avoid including action sets more than once
                        ActionMappingTable parentTable = GetActionsForState(parentStates[i], (i == 0));
                        actionMappings.AddParentTable(parentTable);
                    }
                }

                // Cache
                _actionMappingsCache[id] = actionMappings;

                //Trace.WriteLine("ActionMappingCache: " + _actionMappingsCache.Count);
            }

            return actionMappings;
        }

        /// <summary>
        /// Return the actions for the specified logical state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionSet GetActionsForInputControl(StateVector state, KxControlEventArgs inputControl, bool includeDefaults)
        {
            ActionSet matchedActionSet = null;
            int controlID = inputControl.ToID();
            long cacheID = (uint)state.ID | ((long)controlID << 32) | (includeDefaults ? (1L << 63) : 0L);
            if (_actionSetCache.ContainsKey(cacheID))
            {
                matchedActionSet = _actionSetCache[cacheID];
            }
            else
            {
                // Get the action sets for all situations for this control
                if (ActionsForInputControl.ContainsKey(controlID))
                {
                    List<ActionSet> actionSets = ActionsForInputControl[controlID];
                    foreach (ActionSet actionSet in actionSets)
                    {
                        // If defaults are included, the actions to use are the ones for the most specific matching state
                        // Otherwise, only actions for the exact state apply
                        if (includeDefaults)
                        {
                            // See if the action list applies to the current state
                            if (actionSet.LogicalState.Contains(state))
                            {
                                // See if the action list is more specific that any previously matched
                                if (matchedActionSet == null ||
                                    matchedActionSet.LogicalState.Contains(actionSet.LogicalState))
                                {
                                    matchedActionSet = actionSet;

                                    // Here be dragons!
                                    // If the action set is inherited, 
                                    // check that the control's directionality is compatible between the two situations
                                    // E.g. If a thumbstick is configured to be discrete, don't inherit continuous actions 
                                    // from a parent situation, and vice versa
                                    // Mustn't perform this check if we're retrieving settings for a control, otherwise causes infinite loop                                    
                                    if (inputControl.SettingType == EControlSetting.None &&
                                        matchedActionSet.EventArgs.ControlType == EVirtualControlType.Stick &&
                                        !matchedActionSet.LogicalState.IsSameAs(state))
                                    {
                                        // Inherited actions       
                                        EDirectionMode thisStateDirectionality = _parent.Utils.GetActiveDirectionMode(state, matchedActionSet.EventArgs);
                                        EDirectionMode inheritedDirectionality = _parent.Utils.GetActiveDirectionMode(matchedActionSet.LogicalState, matchedActionSet.EventArgs);

                                        // Don't inherit actions if one state is continuous and other is not
                                        if ((thisStateDirectionality == EDirectionMode.Continuous) != (inheritedDirectionality == EDirectionMode.Continuous))
                                        {
                                            matchedActionSet = null;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Check for exact match
                            if (actionSet.LogicalState.IsSameAs(state))
                            {
                                matchedActionSet = actionSet;
                                break;
                            }
                        }
                    }
                }

                // Cache
                _actionSetCache[cacheID] = matchedActionSet;

                //Trace.WriteLine("ActionSetCache: " + _actionSetCache.Count);
            }

            return matchedActionSet;
        }

        /// <summary>
        /// Get a list of actions of the specified type
        /// </summary>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public List<BaseAction> GetActionsOfType(EActionType actionType)
        {
            List<BaseAction> matchingActionsList = new List<BaseAction>();
            foreach (ActionSet actionSet in _actionSets)
            {
                foreach (ActionList actionList in actionSet.ActionLists)
                {
                    foreach (BaseAction action in actionList)
                    {
                        if (action.ActionType == actionType)
                        {
                            matchingActionsList.Add(action);
                        }
                    }
                }
            }

            return matchingActionsList;
        }

        /// <summary>
        /// Return whether or not the profile has actions of the specified type
        /// </summary>
        /// <returns></returns>
        public bool HasActionsOfType(EActionType actionType)
        {
            bool has = false;
            foreach (ActionSet actionSet in _actionSets)
            {
                foreach (ActionList actionList in actionSet.ActionLists)
                {
                    if (actionList.HasActionsOfType(actionType))
                    {
                        has = true;
                        break;
                    }
                }
            }

            return has;
        }        

        /// <summary>
        /// Return whether or not the situation uses word prediction
        /// </summary>
        /// <param name="situation"></param>
        /// <returns></returns>
        public bool HasWordPredictionInMode(int modeID)
        {
            bool has = false;
            if (_hasPredictionCache.ContainsKey(modeID))
            {
                has = _hasPredictionCache[modeID];
            }
            else
            {
                foreach (ActionSet actionSet in _actionSets)
                {
                    if (actionSet.LogicalState.ModeID == modeID)
                    {
                        foreach (ActionList actionList in actionSet.ActionLists)
                        {
                            if (actionList.HasActionsOfType(EActionType.WordPrediction))
                            {
                                has = true;
                                break;
                            }
                        }
                    }
                }
                
                _hasPredictionCache[modeID] = has;
            }

            return has;
        }

        /// <summary>
        /// Add a new action list
        /// </summary>
        /// <param name="actionSet"></param>
        public void AddActionSet(ActionSet actionSet)
        {
            _actionSets.Add(actionSet);
            CategoriseActionSet(actionSet);
            IsModified = true;
        }

        /// <summary>
        /// Remove an action list
        /// </summary>
        /// <param name="actionSet"></param>
        public void RemoveActionSet(ActionSet actionSet)
        {
            if (_actionSets.Contains(actionSet))
            {
                _actionSets.Remove(actionSet);
                DecategoriseActionSet(actionSet);
                IsModified = true;
            }
        }

        /// <summary>
        /// Clear all action sets
        /// </summary>
        public void Clear()
        {
            _actionSets.Clear();
            IsModified = true;
            
            // Force recalculation of look up tables
            _actionsForInputControl = null;
            ClearCache();
        }

        private void ClearCache()
        {
            _actionMappingsCache.Clear();
            _actionSetCache.Clear();
            _hasPredictionCache.Clear();
        }

        /// <summary>
        /// Situations and / or actions added
        /// </summary>
        public void ActionSetUpdated(ActionSet actionSet)
        {
            IsModified = true;

            // Force recalculation of look up tables
            _actionsForInputControl = null;           
            ClearCache();
        }

        /// <summary>
        /// Validate the action sets when a situation is being deleted
        /// </summary>
        public void Validate()
        {
            int i = 0;
            while (i < _actionSets.Count)
            {
                ActionSet actionSet = _actionSets[i];

                bool isValid = ValidateActionSet(actionSet);
                if (!isValid || actionSet.IsEmpty())
                {
                    DecategoriseActionSet(actionSet);
                    _actionSets.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        /// <summary>
        /// Remove any potentially harmful actions
        /// </summary>
        public void Validate_Security()
        {
            int i = 0;
            while (i < _actionSets.Count)
            {
                ActionSet actionSet = _actionSets[i];

                // Validate action list state
                ValidateActionSet_Security(actionSet);
                if (actionSet.IsEmpty())
                {
                    DecategoriseActionSet(actionSet);
                    _actionSets.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        /// <summary>
        /// Validate an action set
        /// </summary>
        /// <param name="actionSet"></param>
        private bool ValidateActionSet(ActionSet actionSet)
        {
            bool isValid = true;
            KxControlEventArgs inputControl = actionSet.EventArgs;

            // Validate triggering event
            if (_parent.GetVirtualControl(inputControl) == null)
            {
                isValid = false;
            }
            // Validate situation
            else if (!_parent.StateTree.HasState(actionSet.LogicalState))
            {
                isValid = false;
            }
            // Validate directions
            else if (inputControl.SettingType == EControlSetting.None)
            {
                // Directional control
                EDirectionMode directionMode = _parent.Utils.GetActiveDirectionMode(actionSet.LogicalState, inputControl);
                ELRUDState direction = inputControl.LRUDState;
                if (_parent.Utils.IsDirectionValid(direction, directionMode/*, false*/))
                {
                    // Get supported reasons
                    BaseControl control = _parent.GetVirtualControl(inputControl);
                    if (control != null)
                    {
                        foreach (EEventReason reason in Enum.GetValues(typeof(EEventReason)))
                        {
                            if (actionSet.HasActionsForReason(reason) &&
                                !control.IsReasonSupported(inputControl, /*directionMode,*/ reason))
                            {
                                // Action for this reason no longer valid
                                actionSet.SetActions(reason, null);
                            }
                        }
                    }
                }
                else
                {
                    isValid = false;
                }
            }
            
            // Validate specific action types
            if (isValid)
            {
                foreach (ActionList actionList in actionSet.ActionLists)
                {
                    int i = 0;
                    while (i < actionList.Count)
                    {
                        BaseAction action = actionList[i];

                        // Validate situation reference
                        if (action is ChangeSituationAction)
                        {
                            ChangeSituationAction csa = (ChangeSituationAction)action;
                            StateVector interprettedState = _parent.Utils.RelativeStateToAbsolute(csa.NewSituation, actionSet.LogicalState);
                            if (!_parent.StateTree.HasState(interprettedState))
                            {
                                actionList.RemoveAt(i);
                                actionSet.Updated();
                                continue;
                            }
                            else
                            {
                                csa.SituationName = _parent.Utils.GetRelativeSituationName(csa.NewSituation, actionSet.LogicalState);
                                actionSet.Updated();
                            }
                        }

                        i++;
                    }
                }
            }

            return isValid;
        }

        /// <summary>
        /// Remove any potentially harmful actions from an action set
        /// </summary>
        /// <param name="actionSet"></param>
        private void ValidateActionSet_Security(ActionSet actionSet)
        {
            bool hasDeleteOrBackspace = false;
            bool hasOtherRelevantActions = false;
            
            // Loop over action lists
            foreach (ActionList actionList in actionSet.ActionLists)
            {
                bool onlyChangeSituationAllowed = (actionList.Reason == EEventReason.DirectionRepeated || actionList.Reason == EEventReason.PressRepeated) &&
                                                    (actionList.HasActionsOfType(EActionType.ChangeControlSet) || actionList.HasActionsOfType(EActionType.NavigateCells));

                int i = 0;
                while (i < actionList.Count)
                {
                    BaseAction action = actionList[i];

                    // Check length of action list
                    bool isValid = true;
                    if (i >= _maxActionListLength)
                    {
                        isValid = false;
                    }

                    if (isValid)
                    {
                        // Only allow one action if the reason is auto-repeat and the action is a change situation action
                        if (onlyChangeSituationAllowed && 
                            action.ActionType != EActionType.ChangeControlSet &&
                            action.ActionType != EActionType.NavigateCells)
                        {
                            isValid = false;
                        }
                    }

                    if (isValid)
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

                    // Check that the parameter sources are still valid
                    if (!isValid)
                    {
                        actionList.RemoveAt(i);
                        actionSet.Updated();
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            // Don't allow delete or backspace with other non-situation actions
            if (hasDeleteOrBackspace && hasOtherRelevantActions)
            {
                // Remove all actions
                actionSet.ActionLists.Clear();
                actionSet.Updated();
            }
        }

        /// <summary>
        /// Update internal tables used for efficient retrieval of actions
        /// </summary>
        private void CategoriseActionSets()
        {
            // Categorise action lists by event type
            _actionsForInputControl = new Dictionary<int, List<ActionSet>>();
            foreach (ActionSet actionSet in _actionSets)
            {
                CategoriseActionSet(actionSet);
            }

            ClearCache();
        }

        /// <summary>
        /// Categorise action list by event type
        /// </summary>
        /// <param name="actionSet"></param>
        private void CategoriseActionSet(ActionSet actionSet)
        {
            // Update actions by event type
            if (_actionsForInputControl != null)
            {
                int controlID = actionSet.EventArgs.ToID();
                List<ActionSet> actionSets;
                if (_actionsForInputControl.ContainsKey(controlID))
                {
                    actionSets = _actionsForInputControl[controlID];
                    if (!actionSets.Contains(actionSet))
                    {
                        actionSets.Add(actionSet);
                    }
                }
                else
                {
                    actionSets = new List<ActionSet>();
                    actionSets.Add(actionSet);
                    _actionsForInputControl[controlID] = actionSets;
                }
            }

            ClearCache();
        }

        /// <summary>
        /// Remove an action list from the categorisation by event ID
        /// </summary>
        /// <param name="actionSet"></param>
        private void DecategoriseActionSet(ActionSet actionSet)
        {
            // Update actions by event type
            if (_actionsForInputControl != null)
            {
                int controlID = actionSet.EventArgs.ToID();
                if (_actionsForInputControl.ContainsKey(controlID))
                {
                    List<ActionSet> actionSets = _actionsForInputControl[controlID];
                    if (actionSets.Contains(actionSet))
                    {
                        actionSets.Remove(actionSet);
                        if (actionSets.Count == 0)
                        {
                            _actionsForInputControl.Remove(controlID);
                        }
                    }
                }
            }

            ClearCache();
        }

        /// <summary>
        /// Read from xml
        /// </summary>
        /// <param name="element"></param>
        public void FromXml(XmlElement element)
        {
            // Read action sets
            XmlNodeList actionSetNodes = element.SelectNodes("actionset");
            foreach (XmlElement actionSetElement in actionSetNodes)
            {
                ActionSet actionSet = new ActionSet();
                actionSet.FromXml((XmlElement)actionSetElement);                

                _actionSets.Add(actionSet);
            }

            // Initialise optimisation tables
            CategoriseActionSets();
        }
        
        /// <summary>
        /// Write to xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public void ToXml(XmlElement element, XmlDocument doc)
        {
            // Action sets
            foreach (ActionSet actionSet in _actionSets)
            {
                XmlElement actionSetElement = doc.CreateElement("actionset");
                actionSet.ToXml(actionSetElement, doc);
                element.AppendChild(actionSetElement);
            }
        }
    }
}
