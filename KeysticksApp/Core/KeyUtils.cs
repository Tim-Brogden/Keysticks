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
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using Keysticks.Sys;

namespace Keysticks.Core
{
    /// <summary>
    /// Keyboard utilities
    /// </summary>
    public static class KeyUtils
    {
        // Fields
        private static Mutex _keyDataMutex = new Mutex();
        private static string[] _defaultKeyNames;
        private static string[] _defaultTinyNames;
        private static Dictionary<int, KeyboardLayoutData> _keyboardLayouts;        

        /// <summary>
        /// Constructor
        /// </summary>
        static KeyUtils()
        {
            _keyboardLayouts = new Dictionary<int, KeyboardLayoutData>();
            InitialiseDefaultKeyNames();
            InitialiseDefaultTinyNames();
        }

        /// <summary>
        /// Select a keyboard layout
        /// </summary>
        public static void InitialiseKeyboardLayout(IntPtr dwhkl)
        {
            if (_keyDataMutex.WaitOne(5000))
            {
                try
                {
                    int ihkl = dwhkl.ToInt32();
                    if (!_keyboardLayouts.ContainsKey(ihkl))
                    {                        
                        // Read the keyboard layout and cache it
                        KeyboardLayoutData layoutData = ReadKeyboardLayoutData(dwhkl);
                        _keyboardLayouts[ihkl] = layoutData;
                    }
                }
                catch (Exception)
                {
                    // Ignore
                }
                finally
                {
                    _keyDataMutex.ReleaseMutex();
                }
            }
        }

        /// <summary>
        /// Read the keyboard layout into a useable object
        /// </summary>
        /// <param name="dwhkl"></param>
        /// <returns></returns>
        private static KeyboardLayoutData ReadKeyboardLayoutData(IntPtr dwhkl)
        {
            // Initialise collections
            Dictionary<ushort, VirtualKeyData> virtualKeysByScanCode = new Dictionary<ushort, VirtualKeyData>();
            VirtualKeyData[] virtualKeysByKeyCode = new VirtualKeyData[256];
            List<Keys> extendedKeys = GetExtendedKeys();

            int capacity = 8;
            StringBuilder sb = new StringBuilder(capacity);
            byte[] keyboardState = new byte[256];
            char[] outputChars = new char[capacity];
            uint previous = 0;
            foreach (Keys keyCode in Enum.GetValues(typeof(Keys)))
            {
                // Ignore duplicate names and special values above 255
                uint keyVal = (uint)keyCode;
                if (keyVal > 255 || keyVal == previous)
                {
                    continue;
                }
                previous = keyVal;

                VirtualKeyData virtualKey = new VirtualKeyData();
                virtualKey.KeyCode = keyCode;
                //Trace.WriteLine(keyCode.ToString());
                virtualKey.Name = _defaultKeyNames[keyVal];
                virtualKey.TinyName = _defaultTinyNames[keyVal];
                if (virtualKey.Name != null && virtualKey.TinyName != null)
                {
                    // Get the scan code of the key if there is one
                    ushort scanCode = (ushort)WindowsAPI.MapVirtualKeyEx(keyVal, WindowsAPI.MAPVK_VK_TO_VSC, dwhkl);
                    if (scanCode != 0u)
                    {
                        if (extendedKeys.Contains(keyCode))
                        {
                            scanCode |= 0x0100;
                        }                        

                        //Trace.WriteLine(string.Format("KEY,{0},KEYCODE,{1},SCANCODE,{2}", (int)keyCode, keyCode.ToString(), scanCode));

                        // Get character value for each combination of modifier keys
                        for (int modifierFlags = 0; modifierFlags < 8; modifierFlags++)
                        {
                            EModifierKeyStates modifiers = EModifierKeyStates.None;
                            if ((modifierFlags & 1) != 0) modifiers |= EModifierKeyStates.ShiftKey;
                            if ((modifierFlags & 2) != 0) modifiers |= EModifierKeyStates.ControlKey;
                            if ((modifierFlags & 4) != 0) modifiers |= EModifierKeyStates.Menu;

                            keyboardState[(int)Keys.ShiftKey] = (byte)((modifiers & EModifierKeyStates.ShiftKey) != 0 ? 0x80 : 0x00);
                            keyboardState[(int)Keys.ControlKey] = (byte)((modifiers & EModifierKeyStates.ControlKey) != 0 ? 0x80 : 0x00);
                            keyboardState[(int)Keys.Menu] = (byte)((modifiers & EModifierKeyStates.Menu) != 0 ? 0x80 : 0x00);

                            int numChars = WindowsAPI.ToUnicodeEx(keyVal, scanCode, keyboardState, outputChars, capacity, 0, dwhkl);
                            if (numChars != 0)  // -1 indicates it's a dead key, >=2 indicates outputChars starts with a dead key
                            {
                                // Take the last character because the output sometimes has a dead key character preceding it
                                int charIndex = Math.Max(0, numChars - 1);
                                char singleChar = outputChars[charIndex];
                                UnicodeCategory category = char.GetUnicodeCategory(singleChar);
                                if (category != UnicodeCategory.Control && category != UnicodeCategory.SpaceSeparator)
                                {
                                    // Display the soft hyphen character as a hyphen, otherwise it might be invisible!
                                    if (singleChar == 173)
                                    {
                                        singleChar = '-';
                                    }

                                    virtualKey.OemKeyCombinations[modifiers] = singleChar.ToString();                                    
                                }
                            }
                        }

                        // Set names
                        bool isKeyForScanCode = false;
                        bool hasExisting = virtualKeysByScanCode.ContainsKey(scanCode);
                        if (virtualKey.OemKeyCombinations.ContainsKey(EModifierKeyStates.None))
                        {
                            // Character key - add or replace if its the first character key code with this scan code
                            if (!hasExisting || !virtualKeysByScanCode[scanCode].OemKeyCombinations.ContainsKey(EModifierKeyStates.None))
                            {
                                // Better not to use single character labels for keys that often 
                                // perform a different function when NumLock is on
                                string str = virtualKey.OemKeyCombinations[EModifierKeyStates.None];
                                if (keyVal < (uint)Keys.NumPad0 || keyVal > (uint)Keys.Divide)
                                {
                                    virtualKey.Name = str.ToUpper();
                                }
                                if ((keyVal < (uint)Keys.NumPad0 || keyVal > (uint)Keys.NumPad9) && 
                                    keyVal != (uint)Keys.Decimal)  
                                {
                                    virtualKey.TinyName = str;
                                }
                                isKeyForScanCode = true;
                            }
                        }
                        else if (!hasExisting)
                        {
                            isKeyForScanCode = true;
                        }

                        if (isKeyForScanCode)
                        {
                            // Set the scan codes
                            virtualKey.WindowsScanCode = scanCode;

                            // Store key by scan code
                            VirtualKeyData vk = new VirtualKeyData(virtualKey);
                            virtualKeysByScanCode[scanCode] = vk;
                        }
                    }                    

                    // Add to virtual keys array
                    virtualKeysByKeyCode[keyVal] = virtualKey;
                }
            }            

            // Create single data object to return
            KeyboardLayoutData layoutData = new KeyboardLayoutData(virtualKeysByKeyCode, virtualKeysByScanCode);
            return layoutData;
        }

