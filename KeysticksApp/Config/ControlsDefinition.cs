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
using System.Xml;
using Keysticks.Core;
using Keysticks.Event;

namespace Keysticks.Config
{
    /// <summary>
    /// Defines the controls for a keyboard control set
    /// </summary>
    public class ControlsDefinition
    {
        // Fields
        private EControlRestrictions _restrictions = EControlRestrictions.None;
        private GeneralisedControl _navigationControl;
        private GeneralisedControl _selectionControl;

        // Properties
        public EControlRestrictions Restrictions { get { return _restrictions; } }
        public GeneralisedControl NavigationControl { get { return _navigationControl; } }
        public GeneralisedControl SelectionControl { get { return _selectionControl; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public ControlsDefinition()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="navigationControl"></param>
        /// <param name="selectionControl"></param>
        /// <param name="restrictions"></param>
        public ControlsDefinition(GeneralisedControl navigationControl,
                                    GeneralisedControl selectionControl,
                                    EControlRestrictions restrictions)
        {
            _navigationControl = navigationControl;
            _selectionControl = selectionControl;
            _restrictions = restrictions;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="controls"></param>
        public ControlsDefinition(ControlsDefinition controls)
        {
            if (controls._navigationControl != null)
            {
                _navigationControl = new GeneralisedControl(controls._navigationControl);
            }
            if (controls._selectionControl != null)
            {
                _selectionControl = new GeneralisedControl(controls._selectionControl);
            }
            _restrictions = controls._restrictions;
        }

        public void FromXml(XmlElement element)
        {
            _restrictions = (EControlRestrictions)Enum.Parse(typeof(EControlRestrictions), element.GetAttribute("restrictions"));
            XmlElement childElement = (XmlElement)element.SelectSingleNode("navigation");
            if (childElement != null)
            {
                _navigationControl = new GeneralisedControl();
                _navigationControl.FromXml(childElement);
            }
            else if (element.HasAttribute("navigationtype") && element.HasAttribute("navigation"))
            {
                // Upgrade from previous versions
                EDirectionMode directionMode = (EDirectionMode)Enum.Parse(typeof(EDirectionMode), element.GetAttribute("navigationtype"));
                EGeneralisedControl eControl = (EGeneralisedControl)Enum.Parse(typeof(EGeneralisedControl), element.GetAttribute("navigation"));
                StringUtils utils = new StringUtils();
                KxControlEventArgs inputControl = EGeneralisedControlToControl(eControl);
                _navigationControl = new GeneralisedControl(directionMode, inputControl);
            }

            childElement = (XmlElement)element.SelectSingleNode("selection");
            if (childElement != null)
            {
                _selectionControl = new GeneralisedControl();
                _selectionControl.FromXml(childElement);
            }
            else if (element.HasAttribute("selectiontype") && element.HasAttribute("selection"))
            {
                // Upgrade from previous versions
                EDirectionMode directionMode = (EDirectionMode)Enum.Parse(typeof(EDirectionMode), element.GetAttribute("selectiontype"));
                EGeneralisedControl eControl = (EGeneralisedControl)Enum.Parse(typeof(EGeneralisedControl), element.GetAttribute("selection"));
                StringUtils utils = new StringUtils();
                KxControlEventArgs inputControl = EGeneralisedControlToControl(eControl);
                _selectionControl = new GeneralisedControl(directionMode, inputControl);
            }
        }

        public void ToXml(XmlElement element, XmlDocument doc)
        {
            element.SetAttribute("restrictions", _restrictions.ToString());
            if (_navigationControl != null)
            {
                XmlElement childElement = doc.CreateElement("navigation");
                _navigationControl.ToXml(childElement, doc);
                element.AppendChild(childElement);
            }
            if (_selectionControl != null)
            {
                XmlElement childElement = doc.CreateElement("selection");
                _selectionControl.ToXml(childElement, doc);
                element.AppendChild(childElement);
            }
        }

        /// <summary>
        /// Legacy method: convert generalised control to specific control
        /// </summary>
        /// <param name="eControl"></param>
        /// <returns></returns>
        private KxControlEventArgs EGeneralisedControlToControl(EGeneralisedControl eControl)
        {
            KxControlEventArgs inputControl = null;
            switch (eControl)
            {
                case EGeneralisedControl.LeftThumb:
                case EGeneralisedControl.LeftThumbDirection:
                    inputControl = new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID1, EControlSetting.None, ELRUDState.None);
                    break;
                case EGeneralisedControl.RightThumb:
                case EGeneralisedControl.RightThumbDirection:
                    inputControl = new KxControlEventArgs(EVirtualControlType.Stick, Constants.ID2, EControlSetting.None, ELRUDState.None);
                    break;
                case EGeneralisedControl.DPad:
                case EGeneralisedControl.DPadDirection:
                    inputControl = new KxControlEventArgs(EVirtualControlType.DPad, Constants.ID1, EControlSetting.None, ELRUDState.None);
                    break;
                case EGeneralisedControl.ABXYButtons:
                    inputControl = new KxControlEventArgs(EVirtualControlType.ButtonDiamond, Constants.ID1, EControlSetting.None, ELRUDState.None);
                    break;
                case EGeneralisedControl.Buttons:
                case EGeneralisedControl.A:
                    inputControl = new KxControlEventArgs(EVirtualControlType.Button, (int)EButtonState.A, EControlSetting.None, ELRUDState.None);
                    break;
                case EGeneralisedControl.B:
                    inputControl = new KxControlEventArgs(EVirtualControlType.Button, (int)EButtonState.B, EControlSetting.None, ELRUDState.None);
                    break;
                case EGeneralisedControl.X:
                    inputControl = new KxControlEventArgs(EVirtualControlType.Button, (int)EButtonState.X, EControlSetting.None, ELRUDState.None);
                    break;
                case EGeneralisedControl.Y:
                    inputControl = new KxControlEventArgs(EVirtualControlType.Button, (int)EButtonState.Y, EControlSetting.None, ELRUDState.None);
                    break;
                case EGeneralisedControl.LeftShoulder:
                    inputControl = new KxControlEventArgs(EVirtualControlType.Button, (int)EButtonState.LeftShoulder, EControlSetting.None, ELRUDState.None);
                    break;
                case EGeneralisedControl.RightShoulder:
                    inputControl = new KxControlEventArgs(EVirtualControlType.Button, (int)EButtonState.RightShoulder, EControlSetting.None, ELRUDState.None);
                    break;
                case EGeneralisedControl.Back:
                    inputControl = new KxControlEventArgs(EVirtualControlType.Button, (int)EButtonState.Back, EControlSetting.None, ELRUDState.None);
                    break;
                case EGeneralisedControl.Start:
                    inputControl = new KxControlEventArgs(EVirtualControlType.Button, (int)EButtonState.Start, EControlSetting.None, ELRUDState.None);
                    break;
                // Note: left and right stick buttons not required here (treated as stick controls)
                case EGeneralisedControl.LeftTrigger:
                    inputControl = new KxControlEventArgs(EVirtualControlType.Trigger, Constants.ID1, EControlSetting.None, ELRUDState.None);
                    break;
                case EGeneralisedControl.RightTrigger:
                    inputControl = new KxControlEventArgs(EVirtualControlType.Trigger, Constants.ID2, EControlSetting.None, ELRUDState.None);
                    break;
            }

            return inputControl;
        }
    }
}
