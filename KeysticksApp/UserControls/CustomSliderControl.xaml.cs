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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Keysticks.UI;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Custom slider control with logarithmic scale option
    /// </summary>
    public partial class CustomSliderControl : UserControl
    {
        // Fields
        bool _isLoaded = false;
        private double _minimum = 0.0;
        private double _maximum = 1.0;
        private double _currentVal = 0.0;
        private double _smallChange = 0.01;
        private double _largeChange = 0.1;
        private bool _isLogScale = false;
        private double _logBase = 5;
        private int _decimalPlaces = 3;

        // Properties
        public double Value 
        { 
            get { return _currentVal; } 
            set 
            {
                double val = Math.Max(_minimum, Math.Min(_maximum, value));
                val = Math.Round(val, _decimalPlaces);
                if (val != _currentVal)
                {
                    _currentVal = val;
                    if (_isLoaded)
                    {
                        this.ValueTextBox.Text = _currentVal.ToString();   // Use local culture because it's a UI operation
                        SetSliderVal(this.ValueSlider, _currentVal);
                    }
                    RaiseEvent(new KxDoubleValRoutedEventArgs(KxSliderValueChangedEvent, _currentVal));
                }
            } 
        }
        public double Minimum 
        { 
            get { return _minimum; } 
            set 
            { 
                _minimum = value;
                if (_isLoaded)
                {
                    if (_isLogScale)
                    {
                        this.ValueSlider.Minimum = ConvertToLogBase(_minimum);
                    }
                    else
                    {
                        this.ValueSlider.Minimum = _minimum;
                    }
                }
            } 
        }
        public double Maximum 
        { 
            get { return _maximum; } 
            set 
            { 
                _maximum = value;
                if (_isLoaded)
                {
                    if (_isLogScale)
                    {
                        this.ValueSlider.Maximum = ConvertToLogBase(_maximum);
                    }
                    else
                    {
                        this.ValueSlider.Maximum = _maximum;
                    }
                }
            } 
        }
        public double SmallChange
        {
            get { return _smallChange; }
            set
            {
                _smallChange = value;
                if (_isLoaded)
                {
                    this.ValueSlider.SmallChange = value;
                }
            }
        }
        public double LargeChange 
        { 
            get { return _largeChange; } 
            set 
            { 
                _largeChange = value;
                if (_isLoaded)
                {
                    this.ValueSlider.LargeChange = value;
                }
            } 
        }
        public bool IsLogScale { get { return _isLogScale; } set { _isLogScale = value; } }
        public double LogBase { get { return _logBase; } set { _logBase = value; } }
        public int DecimalPlaces { get { return _decimalPlaces; } set { _decimalPlaces = value; } }

        // Events
        public event KxDoubleValRoutedEventHandler ValueChanged
        {
            add { AddHandler(KxSliderValueChangedEvent, value); }
            remove { RemoveHandler(KxSliderValueChangedEvent, value); }
        }
        public static readonly RoutedEvent KxSliderValueChangedEvent = EventManager.RegisterRoutedEvent(
            "ValueChanged", RoutingStrategy.Bubble, typeof(KxDoubleValRoutedEventHandler), typeof(CustomSliderControl));


        /// <summary>
        /// Constructor
        /// </summary>
        public CustomSliderControl()
            : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Slider changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isLoaded)
            {
                // Convert slider value to actual value
                double newVal;
                if (_isLogScale)
                {
                    newVal = ConvertFromLogBase(this.ValueSlider.Value);
                }
                else
                {
                    newVal = this.ValueSlider.Value;
                }

                Value = newVal;
            }

            e.Handled = true;
        }

        /// <summary>
        /// Update value if text has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValueTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            double val;
            if (double.TryParse(this.ValueTextBox.Text, out val))   // Use local culture because it's a UI operation
            {
                Value = val;
            }            

            e.Handled = true;
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLogScale)
            {
                this.ValueSlider.Minimum = ConvertToLogBase(_minimum);
                this.ValueSlider.Maximum = ConvertToLogBase(_maximum);
            }
            else
            {
                this.ValueSlider.Minimum = _minimum;
                this.ValueSlider.Maximum = _maximum;
            }
            this.ValueSlider.SmallChange = _smallChange;
            this.ValueSlider.LargeChange = _largeChange;

            this.ValueTextBox.Text = _currentVal.ToString();   // Use local culture because it's a UI operation
            SetSliderVal(this.ValueSlider, _currentVal);

            _isLoaded = true;
        }

        /// <summary>
        /// Set the value of a slider control
        /// </summary>
        /// <param name="slider"></param>
        /// <param name="val"></param>
        private void SetSliderVal(Slider slider, double val)
        {
            // Convert to log value if required
            if (_isLogScale)
            {
                slider.Value = ConvertToLogBase(val);
            }
            else
            {
                slider.Value = val;
            }
        }

        /// <summary>
        /// Convert to log scale
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private double ConvertToLogBase(double val)
        {
            return Math.Log(val, _logBase) + 1;
        }

        /// <summary>
        /// Convert from log scale
        /// </summary>
        /// <returns></returns>
        private double ConvertFromLogBase(double val)
        {
            return Math.Pow(_logBase, val - 1);
        }

    }
}