        /// <summary>
        /// Get the keyboard layout by scan code
        /// </summary>
        /// <returns></returns>
        public static Dictionary<ushort, VirtualKeyData> GetVirtualKeysByScanCode(IntPtr dwhkl)
        {
            Dictionary<ushort, VirtualKeyData> virtualKeyData = new Dictionary<ushort, VirtualKeyData>();

            if (_keyDataMutex.WaitOne(5000))
            {
                int ihkl = dwhkl.ToInt32();
                if (_keyboardLayouts.ContainsKey(ihkl))
                {
                    // Copy to new array
                    Dictionary<ushort, VirtualKeyData>.Enumerator eKey = _keyboardLayouts[ihkl].VirtualKeysByScanCode.GetEnumerator();
                    while (eKey.MoveNext())
                    {
                        virtualKeyData[eKey.Current.Key] = new VirtualKeyData(eKey.Current.Value);
                    }
                }

                _keyDataMutex.ReleaseMutex();
            }

            return virtualKeyData;
        }

        /// <summary>
        /// Get the keyboard layout by key code
        /// </summary>
        /// <returns></returns>
        public static VirtualKeyData[] GetVirtualKeysByKeyCode(IntPtr dwhkl)
        {
            VirtualKeyData[] virtualKeyData = new VirtualKeyData[256];

            if (_keyDataMutex.WaitOne(5000))
            {
                int ihkl = dwhkl.ToInt32();
                if (_keyboardLayouts.ContainsKey(ihkl))
                {
                    // Copy to new array
                    for (int i = 0; i < 256; i++)
                    {
                        VirtualKeyData vk = _keyboardLayouts[ihkl].VirtualKeysByKeyCode[i];
                        if (vk != null)
                        {
                            virtualKeyData[i] = new VirtualKeyData(vk);
                        }
                    }
                }

                _keyDataMutex.ReleaseMutex();
            }

            return virtualKeyData;
        }

        /// <summary>
        /// Get the specified virtual key
        /// </summary>
        /// <param name="scanCode"></param>
        /// <returns></returns>
        public static VirtualKeyData GetVirtualKeyByScanCode(ushort scanCode, IntPtr dwhkl)
        {
            VirtualKeyData vk = null;

            if (_keyDataMutex.WaitOne(5000))
            {
                int ihkl = dwhkl.ToInt32();
                if (_keyboardLayouts.ContainsKey(ihkl))
                {
                    KeyboardLayoutData layoutData = _keyboardLayouts[ihkl];
                    if (layoutData.VirtualKeysByScanCode.ContainsKey(scanCode))
                    {
                        vk = new VirtualKeyData(layoutData.VirtualKeysByScanCode[scanCode]);
                    }
                }

                _keyDataMutex.ReleaseMutex();
            }

            return vk;
        }

        /// <summary>
        /// Get the specified virtual key
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        public static VirtualKeyData GetVirtualKeyByKeyCode(Keys keyCode, IntPtr dwhkl)
        {
            VirtualKeyData vk = null;

            if (_keyDataMutex.WaitOne(5000))
            {
                int ihkl = dwhkl.ToInt32();
                if (_keyboardLayouts.ContainsKey(ihkl))
                {
                    VirtualKeyData virtualKey = _keyboardLayouts[ihkl].VirtualKeysByKeyCode[(byte)keyCode];
                    if (virtualKey != null)
                    {
                        vk = new VirtualKeyData(virtualKey);
                    }
                }

                _keyDataMutex.ReleaseMutex();
            }

            return vk;
        }

