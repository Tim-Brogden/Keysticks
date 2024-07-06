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
using System.Windows.Media;

namespace Keysticks.Core
{
    /// <summary>
    /// Utility methods relating to colours
    /// </summary>
    public class ColourUtils
    {
        /// <summary>
        /// Convert colour to ID
        /// </summary>
        /// <param name="colour"></param>
        /// <returns></returns>
        public static int ColorToID(Color colour)
        {
            return colour.A | (colour.R << 8) | (colour.G << 16) | (colour.B << 24);
        }

        /// <summary>
        /// Get colour from ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Color ColorFromID(int id)
        {
            byte a = (byte)(id & 0xFF);
            byte r = (byte)((id >> 8) & 0xFF);
            byte g = (byte)((id >> 16) & 0xFF);
            byte b = (byte)((id >> 24) & 0xFF);

            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Get colour name
        /// </summary>
        /// <param name="colour"></param>
        /// <returns></returns>
        public static string ColorToString(Color colour)
        {
            ColorConverter converter = new ColorConverter();
            string name = converter.ConvertToString(colour);

            return name;
        }

        /// <summary>
        /// Get colour from name
        /// </summary>
        /// <param name="colourName"></param>
        /// <param name="colour"></param>
        /// <returns></returns>
        public static Color ColorFromString(string colourName, Color defaultColour)
        {
            Color colour = defaultColour;
            object obj = ColorConverter.ConvertFromString(colourName);
            if (obj != null)
            {
                colour = (Color)obj;
            }

            return colour;
        }

        /// <summary>
        /// Get a colour from a name and an opacity value
        /// </summary>
        /// <param name="colourName"></param>
        /// <param name="opacity"></param>
        /// <param name="colour"></param>
        /// <returns></returns>
        public static Color ColorFromString(string colourName, double opacity, Color defaultColour)
        {
            Color colour = defaultColour;
            object obj = ColorConverter.ConvertFromString(colourName);
            if (obj != null)
            {
                colour = (Color)obj;
                colour.A = (byte)(255 * opacity);
            }

            return colour;
        }
    }
}
