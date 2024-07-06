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
using System.Globalization;
using System.Xml;
using Keysticks.Core;

namespace Keysticks.Input
{
    /// <summary>
    /// Generic representation of an input device e.g. gamepad, joystick
    /// </summary>
    public class PhysicalInput : NamedItem
    {
        // Fields
        private string _description;
        private bool _isDirectInput;
        private EDeviceType _deviceType;
        private int _deviceID;
        private NamedItemList _controls = new NamedItemList();
        private BaseInputDevice _boundDevice;
        private bool _isStateChanged;

        // Properties
        public string Description { get { return _description; } }
        public bool IsDirectInput { get { return _isDirectInput; } }
        public EDeviceType DeviceType { get { return _deviceType; } }
        public int DeviceID { get { return _deviceID; } }
        public NamedItemList Controls { get { return _controls; } }
        public BaseInputDevice BoundDevice { get { return _boundDevice; } set { _boundDevice = value; } }
        public bool IsStateChanged { get { return _isStateChanged; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public PhysicalInput()
            :base()
        {            
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        public PhysicalInput(int id,
                            string name,
                            string description,
                            bool isDirectInput,
                            EDeviceType deviceType,
                            int deviceID)
            :base(id, name)
        {
            _description = description;
            _isDirectInput = isDirectInput;
            _deviceType = deviceType;
            _deviceID = deviceID;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public PhysicalInput(PhysicalInput args)
            :base(args)
        {
            _description = args._description;
            _isDirectInput = args._isDirectInput;
            _deviceType = args._deviceType;
            _deviceID = args._deviceID;

            foreach (PhysicalControl control in args._controls)
            {
                AddControl(new PhysicalControl(control));
            }
        }

        /// <summary>
        /// Add a physical control
        /// </summary>
        /// <param name="control"></param>
        public void AddControl(PhysicalControl control)
        {
            control.ParentInput = this;
            _controls.Add(control);
        }

        /// <summary>
        /// Get a control by its type and index e.g. Slider 0, Axis 3
        /// </summary>
        /// <param name="controlType"></param>
        /// <param name="controlIndex"></param>
        /// <returns></returns>
        public PhysicalControl GetControl(EPhysicalControlType controlType, int controlIndex)
        {
            PhysicalControl matchedControl = null;
            foreach (PhysicalControl control in _controls)
            {
                if (control.ControlType == controlType && control.ControlIndex == controlIndex)
                {
                    matchedControl = control;
                    break;
                }
            }

            return matchedControl;
        }

        /// <summary>
        /// Update the value of the controls using the current the device state
        /// </summary>
        public bool UpdateState()
        {
            bool bound = _boundDevice != null;
            _isStateChanged = false;

            if (bound && _boundDevice.IsStateChanged)
            {
                foreach (PhysicalControl control in _controls)
                {
                    control.UpdateState(_boundDevice);
                }
                _isStateChanged = true;
            }

            return bound;
        }

        /// <summary>
        /// Read from Xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            base.FromXml(element);

            _description = element.GetAttribute("description");
            _isDirectInput = bool.Parse(element.GetAttribute("isdirectinput"));
            _deviceType = (EDeviceType)Enum.Parse(typeof(EDeviceType), element.GetAttribute("devicetype"));
            _deviceID = int.Parse(element.GetAttribute("deviceid"), CultureInfo.InvariantCulture);
            XmlNodeList controlElements = element.SelectNodes("controls/control");
            foreach (XmlElement controlElement in controlElements)
            {
                PhysicalControl control = new PhysicalControl();
                control.FromXml(controlElement);
                AddControl(control);
            }
        }

        /// <summary>
        /// Write to Xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("description", _description);
            element.SetAttribute("isdirectinput", _isDirectInput.ToString());
            element.SetAttribute("devicetype", _deviceType.ToString());
            element.SetAttribute("deviceid", _deviceID.ToString(CultureInfo.InvariantCulture));
            XmlElement controlsElement = doc.CreateElement("controls");
            foreach (PhysicalControl control in _controls)
            {
                XmlElement controlElement = doc.CreateElement("control");
                control.ToXml(controlElement, doc);
                controlsElement.AppendChild(controlElement);
            }
            element.AppendChild(controlsElement);
        }
    }
}