        /// <summary>
        /// Get an array containing the extended keys
        /// </summary>
        /// <returns></returns>
        public static List<Keys> GetExtendedKeys()
        {
            List<Keys> extendedKeys = new List<Keys>
            {
                Keys.Home, Keys.End, Keys.Next, Keys.Prior, Keys.Insert, Keys.Delete,
                Keys.Left, Keys.Right, Keys.Up, Keys.Down,
                Keys.Clear, Keys.Divide,
                Keys.RControlKey, Keys.RMenu, Keys.LWin, Keys.RWin, Keys.Apps,
                Keys.VolumeDown, Keys.VolumeMute, Keys.VolumeUp,
                Keys.MediaNextTrack, Keys.MediaPlayPause, Keys.MediaPreviousTrack, Keys.MediaStop,
                Keys.BrowserBack, Keys.BrowserFavorites, Keys.BrowserForward, Keys.BrowserHome,
                Keys.BrowserRefresh, Keys.BrowserSearch, Keys.BrowserStop,
                Keys.LaunchApplication1, Keys.LaunchApplication2, Keys.LaunchMail, Keys.SelectMedia
            };

            return extendedKeys;
        }

        /// <summary>
        /// Initialise a table of default names for keys that have long names
        /// </summary>
        private static void InitialiseDefaultKeyNames()
        {
            _defaultKeyNames = new string[256];

            //_defaultKeyNames[(int)Keys.None] = "None";
            //_defaultKeyNames[(int)Keys.LButton] = "Left Button";
            //_defaultKeyNames[(int)Keys.RButton] = "Right Button";
            _defaultKeyNames[(int)Keys.Cancel] = "Break";
            //_defaultKeyNames[(int)Keys.MButton] = "Middle Button";
            //_defaultKeyNames[(int)Keys.XButton1] = "X1 Button";
            //_defaultKeyNames[(int)Keys.XButton2] = "X2 Button";
            _defaultKeyNames[(int)Keys.Back] = "Backspace";
            _defaultKeyNames[(int)Keys.Tab] = "Tab";
            //_defaultKeyNames[(int)Keys.LineFeed] = "Line Feed";
            _defaultKeyNames[(int)Keys.Clear] = "Clear";
            _defaultKeyNames[(int)Keys.Enter] = "Enter";
            _defaultKeyNames[(int)Keys.ShiftKey] = "Shift";
            _defaultKeyNames[(int)Keys.ControlKey] = "Ctrl";
            _defaultKeyNames[(int)Keys.Menu] = "Alt";
            _defaultKeyNames[(int)Keys.Pause] = "Pause";
            _defaultKeyNames[(int)Keys.CapsLock] = "Caps Lock";
            _defaultKeyNames[(int)Keys.KanaMode] = "Kana Mode";
            //_defaultKeyNames[(int)Keys.HangulMode] = "Hangul Mode";
            _defaultKeyNames[(int)Keys.JunjaMode] = "Junja Mode";
            _defaultKeyNames[(int)Keys.FinalMode] = "Final Mode";
            _defaultKeyNames[(int)Keys.KanjiMode] = "Kanji Mode";
            //_defaultKeyNames[(int)Keys.HanjaMode] = "Hanja Mode";
            _defaultKeyNames[(int)Keys.Escape] = "Escape";
            _defaultKeyNames[(int)Keys.IMEConvert] = "IME Convert";
            _defaultKeyNames[(int)Keys.IMENonconvert] = "IME No Convert";
            _defaultKeyNames[(int)Keys.IMEAccept] = "IME Accept";
            _defaultKeyNames[(int)Keys.IMEModeChange] = "IME Mode Change";
            _defaultKeyNames[(int)Keys.Space] = "Space";
            _defaultKeyNames[(int)Keys.PageUp] = "Page Up";
            _defaultKeyNames[(int)Keys.PageDown] = "Page Down";
            _defaultKeyNames[(int)Keys.End] = "End";
            _defaultKeyNames[(int)Keys.Home] = "Home";
            _defaultKeyNames[(int)Keys.Left] = "Left";
            _defaultKeyNames[(int)Keys.Up] = "Up";
            _defaultKeyNames[(int)Keys.Right] = "Right";
            _defaultKeyNames[(int)Keys.Down] = "Down";
            //_defaultKeyNames[(int)Keys.Select] = "Select";
            //_defaultKeyNames[(int)Keys.Print] = "Print";
            //_defaultKeyNames[(int)Keys.Execute] = "Execute";
            _defaultKeyNames[(int)Keys.PrintScreen] = "Print Screen";
            _defaultKeyNames[(int)Keys.Insert] = "Insert";
            _defaultKeyNames[(int)Keys.Delete] = "Delete";
            _defaultKeyNames[(int)Keys.Help] = "Help";
            _defaultKeyNames[(int)Keys.D0] = "0";
            _defaultKeyNames[(int)Keys.D1] = "1";
            _defaultKeyNames[(int)Keys.D2] = "2";
            _defaultKeyNames[(int)Keys.D3] = "3";
            _defaultKeyNames[(int)Keys.D4] = "4";
            _defaultKeyNames[(int)Keys.D5] = "5";
            _defaultKeyNames[(int)Keys.D6] = "6";
            _defaultKeyNames[(int)Keys.D7] = "7";
            _defaultKeyNames[(int)Keys.D8] = "8";
            _defaultKeyNames[(int)Keys.D9] = "9";
            _defaultKeyNames[(int)Keys.A] = "A";
            _defaultKeyNames[(int)Keys.B] = "B";
            _defaultKeyNames[(int)Keys.C] = "C";
            _defaultKeyNames[(int)Keys.D] = "D";
            _defaultKeyNames[(int)Keys.E] = "E";
            _defaultKeyNames[(int)Keys.F] = "F";
            _defaultKeyNames[(int)Keys.G] = "G";
            _defaultKeyNames[(int)Keys.H] = "H";
            _defaultKeyNames[(int)Keys.I] = "I";
            _defaultKeyNames[(int)Keys.J] = "J";
            _defaultKeyNames[(int)Keys.K] = "K";
            _defaultKeyNames[(int)Keys.L] = "L";
            _defaultKeyNames[(int)Keys.M] = "M";
            _defaultKeyNames[(int)Keys.N] = "N";
            _defaultKeyNames[(int)Keys.O] = "O";
            _defaultKeyNames[(int)Keys.P] = "P";
            _defaultKeyNames[(int)Keys.Q] = "Q";
            _defaultKeyNames[(int)Keys.R] = "R";
            _defaultKeyNames[(int)Keys.S] = "S";
            _defaultKeyNames[(int)Keys.T] = "T";
            _defaultKeyNames[(int)Keys.U] = "U";
            _defaultKeyNames[(int)Keys.V] = "V";
            _defaultKeyNames[(int)Keys.W] = "W";
            _defaultKeyNames[(int)Keys.X] = "X";
            _defaultKeyNames[(int)Keys.Y] = "Y";
            _defaultKeyNames[(int)Keys.Z] = "Z";
            _defaultKeyNames[(int)Keys.LWin] = "Left Windows";
            _defaultKeyNames[(int)Keys.RWin] = "Right Windows";
            _defaultKeyNames[(int)Keys.Apps] = "Application";
            _defaultKeyNames[(int)Keys.Sleep] = "Sleep";
            _defaultKeyNames[(int)Keys.NumPad0] = "Num 0";
            _defaultKeyNames[(int)Keys.NumPad1] = "Num 1";
            _defaultKeyNames[(int)Keys.NumPad2] = "Num 2";
            _defaultKeyNames[(int)Keys.NumPad3] = "Num 3";
            _defaultKeyNames[(int)Keys.NumPad4] = "Num 4";
            _defaultKeyNames[(int)Keys.NumPad5] = "Num 5";
            _defaultKeyNames[(int)Keys.NumPad6] = "Num 6";
            _defaultKeyNames[(int)Keys.NumPad7] = "Num 7";
            _defaultKeyNames[(int)Keys.NumPad8] = "Num 8";
            _defaultKeyNames[(int)Keys.NumPad9] = "Num 9";
            _defaultKeyNames[(int)Keys.Multiply] = "Num *";
            _defaultKeyNames[(int)Keys.Add] = "Num +";
            _defaultKeyNames[(int)Keys.Separator] = "Num ,";
            _defaultKeyNames[(int)Keys.Subtract] = "Num -";
            _defaultKeyNames[(int)Keys.Decimal] = "Num .";
            _defaultKeyNames[(int)Keys.Divide] = "Num /";
            _defaultKeyNames[(int)Keys.F1] = "F1";
            _defaultKeyNames[(int)Keys.F2] = "F2";
            _defaultKeyNames[(int)Keys.F3] = "F3";
            _defaultKeyNames[(int)Keys.F4] = "F4";
            _defaultKeyNames[(int)Keys.F5] = "F5";
            _defaultKeyNames[(int)Keys.F6] = "F6";
            _defaultKeyNames[(int)Keys.F7] = "F7";
            _defaultKeyNames[(int)Keys.F8] = "F8";
            _defaultKeyNames[(int)Keys.F9] = "F9";
            _defaultKeyNames[(int)Keys.F10] = "F10";
            _defaultKeyNames[(int)Keys.F11] = "F11";
            _defaultKeyNames[(int)Keys.F12] = "F12";
            _defaultKeyNames[(int)Keys.F13] = "F13";
            _defaultKeyNames[(int)Keys.F14] = "F14";
            _defaultKeyNames[(int)Keys.F15] = "F15";
            _defaultKeyNames[(int)Keys.F16] = "F16";
            _defaultKeyNames[(int)Keys.F17] = "F17";
            _defaultKeyNames[(int)Keys.F18] = "F18";
            _defaultKeyNames[(int)Keys.F19] = "F19";
            _defaultKeyNames[(int)Keys.F20] = "F20";
            _defaultKeyNames[(int)Keys.F21] = "F21";
            _defaultKeyNames[(int)Keys.F22] = "F22";
            _defaultKeyNames[(int)Keys.F23] = "F23";
            _defaultKeyNames[(int)Keys.F24] = "F24";
            _defaultKeyNames[(int)Keys.NumLock] = "Num Lock";
            _defaultKeyNames[(int)Keys.Scroll] = "Scroll Lock";
            _defaultKeyNames[(int)Keys.LShiftKey] = "Left Shift";
            _defaultKeyNames[(int)Keys.RShiftKey] = "Right Shift";
            _defaultKeyNames[(int)Keys.LControlKey] = "Left Ctrl";
            _defaultKeyNames[(int)Keys.RControlKey] = "Right Ctrl";
            _defaultKeyNames[(int)Keys.LMenu] = "Left Alt";
            _defaultKeyNames[(int)Keys.RMenu] = "Right Alt";
            _defaultKeyNames[(int)Keys.BrowserBack] = "Browser Back";
            _defaultKeyNames[(int)Keys.BrowserForward] = "Browser Forward";
            _defaultKeyNames[(int)Keys.BrowserRefresh] = "Browser Refresh";
            _defaultKeyNames[(int)Keys.BrowserStop] = "Browser Stop";
            _defaultKeyNames[(int)Keys.BrowserSearch] = "Browser Search";
            _defaultKeyNames[(int)Keys.BrowserFavorites] = "Browser Favourites";
            _defaultKeyNames[(int)Keys.BrowserHome] = "Browser Home";
            _defaultKeyNames[(int)Keys.VolumeMute] = "Volume Mute";
            _defaultKeyNames[(int)Keys.VolumeDown] = "Volume Down";
            _defaultKeyNames[(int)Keys.VolumeUp] = "Volume Up";
            _defaultKeyNames[(int)Keys.MediaNextTrack] = "Next Track";
            _defaultKeyNames[(int)Keys.MediaPreviousTrack] = "Previous Track";
            _defaultKeyNames[(int)Keys.MediaStop] = "Stop Playing";
            _defaultKeyNames[(int)Keys.MediaPlayPause] = "Play/Pause";
            _defaultKeyNames[(int)Keys.LaunchMail] = "Launch Mail";
            _defaultKeyNames[(int)Keys.SelectMedia] = "Select Media";
            _defaultKeyNames[(int)Keys.LaunchApplication1] = "Launch App1";
            _defaultKeyNames[(int)Keys.LaunchApplication2] = "Launch App2";
            _defaultKeyNames[(int)Keys.Oem1] = "Oem 1";
            _defaultKeyNames[(int)Keys.OemSemicolon] = "Oem ;";
            _defaultKeyNames[(int)Keys.Oemplus] = "Oem +";
            _defaultKeyNames[(int)Keys.Oemcomma] = "Oem ,";
            _defaultKeyNames[(int)Keys.OemMinus] = "Oem -";
            _defaultKeyNames[(int)Keys.OemPeriod] = "Oem .";
            _defaultKeyNames[(int)Keys.OemQuestion] = "Oem ?";
            _defaultKeyNames[(int)Keys.Oem2] = "Oem 2";
            _defaultKeyNames[(int)Keys.Oemtilde] = "Oem ~";
            _defaultKeyNames[(int)Keys.Oem3] = "Oem 3";
            _defaultKeyNames[(int)Keys.Oem4] = "Oem 4";
            _defaultKeyNames[(int)Keys.OemOpenBrackets] = "Oem [";
            _defaultKeyNames[(int)Keys.OemPipe] = "Oem |";
            _defaultKeyNames[(int)Keys.Oem5] = "Oem 5";
            _defaultKeyNames[(int)Keys.Oem6] = "Oem 6";
            _defaultKeyNames[(int)Keys.OemCloseBrackets] = "Oem ]";
            _defaultKeyNames[(int)Keys.Oem7] = "Oem 7";
            _defaultKeyNames[(int)Keys.OemQuotes] = "Oem '";
            _defaultKeyNames[(int)Keys.Oem8] = "Oem 8";
            _defaultKeyNames[(int)Keys.Oem102] = "Oem 102";
            _defaultKeyNames[(int)Keys.OemBackslash] = "Oem \\";
            //_defaultKeyNames[(int)Keys.ProcessKey] = "Process";
            //_defaultKeyNames[(int)Keys.Packet] = "Packet";
            //_defaultKeyNames[(int)Keys.Attn] = "Attn";
            //_defaultKeyNames[(int)Keys.Crsel] = "Crsel";
            //_defaultKeyNames[(int)Keys.Exsel] = "Exsel";
            //_defaultKeyNames[(int)Keys.EraseEof] = "Erase Eof";
            //_defaultKeyNames[(int)Keys.Play] = "Play";
            //_defaultKeyNames[(int)Keys.Zoom] = "Zoom";
            //_defaultKeyNames[(int)Keys.NoName] = "No Name";
            //_defaultKeyNames[(int)Keys.Pa1] = "Pa 1";
            //_defaultKeyNames[(int)Keys.OemClear] = "Clear";
        }

