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
using System.Collections.Generic;

namespace Keysticks.Core
{
    public class KeyboardLayoutData
    {
        // Fields
        private VirtualKeyData[] _virtualKeysByKeyCode;
        private Dictionary<ushort, VirtualKeyData> _virtualKeysByScanCode;

        // Properties
        public VirtualKeyData[] VirtualKeysByKeyCode { get { return _virtualKeysByKeyCode; } }
        public Dictionary<ushort, VirtualKeyData> VirtualKeysByScanCode { get { return _virtualKeysByScanCode; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="virtualKeysByKeyCode"></param>
        /// <param name="virtualKeysByScanCode"></param>
        public KeyboardLayoutData(VirtualKeyData[] virtualKeysByKeyCode, Dictionary<ushort, VirtualKeyData> virtualKeysByScanCode)
        {
            _virtualKeysByKeyCode = virtualKeysByKeyCode;
            _virtualKeysByScanCode = virtualKeysByScanCode;
        }
    }
}
