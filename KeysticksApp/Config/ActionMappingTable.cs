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
using System.Collections.Generic;

namespace Keysticks.Config
{
    /// <summary>
    /// Define the lists of actions to perform for each type of event
    /// </summary>
    public class ActionMappingTable
    {
        // Fields
        private StateVector _state;
        private List<ActionMappingTable> _parentTablesList;
        private Dictionary<int, ActionSet> _inputMappings = new Dictionary<int, ActionSet>();

        // Properties
        public StateVector State { get { return _state; } }
        public List<ActionMappingTable> ParentTablesList { get { return _parentTablesList; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public ActionMappingTable(StateVector state)
        {
            _state = state;
        }

        /// <summary>
        /// Configure actions for a particular control
        /// </summary>
        /// <param name="inputID"></param>
        /// <param name="actionSet"></param>
        public void SetActions(int inputID, ActionSet actionSet)
        {
            // Actions for this table
            _inputMappings[inputID] = actionSet;
        }

        /// <summary>
        /// Get the set of actions to perform for a particular control
        /// </summary>
        /// <param name="inputID"></param>
        /// <returns></returns>
        public ActionSet GetActions(int inputID, bool includeDefaults)
        {
            // Look for actions for this table's situation
            ActionSet actionSet = null;
            if (_inputMappings.ContainsKey(inputID))
            {
                actionSet = _inputMappings[inputID];
            }
            
            if (includeDefaults && actionSet == null && _parentTablesList != null)
            {
                // If no actions where found, look one level up
                foreach (ActionMappingTable parentTable in _parentTablesList)
                {
                    actionSet = parentTable.GetActions(inputID, false);
                    if (actionSet != null)
                    {
                        break;
                    }
                }

                // If still no actions where found, look at all levels
                if (actionSet == null)
                {
                    foreach (ActionMappingTable parentTable in _parentTablesList)
                    {
                        actionSet = parentTable.GetActions(inputID, true);
                        if (actionSet != null)
                        {
                            break;
                        }
                    }
                }
            }

            return actionSet;
        }               

        /// <summary>
        /// Set the action mappings for the parent situation
        /// </summary>
        /// <param name="parentTable"></param>
        public void AddParentTable(ActionMappingTable parentTable)
        {
            if (_parentTablesList == null)
            {
                _parentTablesList = new List<ActionMappingTable>();
            }
            _parentTablesList.Add(parentTable);
        }

        /// <summary>
        /// Return an enumerator for looping through action sets
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, ActionSet>.Enumerator GetEnumerator()
        {
            return _inputMappings.GetEnumerator();
        }    
    }
}
