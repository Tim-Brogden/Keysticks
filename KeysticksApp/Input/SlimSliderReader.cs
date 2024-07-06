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
    /// Reads a slider value
    /// </summary>
    public class SlimSliderReader
    {
        // Fields
        private ESliderType _sliderType;
        private int _sliderTypeIndex;
        private int _min;
        private int _max;
        private int _deadZone;
        private float _range;
        private bool _initialised;
        private bool _hasChanged;
        private int _initialValue;

        // Properties
        public ESliderType SliderType { get { return _sliderType; } }
        public int SliderTypeIndex { get { return _sliderTypeIndex; } }
        public int Min { get { return _min; } }
        public int Max { get { return _max; } }
        public int DeadZone { get { return _deadZone; } }
        public float Range { get { return _range; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="axis"></param>
        public SlimSliderReader(ESliderType sliderType, int sliderTypeIndex, int min, int max, int deadZone)
        {
            _sliderType = sliderType;
            _sliderTypeIndex = sliderTypeIndex;
            _min = min;
            _max = max;
            _deadZone = deadZone;

            _range = Math.Max(1, Math.Abs(_max - _min));
        }

        /// <summary>
        /// Get the slider value (0 to 1 range)
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public float GetValue(JoystickState state)
        {
            int[] sliderValues;
            switch (_sliderType)
            {
                case ESliderType.Position:
                default:
                    sliderValues = state.GetSliders(); break;
                case ESliderType.Velocity:
                    sliderValues = state.GetVelocitySliders(); break;
                case ESliderType.Acceleration:
                    sliderValues = state.GetAccelerationSliders(); break;
                case ESliderType.Force:
                    sliderValues = state.GetForceSliders(); break;
            }

            float normalVal = 0f;
            if (_sliderTypeIndex < sliderValues.Length)
            {
                // Fix for some devices where sliders have an initial value of 0.5 until any control is used
                if (_hasChanged)
                {
                    normalVal = (sliderValues[_sliderTypeIndex] - _min) / _range;
                }
                else if (_initialised)
                {
                    if (sliderValues[_sliderTypeIndex] != _initialValue)
                    {
                        normalVal = (sliderValues[_sliderTypeIndex] - _min) / _range;
                        _hasChanged = true;
                    }
                }
                else
                {
                    // Store initial value
                    _initialValue = sliderValues[_sliderTypeIndex];
                    _initialised = true;
                }                
            }

            return normalVal;
        }
    }
}
