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
using System.Xml;
using System.Text;
using System.Windows;
using Keysticks.Core;

namespace Keysticks.Config
{
    /// <summary>
    /// Describes a grid for displaying what controls do in a certain situation
    /// </summary>
    public class GridConfig
    {
        // Fields
        private EGridType _gridType = EGridType.None;
        private int _numCols = Constants.DefaultID;
        private List<GridBinding> _gridBindings = new List<GridBinding>();

        // Properties
        public EGridType GridType { get { return _gridType; } }
        public int NumCols { get { return _numCols; } set { _numCols = value; } }
        public List<GridBinding> Bindings { get { return _gridBindings; } set { _gridBindings = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public GridConfig()            
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gridType"></param>
        public GridConfig(EGridType gridType)
        {
            _gridType = gridType;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gridType"></param>
        public GridConfig(EGridType gridType, int numCols)
        {
            _gridType = gridType;
            _numCols = numCols;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="grid"></param>
        public GridConfig(GridConfig grid)
        {
            _gridType = grid.GridType;
            _numCols = grid.NumCols;
            foreach (GridBinding binding in grid.Bindings)
            {
                _gridBindings.Add(new GridBinding(binding));
            }
        }

        /// <summary>
        /// Read from Xml
        /// </summary>
        /// <param name="element"></param>
        public void FromXml(XmlElement element)
        {
            // Grid type
            _gridType = (EGridType)Enum.Parse(typeof(EGridType), element.GetAttribute("type"));

            // Num columns            
            _numCols = element.HasAttribute("cols") ? int.Parse(element.GetAttribute("cols"), System.Globalization.CultureInfo.InvariantCulture) : Constants.DefaultID;

            // Bindings
            XmlNodeList bindingElements = element.SelectNodes("binding");
            foreach (XmlElement bindingElement in bindingElements)
            {
                GridBinding binding = new GridBinding();
                binding.FromXml(bindingElement);
                _gridBindings.Add(binding);
            }
        }

        /// <summary>
        /// Write to Xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public void ToXml(XmlElement element, XmlDocument doc)
        {
            // Grid type
            element.SetAttribute("type", _gridType.ToString());

            // Num columns
            if (_numCols != Constants.DefaultID)
            {
                element.SetAttribute("cols", _numCols.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            // Bindings
            foreach (GridBinding binding in _gridBindings)
            {
                XmlElement bindingElement = doc.CreateElement("binding");
                binding.ToXml(bindingElement, doc);
                element.AppendChild(bindingElement);
            }
        }
    }
}
