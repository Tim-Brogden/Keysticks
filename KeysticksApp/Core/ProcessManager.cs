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
using System.Diagnostics;
using Keysticks.Actions;

namespace Keysticks.Core
{
    /// <summary>
    /// Starts processes
    /// </summary>
    public static class ProcessManager
    {
        /// <summary>
        /// Starts a process
        /// </summary>
        /// <param name="action"></param>
        public static void Start(StartProgramAction action)
        {
            string fileTarget = action.GetTarget();

            ProcessStartInfo psi = null;
            if (fileTarget != "")
            {
                if (action.ProgramArgs != "")
                {
                    psi = new ProcessStartInfo(fileTarget, action.ProgramArgs);
                }
                else
                {
                    psi = new ProcessStartInfo(fileTarget);
                }
            }
            else if (action.ProgramArgs != "")
            {
                psi = new ProcessStartInfo(action.ProgramArgs);
            }

            if (psi != null)
            {
                Process.Start(psi);
            }
        }

        /// <summary>
        /// Start a process with an application or document path
        /// </summary>
        /// <param name="fileName"></param>
        public static void Start(string fileName)
        {
            Process.Start(fileName);
        }

        /// <summary>
        /// See whether a process is already running
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        public static bool IsRunning(string processName)
        {
            bool isRunning = false;
            if (processName != "")
            {
                // Convert to friendly name without extension if required
                if (processName.EndsWith(".exe"))
                {
                    processName = processName.Substring(0, processName.Length - 4);
                }

                Process[] processes = Process.GetProcessesByName(processName);
                if (processes != null && processes.Length != 0)
                {
                    isRunning = true;
                }
            }

            return isRunning;
        }
    }
}
