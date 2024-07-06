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
using System.IO;
using System.Text;
using System.Globalization;
using Keysticks.Core;
using Keysticks.Sources;

namespace Keysticks.Config
{
    /// <summary>
    /// Configuration profile that maps input sources and controls to actions,
    /// according to the current logical state
    /// </summary>
    public class Profile : INotifyPropertyChanged
    {
        // Config
        private bool _isTemplate = false;
        private string _profileName = "";
        private MetaDataTable _metaData = new MetaDataTable();
        private NamedItemList _virtualSources = new NamedItemList();

        // State
        private bool _isModified = false;
        private IKeyboardContext _keyboardContext;

        // Events
        public event PropertyChangedEventHandler PropertyChanged;

        // Properties
        public bool IsTemplate { get { return _isTemplate; } }
        public string Name { get { return _profileName; } set { _profileName = value; } }
        public MetaDataTable MetaData { get { return _metaData; } }
        public NamedItemList VirtualSources { get { return _virtualSources; } }
        public IKeyboardContext KeyboardContext { get { return _keyboardContext; } }
        public bool IsModified
        {
            get { return _isModified; }
            set
            {
                if (_isModified != value)
                {
                    _isModified = value;
                    NotifyPropertyChanged("IsModified");
                }
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Profile()
        {
            _profileName = Properties.Resources.String_DefaultProfileName;
        }

        /// <summary>
        /// Constructor for template profile
        /// </summary>
        /// <param name="isTemplates"></param>
        public Profile(bool isTemplate)
        {
            _profileName = Properties.Resources.String_DefaultProfileName;
            _isTemplate = isTemplate;
        }

        /// <summary>
        /// Constructor from meta data
        /// </summary>
        /// <param name="metaData"></param>
        public Profile(MetaDataTable metaData)
        {
            _profileName = Properties.Resources.String_DefaultProfileName;
            _metaData = metaData;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="profile"></param>
        public Profile(Profile profile)
        {
            FromXml(profile.ToXml());
        }

        /// <summary>
        /// Handle change of app config
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            foreach (BaseSource source in _virtualSources)
            {
                source.SetAppConfig(appConfig);
            }
        }

        /// <summary>
        /// Handle keyboard layout change
        /// </summary>
        public void Initialise(IKeyboardContext context)
        {
            _keyboardContext = context;
            foreach (BaseSource source in _virtualSources)
            {
                source.Initialise(context);
            }
        }        

        /// <summary>
        /// Initialise from a file
        /// </summary>
        /// <param name="filePath"></param>
        public void FromFile(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath, Encoding.UTF8))
            {
                string str = sr.ReadToEnd();
                FromString(str);
            }
        }

        /// <summary>
        /// Read from a string format which consists of a header line
        /// then XML data which can be compressed and/or encrypted
        /// </summary>
        /// <param name="str"></param>
        public void FromString(string str)
        {
            // Read the version number, and whether or not it's encrypted
            using (StringReader sr = new StringReader(str))
            {
                double fileVersion;
                double thisVersion = double.Parse(Constants.FileVersionString, CultureInfo.InvariantCulture);

                string line = sr.ReadLine();
                string[] tokens = line.Split(',');  // Token 0 = version number, token 1 = whether encrypted, token 2 = whether compressed
                bool isEncrypted;
                if (tokens.Length > 1 &&
                    double.TryParse(tokens[0], NumberStyles.Number, CultureInfo.InvariantCulture, out fileVersion) &&
                    bool.TryParse(tokens[1], out isEncrypted))
                {
                    if (fileVersion < thisVersion + 1E-6)
                    {
                        string data = sr.ReadToEnd();

                        // Decrypt
                        if (isEncrypted)
                        {
                            SymmetricCrypto crypto = new SymmetricCrypto(Constants.ProfileSeed);
                            data = crypto.DecryptString(data);
                        }

                        // Decompress
                        bool isCompressed;
                        if (tokens.Length > 2 && bool.TryParse(tokens[2], out isCompressed) && isCompressed)
                        {
                            CompressionUtils compUtils = new CompressionUtils();
                            data = compUtils.Decompress(data);
                        }
                       
                        // Create xml doc from string
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(data);
                        
                        // Upgrade profile
                        ProfileUpgrader upgrader = new ProfileUpgrader();
                        upgrader.UpgradeXml(ref doc);

                        // Parse xml
                        FromXml(doc);
                    }
                    else
                    {
                        // Could check profile against a schema here
                        string message = string.Format(Properties.Resources.E_NewerProfile, Constants.ProductName);
                        throw new KxException(message);
                    }
                }
                else
                {
                    throw new KxException(Properties.Resources.E_ReadProfile);
                }
            }         
        }

        /// <summary>
        /// Initialise from an xml string
        /// </summary>
        /// <param name="xml"></param>
        public void FromXml(XmlDocument doc)
        {
            try
            {
                // Get the profile node
                XmlElement profileElement = (XmlElement)doc.SelectSingleNode("/profile");

                // Read whether contains templates
                if (profileElement.HasAttribute("istemplate"))
                {
                    string isTemplate = profileElement.GetAttribute("istemplate");
                    _isTemplate = bool.Parse(isTemplate);
                }

                // Profile name
                if (profileElement.HasAttribute("name"))
                {
                    _profileName = profileElement.GetAttribute("name");
                }
               
                // Read sources
                XmlNodeList sourcesNodes = profileElement.SelectNodes("sources/source");
                foreach (XmlElement sourceNode in sourcesNodes)
                {
                    BaseSource source = new BaseSource();
                    source.FromXml(sourceNode);
                    source.Profile = this;
                    _virtualSources.Add(source);
                }
       
                // Meta data
                XmlElement metaDataElement = (XmlElement)profileElement.SelectSingleNode("metadata");
                if (metaDataElement != null)
                {
                    _metaData.FromXml(metaDataElement);
                }                                 
            }
            catch (Exception ex)
            {
                throw new KxException(Properties.Resources.E_LoadFromXml, ex);
            }
        }

