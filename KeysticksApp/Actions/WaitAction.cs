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
using Keysticks.Sys;

namespace Keysticks.Actions
{
    /// <summary>
    /// Action for waiting for an amount of time, for use in a series of actions in an action list
    /// </summary>
    public class WaitAction : BaseAction
    {
        // Configuration
        private long _waitTimeTicks = 1000L * TimeSpan.TicksPerMillisecond;

        // State
        private long _startTimeTicks;

        // Properties
        public int WaitTimeMS { get { return (int)(_waitTimeTicks / TimeSpan.TicksPerMillisecond); } set { _waitTimeTicks = value * TimeSpan.TicksPerMillisecond; } }

        /// <summary>
        /// Return what type of action this is
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.Wait; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public WaitAction()
            : base()
        {
        }

        /// <summary>
        /// Return the name of the action
        /// </summary>
        /// <returns></returns>
        public override string Name
        {
            get
            {
                return Properties.Resources.String_WaitFor + string.Format(" {0:0.00}s", WaitTimeMS * 0.001);
            }
        }

        /// <summary>
        /// Get the icon to use
        /// </summary>
        public override EAnnotationImage IconRef
        {
            get
            {
                return EAnnotationImage.Wait;
            }
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            base.FromXml(element);

            _waitTimeTicks = TimeSpan.TicksPerMillisecond * long.Parse(element.GetAttribute("duration"), System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("duration", (_waitTimeTicks / TimeSpan.TicksPerMillisecond).ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Start the action
        /// </summary>
        public override void Start(IStateManager parent, KxSourceEventArgs args)
        {
            _startTimeTicks = DateTime.UtcNow.Ticks;

            IsOngoing = true;
        }

        /// <summary>
        /// Continue the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void Continue(IStateManager parent, KxSourceEventArgs args)
        {
            if (DateTime.UtcNow.Ticks - _startTimeTicks > _waitTimeTicks)
            {
                // Finished waiting
                IsOngoing = false;
            }
        }
    }
}
