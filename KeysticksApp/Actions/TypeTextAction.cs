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
using Keysticks.Core;
using Keysticks.Event;

namespace Keysticks.Actions
{
    /// <summary>
    /// Action for typing a text string
    /// </summary>
    public class TypeTextAction : BaseAction
    {
        // Fields
        private string _textToType = "";
        private string _name = "";
        private string _tinyName = "";

        // Properties
        public string TextToType { get { return _textToType; } set { _textToType = value; } }

        /// <summary>
        /// Return what type of action this is
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.TypeText; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public TypeTextAction()
            :base()
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
                return _name;
            }
        }

        /// <summary>
        /// Return the tiny name of the action
        /// </summary>
        public override string TinyName
        {
            get
            {
                return _tinyName;
            }
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _textToType = element.GetAttribute("text");

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("text", _textToType);
        }

        /// <summary>
        /// Recreate the names when the text changes
        /// </summary>
        public override void Initialise(IKeyboardContext context)
        {
            base.Initialise(context);

            string singleLine = _textToType.Replace("\r", "").Replace('\n', ' ');
            _name = Properties.Resources.String_Type + " '" + singleLine + "'";
            _tinyName = singleLine.Substring(0, Math.Min(singleLine.Length, Constants.MaxTinyDescriptionLen));
        }

        /// <summary>
        /// Perform the action
        /// </summary>
        public override void Start(IStateManager parent, KxSourceEventArgs args)
        {
            parent.HandleTextEvent(new KxTextEventArgs(_textToType));
        }
    }
}
