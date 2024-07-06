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
    /// Holds a double valued property and provides change notifications
    /// </summary>
    public class DoubleValItem : INotifyPropertyChanged
    {
        private double _val;

        // Events
        public event PropertyChangedEventHandler PropertyChanged;

        // Properties
        public double Value
        {
            get
            {
                return _val;
            }
            set
            {
                if (_val != value)
                {
                    _val = value;
                    NotifyPropertyChanged("Value");
                }
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public DoubleValItem()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="val"></param>
        public DoubleValItem(double val)
        {
            _val = val;
        }

        /// <summary>
        /// Notify change
        /// </summary>
        /// <param name="info"></param>
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
