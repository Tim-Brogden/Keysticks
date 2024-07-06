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

namespace Keysticks.Event
{
    /// <summary>
    /// Stores a key event
    /// </summary>
    public class KxKeyEventArgs : KxEventArgs
    {
        // Fields
        private System.Windows.Forms.Keys _key;
        private EEventReason _eventReason = EEventReason.None;
        private bool _isInput;

        // Properties
        public System.Windows.Forms.Keys Key { get { return _key; } }
        public EEventReason EventReason { get { return _eventReason; } }
        public bool IsInput { get { return _isInput; } }
        public override EEventType EventType { get { return EEventType.Key; } }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="key"></param>
        public KxKeyEventArgs(System.Windows.Forms.Keys key, EEventReason eventReason, bool isInput)
            : base()
        {
            _key = key;
            _eventReason = eventReason;
            _isInput = isInput;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public KxKeyEventArgs(KxKeyEventArgs args)
            : base(args)
        {
            _key = args._key;
            _eventReason = args._eventReason;
            _isInput = args._isInput;
        }

        /// <summary>
        /// Get string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} {1}", _key.ToString(), _eventReason == EEventReason.Pressed ? Properties.Resources.String_Pressed : Properties.Resources.String_Released);
        }
    }
}
