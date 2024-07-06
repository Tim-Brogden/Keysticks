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
using System.ComponentModel;
using System.Text.RegularExpressions;
using Keysticks.Core;

namespace Keysticks.Core
{
    /// <summary>
    /// Stores the security rule for a command which could be executed by a StartProgram action
    /// </summary>
    public class CommandRuleItem : INotifyPropertyChanged
    {
        // Fields
        private string _command = "";
        private ECommandAction _actionType = ECommandAction.AskMe;
        private static NamedItemList _actionChoices = new NamedItemList() 
        { new NamedItem((int)ECommandAction.Run, Properties.Resources.String_Run), 
            new NamedItem((int)ECommandAction.DontRun, Properties.Resources.String_DontRun), 
            new NamedItem((int)ECommandAction.AskMe, Properties.Resources.String_AskMe) };

        // Properties
        public string Command
        {
            get { return _command; }
            set
            {
                if (_command != value)
                {
                    // Check value provided
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new ArgumentException(Properties.Resources.E_CommandEmpty);
                    }

                    // Validate regex
                    if (value.StartsWith("^"))
                    {
                        try
                        {
                            Regex regex = new Regex(value);
                        }
                        catch (Exception ex)
                        {
                            throw new ArgumentException(Properties.Resources.E_InvalidRegex + " " + Properties.Resources.String_Details + ": " + ex.Message);
                        }
                    }

                    _command = value;
                    NotifyPropertyChanged("Command");
                }
            }
        }
        public ECommandAction ActionType
        {
            get { return _actionType; }
            set
            {
                if (_actionType != value)
                {
                    _actionType = value;
                    NotifyPropertyChanged("ActionType");
                }
            }
        }
        public NamedItem ActionItem
        {
            get { return _actionChoices.GetItemByID((int)_actionType); }
            set
            {
                ECommandAction actionType = (value != null) ? (ECommandAction)value.ID : ECommandAction.None;
                if (actionType != _actionType)
                {
                    _actionType = actionType;
                    NotifyPropertyChanged("ActionItem");
                }
            }
        }
        public NamedItemList Choices { get { return _actionChoices; } }

        // Events
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CommandRuleItem()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="command"></param>
        /// <param name="actionType"></param>
        public CommandRuleItem(string command, ECommandAction actionType)
        {
            _command = command;
            _actionType = actionType;
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
    }
}
