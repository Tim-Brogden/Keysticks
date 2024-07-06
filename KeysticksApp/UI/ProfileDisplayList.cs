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
using System.Text;
using System.Xml;
using System.IO;
using System.Globalization;
using Keysticks.Core;

namespace Keysticks.UI
{
    /// <summary>
    /// Stores a list of profile metadata
    /// </summary>
    public class ProfileDisplayList : NamedItemList
    {
        // Fields
        private bool _changed = false;

        // Properties
        public bool Changed { get { return _changed; } set { _changed = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public ProfileDisplayList()
            :base()
        {
        }

        /// <summary>
        /// Read from file
        /// </summary>
        /// <param name="filePath"></param>
        public void FromFile(string filePath)
        {
            // Clear the list
            Clear();

            if (File.Exists(filePath))
            {
                string data = File.ReadAllText(filePath, Encoding.UTF8);

                // Decompress
                CompressionUtils compUtils = new CompressionUtils();
                data = compUtils.Decompress(data);

                // Load Xml
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(data);

                // Parse
                XmlElement rootElement = (XmlElement)doc.SelectSingleNode("profiles");
                FromXml(rootElement);
            }

            _changed = false;
        }
        
        /// <summary>
        /// Write to file
        /// </summary>
        /// <param name="filePath"></param>
        public void ToFile(string filePath)
        {
            XmlDocument doc = new XmlDocument();

            // Write to Xml
            XmlElement rootElement = doc.CreateElement("profiles");
            ToXml(rootElement, doc);            
            doc.AppendChild(rootElement);

            // Write to string
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = Encoding.UTF8;
            XmlWriter xWriter = XmlTextWriter.Create(sb, settings);
            doc.Save(xWriter);

            // Compress
            CompressionUtils compUtils = new CompressionUtils();
            string compressed = compUtils.Compress(sb.ToString());
            File.WriteAllText(filePath, compressed, Encoding.UTF8);

            _changed = false;
        }

        /// <summary>
        /// Read from Xml
        /// </summary>
        /// <param name="parentElement"></param>
        public void FromXml(XmlElement parentElement)
        {
            // Loop through profile meta data
            XmlNodeList rows = parentElement.SelectNodes("profile");
            foreach (XmlElement rowElement in rows)
            {
                ProfileDisplayItem displayItem = new ProfileDisplayItem();
                displayItem.FromXml(rowElement);
                Add(displayItem);
            }            
        }

        /// <summary>
        /// Write to Xml
        /// </summary>
        /// <param name="parentElement"></param>
        /// <param name="doc"></param>
        public void ToXml(XmlElement parentElement, XmlDocument doc)
        {
            // Profiles
            foreach (ProfileDisplayItem displayItem in this)
            {
                XmlElement rowElement = doc.CreateElement("profile");
                displayItem.ToXml(rowElement, doc);
                parentElement.AppendChild(rowElement);
            }
        }
    }
}
