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
using SlimDX.XInput;
using Keysticks.Core;

namespace Keysticks.Input
{
    /// <summary>
    /// Retrieves input from an XInput device
    /// </summary>
    public class SlimXInputDevice : BaseInputDevice
    {
        // Fields
        private Controller _controller;
        private PhysicalInput _capabilities;
        private State _controllerState;
        private uint _packetNumber;

        // Properties
        public override PhysicalInput Capabilities { get { return _capabilities; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userIndex"></param>
        public SlimXInputDevice(int id, string name, UserIndex userIndex)
            :base(id, name)
        {
            _controller = new Controller(userIndex);
            StoreCapabilities(id, name);
        }

        /// <summary>
        /// Store the capabilities of the controller
        /// </summary>
        /// <param name="capabilities"></param>
        private void StoreCapabilities(int id, string name)
        {
            IsConnected = _controller.IsConnected;
            if (IsConnected)
            {
                PhysicalControl control;
                StringUtils utils = new StringUtils();
                Capabilities cap = _controller.GetCapabilities(DeviceQueryType.Gamepad);
                Gamepad gamepad = cap.Gamepad;

                // Create a physical input which represents the capabilities of the device
                EDeviceType deviceType = GetGenericDeviceType(cap.Subtype);
                string description = utils.DeviceTypeToString(deviceType);
                _capabilities = new PhysicalInput(id, name, description, false, deviceType, id);

                // Create Dpad
                int controlID = 1;
                if (gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) && gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) &&
                    gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) && gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight))
                {
                    control = new PhysicalControl(controlID++, Properties.Resources.String_DPad, "DP", EPhysicalControlType.POV, Constants.Index0, 0f);
                    _capabilities.AddControl(control);
                }

                // Create axes
                if (gamepad.LeftThumbX != 0)
                {
                    control = new PhysicalControl(controlID++, Properties.Resources.String_Axis + " X1", "X1", EPhysicalControlType.Axis, Constants.Index0, Gamepad.GamepadLeftThumbDeadZone / 32768f);
                    _capabilities.AddControl(control);
                }
                if (gamepad.LeftThumbY != 0)
                {
                    control = new PhysicalControl(controlID++, Properties.Resources.String_Axis + " Y1", "Y1", EPhysicalControlType.Axis, Constants.Index1, Gamepad.GamepadLeftThumbDeadZone / 32768f);
                    _capabilities.AddControl(control);
                }
                if (gamepad.RightThumbX != 0)
                {
                    control = new PhysicalControl(controlID++, Properties.Resources.String_Axis + " X2", "X2", EPhysicalControlType.Axis, Constants.Index2, Gamepad.GamepadRightThumbDeadZone / 32768f);
                    _capabilities.AddControl(control);
                }
                if (gamepad.RightThumbY != 0)
                {
                    control = new PhysicalControl(controlID++, Properties.Resources.String_Axis + " Y2", "Y2", EPhysicalControlType.Axis, Constants.Index3, Gamepad.GamepadRightThumbDeadZone / 32768f);
                    _capabilities.AddControl(control);
                }

                // Create sliders
                if (gamepad.LeftTrigger != 0)
                {
                    control = new PhysicalControl(controlID++, Properties.Resources.String_LeftTrigger, "T1", EPhysicalControlType.Slider, Constants.Index0, Gamepad.GamepadTriggerThreshold / 256f);
                    _capabilities.AddControl(control);
                }
                if (gamepad.RightTrigger != 0)
                {
                    control = new PhysicalControl(controlID++, Properties.Resources.String_RightTrigger, "T2", EPhysicalControlType.Slider, Constants.Index1, Gamepad.GamepadTriggerThreshold / 256f);
                    _capabilities.AddControl(control);
                }

