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
    /// Action for configuring the dwell and auto-repeat times of a control
    /// </summary>
    public class SetDwellAndAutorepeatAction : BaseAction
    {
        // Fields
        private int _dwellTimeMS = Constants.DefaultHoldTimeMS;
        private int _autoRepeatIntervalMS = Constants.DefaultAutoRepeatIntervalMS;        
        
        // Properties
        public int DwellTimeMS { get { return _dwellTimeMS; } set { _dwellTimeMS = value; } }
        public int AutoRepeatInterval { get { return _autoRepeatIntervalMS; } set { _autoRepeatIntervalMS = value; } }
        
        /// <summary>
        /// Return what type of action this is
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.SetDwellAndAutorepeat; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public SetDwellAndAutorepeatAction()
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
                return string.Format(Properties.Resources.String_SetToDwellRepeat,
                                            (0.001 * _dwellTimeMS).ToString(),
                                            (0.001 * _autoRepeatIntervalMS).ToString());
            }
        }
        
        /// <summary>
        /// Tiny name
        /// </summary>
        public override string TinyName
        {
            get
            {
                return string.Format(Properties.Resources.String_DwellRepeat,
                                            (0.001 * _dwellTimeMS).ToString(),
                                            (0.001 * _autoRepeatIntervalMS).ToString());
            }
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _dwellTimeMS = int.Parse(element.GetAttribute("dwell"), System.Globalization.CultureInfo.InvariantCulture);
            _autoRepeatIntervalMS = int.Parse(element.GetAttribute("repeatinterval"), System.Globalization.CultureInfo.InvariantCulture);

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("dwell", _dwellTimeMS.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("repeatinterval", _autoRepeatIntervalMS.ToString(System.Globalization.CultureInfo.InvariantCulture));
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
                BaseControl control = source.GetVirtualControl(args);
                if (control != null)
                {
                    control.ApplyHoldTime(_dwellTimeMS, enable);
                    control.ApplyAutoRepeatInterval(_autoRepeatIntervalMS, enable);
                }
            }
        }
    }
}
