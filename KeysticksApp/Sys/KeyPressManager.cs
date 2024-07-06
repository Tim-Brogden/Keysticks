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
using System.Windows.Forms;
using Keysticks.Core;
using Keysticks.Event;

namespace Keysticks.Sys
{
    /// <summary>
    /// Manages keyboard state
    /// </summary>
    public class KeyPressManager
    {
        private bool[] _isKeyPressed = new bool[256];
        private bool[] _isExtendedKey = new bool[256];
        private VirtualKeyData[] _virtualKeyData;
        private EModifierKeyStates _modifierKeyStates = EModifierKeyStates.None;
        private EToggleKeyStates _toggleKeyStates = EToggleKeyStates.None;
        private IStateManager _parent;
        private bool _useScanCodes = Constants.DefaultUseScanCodes;
        private long _keyStrokeLengthTicks = Constants.DefaultKeyStrokeLengthMS * TimeSpan.TicksPerMillisecond;
        private bool _disallowShiftDelete = Constants.DefaultDisallowShiftDelete;

        // Properties
        public EModifierKeyStates ModifierKeyStates { get { return _modifierKeyStates; } }
        public EToggleKeyStates ToggleKeyStates { get { return _toggleKeyStates; } }
        public bool UseScanCodes { get { return _useScanCodes; } set { _useScanCodes = value; } }
        public long KeyStrokeLengthTicks { get { return _keyStrokeLengthTicks; } set { _keyStrokeLengthTicks = value; } }
        public bool DisallowShiftDelete { get { return _disallowShiftDelete; } set { _disallowShiftDelete = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public KeyPressManager(IStateManager parent)
        {
            _parent = parent;

            // Initialise extended keys array
            InitialiseExtendedKeysList();

            // Initialise and report the keyboard state
            KeyboardLayoutChanged(parent.KeyboardContext);
            //InitialiseToggleKeyStates();
            //InitialiseModifierKeyStates();
            //_parent.SubmitUIEvent(new KxKeyboardStateEventArgs(_modifierKeyStates, _toggleKeyStates));
        }

        /// <summary>
        /// Initialise or re-initialise the keyboard layout
        /// </summary>
        public void KeyboardLayoutChanged(IKeyboardContext context)
        {
            _virtualKeyData = KeyUtils.GetVirtualKeysByKeyCode(context.KeyboardHKL);
        }

        /// <summary>
        /// Initialise the state of the modifier keys
        /// </summary>
        //private void InitialiseModifierKeyStates()
        //{
        //    if (IsKeyDown(System.Windows.Forms.Keys.LShiftKey)) _modifierKeyStates |= EModifierKeyStates.LShiftKey;
        //    if (IsKeyDown(System.Windows.Forms.Keys.RShiftKey)) _modifierKeyStates |= EModifierKeyStates.RShiftKey;
        //    if (IsKeyDown(System.Windows.Forms.Keys.ShiftKey)) _modifierKeyStates |= EModifierKeyStates.ShiftKey;
        //    if (IsKeyDown(System.Windows.Forms.Keys.LControlKey)) _modifierKeyStates |= EModifierKeyStates.LControlKey;
        //    if (IsKeyDown(System.Windows.Forms.Keys.RControlKey)) _modifierKeyStates |= EModifierKeyStates.RControlKey;
        //    if (IsKeyDown(System.Windows.Forms.Keys.ControlKey)) _modifierKeyStates |= EModifierKeyStates.ControlKey;
        //    if (IsKeyDown(System.Windows.Forms.Keys.LMenu)) _modifierKeyStates |= EModifierKeyStates.LMenu;
        //    if (IsKeyDown(System.Windows.Forms.Keys.RMenu)) _modifierKeyStates |= EModifierKeyStates.RMenu;
        //    if (IsKeyDown(System.Windows.Forms.Keys.Menu)) _modifierKeyStates |= EModifierKeyStates.Menu;
        //    if (IsKeyDown(System.Windows.Forms.Keys.LWin)) _modifierKeyStates |= EModifierKeyStates.LWin;
        //    if (IsKeyDown(System.Windows.Forms.Keys.RWin)) _modifierKeyStates |= EModifierKeyStates.RWin;
        //}

        /// <summary>
        /// Initialise the state of the toggle keys
        /// </summary>
        //private void InitialiseToggleKeyStates()
        //{
        //    if (IsKeyToggled(System.Windows.Forms.Keys.CapsLock)) _toggleKeyStates |= EToggleKeyStates.CapsLock;
        //    if (IsKeyToggled(System.Windows.Forms.Keys.NumLock)) _toggleKeyStates |= EToggleKeyStates.NumLock;
        //    if (IsKeyToggled(System.Windows.Forms.Keys.Scroll)) _toggleKeyStates |= EToggleKeyStates.Scroll;
        //    if (IsKeyToggled(System.Windows.Forms.Keys.Insert)) _toggleKeyStates |= EToggleKeyStates.Insert;
        //}

        /// <summary>
        /// Release any keyboard keys that are held
        /// </summary>
        public void ReleaseAll()
        {
            // Release any keys we pressed
            for (int i=0; i<256; i++)
            {
                if (_isKeyPressed[i])
                {
                    SetKeyState(_virtualKeyData[i], false, true);
                }
            }
        }        

        /// <summary>
        /// Emulate a key down or up using Windows API
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyDown"></param>
        public void SetVirtualKeyState(Keys key, bool keyDown)
        {
            SetKeyState(_virtualKeyData[(byte)key], keyDown, true);
        }

        /// <summary>
        /// Emulate a key down or up using Windows API
        /// </summary>
        /// <param name="keyCode"></param>
        public void SetKeyState(VirtualKeyData vk, bool keyDown, bool isVirtualKey)
        {
            // Don't release key if it's already released
            if (vk != null && (keyDown || _isKeyPressed[(byte)vk.KeyCode]))
            {
                // Block Shift+Delete if required
                if (vk.KeyCode == Keys.Delete && keyDown && _disallowShiftDelete && 
                    (_modifierKeyStates & EModifierKeyStates.AnyShiftKey) != 0)
                {
                    _parent.ThreadManager.SubmitUIEvent(new KxErrorMessageEventArgs(Properties.Resources.E_ShiftDeleteDisabled, null));
                }
                else
                {
                    INPUT[] inputs = new INPUT[1];
                    inputs[0].type = WindowsAPI.INPUT_KEYBOARD;
                    inputs[0].ki.dwFlags = keyDown ? 0u : WindowsAPI.KEYEVENTF_KEYUP;
                    if (vk.WindowsScanCode != 0 && !isVirtualKey && _useScanCodes)
                    {
                        // Use scan code
                        inputs[0].ki.dwFlags |= WindowsAPI.KEYEVENTF_SCANCODE;
                        inputs[0].ki.wScan = vk.WindowsScanCode;

                        // Extended key flag
                        if (_isExtendedKey[(byte)vk.KeyCode])
                        {
                            inputs[0].ki.dwFlags |= WindowsAPI.KEYEVENTF_EXTENDEDKEY;
                        }
                    }
                    else
                    {
                        // Use virtual key code (scan code zero indicates vk must be used)
                        inputs[0].ki.wVk = (ushort)vk.KeyCode;
                    }

                    WindowsAPI.SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));

                    // Let the state manager handle the event immediately
                    _parent.HandleKeyEvent(new KxKeyEventArgs(vk.KeyCode, keyDown ? EEventReason.Pressed : EEventReason.Released, false));

                    // Store the key state so that we know which keys we pressed
                    _isKeyPressed[(byte)vk.KeyCode] = keyDown;
                }
            }
        }

