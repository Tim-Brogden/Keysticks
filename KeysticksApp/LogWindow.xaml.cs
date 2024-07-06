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
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Keysticks.Config;
using Keysticks.Core;
using System.Collections.Specialized;
using System.Windows.Media;

namespace Keysticks
{
    /// <summary>
    /// Message log window
    /// </summary>
    public partial class LogWindow : Window
    {
        // Fields
        private ITrayManager _parent;
        private AppConfig _appConfig;
        private MessageLogger _messageLogger;
        private NamedItemList _loggingLevels;
        private ObservableCollection<MessageItem> _logFileMessages = new ObservableCollection<MessageItem>();
        private int _originalLoggingLevel = Constants.DefaultMessageLoggingLevel;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent"></param>
        public LogWindow(ITrayManager parent, MessageLogger messageLogger)
        {
            _parent = parent;
            _messageLogger = messageLogger;

            // Store the original logging level
            _originalLoggingLevel = _messageLogger.LoggingLevel;

            _loggingLevels = new NamedItemList();
            _loggingLevels.Add(new NamedItem(Constants.LoggingLevelErrors, Properties.Resources.String_Errors_only));
            _loggingLevels.Add(new NamedItem(Constants.LoggingLevelInfo, Properties.Resources.String_Errors_and_info));
            _loggingLevels.Add(new NamedItem(Constants.LoggingLevelDebug, Properties.Resources.String_All));

            InitializeComponent();
        }

        /// <summary>
        /// Set the config
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            _appConfig = appConfig;            
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
                // Register autoscroll handler
                ((INotifyCollectionChanged)MessagesTable.Items).CollectionChanged += LogWindow_CollectionChanged;

                // Bind logging levels
                this.LoggingLevelComboBox.ItemsSource = _loggingLevels;
                this.LoggingLevelComboBox.SelectedValue = Constants.LoggingLevelInfo;   // By default, show errors and info while window open

                // Start by showing 'Current session' messages
                this.CurrentSessionRadioButton.IsChecked = true;
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_LoadWindow, ex);
            }
        }

        /// <summary>
        /// Handle addition of data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogWindow_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (MessagesTable.Items.Count > 0)
            {
                var border = VisualTreeHelper.GetChild(MessagesTable, 0) as Decorator;
                if (border != null)
                {
                    // Scroll to end of list
                    var scroll = border.Child as ScrollViewer;
                    if (scroll != null) scroll.ScrollToEnd();
                }
            }
        }

        /// <summary>
        /// Close window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Clear();
            Close();
        }

        /// <summary>
        /// Closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Restore the original logging level before closing
            int loggingLevel = _appConfig.GetIntVal(Constants.ConfigMessageLoggingLevel, Constants.DefaultMessageLoggingLevel);
            if (loggingLevel != _originalLoggingLevel)
            {
                _appConfig.SetIntVal(Constants.ConfigMessageLoggingLevel, _originalLoggingLevel);
                _parent.ApplyAppConfig(_appConfig);
            }

            _parent.ChildWindowClosing(this);
        }

        /// <summary>
        /// New message selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MessagesTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();
                ShowMessageDetails(MessagesTable.SelectedIndex);
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_SelectionChange, ex);
            }
        }

        /// <summary>
        /// Show the details of the selected message, if any
        /// </summary>
        private void ShowMessageDetails(int row)
        {
            string details = null;
            if (row > -1 && row < MessagesTable.Items.Count)
            {
                MessageItem item = (MessageItem)MessagesTable.Items[row];
                details = item.Details;
            }

            DetailsTextBox.Text = !string.IsNullOrEmpty(details) ? details : "";            
        }

        /// <summary>
        /// Save the messages displayed to a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveLogButton_Click(object sender, RoutedEventArgs e)
        {            
            StringBuilder sb = new StringBuilder();
            foreach (MessageItem item in _messageLogger.MessageLog)
            {
                sb.AppendLine(item.ToString());
            }

            // Show the save dialog
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.InitialDirectory = AppConfig.LocalAppDataDir;
            dialog.CheckPathExists = true;
            dialog.DefaultExt = ".txt";
            dialog.Filter = Properties.Resources.String_TextFiles + " (*.txt)|*.txt|" + Properties.Resources.String_AllFiles + " (*.*)|*.*";
            dialog.OverwritePrompt = true;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Prepare csv text
                string filePath = dialog.FileName;

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
        }

        /// <summary>
        /// Clear the log
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                if (CurrentSessionRadioButton.IsChecked == true)
                {
                    _messageLogger.MessageLog.Clear();
                }
                else if (LogFileRadioButton.IsChecked == true)
                {
                    _logFileMessages.Clear();

                    // Delete the log file
                    string filePath = _messageLogger.LogFilePath;
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_ClearLog, ex);
            }
        }

        /// <summary>
        /// Logging type radio checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoggingTypeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                if (CurrentSessionRadioButton.IsChecked == true)
                {
                    this.MessagesTable.ItemsSource = _messageLogger.MessageLog;
                    this.MessagesTable.SelectedIndex = this.MessagesTable.Items.Count - 1;

                    SaveLogButton.IsEnabled = true;
                    LoggingLevelComboBox.IsEnabled = true;
                }
                else if (LogFileRadioButton.IsChecked == true)
                {
                    LoadMessageLogFile();

                    this.MessagesTable.ItemsSource = _logFileMessages;
                    this.MessagesTable.SelectedIndex = this.MessagesTable.Items.Count - 1;

                    SaveLogButton.IsEnabled = false;
                    LoggingLevelComboBox.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_SelectionChange, ex);
            }
        }

        /// <summary>
        /// Logging level changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoggingLevelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                if (e.AddedItems.Count > 0)
                {
                    int loggingLevel = ((NamedItem)e.AddedItems[0]).ID;
                    if (loggingLevel > -1 &&
                        loggingLevel != _messageLogger.LoggingLevel)
                    {
                        _appConfig.SetIntVal(Constants.ConfigMessageLoggingLevel, loggingLevel);
                        _parent.ApplyAppConfig(_appConfig);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_SelectionChange, ex);
            }
        }
        
        /// <summary>
        /// Populate the message list collection
        /// </summary>
        private void LoadMessageLogFile()
        {
            _logFileMessages.Clear();

            string filePath = _messageLogger.LogFilePath;
            if (File.Exists(filePath))
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    StringBuilder sb = new StringBuilder();
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line == "")
                        {
                            if (sb.Length != 0)
                            {
                                // End of message
                                MessageItem messageItem = new MessageItem();
                                if (messageItem.TryParse(sb.ToString()))
                                {
                                    _logFileMessages.Add(messageItem);
                                }
                                sb.Clear();
                            }
                        }
                        else
                        {
                            sb.AppendLine(line);
                        }
                    }

                    if (sb.Length != 0)
                    {
                        MessageItem messageItem = new MessageItem();
                        if (messageItem.TryParse(sb.ToString()))
                        {
                            _logFileMessages.Add(messageItem);
                        }
                    }
                }
            }
        }
    }
}
