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
using System.Xml;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Sources;
using Keysticks.Input;
using Keysticks.Event;
//using System.Diagnostics;

namespace Keysticks.Controls
{
    /// <summary>
    /// Base class for pressable input controls
    /// </summary>
    public abstract class BaseControl : NamedItem
    {
        // Config
        private BaseSource _parent;
        private InputMapping[] _inputMappings = new InputMapping[0];
        private List<int> _holdTimeStack = new List<int>();
        private List<int> _repeatTimeStack = new List<int>();

        // State
        private bool _isActive;
        private int _totalActionSets;
        private int[] _actionSetsByEventType;
        private PhysicalControl[] _boundControls = new PhysicalControl[0];
        private bool _oldIsPressed;
        private bool _isPressed;
        private TimeoutMonitor _pressTimeoutMonitor;
        private TimeoutMonitor _repeatPressTimeoutMonitor;
        private KxSourceEventFactory _eventFactory;

        // Properties
        public abstract EVirtualControlType ControlType { get; }
        public BaseSource Parent { get { return _parent; } set { _parent = value; } }
        protected InputMapping[] InputMappings { get { return _inputMappings; } }
        protected PhysicalControl[] BoundControls { get { return _boundControls; } }
        public bool IsActive { get { return _isActive; } }
        public bool IsPressed { get { return _isPressed; } protected set { _isPressed = value; } }
        protected TimeoutMonitor PressTimeoutMonitor { get { return _pressTimeoutMonitor; } }
        protected TimeoutMonitor RepeatPressTimeoutMonitor { get { return _repeatPressTimeoutMonitor; } }
        public int NumInputMappings { get { return _inputMappings != null ? _inputMappings.Length : 0; } }
        protected int CurrentHoldTime { get { return _holdTimeStack.Count != 0 ? _holdTimeStack[0] : Constants.DefaultHoldTimeMS; } }
        protected int CurrentRepeatTime { get { return _repeatTimeStack.Count != 0 ? _repeatTimeStack[0] : Constants.DefaultAutoRepeatIntervalMS; } }
        protected virtual bool IsPressableControl { get { return false; } }
        protected virtual KxSourceEventArgs TemplateEvent
        {
            get
            {
                return new KxSourceEventArgs(Parent.ID,
                                                ControlType,
                                                ID,
                                                EControlSetting.None,
                                                EEventReason.None,
                                                ELRUDState.None);
            }
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent"></param>
        public BaseControl()
            :base()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public BaseControl(int id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="control"></param>
        public BaseControl(BaseControl control)
            : base(control)
        {
            if (control._inputMappings != null)
            {
                _inputMappings = new InputMapping[control._inputMappings.Length];
                for (int i=0; i<control._inputMappings.Length; i++)
                {
                    _inputMappings[i] = new InputMapping(control._inputMappings[i]);
                }
            }
        }

        /// <summary>
        /// Set the parent source and initialise the control so that it can raise events
        /// </summary>
        /// <param name="parent"></param>
        public void SetParent(BaseSource parent)
        {
            _parent = parent;
            Initialise();
        }

        /// <summary>
        /// Set the mappings to physical controls
        /// </summary>
        /// <param name="inputMappings"></param>
        public void SetInputMappings(InputMapping[] inputMappings)
        {
            _inputMappings = inputMappings;
        }

        /// <summary>
        /// Get the specified input mapping
        /// </summary>
        /// <param name="index"></param>
        public InputMapping GetInputMapping(int index)
        {
            InputMapping mapping = null;
            if (index > -1 && index < _inputMappings.Length)
            {
                mapping = _inputMappings[index];
            }

            return mapping;
        }
       
        /// <summary>
        /// Bind the virtual control to its physical inputs
        /// </summary>
        /// <param name="boundControls"></param>
        public virtual void SetBoundControls(PhysicalControl[] boundControls)
        {
            _boundControls = boundControls;
        }

        /// <summary>
        /// Perform any initialisation
        /// </summary>
        protected virtual void Initialise()
        {
            _totalActionSets = 0;
            _actionSetsByEventType = new int[Enum.GetValues(typeof(EEventReason)).Length];            
            _pressTimeoutMonitor = new TimeoutMonitor(Constants.DefaultHoldTimeMS);
            _repeatPressTimeoutMonitor = new TimeoutMonitor(Constants.DefaultAutoRepeatIntervalMS);
            _eventFactory = new KxSourceEventFactory(TemplateEvent);
        }

        /// <summary>
        /// Set the hold time
        /// </summary>
        /// <param name="holdTimeMS"></param>
        public virtual void ApplyHoldTime(int holdTimeMS, bool enable)
        {
            // Update the stack of applied hold times
            if (enable)
            {
                _holdTimeStack.Insert(0, holdTimeMS);
            }
            else if (_holdTimeStack.Count != 0)
            {
                _holdTimeStack.RemoveAt(0);
            }

            // Apply the current setting
            _pressTimeoutMonitor.SetTimeout(CurrentHoldTime);
            //Trace.WriteLine(string.Format("{0}{1}: Hold time {2}ms", _side != ESide.None ? _side.ToString() : "", ControlType, holdTimeMS));
        }

        /// <summary>
        /// Configure auto-repeat behaviour
        /// </summary>
        /// <param name="repeatIntervalMS"></param>
        public virtual void ApplyAutoRepeatInterval(int repeatIntervalMS, bool enable)
        {
            // Update the stack of applied repeat times
            if (enable)
            {
                _repeatTimeStack.Insert(0, repeatIntervalMS);
            }
            else if (_repeatTimeStack.Count != 0)
            {
                _repeatTimeStack.RemoveAt(0);
            }

            // Apply the current setting
            _repeatPressTimeoutMonitor.SetTimeout(CurrentRepeatTime);
            //Trace.WriteLine(string.Format("{0}{1}: Repeat time {2}ms", _side != ESide.None ? _side.ToString() : "", ControlType, repeatIntervalMS));
        }
        
        /// <summary>
        /// Read from Xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            base.FromXml(element);

            XmlNodeList mappingElementList = element.SelectNodes("mappings/mapping");
            _inputMappings = new InputMapping[mappingElementList.Count];
            int i=0;
            foreach (XmlElement mappingElement in mappingElementList)
            {
                InputMapping mapping = new InputMapping();
                mapping.FromXml(mappingElement);
                _inputMappings[i++] = mapping;
            }
        }

        /// <summary>
        /// Write to Xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            if (_inputMappings != null)
            {
                XmlElement mappingsElement = doc.CreateElement("mappings");
                foreach (InputMapping mapping in _inputMappings)
                {
                    XmlElement mappingElement = doc.CreateElement("mapping");
                    mapping.ToXml(mappingElement, doc);
                    mappingsElement.AppendChild(mappingElement);
                }
                element.AppendChild(mappingsElement);
            }
        }


