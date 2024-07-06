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
using Keysticks.Actions;
using Keysticks.Core;
using Keysticks.Sources;

namespace Keysticks.Config
{
    /// <summary>
    /// Creates meta data to summarise a profile
    /// </summary>
    public class ProfileSummariser
    {
        // Constants
        private const int _maxListLen = 5;

        // Fields
        private Profile _profile;
        private StringUtils _utils = new StringUtils();
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="profile"></param>
        public ProfileSummariser(Profile profile)
        {
            _profile = profile;
        }

        /// <summary>
        /// Recalculate meta data for profile
        /// </summary>
        /// <remarks>Not applicable to meta data only profiles without sources</remarks>
        public void UpdateMetaData()
        {
            if (_profile.VirtualSources.Count != 0)
            {
                // Number of players
                int numPlayers = _profile.VirtualSources.Count;
                _profile.MetaData.SetIntVal(EMetaDataItem.NumPlayers.ToString(), numPlayers);

                // Control sets
                string summary = CreateControlSetSummmary();
                _profile.MetaData.SetStringVal(EMetaDataItem.ControlSets.ToString(), summary);

                // Keyboard types
                summary = CreateKeyboardTypesSummary();
                _profile.MetaData.SetStringVal(EMetaDataItem.KeyboardTypes.ToString(), summary);

                // Start program actions
                summary = CreateProgramActionsSummary();
                _profile.MetaData.SetStringVal(EMetaDataItem.ProgramActions.ToString(), summary);

                // Auto-activations
                summary = CreateAutoActivationSummary();
                _profile.MetaData.SetStringVal(EMetaDataItem.AutoActivations.ToString(), summary);

                _profile.IsModified = true;
            }
        }

        /// <summary>
        /// Summarise control sets in profile
        /// </summary>
        /// <returns></returns>
        private string CreateControlSetSummmary()
        {
            List<string> nameList = new List<string>();
            foreach (BaseSource source in _profile.VirtualSources)
            {
                foreach (AxisValue controlSet in source.StateTree.SubValues)
                {
                    if (controlSet.ID != Constants.DefaultID && !nameList.Contains(controlSet.Name))
                    {
                        nameList.Add(controlSet.Name);
                    }
                }
            }

            return SummariseStringList(nameList, Properties.Resources.String_None);
        }

        /// <summary>
        /// Summarise keyboard types in profile
        /// </summary>
        /// <returns></returns>
        private string CreateKeyboardTypesSummary()
        {
            List<string> nameList = new List<string>();
            foreach (BaseSource source in _profile.VirtualSources)
            {
                foreach (AxisValue controlSet in source.StateTree.SubValues)
                {
                    if (controlSet.GridType != EGridType.None)
                    {
                        string name = _utils.GridTypeToString(controlSet.GridType);
                        if (!nameList.Contains(name))
                        {
                            nameList.Add(name);
                        }
                    }
                }
            }

            return SummariseStringList(nameList, Properties.Resources.String_None);
        }

        /// <summary>
        /// Summarise start program actions in profile
        /// </summary>
        /// <returns></returns>
        private string CreateProgramActionsSummary()
        {
            List<string> nameList = new List<string>();
            foreach (BaseSource source in _profile.VirtualSources)
            {
                List<BaseAction> programActionsList = source.Actions.GetActionsOfType(EActionType.StartProgram);
                foreach (StartProgramAction action in programActionsList)
                {
                    string summary = action.ProgramName != "" ? action.ProgramName : action.GetCommandLine();
                    if (!nameList.Contains(summary))
                    {
                        nameList.Add(summary);
                    }
                }
            }

            return SummariseStringList(nameList, Properties.Resources.String_None);
        }

        /// <summary>
        /// Summarise auto-activations in profile
        /// </summary>
        /// <returns></returns>
        private string CreateAutoActivationSummary()
        {
            List<string> nameList = new List<string>();
            foreach (BaseSource source in _profile.VirtualSources)
            {
                foreach (AutoActivation activation in source.AutoActivations)
                {
                    string name = activation.Name;
                    if (!nameList.Contains(name))
                    {
                        nameList.Add(name);
                    }
                }
            }

            return SummariseStringList(nameList, Properties.Resources.String_None);
        }
        
        private string SummariseStringList(List<string> nameList, string textIfEmpty)
        {
            string summary;
            if (nameList.Count != 0)
            {
                StringBuilder sb = new StringBuilder();
                int count = 0;
                foreach (string name in nameList)
                {
                    if (count != 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(name);
                    if (++count == _maxListLen)
                    {
                        break;
                    }
                }
                
                summary = sb.ToString();                
            }
            else
            {
                summary = textIfEmpty;
            }
            
            return summary;
        }
    }
}
