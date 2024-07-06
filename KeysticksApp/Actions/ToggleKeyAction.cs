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
using Keysticks.Core;
using Keysticks.Event;

namespace Keysticks.Actions
{
    /// <summary>
    /// Action for releasing a keyboard key if it's pressed, and vice versa
    /// </summary>
    public class ToggleKeyAction : BaseKeyAction
    {
        /// <summary>
        /// Return what type of action this is
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.ToggleKey; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public ToggleKeyAction()
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
                    name = Properties.Resources.String_Toggle + " [" + base.Name + "]";
                }
                else
                {
                    name = Properties.Resources.String_Toggle + " " + base.Name;
                }

                return name;
            }
        }

        /// <summary>
        /// Perform the action
        /// </summary>
        public override void Start(IStateManager parent, KxSourceEventArgs args)
        {
            // Toggle key
            parent.KeyStateManager.ToggleKeyState(KeyData, IsVirtualKey);
        }
    }
}
