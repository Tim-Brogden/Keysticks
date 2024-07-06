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
using System.ComponentModel;

namespace Keysticks.Core
{
    /// <summary>
    /// Stores a named item which can be enabled or disabled
    /// </summary>
    public class OptionalNamedItem : NamedItem
    {
        // Fields
        private bool _isEnabled;

        // Properties
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    NotifyPropertyChanged("IsEnabled");
                }
            }
        }        
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public OptionalNamedItem()
            :base()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OptionalNamedItem(int id, string name)
            : base(id, name)
        {            
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public OptionalNamedItem(int id, string name, bool isEnabled)
            :base(id, name)
        {
            _isEnabled = isEnabled;
        }
    }
}
