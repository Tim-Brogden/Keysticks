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
using Keysticks.Actions;
using Keysticks.Core;
using Keysticks.Controls;
using Keysticks.Event;
using Keysticks.Sources;

namespace Keysticks.Config
{
    /// <summary>
    /// Utility class of methods for input sources and their control set hierarchies
    /// </summary>
    public class SourceUtils
    {
        private BaseSource _source;
        private StringUtils _utils = new StringUtils();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="profile"></param>
        public SourceUtils(BaseSource source)
        {
            _source = source;
        }

        /// <summary>
        /// Get the style of directions that apply to a control in a particular situation
        /// </summary>
        /// <param name="situation"></param>
        /// <param name="directionControl"></param>
        /// <returns></returns>
        public EDirectionMode GetActiveDirectionMode(StateVector situation, KxControlEventArgs inputControl)
        {
            EDirectionMode directionMode = EDirectionMode.NonDirectional;
            //if (directionControl is KxControllerEventArgs)
            //{
            //    KxControllerEventArgs cev = (KxControllerEventArgs)directionControl;
            if (inputControl.ControlType == EVirtualControlType.ButtonDiamond)
            {
                directionMode = EDirectionMode.FourWay;
            }
            else if (inputControl.ControlType == EVirtualControlType.DPad ||
                        inputControl.ControlType == EVirtualControlType.Stick)
            {
                directionMode = Constants.DefaultDirectionMode;

                // Look up direction mode actions for this control
                KxControlEventArgs directionControl = new KxControlEventArgs(inputControl);
                directionControl.SettingType = EControlSetting.DirectionMode;
                directionControl.LRUDState = ELRUDState.None;
                ActionSet actionSet = _source.Actions.GetActionsForInputControl(situation, directionControl, true);
                if (actionSet != null && actionSet.HasActionsForReason(EEventReason.Activated))
                {
                    ActionList actionList = actionSet.GetActions(EEventReason.Activated);
                    foreach (BaseAction action in actionList)
                    {
                        if (action.ActionType == EActionType.SetDirectionMode)
                        {
                            SetDirectionModeAction sdma = (SetDirectionModeAction)action;
                            directionMode = sdma.DirectionMode;
                        }
                    }
                }
            }
            //}

            return directionMode;
        }

