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

namespace Keysticks.Event
{
    /// <summary>
    /// Buffer for exchanging events between threads
    /// </summary>
    public class EventReportingBuffer
    {
        // Fields
        private const int _maxEventsInBuffer = 1000;
        private List<KxEventArgs> _pendingEvents;
        private List<KxEventArgs> _receivedEvents;
        private bool _eventsPending = false;
        private Object _lockObject = new object();

        /// <summary>
        /// Constructor
        /// </summary>
        public EventReportingBuffer()
        {
            _pendingEvents = new List<KxEventArgs>();
            _receivedEvents = new List<KxEventArgs>();
        }

        /// <summary>
        /// Submit an event to another thread
        /// </summary>
        /// <param name="args"></param>
        public void SubmitEvent(KxEventArgs eventReport)
        {
            lock (_lockObject)
            {
                _pendingEvents.Add(eventReport);

                // Check buffer isn't overflowing
                if (_pendingEvents.Count == _maxEventsInBuffer)
                {
                    // Remove half of max capacity
                    _pendingEvents.RemoveRange(0, _maxEventsInBuffer >> 1);
                }

                _eventsPending = true;
            }
        }

        /// <summary>
        /// Allow the state manager to check for any events submitted from the UI thread
        /// </summary>
        /// <param name="stateHandler"></param>
        public List<KxEventArgs> ReceiveEvents()
        {
            if (_eventsPending)
            {
                // Clear previous submissions
                _receivedEvents.Clear();

                // Switch buffers
                lock (_lockObject)
                {
                    List<KxEventArgs> temp = _pendingEvents;
                    _pendingEvents = _receivedEvents;
                    _receivedEvents = temp;
                    _eventsPending = false;
                }

                return _receivedEvents;
            }
            else
            {
                return null;
            }
        }
        
    }
}
