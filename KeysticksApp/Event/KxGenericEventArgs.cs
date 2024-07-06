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
using Keysticks.Core;

namespace Keysticks.Event
{
    /// <summary>
    /// Generic event without data
    /// </summary>
    public class KxGenericEventArgs : KxEventArgs
    {
        // Fields
        private EEventType _eventType;

        // Properties
        public override EEventType EventType { get { return _eventType; } }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="eventType"></param>
        public KxGenericEventArgs(EEventType eventType)
            : base()
        {
            _eventType = eventType;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public KxGenericEventArgs(KxGenericEventArgs args)
            :base(args)
        {
            _eventType = args._eventType;
        }
    }
}