        /// <summary>
        /// Initialise a table of default tiny names for keys that have long names
        /// </summary>
        private static void InitialiseDefaultTinyNames()
        {
            _defaultTinyNames = new string[256];

            //_defaultTinyNames[(int)Keys.None] = "None";
            //_defaultTinyNames[(int)Keys.LButton] = "LBtn";
            //_defaultTinyNames[(int)Keys.RButton] = "RBtn";
            _defaultTinyNames[(int)Keys.Cancel] = "Break";
            //_defaultTinyNames[(int)Keys.MButton] = "MBtn";
            //_defaultTinyNames[(int)Keys.XButton1] = "X1";
            //_defaultTinyNames[(int)Keys.XButton2] = "X2";
            _defaultTinyNames[(int)Keys.Back] = "Backs";
            _defaultTinyNames[(int)Keys.Tab] = "Tab";
            //_defaultTinyNames[(int)Keys.LineFeed] = "LF";
            _defaultTinyNames[(int)Keys.Clear] = "Clear";
            _defaultTinyNames[(int)Keys.Enter] = "Enter";
            _defaultTinyNames[(int)Keys.ShiftKey] = "Shift";
            _defaultTinyNames[(int)Keys.ControlKey] = "Ctrl";
            _defaultTinyNames[(int)Keys.Menu] = "Alt";
            _defaultTinyNames[(int)Keys.Pause] = "Pause";
            _defaultTinyNames[(int)Keys.CapsLock] = "Caps";
            _defaultTinyNames[(int)Keys.KanaMode] = "Kana";
            //_defaultTinyNames[(int)Keys.HangulMode] = "H.gul";
            _defaultTinyNames[(int)Keys.JunjaMode] = "Junja";
            _defaultTinyNames[(int)Keys.FinalMode] = "Final";
            _defaultTinyNames[(int)Keys.KanjiMode] = "Kanji";
            //_defaultTinyNames[(int)Keys.HanjaMode] = "Hanja";
            _defaultTinyNames[(int)Keys.Escape] = "Esc";
            _defaultTinyNames[(int)Keys.IMEConvert] = "IMECo";
            _defaultTinyNames[(int)Keys.IMENonconvert] = "IMENo";
            _defaultTinyNames[(int)Keys.IMEAccept] = "IMEAc";
            _defaultTinyNames[(int)Keys.IMEModeChange] = "IMECh";
            _defaultTinyNames[(int)Keys.Space] = "Space";
            _defaultTinyNames[(int)Keys.PageUp] = "Pg Up";
            _defaultTinyNames[(int)Keys.PageDown] = "Pg Dn";
            _defaultTinyNames[(int)Keys.End] = "End";
            _defaultTinyNames[(int)Keys.Home] = "Home";
            _defaultTinyNames[(int)Keys.Left] = "Left";
            _defaultTinyNames[(int)Keys.Up] = "Up";
            _defaultTinyNames[(int)Keys.Right] = "Right";
            _defaultTinyNames[(int)Keys.Down] = "Down";
            //_defaultTinyNames[(int)Keys.Select] = "Sel";
            //_defaultTinyNames[(int)Keys.Print] = "Print";
            //_defaultTinyNames[(int)Keys.Execute] = "Exec";
            _defaultTinyNames[(int)Keys.PrintScreen] = "PrScr";
            _defaultTinyNames[(int)Keys.Insert] = "Ins";
            _defaultTinyNames[(int)Keys.Delete] = "Del";
            _defaultTinyNames[(int)Keys.Help] = "Help";
            _defaultTinyNames[(int)Keys.D0] = "0";
            _defaultTinyNames[(int)Keys.D1] = "1";
            _defaultTinyNames[(int)Keys.D2] = "2";
            _defaultTinyNames[(int)Keys.D3] = "3";
            _defaultTinyNames[(int)Keys.D4] = "4";
            _defaultTinyNames[(int)Keys.D5] = "5";
            _defaultTinyNames[(int)Keys.D6] = "6";
            _defaultTinyNames[(int)Keys.D7] = "7";
            _defaultTinyNames[(int)Keys.D8] = "8";
            _defaultTinyNames[(int)Keys.D9] = "9";
            _defaultTinyNames[(int)Keys.A] = "a";
            _defaultTinyNames[(int)Keys.B] = "b";
            _defaultTinyNames[(int)Keys.C] = "c";
            _defaultTinyNames[(int)Keys.D] = "d";
            _defaultTinyNames[(int)Keys.E] = "e";
            _defaultTinyNames[(int)Keys.F] = "f";
            _defaultTinyNames[(int)Keys.G] = "g";
            _defaultTinyNames[(int)Keys.H] = "h";
            _defaultTinyNames[(int)Keys.I] = "i";
            _defaultTinyNames[(int)Keys.J] = "j";
            _defaultTinyNames[(int)Keys.K] = "k";
            _defaultTinyNames[(int)Keys.L] = "l";
            _defaultTinyNames[(int)Keys.M] = "m";
            _defaultTinyNames[(int)Keys.N] = "n";
            _defaultTinyNames[(int)Keys.O] = "o";
            _defaultTinyNames[(int)Keys.P] = "p";
            _defaultTinyNames[(int)Keys.Q] = "q";
            _defaultTinyNames[(int)Keys.R] = "r";
            _defaultTinyNames[(int)Keys.S] = "s";
            _defaultTinyNames[(int)Keys.T] = "t";
            _defaultTinyNames[(int)Keys.U] = "u";
            _defaultTinyNames[(int)Keys.V] = "v";
            _defaultTinyNames[(int)Keys.W] = "w";
            _defaultTinyNames[(int)Keys.X] = "x";
            _defaultTinyNames[(int)Keys.Y] = "y";
            _defaultTinyNames[(int)Keys.Z] = "z";
            _defaultTinyNames[(int)Keys.LWin] = "Win";
            _defaultTinyNames[(int)Keys.RWin] = "RWin";
            _defaultTinyNames[(int)Keys.Apps] = "Apps";
            _defaultTinyNames[(int)Keys.Sleep] = "Sleep";
            _defaultTinyNames[(int)Keys.NumPad0] = "Num 0";
            _defaultTinyNames[(int)Keys.NumPad1] = "Num 1";
            _defaultTinyNames[(int)Keys.NumPad2] = "Num 2";
            _defaultTinyNames[(int)Keys.NumPad3] = "Num 3";
            _defaultTinyNames[(int)Keys.NumPad4] = "Num 4";
            _defaultTinyNames[(int)Keys.NumPad5] = "Num 5";
            _defaultTinyNames[(int)Keys.NumPad6] = "Num 6";
            _defaultTinyNames[(int)Keys.NumPad7] = "Num 7";
            _defaultTinyNames[(int)Keys.NumPad8] = "Num 8";
            _defaultTinyNames[(int)Keys.NumPad9] = "Num 9";
            _defaultTinyNames[(int)Keys.Multiply] = "Num *";
            _defaultTinyNames[(int)Keys.Add] = "Num +";
            _defaultTinyNames[(int)Keys.Separator] = "Num ,";
            _defaultTinyNames[(int)Keys.Subtract] = "Num -";
            _defaultTinyNames[(int)Keys.Decimal] = "Num .";
            _defaultTinyNames[(int)Keys.Divide] = "Num /";
            _defaultTinyNames[(int)Keys.F1] = "F1";
            _defaultTinyNames[(int)Keys.F2] = "F2";
            _defaultTinyNames[(int)Keys.F3] = "F3";
            _defaultTinyNames[(int)Keys.F4] = "F4";
            _defaultTinyNames[(int)Keys.F5] = "F5";
            _defaultTinyNames[(int)Keys.F6] = "F6";
            _defaultTinyNames[(int)Keys.F7] = "F7";
            _defaultTinyNames[(int)Keys.F8] = "F8";
            _defaultTinyNames[(int)Keys.F9] = "F9";
            _defaultTinyNames[(int)Keys.F10] = "F10";
            _defaultTinyNames[(int)Keys.F11] = "F11";
            _defaultTinyNames[(int)Keys.F12] = "F12";
            _defaultTinyNames[(int)Keys.F13] = "F13";
            _defaultTinyNames[(int)Keys.F14] = "F14";
            _defaultTinyNames[(int)Keys.F15] = "F15";
            _defaultTinyNames[(int)Keys.F16] = "F16";
            _defaultTinyNames[(int)Keys.F17] = "F17";
            _defaultTinyNames[(int)Keys.F18] = "F18";
            _defaultTinyNames[(int)Keys.F19] = "F19";
            _defaultTinyNames[(int)Keys.F20] = "F20";
            _defaultTinyNames[(int)Keys.F21] = "F21";
            _defaultTinyNames[(int)Keys.F22] = "F22";
            _defaultTinyNames[(int)Keys.F23] = "F23";
            _defaultTinyNames[(int)Keys.F24] = "F24";
            _defaultTinyNames[(int)Keys.NumLock] = "Num";
            _defaultTinyNames[(int)Keys.Scroll] = "Scr";
            _defaultTinyNames[(int)Keys.LShiftKey] = "Shift";
            _defaultTinyNames[(int)Keys.RShiftKey] = "Shift";
            _defaultTinyNames[(int)Keys.LControlKey] = "Ctrl";
            _defaultTinyNames[(int)Keys.RControlKey] = "Ctrl";
            _defaultTinyNames[(int)Keys.LMenu] = "Alt";
            _defaultTinyNames[(int)Keys.RMenu] = "Alt";
            _defaultTinyNames[(int)Keys.BrowserBack] = "Back";
            _defaultTinyNames[(int)Keys.BrowserForward] = "Fwd";
            _defaultTinyNames[(int)Keys.BrowserRefresh] = "Refr.";
            _defaultTinyNames[(int)Keys.BrowserStop] = "BStop";
            _defaultTinyNames[(int)Keys.BrowserSearch] = "Sear.";
            _defaultTinyNames[(int)Keys.BrowserFavorites] = "Fav";
            _defaultTinyNames[(int)Keys.BrowserHome] = "Home";
            _defaultTinyNames[(int)Keys.VolumeMute] = "Mute";
            _defaultTinyNames[(int)Keys.VolumeDown] = "Vol-";
            _defaultTinyNames[(int)Keys.VolumeUp] = "Vol+";
            _defaultTinyNames[(int)Keys.MediaNextTrack] = ">>|";
            _defaultTinyNames[(int)Keys.MediaPreviousTrack] = "|<<";
            _defaultTinyNames[(int)Keys.MediaStop] = "Stop";
            _defaultTinyNames[(int)Keys.MediaPlayPause] = "Play";
            _defaultTinyNames[(int)Keys.LaunchMail] = "Mail";
            _defaultTinyNames[(int)Keys.SelectMedia] = "Media";
            _defaultTinyNames[(int)Keys.LaunchApplication1] = "App1";
            _defaultTinyNames[(int)Keys.LaunchApplication2] = "App2";
            _defaultTinyNames[(int)Keys.Oem1] = "Oem1";
            _defaultTinyNames[(int)Keys.OemSemicolon] = "Oem;";
            _defaultTinyNames[(int)Keys.Oemplus] = "Oem+";
            _defaultTinyNames[(int)Keys.Oemcomma] = "Oem,";
            _defaultTinyNames[(int)Keys.OemMinus] = "Oem-";
            _defaultTinyNames[(int)Keys.OemPeriod] = "Oem.";
            _defaultTinyNames[(int)Keys.OemQuestion] = "Oem?";
            _defaultTinyNames[(int)Keys.Oem2] = "Oem2";
            _defaultTinyNames[(int)Keys.Oemtilde] = "Oem~";
            _defaultTinyNames[(int)Keys.Oem3] = "Oem3";
            _defaultTinyNames[(int)Keys.Oem4] = "Oem4";
            _defaultTinyNames[(int)Keys.OemOpenBrackets] = "Oem[";
            _defaultTinyNames[(int)Keys.OemPipe] = "Oem|";
            _defaultTinyNames[(int)Keys.Oem5] = "Oem5";
            _defaultTinyNames[(int)Keys.Oem6] = "Oem6";
            _defaultTinyNames[(int)Keys.OemCloseBrackets] = "Oem]";
            _defaultTinyNames[(int)Keys.Oem7] = "Oem7";
            _defaultTinyNames[(int)Keys.OemQuotes] = "Oem'";
            _defaultTinyNames[(int)Keys.Oem8] = "Oem8";
            _defaultTinyNames[(int)Keys.Oem102] = "O102";
            _defaultTinyNames[(int)Keys.OemBackslash] = "Oem\\";
            //_defaultTinyNames[(int)Keys.ProcessKey] = "Proc";
            //_defaultTinyNames[(int)Keys.Packet] = "Pkt";
            //_defaultTinyNames[(int)Keys.Attn] = "Attn";
            //_defaultTinyNames[(int)Keys.Crsel] = "Crsel";
            //_defaultTinyNames[(int)Keys.Exsel] = "Exsel";
            //_defaultTinyNames[(int)Keys.EraseEof] = "E.Eof";
            //_defaultTinyNames[(int)Keys.Play] = "Play";
            //_defaultTinyNames[(int)Keys.Zoom] = "Zoom";
            //_defaultTinyNames[(int)Keys.NoName] = "NoNa";
            //_defaultTinyNames[(int)Keys.Pa1] = "Pa1";
            //_defaultTinyNames[(int)Keys.OemClear] = "Clear";
        }

