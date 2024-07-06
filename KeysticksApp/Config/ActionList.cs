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
using System.Text;
using System.Reflection;
using Keysticks.Actions;
using Keysticks.Core;
using Keysticks.Event;

namespace Keysticks.Config
{
    /// <summary>
    /// Container class for the list of actions to perform in a given logical state when a particular event occurs
    /// </summary>
    public class ActionList : List<BaseAction>
    {
        // Configuration
        private EEventReason _reason;

        // Execution
        private bool _isOngoing = false;
        private KxSourceEventArgs _currentEvent;

        // Properties
        public EEventReason Reason { get { return _reason; } set { _reason = value; } }
        public bool IsOngoing { get { return _isOngoing; } }
        public KxSourceEventArgs CurrentEvent { get { return _currentEvent; } }

        public string Description
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (this.Count != 0)
                {
                    StringUtils utils = new StringUtils();
                    bool isFirst = true;
                    foreach (BaseAction action in this)
                    {
                        string name = action.Name;
                        if (name != "")
                        {
                            if (!isFirst)
                            {
                                sb.Append(", " + Properties.Resources.String_then + string.Format(" {0}{1}", char.ToLower(name[0]), name.Substring(1)));
                            }
                            else
                            {
                                sb.Append(name);
                            }
                            isFirst = false;
                        }
                    }
                }
                return sb.ToString();
            }
        }
        public string ShortDescription
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (this.Count != 0)
                {
                    StringUtils utils = new StringUtils();
                    bool isFirst = true;
                    foreach (BaseAction action in this)
                    {
                        string name = action.ShortName;
                        if (name != "")
                        {
                            if (!isFirst)
                            {
                                sb.Append(", " + Properties.Resources.String_then + string.Format(" {0}{1}", char.ToLower(name[0]), name.Substring(1)));
                            }
                            else
                            {
                                sb.Append(name);
                            }
                            isFirst = false;
                        }
                    }
                }
                return sb.ToString();
            }
        }
        public string TinyDescription
        {
            get
            {
                // Return first action with a tiny name
                string tinyName = "";
                foreach (BaseAction action in this)
                {
                    tinyName = action.TinyName;
                    if (tinyName != "")
                    {
                        break;
                    }
                }
                return tinyName;
            }
        }
        public EAnnotationImage IconRef
        {
            get
            {
                // Return the icon found
                EAnnotationImage iconRef = EAnnotationImage.None;
                foreach (BaseAction action in this)
                {
                    iconRef = action.IconRef;
                    if (iconRef != EAnnotationImage.None)
                    {
                        break;
                    }
                }
                return iconRef;
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ActionList()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="reason"></param>
        public ActionList(EEventReason reason)
        {
            _reason = reason;
        }

        /// <summary>
        /// Activate actions
        /// </summary>
        /// <param name="stateHandler"></param>
        /// <param name="args"></param>
        public void Activate(IStateManager stateHandler, KxSourceEventArgs args)
        {
            _isOngoing = false;
            foreach (BaseAction action in this)
            {
                action.Activate(stateHandler, args);
            }
        }

        /// <summary>
        /// Deactivate actions
        /// </summary>
        /// <param name="stateHandler"></param>
        /// <param name="args"></param>
        public void Deactivate(IStateManager stateHandler, KxSourceEventArgs args)
        {
            _isOngoing = false;
            foreach (BaseAction action in this)
            {
                action.Deactivate(stateHandler, args);
            }
        }

        /// <summary>
        /// Start performing the actions
        /// </summary>
        /// <param name="stateHandler"></param>
        /// <param name="args"></param>
        public void Start(IStateManager stateHandler, KxSourceEventArgs args)
        {
            // Reset ongoing status
            _isOngoing = false;
            _currentEvent = args;

            // Start performing actions
            foreach (BaseAction action in this)
            {
                // Start the action
                action.Start(stateHandler, args);

                // See if the action needs to be continued in next cycle
                if (action.IsOngoing)
                {
                    _isOngoing = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Continue the actions
        /// </summary>
        /// <param name="stateHandler"></param>
        public void Continue(IStateManager stateHandler)
        {
            // Reset ongoing status
            _isOngoing = false;
            bool foundCurrentAction = false;

            int count = 0;
            foreach (BaseAction action in this)
            {
                count++;

                // Skip completed actions at the start
                if (!foundCurrentAction && !action.IsOngoing)
                {
                    continue;
                }
                foundCurrentAction = true;

                if (!action.IsOngoing)
                {
                    // Action isn't started yet - start the next action in the series
                    action.Start(stateHandler, _currentEvent);
                }
                else
                {
                    // Continue the current action
                    action.Continue(stateHandler, _currentEvent);
                }

                // See if the action is finished yet
                if (action.IsOngoing)
                {
                    // Can't move on to the next action yet, because this one is ongoing
                    _isOngoing = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Whether action list has actions of specified type
        /// </summary>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public bool HasActionsOfType(EActionType actionType)
        {
            bool hasActionType = false;
            foreach (BaseAction action in this)
            {
                if (action.ActionType == actionType)
                {
                    hasActionType = true;
                    break;
                }
            }

            return hasActionType;
        }

        /// <summary>
        /// Read from xml
        /// </summary>
        /// <param name="actionListElement"></param>
        public void FromXml(XmlElement actionListElement)
        {
            // Read event reason
            _reason = (EEventReason)Enum.Parse(typeof(EEventReason), actionListElement.GetAttribute("eventreason"));

            // Read actions
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (XmlNode actionNode in actionListElement.ChildNodes)
            {
                if (actionNode is XmlElement)
                {
                    XmlElement actionElement = (XmlElement)actionNode;

                    // Read the action
                    string typeFullName = string.Format("{0}.{1}", typeof(BaseAction).Namespace, actionElement.Name);
                    if (assembly.GetType(typeFullName, false, false) != null)
                    {
                        BaseAction action = (BaseAction)assembly.CreateInstance(typeFullName);
                        if (action != null)
                        {
                            action.FromXml(actionElement);
                            this.Add(action);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Write to xml
        /// </summary>
        /// <param name="parentElement"></param>
        /// <param name="doc"></param>
        public void ToXml(XmlElement parentElement, XmlDocument doc)
        {           
            // Write event reason
            parentElement.SetAttribute("eventreason", _reason.ToString());

            // Write actions
            foreach (BaseAction action in this)
            {
                XmlElement actionElement = doc.CreateElement(action.GetType().Name);
                action.ToXml(actionElement, doc);
                parentElement.AppendChild(actionElement);
            }
        }
    }
}
