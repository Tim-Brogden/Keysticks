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
using System.Diagnostics;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Sys;

namespace Keysticks.Actions
{
    /// <summary>
    /// Set the active window
    /// </summary>
    public class ActivateWindowAction : BaseAction
    {
        // Fields
        private string _programName = "";
        private string _windowTitle = "";
        private string _windowTitleLower = "";
        private EMatchType _matchType = EMatchType.Equals;
        private bool _restoreIfMinimised = true;
        private bool _minimiseIfActive = true;
        private bool _includeHiddenWindows = false;
        private IntPtr _matchedWindow;
        private CallBackPtr _enumWindowsCallback;

        // Properties
        public string ProgramName { get { return _programName; } set { _programName = value; } }
        public string WindowTitle { get { return _windowTitle; } set { _windowTitle = value; } }
        public EMatchType MatchType { get { return _matchType; } set { _matchType = value; } }
        public bool RestoreIfMinimised { get { return _restoreIfMinimised; } set { _restoreIfMinimised = value; } }
        public bool MinimiseIfActive { get { return _minimiseIfActive; } set { _minimiseIfActive = value; } }
        public bool IncludeHiddenWindows { get { return _includeHiddenWindows; } set { _includeHiddenWindows = value; } }
        public override EActionType ActionType
        {
            get { return EActionType.ActivateWindow; }
        }

        /// <summary>
        /// Name of action
        /// </summary>
        public override string Name
        {
            get 
            {
                string name = ShortName;
                if (_restoreIfMinimised)
                {
                    if (_minimiseIfActive)
                    {
                        name += string.Format(" ({0}, {1})", Properties.Resources.String_restore, Properties.Resources.String_minimise);
                    }
                    else
                    {
                        name += string.Format(" ({0})", Properties.Resources.String_restore);
                    }
                }
                else if (_minimiseIfActive)
                {
                    name += string.Format(" ({0})", Properties.Resources.String_minimise);
                }
                return name;
            }
        }

        public override string ShortName
        {
            get
            {
                string matchTypeText;
                switch (_matchType)
                {
                    case EMatchType.StartsWith:
                        matchTypeText = Properties.Resources.String_starting + " "; break;
                    case EMatchType.EndsWith:
                        matchTypeText = Properties.Resources.String_ending + " "; break;
                    default:
                        matchTypeText = ""; break;
                }
            
                return Properties.Resources.String_ActivateWindow + string.Format(" {0}'{1}'", matchTypeText, _windowTitle);
            }
        }

        /// <summary>
        /// Icon
        /// </summary>
        public override EAnnotationImage IconRef
        {
            get
            {
                return EAnnotationImage.ActivateWindow;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ActivateWindowAction()
            : base()
        {
        }

        /// <summary>
        /// Updated
        /// </summary>
        public override void Initialise(IKeyboardContext context)
        {
            base.Initialise(context);

            _windowTitleLower = _windowTitle.ToLower();
        }

        /// <summary>
        /// Read from Xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _programName = element.GetAttribute("program");
            _windowTitle = element.GetAttribute("window");
            _matchType = (EMatchType)Enum.Parse(typeof(EMatchType), element.GetAttribute("matchtype"));
            _restoreIfMinimised = bool.Parse(element.GetAttribute("restore"));
            _minimiseIfActive = bool.Parse(element.GetAttribute("minimise"));
            _includeHiddenWindows = bool.Parse(element.GetAttribute("includehidden"));

            base.FromXml(element);
        }

        /// <summary>
        /// Write to Xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("program", _programName);
            element.SetAttribute("window", _windowTitle);
            element.SetAttribute("matchtype", _matchType.ToString());
            element.SetAttribute("restore", _restoreIfMinimised.ToString());
            element.SetAttribute("minimise", _minimiseIfActive.ToString());
            element.SetAttribute("includehidden", _includeHiddenWindows.ToString());
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
                _matchedWindow = IntPtr.Zero;
                _enumWindowsCallback = new CallBackPtr(this.EnumWindowsCallback);
                if (_programName != "")                
                {
                    string processName = _programName;
                    if (processName.EndsWith(".exe"))
                    {
                        processName = processName.Substring(0, processName.Length - 4);
                    }
                    Process[] processes = Process.GetProcessesByName(processName);
                    if (processes != null)
                    {
                        foreach (Process process in processes)
                        {
                            foreach (ProcessThread thread in process.Threads)
                            {
                                if (!WindowsAPI.EnumThreadWindows((uint)thread.Id, _enumWindowsCallback, IntPtr.Zero))
                                {
                                    break;
                                }
                            }

                            if (_matchedWindow != IntPtr.Zero)
                            {
                                // Matched a window
                                break;
                            }
                        }
                    }
                }
                else
                {
                    WindowsAPI.EnumWindows(_enumWindowsCallback, IntPtr.Zero);
                }

                if (_matchedWindow != IntPtr.Zero)
                {
                    WindowsAPI.ForceActivateWindow(_matchedWindow, _restoreIfMinimised, _minimiseIfActive);
                }
            }
            catch (Exception ex)
            {
                parent.ThreadManager.SubmitUIEvent(new KxErrorMessageEventArgs(Properties.Resources.E_ActivateWindow, ex));
            }

            IsOngoing = false;
        }

        /// <summary>
        /// Callback for EnumThreadWindows / EnumWindows calls
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private bool EnumWindowsCallback(IntPtr hWnd, IntPtr lParam)
        {
            try
            {
                // Only consider relevant windows
                uint style = WindowsAPI.GetWindowStyle(hWnd);
                bool isMatch = WindowsAPI.IsStandardWindowStyle(style);
                if (isMatch) 
                {
                    string titleBarText = WindowsAPI.GetTitleBarText(hWnd).ToLower();
                    switch(_matchType)
                    {
                        case EMatchType.StartsWith:
                            isMatch = titleBarText.StartsWith(_windowTitleLower);
                            break;
                        case EMatchType.EndsWith:
                            isMatch = titleBarText.EndsWith(_windowTitleLower);
                            break;
                        case EMatchType.Equals:
                            isMatch = titleBarText == _windowTitleLower;
                            break;
                        default:
                            isMatch = false;
                            break;
                    }

                    if (isMatch)
                    {
                        // Matched window
                        _matchedWindow = hWnd;
                    }
                    else
                    {
                        // Recurse through child windows
                        WindowsAPI.EnumChildWindows(hWnd, _enumWindowsCallback, IntPtr.Zero);
                    }
                }
            }
            catch (Exception)
            {
                // Ignore errors
            }

            // Continue unless matched
            return _matchedWindow == IntPtr.Zero;
        }
    }
}
