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
using System.Diagnostics;
using SlimDX.DirectInput;
using Keysticks.Core;

namespace Keysticks.Input
{
    /// <summary>
    /// Retrieves input from a DirectInput device
    /// </summary>
    public class SlimDirectInputDevice : BaseInputDevice
    {
        // Fields
        private DeviceInstance _deviceInstance;
        private Joystick _device;
        private PhysicalInput _capabilities;
        //private IList<EffectInfo> _effects;
        private bool _supportsXInput;
        private List<SlimSliderReader> _sliderReaders = new List<SlimSliderReader>();
        private List<SlimAxisReader> _axisReaders = new List<SlimAxisReader>();
        private JoystickState _deviceState;

        // Properties
        public override PhysicalInput Capabilities { get { return _capabilities; } }
        public bool SupportsXInput { get { return _supportsXInput; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="deviceInstance"></param>
        /// <param name="directInput"></param>
        public SlimDirectInputDevice(int id, string name, DeviceInstance deviceInstance, DirectInput directInput)
            :base(id, name)
        {
            _deviceInstance = deviceInstance;
            _device = new Joystick(directInput, deviceInstance.InstanceGuid);
            _deviceState = new JoystickState();

            // Note: Could set cooperative level to background non-exclusive. Needs window / control handle.

            // Determine slider and axis mappings
            IList<DeviceObjectInstance> deviceObjects = _device.GetObjects(ObjectDeviceType.Axis);
            foreach (DeviceObjectInstance deviceObject in deviceObjects)
            {
                ObjectProperties properties = _device.GetObjectPropertiesById((int)deviceObject.ObjectType);
                //try
                //{
                    //properties.SetRange(-1000, 1000);
                //}
                //catch (Exception)
                //{
                //}

                if (deviceObject.ObjectTypeGuid == ObjectGuid.Slider)
                {
                    AddSliderReader(deviceObject, properties);
                }
                else
                {
                    AddAxisReader(deviceObject, properties);
                }
            }

            // Store effects
            //_effects = _device.GetEffects();
            //foreach (EffectInfo effect in _effects)
            //{
            //    Console.WriteLine(effect.Name);
            //}

            StoreCapabilities(id, name);

            // See if it's an XInput controller
            _supportsXInput = false;
            try
            {
                _supportsXInput = _device.Properties.InterfacePath.ToLower().Contains("ig_");
            }
            catch (Exception)
            {
                // Ignore error when InterfacePath is not implemented
            }
        }

        /// <summary>
        /// Store the device's capabilities
        /// </summary>
        private void StoreCapabilities(int id, string name)
        {
            PhysicalControl control;
            StringUtils utils = new StringUtils();
            Capabilities cap = _device.Capabilities;

            // Create a physical input object which represents the capabilities of the device
            EDeviceType deviceType = GetGenericDeviceType(cap.Type);
            string description = _deviceInstance.ProductName;
            _capabilities = new PhysicalInput(id, name, description, true, deviceType, id);

            // Create DPads
            int controlID = 1;
            int index = 0;
            while (index < cap.PovCount)
            {
                string controlName = Properties.Resources.String_POV + " " + (index + 1).ToString();
                string shortName = "P" + (index + 1).ToString();
                control = new PhysicalControl(controlID++, controlName, shortName, EPhysicalControlType.POV, index, 0f);
                _capabilities.AddControl(control);
                index++;
            }

            // Create axes
            index = 0;
            foreach (SlimAxisReader axisReader in _axisReaders)
            {
                float deadZone = Math.Max(0f, Math.Min(1f, axisReader.DeadZone / axisReader.HalfRange));
                if (deadZone == 0f)
                {
                    deadZone = Constants.DefaultStickDeadZoneFraction;
                }
                string controlName = Properties.Resources.String_Axis + " " + axisReader.Axis.ToString();
                string shortName = axisReader.Axis.ToString();
                control = new PhysicalControl(controlID++, controlName, shortName, EPhysicalControlType.Axis, index, deadZone);
                _capabilities.AddControl(control);
                index++;
            }

            // Create sliders
            index = 0;
            foreach (SlimSliderReader sliderReader in _sliderReaders)
            {
                float deadZone = Math.Max(0f, Math.Min(1f, sliderReader.DeadZone / (float)sliderReader.Range));
                if (deadZone == 0f)
                {
                    deadZone = Constants.DefaultTriggerDeadZoneFraction;
                }
                string controlName = Properties.Resources.String_Slider + " " + (index + 1).ToString();
                string shortName = "S" + (index + 1).ToString();
                control = new PhysicalControl(controlID++, controlName, shortName, EPhysicalControlType.Slider, index, deadZone);
                _capabilities.AddControl(control);
                index++;
            }

            // Create buttons
            index = 0;
            while (index < cap.ButtonCount)
            {
                string controlName = Properties.Resources.String_Button + " " + (index + 1).ToString();
                string shortName = "B" + (index + 1).ToString();
                control = new PhysicalControl(controlID++, controlName, shortName, EPhysicalControlType.Button, index, 0f);
                _capabilities.AddControl(control);
                index++;
            }
        }

        /// <summary>
        /// Add a slider reader
        /// </summary>
        private void AddSliderReader(DeviceObjectInstance deviceObject, ObjectProperties properties)
        {
            // Get type of slider
            ESliderType sliderType;
            switch (deviceObject.Aspect)
            {
                case ObjectAspect.Position:
                default:
                    sliderType = ESliderType.Position; break;
                case ObjectAspect.Velocity:
                    sliderType = ESliderType.Velocity; break;
                case ObjectAspect.Acceleration:
                    sliderType = ESliderType.Acceleration; break;
                case ObjectAspect.Force:
                    sliderType = ESliderType.Force; break;
            }

            // Get slider type index
            int sliderTypeIndex = 0;
            foreach (SlimSliderReader reader in _sliderReaders)
            {
                if (reader.SliderType == sliderType)
                {
                    sliderTypeIndex++;
                }
            }

            // Some properties may not be implemented
            int deadZone = 0;
            int lowerRange = 0;
            int upperRange = 65535;
            try
            {
                deadZone = properties.DeadZone;
                lowerRange = properties.LowerRange;
                upperRange = properties.UpperRange;
            }
            catch (Exception)
            {
                // Ignore
            }
            
            // Add slider reader
            SlimSliderReader sliderReader = new SlimSliderReader(sliderType, 
                                                                    sliderTypeIndex, 
                                                                    lowerRange, 
                                                                    upperRange, 
                                                                    deadZone);
            _sliderReaders.Add(sliderReader);
        }

        /// <summary>
        /// Add an axis reader
        /// </summary>
        private void AddAxisReader(DeviceObjectInstance deviceObject, ObjectProperties properties)
        {
            // Get which axis group it is
            int axisGroup;
            switch (deviceObject.Aspect)
            {
                case ObjectAspect.Position:
                default:
                    axisGroup = (int)EAxis.X1;
                    break;
                case ObjectAspect.Velocity:
                    axisGroup = (int)EAxis.X3;
                    break;
                case ObjectAspect.Acceleration:
                    axisGroup = (int)EAxis.X5;
                    break;
                case ObjectAspect.Force:
                    axisGroup = (int)EAxis.X7;
                    break;
            }

            // Get which axis it is
            int axisIncrement = 0;
            if (deviceObject.ObjectTypeGuid == ObjectGuid.XAxis)
            {
                axisIncrement = 0;
            }
            else if (deviceObject.ObjectTypeGuid == ObjectGuid.YAxis)
            {
                axisIncrement = 1;
            }
            else if (deviceObject.ObjectTypeGuid == ObjectGuid.ZAxis)
            {
                axisIncrement = 2;
            }
            else if (deviceObject.ObjectTypeGuid == ObjectGuid.RotationalXAxis)
            {
                axisIncrement = 3;
            }
            else if (deviceObject.ObjectTypeGuid == ObjectGuid.RotationalYAxis)
            {
                axisIncrement = 4;
            }
            else if (deviceObject.ObjectTypeGuid == ObjectGuid.RotationalZAxis)
            {
                axisIncrement = 5;
            } 

            EAxis axis = (EAxis)(axisGroup + axisIncrement);

            // Some properties may not be implemented
            int deadZone = 0;
            int lowerRange = 0;
            int upperRange = 65535;
            try
            {
                deadZone = properties.DeadZone;
                lowerRange = properties.LowerRange;
                upperRange = properties.UpperRange;
            }
            catch (Exception)
            {
                // Ignore
            }

            // Add axis reader
            SlimAxisReader axisReader = new SlimAxisReader(axis,
                                                            lowerRange,
                                                            upperRange,
                                                            deadZone);
            // Add the readers in order of axis
            int i = 0;
            while (i < _axisReaders.Count)
            {
                if ((int)axis < (int)_axisReaders[i].Axis)
                {
                    break;
                }
                i++;
            }
            _axisReaders.Insert(i, axisReader);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {
            _device.Unacquire();
            _device.Dispose();
        }
        
        /// <summary>
        /// Acquire the device and initialise the stored device state
        /// </summary>
        public override void InitialiseState()
        {
            try
            {
                //_device.Properties.BufferSize = 128;
                SlimDX.Result result = _device.Acquire();
                IsConnected = result.IsSuccess;
            }
            catch (Exception)
            {
                IsConnected = false;
            }
        }

        /// <summary>
        /// Update the stored device state
        /// </summary>
        /// <returns></returns>
        public override void UpdateState()
        {
            SlimDX.Result result = ResultCode.NotAcquired;
            try
            {
                if (IsConnected)
                {
                    result = _device.Poll();
                    if (result.IsSuccess)
                    {
                        result = _device.GetCurrentState(ref _deviceState);
                    }
                }
            }
            catch (SlimDX.SlimDXException ex)
            {
                result = ex.ResultCode;
            }
            finally
            {
                IsConnected = result.IsSuccess;
                IsStateChanged = IsConnected;

                // Check error code
                if (result == ResultCode.InputLost || result == ResultCode.NotAcquired)
                {
                    InitialiseState();  // Try to reacquire                    
                }
            }
        }        

        /// <summary>
        /// Get an input value from the device
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
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
            bool[] buttons = _deviceState.GetButtons();
            if (controlIndex < buttons.Length)
            {
                isPressed = buttons[controlIndex];
            }

            return isPressed;
        }

        /// <summary>
        /// Get a slider value
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        private float GetSliderValue(int controlIndex)
        {
            float val = 0f;
            if (controlIndex < _sliderReaders.Count)
            {
                val = _sliderReaders[controlIndex].GetValue(_deviceState);
            }

            return val;
        }

        /// <summary>
        /// Get an axis value
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        private float GetAxisValue(int controlIndex)
        {
            float val = 0f;
            if (controlIndex < _axisReaders.Count)
            {
                val = _axisReaders[controlIndex].GetValue(_deviceState);
            }           

            return val;
        }

        /// <summary>
        /// Get a DPad direction
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        private ELRUDState GetDPadDirection(int controlIndex)
        {
            ELRUDState direction = ELRUDState.Centre;
            int[] dpadStates = _deviceState.GetPointOfViewControllers();
            if (controlIndex < dpadStates.Length)
            {
                switch (dpadStates[controlIndex])
                {
                    case 0: direction = ELRUDState.Up; break;
                    case 4500: direction = ELRUDState.UpRight; break;
                    case 9000: direction = ELRUDState.Right; break;
                    case 13500: direction = ELRUDState.DownRight; break;
                    case 18000: direction = ELRUDState.Down; break;
                    case 22500: direction = ELRUDState.DownLeft; break;
                    case 27000: direction = ELRUDState.Left; break;
                    case 31500: direction = ELRUDState.UpLeft; break;
                }
            }

            return direction;
        }

        /// <summary>
        /// Convert a DirectInput device type to a generic device type
        /// </summary>
        /// <param name="subType"></param>
        /// <returns></returns>
        private EDeviceType GetGenericDeviceType(DeviceType deviceType)
        {
            EDeviceType genericType;
            switch (deviceType)
            {
                case DeviceType.Mouse:
                    genericType = EDeviceType.Mouse; break;
                case DeviceType.Keyboard:
                    genericType = EDeviceType.Keyboard; break;
                case DeviceType.Joystick:
                    genericType = EDeviceType.Joystick; break;
                case DeviceType.Gamepad:
                    genericType = EDeviceType.Gamepad; break;
                case DeviceType.Driving:
                    genericType = EDeviceType.Wheel; break;
                case DeviceType.Flight:
                    genericType = EDeviceType.FlightStick; break;
                //case DeviceType.FirstPerson:
                //case DeviceType.ControlDevice:
                //case DeviceType.ScreenPointer:
                //case DeviceType.Remote:
                //case DeviceType.Supplemental:
                default:
                    genericType = EDeviceType.Controller; break;
            }

            return genericType;
        }
    }
}
