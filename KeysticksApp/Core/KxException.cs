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

namespace Keysticks.Core
{
    /// <summary>
    /// Custom exception class
    /// </summary>
    public class KxException : Exception
    {
        public KxException(string message)
            :base(message)
        {
        }

        public KxException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
