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
using System.Windows;
using Keysticks.Config;

namespace Keysticks.UI
{
    /// <summary>
    /// Logical state event data
    /// </summary>
    public class KxStateRoutedEventArgs : RoutedEventArgs
    {
        private StateVector _state;

        public StateVector State { get { return _state; } }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="ex"></param>
        public KxStateRoutedEventArgs(RoutedEvent eventType, StateVector state)
            :base(eventType)
        {
            _state = state;
        }
    }
}
