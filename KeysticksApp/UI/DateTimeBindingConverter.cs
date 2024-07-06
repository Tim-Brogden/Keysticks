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
    [ValueConversion(typeof(DateTime), typeof(String))]
    public class DateTimeBindingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime date = (DateTime)value;
            date = date.ToLocalTime();  // Display as local time
            return date.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime date = DateTime.Parse((string)value);
            return date.ToUniversalTime();  // Store as UTC time
        }
    }
}
