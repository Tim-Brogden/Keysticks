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
using System.Text;
using Keysticks.Core;
using Keysticks.Event;

namespace Keysticks.Config
{
    // Manages the textual description and icon for an action set
    public class ActionSetAnnotation
    {
        // Fields
        private ActionSet _actionSet;
        private string _description;
        private string _shortDescription;
        private string _tinyDescription;
        private EAnnotationImage _iconRef = EAnnotationImage.None;

        // Properties
        public string Description { get { return _description; } }
        public string ShortDescription { get { return _shortDescription; } }
        public string TinyDescription { get { return _tinyDescription; } }
        public EAnnotationImage IconRef { get { return _iconRef; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="actionSet"></param>
        public ActionSetAnnotation(ActionSet actionSet)
        {
            _actionSet = actionSet;

            _description = GetDescription();
            _shortDescription = GetShortDescription();
            if (actionSet.AlternativeText != null)
            {
                _tinyDescription = actionSet.AlternativeText;
            }
            else
            {
                _tinyDescription = GetTinyDescription();
            }

            if (actionSet.AlternativeIcon != EAnnotationImage.None)
            {
                _iconRef = actionSet.AlternativeIcon;
            }
            else
            {
                _iconRef = GetIconRef();
            }
        }


        /// <summary>
        /// Get a full description of the action set
        /// </summary>
        /// <returns></returns>
        private string GetDescription()
        {
            StringBuilder sb = new StringBuilder();
            StringUtils utils = new StringUtils();
            ELRUDState direction = _actionSet.EventArgs.LRUDState;
            foreach (EEventReason reason in Enum.GetValues(typeof(EEventReason)))
            {
                ActionList actionList = _actionSet.GetActions(reason);
                if (actionList != null)
                {
                    sb.Append(Properties.Resources.String_When + " ");
                    string eventDesc = utils.GetEventDescription(actionList.Reason, direction);
                    if (eventDesc.Length > 0)
                    {
                        sb.Append(char.ToLower(eventDesc[0]));
                        sb.Append(eventDesc.Substring(1));
                    }
                    sb.Append(": ");
                    sb.AppendLine(actionList.Description);
                }
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Return short description
        /// </summary>
        /// <returns></returns>
        private string GetShortDescription()
        {
            string previous = "";
            StringBuilder sb = new StringBuilder();
            foreach (EEventReason reason in Enum.GetValues(typeof(EEventReason)))
            {
                ActionList actionList = _actionSet.GetActions(reason);
                if (actionList != null)
                {
                    string desc = actionList.ShortDescription;
                    // Don't duplicate description where 2 action lists have identical descriptions (e.g. with auto-repeat action sets)
                    if (!string.Equals(desc, previous))
                    {
                        if (sb.Length != 0)
                        {
                            sb.Append(" / ");
                        }
                        sb.Append(desc);
                        previous = desc;
                    }
                }
            }

            // Indicate when auto-repeat is in use
            if (_actionSet.HasActionsForReason(EEventReason.DirectionRepeated) ||
                _actionSet.HasActionsForReason(EEventReason.PressRepeated))
            {
                sb.Append(" (auto-repeat)");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Return tiny name of first action list that has a tiny name             
        /// </summary>
        /// <returns></returns>
        private string GetTinyDescription()
        {
            string tinyDescription = "";
            foreach (EEventReason reason in Enum.GetValues(typeof(EEventReason)))
            {
                ActionList actionList = _actionSet.GetActions(reason);
                if (actionList != null)
                {
                    tinyDescription = actionList.TinyDescription;
                    if (tinyDescription != "")
                    {
                        break;
                    }
                }
            }

            return tinyDescription;
        }

        /// <summary>
        /// Get the icon to use for this action set
        /// </summary>
        /// <returns></returns>
        private EAnnotationImage GetIconRef()
        {
            EAnnotationImage iconRef = EAnnotationImage.None;
            foreach (ActionList actionList in _actionSet.ActionLists)
            {
                iconRef = actionList.IconRef;
                if (iconRef != EAnnotationImage.None)
                {
                    break;
                }
            }

            return iconRef;
        }

    }
}
