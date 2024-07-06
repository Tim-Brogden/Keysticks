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
using Keysticks.Core;
using Keysticks.Event;

namespace Keysticks.Sys
{
    /// <summary>
    /// Manages mouse state
    /// </summary>
    public class MouseManager
    {
        private bool[] _buttonsWePressed = new bool[32];  // Enough for any possible EMouseState value
        private EMouseState _mouseButtonState = EMouseState.None;
        private StateManager _parent;
        private long _clickLengthTicks = Constants.DefaultMouseClickLengthMS * TimeSpan.TicksPerMillisecond;
        private float _pointerSpeed = Constants.DefaultMousePointerSpeed;
        private float _pointerAcceleration = Constants.DefaultMousePointerAcceleration;
        private bool _isPositionedOffCentre = false;   // State variable for preventing jumps when switching between pointer control actions

        public long ClickLengthTicks { get { return _clickLengthTicks; } set { _clickLengthTicks = value; } }
        public float PointerSpeed { get { return _pointerSpeed; } set { _pointerSpeed = value; } }
        public float PointerAcceleration { get { return _pointerAcceleration; } set { _pointerAcceleration = value; } }
        public bool IsPositionedOffCentre { get { return _isPositionedOffCentre; } set { _isPositionedOffCentre = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public MouseManager(StateManager parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Release any mouse buttons that are held
        /// </summary>
        public void ReleaseAll()
        {
            // Release any buttons we pressed
            foreach (EMouseState button in Enum.GetValues(typeof(EMouseState)))
            {
                if (button != EMouseState.None)
                {
                    SetButtonState(button, false);
                }
            }
        }

        /// <summary>
        /// Press down a button if it's not pressed, and release it if it is pressed
        /// </summary>
        /// <param name="eButton"></param>
        public void ToggleButtonState(EMouseState eButton)
        {
            SetButtonState(eButton, !_buttonsWePressed[(int)eButton]);
        }

        /// <summary>
        /// Emulate a mouse button down or up using Windows API
        /// </summary>
        public void SetButtonState(EMouseState eButton, bool buttonDown)
        {
            // Check button is not already in required state
            if (_buttonsWePressed[(int)eButton] != buttonDown)     // Whether we think the button is up/down                               
            {
                INPUT[] inputs = new INPUT[1];
                inputs[0].type = WindowsAPI.INPUT_MOUSE;
                switch (eButton)
                {
                    case EMouseState.Left:
                        inputs[0].mi.dwFlags = buttonDown ? WindowsAPI.MOUSEEVENTF_LEFTDOWN : WindowsAPI.MOUSEEVENTF_LEFTUP;
                        break;
                    case EMouseState.Middle:
                        inputs[0].mi.dwFlags = buttonDown ? WindowsAPI.MOUSEEVENTF_MIDDLEDOWN : WindowsAPI.MOUSEEVENTF_MIDDLEUP;
                        break;
                    case EMouseState.Right:
                        inputs[0].mi.dwFlags = buttonDown ? WindowsAPI.MOUSEEVENTF_RIGHTDOWN : WindowsAPI.MOUSEEVENTF_RIGHTUP;
                        break;
                    case EMouseState.X1:
                        inputs[0].mi.dwFlags = buttonDown ? WindowsAPI.MOUSEEVENTF_XDOWN : WindowsAPI.MOUSEEVENTF_XUP;
                        inputs[0].mi.mouseData = WindowsAPI.XBUTTON1;
                        break;
                    case EMouseState.X2:
                        inputs[0].mi.dwFlags = buttonDown ? WindowsAPI.MOUSEEVENTF_XDOWN : WindowsAPI.MOUSEEVENTF_XUP;
                        inputs[0].mi.mouseData = WindowsAPI.XBUTTON2;
                        break;
                }
                WindowsAPI.SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));

                // Update mouse button state
                _mouseButtonState |= eButton;
                if (!buttonDown)
                {
                    _mouseButtonState ^= eButton;
                }

                // Report mouse button state to UI
                KxMouseButtonStateEventArgs args = new KxMouseButtonStateEventArgs(_mouseButtonState);
                _parent.ThreadManager.SubmitUIEvent(args);

                // Store the key state so that we know which buttons we pressed
                _buttonsWePressed[(int)eButton] = buttonDown;
            }
        }

        /// <summary>
        /// Emulate a mouse wheel up or down
        /// </summary>
        /// <param name="isUpDirection"></param>
        public void ChangeMouseWheel(bool isUpDirection)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = WindowsAPI.INPUT_MOUSE;
            inputs[0].mi.dwFlags = WindowsAPI.MOUSEEVENTF_WHEEL;
            inputs[0].mi.mouseData = (uint)(isUpDirection ? WindowsAPI.WHEEL_DELTA : -WindowsAPI.WHEEL_DELTA);
            WindowsAPI.SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        }

        /// <summary>
        /// Move the mouse pointer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MoveMouse(int x, int y, bool absolute)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = WindowsAPI.INPUT_MOUSE;
            inputs[0].mi.dwFlags = WindowsAPI.MOUSEEVENTF_MOVE;
            if (absolute)
            {
                inputs[0].mi.dwFlags |= WindowsAPI.MOUSEEVENTF_ABSOLUTE;
            }
            inputs[0].mi.dx = x;            
            inputs[0].mi.dy = y;
            WindowsAPI.SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        }
    }
}
