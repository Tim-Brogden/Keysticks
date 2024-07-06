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

namespace Keysticks.Core
{
    /// <summary>
    /// Reports when a timeout has occurred
    /// </summary>
    public class TimeoutMonitor
    {
        // Fields
        private int _timeoutIntervalMS;
        private DateTime _startTime;
        private DateTime _endTime;
        private bool _isStarted;

        /// <summary>
        /// Constructor
        /// </summary>
        public TimeoutMonitor()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="timeoutIntervalMS"></param>
        public TimeoutMonitor(int timeoutIntervalMS)
        {
            SetTimeout(timeoutIntervalMS);
        }

        /// <summary>
        /// Configure the timeout
        /// </summary>
        /// <param name="timeoutIntervalMS"></param>
        public void SetTimeout(int timeoutIntervalMS)
        {
            _timeoutIntervalMS = timeoutIntervalMS;
            if (_isStarted)
            {
                _endTime = _startTime.AddMilliseconds(timeoutIntervalMS);    // Update in case timer is running
            }
        }

        /// <summary>
        /// Start the timer
        /// </summary>
        /// <param name="currentTimeTicks"></param>
        public void Start()
        {
            _startTime = DateTime.UtcNow;
            _endTime = _startTime.AddMilliseconds(_timeoutIntervalMS);
            _isStarted = true;
        }

        /// <summary>
        /// Stop the timer
        /// </summary>
        public void Stop()
        {
            _isStarted = false;
        }

        /// <summary>
        /// Whether the timer is running
        /// </summary>
        /// <returns></returns>
        public bool IsStarted()
        {
            return _isStarted;
        }

        /// <summary>
        /// Check whether a timeout has occurred
        /// </summary>
        /// <returns>Whether a timeout has occurred</returns>
        public bool IsTimeout()
        {
            bool isTimeout = false;
            if (_isStarted && DateTime.UtcNow > _endTime)
            {
                isTimeout = true;
                Start();   // Reset the timer
            }

            return isTimeout;
        }
    }
}
