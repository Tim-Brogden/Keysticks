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
using System.Xml;
using System.Globalization;
using Keysticks.Core;

namespace Keysticks.Controls
{
    /// <summary>
    /// Represents a meta-control comprised of four buttons (e.g. ABXY) 
    /// that can be used as a directional control (e.g. to navigate a keyboard grid)
    /// </summary>
    public class ControllerButtonDiamond : DirectionalControl
    {
        // Config
        private int[] _lrudControlIDs = new int[4];

        /// <summary>
        /// Get the type of control
        /// </summary>
        public override EVirtualControlType ControlType
        {
            get { return EVirtualControlType.ButtonDiamond; }
        }

        /// <summary>
        /// Child controls
        /// </summary>
        public int[] LRUDControlIDs { get { return _lrudControlIDs; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public ControllerButtonDiamond()
            : base()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        public ControllerButtonDiamond(int id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="control"></param>
        public ControllerButtonDiamond(ControllerButtonDiamond control)
            : base(control)
        {
            if (control._lrudControlIDs != null && control._lrudControlIDs.Length == 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    _lrudControlIDs[i] = control._lrudControlIDs[i];
                }
            }
        }

        /// <summary>
        /// Get the control ID for the specified direction
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public int GetControlIDForDirection(ELRUDState direction)
        {
            int id;
            switch (direction)
            {
                case ELRUDState.Left:
                    id = _lrudControlIDs[0]; break;
                case ELRUDState.Right:
                    id = _lrudControlIDs[1]; break;
                case ELRUDState.Up:
                    id = _lrudControlIDs[2]; break;
                case ELRUDState.Down:
                    id = _lrudControlIDs[3]; break;
                default:
                    id = Constants.DefaultID; break;
            }

            return id;
        }

        /// <summary>
        /// Get the direction of a control
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ELRUDState GetDirectionForControlID(int id)
        {
            ELRUDState direction = ELRUDState.None;
            if (_lrudControlIDs[0] == id)
            {
                direction = ELRUDState.Left;
            }
            else if (_lrudControlIDs[1] == id)
            {
                direction = ELRUDState.Right;
            }
            else if (_lrudControlIDs[2] == id)
            {
                direction = ELRUDState.Up;
            }
            else if (_lrudControlIDs[3] == id)
            {
                direction = ELRUDState.Down;
            }

            return direction;
        }

        /// <summary>
        /// Set which four buttons comprise this button group
        /// </summary>
        /// <param name="lrudControlIDs"></param>
        public void SetButtonControlIDs(int[] lrudControlIDs)
        {
            if (lrudControlIDs != null && lrudControlIDs.Length == 4)
            {
                _lrudControlIDs = lrudControlIDs;
            }
        }

        /// <summary>
        /// Read from Xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            base.FromXml(element);

            XmlElement controlsElement = (XmlElement)element.SelectSingleNode("controls");
            _lrudControlIDs[0] = int.Parse(controlsElement.GetAttribute("left"), CultureInfo.InvariantCulture);
            _lrudControlIDs[1] = int.Parse(controlsElement.GetAttribute("right"), CultureInfo.InvariantCulture);
            _lrudControlIDs[2] = int.Parse(controlsElement.GetAttribute("up"), CultureInfo.InvariantCulture);
            _lrudControlIDs[3] = int.Parse(controlsElement.GetAttribute("down"), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Write to Xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            XmlElement controlsElement = doc.CreateElement("controls");
            controlsElement.SetAttribute("left", _lrudControlIDs[0].ToString(CultureInfo.InvariantCulture));
            controlsElement.SetAttribute("right", _lrudControlIDs[1].ToString(CultureInfo.InvariantCulture));
            controlsElement.SetAttribute("up", _lrudControlIDs[2].ToString(CultureInfo.InvariantCulture));
            controlsElement.SetAttribute("down", _lrudControlIDs[3].ToString(CultureInfo.InvariantCulture));
            element.AppendChild(controlsElement);
        }        
    }
}