        /// <summary>
        /// Write profile to a file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="profile"></param>
        public void ToFile(string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                sw.Write(ToString());
            }            
        }

        /// <summary>
        /// Convert to a string format which consists of a header line
        /// then XML data which can be compressed and/or encrypted
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            bool encrypt = false;
            bool compress = true;
            #if DEBUG
                encrypt = false;
                compress = false;
            #endif

            // Write to xml
            XmlDocument doc = ToXml();

            // Write to string
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = Encoding.UTF8;
            XmlWriter xWriter = XmlTextWriter.Create(sb, settings);
            doc.Save(xWriter);
            string data = sb.ToString();

            if (compress)
            {
                // Compress
                CompressionUtils compUtils = new CompressionUtils();
                data = compUtils.Compress(data);
            }
            if (encrypt)
            {
                // Encrypt
                SymmetricCrypto crypto = new SymmetricCrypto(Constants.ProfileSeed);
                data = crypto.EncryptString(data);
            }

            // Write the version number, and whether or not it's encrypted or compressed
            sb.Clear();
            sb.AppendLine(string.Format("{0},{1},{2}", Constants.FileVersionString, encrypt, compress));
            sb.Append(data);

            return sb.ToString();
        }

        /// <summary>
        /// Convert to a string representation
        /// </summary>
        public XmlDocument ToXml()
        {
            try
            {
                XmlDocument doc = new XmlDocument();

                // Root node
                XmlElement profileElement = doc.CreateElement("profile");
                doc.AppendChild(profileElement);

                // Version
                profileElement.SetAttribute("version", Constants.FileVersionString);

                // Whether contains templates
                profileElement.SetAttribute("istemplate", _isTemplate.ToString());

                // Name
                profileElement.SetAttribute("name", _profileName);

                // Sources
                XmlElement sourcesElement = doc.CreateElement("sources");
                foreach (BaseSource source in _virtualSources)
                {
                    XmlElement element = doc.CreateElement("source");
                    source.ToXml(element, doc);
                    sourcesElement.AppendChild(element);
                }
                profileElement.AppendChild(sourcesElement);

                // Meta data
                XmlElement metaDataElement = doc.CreateElement("metadata");
                _metaData.ToXml(metaDataElement, doc);
                profileElement.AppendChild(metaDataElement);

                return doc;
            }
            catch (Exception ex)
            {
                throw new KxException(Properties.Resources.E_WriteToXml, ex);
            }
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
        /// Get the specified source
        /// </summary>
        /// <param name="args">Not null</param>
        /// <returns></returns>
        public BaseSource GetSource(int sourceID)
        {
            return (BaseSource)_virtualSources.GetItemByID(sourceID);
        }

        /// <summary>
        /// Add a new input source
        /// </summary>
        /// <param name="source"></param>
        public void AddSource(BaseSource source)
        {
            source.Profile = this;
            _virtualSources.Add(source);
            IsModified = true;
        }

        /// <summary>
        /// Remove an input source
        /// </summary>
        /// <param name="source"></param>
        public void RemoveSource(BaseSource source)
        {
            _virtualSources.Remove(source);
            IsModified = true;
        }

        /// <summary>
        /// Return whether or not the profile has actions of a particular type
        /// </summary>
        /// <returns></returns>
        public bool HasActionsOfType(EActionType actionType)
        {
            bool has = false;
            foreach (BaseSource source in _virtualSources)
            {
                if (source.Actions.HasActionsOfType(actionType))
                {
                    has = true;
                    break;
                }
            }
            
            return has;
        }
        
        /// <summary>
        /// Check whether the profile has states to be activated automatically when the active process changes
        /// </summary>
        /// <returns></returns>
        public bool HasAutoActivations()
        {
            bool has = false;
            foreach (BaseSource source in _virtualSources)
            {
                if (source.AutoActivations.Count != 0)
                {
                    has = true;
                    break;
                }
            }

            return has;
        }

        /// <summary>
        /// Validate profile
        /// </summary>
        public void Validate()
        {
            foreach (BaseSource source in _virtualSources)
            {
                source.Validate();
            }
        }

        /// <summary>
        /// Remove potentially harmful actions from the profile
        /// </summary>
        /// <returns></returns>
        public void ValidateSecurity()
        {
            foreach (BaseSource source in _virtualSources)
            {
                source.Actions.Validate_Security();
            }
        }

        /// <summary>
        /// Replace language-specific control set names
        /// </summary>
        public void SetKeyboardSpecificControlSetNames(IKeyboardContext context)
        {
            foreach (BaseSource source in _virtualSources)
            {
                source.StateTree.SetKeyboardSpecificKeyNames(context);
                source.StateTree.SetKeyboardSpecificVirtualKeyNames(context);
            }
        }
    }
}