        /// <summary>
        /// Handle a change of app config
        /// </summary>
        /// <param name="appConfig"></param>
        public virtual void SetAppConfig(AppConfig appConfig)
        {
        }

        /// <summary>
        /// Turn event handling on or off for a type of event
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="handler"></param>
        /// <param name="enable"></param>
        public void EnableInputEvents(ActionSet actionSet, bool enable)
        {
            // Count number of times each event type is required
            foreach (ActionList actionList in actionSet.ActionLists)
            {
                if (enable)
                {                    
                    _actionSetsByEventType[(int)actionList.Reason]++;
                    _totalActionSets++;
                }
                else
                {
                    _actionSetsByEventType[(int)actionList.Reason]--;
                    _totalActionSets--;
                }
            }

            //if (_isActive != (_totalActionSets != 0))
            //{
            //    Trace.WriteLine(string.Format("{0}: active={1}", ControlType, !_isActive));
            //}

            _isActive = _totalActionSets != 0;
        }

        /// <summary>
        /// Return whether or not a certain type of event should be raised
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        protected bool IsEventTypeEnabled(EEventReason reason)
        {
            return _actionSetsByEventType[(int)reason] != 0;
        }

        /// <summary>
        /// Return which types of event the control can raise
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual List<EEventReason> GetSupportedEventReasons(KxControlEventArgs args/*, EDirectionMode directionMode*/)
        {
            return new List<EEventReason>();
        }

