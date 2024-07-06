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

namespace Keysticks.Event
{
    /// <summary>
    /// Event data provided by input controls
    /// </summary>
    public class KxControlEventArgs : KxEventArgs
    {
        // Fields
        private EVirtualControlType _controlType;
        private int _controlID = Constants.ID1;
        private EControlSetting _settingType;
        private ELRUDState _lrudState;

        // Properties        
        public EVirtualControlType ControlType { get { return _controlType; } set { _controlType = value; } }
        public int ControlID { get { return _controlID; } set { _controlID = value; } }
        public EControlSetting SettingType { get { return _settingType; } set { _settingType = value; } }
        public ELRUDState LRUDState { get { return _lrudState; } set { _lrudState = value; } }
        public override EEventType EventType { get { return EEventType.Control; } }
       
        /// <summary>
        /// Default constructor
        /// </summary>
        public KxControlEventArgs()
            :base()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        public KxControlEventArgs(EVirtualControlType controlType,
                                    int controlID,
                                    EControlSetting settingType,
                                    ELRUDState lrudState)
            :base()
        {
            _controlType = controlType;
            _controlID = controlID;
            _settingType = settingType;
            _lrudState = lrudState;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public KxControlEventArgs(KxControlEventArgs args)
            : base(args)
        {
            _controlType = args._controlType;
            _controlID = args._controlID;
            _settingType = args._settingType;
            _lrudState = args.LRUDState;
        }

        /// <summary>
        /// Convert to a specific ID (includes direction)
        /// </summary>
        /// <returns></returns>
        public int ToID()
        {
            int id = ((int)_controlType) |
                        (_controlID << 4) |
                        ((int)_settingType << 12) |
                        ((int)_lrudState << 16);
            return id;
        }

        /// <summary>
        /// Convert to a general control ID based on control type and ID
        /// </summary>
        /// <returns></returns>
        public int ToGeneralID()
        {
            return ToID() & 0xFFF;
        }

        /// <summary>
        /// Write to xml node
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("controltype", _controlType.ToString());
            element.SetAttribute("controlid", _controlID.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("settingtype", _settingType.ToString());
            element.SetAttribute("lrudstate", _lrudState.ToString());
        }

        /// <summary>
        /// Read from an xml node
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public override bool FromXml(XmlElement element)
        {
            bool success = base.FromXml(element);
            if (success)
            {
                _controlType = (EVirtualControlType)Enum.Parse(typeof(EVirtualControlType), element.GetAttribute("controltype"));
                _controlID = int.Parse(element.GetAttribute("controlid"), CultureInfo.InvariantCulture);
                if (element.HasAttribute("settingtype"))
                {
                    _settingType = (EControlSetting)Enum.Parse(typeof(EControlSetting), element.GetAttribute("settingtype"));
                }
                _lrudState = (ELRUDState)Enum.Parse(typeof(ELRUDState), element.GetAttribute("lrudstate"));

                success = true;
            }

            return success;
        }

        /// <summary>
        /// Get string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringUtils utils = new StringUtils();
            string str = string.Format("{0} {1} {2}", utils.ControlTypeToString(_controlType), _controlID, utils.DirectionToString(_lrudState));

            return str;
        }
    }
}
