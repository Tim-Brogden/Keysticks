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
    // Show or hide the controller window for this player
    public class ToggleControlsWindowAction : BaseAction
    {
        // Properties
        public override EActionType ActionType
        {
            get { return EActionType.ToggleControlsWindow; }
        }

        public override string Name
        {
            get { return Properties.Resources.String_ShowOrHideControls; }
        }

        public override EAnnotationImage IconRef
        {
            get { return EAnnotationImage.Controller; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ToggleControlsWindowAction()
            : base()
        {
        }

        /// <summary>
        /// Start the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void Start(IStateManager parent, KxSourceEventArgs args)
        {
            try
            {
                // Tell the UI to show or hide the controller window for this player
                parent.ThreadManager.SubmitUIEvent(new KxToggleControlsEventArgs(args.SourceID));
            }
            catch (Exception ex)
            {
                parent.ThreadManager.SubmitUIEvent(new KxErrorMessageEventArgs(Properties.Resources.E_ShowOrHideControls, ex));
            }

            IsOngoing = false;
        }
    }
}
