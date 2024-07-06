/******************************************************************************
 *
 * Copyright 2019 Tim Brogden
 * All rights reserved. This program and the accompanying materials   
 * are made available under the terms of the Eclipse Public License v1.0  
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *           
 * Contributors: 
 * Tim Brogden - WordPredictor COM component with demo application
 *
 *****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace WordPredictionDemo
{
    public class DictionaryItem : INotifyPropertyChanged
    {
        // Fields
        private bool _isEnabled;
        private string _name = "";

        // Events
        public event PropertyChangedEventHandler PropertyChanged;

        // Properties
        public bool IsEnabled 
        { 
            get 
            {
                return _isEnabled; 
            } 
            set 
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value; 
                    NotifyPropertyChanged("IsEnabled");
                }
            } 
        }
        public string Name 
        { 
            get 
            { 
                return _name; 
            } 
            set 
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            } 
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public DictionaryItem()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DictionaryItem(string name, bool isEnabled)
        {
            _name = name;
            _isEnabled = isEnabled;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="item"></param>
        public DictionaryItem(DictionaryItem item)
        {
            _isEnabled = item._isEnabled;
            _name = item._name;
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

        /// <summary>
        /// String representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
