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
    /// Action for pressing down a keyboard key (and not releasing)
    /// </summary>
    public class PressDownKeyAction : BaseKeyAction
    {
        /// <summary>
        /// Return what type of action this is
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.PressDownKey; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public PressDownKeyAction()
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
                string name;
                if (IsVirtualKey)
                {
                    name = Properties.Resources.String_Press + " [" + base.Name + "]";
                }
                else
                {
                    name = Properties.Resources.String_Press + " " + base.Name;
                }

                return name;
            }
        }

        /// <summary>
        /// Perform the action
        /// </summary>
        public override void Start(IStateManager parent, KxSourceEventArgs args)
        {
            // Hold down key
            parent.KeyStateManager.SetKeyState(KeyData, true, IsVirtualKey);
        }
    }
}