        /// <summary>
        /// Return whether the control can raise the specified event type
        /// </summary>
        /// <param name="args"></param>
        /// <param name="directionMode"></param>
        /// <returns></returns>
        public virtual bool IsReasonSupported(KxControlEventArgs args, /*EDirectionMode directionMode,*/ EEventReason reason)
        {
            return false;
        }

        /// <summary>
        /// Handle an input event
        /// </summary>
        /// <param name="args"></param>
        protected void RaiseInputEvent(KxSourceEventArgs args)
        {
            _parent.HandleInputEvent(this, args);
        }

        /// <summary>
        /// Update the control's state
        /// </summary>
        /// <param name="newState"></param>
        public virtual void UpdateState()
        {
        }

        /// <summary>
        /// Raise events when the control's state changes
        /// </summary>
        /// <param name="newState"></param>
        public virtual void RaiseEvents(IStateManager stateManager)
        {
            if (!IsPressableControl)
            {
                // Not a button control
                return;
            }

            // Update the button states if they have changed
            KxSourceEventArgs args;
            if (_isPressed != _oldIsPressed)
            {
                if (_isPressed)
                {
                    // Button pressed
                    //if (IsEventTypeEnabled(EEventReason.Pressed))
                    //{
                    args = _eventFactory.Create();
                    args.EventReason = EEventReason.Pressed;
                    RaiseInputEvent(args);
                    //}

                    // Start dwell monitoring
                    if (IsEventTypeEnabled(EEventReason.PressedShort) || IsEventTypeEnabled(EEventReason.PressedLong) || IsEventTypeEnabled(EEventReason.PressRepeated))
                    {
                        PressTimeoutMonitor.Start();
                    }
                    else
                    {
                        PressTimeoutMonitor.Stop();
                    }
                }
                else
                {
                    // Button released
                    if (IsEventTypeEnabled(EEventReason.PressedShort) && PressTimeoutMonitor.IsStarted())
                    {
                        // Dwell timer still running, so raise a short press event
                        args = _eventFactory.Create();
                        args.EventReason = EEventReason.PressedShort;
                        RaiseInputEvent(args);
                    }

                    //if (IsEventTypeEnabled(EEventReason.Released))
                    //{
                    args = _eventFactory.Create();
                    args.EventReason = EEventReason.Released;
                    RaiseInputEvent(args);
                    //}

                    PressTimeoutMonitor.Stop();
                }

                // Stop the auto-repeat timer
                RepeatPressTimeoutMonitor.Stop();

                _oldIsPressed = _isPressed;
            }
            // See if a dwell or repeat event is due
            else if (PressTimeoutMonitor.IsTimeout())
            {
                //Trace.WriteLine("Dwell occurred");
                // Only want to dwell once, so stop the dwell timer and start the repeat timer if reqd
                PressTimeoutMonitor.Stop();

                // Dwell occurred
                if (IsEventTypeEnabled(EEventReason.PressedLong))
                {
                    //Trace.WriteLine("Do dwell");
                    args = _eventFactory.Create();
                    args.EventReason = EEventReason.PressedLong;
                    RaiseInputEvent(args);
                }

                if (IsEventTypeEnabled(EEventReason.PressRepeated))
                {
                    // Raise the first repeat event when the dwell occurs (so users don't have to assign actions to Dwell onerously)
                    //Trace.WriteLine("Do 1st repeat");
                    args = _eventFactory.Create();
                    args.EventReason = EEventReason.PressRepeated;
                    RaiseInputEvent(args);

                    // Start the timer to trigger further repeats
                    RepeatPressTimeoutMonitor.Start();
                }
            }
            else if (RepeatPressTimeoutMonitor.IsTimeout())
            {
                // Repeat occurred
                //Trace.WriteLine("Do repeat");
                args = _eventFactory.Create();
                args.EventReason = EEventReason.PressRepeated;
                RaiseInputEvent(args);
            }
        }
    }
}
