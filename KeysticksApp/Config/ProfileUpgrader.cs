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
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Windows.Forms;
using Keysticks.Core;
using Keysticks.Sources;

namespace Keysticks.Config
{
    /// <summary>
    /// Upgrades profiles to the current version
    /// </summary>
    public class ProfileUpgrader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ProfileUpgrader()
        {
        }

        /// <summary>
        /// Upgrade a profile to the current version
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>whether upgraded or not</returns>        
        public void UpgradeXml(ref XmlDocument doc)
        {
            try
            {
                double version = GetProfileVersionNumber(doc);
                if (version < 1.9)
                {
                    // Read embedded resource
                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Keysticks.AutoUpgrade.Upgrade to v1.9.xsl"))
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        XslCompiledTransform transform = new XslCompiledTransform();
                        transform.Load(reader);

                        // Apply transform
                        StringBuilder sb = new StringBuilder();
                        XmlWriter writer = XmlWriter.Create(sb, transform.OutputSettings);
                        transform.Transform(doc, writer);
                        
                        // Replace doc with transformed doc
                        doc = new XmlDocument();
                        doc.LoadXml(sb.ToString());                      

                        // Programmatic upgrade
                        UpgradeToVersion_1_9(ref doc);
                    }
                }
                if (version < 2.0)
                {
                    UpgradeToVersion_2_0(ref doc);
                }

                // Write to formatted string for debugging
                //string debug = DocToString(doc);
            }
            catch (Exception ex)
            {
                throw new KxException(Properties.Resources.E_UpgradeProfile, ex);
            }
        }

        /// <summary>
        /// Perform programmatic upgrades
        /// </summary>
        /// <param name="doc"></param>
        private void UpgradeToVersion_1_9(ref XmlDocument doc)
        {
            // Create default source if none present
            XmlElement profileElement = (XmlElement)doc.SelectSingleNode("/profile");
            if (profileElement != null)
            {
                XmlElement sourcesElement = (XmlElement)profileElement.SelectSingleNode("sources");
                if (sourcesElement != null)
                {
                    XmlNodeList sourceNodes = sourcesElement.SelectNodes("source");
                    if (sourceNodes.Count == 0)
                    {
                        // Add source
                        ProfileBuilder builder = new ProfileBuilder();
                        BaseSource source = builder.CreateDefaultVirtualController(Constants.ID1, null);
                        XmlElement sourceElement = doc.CreateElement("source");
                        source.ToXml(sourceElement, doc);
                        sourcesElement.AppendChild(sourceElement);

                        // Replace statetree under source element (omitting root state)
                        XmlElement newStateTreeElement = (XmlElement)sourceElement.SelectSingleNode("statetree");
                        if (newStateTreeElement != null)
                        {
                            XmlNodeList oldValueNodes = profileElement.SelectNodes("statetree/value[@id!='-1']");
                            foreach (XmlNode node in oldValueNodes)
                            {
                                newStateTreeElement.AppendChild(node);
                            }
                        }

                        // Add activations element
                        XmlElement activationsElement = doc.CreateElement("activations");
                        sourceElement.AppendChild(activationsElement);

                        // Replace actions under source element
                        XmlElement newActionSetsElement = (XmlElement)sourceElement.SelectSingleNode("actionsets");
                        if (newActionSetsElement != null)
                        {
                            XmlNodeList oldActionSetNodes = profileElement.SelectNodes("actionsets/actionset");
                            foreach (XmlNode node in oldActionSetNodes)
                            {
                                newActionSetsElement.AppendChild(node);
                            }
                        }
                    }
                }
            }

            // Move notes into metadata if required
            XmlElement notesElement = (XmlElement)profileElement.SelectSingleNode("notes");
            if (notesElement != null)
            {
                // Create metadata element if required
                XmlElement metaDataElement = (XmlElement)profileElement.SelectSingleNode("metadata");
                if (metaDataElement == null)
                {
                    metaDataElement = doc.CreateElement("metadata");
                    profileElement.AppendChild(metaDataElement);
                }

                // Add setting element for notes
                XmlElement settingElement = doc.CreateElement("setting");
                settingElement.SetAttribute("name", "Notes");
                settingElement.SetAttribute("value", notesElement.InnerText);
                metaDataElement.AppendChild(settingElement);
            }
        }

        private void UpgradeToVersion_2_0(ref XmlDocument doc)
        {
            List<Keys> extendedKeys = KeyUtils.GetExtendedKeys();

            // Adjustment to scan codes for certain extended keys
            XmlNodeList baseKeyActionsList = doc.SelectNodes("/profile/sources/source/actionsets/actionset/actionlist/*[@scancode]");
            foreach (XmlElement actionElement in baseKeyActionsList)
            {
                ushort scanCode;
                string scanCodeStr = actionElement.GetAttribute("scancode");
                if (ushort.TryParse(scanCodeStr, out scanCode) &&
                    actionElement.HasAttribute("keycode"))
                {
                    Keys keyCode;
                    string keyCodeStr = actionElement.GetAttribute("keycode");
                    if (Enum.TryParse<Keys>(keyCodeStr, out keyCode))
                    {
                        switch (keyCode)
                        {
                            case Keys.Pause:
                            case Keys.PrintScreen:
                            case Keys.NumLock:
                                scanCode &= 0xFEFF;     // Clears the 0x0100 bit
                                actionElement.SetAttribute("scancode", scanCode.ToString(CultureInfo.InvariantCulture));
                                break;
                            default:
                                if (extendedKeys.Contains(keyCode))
                                {
                                    scanCode |= 0x0100;     // Sets the 0x0100 bit
                                    actionElement.SetAttribute("scancode", scanCode.ToString(CultureInfo.InvariantCulture));
                                }
                                break;
                        }
                    }
                }                
            }
        }

        /// <summary>
        /// Convert an XML document to a string
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private string DocToString(XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = Encoding.UTF8;
            XmlWriter xWriter = XmlTextWriter.Create(sb, settings);
            doc.Save(xWriter);
            return sb.ToString();
        }

        /// <summary>
        /// Get the version number of an Alt Controller profile
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private double GetProfileVersionNumber(XmlDocument doc)
        {
            double version = 0.0;

            // Read version
            XmlNode versionNode = doc.SelectSingleNode("/profile/@version");
            if (versionNode != null)
            {
                version = double.Parse(versionNode.Value, System.Globalization.CultureInfo.InvariantCulture);
            }
            
            return version;
        }
    }
}
