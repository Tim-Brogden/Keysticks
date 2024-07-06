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
using Keysticks.Core;

namespace Keysticks.Event
{
    /// <summary>
    /// Stores the state of the keyboard's modifier (e.g. Shift) and toggle keys (e.g. NumLock)
    /// </summary>
    public class KxKeyboardStateEventArgs : KxEventArgs
    {
        // Fields
        public EModifierKeyStates ModifierState;
        public EToggleKeyStates ToggleState;

        // Properties
        public override EEventType EventType { get { return EEventType.KeyboardState; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="modifierState"></param>
        /// <param name="toggleState"></param>
        public KxKeyboardStateEventArgs(EModifierKeyStates modifierState, EToggleKeyStates toggleState)
            : base()
        {
            ModifierState = modifierState;
            ToggleState = toggleState;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public KxKeyboardStateEventArgs(KxKeyboardStateEventArgs args)
        :base(args)
        {
            ModifierState = args.ModifierState;
            ToggleState = args.ToggleState;
        }

        /// <summary>
        /// Get string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringUtils utils = new StringUtils();
            string name = utils.ModifierTypeToString(ModifierState);
            if (ToggleState != EToggleKeyStates.None) {
                if (name != "")
                {
                    name += ", ";
                }
                name += utils.ToggleTypeToString(ToggleState);
            }
            if (name == "")
            {
                name = Properties.Resources.String_None;
            }
            return name;
        }
    }
}
