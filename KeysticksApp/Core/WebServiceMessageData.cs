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
using System.Collections.Generic;
using System.Xml;

namespace Keysticks.Core
{
    public class WebServiceMessageData
    {
        // Fields
        private EMessageType _messageType = EMessageType.None;
        private string _content;
        private Dictionary<string, string> _metadata = new Dictionary<string, string>();

        // Properties
        public EMessageType MessageType { get { return _messageType; } set { _messageType = value; } }
        public string Content { get { return _content; } set { _content = value; } }
        public Dictionary<string, string> Metadata { get { return _metadata; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        public WebServiceMessageData()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public WebServiceMessageData(EMessageType messageType)
        {
            _messageType = messageType;
        }

        /// <summary>
        /// Set meta data value
        /// </summary>
        public void SetMetaVal(string key, string val)
        {
            _metadata[key] = val;
        }

        /// <summary>
        /// Get meta data value
        /// </summary>
        public string GetMetaVal(string key, string def = "")
        {
            return _metadata.ContainsKey(key) ? _metadata[key] : def;
        }

        /// <summary>
        /// Convert from string
        /// </summary>
        /// <param name="xml"></param>
        public bool FromString(string xml)
        {
            bool success = false;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            XmlElement rootElement = (XmlElement)doc.SelectSingleNode("/message");
            if (rootElement != null)
            {
                // Read message attributes
                Enum.TryParse<EMessageType>(rootElement.GetAttribute("type"), out _messageType);

                // Read meta data items
                foreach (XmlElement itemElement in rootElement.SelectNodes("metadata/item"))
                {
                    string key = itemElement.GetAttribute("key");
                    string val = itemElement.GetAttribute("value");
                    _metadata[key] = val;
                }

                // Read content
                XmlElement contentElement = (XmlElement)rootElement.SelectSingleNode("content");
                if (contentElement != null)
                {
                    _content = contentElement.InnerText;
                }

                success = true;
            }

            return success;
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // Create Xml doc
            XmlDocument doc = new XmlDocument();
            XmlElement rootElement = doc.CreateElement("message");

            // Set message attributes
            rootElement.SetAttribute("type", _messageType.ToString());
            doc.AppendChild(rootElement);

            // Add meta data items
            XmlElement metadataElement = doc.CreateElement("metadata");
            XmlElement itemElement;
            Dictionary<string, string>.Enumerator eDict = _metadata.GetEnumerator();
            while (eDict.MoveNext())
            {
                itemElement = doc.CreateElement("item");
                itemElement.SetAttribute("key", eDict.Current.Key);
                itemElement.SetAttribute("value", eDict.Current.Value);
                metadataElement.AppendChild(itemElement);
            }
            rootElement.AppendChild(metadataElement);

            // Add content
            if (_content != null)
            {
                XmlElement contentElement = doc.CreateElement("content");
                contentElement.InnerText = _content;
                rootElement.AppendChild(contentElement);
            }

            // Write to string
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = Encoding.UTF8;
            XmlWriter xWriter = XmlTextWriter.Create(sb, settings);
            doc.Save(xWriter);

            return sb.ToString();
        }

    }
}