        /// <summary>
        /// Toggle the specified key
        /// </summary>
        /// <param name="key"></param>
        public void ToggleKeyState(VirtualKeyData vk, bool isVirtualKey)
        {
            if (vk != null)
            {
                SetKeyState(vk, !_isKeyPressed[(byte)vk.KeyCode], isVirtualKey);
            }
        }

        /// <summary>
        /// Send a repeated press and release of a key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="count"></param>
        public void RepeatVirtualKey(Keys key, uint count)
        {
            INPUT[] inputs = new INPUT[count << 1];
            byte keyCode = (byte)key;
            VirtualKeyData vk = _virtualKeyData[keyCode];
            if (vk != null)
            {
                if (vk.KeyCode == Keys.Delete &&
                    _disallowShiftDelete &&
                    (_modifierKeyStates & EModifierKeyStates.AnyShiftKey) != 0)
                {
                    _parent.ThreadManager.SubmitUIEvent(new KxErrorMessageEventArgs(Properties.Resources.E_ShiftDeleteDisabled, null));
                }
                else
                {
                    for (uint i = 0; i < inputs.Length; i++)
                    {
                        inputs[i].type = WindowsAPI.INPUT_KEYBOARD;
                        inputs[i].ki.wVk = keyCode;
                        if ((i & 1) != 0)
                        {
                            // Odd numbers are release events
                            inputs[i].ki.dwFlags |= WindowsAPI.KEYEVENTF_KEYUP;
                        }                        
                    }

                    WindowsAPI.SendInput((uint)inputs.Length, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));

                    // Store the key state so that we know which keys we pressed
                    _isKeyPressed[keyCode] = false;
                }
            }      
        }

