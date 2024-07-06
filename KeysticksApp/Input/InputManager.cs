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
using System.Diagnostics;
using SlimDX.DirectInput;
using SlimDX.XInput;
using Keysticks.Config;
using Keysticks.Controls;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Sources;

namespace Keysticks.Input
{
    /// <summary>
    /// Binds input devices to virtual controls and retrieves their state
    /// </summary>
    public class InputManager : IDisposable
    {
        // Fields
        private IThreadManager _threadManager;
        private DirectInput _directInput;
        private NamedItemList _deviceList;
        private Profile _profile;
        private int _loggingLevel = Constants.DefaultMessageLoggingLevel;

        /// <summary>
        /// Constructor
        /// </summary>
        public InputManager()
        {
            _directInput = new DirectInput();
            _deviceList = new NamedItemList();
        }

        /// <summary>
        /// Set the thread manager to allow event reporting
        /// </summary>
        /// <param name="threadManager"></param>
        public void SetThreadManager(IThreadManager threadManager)
        {
            _threadManager = threadManager;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            UnbindDevices();
            _directInput.Dispose();
        }

        /// <summary>
        /// Set config
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            _loggingLevel = appConfig.GetIntVal(Constants.ConfigMessageLoggingLevel, Constants.DefaultMessageLoggingLevel);
        }

        /// <summary>
        /// Get the list of connected devices
        /// </summary>
        public NamedItemList GetAvailableInputs()
        {
            NamedItemList inputsList = new NamedItemList();

            // Create temporary device list
            foreach (BaseInputDevice device in _deviceList)
            {
                inputsList.Add(device.Capabilities);
            }

            return inputsList;
        }

        /// <summary>
        /// Set the profile
        /// </summary>
        /// <returns></returns>
        public void SetProfile(Profile profile)
        {
            _profile = profile;
        }

        /// <summary>
        /// Bind devices
        /// </summary>
        /// <returns>Whether a device is bound to each physical input</returns>
        public bool BindProfile()
        {
            bool isBound = true;

            // Bind inputs to devices
            if (_profile != null)
            {
                try
                {
                    StringBuilder trace = new StringBuilder();

                    foreach (BaseSource source in _profile.VirtualSources)
                    {
                        // Bind physical inputs to devices
                        trace.AppendLine(string.Format("Binding source {0} inputs:", source.ID));
                        foreach (PhysicalInput input in source.PhysicalInputs)
                        {
                            BaseInputDevice device = (BaseInputDevice)_deviceList.GetItemByID(input.DeviceID);
                            input.BoundDevice = device;
                            trace.Append("- " + string.Format("Binding input {0} to device {1}...", input.ID, input.DeviceID) + " ");
                            trace.AppendLine(device != null ? "Found" : "Not found");
                            if (device != null)
                            {
                                device.IsRequired = true;
                            }
                            else
                            {
                                isBound = false;
                            }
                        }
                    
                        // Bind virtual controls to device controls
                        trace.AppendLine("- Binding virtual controls...");
                        foreach (BaseControl control in source.InputControls)
                        {
                            int numMappings = control.NumInputMappings;
                            PhysicalControl[] boundControls = new PhysicalControl[numMappings];
                            for (int i = 0; i < numMappings; i++)
                            {
                                PhysicalControl physicalControl = null;
                                InputMapping mapping = control.GetInputMapping(i);
                                if (mapping != null)
                                {
                                    PhysicalInput input = (PhysicalInput)source.PhysicalInputs.GetItemByID(mapping.InputID);
                                    if (input != null && input.BoundDevice != null)
                                    {
                                        physicalControl = (PhysicalControl)input.GetControl(mapping.ControlType, mapping.ControlIndex);
                                        if (physicalControl != null)
                                        {
                                            // Check that the bound device supports this type of control
                                            if (null == input.BoundDevice.Capabilities.GetControl(physicalControl.ControlType, physicalControl.ControlIndex))
                                            {
                                                // Device doesn't support this control                                        
                                                physicalControl = null;
                                            }
                                        }
                                    }
                                }
                                boundControls[i] = physicalControl;
                            }
                            control.SetBoundControls(boundControls);

                            if (control.ControlType != EVirtualControlType.ButtonDiamond)
                            {
                                trace.AppendLine(string.Format("  virtual control {0} ({1}):", control.ID, control.Name));
                                for (int k = 0; k < boundControls.Length; k++)
                                {
                                    PhysicalControl p = boundControls[k];
                                    if (p != null)
                                    {
                                        trace.AppendLine(string.Format("    {0}: bind to physical control {1} ({2})", k, p.ID, p.Name));
                                    }
                                    else
                                    {
                                        trace.AppendLine(string.Format("    {0}: unbound", k));
                                    }
                                }
                            }
                        }

                        trace.AppendLine();
                    }

                    // Diagnostics
                    SendDebugMessage("InputManager BindProfile", trace.ToString());
                    //Trace.WriteLine(trace.ToString());
                }
                catch (Exception ex)
                {
                    isBound = false;
                    HandleError(Properties.Resources.E_BindingProfile, ex);                    
                }
            }            

            return isBound;
        }

        /// <summary>
        /// Unbind devices
        /// </summary>
        private void UnbindDevices()
        {
            try
            {
                foreach (BaseInputDevice device in _deviceList)
                {
                    device.Dispose();
                }
            }
            finally
            {
                _deviceList.Clear();
            }
        }
        
