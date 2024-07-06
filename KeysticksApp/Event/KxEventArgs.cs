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

namespace Keysticks.Event
{
    /// <summary>
    /// Event data base class
    /// </summary>
    public class KxEventArgs : EventArgs
    {
        // Properties
        public virtual EEventType EventType { get { return EEventType.Unknown; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        public KxEventArgs()
            :base()
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public KxEventArgs(KxEventArgs args)
            :base()
        {
        }

        /// <summary>
        /// Write to xml node
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public virtual void ToXml(XmlElement element, XmlDocument doc)
        {
            element.SetAttribute("eventtype", EventType.ToString());
        }

        /// <summary>
        /// Read from an xml node
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public virtual bool FromXml(XmlElement element)
        {
            bool success = false;
            if (element.HasAttribute("eventtype"))
            {
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
            return EventType.ToString();
        }
    }
}
