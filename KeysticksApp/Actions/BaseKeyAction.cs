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
using System.Windows.Forms;
using Keysticks.Core;

namespace Keysticks.Actions
{
    // Base class for keystroke actions
    public abstract class BaseKeyAction : BaseAction
    {
        private ushort _scanCode;
        private Keys _keyCode;
        private bool _isVirtualKey;
        private VirtualKeyData _keyData;

        public int UniqueKeyID
        {
            get
            {
                return _scanCode | ((int)_keyCode << 16);                
            }
            set
            {
                _scanCode = (ushort)(value & 0xFFFF);
                _keyCode = (Keys)(value >> 16);               
            }
        }
        public bool IsVirtualKey
        {
            get { return _isVirtualKey; }
            set
            {
                if (_isVirtualKey != value)
                {
                    _isVirtualKey = value;                   
                }
            }
        }

        public VirtualKeyData KeyData { get { return _keyData; } }

        /// <summary>
        /// Return a name for the key
        /// </summary>
        public override string Name
        {
            get
            {
                string name;
                if (_keyData != null)
                {
                    name = _keyData.Name;
                }
                else
                {
                    name = Properties.Resources.String_NotKnownAbbrev;
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
                if (_keyData != null)
                {
                    switch (_keyData.KeyCode)
                    {
                        case Keys.LWin:
                        case Keys.RWin:
                        case Keys.Apps:
                        case Keys.Left:
                        case Keys.Right:
                        case Keys.Up:
                        case Keys.Down:
                        case Keys.BrowserBack:
                        case Keys.BrowserFavorites:
                        case Keys.BrowserForward:
                        case Keys.BrowserHome:
                        case Keys.BrowserRefresh:
                        case Keys.BrowserSearch:
                        case Keys.BrowserStop:
                        case Keys.LaunchApplication1:
                        case Keys.LaunchApplication2:
                        case Keys.LaunchMail:
                        case Keys.MediaNextTrack:
                        case Keys.MediaPlayPause:
                        case Keys.MediaPreviousTrack:
                        case Keys.MediaStop:
                        case Keys.SelectMedia:
                        case Keys.VolumeDown:
                        case Keys.VolumeMute:
                        case Keys.VolumeUp:
                            tinyName = "";      // Use icon instead
                            break;
                        default:
                            tinyName = _keyData.TinyName;
                            break;
                    }
                }
                else
                {
                    tinyName = Properties.Resources.String_NotKnownAbbrev;
                }

                return tinyName;
            }
        }

        public override EAnnotationImage IconRef
        {
            get
            {
                EAnnotationImage icon = EAnnotationImage.None;
                if (_keyData != null)
                {
                    switch (_keyData.KeyCode)
                    {
                        case Keys.LWin:
                        case Keys.RWin:
                            icon = EAnnotationImage.WindowsKey; break;
                        case Keys.Apps:
                            icon = EAnnotationImage.ApplicationsKey; break;
                        case Keys.Left:
                            icon = EAnnotationImage.LeftArrow; break;
                        case Keys.Right:
                            icon = EAnnotationImage.RightArrow; break;
                        case Keys.Up:
                            icon = EAnnotationImage.UpArrow; break;
                        case Keys.Down:
                            icon = EAnnotationImage.DownArrow; break;
                        case Keys.BrowserBack:
                            icon = EAnnotationImage.BrowserBack; break;
                        case Keys.BrowserFavorites:
                            icon = EAnnotationImage.BrowserFavourites; break;
                        case Keys.BrowserForward:
                            icon = EAnnotationImage.BrowserForward; break;
                        case Keys.BrowserHome:
                            icon = EAnnotationImage.BrowserHome; break;
                        case Keys.BrowserRefresh:
                            icon = EAnnotationImage.BrowserRefresh; break;
                        case Keys.BrowserSearch:
                            icon = EAnnotationImage.BrowserSearch; break;
                        case Keys.BrowserStop:
                            icon = EAnnotationImage.BrowserStop; break;
                        case Keys.LaunchApplication1:
                            icon = EAnnotationImage.Explorer; break;
                        case Keys.LaunchApplication2:
                            icon = EAnnotationImage.Calculator; break;
                        case Keys.LaunchMail:
                            icon = EAnnotationImage.Mail; break;
                        case Keys.MediaNextTrack:
                            icon = EAnnotationImage.NextTrack; break;
                        case Keys.MediaPlayPause:
                            icon = EAnnotationImage.PlayPause; break;
                        case Keys.MediaPreviousTrack:
                            icon = EAnnotationImage.PreviousTrack; break;
                        case Keys.MediaStop:
                            icon = EAnnotationImage.StopPlaying; break;
                        case Keys.SelectMedia:
                            icon = EAnnotationImage.MediaPlayer; break;
                        case Keys.VolumeDown:
                            icon = EAnnotationImage.VolumeDown; break;
                        case Keys.VolumeMute:
                            icon = EAnnotationImage.VolumeMute; break;
                        case Keys.VolumeUp:
                            icon = EAnnotationImage.VolumeUp; break;
                    }
                }

                return icon;
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseKeyAction()
            : base()
        {
            _scanCode = 30;   // usually A key
            _keyCode = Keys.A;
        }
        
        public override void Initialise(IKeyboardContext context)
        {
            base.Initialise(context);

            if (_scanCode != 0 && !_isVirtualKey)
            {
                _keyData = KeyUtils.GetVirtualKeyByScanCode(_scanCode, context.KeyboardHKL);

                // Make sure the key code is set
                if (_keyData != null)
                {
                    _keyCode = _keyData.KeyCode;
                }
            }
            else
            {
                _keyData = KeyUtils.GetVirtualKeyByKeyCode(_keyCode, context.KeyboardHKL);

                // Make sure the scan code is set
                if (_keyData != null)
                {
                    _scanCode = _keyData.WindowsScanCode;
                }
            }
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _scanCode = ushort.Parse(element.GetAttribute("scancode"), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture);
            if (element.HasAttribute("keycode"))
            {
                _keyCode = (Keys)Enum.Parse(typeof(Keys), element.GetAttribute("keycode"));
            }
            if (element.HasAttribute("isvirtual"))
            {
                _isVirtualKey = bool.Parse(element.GetAttribute("isvirtual"));
            }            

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("scancode", _scanCode.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("keycode", _keyCode.ToString());
            element.SetAttribute("isvirtual", _isVirtualKey.ToString());
        }

    }
}
