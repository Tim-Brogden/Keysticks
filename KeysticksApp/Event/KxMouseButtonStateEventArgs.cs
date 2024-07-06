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
    /// Event data for reporting the state of the mouse buttons
    /// </summary>
    public class KxMouseButtonStateEventArgs : KxEventArgs
    {
        // Fields
        private EMouseState _mouseButton;

        // Properties
        public EMouseState MouseButton { get { return _mouseButton; } }
        public override EEventType EventType { get { return EEventType.MouseButtonState; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        public KxMouseButtonStateEventArgs()
            : base()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="key"></param>
        public KxMouseButtonStateEventArgs(EMouseState mouseButton)
            : base()
        {
            _mouseButton = mouseButton;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public KxMouseButtonStateEventArgs(KxMouseButtonStateEventArgs args)
            : base(args)
        {
            _mouseButton = args._mouseButton;
        }

        /// <summary>
        /// Get string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringUtils utils = new StringUtils();
            return utils.MouseButtonToString(_mouseButton);
        }
    }
}
