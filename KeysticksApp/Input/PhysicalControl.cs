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
using System.Globalization;
using System.Xml;
using Keysticks.Core;

namespace Keysticks.Input
{
    /// <summary>
    /// Represents a physical control on a gamepad or joystick (e.g. axis, button, slider, POV switch)
    /// </summary>
    public class PhysicalControl : NamedItem
    {
        // Fields
        private string _shortName;
        private EPhysicalControlType _controlType;
        private int _controlIndex;
        private float _deadZone;
        private PhysicalInput _parentInput;
        private ParamValue _currentValue = new ParamValue();

        // Properties
        public PhysicalInput ParentInput { set { _parentInput = value; } }
        public string ShortName { get { return _shortName; } }
        public EPhysicalControlType ControlType { get { return _controlType; } }
        public int ControlIndex { get { return _controlIndex; } }
        public float DeadZone
        {
            get { return _deadZone; }
            set { _deadZone = value; }
        }
        public ParamValue CurrentValue { get { return _currentValue; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public PhysicalControl()
            : base()
        {            
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="inputIndex"></param>
        /// <param name="controlType"></param>
        /// <param name="controlIndex"></param>
        public PhysicalControl(int id,
                            string name, 
                            string shortName,
                            EPhysicalControlType controlType,
                            int controlIndex,
                            float deadZone)
            : base(id, name)
        {
            _shortName = shortName;
            _controlType = controlType;
            _controlIndex = controlIndex;
            _deadZone = deadZone;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public PhysicalControl(PhysicalControl args)
            :base(args)
        {
            _shortName = args._shortName;
            _controlType = args._controlType;
            _controlIndex = args._controlIndex;
            _deadZone = args._deadZone;
        }

        /// <summary>
        /// Update the control from the device state
        /// </summary>
        /// <param name="device"></param>
        public void UpdateState(BaseInputDevice device)
        {
            device.GetInputValue(this, ref _currentValue);
        }

        /// <summary>
        /// Get the current control state (bool)
        /// </summary>
        /// <returns></returns>
        public bool GetBoolVal()
        {
            bool val = false;
            if (_currentValue.DataType == EDataType.Bool)
            {
                val = (bool)_currentValue.Value;
            }

            return val;
        }

        /// <summary>
        /// Get the current control state (LRUD)
        /// </summary>
        /// <returns></returns>
        public ELRUDState GetDirectionVal()
        {
            ELRUDState val = ELRUDState.None;
            if (_currentValue.DataType == EDataType.LRUD)
            {
                val = (ELRUDState)_currentValue.Value;
            }

            return val;
        }

        /// <summary>
        /// Get the current control state (float)
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public float GetFloatVal(EPhysicalControlOption option)
        {
            float val = 0f;
            if (_currentValue.DataType == EDataType.Float)
            {
                val = (float)_currentValue.Value;
                if (_controlType == EPhysicalControlType.Slider)
                {
                    if (option == EPhysicalControlOption.Inverted)
                    {
                        // Inverted
                        val = 1f - val;
                    }
                }
                else if (_controlType == EPhysicalControlType.Axis)
                {
                    switch (option)
                    {
                        case EPhysicalControlOption.Inverted:
                            val = -val; break;
                        case EPhysicalControlOption.PositiveSide:
                            val = Math.Max(val, 0f); break;
                        case EPhysicalControlOption.NegativeSide:
                            val = Math.Max(-val, 0f); break;
                    }
                }
            }

            return val;
        }

        /// <summary>
        /// Whether the control is in a pressed / non-centre position
        /// </summary>
        /// <param name="paramValue"></param>
        /// <returns></returns>
        public bool HasNonDefaultValue()
        {
            bool isNonDefault = false;
            switch (_currentValue.DataType)
            {
                case EDataType.Bool:
                    isNonDefault = ((bool)_currentValue.Value) == true;
                    break;
                case EDataType.LRUD:
                    ELRUDState direction = (ELRUDState)_currentValue.Value;
                    isNonDefault = direction != ELRUDState.Centre && direction != ELRUDState.None;
                    break;
                case EDataType.Float:
                    isNonDefault = Math.Abs((float)_currentValue.Value) > _deadZone;
                    break;
            }

            return isNonDefault;
        }
    
        /// <summary>
        /// Read from Xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            base.FromXml(element);

            _shortName = element.GetAttribute("shortname");
            _controlType = (EPhysicalControlType)Enum.Parse(typeof(EPhysicalControlType), element.GetAttribute("controltype"));
            _controlIndex = int.Parse(element.GetAttribute("controlindex"), CultureInfo.InvariantCulture);
            _deadZone = float.Parse(element.GetAttribute("deadzone"), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Write to Xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("shortname", _shortName);
            element.SetAttribute("controltype", _controlType.ToString());
            element.SetAttribute("controlindex", _controlIndex.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("deadzone", _deadZone.ToString(CultureInfo.InvariantCulture));
        }
    }
    
}
