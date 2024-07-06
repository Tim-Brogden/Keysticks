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
using System.Xml;
using System.Windows.Forms;
using Keysticks.Core;
using Keysticks.Event;

namespace Keysticks.Actions
{
    /// <summary>
    /// Moves the mouse pointer to a new screen position
    /// </summary>
    public class MoveThePointerAction : BaseAction
    {
        // Fields
        private bool _absoluteMove = true;
        private bool _relativeToWindow = true;
        private bool _percentOrPixels = true;
        private double _x = 50;
        private double _y = 2;

        // Properties
        public bool AbsoluteMove { get { return _absoluteMove; } set { _absoluteMove = value; } }
        public bool RelativeToWindow { get { return _relativeToWindow; } set { _relativeToWindow = value; } }
        public bool PercentOrPixels { get { return _percentOrPixels; } set { _percentOrPixels = value; } }
        public double X { get { return _x; } set { _x = value; } }
        public double Y { get { return _y; } set { _y = value; } }

        /// <summary>
        /// Type of action
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.MoveThePointer; }
        }

        /// <summary>
        /// Name of action
        /// </summary>
        public override string Name
        {
            get
            {
                string name = string.Format("{0} ({1}, {2}){3}",
                    _absoluteMove ? Properties.Resources.String_MovePointerTo : Properties.Resources.String_MovePointerBy,
                    _percentOrPixels ? string.Format("{0:0.0}%", _x) : string.Format("{0}px", (int)_x),
                    _percentOrPixels ? string.Format("{0:0.0}%", _y) : string.Format("{0}px", (int)_y),
                    (_percentOrPixels || _absoluteMove) ? _relativeToWindow ? ", " + Properties.Resources.String_WindowCoords : ", " + Properties.Resources.String_DesktopCoords : ""
                    );
                
                return name;
            }
        }

        /// <summary>
        /// Short name of action
        /// </summary>
        public override string ShortName
        {
            get
            {
                string name = string.Format("{0} ({1}, {2})",
                    _absoluteMove ? Properties.Resources.String_MoveTo : Properties.Resources.String_MoveBy,
                    _percentOrPixels ? string.Format("{0:0.0}%", _x) : string.Format("{0}px", (int)_x),
                    _percentOrPixels ? string.Format("{0:0.0}%", _y) : string.Format("{0}px", (int)_y)
                    );

                return name;
            }
        }

        /// <summary>
        /// Get the icon to use
        /// </summary>
        public override EAnnotationImage IconRef
        {
            get
            {
                EAnnotationImage icon = _absoluteMove ? EAnnotationImage.MousePointerAbsolute : EAnnotationImage.MousePointerRelative;

                return icon;
            }
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _absoluteMove = bool.Parse(element.GetAttribute("isabsolutemove"));
            _relativeToWindow = bool.Parse(element.GetAttribute("iswindowcoord"));
            _percentOrPixels = bool.Parse(element.GetAttribute("ispercent"));
            _x = double.Parse(element.GetAttribute("x"), System.Globalization.CultureInfo.InvariantCulture);
            _y = double.Parse(element.GetAttribute("y"), System.Globalization.CultureInfo.InvariantCulture);

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("isabsolutemove", _absoluteMove.ToString());
            element.SetAttribute("iswindowcoord", _relativeToWindow.ToString());
            element.SetAttribute("ispercent", _percentOrPixels.ToString());
            element.SetAttribute("x", _x.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("y", _y.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Start the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void Start(IStateManager parent, KxSourceEventArgs args)
        {
            int screenWidth = SystemInformation.VirtualScreen.Width;
            int screenHeight = SystemInformation.VirtualScreen.Height;

            // Get the amount to move in pixels
            int x;
            int y;
            if (_percentOrPixels)
            {
                // Convert percent to pixels
                if (_relativeToWindow)
                {
                    x = (int)(0.01 * _x * (parent.CurrentWindowRect.Right - parent.CurrentWindowRect.Left));
                    y = (int)(0.01 * _y * (parent.CurrentWindowRect.Bottom - parent.CurrentWindowRect.Top));
                }
                else
                {
                    x = (int)(0.01 * _x * screenWidth);
                    y = (int)(0.01 * _y * screenHeight);
                }
            }
            else
            {
                // Pixels
                x = (int)_x;
                y = (int)_y;
            }

            // Add window / current pointer position offset
            if (_absoluteMove)
            {
                if (_relativeToWindow)
                {
                    x += (int)parent.CurrentWindowRect.Left;
                    y += (int)parent.CurrentWindowRect.Top;
                }
            }
            else
            {
                System.Drawing.Point curPos = Cursor.Position;
                x += curPos.X;
                y += curPos.Y;
            }

            // Convert to normalised co-ords
            x = (x * 0xFFFF) / screenWidth;
            y = (y * 0xFFFF) / screenHeight;

            // Check bounds
            x = Math.Max(0, Math.Min(0xFFFF, x));
            y = Math.Max(0, Math.Min(0xFFFF, y));

            // Move the mouse
            parent.MouseStateManager.MoveMouse(x, y, true);
        }
    }
}
