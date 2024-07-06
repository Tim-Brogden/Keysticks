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
    /// Error event data
    /// </summary>
    public class KxErrorMessageEventArgs : KxEventArgs
    {
        // Fields
        private string _message;
        private Exception _exception;

        // Properties        
        public string Message { get { return _message; } }
        public Exception Ex { get { return _exception; } }
        public override EEventType EventType { get { return EEventType.ErrorMessage; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        public KxErrorMessageEventArgs(string message, Exception ex)
            : base()
        {
            _message = message;
            _exception = ex;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public KxErrorMessageEventArgs(KxErrorMessageEventArgs args)
            : base(args)
        {
            // Shallow copy
            _message = args._message;
            _exception = args._exception;
        }

        /// <summary>
        /// Get string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _message;
        }
    }
}
