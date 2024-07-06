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
using Keysticks.Event;

namespace Keysticks.Actions
{
    /// <summary>
    /// Base class for all actions
    /// </summary>
    public abstract class BaseAction
    {
        private bool _isOngoing = false;
        private ParamDefinition[] _requiredParameters;

        abstract public EActionType ActionType { get; }
        public ParamDefinition[] RequiredParameters { get { return _requiredParameters; } }
        public bool IsOngoing { get { return _isOngoing; } protected set { _isOngoing = value; } }

        /// <summary>
        /// Name of action
        /// </summary>
        public abstract string Name { get; }
        public virtual string ShortName { get { return Name; } }
        public virtual string TinyName { get { return ""; } }
        public virtual EAnnotationImage IconRef { get { return EAnnotationImage.None; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseAction()
        {
        }
        
        /// <summary>
        /// Set the parameters required by the action
        /// </summary>
        /// <param name="parametersTypes"></param>
        protected void SetRequiredParameters(ParamDefinition[] requiredParameters)
        {
            // Initialise arrays for parameter types and sources and value getting methods
            _requiredParameters = requiredParameters;
        }
        
        /// <summary>
        /// Update internal data after a property has changed
        /// </summary>
        public virtual void Initialise(IKeyboardContext context)
        {
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public virtual void FromXml(XmlElement element)
        {
        }        

        /// <summary>
        /// Convert to xml representation
        /// </summary>
        /// <param name="element"></param>
        public virtual void ToXml(XmlElement element, XmlDocument doc)
        {            
        }

        /// <summary>
        /// Activate the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public virtual void Activate(IStateManager parent, KxSourceEventArgs args)
        {
            _isOngoing = false;
        }

        /// <summary>
        /// Deactivate the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public virtual void Deactivate(IStateManager parent, KxSourceEventArgs args)
        {
            _isOngoing = false;
        }

        /// <summary>
        /// Start performing the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public virtual void Start(IStateManager parent, KxSourceEventArgs args)
        {
            _isOngoing = false;
        }

        /// <summary>
        /// Continue the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public virtual void Continue(IStateManager parent, KxSourceEventArgs args)
        {
            _isOngoing = false;
        }

    }
}