        /// <summary>
        /// Get the scan codes for an en-gb keyboard layout
        /// </summary>
        /// <returns></returns>
        public static Dictionary<char, ushort> GetDefaultAlphanumScanCodes()
        {
            Dictionary<char, ushort> defaultScanCodes = new Dictionary<char,ushort>();

            defaultScanCodes['0'] = 11;
            defaultScanCodes['1'] = 2;
            defaultScanCodes['2'] = 3;
            defaultScanCodes['3'] = 4;
            defaultScanCodes['4'] = 5;
            defaultScanCodes['5'] = 6;
            defaultScanCodes['6'] = 7;
            defaultScanCodes['7'] = 8;
            defaultScanCodes['8'] = 9;
            defaultScanCodes['9'] = 10;
            defaultScanCodes['a'] = 30;
            defaultScanCodes['b'] = 48;
            defaultScanCodes['c'] = 46;
            defaultScanCodes['d'] = 32;
            defaultScanCodes['e'] = 18;
            defaultScanCodes['f'] = 33;
            defaultScanCodes['g'] = 34;
            defaultScanCodes['h'] = 35;
            defaultScanCodes['i'] = 23;
            defaultScanCodes['j'] = 36;
            defaultScanCodes['k'] = 37;
            defaultScanCodes['l'] = 38;
            defaultScanCodes['m'] = 50;
            defaultScanCodes['n'] = 49;
            defaultScanCodes['o'] = 24;
            defaultScanCodes['p'] = 25;
            defaultScanCodes['q'] = 16;
            defaultScanCodes['r'] = 19;
            defaultScanCodes['s'] = 31;
            defaultScanCodes['t'] = 20;
            defaultScanCodes['u'] = 22;
            defaultScanCodes['v'] = 47;
            defaultScanCodes['w'] = 17;
            defaultScanCodes['x'] = 45;
            defaultScanCodes['y'] = 21;
            defaultScanCodes['z'] = 44;            
            defaultScanCodes[';'] = 39;
            defaultScanCodes['='] = 13;
            defaultScanCodes[','] = 51;
            defaultScanCodes['-'] = 12;
            defaultScanCodes['.'] = 52;
            defaultScanCodes['/'] = 53;
            defaultScanCodes['\''] = 40;
            defaultScanCodes['['] = 26;
            defaultScanCodes['\\'] = 86;
            defaultScanCodes[']'] = 27;
            defaultScanCodes['#'] = 43;
            defaultScanCodes['`'] = 41;
            
            return defaultScanCodes;
        }        
    }
}
