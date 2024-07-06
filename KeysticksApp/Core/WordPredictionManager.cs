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
using Keysticks.Event;

namespace Keysticks.Core
{
    /// <summary>
    /// Manages the word prediction state
    /// </summary>
    public class WordPredictionManager
    {
        // Fields
        private IStateManager _parent;
        private VirtualKeyData[] _virtualKeyData;
        private bool _predictionEnabled;
        private int _charsSent;
        private int _cursorPos;
        private bool _ignoreCurrentWord;

        /// <summary>
        /// Constructor
        /// </summary>
        public WordPredictionManager(IStateManager parent)
        {
            _parent = parent;
            _predictionEnabled = false;
            _charsSent = 0;
            _cursorPos = 0;
            _ignoreCurrentWord = false;

            KeyboardLayoutChanged(parent.KeyboardContext);
        }

        /// <summary>
        /// Initialise or re-initialise the keyboard layout
        /// </summary>
        public void KeyboardLayoutChanged(IKeyboardContext context)
        {
            _virtualKeyData = KeyUtils.GetVirtualKeysByKeyCode(context.KeyboardHKL);
        }

        /// <summary>
        /// Handle a key event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleKeyEvent(KxKeyEventArgs args)
        {
            if (_predictionEnabled && args.EventReason == EEventReason.Pressed)
            {
                HandleKeyDown(args.Key);
            }
        }

        /// <summary>
        /// Handle a repeat key event
        /// </summary>
        /// <param name="args"></param>
        public void HandleRepeatKeyEvent(KxRepeatKeyEventArgs args)
        {
            if (_predictionEnabled)
            {
                // Only these repeat key presses have meaning to word prediction
                switch (args.Key)
                {
                    case System.Windows.Forms.Keys.Back:
                        PredictionSendBackspace(args.Count);
                        break;
                    case System.Windows.Forms.Keys.Delete:
                        PredictionSendDelete(args.Count);
                        break;
                    case System.Windows.Forms.Keys.Left:
                        PredictionSendLeftCursor(args.Count);
                        break;
                    case System.Windows.Forms.Keys.Right:
                        PredictionSendRightCursor(args.Count);
                        break;
                }
            }
        }

        /// <summary>
        /// Handle a word prediction event
        /// </summary>
        /// <param name="report"></param>
        public void HandlePredictionEvent(KxPredictionEventArgs args)
        {
            if (args.PredictionEventType == EWordPredictionEventType.Enable)
            {
                EnablePrediction(true);
            }
            else if (args.PredictionEventType == EWordPredictionEventType.Disable)
            {
                EnablePrediction(false);
            }
        }

        /// <summary>
        /// Enable or disable word prediction
        /// </summary>
        /// <param name="enable"></param>
        private void EnablePrediction(bool enable)
        {
            if (enable)
            {
                _predictionEnabled = true;
                
                // Reset state
                _parent.ThreadManager.SubmitPredictionEvent(new KxKeyEventArgs(System.Windows.Forms.Keys.None, EEventReason.Pressed, true));
                _charsSent = 0;
                _cursorPos = 0;
                _ignoreCurrentWord = false;
            }
            else
            {
                _predictionEnabled = false;
            }
        }

