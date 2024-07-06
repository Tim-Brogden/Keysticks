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
using System.Xml;
using Keysticks.Core;
using Keysticks.Event;

namespace Keysticks.Config
{
    /// <summary>
    /// Defines a control and its directional behaviour
    /// </summary>
    public class GeneralisedControl
    {
        // Fields
        private EDirectionMode _directionMode;
        private KxControlEventArgs _referenceControl;

        // Properties
        public EDirectionMode DirectionMode { get { return _directionMode; } }
        public KxControlEventArgs ReferenceControl { get { return _referenceControl; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public GeneralisedControl()
        {
            _directionMode = EDirectionMode.None;
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="directionMode"></param>
        /// <param name="referenceControl"></param>
        public GeneralisedControl(EDirectionMode directionMode, KxControlEventArgs referenceControl)
        {
            _directionMode = directionMode;
            _referenceControl = referenceControl;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="control"></param>
        public GeneralisedControl(GeneralisedControl control)
        {
            _directionMode = control._directionMode;
            if (control._referenceControl != null)
            {
                _referenceControl = new KxControlEventArgs(control._referenceControl);
            }
        }

        /// <summary>
        /// From xml
        /// </summary>
        /// <param name="element"></param>
        public void FromXml(XmlElement element)
        {
            _directionMode = (EDirectionMode)Enum.Parse(typeof(EDirectionMode), element.GetAttribute("directionmode"));

            KxControlEventArgs args = new KxControlEventArgs();
            if (args.FromXml(element))
            {
                _referenceControl = args;
            }
        }

        /// <summary>
        /// To xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public void ToXml(XmlElement element, XmlDocument doc)
        {            
            element.SetAttribute("directionmode", _directionMode.ToString());
            if (_referenceControl != null)
            {
                _referenceControl.ToXml(element, doc);
            }
        }

        public int ToID()
        {
            return _referenceControl != null ? _referenceControl.ToID() : 0;
        }
    }
}
