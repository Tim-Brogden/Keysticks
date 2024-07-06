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
using System.IO;
using System.Windows;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Sources;

namespace Keysticks
{
    /// <summary>
    /// Import control sets window
    /// </summary>
    public partial class ImportWindow : Window
    {
        // Fields
        private BaseSource _toSource;
        private string _itemName = "";
        private bool _isFromFile;
        private string _filePath;
        private int _fromSourceID;

        // Properties
        public string ItemName { set { _itemName = value; } }
        public bool IsFromFile { get { return _isFromFile; } }
        public string FilePath { get { return _filePath; } }
        public int FromSourceID { get { return _fromSourceID; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="profile"></param>
        public ImportWindow(BaseSource toSource)
        {
            _toSource = toSource;

            InitializeComponent();
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set caption
                string caption = Properties.Resources.String_Import + " " + _itemName;
                this.Title = caption;
                CaptionTextBlock.Text = caption;

                // Set initial selections
                PlayerOneRadioButton.IsChecked = true;
                ThisProfileRadioButton.IsEnabled = _toSource.Profile.VirtualSources.Count > 1;
                if (_toSource.ID != Constants.ID1)
                {
                    ThisProfileRadioButton.IsChecked = true;
                }
                else
                {
                    AnotherProfileRadioButton.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_LoadWindow, ex);
            }
        }

        /// <summary>
        /// Player radio button checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayerRadioButton_Checked(object sender, RoutedEventArgs e)
        {            
        }

        /// <summary>
        /// This profile / another profile checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProfileRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                if (ThisProfileRadioButton.IsChecked == true)
                {
                    // Import from this profile
                    FilePathTextBox.IsEnabled = false;
                    BrowseButton.IsEnabled = false;

                    // Enable valid player buttons
                    int numPlayers = _toSource.Profile.VirtualSources.Count;
                    PlayerOneRadioButton.IsEnabled = _toSource.ID != Constants.ID1;
                    PlayerTwoRadioButton.IsEnabled = _toSource.ID != Constants.ID2;
                    PlayerThreeRadioButton.IsEnabled = numPlayers > 2 && _toSource.ID != Constants.ID3;
                    PlayerFourRadioButton.IsEnabled = numPlayers > 3 && _toSource.ID != Constants.ID4;

                    // Select a valid player
                    if (_toSource.ID != Constants.ID1)
                    {
                        PlayerOneRadioButton.IsChecked = true;
                    }
                    else
                    {
                        PlayerTwoRadioButton.IsChecked = true;
                    }
                }
                else
                {
                    // Import from another profile
                    FilePathTextBox.IsEnabled = true;
                    BrowseButton.IsEnabled = true;

                    // Enable all player buttons
                    PlayerOneRadioButton.IsEnabled = true;
                    PlayerTwoRadioButton.IsEnabled = true;
                    PlayerThreeRadioButton.IsEnabled = true;
                    PlayerFourRadioButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_SelectionChange, ex);
            }
        }

        /// <summary>
        /// Browse for profile file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                string initialDir = Path.Combine(AppConfig.LocalAppDataDir, "Profiles");
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.AddExtension = false;
                dialog.CheckFileExists = true;
                dialog.Filter = Properties.Resources.String_ProfileFiles + " (*.keyx)|*.keyx";
                dialog.InitialDirectory = initialDir;
                dialog.Multiselect = false;
                dialog.ShowReadOnly = false;
                dialog.Title = Properties.Resources.String_ChooseAProfileFile;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Remove the folder path if it's the same as the user's profiles folder
                    string fileName = dialog.FileName;
                    if (fileName.StartsWith(initialDir))
                    {
                        FileInfo fi = new FileInfo(fileName);
                        fileName = fi.Name;
                    }
                    FilePathTextBox.Text = fileName;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_FileSelection, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// OK clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            bool success = true;

            try
            {
                ErrorMessage.Clear();

                // Store selections
                _isFromFile = AnotherProfileRadioButton.IsChecked == true;
                _fromSourceID = GetSelectedPlayerID();
                _filePath = FilePathTextBox.Text;

                // Prepend the profiles folder if no folder specified
                if (!string.IsNullOrWhiteSpace(_filePath) && !_filePath.Contains("\\") && !_filePath.Contains("/"))
                {
                    _filePath = Path.Combine(AppConfig.LocalAppDataDir, "Profiles", _filePath);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_InvalidDetails, ex);
                success = false;
            }

            // Validate
            if (_isFromFile)
            {
                if (_filePath == "")
                {
                    ErrorMessage.Show(Properties.Resources.String_ChooseAProfileFile, null);
                    success = false;
                }
                else if (!File.Exists(_filePath))
                {
                    ErrorMessage.Show(Properties.Resources.E_FileNotFound, null);
                    success = false;
                }
            }

            if (success)
            {
                // Close
                this.DialogResult = true;
                Close();
            }
        }

        /// <summary>
        /// Get the selected player
        /// </summary>
        /// <returns></returns>
        private int GetSelectedPlayerID()
        {
            int playerID = Constants.NoneID;
            if (PlayerOneRadioButton.IsChecked == true)
            {
                playerID = Constants.ID1;
            }
            else if (PlayerTwoRadioButton.IsChecked == true)
            {
                playerID = Constants.ID2;
            }
            else if (PlayerThreeRadioButton.IsChecked == true)
            {
                playerID = Constants.ID3;
            }
            else if (PlayerFourRadioButton.IsChecked == true)
            {
                playerID = Constants.ID4;
            }

            return playerID;
        }

        /// <summary>
        /// Cancel clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Clear();
            Close();
        }
    }
}
