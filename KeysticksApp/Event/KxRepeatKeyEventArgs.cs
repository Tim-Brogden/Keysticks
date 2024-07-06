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
    /// Repeated keypress event data
    /// </summary>
    public class KxRepeatKeyEventArgs : KxEventArgs
    {
        // Fields
        public System.Windows.Forms.Keys Key;
        public uint Count;

        // Properties
        public override EEventType EventType { get { return EEventType.RepeatKey; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        public KxRepeatKeyEventArgs(System.Windows.Forms.Keys key, uint count)
            : base()
        {
            Key = key;
            Count = count;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public KxRepeatKeyEventArgs(KxRepeatKeyEventArgs args)
            : base(args)
        {
            Key = args.Key;
            Count = args.Count;
        }

        /// <summary>
        /// Get string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} (x{1})", Key.ToString(), Count);
        }
    }
}
