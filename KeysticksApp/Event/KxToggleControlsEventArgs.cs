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
using System.Globalization;
using Keysticks.Core;

namespace Keysticks.Event
{
    /// <summary>
    /// Event for requesting the controller window to be shown or hidden for the specified player
    /// </summary>
    public class KxToggleControlsEventArgs : KxEventArgs
    {
        // Fields
        private int _playerID;

        // Properties
        public int PlayerID { get { return _playerID; } }
        public override EEventType EventType { get { return EEventType.ToggleControls; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        public KxToggleControlsEventArgs()
            : base()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="playerID"></param>
        public KxToggleControlsEventArgs(int playerID)
            : base()
        {
            _playerID = playerID;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public KxToggleControlsEventArgs(KxToggleControlsEventArgs args)
            : base(args)
        {
            _playerID = args._playerID;
        }

        /// <summary>
        /// Get string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} {1} {2}", 
                Properties.Resources.String_Player, 
                _playerID.ToString(CultureInfo.InvariantCulture),
                Properties.Resources.String_toggle_controls);
        }
    }
}