        /// <summary>
        /// Get the controls allowed for a particular type of navigation or selection
        /// </summary>
        /// <param name="directionality"></param>
        /// <param name="avoidControl">Can be null</param>
        /// <returns></returns>
        public List<GeneralisedControl> GetControlsWithDirectionType(EDirectionMode directionality, GeneralisedControl avoidControl)
        {
            ELRUDState[] directions = new ELRUDState[] { ELRUDState.Left, ELRUDState.Right, ELRUDState.Up, ELRUDState.Down, ELRUDState.Centre };
            List<KxControlEventArgs> controlsList = new List<KxControlEventArgs>();
            if (directionality == EDirectionMode.None)
            {
                controlsList.Add(null);
            }
            else
            {
                foreach (BaseControl control in _source.InputControls)
                {
                    switch (directionality)
                    {
                        case EDirectionMode.Continuous:
                            if (control.ControlType == EVirtualControlType.Stick)
                            {
                                controlsList.Add(new KxControlEventArgs(control.ControlType, control.ID, EControlSetting.None, ELRUDState.None));
                            }
                            break;
                        case EDirectionMode.EightWay:
                        case EDirectionMode.AxisStyle:
                        case EDirectionMode.FourWay:
                        case EDirectionMode.TwoWay:
                            if (control.ControlType == EVirtualControlType.Stick ||
                                control.ControlType == EVirtualControlType.DPad)
                            {
                                controlsList.Add(new KxControlEventArgs(control.ControlType, control.ID, EControlSetting.None, ELRUDState.None));
                            }
                            else if (directionality == EDirectionMode.FourWay && control.ControlType == EVirtualControlType.ButtonDiamond)
                            {
                                controlsList.Add(new KxControlEventArgs(control.ControlType, control.ID, EControlSetting.None, ELRUDState.None));
                            }
                            break;
                        case EDirectionMode.NonDirectional:
                            if (control.ControlType == EVirtualControlType.Button ||
                                control.ControlType == EVirtualControlType.Trigger)
                            {
                                controlsList.Add(new KxControlEventArgs(control.ControlType, control.ID, EControlSetting.None, ELRUDState.None));
                            }
                            else if (control.ControlType == EVirtualControlType.Stick)
                            {
                                foreach (ELRUDState direction in directions)
                                {
                                    KxControlEventArgs ev = new KxControlEventArgs(control.ControlType, control.ID, EControlSetting.None, direction);
                                    controlsList.Add(ev);
                                }
                            }
                            else if (control.ControlType == EVirtualControlType.DPad)
                            {
                                foreach (ELRUDState direction in directions)
                                {
                                    if (direction != ELRUDState.Centre)
                                    {
                                        KxControlEventArgs ev = new KxControlEventArgs(control.ControlType, control.ID, EControlSetting.None, direction);
                                        controlsList.Add(ev);
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            List<GeneralisedControl> generalisedControls = new List<GeneralisedControl>();
            foreach (KxControlEventArgs inputControl in controlsList)
            {
                GeneralisedControl genControl = new GeneralisedControl(directionality, inputControl);
                if (avoidControl == null || !DoControlsOverlap(genControl, avoidControl))
                {
                    generalisedControls.Add(genControl);
                }
            }

            return generalisedControls;
        }

        /// <summary>
        /// Return which directions are used for a particular direction style
        /// </summary>
        /// <param name="directionIndex"></param>
        /// <param name="directionMode"></param>
        /// <returns></returns>
        public bool IsDirectionValid(ELRUDState direction, EDirectionMode directionMode/*, bool allowCentre*/)
        {
            bool isValid;
            switch (directionMode)
            {
                case EDirectionMode.FourWay:
                    isValid = (/*allowCentre && */direction == ELRUDState.Centre) ||
                        (direction == ELRUDState.Left) ||
                        (direction == ELRUDState.Right) ||
                        (direction == ELRUDState.Up) ||
                        (direction == ELRUDState.Down);
                    break;
                case EDirectionMode.AxisStyle:
                    isValid = (direction == ELRUDState.Centre) ||
                        (direction == ELRUDState.Left) ||
                        (direction == ELRUDState.Right) ||
                        (direction == ELRUDState.Up) ||
                        (direction == ELRUDState.Down);
                    break;
                case EDirectionMode.TwoWay:
                    isValid = (direction == ELRUDState.Left) ||
                                (direction == ELRUDState.Right);
                    break;
                case EDirectionMode.Continuous:
                    isValid = (direction == ELRUDState.Centre);
                    break;
                default:
                    isValid = true;
                    break;
            }

            return isValid;
        }

        /// <summary>
        /// See whether two general controls clash
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public bool DoControlsOverlap(GeneralisedControl first, GeneralisedControl second)
        {
            bool isOverlap = IsGeneralTypeOfControl(first.ReferenceControl, second) ||
                            IsGeneralTypeOfControl(second.ReferenceControl, first);

            return isOverlap;
        }

        /// <summary>
        /// See if the input control is of the general type specified
        /// </summary>
        /// <param name="inputControl"></param>
        /// <param name="generalControl"></param>
        /// <returns></returns>
        public bool IsGeneralTypeOfControl(KxControlEventArgs inputControl, GeneralisedControl generalControl)
        {
            bool isMatch = false;

            if (inputControl != null && generalControl.ReferenceControl != null)
            {
                KxControlEventArgs reference = generalControl.ReferenceControl;
                switch (generalControl.DirectionMode)
                {
                    case EDirectionMode.NonDirectional:
                        isMatch = inputControl.ToGeneralID() == reference.ToGeneralID() && inputControl.LRUDState == reference.LRUDState;
                        break;
                    case EDirectionMode.TwoWay:
                        isMatch = inputControl.ToGeneralID() == reference.ToGeneralID();
                        if (isMatch)
                        {
                            switch (inputControl.LRUDState)
                            {
                                case ELRUDState.None:
                                case ELRUDState.Left:
                                case ELRUDState.Right:
                                    break;
                                default:
                                    isMatch = false;
                                    break;
                            }
                        }
                        break;
                    case EDirectionMode.FourWay:
                        if (reference.ControlType == EVirtualControlType.ButtonDiamond)
                        {
                            if (inputControl.ControlType == EVirtualControlType.ButtonDiamond)
                            {
                                isMatch = inputControl.ToGeneralID() == reference.ToGeneralID();
                            }
                            else if (inputControl.ControlType == EVirtualControlType.Button)
                            {
                                // See if the button is in this diamond
                                ControllerButtonDiamond diamond = (ControllerButtonDiamond)_source.ButtonDiamonds.GetItemByID(Constants.ID1);
                                if (diamond != null)
                                {
                                    foreach (int id in diamond.LRUDControlIDs)
                                    {
                                        if (id == inputControl.ControlID)
                                        {
                                            isMatch = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else if (reference.ControlType == EVirtualControlType.Stick ||
                                reference.ControlType == EVirtualControlType.DPad)
                        {
                            isMatch = inputControl.ToGeneralID() == reference.ToGeneralID();
                            switch (inputControl.LRUDState)
                            {
                                case ELRUDState.None:
                                case ELRUDState.Left:
                                case ELRUDState.Right:
                                case ELRUDState.Up:
                                case ELRUDState.Down:
                                    break;
                                default:
                                    isMatch = false;
                                    break;
                            }
                        }                        
                        break;
                    case EDirectionMode.AxisStyle:                        
                    case EDirectionMode.Continuous:                    
                    case EDirectionMode.EightWay:
                        isMatch = inputControl.ToGeneralID() == reference.ToGeneralID();
                        break;
                }
            }

            return isMatch;
        }

        /// <summary>
        /// Get the directionality of a control, treating it as a generalised control
        /// </summary>
        /// <param name="inputControl"></param>
        /// <returns></returns>
        public ELRUDState GetInputControlDirection(KxControlEventArgs inputControl, GeneralisedControl genControl)
        {
            ELRUDState direction = ELRUDState.None;
            if (inputControl.SettingType == EControlSetting.None &&
                genControl.DirectionMode != EDirectionMode.None && 
                genControl.DirectionMode != EDirectionMode.NonDirectional)
            {
                switch (inputControl.ControlType)
                {
                    case EVirtualControlType.Button:
                        if (genControl.ReferenceControl.ControlType == EVirtualControlType.ButtonDiamond)
                        {
                            ControllerButtonDiamond diamond = (ControllerButtonDiamond)_source.ButtonDiamonds.GetItemByID(Constants.ID1);
                            if (diamond != null)
                            {
                                if (inputControl.ControlID == diamond.LRUDControlIDs[0])
                                {
                                    direction = ELRUDState.Left;
                                }
                                else if (inputControl.ControlID == diamond.LRUDControlIDs[1])
                                {
                                    direction = ELRUDState.Right;
                                }
                                else if (inputControl.ControlID == diamond.LRUDControlIDs[2])
                                {
                                    direction = ELRUDState.Up;
                                }
                                else if (inputControl.ControlID == diamond.LRUDControlIDs[3])
                                {
                                    direction = ELRUDState.Down;
                                }
                            }
                        }
                        break;
                    case EVirtualControlType.DPad:
                    case EVirtualControlType.Stick:
                        direction = inputControl.LRUDState;
                        break;
                }
            }

            return direction;
        }

        /// <summary>
        /// Get the name of a general control
        /// </summary>
        /// <param name="genControl"></param>
        /// <returns></returns>
        public string GetGeneralControlName(GeneralisedControl genControl)
        {
            string name;

            BaseControl control = null;
            if (genControl.ReferenceControl != null)
            {
                control = _source.GetVirtualControl(genControl.ReferenceControl);
            }

            if (control != null)
            {
                name = control.Name;
                switch (genControl.DirectionMode)
                {                    
                    case EDirectionMode.NonDirectional:
                        if (genControl.ReferenceControl.LRUDState != ELRUDState.None)
                        {
                            name += string.Format(" ({0})", _utils.DirectionToString(genControl.ReferenceControl.LRUDState).ToLower());
                        }
                        break;
                    case EDirectionMode.TwoWay:
                        name += " " + Properties.Resources.String_LeftOrRight;
                        break;                    
                }
            }
            else
            {
                name = Properties.Resources.String_NotApplicableAbbrev;
            }

            return name;
        }

        /// <summary>
        /// Get a specific input control which represents a generalised control e.g. ABXY buttons -> A button, DPad -> DPad centre
        /// </summary>
        /// <param name="generalType"></param>
        /// <returns></returns>
        public KxControlEventArgs GetSpecificInputControl(GeneralisedControl genControl, EControlSetting settingType, ELRUDState direction)
        {
            KxControlEventArgs reference = genControl.ReferenceControl;
            KxControlEventArgs specificControl = null;

            // Set direction
            if (reference != null)
            {
                switch (reference.ControlType)
                {
                    case EVirtualControlType.Trigger:
                        if (genControl.DirectionMode == EDirectionMode.NonDirectional)
                        {
                            specificControl = new KxControlEventArgs(reference);
                        }
                        break;
                    case EVirtualControlType.Button:
                    case EVirtualControlType.ButtonDiamond:
                        if (genControl.DirectionMode == EDirectionMode.NonDirectional)
                        {
                            specificControl = new KxControlEventArgs(reference);
                        }
                        else if (genControl.DirectionMode == EDirectionMode.FourWay)
                        {
                            int controlID = -1;
                            ControllerButtonDiamond diamond = (ControllerButtonDiamond)_source.ButtonDiamonds.GetItemByID(Constants.ID1);
                            if (settingType != EControlSetting.DirectionMode && diamond != null)    // Direction mode actions are N/A for buttons
                            {
                                switch (direction)
                                {
                                    case ELRUDState.Left:
                                        controlID = diamond.LRUDControlIDs[0];
                                        break;
                                    case ELRUDState.Right:
                                        controlID = diamond.LRUDControlIDs[1];
                                        break;
                                    case ELRUDState.Up:
                                        controlID = diamond.LRUDControlIDs[2];
                                        break;
                                    case ELRUDState.Down:
                                        controlID = diamond.LRUDControlIDs[3];
                                        break;
                                }
                            }

                            if (controlID != -1)
                            {
                                specificControl = new KxControlEventArgs(EVirtualControlType.Button, controlID, EControlSetting.None, ELRUDState.None);
                            }
                            else
                            {
                                specificControl = null;
                            }
                        }
                        break;
                    case EVirtualControlType.DPad:
                    case EVirtualControlType.Stick:
                        specificControl = new KxControlEventArgs(reference);
                        if (settingType != EControlSetting.None)
                        {
                            specificControl.LRUDState = ELRUDState.None;
                        }
                        else
                        {
                            if (genControl.DirectionMode != EDirectionMode.None &&
                               genControl.DirectionMode != EDirectionMode.NonDirectional &&
                               direction != ELRUDState.None)
                            {
                                specificControl.LRUDState = direction;
                            }
                        }
                        break;
                }
            } 
            
            // Set setting type
            if (specificControl != null)
            {
                specificControl.SettingType = settingType;                
            }

            return specificControl;
        }

        /// <summary>
        /// Get the types of direction supported by a control
        /// </summary>
        /// <param name="inputControl"></param>
        /// <returns></returns>
        public EDirectionMode GetSupportedDirectionTypes(KxControlEventArgs inputControl)
        {
            EDirectionMode supportedDirections = EDirectionMode.None;
            switch (inputControl.ControlType)
            {
                case EVirtualControlType.Button:
                    // If button is in a diamond, it also supports 4-way directions
                    supportedDirections = EDirectionMode.NonDirectional;
                    BaseSource controller = _source as BaseSource;
                    if (controller != null)
                    {
                        foreach (ControllerButtonDiamond diamond in controller.ButtonDiamonds)
                        {
                            foreach (int controlID in diamond.LRUDControlIDs)
                            {
                                if (controlID == inputControl.ControlID)
                                {
                                    supportedDirections |= EDirectionMode.FourWay;
                                    break;
                                }
                            }
                        }
                    }                    
                    break;
                case EVirtualControlType.DPad:
                    supportedDirections = EDirectionMode.NonDirectional | EDirectionMode.AxisStyle | EDirectionMode.TwoWay | EDirectionMode.FourWay | EDirectionMode.EightWay;
                    break;
                case EVirtualControlType.Stick:
                    supportedDirections = EDirectionMode.NonDirectional | EDirectionMode.AxisStyle | EDirectionMode.TwoWay | EDirectionMode.FourWay | EDirectionMode.EightWay | EDirectionMode.Continuous;
                    break;
                case EVirtualControlType.Trigger:
                    supportedDirections = EDirectionMode.NonDirectional;
                    break;
            }

            return supportedDirections;
        }
        
        /// <summary>
        /// Get the display name of a situation
        /// This method is for situations that don't include zero values i.e. are not relative to another situation
        /// </summary>
        /// <param name="stateVector"></param>
        /// <param name="_profileAxes"></param>
        /// <returns></returns>
        public string GetAbsoluteSituationName(StateVector stateVector)
        {
            StringBuilder sb = new StringBuilder();

            if (stateVector != null)
            {
                // Find the last specific state
                int stopAfter = 0;
                for (int i = 2; i != -1; i--)
                {
                    if (stateVector.GetAxisValue(i) > 0)
                    {
                        stopAfter = i;
                        break;
                    }
                }

                NamedItemList valueList = _source.StateTree.SubValues;
                for (int i = 0; i <= stopAfter; i++)
                {
                    AxisValue matchedValue = (AxisValue)valueList.GetItemByID(stateVector.GetAxisValue(i));
                    if (matchedValue != null)
                    {
                        if (sb.Length != 0)
                        {
                            sb.Append(" - ");
                        }
                        sb.Append(matchedValue.Name);
                        //sb.Append(" ");
                        //sb.Append(axis.Name.ToLower());

                        // Go down to next level in the state tree
                        valueList = matchedValue.SubValues;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get the display name of a situation which is relative to a reference situation
        /// </summary>
        /// <param name="relativeState"></param>
        /// <param name="referenceState"></param>
        /// <returns></returns>
        public string GetRelativeSituationName(StateVector relativeState, StateVector referenceState)
        {
            StringBuilder sb = new StringBuilder();

            if (relativeState != null && referenceState != null)
            {
                StateVector interprettedState = RelativeStateToAbsolute(relativeState, referenceState);

                NamedItemList valueList = _source.StateTree.SubValues;
                for (int i = 0; i < 3; i++)
                {
                    AxisValue matchedValue = null;
                    if (valueList != null)
                    {
                        matchedValue = (AxisValue)valueList.GetItemByID(interprettedState.GetAxisValue(i));
                        valueList = matchedValue != null ? matchedValue.SubValues : null;
                    }

                    // Don't include "No change" in the name
                    if (relativeState.GetAxisValue(i) != Constants.NoneID)
                    {
                        if (relativeState.GetAxisValue(i) == Constants.NextID)
                        {
                            sb.Append(" - " + string.Format(Properties.Resources.String_NextX, _utils.GetAxisName(i).ToLower()));
                        }
                        else if (relativeState.GetAxisValue(i) == Constants.PreviousID)
                        {
                            sb.Append(" - " + string.Format(Properties.Resources.String_PreviousX, _utils.GetAxisName(i).ToLower()));
                        }
                        else if (matchedValue != null)
                        {
                            sb.Append(" - " + matchedValue.Name);
                        }
                        //sb.Append(" ");
                        //sb.Append(axis.Name.ToLower());
                    }
                }
            }

            return sb.ToString().Trim(' ', '-');
        }

        /// <summary>
        /// Interpret a state which may contain special ID values (None, Next, Previous)
        /// which define a situation relative to a reference situation
        /// </summary>
        /// <param name="relativeState"></param>
        /// <param name="referenceState"></param>
        /// <returns></returns>
        public StateVector RelativeStateToAbsolute(StateVector relativeState, StateVector referenceState/*, IStateManager stateManager*/)
        {
            StateVector interpretedState = relativeState;
            if (relativeState != null && relativeState.IsRelative() && referenceState != null)
            {
                interpretedState = StateVector.GetRootSituation();

                // Interpret any values signifying "no change", "next" or "previous"
                NamedItemList valueList = _source.StateTree.SubValues;
                for (int i = 0; i < 3; i++)
                {
                    // Replace any default value
                    int valueID = relativeState.GetAxisValue(i);
                    if (valueID == Constants.NoneID)
                    {
                        valueID = referenceState.GetAxisValue(i);
                    }
                    else if (valueID == Constants.NextID)
                    {
                        valueID = valueList.FindNextID(referenceState.GetAxisValue(i));
                    }
                    else if (valueID == Constants.PreviousID)
                    {
                        valueID = valueList.FindPreviousID(referenceState.GetAxisValue(i));
                    }

                    // Go down to next level in the state tree
                    AxisValue matchedValue = (AxisValue)valueList.GetItemByID(valueID);
                    if (matchedValue != null)
                    {
                        // Found value                        
                        interpretedState.SetAxisValue(i, valueID);

                        valueList = matchedValue.SubValues;
                    }
                    else
                    {
                        // Value not in state tree                        
                        break;
                    }
                }
            }

            return interpretedState;
        }

        /// <summary>
        /// For modes and pages that have non-default values, replace default values (-1) with the a sensible specific value
        /// </summary>
        /// <param name="absoluteSituation"></param>
        /// <returns></returns>
        public void MakeStateSpecific(StateVector absoluteSituation/*, IStateManager stateManager*/)
        {
            if (absoluteSituation != null && !absoluteSituation.IsSpecific())
            {
                NamedItemList valueList = _source.StateTree.SubValues;
                for (int i = 0; i < 3; i++)
                {
                    int valueID = absoluteSituation.GetAxisValue(i);

                    // Replace any default value
                    if (valueID == Constants.DefaultID)
                    {
                        // If it's still default value, select a sensible cell, or just the first value we find
                        if (i == 2)
                        {
                            // Set to appropriate cell according to grid type
                            AxisValue modeItem = (AxisValue)GetModeItem(absoluteSituation);
                            if (modeItem != null)
                            {
                                switch (modeItem.GridType)
                                {
                                    case EGridType.Keyboard:
                                        valueID = (int)EKeyboardKey.A;
                                        break;
                                    case EGridType.ActionStrip:
                                        valueID = 1;    // First cell
                                        break;
                                    case EGridType.Square4x4:
                                    case EGridType.Square8x4:
                                        valueID = Constants.CentreCellID;
                                        break;
                                }
                            }
                        }
                        else
                        {
                            // Set to the first non-default value
                            foreach (AxisValue axisValue in valueList)
                            {
                                if (axisValue.ID > 0)
                                {
                                    valueID = axisValue.ID;
                                    break;
                                }
                            }
                        }

                        absoluteSituation.SetAxisValue(i, valueID);
                    }

                    // Go down to next level in the state tree
                    AxisValue matchedValue = (AxisValue)valueList.GetItemByID(valueID);
                    if (matchedValue != null)
                    {
                        valueList = matchedValue.SubValues;
                    }
                    else
                    {
                        // Value not in state tree
                        while (i < 3)
                        {
                            absoluteSituation.SetAxisValue(i++, Constants.DefaultID);
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Get the mode or page name of the specified state
        /// </summary>
        /// <param name="state">Not null</param>
        /// <returns></returns>
        public string GetModeOrPageName(StateVector state)
        {
            string name = "";
            AxisValue modeItem = _source.StateTree.GetMode(state.ModeID);
            if (modeItem != null && modeItem.ID != Constants.DefaultID)
            {
                int pageID = state.PageID;
                if (pageID != Constants.DefaultID)
                {
                    AxisValue pageItem = (AxisValue)modeItem.SubValues.GetItemByID(pageID);
                    if (pageItem != null)
                    {
                        name = pageItem.Name;
                    }
                }
                else
                {
                    name = modeItem.Name;
                }
            }

            return name;
        }
        
        /// <summary>
        /// Decide whether an action type is valid for a control and event reason in a given situation
        /// </summary>
        /// <param name="situation"></param>
        /// <param name="inputControl"></param>
        /// <param name="supportedReason"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public bool IsActionTypeValid(StateVector situation, KxControlEventArgs inputControl, EEventReason supportedReason, EActionType actionType)
        {
            bool add = false;
            if (inputControl.SettingType == EControlSetting.DirectionMode)
            {
                if (actionType == EActionType.SetDirectionMode)
                {
                    add = supportedReason == EEventReason.Activated;
                }
            }
            else if (inputControl.SettingType == EControlSetting.DwellAndRepeat)
            {
                if (actionType == EActionType.SetDwellAndAutorepeat)
                {
                    add = supportedReason == EEventReason.Activated;
                }
            }
            else
            {
                switch (actionType)
                {
                    case EActionType.ControlThePointer:
                        add = supportedReason == EEventReason.Moved;
                        break;
                    case EActionType.SetDirectionMode:
                    case EActionType.SetDwellAndAutorepeat:
                        add = false;
                        break;
                    case EActionType.DoNothing:
                        add = true;
                        break;
                    case EActionType.NavigateCells:
                    case EActionType.WordPrediction:
                        // Exclude keyboard actions if required
                        AxisValue modeItem = GetModeItem(situation);
                        bool includeGridActions =  _source.Profile.IsTemplate || (modeItem != null && modeItem.GridType != EGridType.None);
                        if (includeGridActions)
                        {
                            switch (supportedReason)
                            {
                                case EEventReason.Moved:
                                    add = false; break;
                                case EEventReason.DirectionRepeated:
                                case EEventReason.DirectedLong:
                                case EEventReason.DirectedShort:
                                    add = (inputControl.LRUDState != ELRUDState.Centre); break;
                                default:
                                    add = true; break;
                            }
                        }
                        break;
                    case EActionType.ReleaseKey:
                    case EActionType.ReleaseMouseButton:
                    case EActionType.MoveThePointer:
                    case EActionType.ChangeControlSet:
                    case EActionType.ToggleControlsWindow:
                    case EActionType.ToggleKey:
                    case EActionType.ToggleMouseButton:
                        switch (supportedReason)
                        {
                            case EEventReason.Moved:
                                add = false; break;
                            case EEventReason.DirectionRepeated:
                            case EEventReason.DirectedLong:
                            case EEventReason.DirectedShort:
                                add = (inputControl.LRUDState != ELRUDState.Centre); break;
                            default:
                                add = true; break;
                        }
                        break;
                    case EActionType.TypeKey:
                    case EActionType.PressDownKey:
                    case EActionType.TypeText:
                    case EActionType.ClickMouseButton:
                    case EActionType.DoubleClickMouseButton:
                    case EActionType.PressDownMouseButton:
                    case EActionType.MouseWheelUp:
                    case EActionType.MouseWheelDown:
                    case EActionType.Wait:
                        // Disallow in continuous mode or centre position
                        switch (supportedReason)
                        {
                            case EEventReason.Moved:
                                add = false; break;
                            case EEventReason.Directed:
                            case EEventReason.DirectionRepeated:
                            case EEventReason.DirectedLong:
                            case EEventReason.DirectedShort:
                                add = (inputControl.LRUDState != ELRUDState.Centre); break;
                            default:
                                add = true; break;
                        }
                        break;
                    default:
                        switch (supportedReason)
                        {
                            // Disallow for continuous mode, auto-repeat and centre position
                            case EEventReason.Moved:
                            case EEventReason.DirectionRepeated:
                            case EEventReason.PressRepeated:
                                add = false; break;
                            case EEventReason.Directed:
                            case EEventReason.DirectedLong:
                            case EEventReason.DirectedShort:
                                add = (inputControl.LRUDState != ELRUDState.Centre); break;
                            default:
                                add = true; break;
                        }
                        break;
                }
            }

            return add;
        }

        /// <summary>
        /// Get the mode item for the specified situation
        /// </summary>
        /// <returns></returns>
        public AxisValue GetModeItem(StateVector situation)
        {
            return _source.StateTree.GetMode(situation.ModeID);
        }

        /// <summary>
        /// Get the page item for the specified situation
        /// </summary>
        /// <returns></returns>
        public AxisValue GetPageItem(StateVector situation)
        {
            AxisValue pageItem = null;
            AxisValue modeAxisValue = (AxisValue)GetModeItem(situation);
            if (modeAxisValue != null)
            {
                AxisValue pageAxisValue = (AxisValue)modeAxisValue.SubValues.GetItemByID(situation.PageID);
                if (pageAxisValue != null)
                {
                    pageItem = pageAxisValue;
                }
            }

            return pageItem;
        }
    }
}
