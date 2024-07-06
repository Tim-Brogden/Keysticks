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
using System.Xml;
using Keysticks.Controls;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Sources;

namespace Keysticks.Actions
{
    /// <summary>
    /// Action for configuring a directional control
    /// </summary>
    public class SetDirectionModeAction : BaseAction
    {
        // Fields
        private StringUtils _utils = new StringUtils();
        private EDirectionMode _directionMode = Constants.DefaultDirectionMode;

        // Properties
        public EDirectionMode DirectionMode { get { return _directionMode; } set { _directionMode = value; } }

        /// <summary>
        /// Return what type of action this is
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.SetDirectionMode; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public SetDirectionModeAction()
            : base()
        {
        }

        /// <summary>
        /// Return the name of the action
        /// </summary>
        /// <returns></returns>
        public override string Name
        {
            get
            {
                return string.Format(Properties.Resources.String_UseXDirections, _utils.DirectionModeToString(_directionMode).ToLower());
            }
        }
       
        /// <summary>
        /// Tiny name
        /// </summary>
        public override string TinyName
        {
            get
            {
                return _utils.DirectionModeToString(_directionMode);
            }
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _directionMode = (EDirectionMode)Enum.Parse(typeof(EDirectionMode), element.GetAttribute("mode"));

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("mode", _directionMode.ToString());
        }

        /// <summary>
        /// Activate the setting
        /// </summary>
        public override void Activate(IStateManager parent, KxSourceEventArgs args)
        {
            ApplySetting(parent, args, true);
        }

        /// <summary>
        /// Deactivate the setting
        /// </summary>
        public override void Deactivate(IStateManager parent, KxSourceEventArgs args)
        {
            ApplySetting(parent, args, false);
        }

        /// <summary>
        /// Apply the setting
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        /// <param name="enable"></param>
        private void ApplySetting(IStateManager parent, KxSourceEventArgs args, bool enable)
        {
            BaseSource source = parent.CurrentProfile.GetSource(args.SourceID);
            if (source != null)
            {
                DirectionalControl control = source.GetVirtualControl(args) as DirectionalControl;
                if (control != null)
                {
                    control.ApplyDirectionMode(_directionMode, enable);
                }
            }            
        }
       
    }
}
