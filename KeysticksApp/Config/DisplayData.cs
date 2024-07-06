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
using System.Windows;
using System.Xml;
using Keysticks.Core;

namespace Keysticks.Config
{
    /// <summary>
    /// Stores the display configuration of a controller window
    /// </summary>
    public class DisplayData
    {
        // Config
        private double _width = Constants.DefaultControllerWidth;
        private double _height = Constants.DefaultControllerHeight;
        private AnnotationData _titleBarData = new AnnotationData();
        private AnnotationData _menuBarData = new AnnotationData();
        EBackgroundType _backgroundType = EBackgroundType.Default;
        private string _backgroundColour = Constants.DefaultControllerBackgroundColourName;
        private string _backgroundImageUri = "";

        // Properties
        public double Width { get { return _width; } }
        public double Height { get { return _height; } }
        public AnnotationData TitleBarData { get { return _titleBarData; } set { _titleBarData = value; } }
        public AnnotationData MenuBarData { get { return _menuBarData; } set { _menuBarData = value; } }
        public EBackgroundType BackgroundType { get { return _backgroundType; } set { _backgroundType = value; } }
        public string BackgroundColour { get { return _backgroundColour; } set { _backgroundColour = value; } }
        public string BackgroundImageUri { get { return _backgroundImageUri; } set { _backgroundImageUri = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source"></param>
        public DisplayData()
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="display"></param>
        public DisplayData(DisplayData display)
        {
            _width = display._width;
            _height = display._height;
            if (display._titleBarData != null)
            {
                _titleBarData = new AnnotationData(display._titleBarData);
            }
            if (display._menuBarData != null)
            {
                _menuBarData = new AnnotationData(display._menuBarData);
            }
            _backgroundType = display._backgroundType;
            if (display._backgroundColour != null)
            {
                _backgroundColour = string.Copy(display._backgroundColour);
            }
            if (display._backgroundImageUri != null)
            {
                _backgroundImageUri = string.Copy(display._backgroundImageUri);
            }
        }

        /// <summary>
        /// Set the size of the controller display
        /// </summary>
        /// <param name="size"></param>
        public void SetSize(double width, double height)
        {
            _width = width;
            _height = height;

            //ValidateControlPositions();
        }

        /// <summary>
        /// Read the default display data from the app config
        /// </summary>
        /// <param name="appConfig"></param>
        /// <returns></returns>
        public void FromConfig(AppConfig appConfig)
        {
            // Background type
            string backgroundType = appConfig.GetStringVal(Constants.ConfigDefaultControllerBackgroundType, EBackgroundType.Default.ToString());
            EBackgroundType eBackgroundType;
            if (!Enum.TryParse<EBackgroundType>(backgroundType, out eBackgroundType))
            {
                eBackgroundType = EBackgroundType.Default;
            }
            _backgroundType = eBackgroundType;

            // Background colour
            _backgroundColour = appConfig.GetStringVal(Constants.ConfigDefaultControllerBackgroundColour, Constants.DefaultControllerBackgroundColourName);

            // Background image uri
            _backgroundImageUri = appConfig.GetStringVal(Constants.ConfigDefaultControllerBackgroundUri, Constants.DefaultControllerBackgroundUri);
        }

        /// <summary>
        /// Store the display data in the app config
        /// </summary>
        /// <param name="appConfig"></param>
        public void ToConfig(AppConfig appConfig)
        {
            appConfig.SetStringVal(Constants.ConfigDefaultControllerBackgroundType, _backgroundType.ToString());
            appConfig.SetStringVal(Constants.ConfigDefaultControllerBackgroundColour, _backgroundColour);
            appConfig.SetStringVal(Constants.ConfigDefaultControllerBackgroundUri, _backgroundImageUri);
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public void FromXml(XmlElement element)
        {            
            // Display size
            _width = double.Parse(element.GetAttribute("width"), CultureInfo.InvariantCulture);
            _height = double.Parse(element.GetAttribute("height"), CultureInfo.InvariantCulture);

            // Title bar pos
            XmlElement titleBarElement = (XmlElement)element.SelectSingleNode("titlebar");
            if (titleBarElement != null)
            {
                _titleBarData = new AnnotationData();
                _titleBarData.FromXml(titleBarElement);
            }

            // Menu bar pos
            XmlElement menuBarElement = (XmlElement)element.SelectSingleNode("menubar");
            if (menuBarElement != null)
            {
                _menuBarData = new AnnotationData();
                _menuBarData.FromXml(menuBarElement);
            }

            // Background
            XmlElement backgroundElement = (XmlElement)element.SelectSingleNode("background");
            if (backgroundElement != null)
            {
                _backgroundType = (EBackgroundType)Enum.Parse(typeof(EBackgroundType), backgroundElement.GetAttribute("type"));
                _backgroundColour = backgroundElement.GetAttribute("colour");
                _backgroundImageUri = backgroundElement.GetAttribute("imagefilepath");
            }
        }

        /// <summary>
        /// Write the source config to an xml node
        /// </summary>
        /// <param name="element"></param>
        public void ToXml(XmlElement element, XmlDocument doc)
        {
            // Display size
            element.SetAttribute("width", _width.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("height", _height.ToString(CultureInfo.InvariantCulture));

            // Title bar pos
            XmlElement titleBarElement = doc.CreateElement("titlebar");
            _titleBarData.ToXml(titleBarElement, doc);
            element.AppendChild(titleBarElement);

            // Menu bar pos
            XmlElement menuBarElement = doc.CreateElement("menubar");
            _menuBarData.ToXml(menuBarElement, doc);
            element.AppendChild(menuBarElement);

            // Background
            XmlElement backgroundElement = doc.CreateElement("background");
            backgroundElement.SetAttribute("type", _backgroundType.ToString());
            backgroundElement.SetAttribute("colour", _backgroundColour);
            backgroundElement.SetAttribute("imagefilepath", _backgroundImageUri);
            element.AppendChild(backgroundElement);
        }


        /// <summary>
        /// Calculate enclosing polygon
        /// </summary>
        /// <returns></returns>
        public List<Point> GetBoundingPolygon(List<AnnotationData> allAnnotations, Point origin)
        {
            Point[] cornerPoints = GetCornerPoints(allAnnotations, origin, Constants.DefaultCompactControllerPadding, true);

            // Calculate convex hull
            GeometryUtils geomUtils = new GeometryUtils();
            List<Point> vertices = geomUtils.CalculateConvexHull(cornerPoints);

            return vertices;
        }

        /// <summary>
        /// Calculate the bounding rectangle containing the all the virtual control annotations
        /// </summary>
        /// <returns></returns>
        public Rect GetBoundingRect(List<AnnotationData> allAnnotations)
        {
            Point[] cornerPoints = GetCornerPoints(allAnnotations, new Point(0, 0), Constants.DefaultCompactControllerPadding, true);

            GeometryUtils geomUtils = new GeometryUtils();
            Rect boundingRect = geomUtils.CalcSimpleBoundingRect(cornerPoints);

            return boundingRect;
        }
        
        /// <summary>
        /// Get a list of corners of the display items which comprise the controller, relative to the origin provided
        /// </summary>
        /// <returns></returns>
        private Point[] GetCornerPoints(List<AnnotationData> annotationDataList, Point origin, int padding, bool clipToDisplayArea)
        {
            Point[] cornerPoints = new Point[4 * annotationDataList.Count];
            double xMin;
            double xMax;
            double yMin;
            double yMax;

            // Add the corners of the control annotations
            int i = 0;
            foreach (AnnotationData annotationData in annotationDataList)
            {
                xMin = annotationData.DisplayRect.X - padding;
                xMax = annotationData.DisplayRect.BottomRight.X + padding;
                yMin = annotationData.DisplayRect.Y - padding;
                yMax = annotationData.DisplayRect.BottomRight.Y + padding;
                if (clipToDisplayArea)
                {
                    xMin = Math.Max(0, Math.Min(_width, xMin));
                    xMax = Math.Max(0, Math.Min(_width, xMax));
                    yMin = Math.Max(0, Math.Min(_height, yMin));
                    yMax = Math.Max(0, Math.Min(_height, yMax));
                }
                cornerPoints[i++] = new Point(xMin - origin.X, yMin - origin.Y);
                cornerPoints[i++] = new Point(xMax - origin.X, yMin - origin.Y);
                cornerPoints[i++] = new Point(xMax - origin.X, yMax - origin.Y);
                cornerPoints[i++] = new Point(xMin - origin.X, yMax - origin.Y);
            }

            return cornerPoints;
        }

    }
}
