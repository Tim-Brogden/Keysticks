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
using Keysticks.Core;
using System;
using System.Text;
using System.Xml;

namespace Keysticks.Config
{
    public class AutoActivation : NamedItem
    {
        // Fields
        private StateVector _state;
        private string _processName = "";
        private string _processNameLower = "";
        private string _windowTitle = "";
        private string _windowTitleLower = "";
        private EMatchType _matchType = EMatchType.Equals;

        // Properties
        public StateVector State { get { return _state; } }
        public string ProcessName { get { return _processName; } }
        public string WindowTitle { get { return _windowTitle; } }
        public EMatchType MatchType { get { return _matchType; } }
        public string ProcessNameDisplay
        {
            get
            {
                return _processName != "" ? _processName : "(Any)";
            }
        }
        public string WindowTitleDisplay
        {
            get
            {
                string str;
                if (_windowTitle != "")
                {
                    string prefix;
                    switch (_matchType)
                    {
                        case EMatchType.StartsWith:
                            prefix = Properties.Resources.String_Starts + ": "; break;
                        case EMatchType.EndsWith:
                            prefix = Properties.Resources.String_Ends + ": "; break;
                        default:
                            prefix = ""; break;
                    }
                    str = prefix + _windowTitle;
                }
                else
                {
                    str = "(Any)";
                }
                return str;
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public AutoActivation()
            : base()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>        
        public AutoActivation(int id, 
                                StateVector state, 
                                string processName, 
                                string windowTitle, 
                                EMatchType matchType)
            : base()
        {
            ID = id;
            _state = state;
            _processName = processName;
            _windowTitle = windowTitle;
            _matchType = matchType;
            Name = CreateName();
            Initialise();
        }

        private string CreateName()
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(_processName))
            {
                sb.Append(_processName);
                sb.Append(": ");
            }
            if (!string.IsNullOrEmpty(_windowTitle))
            {
                if (_matchType == EMatchType.EndsWith)
                {
                    sb.Append("*");
                }
                sb.Append(_windowTitle);
                if (_matchType == EMatchType.StartsWith)
                {
                    sb.Append("*");
                }
            }
            else
            {
                sb.Append("(Any)");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="activation"></param>
        public AutoActivation(AutoActivation activation)
            :base(activation)
        {
            _state = new StateVector(activation._state);
            _processName = activation._processName;
            _windowTitle = activation._windowTitle;
            _matchType = activation._matchType;
            Initialise();
        }

        public override void FromXml(XmlElement element)
        {
            _state = new StateVector();
            _state.FromXml(element);
            _processName = element.GetAttribute("process");
            if (element.HasAttribute("windowtitle"))    // From v2.10
            {
                base.FromXml(element);
                _windowTitle = element.GetAttribute("windowtitle");
                _matchType = (EMatchType)Enum.Parse(typeof(EMatchType), element.GetAttribute("matchtype"));
            }
            else
            {
                // Legacy: upgrade from pre v2.10
                Name = CreateName();
            }
            Initialise();
        }

        /// <summary>
        /// Initialise
        /// </summary>
        private void Initialise()
        {
            _processNameLower = _processName.ToLower();
            _windowTitleLower = _windowTitle.ToLower();
        }

        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            _state.ToXml(element, doc);
            element.SetAttribute("process", _processName);
            element.SetAttribute("windowtitle", _windowTitle);
            element.SetAttribute("matchtype", _matchType.ToString());
        }

        /// <summary>
        /// See if the specified window details match this rule
        /// </summary>
        /// <param name="processNameLower">Lower case process name</param>
        /// <param name="windowTitleLower">Lower case window titlebar text</param>
        /// <returns></returns>
        public bool IsMatch(string processNameLower, string windowTitleLower)
        {
            bool isMatch = _processNameLower == "" || _processNameLower == processNameLower;
            if (isMatch && _windowTitleLower != "")
            {
                switch (_matchType)
                {
                    case EMatchType.StartsWith:
                        isMatch = windowTitleLower.StartsWith(_windowTitleLower);
                        break;
                    case EMatchType.EndsWith:
                        isMatch = windowTitleLower.EndsWith(_windowTitleLower);
                        break;
                    case EMatchType.Equals:
                        isMatch = windowTitleLower == _windowTitleLower;
                        break;
                    default:
                        isMatch = false;
                        break;
                }
            }

            return isMatch;
        }
    }
}
