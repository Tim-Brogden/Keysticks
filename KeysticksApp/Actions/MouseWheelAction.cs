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
    /// Emulate a mouse wheel up or down
    /// </summary>
    public class MouseWheelAction : BaseAction
    {
        // Config
        private bool _isUpDirection = true;

        // Properties
        public bool IsUpDirection { get { return _isUpDirection; } set { _isUpDirection = value; } }

        /// <summary>
        /// Return the type of action
        /// </summary>
        public override EActionType ActionType
        {
            get 
            {
                return _isUpDirection ? EActionType.MouseWheelUp : EActionType.MouseWheelDown;
            }
        }

        /// <summary>
        /// Return the name of the action
        /// </summary>
        /// <returns></returns>
        public override string Name
        {
            get
            {
                return _isUpDirection ? Properties.Resources.String_MouseWheelUp : Properties.Resources.String_MouseWheelDown;
            }
        }                
        
        /// <summary>
        /// Get the icon to use
        /// </summary>
        public override EAnnotationImage IconRef
        {
            get
            {
                return _isUpDirection ? EAnnotationImage.MouseWheelUp : EAnnotationImage.MouseWheelDown;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MouseWheelAction()
            : base()
        {
        }
        
        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _isUpDirection = bool.Parse(element.GetAttribute("updirection"));

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("updirection", _isUpDirection.ToString());
        }

        /// <summary>
        /// Start the action
        /// </summary>
        public override void Start(IStateManager parent, KxSourceEventArgs args)
        {
            parent.MouseStateManager.ChangeMouseWheel(_isUpDirection);
            IsOngoing = false;
        }
    }
}
