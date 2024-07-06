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
using SlimDX.DirectInput;
using Keysticks.Core;

namespace Keysticks.Input
{
    /// <summary>
    /// Reads an axis value
    /// </summary>
    public class SlimAxisReader
    {
        // Fields
        private EAxis _axis;
        private int _min;
        private int _max;
        private int _deadZone;
        private float _halfRange;
        private int _midPoint;

        // Properties
        public EAxis Axis { get { return _axis; } }
        public int Min { get { return _min; } }
        public int Max { get { return _max; } }
        public int DeadZone { get { return _deadZone; } }
        public float HalfRange { get { return _halfRange; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="axis"></param>
        public SlimAxisReader(EAxis axis, int min, int max, int deadZone)
        {
            _axis = axis;
            _min = min;
            _max = max;
            _deadZone = deadZone;

            _midPoint = (_min + _max) / 2;
            _halfRange = Math.Max(1, Math.Abs(_max - _midPoint));
        }

        /// <summary>
        /// Get the axis value (-1 to +1 range)
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public float GetValue(JoystickState state)
        {
            // Note: Y axes are inverted by default
            int val = 0;
            switch (_axis)
            {
                case EAxis.X1: val = state.X - _midPoint; break;
                case EAxis.Y1: val = _midPoint - state.Y; break;
                case EAxis.Z1: val = state.Z - _midPoint; break;
                case EAxis.X2: val = state.RotationX - _midPoint; break;
                case EAxis.Y2: val = _midPoint - state.RotationY; break;
                case EAxis.Z2: val = state.RotationZ - _midPoint; break;
                case EAxis.X3: val = state.VelocityX - _midPoint; break;
                case EAxis.Y3: val = _midPoint - state.VelocityY; break;
                case EAxis.Z3: val = state.VelocityZ - _midPoint; break;
                case EAxis.X4: val = state.AngularVelocityX - _midPoint; break;
                case EAxis.Y4: val = _midPoint - state.AngularVelocityY; break;
                case EAxis.Z4: val = state.AngularVelocityZ - _midPoint; break;
                case EAxis.X5: val = state.AccelerationX - _midPoint; break;
                case EAxis.Y5: val = _midPoint - state.AccelerationY; break;
                case EAxis.Z5: val = state.AccelerationZ - _midPoint; break;
                case EAxis.X6: val = state.AngularAccelerationX - _midPoint; break;
                case EAxis.Y6: val = _midPoint - state.AngularAccelerationY; break;
                case EAxis.Z6: val = state.AngularAccelerationZ - _midPoint; break;
                case EAxis.X7: val = state.ForceX - _midPoint; break;
                case EAxis.Y7: val = _midPoint - state.ForceY; break;
                case EAxis.Z7: val = state.ForceZ - _midPoint; break;
                case EAxis.X8: val = state.TorqueX - _midPoint; break;
                case EAxis.Y8: val = _midPoint - state.TorqueY; break;
                case EAxis.Z8: val = state.TorqueZ - _midPoint; break;
            }

            return val / _halfRange;
        }
    }
}
