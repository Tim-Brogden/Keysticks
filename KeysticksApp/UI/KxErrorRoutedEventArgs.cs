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
using System.Windows;
using Keysticks.Core;

namespace Keysticks.UI
{
    /// <summary>
    /// Routed event data containing an exception object
    /// </summary>
    public class KxErrorRoutedEventArgs : RoutedEventArgs
    {
        private KxException _error;

        public KxException Error { get { return _error; } }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="ex"></param>
        public KxErrorRoutedEventArgs(RoutedEvent eventType, KxException ex)
            :base(eventType)
        {
            _error = ex;
        }
    }
}
