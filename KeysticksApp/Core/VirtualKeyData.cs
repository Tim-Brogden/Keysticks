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
    /// <summary>
    /// Stores data for a keyboard key
    /// </summary>
    public class VirtualKeyData
    {
        // Fields
        private System.Windows.Forms.Keys _keyCode = System.Windows.Forms.Keys.None;
        private ushort _windowsScanCode;
        private Dictionary<EModifierKeyStates, string> _oemKeyCombinations;
        private string _keyName = "";
        private string _tinyName = null;

        // Properties
        public System.Windows.Forms.Keys KeyCode { get { return _keyCode; } set { _keyCode = value; } }
        public ushort WindowsScanCode { get { return _windowsScanCode; } set { _windowsScanCode = value; } }
        public Dictionary<EModifierKeyStates, string> OemKeyCombinations { get { return _oemKeyCombinations; } }
        public string Name { get { return _keyName; } set { _keyName = value; } }
        public string TinyName
        { 
            get
            {
                if (_tinyName != null)
                {
                    return _tinyName;
                }
                else
                {
                    // Default to key name if tiny name isn't set
                    return _keyName;
                }
            } 
            set { _tinyName = value; } 
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public VirtualKeyData()
        {
            _oemKeyCombinations = new Dictionary<EModifierKeyStates, string>();
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="windowsScanCode"></param>
        /// <param name="keyName"></param>
        public VirtualKeyData(System.Windows.Forms.Keys keyCode,
                                ushort windowsScanCode,
                                Dictionary<EModifierKeyStates, string> oemKeyCombinations,
                                string keyName,
                                string tinyName)
        {
            _keyCode = keyCode;
            _windowsScanCode = windowsScanCode;
            _oemKeyCombinations = oemKeyCombinations;
            _keyName = keyName;
            _tinyName = tinyName;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="keyData"></param>
        public VirtualKeyData(VirtualKeyData vk)
        {
            _keyCode = vk._keyCode;
            _windowsScanCode = vk._windowsScanCode;
            if (vk._oemKeyCombinations != null)
            {
                _oemKeyCombinations = new Dictionary<EModifierKeyStates, string>();
                Dictionary<EModifierKeyStates, string>.Enumerator eCombination = vk._oemKeyCombinations.GetEnumerator();
                while (eCombination.MoveNext())
                {
                    _oemKeyCombinations[eCombination.Current.Key] = string.Copy(eCombination.Current.Value);
                }
            }
            if (vk._keyName != null)
            {
                _keyName = string.Copy(vk._keyName);
            }
            if (vk._tinyName != null)
            {
                _tinyName = string.Copy(vk._tinyName);
            }
        }
    }
}
