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
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Sys;

namespace Keysticks.Actions
{
    /// <summary>
    /// Click, double-click, hold or release a mouse button
    /// </summary>
    public class MouseButtonAction : BaseAction
    {
        // Config
        private EMouseButtonActionType _mouseButtonActionType = EMouseButtonActionType.Click;
        private EMouseState _mouseButton = EMouseState.Left;

        // State
        private long _lastActionTimeTicks;
        private bool _isPressed;
        private int _pressCount;
        private int _numPressesRequired;

        // Properties
        public EMouseButtonActionType MouseButtonActionType { get { return _mouseButtonActionType; } set { _mouseButtonActionType = value; } }
        public EMouseState MouseButton { get { return _mouseButton; } set { _mouseButton = value; } }

        /// <summary>
        /// Return the type of action
        /// </summary>
        public override EActionType ActionType
        {
            get 
            {
                EActionType actionType;
                switch (_mouseButtonActionType)
                {
                    case EMouseButtonActionType.Click:
                    default:
                        actionType = EActionType.ClickMouseButton; break;
                    case EMouseButtonActionType.DoubleClick:
                        actionType = EActionType.DoubleClickMouseButton; break;
                    case EMouseButtonActionType.PressDown:
                        actionType = EActionType.PressDownMouseButton; break;
                    case EMouseButtonActionType.Release:
                        actionType = EActionType.ReleaseMouseButton; break;
                    case EMouseButtonActionType.Toggle:
                        actionType = EActionType.ToggleMouseButton; break;
                }
                return actionType; 
            }
        }

        /// <summary>
        /// Return the name of the action
        /// </summary>
        /// <returns></returns>
        public override string Name
        {
            get
            {
                string verb;
                switch (_mouseButtonActionType)
                {
                    case EMouseButtonActionType.Click:
                    default:
                        verb = Properties.Resources.String_Click; break;
                    case EMouseButtonActionType.DoubleClick:
                        verb = Properties.Resources.String_Double_click; break;
                    case EMouseButtonActionType.PressDown:
                        verb = Properties.Resources.String_Press; break;
                    case EMouseButtonActionType.Release:
                        verb = Properties.Resources.String_Release; break;
                    case EMouseButtonActionType.Toggle:
                        verb = Properties.Resources.String_Toggle; break;
                }
                StringUtils utils = new StringUtils();
                return string.Format(Properties.Resources.String_ActionMouseButton, verb, utils.MouseButtonToString(_mouseButton).ToLower());
            }
        }        

        /// <summary>
        /// Return the short name of the action
        /// </summary>
        /// <returns></returns>
        public override string ShortName
        {
            get
            {
                return Name;
            }
        }
        
        /// <summary>
        /// Get the icon to use
        /// </summary>
        public override EAnnotationImage IconRef
        {
            get
            {
                EAnnotationImage icon;
                switch (_mouseButton)
                {
                    default:
                    case EMouseState.Left:
                        icon = EAnnotationImage.LeftMouseButton; break;
                    case EMouseState.Middle:
                        icon = EAnnotationImage.MiddleMouseButton; break;
                    case EMouseState.Right:
                        icon = EAnnotationImage.RightMouseButton; break;
                    case EMouseState.X1:
                        icon = EAnnotationImage.X1MouseButton; break;
                    case EMouseState.X2:
                        icon = EAnnotationImage.X2MouseButton; break;
                }
                return icon;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MouseButtonAction()
            : base()
        {
        }

        /// <summary>
        /// Update how many presses are required
        /// </summary>
        public override void Initialise(IKeyboardContext context)
        {
            base.Initialise(context);

            _numPressesRequired = (_mouseButtonActionType == EMouseButtonActionType.DoubleClick) ? 2 : 1;
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _mouseButtonActionType = (EMouseButtonActionType)Enum.Parse(typeof(EMouseButtonActionType), element.GetAttribute("mousebuttonactiontype"));
            _mouseButton = (EMouseState)Enum.Parse(typeof(EMouseState), element.GetAttribute("mousebutton"));

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("mousebuttonactiontype", _mouseButtonActionType.ToString());
            element.SetAttribute("mousebutton", _mouseButton.ToString());
        }

        /// <summary>
        /// Start the action
        /// </summary>
        public override void Start(IStateManager parent, KxSourceEventArgs args)
        {
            MouseManager mouseManager = parent.MouseStateManager;
            switch (_mouseButtonActionType)
            {
                case EMouseButtonActionType.Click:
                case EMouseButtonActionType.DoubleClick:
                case EMouseButtonActionType.PressDown:
                    {
                        // Press mouse button
                        mouseManager.SetButtonState(_mouseButton, true);
                        _isPressed = true;
                        _pressCount = 1;
                        _lastActionTimeTicks = DateTime.UtcNow.Ticks;

                        // Action needs to continue if it's click or double-click
                        IsOngoing = (_mouseButtonActionType != EMouseButtonActionType.PressDown);
                    } 
                    break;
                case EMouseButtonActionType.Release:
                    {
                        // Release button
                        mouseManager.SetButtonState(_mouseButton, false);                        
                        IsOngoing = false;
                    }
                    break;
                case EMouseButtonActionType.Toggle:
                    {
                        // Toggle button
                        mouseManager.ToggleButtonState(_mouseButton);
                        IsOngoing = false;
                    }
                    break;
            }

            // Reset word prediction
            parent.WordPredictionManager.PredictionReset();                        
        }

        /// <summary>
        /// Continue the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void Continue(IStateManager parent, KxSourceEventArgs args)
        {
            long currentTimeTicks = DateTime.UtcNow.Ticks;
            if (currentTimeTicks - _lastActionTimeTicks > parent.MouseStateManager.ClickLengthTicks)
            {
                if (_isPressed)
                {
                    // Release  button
                    parent.MouseStateManager.SetButtonState(_mouseButton, false);
                    if (_pressCount >= _numPressesRequired)
                    {
                        // Finished action
                        IsOngoing = false;
                    }
                }
                else
                {
                    // Press button again
                    parent.MouseStateManager.SetButtonState(_mouseButton, true);
                    _pressCount++;
                }

                _isPressed = !_isPressed;
                _lastActionTimeTicks = currentTimeTicks;
            }
        }

        /// <summary>
        /// Stop the action
        /// </summary>
        /// <param name="parent"></param>
        public override void Deactivate(IStateManager parent, KxSourceEventArgs args)
        {
            // Release any held button
            if (IsOngoing)
            {
                if (_isPressed)
                {
                    parent.MouseStateManager.SetButtonState(_mouseButton, false);
                    _isPressed = false;
                }
                IsOngoing = false;
            }
        }
    }
}