        /// <summary>
        /// Handle a text event
        /// </summary>
        /// <param name="args"></param>
        public void HandleTextEvent(KxTextEventArgs args)
        {
            if (_predictionEnabled)
            {
                if (!_ignoreCurrentWord)
                {
                    PredictionSendString(args.Text);
                }
                else
                {
                    foreach (char ch in args.Text)
                    {
                        if (char.IsWhiteSpace(ch))
                        {
                            _parent.ThreadManager.SubmitPredictionEvent(new KxKeyEventArgs(System.Windows.Forms.Keys.None, EEventReason.Pressed, true));
                            _charsSent = 0;
                            _cursorPos = 0;
                            _ignoreCurrentWord = false;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Don't provide predictions for the current word
        /// </summary>
        public void CancelSuggestions()
        {
            if (_predictionEnabled)
            {
                _ignoreCurrentWord = true;

                // Send a cancel event to the UI
                _parent.ThreadManager.SubmitUIEvent(new KxPredictionEventArgs(EWordPredictionEventType.CancelSuggestions));
            }
        }
        
        /// <summary>
        /// Handle keys relevant to word prediction e.g. cursor keys
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        private void HandleKeyDown(System.Windows.Forms.Keys keyCode)
        {
            // Certain keys have special meaning for word prediction, but only when unmodified
            bool handled = false;
            EModifierKeyStates modifierState = _parent.KeyStateManager.ModifierKeyStates;
            if (modifierState == EModifierKeyStates.None)
            {
                // Unmodified special key
                handled = true;
                switch (keyCode)
                {
                    case System.Windows.Forms.Keys.Left:
                        PredictionSendLeftCursor(1);
                        break;
                    case System.Windows.Forms.Keys.Right:
                        PredictionSendRightCursor(1);
                        break;
                    case System.Windows.Forms.Keys.Space:
                        if (!_ignoreCurrentWord)
                        {
                            PredictionSendString(" ");
                        }
                        else
                        {
                            _parent.ThreadManager.SubmitPredictionEvent(new KxKeyEventArgs(System.Windows.Forms.Keys.None, EEventReason.Pressed, true));
                            _charsSent = 0;
                            _cursorPos = 0;
                            _ignoreCurrentWord = false;
                        }
                        break;
                    case System.Windows.Forms.Keys.Back:
                        PredictionSendBackspace(1);
                        break;
                    case System.Windows.Forms.Keys.Delete:
                        PredictionSendDelete(1);
                        break;
                    case System.Windows.Forms.Keys.Tab:
                        if (!_ignoreCurrentWord)
                        {
                            PredictionSendString("\t");
                        }
                        else
                        {
                            _parent.ThreadManager.SubmitPredictionEvent(new KxKeyEventArgs(System.Windows.Forms.Keys.None, EEventReason.Pressed, true));
                            _charsSent = 0;
                            _cursorPos = 0;
                            _ignoreCurrentWord = false;
                        }                        
                        break;                    
                    default:
                        handled = false;
                        break;                    
                }
            }

            if (!_ignoreCurrentWord && !handled)
            {
                switch (keyCode)
                {
                    case System.Windows.Forms.Keys.ShiftKey:
                    case System.Windows.Forms.Keys.ControlKey:
                    case System.Windows.Forms.Keys.Menu:
                    case System.Windows.Forms.Keys.LShiftKey:
                    case System.Windows.Forms.Keys.RShiftKey:
                    case System.Windows.Forms.Keys.LControlKey:
                    case System.Windows.Forms.Keys.RControlKey:
                    case System.Windows.Forms.Keys.LMenu:
                    case System.Windows.Forms.Keys.RMenu:
                    case System.Windows.Forms.Keys.LWin:
                    case System.Windows.Forms.Keys.RWin:
                        // Ignore modifier keys
                        break;
                    default:
                        // See if it's an OEM key or key combination
                        VirtualKeyData vk = _virtualKeyData[(byte)keyCode];
                        if (vk != null && vk.OemKeyCombinations.ContainsKey(modifierState))
                        {
                            PredictionSendString(vk.OemKeyCombinations[modifierState]);
                        }
                        else if (vk != null && vk.WindowsScanCode != 0)
                        {
                            // If it's another physical key (i.e. not something like volume up), reset word prediction
                            PredictionReset();
                        }                        
                        break;         
                }
            }
        }

        /// <summary>
        /// Send a reset to the word prediction manager
        /// </summary>
        public void PredictionReset()
        {
            // Only send reset if some characters have been entered
            if (_charsSent != 0)
            {
                //Trace.WriteLine("Reset");
                _parent.ThreadManager.SubmitPredictionEvent(new KxKeyEventArgs(System.Windows.Forms.Keys.None, EEventReason.Pressed, true));
                _charsSent = 0;
                _cursorPos = 0;
            }
            _ignoreCurrentWord = false;            
        }

        /// <summary>
        /// Send a character string to the word prediction manager
        /// </summary>
        /// <param name="ch"></param>
        private void PredictionSendString(string str)
        {
            //Trace.WriteLine("Sent " + str);
            _parent.ThreadManager.SubmitPredictionEvent(new KxTextEventArgs(string.Copy(str)));
            _charsSent += str.Length;
            _cursorPos += str.Length;          
        }

        /// <summary>
        /// Send backspaces to the word prediction manager
        /// </summary>
        private void PredictionSendBackspace(uint count)
        {
            if (!_ignoreCurrentWord)
            {
                if (_cursorPos > 0)
                {
                    uint numPlaces = Math.Min((uint)_cursorPos, count);
                    _parent.ThreadManager.SubmitPredictionEvent(new KxRepeatKeyEventArgs(System.Windows.Forms.Keys.Back, numPlaces));
                    _charsSent -= (int)numPlaces;
                    _cursorPos -= (int)numPlaces;
                }
            }
        }

        /// <summary>
        /// Send deletes to the word prediction manager
        /// </summary>
        private void PredictionSendDelete(uint count)
        {
            if (!_ignoreCurrentWord)
            {
                //Trace.WriteLine("Delete");
                if (_cursorPos < _charsSent)
                {
                    uint numPlaces = Math.Min((uint)(_charsSent - _cursorPos), count);
                    _parent.ThreadManager.SubmitPredictionEvent(new KxRepeatKeyEventArgs(System.Windows.Forms.Keys.Delete, (uint)numPlaces));
                    _charsSent -= (int)numPlaces;
                }
            }
        }

        /// <summary>
        /// Send left cursors to the word prediction manager
        /// </summary>
        private void PredictionSendLeftCursor(uint count)
        {
            if (!_ignoreCurrentWord)
            {
                if (_cursorPos >= count)
                {
                    //Trace.WriteLine("Left");
                    _parent.ThreadManager.SubmitPredictionEvent(new KxRepeatKeyEventArgs(System.Windows.Forms.Keys.Left, count));
                    _cursorPos -= (int)count;
                }
                else
                {
                    PredictionReset();
                }
            }
        }

        /// <summary>
        /// Send right cursors to the word prediction manager
        /// </summary>
        private void PredictionSendRightCursor(uint count)
        {
            if (!_ignoreCurrentWord)
            {
                if (_cursorPos < _charsSent && count <= _charsSent - _cursorPos)
                {
                    //Trace.WriteLine("Right");
                    _parent.ThreadManager.SubmitPredictionEvent(new KxRepeatKeyEventArgs(System.Windows.Forms.Keys.Right, count));
                    _cursorPos += (int)count;
                }
                else
                {
                    PredictionReset();
                }
            }
        }
       
    }
}
