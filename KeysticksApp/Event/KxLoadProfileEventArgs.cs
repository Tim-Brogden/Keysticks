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
    /// Event data for reporting a profile change
    /// </summary>
    public class KxLoadProfileEventArgs : KxEventArgs
    {
        // Fields
        public string ProfileName;

        // Properties
        public override EEventType EventType { get { return EEventType.LoadProfile; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="appName"></param>
        public KxLoadProfileEventArgs(string profileName)
            : base()
        {
            ProfileName = profileName;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public KxLoadProfileEventArgs(KxLoadProfileEventArgs args)
            : base(args)
        {
            if (args.ProfileName != null)
            {
                ProfileName = string.Copy(args.ProfileName);
            }
        }

        /// <summary>
        /// Get string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ProfileName != null ? ProfileName : "";
        }
    }
}
