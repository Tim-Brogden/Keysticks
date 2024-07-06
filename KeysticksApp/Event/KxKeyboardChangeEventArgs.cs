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
using Keysticks.Core;

namespace Keysticks.Event
{
    /// <summary>
    /// Event data for reporting that the keyboard layout has changed
    /// </summary>
    public class KxKeyboardChangeEventArgs : KxEventArgs, IKeyboardContext
    {
        // Fields
        private IntPtr _dwHKL;
        
        // Properties
        public IntPtr KeyboardHKL { get { return _dwHKL; } }
        public override EEventType EventType { get { return EEventType.KeyboardLayoutChange; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public KxKeyboardChangeEventArgs(IntPtr dwHKL)
            : base()
        {
            _dwHKL = dwHKL;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public KxKeyboardChangeEventArgs(KxKeyboardChangeEventArgs args)
            :base(args)
        {
            _dwHKL = args._dwHKL;
        }

        /// <summary>
        /// Get string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Handle: {0}", _dwHKL != null ? _dwHKL.ToString() : "null");
        }
    }
}
