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

namespace Keysticks.Actions
{
    /// <summary>
    /// Action which does nothing - use to prevent a default action from occurring
    /// </summary>
    public class DoNothingAction : BaseAction
    {

        /// <summary>
        /// Type of action
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.DoNothing; }
        }

        /// <summary>
        /// Action name
        /// </summary>
        public override string Name
        {
            get { return Properties.Resources.String_DoNothing; }
        }

        /// <summary>
        /// Get the icon to use
        /// </summary>
        public override EAnnotationImage IconRef
        {
            get
            {
                return EAnnotationImage.DoNothing;
            }
        }
    }
}
