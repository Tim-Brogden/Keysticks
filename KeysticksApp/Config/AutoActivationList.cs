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
using System.Xml;
using Keysticks.Core;
using Keysticks.Sources;

namespace Keysticks.Config
{
    /// <summary>
    /// Stores the rules for automatically activating states when the user switches between program windows
    /// </summary>
    public class AutoActivationList : NamedItemList
    {
        // Fields
        private BaseSource _parent;
        private StateVector _defaultState;

        // Properties
        private bool IsModified { set { _parent.IsModified = value; } }
        public StateVector DefaultState
        {
            get { return _defaultState; }
            set
            {
                if (_defaultState != value)
                {
                    _defaultState = value;
                    IsModified = true;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AutoActivationList(BaseSource parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Add an activation rule
        /// </summary>
        /// <param name="state"></param>
        /// <param name="processName"></param>
        /// <param name="windowTitle"></param>
        /// <param name="matchType"></param>
        public void AddActivation(StateVector state, string processName, string windowTitle, EMatchType matchType)
        {
            int id = GetFirstUnusedID(this.Count + 1);
            AutoActivation activation = new AutoActivation(id, state, processName, windowTitle, matchType);
            this.Add(activation);

            IsModified = true;
        }

        /// <summary>
        /// Set activation rules for a state
        /// </summary>
        /// <param name="state"></param>
        /// <param name="activations"></param>
        public void SetActivations(StateVector state, NamedItemList activations)
        {
            ClearActivations(state);
            foreach (AutoActivation activation in activations)
            {
                this.Add(activation);
            }

            if (activations.Count != 0)
            {
                IsModified = true;
            }
        }

        /// <summary>
        /// Get the state to activate for this process and/or window title
        /// </summary>
        /// <param name="processNameLower">Lower case process name</param>
        /// <param name="windowTitleLower">Lower case window titlebar text</param>
        /// <returns>Null if no state to activate for this process</returns>
        public StateVector GetActivation(string processNameLower, string windowTitleLower)
        {
            StateVector match = null;

            foreach (AutoActivation activation in this)
            {
                if (activation.IsMatch(processNameLower, windowTitleLower)) 
                {
                    match = activation.State;
                    break;
                }
            }

            return match;
        }

        /// <summary>
        /// Get the activation rules for a state
        /// </summary>
        /// <param name="state"></param>
        /// <returns>Null if no auto-activation for this state</returns>
        public NamedItemList GetActivations(StateVector state)
        {
            int stateID = state.ID;
            NamedItemList activations = new NamedItemList();
            foreach (AutoActivation activation in this)
            {
                if (activation.State.ID == stateID)
                {
                    activations.Add(activation);
                }
            }

            return activations;
        }

        /// <summary>
        /// Clear activation for a state
        /// </summary>
        /// <param name="state"></param>
        public void ClearActivations(StateVector state)
        {
            bool removed = false;
            int stateID = state.ID;
            int i = 0;
            while (i < this.Count)
            {
                AutoActivation activation = (AutoActivation)this[i];
                if (activation.State.ID == stateID)
                {
                    this.RemoveAt(i);
                    removed = true;
                    continue;
                }
                i++;
            }

            if (removed)
            {
                IsModified = true;
            }
        }

        /// <summary>
        /// Read from xml
        /// </summary>
        /// <param name="element"></param>
        public void FromXml(XmlElement element)
        {
            // Default state
            XmlElement defaultElement = (XmlElement)element.SelectSingleNode("default");
            if (defaultElement != null)
            {
                _defaultState = new StateVector();
                _defaultState.FromXml(defaultElement);
            }

            // Activation rules
            this.Clear();
            XmlNodeList activationNodes = element.SelectNodes("activation");
            foreach (XmlElement activationElement in activationNodes)
            {
                AutoActivation activation = new AutoActivation();
                activation.FromXml(activationElement);
                if (activation.ID == Constants.DefaultID)
                {
                    // Legacy: Upgrade from pre v2.10
                    activation.ID = GetFirstUnusedID(this.Count + 1);
                }
                this.Add(activation);
            }
        }

        /// <summary>
        /// Write to xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public void ToXml(XmlElement element, XmlDocument doc)
        {
            // Default state
            if (_defaultState != null)
            {
                XmlElement defaultElement = doc.CreateElement("default");
                _defaultState.ToXml(defaultElement, doc);
                element.AppendChild(defaultElement);
            }

            // Activation rules
            foreach (AutoActivation activation in this)
            {
                XmlElement activationElement = doc.CreateElement("activation");
                activation.ToXml(activationElement, doc);
                element.AppendChild(activationElement);
            }
        }

        /// <summary>
        /// Remove any auto-activations when a state is deleted
        /// </summary>
        /// <param name="deletedState"></param>
        public void Validate()
        {
            // Decide which activations to remove
            int i = 0;
            while (i < this.Count)
            {
                AutoActivation activation = (AutoActivation)this[i];
                if (!_parent.StateTree.HasState(activation.State))
                {
                    this.RemoveAt(i);
                    IsModified = true;
                    continue;
                }
                i++;
            }

            // Clear default state if deleted
            if (_defaultState != null && !_parent.StateTree.HasState(_defaultState))
            {
                _defaultState = null;
                IsModified = true;
            }
        }
    }
}
