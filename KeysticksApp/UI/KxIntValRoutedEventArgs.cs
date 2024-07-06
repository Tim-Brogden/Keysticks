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
using System.Windows;

namespace Keysticks.UI
{
    /// <summary>
    /// Integer-valued event data
    /// </summary>
    public class KxIntValRoutedEventArgs : RoutedEventArgs
    {
        // Fields
        public int Value;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="val"></param>
        public KxIntValRoutedEventArgs(RoutedEvent eventType, int val)
            : base(eventType)
        {
            Value = val;
        }
    }
}
