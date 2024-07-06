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
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Keysticks.Config
{
    public class MetaDataTable
    {
        // Fields
        private bool _isModified = false;
        private Dictionary<string, string> _valuesTable = new Dictionary<string, string>();

        // Properties
        public bool IsModified { get { return _isModified; } protected set { _isModified = value; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MetaDataTable()
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="table"></param>
        public MetaDataTable(MetaDataTable table)
        {
            Dictionary<string, string>.Enumerator eValues = table._valuesTable.GetEnumerator();
            while (eValues.MoveNext())
            {
                _valuesTable[eValues.Current.Key] = string.Copy(eValues.Current.Value);
            }
        }

        /// <summary>
        /// Read from Xml
        /// </summary>
        /// <param name="metaDataElement"></param>
        public void FromXml(XmlElement parentElement)
        {
            XmlNodeList settingElements = parentElement.SelectNodes("setting");
            foreach (XmlElement settingElement in settingElements)
            {
                string name = settingElement.GetAttribute("name");
                string val = settingElement.GetAttribute("value");
                _valuesTable[name] = val;
            }
        }

        /// <summary>
        /// Write to Xml
        /// </summary>
        /// <param name="metaDataElement"></param>
        /// <param name="doc"></param>
        public void ToXml(XmlElement parentElement, XmlDocument doc)
        {
            Dictionary<string, string>.Enumerator eDict = _valuesTable.GetEnumerator();
            while (eDict.MoveNext())
            {
                XmlElement entryElement = doc.CreateElement("setting");
                entryElement.SetAttribute("name", eDict.Current.Key);
                entryElement.SetAttribute("value", eDict.Current.Value);
                parentElement.AppendChild(entryElement);
            }
        }

        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string>.Enumerator GetEnumerator()
        {
            return _valuesTable.GetEnumerator();
        }

        /// <summary>
        /// Set a param value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void SetBoolVal(string key, bool val)
        {
            _valuesTable[key] = val.ToString();

            _isModified = true;
        }

        /// <summary>
        /// Get a bool param value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public bool GetBoolVal(string key, bool defaultVal)
        {
            bool boolVal;
            string val = GetStringVal(key, null);
            if (val == null || !bool.TryParse(val, out boolVal))
            {
                boolVal = defaultVal;
            }

            return boolVal;
        }

        /// <summary>
        /// Set a param value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void SetIntVal(string key, int val)
        {
            _valuesTable[key] = val.ToString(CultureInfo.InvariantCulture);

            _isModified = true;
        }

        /// <summary>
        /// Get an int param value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public int GetIntVal(string key, int defaultVal)
        {
            int intVal;
            string val = GetStringVal(key, null);
            if (val == null || !int.TryParse(val, NumberStyles.Number, CultureInfo.InvariantCulture, out intVal))
            {
                intVal = defaultVal;
            }

            return intVal;
        }

        /// <summary>
        /// Set a param value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void SetDoubleVal(string key, double val)
        {
            _valuesTable[key] = val.ToString(CultureInfo.InvariantCulture);

            _isModified = true;
        }

        /// <summary>
        /// Get a double param value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public double GetDoubleVal(string key, double defaultVal)
        {
            double dblVal;
            string val = GetStringVal(key, null);
            if (val == null || !double.TryParse(val, NumberStyles.Number, CultureInfo.InvariantCulture, out dblVal))
            {
                dblVal = defaultVal;
            }

            return dblVal;
        }

        /// <summary>
        /// Set a float param value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void SetFloatVal(string key, float val)
        {
            _valuesTable[key] = val.ToString(CultureInfo.InvariantCulture);

            _isModified = true;
        }

        /// <summary>
        /// Get a float param value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public float GetFloatVal(string key, float defaultVal)
        {
            float fVal;
            string val = GetStringVal(key, null);
            if (val == null || !float.TryParse(val, NumberStyles.Number, CultureInfo.InvariantCulture, out fVal))
            {
                fVal = defaultVal;
            }

            return fVal;
        }

        /// <summary>
        /// Set a DateTime value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void SetDateVal(string key, DateTime val)
        {
            _valuesTable[key] = val.ToString(CultureInfo.InvariantCulture);

            _isModified = true;
        }

        /// <summary>
        /// Get a DateTime value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public DateTime GetDateVal(string key, DateTime defaultVal)
        {
            DateTime dtVal;
            string val = GetStringVal(key, null);
            if (val == null || !DateTime.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out dtVal))
            {
                dtVal = defaultVal;
            }

            return dtVal;
        }

        /// <summary>
        /// Set a param value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void SetStringVal(string key, string val)
        {
            if (val != null)
            {
                _valuesTable[key] = val.ToString();
            }
            else if (_valuesTable.ContainsKey(key))
            {
                _valuesTable.Remove(key);
            }

            _isModified = true;
        }

        /// <summary>
        /// Get a string param value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetStringVal(string key, string defaultVal)
        {
            string val;
            if (_valuesTable.ContainsKey(key))
            {
                val = _valuesTable[key];
            }
            else
            {
                val = defaultVal;
            }

            return val;
        }

        /// <summary>
        /// Remove the specified key
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            if (_valuesTable.ContainsKey(key))
            {
                _valuesTable.Remove(key);

                _isModified = true;
            }
        }
    }
}
