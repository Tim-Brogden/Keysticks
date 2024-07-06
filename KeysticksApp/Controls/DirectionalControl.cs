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
using Keysticks.Core;
using Keysticks.Event;

namespace Keysticks.Controls
{
    /// <summary>
    /// Base class for directional controls
    /// </summary>
    public abstract class DirectionalControl : BaseControl
    {
        // State
        private ELRUDState _oldDirection = ELRUDState.Centre;
        private ELRUDState _direction = ELRUDState.Centre;
        private EDirectionMode _directionMode = Constants.DefaultDirectionMode;
        private List<EDirectionMode> _directionModeStack = new List<EDirectionMode>();
        private TimeoutMonitor _directionTimeoutMonitor;
        private TimeoutMonitor _repeatDirectionTimeoutMonitor;
        private KxSourceEventFactory _directionEventFactory;

        // Properties
        public ELRUDState CurrentDirection { get { return _direction; } protected set { _direction = value; } }
        protected EDirectionMode DirectionMode { get { return _directionMode; } }
        protected TimeoutMonitor DirectionTimeoutMonitor { get { return _directionTimeoutMonitor; } }
        protected TimeoutMonitor RepeatDirectionTimeoutMonitor { get { return _repeatDirectionTimeoutMonitor; } }
        protected override KxSourceEventArgs TemplateEvent
        {
            get
            {
                return new KxSourceEventArgs(Parent.ID,
                                                ControlType,
                                                ID,
                                                EControlSetting.None,
                                                EEventReason.None,
                                                ELRUDState.Centre);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent"></param>
        public DirectionalControl()
            : base()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public DirectionalControl(int id, string name)
        :base(id, name)
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="control"></param>
        public DirectionalControl(DirectionalControl control)
            :base(control)
        {
        }

        /// <summary>
        /// Perform initialisation
        /// </summary>
        protected override void Initialise()
        {
            base.Initialise();

            _directionTimeoutMonitor = new TimeoutMonitor(Constants.DefaultHoldTimeMS);
            _repeatDirectionTimeoutMonitor = new TimeoutMonitor(Constants.DefaultAutoRepeatIntervalMS);
            _directionEventFactory = new KxSourceEventFactory(TemplateEvent);
        }        

        /// <summary>
        /// Apply a direction mode
        /// </summary>
        /// <param name="mode"></param>
        public void ApplyDirectionMode(EDirectionMode mode, bool enable)
        {
            if (enable)
            {
                _directionModeStack.Insert(0, mode);
                _directionMode = mode;
            }
            else if (_directionModeStack.Count != 0)
            {
                _directionModeStack.RemoveAt(0);
                _directionMode = _directionModeStack.Count != 0 ? _directionModeStack[0] : Constants.DefaultDirectionMode;
            }
            //Trace.WriteLine(string.Format("{0}{1}: {2}", _side != ESide.None ? _side.ToString() : "", ControlType, _directionMode));
        }

        /// <summary>
        /// Set the hold time
        /// </summary>
        /// <param name="holdTimeMS"></param>
        public override void ApplyHoldTime(int holdTimeMS, bool enable)
        {
            // Update the stack of applied hold times
            base.ApplyHoldTime(holdTimeMS, enable);

            // Apply the current setting
            _directionTimeoutMonitor.SetTimeout(CurrentHoldTime);
            //Trace.WriteLine(string.Format("{0}{1}: Hold time {2}ms", _side != ESide.None ? _side.ToString() : "", ControlType, holdTimeMS));
        }

        /// <summary>
        /// Configure auto-repeat behaviour
        /// </summary>
        /// <param name="repeatIntervalMS"></param>
        public override void ApplyAutoRepeatInterval(int repeatIntervalMS, bool enable)
        {
            // Update the stack of applied repeat times
            base.ApplyAutoRepeatInterval(repeatIntervalMS, enable);

            // Apply the current setting
            _repeatDirectionTimeoutMonitor.SetTimeout(CurrentRepeatTime);
            //Trace.WriteLine(string.Format("{0}{1}: Repeat time {2}ms", _side != ESide.None ? _side.ToString() : "", ControlType, repeatIntervalMS));
        }

        /// <summary>
        /// Handle new state and generate any events
        /// </summary>
        /// <param name="newState"></param>
        public override void RaiseEvents(IStateManager stateManager)
        {
            // Raise button events if required
            base.RaiseEvents(stateManager);

            // Raise direction events if required
            if (_direction != _oldDirection)
            {
                // Direction changed
                if (IsEventTypeEnabled(EEventReason.DirectedShort) && DirectionTimeoutMonitor.IsStarted())
                {
                    RaiseDirectionEvents(ELRUDState.Centre, _oldDirection, EEventReason.DirectedShort);
                }

                //if (IsEventTypeEnabled(EEventReason.Undirected))
                //{
                // Previous direction released
                // Note: order of arguments
                RaiseDirectionEvents(_direction, _oldDirection, EEventReason.Undirected);
                //}

                //if (IsEventTypeEnabled(EEventReason.Directed))
                //{
                // New direction pressed
                RaiseDirectionEvents(_oldDirection, _direction, EEventReason.Directed);
                //}

                // Changed direction so reset the timers
                if (_direction != ELRUDState.Centre &&
                    (IsEventTypeEnabled(EEventReason.DirectedShort) || IsEventTypeEnabled(EEventReason.DirectedLong) || IsEventTypeEnabled(EEventReason.DirectionRepeated)))
                {
                    DirectionTimeoutMonitor.Start();
                }
                else
                {
                    DirectionTimeoutMonitor.Stop();
                }
                RepeatDirectionTimeoutMonitor.Stop();

                _oldDirection = _direction;
            }
            // No change in direction
            // See if a dwell or repeat event is due
            else if (DirectionTimeoutMonitor.IsTimeout())
            {
                //Trace.WriteLine("Dwell occurred");
                // Only want to dwell once, so stop the dwell timer and start the repeat timer if reqd
                DirectionTimeoutMonitor.Stop();

                // Dwell occurred
                if (IsEventTypeEnabled(EEventReason.DirectedLong))
                {
                    //Trace.WriteLine("Do dwell");
                    RaiseDirectionEvents(ELRUDState.Centre, _direction, EEventReason.DirectedLong);
                }

                if (IsEventTypeEnabled(EEventReason.DirectionRepeated))
                {
                    // Raise the first repeat event when the dwell occurs (so users don't have to assign actions to Dwell onerously)
                    //Trace.WriteLine("Do 1st repeat");
                    RaiseDirectionEvents(ELRUDState.Centre, _direction, EEventReason.DirectionRepeated);

                    // Start the timer to trigger further repeats
                    RepeatDirectionTimeoutMonitor.Start();
                }
            }
            else if (RepeatDirectionTimeoutMonitor.IsTimeout())
            {
                // Repeat occurred
                //Trace.WriteLine("Do repeat");
                RaiseDirectionEvents(ELRUDState.Centre, _direction, EEventReason.DirectionRepeated);
            }
        }

        /// <summary>
        /// Raise direction events
        /// </summary>
        /// <param name="fromDirection"></param>
        /// <param name="toDirection"></param>
        /// <param name="reason"></param>
        private void RaiseDirectionEvents(ELRUDState fromDirection, ELRUDState toDirection, EEventReason reason)
        {
            KxSourceEventArgs args;
            if (_directionMode == EDirectionMode.AxisStyle)
            {
                // Axis-style
                // X axis (left-right)
                if ((toDirection & ELRUDState.Left) != 0 && (fromDirection & ELRUDState.Left) == 0)
                {
                    args = _directionEventFactory.Create();
                    args.LRUDState = ELRUDState.Left;
                    args.EventReason = reason;
                    RaiseInputEvent(args);
                }
                else if ((toDirection & ELRUDState.Right) != 0 && (fromDirection & ELRUDState.Right) == 0)
                {
                    args = _directionEventFactory.Create();
                    args.LRUDState = ELRUDState.Right;
                    args.EventReason = reason;
                    RaiseInputEvent(args);
                }

                // Y axis (up-down)
                if ((toDirection & ELRUDState.Up) != 0 && (fromDirection & ELRUDState.Up) == 0)
                {
                    args = _directionEventFactory.Create();
                    args.LRUDState = ELRUDState.Up;
                    args.EventReason = reason;
                    RaiseInputEvent(args);
                }
                else if ((toDirection & ELRUDState.Down) != 0 && (fromDirection & ELRUDState.Down) == 0)
                {
                    args = _directionEventFactory.Create();
                    args.LRUDState = ELRUDState.Down;
                    args.EventReason = reason;
                    RaiseInputEvent(args);
                }
            }
            else if (_directionMode != EDirectionMode.Continuous || toDirection == ELRUDState.Centre)
            {
                // Eight-way or four-way
                args = _directionEventFactory.Create();
                args.LRUDState = toDirection;
                args.EventReason = reason;
                RaiseInputEvent(args);
            }
        }

    }
}