                // Create buttons
                int index = 0;
                foreach (EButtonState buttonID in Enum.GetValues(typeof(EButtonState)))
                {
                    if (gamepad.Buttons.HasFlag(EButtonStateToXInputButton(buttonID)))
                    {
                        string buttonName = utils.ButtonToString(buttonID);
                        string shortName = utils.EButtonStateToShortName(buttonID);
                        control = new PhysicalControl(controlID++, buttonName, shortName, EPhysicalControlType.Button, (int)buttonID - 1, 0f);
                        _capabilities.AddControl(control);
                        index++;
                    }
                }
            }
            else
            {
                _capabilities = new PhysicalInput(Constants.NoneID, name, Properties.Resources.String_Disconnected, false, EDeviceType.Gamepad, Constants.NoneID);
            }
        }

        /// <summary>
        /// Acquire the device and initialise the stored device state
        /// </summary>
        /// <returns></returns>
        public override void InitialiseState()
        {
            // Do nothing
        }

        /// <summary>
        /// Update the stored device state
        /// </summary>
        /// <returns></returns>
        public override void UpdateState()
        {
            IsStateChanged = false;
            if (_controller.IsConnected)
            {
                IsConnected = true;
                _controllerState = _controller.GetState();
                if (_controllerState.PacketNumber != _packetNumber)
                {
                    _packetNumber = _controllerState.PacketNumber;
                    IsStateChanged = true;
                }                
            }
            else if (IsConnected)
            {
                // Restore a neutral state
                _controllerState = new State();
                IsStateChanged = true;
                IsConnected = false;
            }
        }

        /// <summary>
        /// Get the state of a control
        /// </summary>
        /// <param name="physicalControl"></param>
        /// <param name="options"></param>
        /// <param name="paramValue"></param>
        public override void GetInputValue(PhysicalControl control, ref ParamValue paramValue)
        {
            switch (control.ControlType)
            {
                case EPhysicalControlType.Button:
                    paramValue.Value = IsButtonPressed(control.ControlIndex);
                    paramValue.DataType = EDataType.Bool;
                    break;
                case EPhysicalControlType.POV:
                    paramValue.Value = GetDPadDirection(control.ControlIndex);
                    paramValue.DataType = EDataType.LRUD;
                    break;
                case EPhysicalControlType.Slider:                    
                    paramValue.Value = GetSliderValue(control.ControlIndex);                        
                    paramValue.DataType = EDataType.Float;
                    break;
                case EPhysicalControlType.Axis:
                    paramValue.Value = GetAxisValue(control.ControlIndex);                        
                    paramValue.DataType = EDataType.Float;
                    break;
            }
        }

        /// <summary>
        /// Get whether a button is pressed
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        private bool IsButtonPressed(int controlIndex)
        {
            bool isPressed = false;
            switch (controlIndex + 1)
            {
                case (int)EButtonState.A:
                    isPressed = (_controllerState.Gamepad.Buttons & GamepadButtonFlags.A) != 0; break;
                case (int)EButtonState.B:
                    isPressed = (_controllerState.Gamepad.Buttons & GamepadButtonFlags.B) != 0; break;
                case (int)EButtonState.X:
                    isPressed = (_controllerState.Gamepad.Buttons & GamepadButtonFlags.X) != 0; break;
                case (int)EButtonState.Y:
                    isPressed = (_controllerState.Gamepad.Buttons & GamepadButtonFlags.Y) != 0; break;
                case (int)EButtonState.LeftShoulder:
                    isPressed = (_controllerState.Gamepad.Buttons & GamepadButtonFlags.LeftShoulder) != 0; break;
                case (int)EButtonState.RightShoulder:
                    isPressed = (_controllerState.Gamepad.Buttons & GamepadButtonFlags.RightShoulder) != 0; break;
                case (int)EButtonState.Back:
                    isPressed = (_controllerState.Gamepad.Buttons & GamepadButtonFlags.Back) != 0; break;
                case (int)EButtonState.Start:
                    isPressed = (_controllerState.Gamepad.Buttons & GamepadButtonFlags.Start) != 0; break;
                case (int)EButtonState.LeftThumb:
                    isPressed = (_controllerState.Gamepad.Buttons & GamepadButtonFlags.LeftThumb) != 0; break;
                case (int)EButtonState.RightThumb:
                    isPressed = (_controllerState.Gamepad.Buttons & GamepadButtonFlags.RightThumb) != 0; break;
            }

            return isPressed;
        }

        /// <summary>
        /// Get a trigger value
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        private float GetSliderValue(int controlIndex)
        {
            float val = 0f;
            if (controlIndex == Constants.Index0)
            {
                val = _controllerState.Gamepad.LeftTrigger;
            }
            else if (controlIndex == Constants.Index1)
            {
                val = _controllerState.Gamepad.RightTrigger;
            }

            return val / 256f;
        }

        /// <summary>
        /// Get an axis value
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        private float GetAxisValue(int controlIndex)
        {
            float val = 0f;
            switch (controlIndex)
            {
                case Constants.Index0:
                    val = _controllerState.Gamepad.LeftThumbX;
                    break;
                case Constants.Index1:
                    val = _controllerState.Gamepad.LeftThumbY;
                    break;
                case Constants.Index2:
                    val = _controllerState.Gamepad.RightThumbX;
                    break;
                case Constants.Index3:
                    val = _controllerState.Gamepad.RightThumbY;
                    break;
            }

            return val / 32768f;
        }

        /// <summary>
        /// Get a DPad direction
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        private ELRUDState GetDPadDirection(int controlIndex)
        {
            ELRUDState direction = ELRUDState.None;
            if (controlIndex == Constants.Index0)
            {
                direction = GetDirections(_controllerState.Gamepad.Buttons);
            }

            return direction;
        }

        /// <summary>
        /// Convert DPad state into directions enum value
        /// </summary>
        /// <param name="buttons"></param>
        /// <returns></returns>
        private ELRUDState GetDirections(GamepadButtonFlags buttons)
        {
            ELRUDState directions = ELRUDState.None;
            directions |= (buttons & GamepadButtonFlags.DPadLeft) != 0 ? ELRUDState.Left : ELRUDState.None;
            directions |= (buttons & GamepadButtonFlags.DPadRight) != 0 ? ELRUDState.Right : ELRUDState.None;
            directions |= (buttons & GamepadButtonFlags.DPadUp) != 0 ? ELRUDState.Up : ELRUDState.None;
            directions |= (buttons & GamepadButtonFlags.DPadDown) != 0 ? ELRUDState.Down : ELRUDState.None;
            if (directions == ELRUDState.None)
            {
                directions = ELRUDState.Centre;
            }

            return directions;
        }

        /// <summary>
        /// Convert an XInput device type to a generic device type
        /// </summary>
        /// <param name="subType"></param>
        /// <returns></returns>
        private EDeviceType GetGenericDeviceType(DeviceSubtype subType)
        {
            EDeviceType genericType = EDeviceType.Controller;
            switch (subType)
            {
                case DeviceSubtype.Gamepad:
                    genericType = EDeviceType.Gamepad; break;
                case DeviceSubtype.Wheel:
                    genericType = EDeviceType.Wheel; break;
                case DeviceSubtype.ArcadeStick:
                    genericType = EDeviceType.ArcadeStick; break;
                case DeviceSubtype.FlightStick:
                    genericType = EDeviceType.FlightStick; break;
                case DeviceSubtype.DancePad:
                    genericType = EDeviceType.DancePad; break;
            }

            return genericType;
        }

        /// <summary>
        /// Convert a generic button state to an XInput button state
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        private GamepadButtonFlags EButtonStateToXInputButton(EButtonState button)
        {
            GamepadButtonFlags buttonFlag = GamepadButtonFlags.None;
            switch (button)
            {
                case EButtonState.A:
                    buttonFlag = GamepadButtonFlags.A; break;
                case EButtonState.B:
                    buttonFlag = GamepadButtonFlags.B; break;
                case EButtonState.X:
                    buttonFlag = GamepadButtonFlags.X; break;
                case EButtonState.Y:
                    buttonFlag = GamepadButtonFlags.Y; break;
                case EButtonState.LeftShoulder:
                    buttonFlag = GamepadButtonFlags.LeftShoulder; break;
                case EButtonState.RightShoulder:
                    buttonFlag = GamepadButtonFlags.RightShoulder; break;
                case EButtonState.Back:
                    buttonFlag = GamepadButtonFlags.Back; break;
                case EButtonState.Start:
                    buttonFlag = GamepadButtonFlags.Start; break;
                case EButtonState.LeftThumb:
                    buttonFlag = GamepadButtonFlags.LeftThumb; break;
                case EButtonState.RightThumb:
                    buttonFlag = GamepadButtonFlags.RightThumb; break;
            }

            return buttonFlag;
        }
    }
}
