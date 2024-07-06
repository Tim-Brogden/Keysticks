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
using System.Text.RegularExpressions;
using System.Xml;
using Keysticks.Core;
using Keysticks.Sources;

namespace Keysticks.Config
{
    /// <summary>
    /// Represents the control set hierarchy of an input source
    /// </summary>
    public class StateCollection : AxisValue
    {
        // Fields
        private BaseSource _parent;

        // Properties
        public int NumModes { get { return SubValues.Count; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent"></param>
        public StateCollection(BaseSource parent)
            :base(Constants.DefaultID, Properties.Resources.String_All, StateVector.GetRootSituation())
        {
            _parent = parent;
        }

        /// <summary>
        /// Get the state to start in when the profile is loaded
        /// </summary>
        /// <returns></returns>
        public StateVector GetInitialState()
        {
            StateVector initialState = null;
            foreach (AxisValue modeItem in SubValues)
            {
                initialState = modeItem.Situation;
                if (modeItem.ID != Constants.DefaultID)
                {
                    break;
                }
            }
            if (initialState == null)
            {
                initialState = StateVector.GetRootSituation();
            }

            return initialState;
        }

        /// <summary>
        /// Get a state from the tree
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool HasState(StateVector state)
        {
            bool matched = true;
            AxisValue item = GetMode(state.ModeID);
            if (item == null)
            {
                matched = false;
            }
            else if (state.PageID != Constants.DefaultID || state.CellID != Constants.DefaultID)
            {
                item = (AxisValue)item.SubValues.GetItemByID(state.PageID);
                if (item == null)
                {
                    matched = false;
                }
                else if (state.CellID != Constants.DefaultID)
                {
                    item = (AxisValue)item.SubValues.GetItemByID(state.CellID);
                    if (item == null)
                    {
                        matched = false;
                    }
                }
            }

            return matched;
        }

        /// <summary>
        /// Get a mode
        /// </summary>
        /// <param name="modeID"></param>
        /// <returns></returns>
        public AxisValue GetMode(int modeID)
        {
            return (AxisValue)SubValues.GetItemByID(modeID);
        }

        /// <summary>
        /// Validate the state collection
        /// </summary>
        public void Validate()
        {
            // Delete modes which reference invalid controls
            bool deleted = false;
            int i = 0;
            while (i < SubValues.Count)
            {
                bool deleteMode = false;
                AxisValue modeItem = (AxisValue)SubValues[i];
                if (modeItem.Controls != null)
                {
                    GeneralisedControl modeControl = modeItem.Controls.NavigationControl;
                    if (modeControl != null && 
                        modeControl.ReferenceControl != null && 
                        _parent.GetVirtualControl(modeControl.ReferenceControl) == null)
                    {
                        deleteMode = true;
                    }

                    if (!deleteMode)
                    {
                        modeControl = modeItem.Controls.SelectionControl;
                        if (modeControl != null &&
                            modeControl.ReferenceControl != null && 
                            _parent.GetVirtualControl(modeControl.ReferenceControl) == null)
                        {
                            deleteMode = true;
                        }
                    }
                }

                if (deleteMode && SubValues.Remove(modeItem))
                {
                    _parent.IsModified = true;
                    deleted = true;
                }
                else
                {
                    i++;
                }
            }

            if (deleted)
            {
                // Validate actions which may reference deleted situations
                _parent.Actions.Validate();
            }
        }

        /// <summary>
        /// Replace key name place holders like [A] with their language-specific names, based upon virtual key codes
        /// </summary>
        public void SetKeyboardSpecificVirtualKeyNames(IKeyboardContext context)
        {
            Regex keyNameRegex = new Regex(@"\[[^]]+\]");
            VirtualKeyData[] virtualKeysByKeyCode = KeyUtils.GetVirtualKeysByKeyCode(context.KeyboardHKL);
            StringBuilder sb = new StringBuilder();
            foreach (AxisValue modeItem in SubValues)
            {
                if (keyNameRegex.IsMatch(modeItem.Name))
                {
                    // Mode has language-specific name
                    sb.Clear();
                    string currentName = modeItem.Name;
                    MatchCollection matches = keyNameRegex.Matches(currentName);
                    int index = 0;
                    foreach (Match match in matches)
                    {
                        if (match.Index > index)
                        {
                            // Duplicate the portions of the name between matches
                            sb.Append(currentName.Substring(index, match.Index - index));
                        }

                        // Replace the matching portion with the language-specific name
                        string keyCodeName = currentName.Substring(match.Index + 1, match.Length - 2);
                        System.Windows.Forms.Keys keyCode;
                        bool keyFound = false;
                        if (Enum.TryParse<System.Windows.Forms.Keys>(keyCodeName, out keyCode))
                        {
                            VirtualKeyData vk = virtualKeysByKeyCode[(byte)keyCode];
                            if (vk != null)
                            {
                                sb.Append(vk.Name);
                                keyFound = true;
                            }
                        }

                        // If the key name wasn't found, keep the same name
                        if (!keyFound)
                        {
                            sb.Append(match.Value);
                        }

                        index = match.Index + match.Length;
                    }

                    // Duplicate the end portion of the name
                    if (index < currentName.Length)
                    {
                        sb.Append(currentName.Substring(index));
                    }

                    // Update the mode's name
                    modeItem.Name = sb.ToString();
                }
            }
        }

        /// <summary>
        /// Replace key name place holders like {A} with their language-specific names, based upon scan codes
        /// </summary>
        public void SetKeyboardSpecificKeyNames(IKeyboardContext context)
        {
            Regex keyNameRegex = new Regex(@"\{.\}");
            Dictionary<char, ushort> defaultScanCodes = KeyUtils.GetDefaultAlphanumScanCodes();
            Dictionary<ushort, VirtualKeyData> virtualKeysByScanCode = KeyUtils.GetVirtualKeysByScanCode(context.KeyboardHKL);
            StringBuilder sb = new StringBuilder();
            foreach (AxisValue modeItem in SubValues)
            {
                if (keyNameRegex.IsMatch(modeItem.Name))
                {
                    // Mode has language-specific name
                    sb.Clear();
                    string currentName = modeItem.Name;
                    MatchCollection matches = keyNameRegex.Matches(currentName);
                    int index = 0;
                    foreach (Match match in matches)
                    {
                        if (match.Index > index)
                        {
                            // Duplicate the portions of the name between matches
                            sb.Append(currentName.Substring(index, match.Index));
                        }

                        // Replace the matching portion with the language-specific name
                        char charToReplace = currentName[match.Index + 1];
                        bool isLower = char.IsLower(charToReplace);
                        char charToReplaceLower = char.ToLower(charToReplace);
                        bool keyFound = false;
                        if (defaultScanCodes.ContainsKey(charToReplaceLower))
                        {
                            ushort scanCode = defaultScanCodes[charToReplaceLower];
                            if (virtualKeysByScanCode.ContainsKey(scanCode))
                            {
                                VirtualKeyData vk = virtualKeysByScanCode[scanCode];
                                if (vk != null)
                                {
                                    sb.Append(isLower ? vk.Name.ToLower() : vk.Name);
                                    keyFound = true;
                                }
                            }
                        }

                        // If the key name wasn't found, keep the same name
                        if (!keyFound)
                        {
                            sb.Append(charToReplace);
                        }

                        index = match.Index + match.Length;
                    }

                    // Duplicate the end portion of the name
                    if (index < currentName.Length)
                    {
                        sb.Append(currentName.Substring(index));
                    }

                    // Update the mode's name
                    modeItem.Name = sb.ToString();
                }
            }
        }
    }
}
