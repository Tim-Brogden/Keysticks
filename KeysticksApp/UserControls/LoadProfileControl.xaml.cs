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
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Keysticks.Actions;
using Keysticks.Config;
using Keysticks.Core;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for editing Load profile actions
    /// </summary>
    public partial class LoadProfileControl : UserControl
    {
        // Fields
        private NamedItemList _profileListItems = new NamedItemList();
        private LoadProfileAction _currentAction = new LoadProfileAction();

        /// <summary>
        /// Constructor
        /// </summary>
        public LoadProfileControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Control loaded event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.ProfileNameCombo.ItemsSource = _profileListItems;

            RefreshDisplay();
        }

        /// <summary>
        /// Populate the list of profiles
        /// </summary>
        public void SetAppConfig(AppConfig appConfig)
        {
            // Add an option to load a new profile
            _profileListItems.Clear();
            _profileListItems.Add(new NamedItem(Constants.NoneID, Properties.Resources.String_NewProfileOption));

            // Get profiles directory
            string defaultProfilesDir = Path.Combine(AppConfig.LocalAppDataDir, "Profiles");
            string profilesDir = appConfig.GetStringVal(Constants.ConfigUserCurrentProfileDirectory, defaultProfilesDir);

            // Add profiles in profiles directory
            DirectoryInfo dirInfo = new DirectoryInfo(profilesDir);
            if (dirInfo.Exists)
            {
                // Find profiles in directory
                FileInfo[] fileList = dirInfo.GetFiles("*" + Constants.ProfileFileExtension);

                // Get list of file names
                for (int i = 0; i < fileList.Length; i++)
                {
                    string fileName = fileList[i].Name;
                    if (fileName.EndsWith(Constants.ProfileFileExtension))
                    {
                        fileName = fileName.Substring(0, fileName.Length - Constants.ProfileFileExtension.Length);
                    }
                    _profileListItems.Add(new NamedItem(i + 1, fileName));
                }
            }
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is LoadProfileAction)
            {
                _currentAction = (LoadProfileAction)action;
                RefreshDisplay();
            }
        }

        /// <summary>
        /// Redisplay the action
        /// </summary>
        private void RefreshDisplay()
        {
            if (IsLoaded && _currentAction != null)
            {
                int id = Constants.NoneID;
                if (!string.IsNullOrEmpty(_currentAction.ProfileName))
                {
                    NamedItem item = _profileListItems.GetItemByName(_currentAction.ProfileName);
                    if (item != null)
                    {
                        id = item.ID;
                    }
                }
                this.ProfileNameCombo.SelectedValue = id;
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = null;

            NamedItem profileNameItem = (NamedItem)this.ProfileNameCombo.SelectedItem;
            if (profileNameItem != null)
            {
                _currentAction = new LoadProfileAction();
                _currentAction.ProfileName = profileNameItem.ID != Constants.NoneID ? profileNameItem.Name : "";
            }

            return _currentAction;
        }
    }
}
