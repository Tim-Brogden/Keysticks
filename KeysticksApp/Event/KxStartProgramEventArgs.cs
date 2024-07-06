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
using Keysticks.Actions;
using Keysticks.Core;

namespace Keysticks.Event
{
    /// <summary>
    /// Event data for a start process action
    /// </summary>
    class KxStartProgramEventArgs : KxEventArgs
    {
        // Fields
        public StartProgramAction Action;

        // Properties
        public override EEventType EventType { get { return EEventType.StartProgram; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="action"></param>
        public KxStartProgramEventArgs(StartProgramAction action)
            : base()
        {
            Action = action;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public KxStartProgramEventArgs(KxStartProgramEventArgs args)
            : base(args)
        {
            if (args.Action != null)
            {
                Action = new StartProgramAction(args.Action);
            }
        }

        /// <summary>
        /// Get string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Action != null ? Action.Name : "";
        }
    }
}
