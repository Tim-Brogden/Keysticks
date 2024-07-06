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
using System.Reflection;
using System.IO;
using Keysticks.Config;
using Keysticks.Core;

namespace Keysticks.Sys
{
    /// <summary>
    /// Creates shortcut files
    /// </summary>
    public class ShortCutManager
    {
        /// <summary>
        /// Create a startup shortcut file (.lnk) to the application
        /// </summary>
        public bool CreateStartupShortCut()
        {
            bool success = false;
            try
            {
                string startupDir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcutPath = Path.Combine(startupDir, Constants.ProductName + ".lnk");

                string targetPath = Assembly.GetEntryAssembly().Location;
                string imagePath = Path.Combine(AppConfig.AppProjectDir, Constants.ProductLogoFileName);

                IWshRuntimeLibrary.WshShellClass shell = new IWshRuntimeLibrary.WshShellClass();
                IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);

                shortcut.TargetPath = targetPath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
                if (File.Exists(imagePath))
                {
                    shortcut.IconLocation = imagePath;
                }
                shortcut.Description = Properties.Resources.String_Start + " " + Constants.ProductName;

                // Save it
                shortcut.Save();

                success = true;
            }
            catch (Exception ex)
            {
                throw new KxException(Properties.Resources.E_CreateShortcut, ex);
            }

            return success;
        }

        /// <summary>
        /// Remove the startup shortcut file (.lnk) to the application
        /// </summary>
        public bool RemoveStartupShortCut()
        {
            bool success = false;
            try
            {
                string startupDir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcutPath = Path.Combine(startupDir, Constants.ProductName + ".lnk");

                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                }

                success = true;
            }
            catch (Exception ex)
            {
                throw new KxException(Properties.Resources.E_RemoveShortcut, ex);
            }

            return success;
        }
    }
}
