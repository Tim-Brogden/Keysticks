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
using System.Windows.Data;
using System.Globalization;

namespace Keysticks.UI
{
    [ValueConversion(typeof(float), typeof(String))]
    public class FloatValBindingConverter : IValueConverter
    {
        private const int _decimalPlaces = 3;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float val = (float)value;
            val = (float)Math.Round(val, _decimalPlaces);
            return val.ToString();    // Local culture because it's a UI operation
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float val = 0f;
            float.TryParse((string)value, out val);    // Local culture because it's a UI operation                
            return val;
        }
    }
}
