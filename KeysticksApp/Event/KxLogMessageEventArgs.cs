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
    /// Event data for reporting diagnostic information to the UI
    /// </summary>
    public class KxLogMessageEventArgs : KxEventArgs
    {
        // Fields
        public int LoggingLevel = Constants.LoggingLevelInfo;
        public string Text = "";
        public string Details = "";

        // Properties
        public override EEventType EventType { get { return EEventType.LogMessage; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public KxLogMessageEventArgs()
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public KxLogMessageEventArgs(KxLogMessageEventArgs args)
        {
            // Shallow copy
            LoggingLevel = args.LoggingLevel;
            Text = args.Text;
            Details = args.Details;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text"></param>
        public KxLogMessageEventArgs(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggingLevel"></param>
        /// <param name="text"></param>
        public KxLogMessageEventArgs(int loggingLevel, string text)
        {
            LoggingLevel = loggingLevel;
            Text = text;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggingLevel"></param>
        /// <param name="text"></param>
        /// <param name="details"></param>
        public KxLogMessageEventArgs(int loggingLevel, string text, string details)
        {
            LoggingLevel = loggingLevel;
            Text = text;
            Details = details;
        }
    }
}
