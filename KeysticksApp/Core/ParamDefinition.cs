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

namespace Keysticks.Core
{
    /// <summary>
    /// Defines a parameter used to exchange data between events and actions
    /// </summary>
    public class ParamDefinition
    {
        // Fields
        private string _name;
        private EDataType _dataType;

        // Properties
        public string Name { get { return _name; } }
        public EDataType DataType { get { return _dataType; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        public ParamDefinition(string name, EDataType dataType)
        {
            _name = name;
            _dataType = dataType;
        }
    }
}
