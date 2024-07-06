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
using System.Xml;

namespace Keysticks.Core
{
    /// <summary>
    /// Stores an item with an ID and name
    /// </summary>
    public class NamedItem : INotifyPropertyChanged
    {
        // Fields
        private int _id = Constants.DefaultID;
        private string _name = "";

        // Events
        public event PropertyChangedEventHandler PropertyChanged;

        // Properties
        public int ID 
        { 
            get 
            { 
                return _id; 
            } 
            set 
            {
                if (_id != value)
                {
                    _id = value; 
                    NotifyPropertyChanged("ID");
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
        public NamedItem()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public NamedItem(int id, string name)
        {
            _id = id;
            _name = name;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="item"></param>
        public NamedItem(NamedItem item)
        {
            _id = item.ID;
            _name = item.Name;
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public virtual void FromXml(XmlElement element)
        {
            _id = int.Parse(element.Attributes["id"].Value, System.Globalization.CultureInfo.InvariantCulture);
            _name = element.Attributes["name"].Value;
        }

        /// <summary>
        /// Write out to xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public virtual void ToXml(XmlElement element, XmlDocument doc)
        {
            element.SetAttribute("id", _id.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("name", _name);
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
            return _name;
        }
    }

}
