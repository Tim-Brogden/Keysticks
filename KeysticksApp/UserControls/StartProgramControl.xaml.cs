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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using Keysticks.Actions;
using Keysticks.Core;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for editing Start program actions
    /// </summary>
    public partial class StartProgramControl : UserControl
    {
        // Fields
        private StartProgramAction _currentAction = new StartProgramAction();
        private Window _parentWindow;

        // Properties
        public Window ParentWindow { set { _parentWindow = value; } }

        public StartProgramControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Control loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshDisplay();
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is StartProgramAction)
            {
                _currentAction = (StartProgramAction)action;
                RefreshDisplay();
            }
        }

        /// <summary>
        /// Display the current action
        /// </summary>
        private void RefreshDisplay()
        {
            if (IsLoaded && _currentAction != null)
            {
                ProgramNameTextBox.Text = _currentAction.ProgramName;
                ProgramFolderTextBox.Text = _currentAction.ProgramFolder;
                ProgramArgsTextBox.Text = _currentAction.ProgramArgs;
                CheckIfRunningCheckBox.IsChecked = _currentAction.CheckIfRunning;
                TryBothFoldersCheckBox.IsChecked = _currentAction.TryBothFolders;
            }
        }

        /// <summary>
        /// Let the user browse to a program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseProgramButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.AddExtension = true;
            dialog.CheckFileExists = true;
            dialog.DefaultExt = ".exe";
            dialog.Filter = Properties.Resources.String_ExecutableFiles + "|*.exe";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            dialog.Multiselect = false;
            dialog.RestoreDirectory = true;
            dialog.ShowReadOnly = false;
            dialog.Title = Properties.Resources.StartProgram_ChooseProgramToolTip;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileInfo fi = new FileInfo(dialog.FileName);
                ProgramNameTextBox.Text = fi.Name.Replace(".exe", "");
                ProgramFolderTextBox.Text = fi.DirectoryName;
                ProgramFolderTextBox.ToolTip = fi.DirectoryName;
            }
        }

        /// <summary>
        /// Browse folder clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = Properties.Resources.String_ChooseAFolder;
            dialog.RootFolder = Environment.SpecialFolder.Desktop;
            dialog.ShowNewFolderButton = false;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ProgramFolderTextBox.Text = dialog.SelectedPath;
                ProgramFolderTextBox.ToolTip = ProgramFolderTextBox.Text != "" ? ProgramFolderTextBox.Text : null;
            }
        }

        /// <summary>
        /// Browse document clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.AddExtension = false;
            dialog.CheckFileExists = false;
            dialog.Filter = Properties.Resources.String_AllFiles + "|*.*";
            dialog.Multiselect = true;
            dialog.ShowReadOnly = false;
            dialog.Title = Properties.Resources.String_ChooseFilesOrDocuments;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StringBuilder sb = new StringBuilder();
                bool isFirst = true;
                foreach (string fileName in dialog.FileNames)
                {
                    if (!isFirst)
                    {
                        sb.Append(' ');
                    }
                    isFirst = false;
                    sb.Append('\"');
                    sb.Append(fileName);
                    sb.Append('\"');
                }
                ProgramArgsTextBox.Text = sb.ToString();
                ProgramArgsTextBox.ToolTip = ProgramArgsTextBox.Text != "" ? ProgramArgsTextBox.Text : null;
            }
        }
        
        /// <summary>
        /// Open a dialog to let the user check the program data they have entered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestActionButton_Click(object sender, RoutedEventArgs e)
        {
            string message;
            StartProgramAction action = (StartProgramAction)GetCurrentAction();
            CustomMessageBox messageBox;
            if (action != null)
            {
                message = string.Format(Properties.Resources.Q_TestCommandMessage, 
                                        Environment.NewLine, 
                                        action.GetCommandLine(),
                                        action.CheckIfRunning ? Properties.Resources.String_IfNotRunningOption + " " : "");
                messageBox = new CustomMessageBox(_parentWindow, message, Properties.Resources.Q_TestCommand, MessageBoxButton.YesNoCancel, true, false);
                messageBox.ShowDialog();
                if (messageBox.Result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool detectedAlreadyRunning = false;
                        if (action.CheckIfRunning)
                        {
                            detectedAlreadyRunning = ProcessManager.IsRunning(action.ProgramName);
                        }

                        if (detectedAlreadyRunning)
                        {
                            messageBox = new CustomMessageBox(_parentWindow, Properties.Resources.String_ProgramAlreadyRunningMessage, Properties.Resources.String_AlreadyRunning, MessageBoxButton.OK, true, false);
                            messageBox.ShowDialog();
                        }
                        else
                        {
                            ProcessManager.Start(action);
                            messageBox = new CustomMessageBox(_parentWindow, Properties.Resources.String_CommandSucceededMessage, Properties.Resources.String_CommandSucceeded, MessageBoxButton.OK, true, false);
                            messageBox.ShowDialog();
                        }
                    }
                    catch (Exception ex)
                    {
                        message = Properties.Resources.String_CommandErrorMessage + Environment.NewLine + ex.Message;
                        messageBox = new CustomMessageBox(_parentWindow, message, Properties.Resources.String_CommandError, MessageBoxButton.OK, true, false);
                        messageBox.ShowDialog();
                    }
                }
            }
            else
            {
                message = Properties.Resources.String_InvalidSettingsMessage;
                messageBox = new CustomMessageBox(_parentWindow, message, Properties.Resources.String_InvalidSettings, MessageBoxButton.OK, true, false);
                messageBox.ShowDialog();
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            string programName = this.ProgramNameTextBox.Text.Trim();
            string programFolder = this.ProgramFolderTextBox.Text.Trim();
            string programArgs = this.ProgramArgsTextBox.Text.Trim();
            if (programName != "" || programFolder != "" || programArgs != "")
            {
                _currentAction = new StartProgramAction();
                _currentAction.ProgramName = programName;
                _currentAction.ProgramFolder = programFolder;
                _currentAction.ProgramArgs = programArgs;
                _currentAction.CheckIfRunning = CheckIfRunningCheckBox.IsChecked == true;
                _currentAction.TryBothFolders = TryBothFoldersCheckBox.IsChecked == true;
            }
            else
            {
                _currentAction = null;
            }

            return _currentAction;
        }

        /// <summary>
        /// Folder text box lost kb focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgramFolderTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ProgramFolderTextBox.ToolTip = ProgramFolderTextBox.Text != "" ? ProgramFolderTextBox.Text : null;
        }

        /// <summary>
        /// Program args text box lost kb focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgramArgsTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ProgramArgsTextBox.ToolTip = ProgramArgsTextBox.Text != "" ? ProgramArgsTextBox.Text : null;
        }

    }
}
