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
using System.Xml;
using System.Globalization;
using Keysticks.Core;
using Keysticks.Event;

namespace Keysticks.Config
{
    /// <summary>
    /// Stores the data needed to display how a control is mapped
    /// </summary>
    public class AnnotationData
    {
        // Fields
        private Rect _displayRect;
        private KxControlEventArgs _inputBinding;

        // Properties
        public Rect DisplayRect { get { return _displayRect; } set { _displayRect = value; } }
        public KxControlEventArgs InputBinding { get { return _inputBinding; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        public AnnotationData()
        {
            _displayRect = new Rect();
            _inputBinding = null;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="data"></param>
        public AnnotationData(AnnotationData data)
        {
            _displayRect = new Rect(data._displayRect.TopLeft, data._displayRect.BottomRight);
            if (data._inputBinding != null)
            {
                _inputBinding = new KxControlEventArgs(data._inputBinding);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="displayRect"></param>
        /// <param name="inputBinding"></param>
        public AnnotationData(Rect displayRect, KxControlEventArgs inputBinding)
        {
            _displayRect = displayRect;
            _inputBinding = inputBinding;
        }

        /// <summary>
        /// Constructor using default width and height
        /// </summary>
        /// <param name="displayPos"></param>
        /// <param name="inputBinding"></param>
        public AnnotationData(Point displayPos, KxControlEventArgs inputBinding)
        {
            _displayRect = new Rect(displayPos, new Size(Constants.DefaultAnnotationWidth, Constants.DefaultAnnotationHeight));
            _inputBinding = inputBinding;
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public void FromXml(XmlElement element)
        {
            if (element.HasAttribute("x") && element.HasAttribute("y") && element.HasAttribute("width") && element.HasAttribute("height"))
            {
                _displayRect.X = double.Parse(element.GetAttribute("x"), CultureInfo.InvariantCulture);
                _displayRect.Y = double.Parse(element.GetAttribute("y"), CultureInfo.InvariantCulture);
                _displayRect.Width = double.Parse(element.GetAttribute("width"), CultureInfo.InvariantCulture);
                _displayRect.Height = double.Parse(element.GetAttribute("height"), CultureInfo.InvariantCulture);
            }

            KxControlEventArgs args = new KxControlEventArgs();
            if (args.FromXml(element))
            {
                _inputBinding = args;
            }
        }

        /// <summary>
        /// Write the source config to an xml node
        /// </summary>
        /// <param name="element"></param>
        public void ToXml(XmlElement element, XmlDocument doc)
        {
            element.SetAttribute("x", _displayRect.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("y", _displayRect.Y.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("width", _displayRect.Width.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("height", _displayRect.Height.ToString(CultureInfo.InvariantCulture));
            if (_inputBinding != null)
            {
                _inputBinding.ToXml(element, doc);
            }
        }

    }
}