        /// <summary>
        /// Emulate typing a string, which can include unicode characters
        /// </summary>
        /// <param name="textToType"></param>
        public void TypeString(string textToType)
        {
            INPUT[] inputs = new INPUT[2 * textToType.Length];
            int i = 0;
            foreach (char ch in textToType)
            {
                inputs[i].type = WindowsAPI.INPUT_KEYBOARD;
                if (ch == '\n')
                {
                    inputs[i].ki.wVk = (ushort)Keys.Return;
                }
                else if (ch != '\r')
                {
                    inputs[i].ki.dwFlags = WindowsAPI.KEYEVENTF_UNICODE;
                    inputs[i].ki.wScan = ch;
                }
                i++;
                inputs[i].type = WindowsAPI.INPUT_KEYBOARD;
                if (ch == '\n')
                {
                    inputs[i].ki.dwFlags = WindowsAPI.KEYEVENTF_KEYUP;
                    inputs[i].ki.wVk = (ushort)Keys.Return;
                }
                else if (ch != '\r')
                {
                    inputs[i].ki.dwFlags = WindowsAPI.KEYEVENTF_UNICODE | WindowsAPI.KEYEVENTF_KEYUP;
                    inputs[i].ki.wScan = ch;
                }
                i++;
            }
            WindowsAPI.SendInput((uint)inputs.Length, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        }

        /// <summary>
        /// Check whether a key is pressed or not
        /// </summary>
        /// <param name="key"></param>
        public bool IsKeyPressed(Keys key)
        {
            return _isKeyPressed[(byte)key];
        }

        /// <summary>
        /// Check whether the keyboard is in upper case mode
        /// </summary>
        /// <returns></returns>
        //public bool IsShiftHeldOrCapsOn()
        //{
        //    return (0 != (_toggleKeyStates & EToggleKeyStates.CapsLock)) ||
        //            (0 != (_modifierKeyStates & EModifierKeyStates.AnyShiftKey));
        //}

        /// <summary>
        /// Update the state of a key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isDown"></param>
        public void UpdateKeyState(KxKeyEventArgs args)
        {
            _isKeyPressed[(byte)args.Key] = args.EventReason == EEventReason.Pressed;

            if (args.EventReason == EEventReason.Pressed)
            {
                // See if it's a modifier key
                HandleModifierKeyDown(args.Key);
            }
            else
            {
                // Keep track of modifier key states
                HandleModifierKeyUp(args.Key);
            }            
        }

        /// <summary>
        /// Update the state of the modifier and toggle keys
        /// </summary>
        /// <param name="args"></param>
        //public void UpdateKeyboardState(KxKeyboardStateEventArgs args)
        //{
        //    _modifierKeyStates = args.ModifierState;
        //    _toggleKeyStates = args.ToggleState;
        //}

        /// <summary>
        /// Return whether a key is pressed or not
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //private bool IsKeyDown(System.Windows.Forms.Keys key)
        //{
        //    return (0 != (WindowsAPI.GetKeyState((int)key) & 0x8000));
        //}

        /// <summary>
        /// Return whether a key is toggled or not
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //private bool IsKeyToggled(System.Windows.Forms.Keys key)
        //{
        //    return (0 != (WindowsAPI.GetKeyState((int)key) & 0x1));
        //}

        /// <summary>
        /// Keep track of modifier key states
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        private bool HandleModifierKeyDown(System.Windows.Forms.Keys keyCode)
        {
            bool isModifier = true;
            switch (keyCode)
            {
                case System.Windows.Forms.Keys.LShiftKey:
                    _modifierKeyStates |= EModifierKeyStates.LShiftKey;
                    break;
                case System.Windows.Forms.Keys.RShiftKey:
                    _modifierKeyStates |= EModifierKeyStates.RShiftKey;
                    break;
                case System.Windows.Forms.Keys.ShiftKey:
                    _modifierKeyStates |= EModifierKeyStates.ShiftKey;
                    break;
                case System.Windows.Forms.Keys.LControlKey:
                    _modifierKeyStates |= EModifierKeyStates.LControlKey;
                    break;
                case System.Windows.Forms.Keys.RControlKey:
                    _modifierKeyStates |= EModifierKeyStates.RControlKey;
                    break;
                case System.Windows.Forms.Keys.ControlKey:
                    _modifierKeyStates |= EModifierKeyStates.ControlKey;
                    break;
                case System.Windows.Forms.Keys.LMenu:
                    _modifierKeyStates |= EModifierKeyStates.LMenu;
                    break;
                case System.Windows.Forms.Keys.RMenu:
                    _modifierKeyStates |= EModifierKeyStates.RMenu;
                    break;
                case System.Windows.Forms.Keys.Menu:
                    _modifierKeyStates |= EModifierKeyStates.Menu;
                    break;
                case System.Windows.Forms.Keys.LWin:
                    _modifierKeyStates |= EModifierKeyStates.LWin;
                    break;
                case System.Windows.Forms.Keys.RWin:
                    _modifierKeyStates |= EModifierKeyStates.RWin;
                    break;
                default:
                    isModifier = false;
                    break;
            }

            if (isModifier)
            {
                // Report keyboard state to UI
                _parent.ThreadManager.SubmitUIEvent(new KxKeyboardStateEventArgs(_modifierKeyStates, _toggleKeyStates));
            }


            return isModifier;
        }

        /// <summary>
        /// Keep track of modifier key states
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        private bool HandleModifierKeyUp(System.Windows.Forms.Keys keyCode)
        {
            bool isModifier = true;
            switch (keyCode)
            {
                case System.Windows.Forms.Keys.LShiftKey:
                    _modifierKeyStates &= EModifierKeyStates.All ^ EModifierKeyStates.LShiftKey;
                    break;
                case System.Windows.Forms.Keys.RShiftKey:
                    _modifierKeyStates &= EModifierKeyStates.All ^ EModifierKeyStates.RShiftKey;
                    break;
                case System.Windows.Forms.Keys.ShiftKey:
                    _modifierKeyStates &= EModifierKeyStates.All ^ EModifierKeyStates.ShiftKey;
                    break;
                case System.Windows.Forms.Keys.LControlKey:
                    _modifierKeyStates &= EModifierKeyStates.All ^ EModifierKeyStates.LControlKey;
                    break;
                case System.Windows.Forms.Keys.RControlKey:
                    _modifierKeyStates &= EModifierKeyStates.All ^ EModifierKeyStates.RControlKey;
                    break;
                case System.Windows.Forms.Keys.ControlKey:
                    _modifierKeyStates &= EModifierKeyStates.All ^ EModifierKeyStates.ControlKey;
                    break;
                case System.Windows.Forms.Keys.LMenu:
                    _modifierKeyStates &= EModifierKeyStates.All ^ EModifierKeyStates.LMenu;
                    break;
                case System.Windows.Forms.Keys.RMenu:
                    _modifierKeyStates &= EModifierKeyStates.All ^ EModifierKeyStates.RMenu;
                    break;
                case System.Windows.Forms.Keys.Menu:
                    _modifierKeyStates &= EModifierKeyStates.All ^ EModifierKeyStates.Menu;
                    break;
                case System.Windows.Forms.Keys.LWin:
                    _modifierKeyStates &= EModifierKeyStates.All ^ EModifierKeyStates.LWin;
                    break;
                case System.Windows.Forms.Keys.RWin:
                    _modifierKeyStates &= EModifierKeyStates.All ^ EModifierKeyStates.RWin;
                    break;
                default:
                    isModifier = false;
                    break;
            }

            if (isModifier)
            {
                // Report keyboard state to UI
                _parent.ThreadManager.SubmitUIEvent(new KxKeyboardStateEventArgs(_modifierKeyStates, _toggleKeyStates));
            }

            return isModifier;
        }

        /// <summary>
        /// Handle key press of toggle key e.g. capslock, numlock
        /// </summary>
        /// <param name="keyCode"></param>
        private bool HandleToggleKeyDown(System.Windows.Forms.Keys keyCode)
        {
            bool isToggleKey = true;
            switch (keyCode)
            {
                // Only toggle capslock, numlock, scrolllock, and insert, for key down events
                case System.Windows.Forms.Keys.CapsLock:
                    _toggleKeyStates ^= EToggleKeyStates.CapsLock;
                    break;
                case System.Windows.Forms.Keys.NumLock:
                    _toggleKeyStates ^= EToggleKeyStates.NumLock;
                    break;
                case System.Windows.Forms.Keys.Scroll:
                    _toggleKeyStates ^= EToggleKeyStates.Scroll;
                    break;
                case System.Windows.Forms.Keys.Insert:
                    _toggleKeyStates ^= EToggleKeyStates.Insert;
                    break;
                default:
                    isToggleKey = false;
                    break;
            }

            if (isToggleKey)
            {                
                // Report keyboard state to UI
                _parent.ThreadManager.SubmitUIEvent(new KxKeyboardStateEventArgs(_modifierKeyStates, _toggleKeyStates));
            }

            return isToggleKey;
        }

        /// <summary>
        /// Initialise the list of extended keys
        /// </summary>
        private void InitialiseExtendedKeysList()
        {
            List<Keys> extendedKeys = KeyUtils.GetExtendedKeys();
            foreach (Keys key in extendedKeys)
            {
                _isExtendedKey[(byte)key] = true;
            }
        }
    }
}
