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

namespace Keysticks.Input
{
    /// <summary>
    /// Base class for input devices
    /// </summary>
    public abstract class BaseInputDevice : NamedItem, IDisposable
    {
        // Fields
        private bool _isRequired;
        private bool _isConnected;
        private bool _isStateChanged;

        // Properties
        public abstract PhysicalInput Capabilities { get; }
        public bool IsRequired { get { return _isRequired; } set { _isRequired = value; } }
        public bool IsConnected { get { return _isConnected; } protected set { _isConnected = value; } }
        public bool IsStateChanged { get { return _isStateChanged; } protected set { _isStateChanged = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseInputDevice()
            :base()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public BaseInputDevice(int id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Acquire the device and initialise the stored device state
        /// </summary>
        /// <returns></returns>
        public abstract void InitialiseState();

        /// <summary>
        /// Update the stored device state
        /// </summary>
        /// <returns></returns>
        public abstract void UpdateState();

        /// <summary>
        /// Get an input value from the stored device state
        /// </summary>
        /// <param name="physicalControl"></param>
        /// <param name="options"></param>
        /// <param name="paramValue"></param>
        public abstract void GetInputValue(PhysicalControl control, ref ParamValue paramValue);
    }
}
