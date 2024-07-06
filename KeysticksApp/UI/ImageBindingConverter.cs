﻿/******************************************************************************
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
using System.Windows.Controls;
using System.Windows.Data;
using System.Globalization;
using Keysticks.Core;

namespace Keysticks.UI
{
    [ValueConversion(typeof(EProfileStatus), typeof(Image))]
    public class ImageBindingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EProfileStatus status = (EProfileStatus)value;
            EAnnotationImage icon = GUIUtils.GetIconForProfileStatus(status);
            return GUIUtils.FindIcon(icon);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return EProfileStatus.None;   // Not required
        }
    }
}