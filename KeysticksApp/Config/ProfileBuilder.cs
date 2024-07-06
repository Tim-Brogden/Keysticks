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
using Keysticks.Controls;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Input;
using Keysticks.Sources;

namespace Keysticks.Config
{
    /// <summary>
    /// Utility class for creating a default profile
    /// </summary>
    public class ProfileBuilder
    {
        private StringUtils _utils = new StringUtils();

        /// <summary>
        /// Constructor
        /// </summary>
        public ProfileBuilder()
        {
        }
        
        /// <summary>
        /// Create the default profile
        /// </summary>
        /// <param name="isTemplates">Whether or not to create a templates profile</param>
        /// <param name="firstModeName">Use null if no first mode is required</param>
        /// <returns></returns>
        public Profile CreateDefaultProfile(bool isTemplates, string firstModeName)
        {
            Profile profile = new Profile(isTemplates);

            // Add a virtual controller
            BaseSource controller = CreateDefaultVirtualController(Constants.ID1, firstModeName);
            profile.AddSource(controller);

            return profile;
        }

        /// <summary>
        /// Create a virtual controller to represent the inputs
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="firstModeName">The name of the first mode (can be null if no first mode is required)</param>
        /// <remarks>The input and control IDs need to match those referenced in CreateDefaultPhysicalController()</remarks>
        /// <returns></returns>
        public BaseSource CreateDefaultVirtualController(int playerID, string firstModeName)
        {
            ControllerTrigger trigger;
            ControllerButton button;
            ControllerDPad dpad;
            ControllerStick stick;
            ControllerButtonDiamond diamond;
            AnnotationData annotationData;
            InputMapping[] inputMappings;
            List<AnnotationData> annotationDataList;

            // Create the virtual controller
            string name = _utils.GetVirtualControllerName(playerID);
            BaseSource virtualController = new BaseSource(playerID, name);

            // Set the size and title bar and menu bar regions
            virtualController.Display.TitleBarData = new AnnotationData(new Rect(Constants.DefaultControllerTitleBarX, Constants.DefaultControllerTitleBarY, Constants.DefaultControllerTitleBarWidth, Constants.DefaultControllerTitleBarHeight), null);
            virtualController.Display.MenuBarData = new AnnotationData(new Rect(Constants.DefaultControllerMenuBarX, Constants.DefaultControllerMenuBarY, Constants.DefaultControllerMenuBarWidth, Constants.DefaultControllerMenuBarHeight), null);

            // DPad
            annotationDataList = new List<AnnotationData>();
            annotationDataList.Add(new AnnotationData(new Point(124, 155), new KxControlEventArgs(EVirtualControlType.DPad, Constants.ID1, EControlSetting.None, ELRUDState.Centre)));
            annotationDataList.Add(new AnnotationData(new Point(89, 155), new KxControlEventArgs(EVirtualControlType.DPad, Constants.ID1, EControlSetting.None, ELRUDState.Left)));
            annotationDataList.Add(new AnnotationData(new Point(89, 134), new KxControlEventArgs(EVirtualControlType.DPad, Constants.ID1, EControlSetting.None, ELRUDState.UpLeft)));
            annotationDataList.Add(new AnnotationData(new Point(124, 134), new KxControlEventArgs(EVirtualControlType.DPad, Constants.ID1, EControlSetting.None, ELRUDState.Up)));
            annotationDataList.Add(new AnnotationData(new Point(159, 134), new KxControlEventArgs(EVirtualControlType.DPad, Constants.ID1, EControlSetting.None, ELRUDState.UpRight)));
            annotationDataList.Add(new AnnotationData(new Point(159, 155), new KxControlEventArgs(EVirtualControlType.DPad, Constants.ID1, EControlSetting.None, ELRUDState.Right)));
            annotationDataList.Add(new AnnotationData(new Point(159, 176), new KxControlEventArgs(EVirtualControlType.DPad, Constants.ID1, EControlSetting.None, ELRUDState.DownRight)));
            annotationDataList.Add(new AnnotationData(new Point(124, 176), new KxControlEventArgs(EVirtualControlType.DPad, Constants.ID1, EControlSetting.None, ELRUDState.Down)));
            annotationDataList.Add(new AnnotationData(new Point(89, 176), new KxControlEventArgs(EVirtualControlType.DPad, Constants.ID1, EControlSetting.None, ELRUDState.DownLeft)));
            dpad = new ControllerDPad(Constants.ID1, Properties.Resources.String_DPad, annotationDataList);
            inputMappings = new InputMapping[] { new InputMapping(Constants.ID1, EPhysicalControlType.POV, Constants.Index0, EPhysicalControlOption.None) };
            dpad.SetInputMappings(inputMappings);
            virtualController.AddControl(dpad);

            // Left stick
            annotationDataList = new List<AnnotationData>();
            annotationDataList.Add(new AnnotationData(new Point(68, 85), new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID1, EControlSetting.None, ELRUDState.Centre)));
            annotationDataList.Add(new AnnotationData(new Point(33, 85), new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID1, EControlSetting.None, ELRUDState.Left)));
            annotationDataList.Add(new AnnotationData(new Point(33, 64), new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID1, EControlSetting.None, ELRUDState.UpLeft)));
            annotationDataList.Add(new AnnotationData(new Point(68, 64), new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID1, EControlSetting.None, ELRUDState.Up)));
            annotationDataList.Add(new AnnotationData(new Point(103, 64), new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID1, EControlSetting.None, ELRUDState.UpRight)));
            annotationDataList.Add(new AnnotationData(new Point(103, 85), new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID1, EControlSetting.None, ELRUDState.Right)));
            annotationDataList.Add(new AnnotationData(new Point(103, 106), new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID1, EControlSetting.None, ELRUDState.DownRight)));
            annotationDataList.Add(new AnnotationData(new Point(68, 106), new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID1, EControlSetting.None, ELRUDState.Down)));
            annotationDataList.Add(new AnnotationData(new Point(33, 106), new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID1, EControlSetting.None, ELRUDState.DownLeft)));
            stick = new ControllerStick(Constants.ID1, Properties.Resources.String_Left_stick, annotationDataList);
            inputMappings = new InputMapping[] { new InputMapping(Constants.ID1, EPhysicalControlType.Axis, Constants.Index0, EPhysicalControlOption.None),
                                                    new InputMapping(Constants.ID1, EPhysicalControlType.Axis, Constants.Index1, EPhysicalControlOption.None),
                                                    new InputMapping(Constants.ID1, EPhysicalControlType.Button, (int)EButtonState.LeftThumb - 1, EPhysicalControlOption.None)};
            stick.SetInputMappings(inputMappings);
            virtualController.AddControl(stick);

            // Right stick
            annotationDataList = new List<AnnotationData>();
            annotationDataList.Add(new AnnotationData(new Point(240, 156), new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID2, EControlSetting.None, ELRUDState.Centre)));
            annotationDataList.Add(new AnnotationData(new Point(205, 156), new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID2, EControlSetting.None, ELRUDState.Left)));
            annotationDataList.Add(new AnnotationData(new Point(205, 135), new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID2, EControlSetting.None, ELRUDState.UpLeft)));
            annotationDataList.Add(new AnnotationData(new Point(240, 135), new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID2, EControlSetting.None, ELRUDState.Up)));
            annotationDataList.Add(new AnnotationData(new Point(275, 135), new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID2, EControlSetting.None, ELRUDState.UpRight)));
            annotationDataList.Add(new AnnotationData(new Point(275, 156), new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID2, EControlSetting.None, ELRUDState.Right)));
            annotationDataList.Add(new AnnotationData(new Point(275, 177), new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID2, EControlSetting.None, ELRUDState.DownRight)));
            annotationDataList.Add(new AnnotationData(new Point(240, 177), new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID2, EControlSetting.None, ELRUDState.Down)));
            annotationDataList.Add(new AnnotationData(new Point(205, 177), new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID2, EControlSetting.None, ELRUDState.DownLeft)));
            stick = new ControllerStick(Constants.ID2, Properties.Resources.String_RightStick, annotationDataList);
            inputMappings = new InputMapping[] { new InputMapping(Constants.ID1, EPhysicalControlType.Axis, Constants.Index2, EPhysicalControlOption.None),
                                                    new InputMapping(Constants.ID1, EPhysicalControlType.Axis, Constants.Index3, EPhysicalControlOption.None),
                                                    new InputMapping(Constants.ID1, EPhysicalControlType.Button, (int)EButtonState.RightThumb - 1, EPhysicalControlOption.None)};
            stick.SetInputMappings(inputMappings);
            virtualController.AddControl(stick);

            // Left trigger
            annotationData = new AnnotationData(new Point(85, 1), new KxControlEventArgs(EVirtualControlType.Trigger, Constants.ID1, EControlSetting.None, ELRUDState.None));
            trigger = new ControllerTrigger(Constants.ID1, Properties.Resources.String_LeftTrigger, annotationData);
            inputMappings = new InputMapping[] { new InputMapping(Constants.ID1, EPhysicalControlType.Slider, Constants.Index0, EPhysicalControlOption.None) };
            trigger.SetInputMappings(inputMappings);
            virtualController.AddControl(trigger);
            
            // Right trigger
            annotationData = new AnnotationData(new Point(282, 1), new KxControlEventArgs(EVirtualControlType.Trigger, Constants.ID2, EControlSetting.None, ELRUDState.None));
            trigger = new ControllerTrigger(Constants.ID2, Properties.Resources.String_RightTrigger, annotationData);
            inputMappings = new InputMapping[] { new InputMapping(Constants.ID1, EPhysicalControlType.Slider, Constants.Index1, EPhysicalControlOption.None) };
            trigger.SetInputMappings(inputMappings);
            virtualController.AddControl(trigger);

            // Buttons
            int index = 0;
            Point[] annotationCoords = new Point[] { 
                new Point(300, 116), new Point(330, 85), new Point(269, 85), new Point(300, 54), 
                new Point(49, 22), new Point(318, 22), new Point(142, 85), new Point(227, 85)//, 
                //new Point(142, 106), new Point(227, 106) 
            };
            foreach (EButtonState buttonID in Enum.GetValues(typeof(EButtonState)))
            {
                // Defensive check
                if (index < annotationCoords.Length)
                {
                    annotationData = new AnnotationData(annotationCoords[index], new KxControlEventArgs(EVirtualControlType.Button, index + 1, EControlSetting.None, ELRUDState.None));
                    button = new ControllerButton(index + 1, _utils.ButtonToString(buttonID), annotationData);
                    inputMappings = new InputMapping[] { new InputMapping(Constants.ID1, EPhysicalControlType.Button, index, EPhysicalControlOption.None) };
                    button.SetInputMappings(inputMappings);
                    virtualController.AddControl(button);
                    index++;
                }
            }

            // Meta controls - button diamond
            diamond = new ControllerButtonDiamond(Constants.ID1, Properties.Resources.String_Direction_buttons);
            int[] buttonIDs = new int[] { (int)EButtonState.X, (int)EButtonState.B, (int)EButtonState.Y, (int)EButtonState.A };
            diamond.SetButtonControlIDs(buttonIDs);
            virtualController.AddControl(diamond);

            // Physical inputs
            PhysicalInput input = CreateDefaultPhysicalController(Constants.ID1, playerID);
            virtualController.PhysicalInputs.Add(input);

            // Add the root state
            AxisValue rootState = new AxisValue(Constants.DefaultID, Properties.Resources.String_All, StateVector.GetRootSituation());
            virtualController.StateTree.SubValues.Add(rootState);

            // Add first mode if required            
            if (firstModeName != null)
            {
                StateVector state = StateVector.GetRootSituation();
                state.ModeID = Constants.ID1;
                AxisValue modeItem = new AxisValue(Constants.ID1, firstModeName, state);
                virtualController.StateTree.SubValues.Add(modeItem);
            }

            return virtualController;
        }

        /// <summary>
        /// Create a default physical controller comprising axes, sliders, dpads and buttons
        /// </summary>
        /// <remarks>The input and control IDs need to match those referenced in CreateDefaultVirtualController()</remarks>
        /// <returns></returns>
        public PhysicalInput CreateDefaultPhysicalController(int inputID, int deviceID)
        {
            int id = 1;
            PhysicalControl control;

            // Create the physical controller
            string inputName = string.Format(Properties.Resources.String_Controller + " {0}", deviceID);
            PhysicalInput physicalController = new PhysicalInput(inputID, inputName, Properties.Resources.String_Gamepad, false, EDeviceType.Gamepad, deviceID);

            // Create Dpad
            control = new PhysicalControl(id++, Properties.Resources.String_DPad, Properties.Resources.String_DPadAbbrev, EPhysicalControlType.POV, Constants.Index0, 0f);
            physicalController.AddControl(control);

            // Create axes
            control = new PhysicalControl(id++, Properties.Resources.String_Axis + " X1", "X1", EPhysicalControlType.Axis, Constants.Index0, Constants.DefaultStickDeadZoneFraction);
            physicalController.AddControl(control);
            control = new PhysicalControl(id++, Properties.Resources.String_Axis + " Y1", "Y1", EPhysicalControlType.Axis, Constants.Index1, Constants.DefaultStickDeadZoneFraction);
            physicalController.AddControl(control);
            control = new PhysicalControl(id++, Properties.Resources.String_Axis + " X2", "X2", EPhysicalControlType.Axis, Constants.Index2, Constants.DefaultStickDeadZoneFraction);
            physicalController.AddControl(control);
            control = new PhysicalControl(id++, Properties.Resources.String_Axis + " Y2", "Y2", EPhysicalControlType.Axis, Constants.Index3, Constants.DefaultStickDeadZoneFraction);
            physicalController.AddControl(control);

            // Create sliders
            control = new PhysicalControl(id++, Properties.Resources.String_LeftTrigger, "T1", EPhysicalControlType.Slider, Constants.Index0, Constants.DefaultTriggerDeadZoneFraction);
            physicalController.AddControl(control);
            control = new PhysicalControl(id++, Properties.Resources.String_RightTrigger, "T2", EPhysicalControlType.Slider, Constants.Index1, Constants.DefaultTriggerDeadZoneFraction);
            physicalController.AddControl(control);

            // Create buttons
            int index = 0;
            foreach (EButtonState buttonID in Enum.GetValues(typeof(EButtonState)))
            {
                string name = _utils.ButtonToString(buttonID);
                string shortName = _utils.EButtonStateToShortName(buttonID);
                control = new PhysicalControl(id++, name, shortName, EPhysicalControlType.Button, index, 0f);
                physicalController.AddControl(control);
                index++;
            }

            return physicalController;
        }

    }
}
