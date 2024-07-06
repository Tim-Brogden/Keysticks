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
using Keysticks.Actions;
using Keysticks.Core;
using Keysticks.Event;

namespace Keysticks.Config
{
    /// <summary>
    /// Stores action lists for a control for each event reason it supports
    /// </summary>
    public class ActionSet
    {
        // Fields
        private List<ActionList> _actionLists = new List<ActionList>();
        private StateVector _logicalState;
        private KxControlEventArgs _eventArgs;
        private bool _isActive = false;
        private bool _isActivePending = false;
        private string _alternativeText = null;
        private EAnnotationImage _alternativeIcon = EAnnotationImage.None;
        private ActionSetAnnotation _annotationInfo = null;
        
        // Properties
        public List<ActionList> ActionLists { get { return _actionLists; } }
        public StateVector LogicalState { get { return _logicalState; } set { _logicalState = value; } }
        public KxControlEventArgs EventArgs { get { return _eventArgs; } set { _eventArgs = value; } }
        public bool IsActivePending { get { return _isActivePending; } set { _isActivePending = value; } }
        public bool IsActive { get { return _isActive; } }
        public string AlternativeText
        {
            get { return _alternativeText; }
            set
            {
                if (_alternativeText != value)
                {
                    _alternativeText = value;
                    Updated();
                }
            }
        }
        public EAnnotationImage AlternativeIcon
        {
            get { return _alternativeIcon; }
            set
            {
                if (_alternativeIcon != value)
                {
                    _alternativeIcon = value;
                    Updated();
                }
            }
        }
        public ActionSetAnnotation Info
        {
            get
            {
                if (_annotationInfo == null)
                {
                    _annotationInfo = new ActionSetAnnotation(this);
                }
                return _annotationInfo;
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ActionSet()
        {
        }
        
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="actionSet"></param>
        public ActionSet(ActionSet actionSet)
        {
            // Using Xml doc here, which is slightly lazy
            XmlDocument doc = new XmlDocument();
            XmlElement actionSetElement = doc.CreateElement("actionset");
            doc.AppendChild(actionSetElement);

            actionSet.ToXml(actionSetElement, doc);
            FromXml(actionSetElement);
        }
        
        /// <summary>
        /// Set the actions for a particular reason
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="actionList">May be null</param>
        public void SetActions(EEventReason reason, ActionList actionList)
        {
            int index = -1;
            for (int i = 0; i < _actionLists.Count; i++)
            {
                if (_actionLists[i].Reason == reason)
                {
                    // Replace or remove existing action list
                    if (actionList != null)
                    {
                        _actionLists[i] = actionList;
                    }
                    else
                    {
                        _actionLists.RemoveAt(i);
                    }
                    index = i;
                    break;
                }
            }

            if (index == -1 && actionList != null)
            {
                // New reason

                // Find the best place to insert it
                index = 0;
                foreach (ActionList list in _actionLists)
                {
                    if (reason < list.Reason)
                    {
                        break;
                    }
                    index++;
                }
                _actionLists.Insert(index, actionList);
            }

            Updated();
        }

        /// <summary>
        /// Get the actions for a particular reason
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public ActionList GetActions(EEventReason reason)
        {
            ActionList actionList = null;
            foreach (ActionList list in _actionLists)
            {
                if (list.Reason == reason)
                {
                    actionList = list;
                    break;
                }
            }

            return actionList;
        }

        /// <summary>
        /// Get all the actions in the action set
        /// </summary>
        /// <returns></returns>
        public List<BaseAction> GetAllActions()
        {
            List<BaseAction> allActionsList = new List<BaseAction>();
            foreach (ActionList actionList in _actionLists)
            {
                allActionsList.AddRange(actionList);
            }

            return allActionsList;
        }

        /// <summary>
        /// Initialise the action names
        /// </summary>
        /// <param name="context"></param>
        public void Initialise(IKeyboardContext context)
        {
            foreach (ActionList actionList in _actionLists)
            {
                foreach (BaseAction action in actionList)
                {
                    // Update the action names
                    action.Initialise(context);
                }
            }

            // Trigger a refresh of the descriptions
            Updated();
        }

        /// <summary>
        /// Call when actions have been updated
        /// </summary>
        public void Updated()
        {
            // Trigger recalculation of description and icon
            _annotationInfo = null;
        }

        /// <summary>
        /// See if the action set has actions for this reason
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool HasActionsForReason(EEventReason reason)
        {
            ActionList actionList = GetActions(reason);

            return (actionList != null && actionList.Count != 0);
        }

        /// <summary>
        /// See if this action set has any actions
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            bool isEmpty = true;
            foreach (ActionList actionList in _actionLists)
            {
                if (actionList.Count != 0)
                {
                    isEmpty = false;
                    break;
                }
            }

            return isEmpty;
        }

        /// <summary>
        /// Activate the action set
        /// </summary>
        /// <param name="stateManager"></param>
        public void Activate(IStateManager stateManager, KxSourceEventArgs args)
        {
            _isActive = true;
            foreach (ActionList actionList in _actionLists)
            {
                actionList.Activate(stateManager, args);
            }
        }

        /// <summary>
        /// Deactivate the action set
        /// </summary>
        /// <param name="stateManager"></param>
        public void Deactivate(IStateManager stateManager, KxSourceEventArgs args)
        {
            _isActive = false;
            foreach (ActionList actionList in _actionLists)
            {
                actionList.Deactivate(stateManager, args);
            }
        }

        /// <summary>
        /// Read from xml
        /// </summary>
        /// <param name="actionListElement"></param>
        public void FromXml(XmlElement element)
        {
            // Read logical state
            _logicalState = new StateVector();
            _logicalState.FromXml(element);

            // Read event args
            KxControlEventArgs args = new KxControlEventArgs();
            if (args.FromXml(element))
            {
                _eventArgs = args;
                //_eventArgs.EventReason = EEventReason.None;     // Action sets are not reason specific
            }

            // Custom text / icon
            if (element.HasAttribute("alternativetext"))
            {
                _alternativeText = element.GetAttribute("alternativetext");
            }
            if (element.HasAttribute("alternativeicon"))
            {
                _alternativeIcon = (EAnnotationImage)Enum.Parse(typeof(EAnnotationImage), element.GetAttribute("alternativeicon"));
            }

            // Read action lists
            XmlNodeList actionListNodes = element.SelectNodes("actionlist");
            foreach (XmlElement actionListElement in actionListNodes)
            {
                ActionList actionList = new ActionList();
                actionList.FromXml(actionListElement);

                SetActions(actionList.Reason, actionList);
            }
        }

        /// <summary>
        /// Write to xml
        /// </summary>
        /// <param name="parentElement"></param>
        /// <param name="doc"></param>
        public void ToXml(XmlElement parentElement, XmlDocument doc)
        {
            // Write logical state
            _logicalState.ToXml(parentElement, doc);

            // Write eventtype
            if (_eventArgs != null)
            {
                _eventArgs.ToXml(parentElement, doc);
            }

            // Write custom text / icon
            if (_alternativeText != null)
            {
                parentElement.SetAttribute("alternativetext", _alternativeText);
            }
            if (_alternativeIcon != EAnnotationImage.None)
            {
                parentElement.SetAttribute("alternativeicon", _alternativeIcon.ToString());
            }

            // Write action lists
            foreach (ActionList actionList in _actionLists)
            {
                XmlElement actionListElement = doc.CreateElement("actionlist");
                actionList.ToXml(actionListElement, doc);
                parentElement.AppendChild(actionListElement);                
            }
        }

    }
}
