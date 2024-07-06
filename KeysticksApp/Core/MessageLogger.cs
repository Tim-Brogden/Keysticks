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
using System.IO;
using Keysticks.Config;

namespace Keysticks.Core
{
    /// <summary>
    /// Message logging utility class
    /// </summary>
    public class MessageLogger
    {
        // Fields
        private const int _maxEvents = 5000;
        private int _loggingLevel = Constants.DefaultMessageLoggingLevel;
        private string _logFilePath;
        private ObservableCollection<MessageItem> _messageLog = new ObservableCollection<MessageItem>();

        // Properties
        public int LoggingLevel { get { return _loggingLevel; } set { _loggingLevel = value; } }
        public string LogFilePath
        {
            get
            {
                if (_logFilePath == null)
                {
                    _logFilePath = Path.Combine(AppConfig.LocalAppDataDir, Constants.MessageLogFileName);
                }
                return _logFilePath;
            }
        }
        public ObservableCollection<MessageItem> MessageLog { get { return _messageLog; } }
        
        public void LogMessage(MessageItem messageItem, bool toFile)
        {
            if (toFile)
            {
                LogMessageToFile(messageItem);
            }

            // Check capacity
            if (_messageLog.Count == _maxEvents)
            {
                _messageLog.Clear();
            }

            // Append to in-memory log
            _messageLog.Add(messageItem);
        }

        /// <summary>
        /// Log error to file
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        private void LogMessageToFile(MessageItem messageItem)
        {
            try
            {                
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(messageItem.ToString());
                
                string filePath = LogFilePath;
                
                // Check file size isn't too big
                FileInfo fi = new FileInfo(filePath);
                if (fi.Exists && fi.Length < 1000000)
                {
                    // Append to log file
                    File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
                }
                else
                {
                    // Create/overwrite log file
                    File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                }
            }
            catch (Exception)
            {
                // Ignore
            }
        }        
    }
}
