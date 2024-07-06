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
    /// Load a profile
    /// </summary>
    public class LoadProfileAction : BaseAction
    {
        // Fields
        private string _profileName = "";

        // Properties
        public string ProfileName { get { return _profileName; } set { _profileName = value; } }

        /// <summary>
        /// Type of action
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.LoadProfile; }
        }

        /// <summary>
        /// Action name
        /// </summary>
        public override string Name
        {
            get 
            {
                string name;
                if (_profileName != "")
                {
                    name = string.Format(Properties.Resources.String_LoadProfileX,  _profileName);
                }
                else
                {
                    name = Properties.Resources.String_LoadNewProfile; 
                }
                
                return name; 
            }
        }

        /// <summary>
        /// Action short name
        /// </summary>
        public override string ShortName
        {
            get
            {
                return _profileName != "" ? _profileName : Properties.Resources.String_DefaultProfileName;
            }
        }

        /// <summary>
        /// Icon
        /// </summary>
        public override EAnnotationImage IconRef
        {
            get
            {
                return _profileName != "" ? EAnnotationImage.LoadProfile : EAnnotationImage.NewProfile;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public LoadProfileAction()
            : base()
        {
        }

        /// <summary>
        /// Load from Xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _profileName = element.GetAttribute("profile");

            base.FromXml(element);
        }

        /// <summary>
        /// Write to Xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("profile", _profileName);
        }

        /// <summary>
        /// Start the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void Start(IStateManager parent, KxSourceEventArgs args)
        {
            // Tell the UI to load the requested profile
            parent.ThreadManager.SubmitUIEvent(new KxLoadProfileEventArgs(_profileName));

            IsOngoing = false;
        }
    }
}
