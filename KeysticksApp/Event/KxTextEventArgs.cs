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
    /// Report a text event
    /// </summary>
    public class KxTextEventArgs : KxEventArgs
    {
        // Fields
        public string Text;

        // Properties
        public override EEventType EventType { get { return EEventType.Text; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        public KxTextEventArgs(string text)
            : base()
        {
            Text = text;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public KxTextEventArgs(KxTextEventArgs args)
            : base(args)
        {
            if (args.Text != null)
            {
                Text = string.Copy(args.Text);
            }
        }

        /// <summary>
        /// Get string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Text != null ? Text : "";
        }
    }
}
