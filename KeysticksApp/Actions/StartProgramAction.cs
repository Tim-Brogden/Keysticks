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
using System.Text;
using System.Xml;
using System.IO;
using Keysticks.Core;
using Keysticks.Event;

namespace Keysticks.Actions
{
    /// <summary>
    /// Action for starting a program
    /// </summary>
    public class StartProgramAction : BaseAction
    {
        // Fields
        private string _programName = "";
        private string _programFolder = "";
        private string _programArgs = "";
        private bool _checkIfRunning = true;
        private bool _tryBothFolders = true;

        // Properties
        public string ProgramName { get { return _programName; } set { _programName = value; } }
        public string ProgramFolder { get { return _programFolder; } set { _programFolder = value; } }
        public string ProgramArgs { get { return _programArgs; } set { _programArgs = value; } }
        public bool CheckIfRunning { get { return _checkIfRunning; } set { _checkIfRunning = value; } }
        public bool TryBothFolders { get { return _tryBothFolders; } set { _tryBothFolders = value; } }

        /// <summary>
        /// Type of action
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.StartProgram; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public StartProgramAction()
            : base()
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="action"></param>
        public StartProgramAction(StartProgramAction action)
        {
            _programName = string.Copy(action._programName);
            _programFolder = string.Copy(action._programFolder);
            _programArgs = string.Copy(action._programArgs);
            _checkIfRunning = action._checkIfRunning;
            _tryBothFolders = action._tryBothFolders;
        }

        /// <summary>
        /// Name of the action
        /// </summary>
        /// <returns></returns>
        public override string Name
        {
            get 
            {
                string options;
                if (_checkIfRunning)
                {
                    options = string.Format(" ({0}{1})", Properties.Resources.String_check_if_running,                        
                        _tryBothFolders ? ", " + Properties.Resources.String_try_both_folders : "");
                }
                else
                {
                    options = _tryBothFolders ? " (" + Properties.Resources.String_try_both_folders + ")" : "";
                }

                return ShortName + options;
            }
        }

        /// <summary>
        /// Short name of the action
        /// </summary>
        /// <returns></returns>
        public override string ShortName
        {
            get
            {
                return Properties.Resources.String_Start + " " + GetCommandLine();
            }
        }

        /// <summary>
        /// Get the icon to use
        /// </summary>
        public override EAnnotationImage IconRef
        {
            get
            {
                return EAnnotationImage.StartProgram;
            }
        }

        /// <summary>
        /// Get the program or directory to execute
        /// </summary>
        /// <returns></returns>
        public string GetTarget()
        {
            string fileTarget = "";
            string programName = _programName;
            if (programName != "")
            {
                if (!programName.EndsWith(".exe"))
                {
                    programName += ".exe";
                }

                if (_programFolder != "")
                {
                    fileTarget = Path.Combine(_programFolder, programName);

                    // See if we need to switch between Program Files and Program Files (x86)
                    if (_tryBothFolders && _programFolder.Contains("Program Files") && !File.Exists(fileTarget))
                    {
                        // Create an alternative target using the other Program Files directory
                        string alternativeFolder;
                        if (_programFolder.Contains("Program Files (x86)"))
                        {
                            alternativeFolder = _programFolder.Replace("Program Files (x86)", "Program Files");
                        }
                        else
                        {
                            alternativeFolder = _programFolder.Replace("Program Files", "Program Files (x86)");
                        }
                        
                        string alternativeTarget = Path.Combine(alternativeFolder, programName);
                        if (File.Exists(alternativeTarget))
                        {
                            // Use the alternative target
                            fileTarget = alternativeTarget;
                        }
                    }                    
                }
                else
                {
                    fileTarget = programName;
                }
            }
            else
            {
                fileTarget = _programFolder;
            }

            return fileTarget;
        }

        /// <summary>
        /// Get the command line string to execute
        /// </summary>
        /// <returns></returns>
        public string GetCommandLine()
        {
            string fileTarget = GetTarget();

            StringBuilder sb = new StringBuilder();
            if (fileTarget != "")
            {
                if (!fileTarget.StartsWith("\"") && !fileTarget.EndsWith("\""))
                {
                    sb.Append('\"');
                    sb.Append(fileTarget);
                    sb.Append('\"');
                }
                else
                {
                    sb.Append(fileTarget);
                }

                if (_programArgs != "")
                {
                    sb.Append(' ');
                    sb.Append(_programArgs);
                }
            }
            else
            {
                sb.Append(_programArgs);
            }

            return sb.ToString();
        }
        
        /// <summary>
        /// Read from Xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _programName = element.GetAttribute("programname");
            _programFolder = element.GetAttribute("programfolder");
            _programArgs = element.GetAttribute("arguments");
            _checkIfRunning = bool.Parse(element.GetAttribute("checkifrunning"));
            _tryBothFolders = bool.Parse(element.GetAttribute("trybothfolders"));

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

            element.SetAttribute("programname", _programName);
            element.SetAttribute("programfolder", _programFolder);
            element.SetAttribute("arguments", _programArgs);
            element.SetAttribute("checkifrunning", _checkIfRunning.ToString());
            element.SetAttribute("trybothfolders", _tryBothFolders.ToString());
        }

        /// <summary>
        /// Start the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void Start(IStateManager parent, KxSourceEventArgs args)
        {
            parent.ThreadManager.SubmitUIEvent(new KxStartProgramEventArgs(new StartProgramAction(this)));

            IsOngoing = false;
        }
    }
}
