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
using Keysticks.Config;

namespace Keysticks.Event
{
    /// <summary>
    /// Event data for logical state changes
    /// </summary>
    public class KxStateChangeEventArgs : KxEventArgs
    {
        // Fields
        private int _playerID;
        private StateVector _logicalState;

        // Properties
        public int PlayerID { get { return _playerID; } }
        public StateVector LogicalState { get { return _logicalState; } }
        public override EEventType EventType { get { return EEventType.StateChange; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        public KxStateChangeEventArgs()
            : base()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="state"></param>
        /// <param name="eventReason"></param>
        public KxStateChangeEventArgs(int playerID, StateVector state)
            : base()
        {
            _playerID = playerID;
            _logicalState = state;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public KxStateChangeEventArgs(KxStateChangeEventArgs args)
            :base(args)
        {
            _playerID = args._playerID;
            _logicalState = new StateVector(args._logicalState);
        }

        /// <summary>
        /// Get string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} {1}: {2}", 
                Properties.Resources.String_Player, 
                _playerID, 
                _logicalState != null ? _logicalState.ToString() : "null");
        }
    }
}
