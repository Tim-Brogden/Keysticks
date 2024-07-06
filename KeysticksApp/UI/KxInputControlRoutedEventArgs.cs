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
using Keysticks.Event;

namespace Keysticks.UI
{
    /// <summary>
    /// Event data specifying an input control
    /// </summary>
    public class KxInputControlRoutedEventArgs : RoutedEventArgs
    {
        private KxControlEventArgs _control;

        public KxControlEventArgs Control { get { return _control; } }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="control"></param>
        public KxInputControlRoutedEventArgs(RoutedEvent eventType, KxControlEventArgs control)
        :base(eventType)
        {
            _control = control;
        }
    }
}
