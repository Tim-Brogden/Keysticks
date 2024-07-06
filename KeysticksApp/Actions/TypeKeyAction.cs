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
using System.Windows.Forms;
using System.Xml;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Sys;
namespace Keysticks.Actions
{
    /// <summary>
    /// Action for typing a keyboard key or key combination
    /// </summary>
    public class TypeKeyAction : BaseKeyAction
    {
        // Configuration
        private bool _isWinModifier = false;
        private bool _isAltModifier = false;
        private bool _isControlModifier = false;
        private bool _isShiftModifier = false;
        private EModifierKeyStates _modifierKeys = EModifierKeyStates.None;

        // State
        private long _lastPressTimeTicks;
        //private bool _ignoreModifiers;

        public bool IsWinModifierSet { get { return _isWinModifier; } set { _isWinModifier = value; } }
        public bool IsAltModifierSet { get { return _isAltModifier; } set { _isAltModifier = value; } }
        public bool IsControlModifierSet { get { return _isControlModifier; } set { _isControlModifier = value; } }
        public bool IsShiftModifierSet { get { return _isShiftModifier; } set { _isShiftModifier = value; } }
        protected EModifierKeyStates ModifierKeys { get { return _modifierKeys; } }

        /// <summary>
        /// Return what type of action this is
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.TypeKey; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public TypeKeyAction()
            :base()
        {
        }
        
        /// <summary>
        /// Return the name of the action
        /// </summary>
        /// <returns></returns>
        public override string Name
        {
            get
            {
                string name = IsVirtualKey ? string.Format("[{0}]", base.Name) : base.Name;
                if (_modifierKeys != EModifierKeyStates.None)
                {
                    name = string.Format(Properties.Resources.String_Type + " {0}{1}{2}{3}{4}",
                                            _isWinModifier ? Properties.Resources.String_Win + "+" : "",
                                            _isAltModifier ? Properties.Resources.String_Alt + "+" : "",
                                            _isControlModifier ? Properties.Resources.String_Ctrl + "+" : "",
                                            _isShiftModifier ? Properties.Resources.String_Shift + "+" : "",
                                            name);
                }
                else
                {
                    name = Properties.Resources.String_Type + " " + name;
                }

                return name;
            }
        }

        /// <summary>
        /// Return a tiny name for the key
        /// </summary>
        public override string TinyName
        {
            get
            {
                string tinyName;
                if (KeyData != null)
                {
                    if (_modifierKeys == EModifierKeyStates.None)
                    {
                        // No modifiers
                        tinyName = base.TinyName;
                    }
                    else if (KeyData.OemKeyCombinations != null && KeyData.OemKeyCombinations.ContainsKey(_modifierKeys))
                    {
                        // Recognised key combination
                        tinyName = KeyData.OemKeyCombinations[_modifierKeys];
                    }
                    else
                    {
                        // Unrecognised key combination
                        tinyName = string.Format("{0}{1}{2}{3}{4}",
                                            (_modifierKeys & EModifierKeyStates.AnyWinKey) != 0 ? "#" : "",
                                            (_modifierKeys & EModifierKeyStates.AnyMenuKey) != 0 ? "%" : "",
                                            (_modifierKeys & EModifierKeyStates.AnyControlKey) != 0 ? "^" : "",
                                            (_modifierKeys & EModifierKeyStates.AnyShiftKey) != 0 ? "+" : "",
                                            KeyData.TinyName);
                    }
                }
                else
                {
                    tinyName = Properties.Resources.String_NotKnownAbbrev;
                } 
                
                return tinyName;
            }
        }

        public override void Initialise(IKeyboardContext context)
        {
            base.Initialise(context);

            // Store an equivalent modifier value to represent this action
            _modifierKeys = EModifierKeyStates.None;
            if (_isShiftModifier) _modifierKeys |= EModifierKeyStates.ShiftKey;
            if (_isControlModifier) _modifierKeys |= EModifierKeyStates.ControlKey;
            if (_isAltModifier) _modifierKeys |= EModifierKeyStates.Menu;
            if (_isWinModifier) _modifierKeys |= EModifierKeyStates.LWin;
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _isWinModifier = bool.Parse(element.GetAttribute("win"));
            _isAltModifier = bool.Parse(element.GetAttribute("alt"));
            _isControlModifier = bool.Parse(element.GetAttribute("control"));
            _isShiftModifier = bool.Parse(element.GetAttribute("shift"));

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("win", _isWinModifier.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("alt", _isAltModifier.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("control", _isControlModifier.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("shift", _isShiftModifier.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Start the action
        /// </summary>
        public override void Start(IStateManager parent, KxSourceEventArgs args)
        {
            // If there isn't an ongoing task for this key, start this task
            KeyPressManager keyStateManager = parent.KeyStateManager;

            // Don't apply modifiers if one of the modifier keys is already held
            // The main reason for this is to allow the user to select 
            // Ctrl or Alt followed by a letter on uppercase keyboard pages (e.g. Alt+F)
            // and the behaviour should not include the Shift modifier i.e. don't perform Alt+Shift+F
            //_ignoreModifiers = keyStateManager.ModifierKeyStates != EModifierKeyStates.None;
            // *** Keysticks v2.10 - This logic has been removed. It's better to let the user decide. ***
            // Press down modifiers
            //if (!_ignoreModifiers)
            //{
                if (_isWinModifier) keyStateManager.SetVirtualKeyState(Keys.LWin, true);
                if (_isAltModifier) keyStateManager.SetVirtualKeyState(Keys.Menu, true);
                if (_isControlModifier) keyStateManager.SetVirtualKeyState(Keys.ControlKey, true);
                if (_isShiftModifier) keyStateManager.SetVirtualKeyState(Keys.ShiftKey, true);
            //}

            // Press key
            keyStateManager.SetKeyState(KeyData, true, IsVirtualKey);            

            // Record time of press
            _lastPressTimeTicks = DateTime.UtcNow.Ticks;

            IsOngoing = true;
        }

        /// <summary>
        /// Continue the action
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public override void Continue(IStateManager parent, KxSourceEventArgs args)
        {
            // If the key has been released, release any modifiers
            if (DateTime.UtcNow.Ticks - _lastPressTimeTicks > parent.KeyStateManager.KeyStrokeLengthTicks)
            {
                // Complete the action
                ReleaseKeys(parent);
            }
        }

        /// <summary>
        /// Stop the action
        /// </summary>
        /// <param name="parent"></param>
        public override void Deactivate(IStateManager parent, KxSourceEventArgs args)
        {
            if (IsOngoing)
            {
                ReleaseKeys(parent);
            }       
        }

        /// <summary>
        /// Release held keys
        /// </summary>
        /// <param name="parent"></param>
        private void ReleaseKeys(IStateManager parent)
        {
            // Release key
            KeyPressManager keyStateManager = parent.KeyStateManager;
            keyStateManager.SetKeyState(KeyData, false, IsVirtualKey);

            // Release modifier keys
            //if (!_ignoreModifiers)
            //{
                if (_isWinModifier) keyStateManager.SetVirtualKeyState(Keys.LWin, false);
                if (_isAltModifier) keyStateManager.SetVirtualKeyState(Keys.Menu, false);
                if (_isControlModifier) keyStateManager.SetVirtualKeyState(Keys.ControlKey, false);
                if (_isShiftModifier) keyStateManager.SetVirtualKeyState(Keys.ShiftKey, false);
            //}

            // Action stopped
            IsOngoing = false;
        }
    }
}
