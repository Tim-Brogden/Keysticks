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
using System.Globalization;
using System.Text;
using System.Xml;
using Keysticks.Core;
using Keysticks.Event;

namespace Keysticks.Actions
{
    /// <summary>
    /// Set the speed of the mouse pointer
    /// </summary>
    public class ControlThePointerAction : BaseAction
    {
        // Configuration
        private bool _isPositionMode = false;
        private bool _isFixedSpeed = false;
        private bool _isAbsolutePosition = false;
        private double _circleRadiusPercent = Constants.DefaultMousePointerCircleSizePercent;
        private EAxisCombination _axisCombo = EAxisCombination.Both;
        private bool _invertX = false;
        private bool _invertY = false;
        private double _speedMultiplier = 1.0;

        // State
        private float _xSpeed;
        private float _ySpeed;
        private float _xTotal;
        private float _yTotal;
        private long _lastUpdateTicks;
        private bool _isDirected;
        private int _xScreen;
        private int _yScreen;
        private int _radiusPixels;
        private int _xCentre;
        private int _yCentre;

        /// <summary>
        /// Return what type of action this is
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.ControlThePointer; }
        }
        public bool IsPositionMode { get { return _isPositionMode; } set { _isPositionMode = value; } }
        public bool IsFixedSpeed { get { return _isFixedSpeed; } set { _isFixedSpeed = value; } }
        public bool IsAbsolutePosition { get { return _isAbsolutePosition; } set { _isAbsolutePosition = value; } }
        public double CircleRadiusPercent { get { return _circleRadiusPercent; } set { _circleRadiusPercent = value; } }
        public EAxisCombination AxisCombo { get { return _axisCombo; } set { _axisCombo = value; } }
        public bool InvertX { get { return _invertX; } set { _invertX = value; } }
        public bool InvertY { get { return _invertY; } set { _invertY = value; } }
        public double SpeedMultiplier { get { return _speedMultiplier; } set { _speedMultiplier = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public ControlThePointerAction()
            : base()
        {
            ParamDefinition[] requiredParams = new ParamDefinition[]
                                                { new ParamDefinition("X", EDataType.Float),
                                                  new ParamDefinition("Y", EDataType.Float) };
            SetRequiredParameters(requiredParams);
        }

        /// <summary>
        /// Return the name of the action
        /// </summary>
        /// <returns></returns>
        public override string Name
        {
            get
            {
                List<string> options = new List<string>();
                if (_isPositionMode)
                {
                    options.Add(_isAbsolutePosition ? Properties.Resources.String_Absolute.ToLower() : 
                        string.Format(Properties.Resources.String_Relative.ToLower() + ": {0}%", (int)_circleRadiusPercent));
                }
                else
                {
                    if (_isFixedSpeed)
                    {
                        options.Add(Properties.Resources.String_fixed_speed.ToLower());
                    }

                    if (Math.Abs(_speedMultiplier - 1.0) > 1E-3)
                    {
                        options.Add(string.Format(Properties.Resources.String_speed + " x {0:0.00}", _speedMultiplier));
                    }
                }


                if (_axisCombo == EAxisCombination.XOnly)
                {
                    options.Add(Properties.Resources.ControlPointer_XAxisOnly);
                }
                else if (_axisCombo == EAxisCombination.YOnly)
                {
                    options.Add(Properties.Resources.ControlPointer_YAxisOnly);
                }

                if (_invertX)
                {
                    options.Add(Properties.Resources.ControlPointer_InvertXAxis.ToLower());
                }
                if (_invertY)
                {
                    options.Add(Properties.Resources.ControlPointer_InvertYAxis.ToLower());
                }

                StringBuilder sb = new StringBuilder();
                sb.Append(Properties.Resources.String_ControlMousePointer);
                if (options.Count != 0)
                {
                    sb.Append(" (");
                    int i = 0;
                    foreach (string option in options)
                    {
                        sb.Append(option);
                        if (i != options.Count - 1)
                        {
                            sb.Append(", ");
                        }

                        i++;
                    }                    
                    sb.Append(")");
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Get the icon to use
        /// </summary>
        public override EAnnotationImage IconRef
        {
            get
            {
                EAnnotationImage icon;
                if (_isPositionMode)
                {
                    icon = _isAbsolutePosition ? EAnnotationImage.MousePointerAbsolute : EAnnotationImage.MousePointerRelative;
                }
                else
                {
                    icon = _isFixedSpeed ? EAnnotationImage.MousePointerFixedSpeed : EAnnotationImage.MousePointer;
                }
                
                return icon;
            }
        }

        /// <summary>
        /// Read from Xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            if (element.HasAttribute("positionmode"))
            {
                _isPositionMode = bool.Parse(element.GetAttribute("positionmode"));
            }
            if (element.HasAttribute("fixedspeed"))
            {
                _isFixedSpeed = bool.Parse(element.GetAttribute("fixedspeed"));
            }
            if (element.HasAttribute("absolute"))
            {
                _isAbsolutePosition = bool.Parse(element.GetAttribute("absolute"));
            }
            if (element.HasAttribute("radius"))
            {
                _circleRadiusPercent = double.Parse(element.GetAttribute("radius"), CultureInfo.InvariantCulture);
            }
            if (element.HasAttribute("axiscombo"))
            {
                _axisCombo = (EAxisCombination)Enum.Parse(typeof(EAxisCombination), element.GetAttribute("axiscombo"));
            }
            if (element.HasAttribute("invertx"))
            {
                _invertX = bool.Parse(element.GetAttribute("invertx"));
            }
            if (element.HasAttribute("inverty"))
            {
                _invertY = bool.Parse(element.GetAttribute("inverty"));
            }
            if (element.HasAttribute("speedmultiplier"))
            {
                _speedMultiplier = double.Parse(element.GetAttribute("speedmultiplier"), CultureInfo.InvariantCulture);
            }

            base.FromXml(element);
        }

        /// <summary>
        /// Write to Xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("positionmode", _isPositionMode.ToString());
            element.SetAttribute("fixedspeed", _isFixedSpeed.ToString());
            element.SetAttribute("absolute", _isAbsolutePosition.ToString());
            element.SetAttribute("radius", _circleRadiusPercent.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("axiscombo", _axisCombo.ToString());
            element.SetAttribute("invertx", _invertX.ToString());
            element.SetAttribute("inverty", _invertY.ToString());
            element.SetAttribute("speedmultiplier", _speedMultiplier.ToString(CultureInfo.InvariantCulture));
        }
        
        /// <summary>
        /// Start the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void Start(IStateManager parent, KxSourceEventArgs args)
        {
            float xCurrent = 0f;
            if (_axisCombo != EAxisCombination.YOnly)
            {
                // Get normalised pointer position
                ParamValue xValue = args.GetParamValue(0);
                if (xValue != null && xValue.DataType == EDataType.Float)
                {
                    xCurrent = (float)(xValue.Value);
                    if (_invertX)
                    {
                        xCurrent = -xCurrent;
                    }
                }
            }

            float yCurrent = 0f;
            if (_axisCombo != EAxisCombination.XOnly)
            {
                // Get normalised pointer position
                ParamValue yValue = args.GetParamValue(1);
                if (yValue != null && yValue.DataType == EDataType.Float)
                {
                    yCurrent = -(float)(yValue.Value);    // Note minus
                    if (_invertY)
                    {
                        yCurrent = -yCurrent;
                    }
                }
            }

            bool isDirected = Math.Abs(xCurrent) > 1E-4 || Math.Abs(yCurrent) > 1E-4;
            if (_isPositionMode)
            {
                // Store centre positions when stick is first moved
                if (!_isDirected)
                {
                    _xScreen = (int)System.Windows.SystemParameters.VirtualScreenWidth;
                    _yScreen = (int)System.Windows.SystemParameters.VirtualScreenHeight;
                    if (_isAbsolutePosition)
                    {
                        _xCentre = _xScreen / 2;
                        _yCentre = _yScreen / 2;
                    }
                    else
                    {
                        System.Drawing.Point cursorPos = System.Windows.Forms.Cursor.Position;
                        _xCentre = cursorPos.X;
                        _yCentre = cursorPos.Y;
                        _radiusPixels = (int)(_xScreen * 0.01 * _circleRadiusPercent);
                    }
                }

                int newX; 
                int newY;
                if (isDirected)
                {
                    if (_isAbsolutePosition)
                    {
                        float radius = (float)Math.Sqrt(xCurrent * xCurrent + yCurrent * yCurrent);
                        float radiusMultiplier = 1.2f * radius / Math.Max(Math.Abs(xCurrent), Math.Abs(yCurrent));     // 1 / Max(|sin|, |cos|) times a bit for deadzone reasons
                        newX = (int)(_xCentre * (1f + xCurrent * radiusMultiplier));
                        newY = (int)(_yCentre * (1f + yCurrent * radiusMultiplier));
                    }
                    else
                    {
                        newX = _xCentre + (int)(_radiusPixels * xCurrent);
                        newY = _yCentre + (int)(_radiusPixels * yCurrent);
                    }
                }
                else
                {
                    newX = _xCentre;
                    newY = _yCentre;
                }

                newX = Math.Max(0, Math.Min(newX, _xScreen - 8));       // Slightly less than full screen so cursor is still visible
                newY = Math.Max(0, Math.Min(newY, _yScreen - 8));
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point(newX, newY);

                IsOngoing = false;
                parent.MouseStateManager.IsPositionedOffCentre = isDirected;
            }
            else
            {
                if (isDirected)
                {
                    // When switching from position mode to speed mode, wait till the thumb is recentred until starting speed-based movements
                    if (!parent.MouseStateManager.IsPositionedOffCentre)
                    {
                        float radius = (float)Math.Sqrt(xCurrent * xCurrent + yCurrent * yCurrent);
                        float multiplier;
                        if (_isFixedSpeed)
                        {
                            // Fixed speed
                            multiplier = parent.MouseStateManager.PointerSpeed * 0.5f / radius;
                        }
                        else
                        {
                            // Speed dependent upon radius
                            multiplier = parent.MouseStateManager.PointerSpeed * (1f + parent.MouseStateManager.PointerAcceleration * radius) / (1f + parent.MouseStateManager.PointerAcceleration);
                        }
                        multiplier *= (float)_speedMultiplier;  // Apply action-specific speed modifier
                        _xSpeed = xCurrent * multiplier;
                        _ySpeed = yCurrent * multiplier;

                        // Move pointer
                        PerformUpdate_SpeedMode(parent);

                        // Allow action to be ongoing so that pointer can still move when thumb is held in fixed position (e.g. full on)
                        IsOngoing = true;
                    }
                    else
                    {
                        IsOngoing = false;
                    }
                }
                else
                {
                    // Centred
                    _xTotal = 0f;
                    _yTotal = 0f;
                    IsOngoing = false;
                    parent.MouseStateManager.IsPositionedOffCentre = false;
                }
            }        

            // Report to UI when mouse starts or stops moving
            if (isDirected != _isDirected)
            {
                //Trace.WriteLine(isMoving ? "Start" : "Stop");
                KxSourceEventArgs ev = new KxSourceEventArgs(args);
                ev.Param0 = new ParamValue(EDataType.Bool, isDirected);
                ev.Param1 = null;
                parent.ThreadManager.SubmitUIEvent(ev);

                _isDirected = isDirected;
            }
        }

        /// <summary>
        /// Continue the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void Continue(IStateManager parent, KxSourceEventArgs args)
        {
            PerformUpdate_SpeedMode(parent);
        }
        
        /// <summary>
        /// Update the mouse position based upon speed control
        /// </summary>
        private void PerformUpdate_SpeedMode(IStateManager parent)
        {
            // Calc unit movement
            long currentTimeTicks = DateTime.UtcNow.Ticks;
            float timeElapsed;
            if (IsOngoing)
            {
                timeElapsed = (currentTimeTicks - _lastUpdateTicks) / TimeSpan.TicksPerMillisecond;
            }
            else
            {
                timeElapsed = parent.PollingIntervalMS;
            }

            // Increment movement amount
            _xTotal += _xSpeed * timeElapsed;
            _yTotal += _ySpeed * timeElapsed;

            // If the amount reaches an integer number of units, move the pointer
            int xMove = (int)_xTotal;
            int yMove = (int)_yTotal;
            if (xMove != 0 || yMove != 0)
            {
                parent.MouseStateManager.MoveMouse((int)_xTotal, (int)_yTotal, false);
                _xTotal -= xMove;
                _yTotal -= yMove;
            }

            _lastUpdateTicks = currentTimeTicks;
        }
    }

}
