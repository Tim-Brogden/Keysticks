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
    /// Event data reported by input controls
    /// </summary>
    public class KxSourceEventArgs : KxControlEventArgs
    {
        // Fields
        private bool _inUse;
        private int _sourceID = Constants.ID1;
        private EEventReason _eventReason;
        private ParamValue _param0;
        private ParamValue _param1;

        // Properties        
        public bool InUse { get { return _inUse; } set { _inUse = value; } }
        public int SourceID { get { return _sourceID; } set { _sourceID = value; } }
        public EEventReason EventReason { get { return _eventReason; } set { _eventReason = value; } }
        public ParamValue Param0 { get { return _param0; } set { _param0 = value; } }
        public ParamValue Param1 { get { return _param1; } set { _param1 = value; } }
        public override EEventType EventType { get { return EEventType.Source; } }
       
        /// <summary>
        /// Default constructor
        /// </summary>
        public KxSourceEventArgs()
            :base()
        {
        }

        /// <summary>
        /// Constructor without data parameters
        /// </summary>
        public KxSourceEventArgs(int sourceID,
                                    EVirtualControlType controlType,
                                    int controlID,
                                    EControlSetting settingType,
                                    EEventReason eventReason,
                                    ELRUDState lrudState)
            : base(controlType, controlID, settingType, lrudState)
        {
            _sourceID = sourceID;
            _eventReason = eventReason;
        }

        /// <summary>
        /// Constructor using control args
        /// </summary>
        public KxSourceEventArgs(int sourceID,                                    
                                    EEventReason eventReason,
                                    KxControlEventArgs args)
            : base(args.ControlType, args.ControlID, args.SettingType, args.LRUDState)
        {
            _sourceID = sourceID;
            _eventReason = eventReason;
        }

        /// <summary>
        /// Full constructor with data parameters
        /// </summary>
        public KxSourceEventArgs(int sourceID,
                                    EVirtualControlType controlType,
                                    int controlID,
                                    EControlSetting settingType,
                                    EEventReason eventReason,
                                    ELRUDState lrudState,
                                    ParamValue param0,
                                    ParamValue param1)
            : base(controlType, controlID, settingType, lrudState)
        {
            _sourceID = sourceID;
            _eventReason = eventReason;
            _param0 = param0;
            _param1 = param1;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public KxSourceEventArgs(KxSourceEventArgs args)
            : base(args)
        {
            _sourceID = args._sourceID;
            _eventReason = args._eventReason;
            if (args._param0 != null)
            {
                _param0 = new ParamValue(args._param0);
            }
            if (args._param1 != null)
            {
                _param1 = new ParamValue(args._param1);
            }
        }

        /// <summary>
        /// Get a parameter attached to this event
        /// </summary>
        /// <param name="paramIndex"></param>
        /// <returns></returns>
        public ParamValue GetParamValue(int paramIndex)
        {
            ParamValue paramValue;
            if (paramIndex == 0)
            {
                paramValue = _param0;
            }
            else if (paramIndex == 1)
            {
                paramValue = _param1;
            }
            else
            {
                paramValue = null;
            }

            return paramValue;
        }

        /// <summary>
        /// Write to xml node
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("eventtype", EventType.ToString());
            element.SetAttribute("sourceid", _sourceID.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("eventreason", _eventReason.ToString());
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
                _sourceID = int.Parse(element.GetAttribute("sourceid"), CultureInfo.InvariantCulture);
                if (element.HasAttribute("eventreason"))
                {
                    _eventReason = (EEventReason)Enum.Parse(typeof(EEventReason), element.GetAttribute("eventreason"));
                }
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
            string direction = "";
            switch (_eventReason)
            {
                case EEventReason.Directed:
                case EEventReason.DirectedLong:
                case EEventReason.DirectionRepeated:
                case EEventReason.DirectedShort:
                case EEventReason.Undirected:
                    direction = utils.DirectionToString(LRUDState);
                    break;
            }

            string str = string.Format("{0} {1} {2} {3} {4} {5}",
                                        Properties.Resources.String_Player,
                                        _sourceID.ToString(CultureInfo.InvariantCulture),
                                        utils.EventReasonToString(_eventReason),
                                        utils.ControlTypeToString(ControlType),
                                        ControlID,
                                        direction);

            return str;
        }
    }
}