        /// <summary>
        /// Update profile state from device values
        /// </summary>
        public bool UpdateState()
        {
            bool success = true;
            foreach (BaseInputDevice device in _deviceList)
            {
                if (device.IsRequired)
                {
                    device.UpdateState();
                    success &= device.IsConnected;
                }
            }            

            return success;
        }

        /// <summary>
        /// Get the list of connected devices
        /// </summary>
        /// <returns></returns>
        public bool RefreshConnectedDeviceList(bool addAll)
        {
            bool changed = false;

            StringBuilder trace = new StringBuilder();
            trace.AppendLine(string.Format("Add all devices option: {0}", addAll));

            List<int> requiredDeviceIDs = new List<int>();
            if (!addAll)
            {
                // Get a list of required device IDs
                foreach (BaseSource source in _profile.VirtualSources)
                {
                    foreach (PhysicalInput input in source.PhysicalInputs)
                    {
                        if (!requiredDeviceIDs.Contains(input.DeviceID))
                        {
                            requiredDeviceIDs.Add(input.DeviceID);
                        }
                    }
                }

                trace.Append("Required device ID list: ");
                bool first = true;
                foreach (int reqid in requiredDeviceIDs)
                {
                    if (!first) trace.Append(',');
                    trace.Append(reqid);
                    first = false;
                }
                trace.AppendLine();
            }

            trace.Append("Current device ID list: ");
            bool isFirst = true;
            foreach (BaseInputDevice d in _deviceList)
            {
                if (!isFirst) trace.Append(',');
                trace.Append(d.ID);
                isFirst = false;
            }
            trace.AppendLine();

            // Remove any devices that have errored or are no longer required
            int i = 0;
            while (i < _deviceList.Count)
            {
                BaseInputDevice device = (BaseInputDevice)_deviceList[i];
                bool remove = !device.IsConnected || (!addAll && !requiredDeviceIDs.Contains(device.ID));
                if (remove)
                {
                    trace.AppendLine(string.Format("Removing device {0} ({1})", device.ID, device.IsConnected ? "connected" : "not connected"));

                    device.Dispose();
                    _deviceList.RemoveAt(i);
                    changed = true;
                }
                else
                {
                    i++;
                }
            }

            // Add XInput devices
            for (int deviceID = 1; deviceID <= 4; deviceID++)
            {
                try
                {
                    bool isRequired = addAll || requiredDeviceIDs.Contains(deviceID);
                    if (isRequired && _deviceList.GetItemByID(deviceID) == null)
                    {
                        // Device is required
                        string name = Properties.Resources.String_Controller + " " + deviceID;
                        SlimXInputDevice device = new SlimXInputDevice(deviceID, name, (UserIndex)deviceID - 1);
                        if (device.IsConnected)
                        {
                            device.InitialiseState();
                            _deviceList.Add(device);
                            changed = true;

                            trace.AppendLine(string.Format("Found XInput device {0}... added (connected)", deviceID));
                        }
                    }
                }
                catch (Exception ex)
                {
                    HandleError(Properties.Resources.E_AssessingXInputDevice, ex);
                }
            }

            // See if DirectInput is required
            bool diRequired = addAll;
            if (!addAll)
            {
                foreach (int id in requiredDeviceIDs)
                {
                    if (id > 4 && _deviceList.GetItemByID(id) == null)
                    {
                        diRequired = true;
                        break;
                    }
                }
            }

            // Add DirectInput devices if required
            if (diRequired)
            {
                // Select required non-XInput devices
                int diID = 5;
                IList<DeviceInstance> deviceInstances = _directInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly);
                foreach (DeviceInstance deviceInstance in deviceInstances)
                {
                    try
                    {
                        bool added = false;
                        string name = Properties.Resources.String_Controller + " " + diID;
                        SlimDirectInputDevice device = new SlimDirectInputDevice(diID, name, deviceInstance, _directInput);
                        if (!device.SupportsXInput)
                        {
                            trace.Append(string.Format("Found DirectInput device {0}... ", diID));

                            bool isRequired = addAll || requiredDeviceIDs.Contains(diID);
                            if (isRequired)
                            {
                                if (_deviceList.GetItemByID(diID) == null)
                                {
                                    device.InitialiseState();
                                    _deviceList.Add(device);
                                    added = true;
                                    changed = true;
                                    trace.AppendLine(string.Format("added ({0})", device.IsConnected ? "connected" : "disconnected"));
                                }
                                else
                                {
                                    trace.AppendLine("already added");
                                }
                            }
                            else
                            {
                                trace.AppendLine("not required");
                            }

                            diID++;
                        }

                        // Dispose of device if not used
                        if (!added)
                        {
                            device.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleError(Properties.Resources.E_AssessingDirectInputDevice, ex);                        
                    }
                }                
            }

            if (changed)
            {
                SendDebugMessage("InputManager RefreshConnectedDeviceList", trace.ToString());
                //Trace.WriteLine(trace.ToString());
            }

            return changed;
        }

        /// <summary>
        /// Handle an error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        private void HandleError(string message, Exception ex)
        {
            if (_threadManager != null)
            {
                _threadManager.SubmitUIEvent(new KxErrorMessageEventArgs(message, ex));
            }
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
    }
}
