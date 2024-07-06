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
using System.Collections.Generic;
using System.Xml;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Sys;

namespace Keysticks.Actions
{
    /// <summary>
    /// Maximise/restore or minimise the currently active window
    /// </summary>
    public class ShowCurrentWindowAction : BaseAction
    {
        // Fields
        private bool _maximiseOrMinimise = false;

        // Properties
        public bool MaximiseOrMinimise { get { return _maximiseOrMinimise; } set { _maximiseOrMinimise = value; } }

        /// <summary>
        /// Type of action
        /// </summary>
        public override EActionType ActionType
        {
            get { return _maximiseOrMinimise ? EActionType.MaximiseWindow : EActionType.MinimiseWindow; }
        }

        /// <summary>
        /// Name of action
        /// </summary>
        public override string Name
        {
            get { return _maximiseOrMinimise ? Properties.Resources.String_MaximiseOrRestoreWindow : Properties.Resources.String_MinimiseWindow; }
        }

        /// <summary>
        /// Icon
        /// </summary>
        public override EAnnotationImage IconRef
        {
            get
            {
                return _maximiseOrMinimise ? EAnnotationImage.MaximiseWindow : EAnnotationImage.MinimiseWindow;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ShowCurrentWindowAction()
            : base()
        {
        }

        /// <summary>
        /// Read from Xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _maximiseOrMinimise = bool.Parse(element.GetAttribute("maximise"));

            base.FromXml(element);
        }

        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("maximise", _maximiseOrMinimise.ToString());
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
                IntPtr hWnd = WindowsAPI.GetForegroundWindow();
                if (hWnd != IntPtr.Zero)
                {
                    // Check it's an appropriate type of window
                    uint style = WindowsAPI.GetWindowStyle(hWnd);
                    uint maxOrMinStyle = _maximiseOrMinimise ? WindowsAPI.WS_MAXIMIZEBOX : WindowsAPI.WS_MINIMIZEBOX;
                    if (WindowsAPI.IsStandardWindowStyle(style) && (style & maxOrMinStyle) != 0)
                    {
                        // Get the window state
                        if (_maximiseOrMinimise)
                        {
                            // Maximise / restore
                            uint showState = WindowsAPI.GetWindowShowState(hWnd);
                            if (showState == WindowsAPI.SW_SHOWMAXIMIZED) 
                            {
                                WindowsAPI.ShowWindow(hWnd, WindowsAPI.SW_RESTORE);
                            }
                            else
                            {
                                WindowsAPI.ShowWindow(hWnd, WindowsAPI.SW_SHOWMAXIMIZED);
                            }
                        }
                        else
                        {
                            // Minimise
                            WindowsAPI.ForceMinimiseWindow(hWnd);
                        }                       
                    }
                }
            }
            catch (Exception ex)
            {
                string message = _maximiseOrMinimise ? Properties.Resources.E_MaximiseOrRestoreWindow : Properties.Resources.E_MinimiseWindow;
                parent.ThreadManager.SubmitUIEvent(new KxErrorMessageEventArgs(message, ex));
            }

            IsOngoing = false;
        }
    }
}
