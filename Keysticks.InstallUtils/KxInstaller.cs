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
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Diagnostics;
using System.IO;

namespace KeysticksInstallUtils
{
    // Install / uninstall tasks
    [RunInstaller(true)]
    public partial class KxInstaller : Installer
    {
        // Fields
        private const string AppManufacturer = "Keysticks.net";
        private const string ProductName = "Keysticks";
        //private const string LicenceFileName = "Keysticks.lic";        
        private const string InstallDirectoriesFileName = "installdirs.txt";
        private const string MessageLogFileName = "Message Log.txt";
        private const string ProfileFileExtension = ".keyx";
        //private const string AppUpdateMSIFilename = "KeysticksSetup.msi";
        //private const string AppUpdaterProgramName = "KeysticksUpdater";
        private const string ConfigFileName = "config.xml";
        private const string ProfileListCacheFileName = "Profiles Cache.dat";
        private const string WordPredictorFileName = "WordPredictor.dll";

        public KxInstaller()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Commit event
        /// </summary>
        /// <param name="savedState"></param>
        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);

            SetPermissions();
            DeleteLogFile();
            RegisterWordPredictor();
        }

        /// <summary>
        /// Uninstall event
        /// </summary>
        /// <param name="savedState"></param>
        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);

            DeleteSampleProfileCopies();
            DeleteCommonAppData();
            DeleteLocalAppData();
            DeleteTempFiles();
            DeleteStartupShortcut();
            DeregisterWordPredictor();
        }

        /// <summary>
        /// Ensure application's common app data folder has write permissions
        /// </summary>
        private void SetPermissions()
        {
            string rootPath = "";
            try
            {
                rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), AppManufacturer);
                DirectoryInfo di = new DirectoryInfo(rootPath);
                if (di.Exists)
                {
                    // Create security idenifier for all users (WorldSid)
                    SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

                    // Add a new file access rule w/ write/modify for all users to the directory security object
                    DirectorySecurity ds = di.GetAccessControl();
                    FileSystemAccessRule rule = new FileSystemAccessRule(sid,
                                                                        FileSystemRights.CreateFiles | FileSystemRights.Modify,
                                                                        InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,   // all sub-dirs to inherit
                                                                        PropagationFlags.None,
                                                                        AccessControlType.Allow);                                            // Turn write and modify on
                    ds.AddAccessRule(rule);

                    // Apply the directory security to the directory
                    di.SetAccessControl(ds);
                }
            }
            catch (Exception ex)
            {
                // Log error
                string message = string.Format("Error while setting permissions for directory '{0}' during install. Details:{1}{2}",
                                                rootPath, Environment.NewLine, ex.Message);
                LogEvent(message, EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Register the word prediction COM component
        /// </summary>
        private void RegisterWordPredictor()
        {
            try
            {
                string targetDir = this.Context.Parameters["targetdir"];
                string filePath = Path.Combine(targetDir, WordPredictorFileName);
                Process.Start("regsvr32", "/s \"" + filePath + "\"");
            }
            catch (Exception ex)
            {
                // Log error
                string message = string.Format("Error while registering {0}. Details:{1}{2}",
                                                WordPredictorFileName, Environment.NewLine, ex.Message);
                LogEvent(message, EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Deregister the word prediction COM component
        /// </summary>
        private void DeregisterWordPredictor()
        {
            try
            {
                string targetDir = this.Context.Parameters["targetdir"];
                string filePath = Path.Combine(targetDir, WordPredictorFileName);
                Process.Start("regsvr32", "/s /u \"" + filePath + "\"");
            }
            catch (Exception ex)
            {
                // Log error
                string message = string.Format("Error while deregistering {0}. Details:{1}{2}",
                                                WordPredictorFileName, Environment.NewLine, ex.Message);
                LogEvent(message, EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Delete the old message log file during install
        /// </summary>
        /// <remarks>Only deleting for this user</remarks>        
        private void DeleteLogFile()
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppManufacturer, ProductName, MessageLogFileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                // Log error
                string message = string.Format("Error while deleting old message log during install. Details:{0}{1}",
                                                Environment.NewLine, ex.Message);
                LogEvent(message, EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Delete any sample profiles that have been deployed to any user profile directories
        /// </summary>
        private void DeleteSampleProfileCopies()
        {
            try
            {
                string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), AppManufacturer, ProductName);

                // Get the list of sample profiles
                string appDataProfilesDir = Path.Combine(appDataPath, "Profiles");
                DirectoryInfo di = new DirectoryInfo(appDataProfilesDir);
                FileInfo[] sampleProfilesList = di.GetFiles("*" + ProfileFileExtension);

                string installDirsFilePath = Path.Combine(appDataPath, InstallDirectoriesFileName);
                if (File.Exists(installDirsFilePath))
                {
                    string[] lines = File.ReadAllLines(installDirsFilePath);
                    foreach (string line in lines)
                    {
                        // Directory is first token
                        string[] tokens = line.Split(',');
                        if (tokens.Length > 0)
                        {
                            string userProfileDir = tokens[0];
                            DirectoryInfo userProfilesDirInfo = new DirectoryInfo(userProfileDir);
                            if (userProfilesDirInfo.Exists)
                            {
                                foreach (FileInfo sampleFile in sampleProfilesList)
                                {
                                    string userFilePath = Path.Combine(userProfileDir, sampleFile.Name);
                                    if (File.Exists(userFilePath))
                                    {
                                        File.Delete(userFilePath);
                                    }
                                }

                                // If the profiles directory is empty, delete it
                                if (userProfilesDirInfo.Exists &&
                                    userProfilesDirInfo.GetFiles().Length == 0 &&
                                    userProfilesDirInfo.GetDirectories().Length == 0)
                                {
                                    userProfilesDirInfo.Delete();                                    
                                }
                            }
                        }
                    }

                    // Remove the install dirs file
                    File.Delete(installDirsFilePath);
                }
            }
            catch (Exception ex)
            {
                // Log error
                string message = string.Format("Error while removing sample profiles during uninstall. Details:{0}{1}",
                                                Environment.NewLine, ex.Message);
                LogEvent(message, EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Delete the common application data that is shared by all users
        /// </summary>
        private void DeleteCommonAppData()
        {
            try 
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), AppManufacturer);
                if (Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, true);
                }
            }
            catch (Exception ex)
            {
                // Log error
                string message = string.Format("Error while deleting common application data during uninstall. Details:{0}{1}",
                                                Environment.NewLine, ex.Message);
                LogEvent(message, EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Delete local application data for installing user        
        /// </summary>
        /// <remarks>Only deleting local app data for this user</remarks>
        private void DeleteLocalAppData()
        {
            try
            {
                string parentDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppManufacturer);
                string childDir = Path.Combine(parentDir, ProductName);

                // Delete config file
                string filePath = Path.Combine(childDir, ConfigFileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Delete profile data cache
                filePath = Path.Combine(childDir, ProfileListCacheFileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Delete message log file
                filePath = Path.Combine(childDir, MessageLogFileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Delete child directory if empty (may still contain user profiles)
                DirectoryInfo dirInfo = new DirectoryInfo(childDir);
                if (dirInfo.GetFiles().Length == 0 &&
                    dirInfo.GetDirectories().Length == 0)
                {
                    dirInfo.Delete();

                    // Delete parent directory if empty (may still contain user profiles)
                    dirInfo = new DirectoryInfo(parentDir);
                    if (dirInfo.GetFiles().Length == 0 &&
                        dirInfo.GetDirectories().Length == 0)
                    {
                        dirInfo.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
                string message = string.Format("Error while deleting local application data during uninstall. Details:{0}{1}",
                                                Environment.NewLine, ex.Message);
                LogEvent(message, EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Delete C:\temp\Keysticks.tmp directory
        /// </summary>
        private void DeleteTempFiles()
        {
            try
            {                
                string tempInstallDir = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), ProductName + ".tmp");
                if (Directory.Exists(tempInstallDir))
                {
                    Directory.Delete(tempInstallDir, true);
                }
            }
            catch (Exception ex)
            {
                // Log error
                string message = string.Format("Error while deleting temporary install directory during uninstall. Details:{0}{1}",
                                                Environment.NewLine, ex.Message);
                LogEvent(message, EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Delete shortcut in Environment.SpecialFolders.Startup folder
        /// </summary>
        private void DeleteStartupShortcut()
        {
            try
            {
                string startupDir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcutPath = Path.Combine(startupDir, ProductName + ".lnk");

                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                }
            }
            catch (Exception ex)
            {
                // Log error
                string message = string.Format("Error while deleting shortcut from startup folder. Details:{0}{1}",
                                                Environment.NewLine, ex.Message);
                LogEvent(message, EventLogEntryType.Warning);
            }
        }
        
        /// <summary>
        /// Write a warning to the error log
        /// </summary>
        /// <param name="message"></param>
        private void LogEvent(string message, EventLogEntryType eventType)
        {
            try
            {
                EventLog.WriteEntry(ProductName, message, eventType);
            }
            catch (Exception)
            {
                // Ignore
            }
        }
    }
}
