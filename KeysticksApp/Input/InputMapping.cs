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
    /// Stores a mapping from a virtual control to an input device
    /// </summary>
    public class InputMapping
    {
        // Fields
        private int _inputID;
        private EPhysicalControlType _controlType;
        private int _controlIndex;
        private EPhysicalControlOption _options;

        // Properties
        public int InputID { get { return _inputID; } set { _inputID = value; } }
        public EPhysicalControlType ControlType { get { return _controlType; } set { _controlType = value; } }
        public int ControlIndex { get { return _controlIndex; } set { _controlIndex = value; } }
        public EPhysicalControlOption Options { get { return _options; } set { _options = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public InputMapping()
        {            
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="inputID"></param>
        /// <param name="controlType"></param>
        /// <param name="controlID"></param>
        public InputMapping(int inputID, 
                            EPhysicalControlType controlType,
                            int controlIndex,
                            EPhysicalControlOption options)
        {
            _inputID = inputID;
            _controlType = controlType;
            _controlIndex = controlIndex;
            _options = options;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="mapping"></param>
        public InputMapping(InputMapping mapping)
        {
            _inputID = mapping._inputID;
            _controlType = mapping._controlType;
            _controlIndex = mapping._controlIndex;
            _options = mapping._options;
        }

        /// <summary>
        /// Read from Xml
        /// </summary>
        /// <param name="element"></param>
        public void FromXml(XmlElement element)
        {
            _inputID = int.Parse(element.GetAttribute("inputid"), CultureInfo.InvariantCulture);
            _controlType = (EPhysicalControlType)Enum.Parse(typeof(EPhysicalControlType), element.GetAttribute("controltype"));
            _controlIndex = int.Parse(element.GetAttribute("controlindex"), CultureInfo.InvariantCulture);
            _options = (EPhysicalControlOption)Enum.Parse(typeof(EPhysicalControlOption), element.GetAttribute("options"));
        }

        /// <summary>
        /// Write to Xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public void ToXml(XmlElement element, XmlDocument doc)
        {
            element.SetAttribute("inputid", _inputID.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("controltype", _controlType.ToString());
            element.SetAttribute("controlindex", _controlIndex.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("options", _options.ToString());
        }

        /// <summary>
        /// Get the ID to use for an input mapping
        /// </summary>
        /// <param name="inputID"></param>
        /// <param name="controlType"></param>
        /// <param name="controlIndex"></param>
        /// <returns></returns>
        public int ToID(bool includeOptions)
        {
            int id = (_inputID << 24) + ((int)_controlType << 16) + (_controlIndex << 8);
            if (includeOptions)
            {
                id += (int)_options;
            }

            return id;
        }

        /// <summary>
        /// Get an input mapping from an item ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static InputMapping FromID(int id)
        {
            InputMapping mapping = null;
            if (id != Constants.DefaultID)
            {
                mapping = new InputMapping();
                mapping.InputID = id >> 24;
                mapping.ControlType = (EPhysicalControlType)((id >> 16) & 0xFF);
                mapping.ControlIndex = (id >> 8) & 0xFF;
                mapping.Options = (EPhysicalControlOption)(id & 0xFF);
            }

            return mapping;
        }
    }
}
