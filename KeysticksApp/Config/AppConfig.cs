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
using System.IO;
using System.Text;
using System.Reflection;
using System.Globalization;
using Keysticks.Core;

namespace Keysticks.Config
{
    /// <summary>
    /// Stores application-wide config settings
    /// </summary>
    public class AppConfig : MetaDataTable
    {
        // Fields
        private string _version = Constants.AppVersionString;
        private ColourScheme _colourScheme; // Cache
        private static bool _isAdminMode;
        private static bool _isLocalMode;
        private static bool _isCatchAll;
        private static string _commonAppDataDir;
        private static string _localAppDataDir;
        private static string _programRootDir;
        private static string _appProjectDir;

        // Properties
        public string Version { get { return _version; } }
        public static bool IsAdminMode { get { return _isAdminMode; } set { _isAdminMode = value; } }
        public static bool IsLocalMode { get { return _isLocalMode; } set { _isLocalMode = value; } }
        public static bool IsCatchAll { get { return _isCatchAll; } set { _isCatchAll = value; } }
        public static string CommonAppDataDir
        {
            get
            {
                if (_commonAppDataDir == null)
                {
                    string appDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    if (appDir.EndsWith("bin\\Release") || appDir.EndsWith("bin\\Debug"))
                    {
                        _commonAppDataDir = Path.Combine(appDir, "..\\..\\..\\Data\\");
                    }
                    else
                    {
                        _commonAppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                                                    Constants.Manufacturer,
                                                                    Constants.ProductName);
                    }

                    // Resolve path
                    DirectoryInfo di = new DirectoryInfo(_commonAppDataDir);
                    _commonAppDataDir = di.FullName;
                }

                return _commonAppDataDir;
            }
        }
        public static string LocalAppDataDir
        {
            get
            {
                if (_localAppDataDir == null)
                {
                    string appDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    if (appDir.EndsWith("bin\\Release") || appDir.EndsWith("bin\\Debug"))
                    {
                        _localAppDataDir = Path.Combine(appDir, "..\\..\\..\\Data\\");
                    }
                    else
                    {
                        _localAppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                                    Constants.Manufacturer,
                                                                    Constants.ProductName);
                    }

                    // Resolve path
                    DirectoryInfo di = new DirectoryInfo(_localAppDataDir);
                    _localAppDataDir = di.FullName;
                }

                return _localAppDataDir;
            }
        }        
        public static string ProgramRootDir
        {
            get
            {
                if (_programRootDir == null)
                {
                    string appDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    if (appDir.EndsWith("bin\\Release") || appDir.EndsWith("bin\\Debug"))
                    {
                        // Allow running from Visual Studio location
                        _programRootDir = Path.Combine(appDir, "..\\..\\..\\");
                    }
                    else
                    {
                        _programRootDir = appDir;
                    }

                    // Resolve path
                    DirectoryInfo di = new DirectoryInfo(_programRootDir);
                    _programRootDir = di.FullName;
                }

                return _programRootDir;
            }
        }
        public static string AppProjectDir
        {
            get
            {
                if (_appProjectDir == null)
                {
                    string appDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    if (appDir.EndsWith("bin\\Release") || appDir.EndsWith("bin\\Debug"))
                    {
                        // Allow running from Visual Studio location
                        _appProjectDir = Path.Combine(appDir, "..\\..\\");
                    }
                    else
                    {
                        _appProjectDir = appDir;
                    }

                    // Resolve path
                    DirectoryInfo di = new DirectoryInfo(_appProjectDir);
                    _appProjectDir = di.FullName;
                }

                return _appProjectDir;
            }
        }
        public ColourScheme ColourScheme
        {
            get
            {
                if (_colourScheme == null)
                {
                    _colourScheme = GetColourScheme();
                }
                return _colourScheme;
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public AppConfig()
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="appConfig"></param>
        public AppConfig(AppConfig appConfig)
        {
            FromXml(appConfig.ToXml());
        }

        /// <summary>
        /// Clear cached data
        /// </summary>
        public void ClearCache()
        {
            _colourScheme = null;
        }

        /// <summary>
        /// Load from file
        /// </summary>
        public bool Load()
        {
            bool success = true;
            try
            {
                // See if config file exists
                string filePath = Path.Combine(LocalAppDataDir, Constants.ConfigFileName);

                // Legacy code: Move config file from old location if it exists
                bool fileExists = File.Exists(filePath);
                if (!fileExists)
                {
                    // Create directory if it doesn't exist
                    if (!Directory.Exists(LocalAppDataDir))
                    {
                        Directory.CreateDirectory(LocalAppDataDir);
                    }
                    
                    string oldFilePath = Path.Combine(CommonAppDataDir, Constants.ConfigFileName);
                    if (File.Exists(oldFilePath))
                    {
                        File.Move(oldFilePath, filePath);
                        fileExists = true;
                    }
                }

                // Read config file if it exists
                if (fileExists)
                {
                    string xml = File.ReadAllText(filePath, Encoding.UTF8);
                    success = FromXml(xml);

                    // Upgrade any keys if it's an earlier version
                    if (_version != Constants.AppVersionString)
                    {
                        UpgradeConfig();
                        _version = Constants.AppVersionString;
                    }
                }
            }
            catch (Exception)
            {
                success = false;
            }

            // Flag as up to date
            IsModified = false;

            return success;
        }

        /// <summary>
        /// Initialise from xml string
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public bool FromXml(string xml)
        {
            bool success = true;
            try
            {
                // Load from file
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                // Read version number
                XmlElement configNode = (XmlElement)doc.SelectSingleNode("/config");
                if (configNode.HasAttribute("version"))
                {
                    _version = configNode.GetAttribute("version");
                }

                // Read settings
                base.FromXml(configNode);
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Save to file
        /// </summary>
        public bool Save()
        {
            bool success = true;
            try
            {
                // Create config dir if required
                string configDir = LocalAppDataDir;
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                // Save XML to file
                string filePath = Path.Combine(configDir, Constants.ConfigFileName);
                string xml = ToXml();
                File.WriteAllText(filePath, xml, Encoding.UTF8);
            }
            catch (Exception)
            {
                success = false;
            }

            // Flag as up to date
            IsModified = false;

            return success;
        }

        /// <summary>
        /// Convert to Xml
        /// </summary>
        /// <returns></returns>
        public string ToXml()
        {
            XmlDocument doc = new XmlDocument();

            // Root element
            XmlElement configElement = doc.CreateElement("config");
            configElement.SetAttribute("version", Constants.AppVersionString);
            doc.AppendChild(configElement);

            // Settings
            base.ToXml(configElement, doc);

            // Save to string
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = Encoding.UTF8;
            XmlWriter xWriter = XmlTextWriter.Create(sb, settings);
            doc.Save(xWriter);

            return sb.ToString();
        }

        /// <summary>
        /// Get the colour scheme settings
        /// </summary>
        /// <returns></returns>
        private ColourScheme GetColourScheme()
        {
            double currentControlsOpacity = 0.01 * GetIntVal(Constants.ConfigCurrentControlsOpacityPercent, Constants.DefaultWindowOpacityPercent);
            double interactiveControlsOpacity = 0.01 * GetIntVal(Constants.ConfigInteractiveControlsOpacityPercent, Constants.DefaultWindowOpacityPercent);
            PlayerColourScheme[] defaultColours = new PlayerColourScheme[] {
                Constants.DefaultPlayer1Colours, Constants.DefaultPlayer2Colours, Constants.DefaultPlayer3Colours, Constants.DefaultPlayer4Colours };

            ColourScheme scheme = new ColourScheme(currentControlsOpacity,
                                                    interactiveControlsOpacity);
            for (int id = Constants.ID1; id <= Constants.ID4; id++)
            {
                PlayerColourScheme defaultPlayerColours = defaultColours[(int)id - 1];
                PlayerColourScheme playerColours = new PlayerColourScheme(
                                                        GetPlayerVal(id, Constants.ConfigCellColour, defaultPlayerColours.CellColour),
                                                        GetPlayerVal(id, Constants.ConfigAlternateCellColour, defaultPlayerColours.AlternateCellColour),
                                                        GetPlayerVal(id, Constants.ConfigHighlightColour, defaultPlayerColours.HighlightColour),
                                                        GetPlayerVal(id, Constants.ConfigSelectionColour, defaultPlayerColours.SelectionColour));
                scheme.SetPlayerColours(id, playerColours);
            }

            return scheme;
        }

        /// <summary>
        /// Get a player-specific option value
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="key"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public string GetPlayerVal(int playerID, string key, string defaultVal)
        {
            return GetStringVal(GetPlayerKey(playerID, key), defaultVal);
        }

        /// <summary>
        /// Set a player-specific option value
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void SetPlayerVal(int playerID, string key, string val)
        {
            SetStringVal(GetPlayerKey(playerID, key), val);
        }

        /// <summary>
        /// Get a player-specific key to look up
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetPlayerKey(int playerID, string key)
        {
            return playerID > Constants.ID1 ? string.Format("{0}_{1}", key, playerID) : key;
        }

        /// <summary>
        /// Upgrade the config to the latest version
        /// </summary>
        private void UpgradeConfig()
        {
            double versionNum = 0.0;
            double.TryParse(_version, NumberStyles.Number, CultureInfo.InvariantCulture, out versionNum);

            if (versionNum < 1.4)
            {
                // Upgrade to V1.4: Change default click and key stroke length
                SetDoubleVal(Constants.ConfigMouseClickLengthMS, Constants.DefaultMouseClickLengthMS);
                SetDoubleVal(Constants.ConfigKeyStrokeLengthMS, Constants.DefaultKeyStrokeLengthMS);
            }

            Dictionary<string, string[]> newKeyNamesDictionary = new Dictionary<string, string[]>();
            if (versionNum < 1.9)
            {
                // Version 1.4 changes
                newKeyNamesDictionary[Constants.OldConfigShowHeldKeysAndMouseButtons] = new string[] { Constants.ConfigShowHeldModifierKeys, Constants.ConfigShowHeldMouseButtons };
                newKeyNamesDictionary[Constants.OldConfigWindowOpacityPercent] = new string[] { Constants.ConfigCurrentControlsOpacityPercent, Constants.ConfigInteractiveControlsOpacityPercent };
                // Version 1.5 changes
                newKeyNamesDictionary[Constants.ConfigUserCurrentProfileDirectory + "_" + Environment.UserName] = new string[] { Constants.ConfigUserCurrentProfileDirectory };
                newKeyNamesDictionary[Constants.ConfigUserLastUsedProfile + "_" + Environment.UserName] = new string[] { Constants.ConfigUserLastUsedProfile };
                newKeyNamesDictionary[Constants.ConfigUserRecentProfilesList + "_" + Environment.UserName] = new string[] { Constants.ConfigUserRecentProfilesList };
                // Version 1.9 changes
                newKeyNamesDictionary[Constants.OldConfigAutoDetectWhichController] = null;
                newKeyNamesDictionary[Constants.OldConfigSpecificControllerNumber] = null;
                Dictionary<string, string>.Enumerator eConfig = GetEnumerator();
                while (eConfig.MoveNext())
                {
                    // Remove old window position settings
                    string key = eConfig.Current.Key;
                    if (key.StartsWith("X_") || key.StartsWith("Y_"))
                    {
                        newKeyNamesDictionary[key] = null;
                    }
                }
            }
            if (versionNum < 2.0)
            {
                // Use machine-specific seed to encrypt download area login details.
                // This provides extra security, for example in the event that someone shares their config file for some reason.
                string userName = GetStringVal(Constants.OldConfigDownloadAreaUsername, null);
                string passwordEnc = GetStringVal(Constants.OldConfigDownloadAreaPasswordEnc, null);
                if (userName != null && passwordEnc != null)
                {
                    // Decrypt using old encryption scheme
                    SymmetricCrypto oldCrypto = new SymmetricCrypto(Constants.PasswordSeed);
                    string password = oldCrypto.DecryptString(passwordEnc);

                    // Re-encrypt using machine-specific seed
                    Keysticks.Sys.IdentityManager idManager = new Sys.IdentityManager();
                    string seed = idManager.GetAnIdentity();
                    SymmetricCrypto newCrypto = new SymmetricCrypto(seed);
                    string userNameSec = newCrypto.EncryptString(userName);
                    string passwordSec = newCrypto.EncryptString(password);

                    // Store newly encrypted credentials
                    // Only store the hash code for the seed, as a clue to what it was
                    SetStringVal(Constants.ConfigDownloadAreaUsernameSec, userNameSec);
                    SetStringVal(Constants.ConfigDownloadAreaPasswordSec, passwordSec);
                    SetIntVal(Constants.ConfigDownloadAreaSeedHint, seed.GetHashCode());
                }

                // Remove old fields
                newKeyNamesDictionary[Constants.OldConfigDownloadAreaUsername] = null;
                newKeyNamesDictionary[Constants.OldConfigDownloadAreaPasswordEnc] = null;
                newKeyNamesDictionary[Constants.OldConfigWordPredictionServerPort] = null;

                // Rename scan codes option
                newKeyNamesDictionary[Constants.OldConfigUseDirectXKeyPresses] = new string[] { Constants.ConfigUseScanCodes };
            }

            // Rename / remove keys
            Dictionary<string, string[]>.Enumerator eDict = newKeyNamesDictionary.GetEnumerator();
            while (eDict.MoveNext())
            {
                string oldKey = eDict.Current.Key;
                string oldValue = GetStringVal(oldKey, null);
                if (oldValue != null)
                {
                    string[] newKeys = eDict.Current.Value;

                    // Copy to new keys
                    if (newKeys != null)
                    {
                        foreach (string newKey in newKeys)
                        {
                            SetStringVal(newKey, string.Copy(oldValue));
                        }
                    }

                    // Remove old key
                    this.Remove(oldKey);
                }
            }
        }

    }
}
