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
using System.Globalization;

namespace Keysticks.Core
{
    /// <summary>
    /// Represents a parameter value used to exchange data between events and actions
    /// </summary>
    public class ParamValue
    {
        // Fields
        private EDataType _dataType;
        private object _value;

        // Properties
        public EDataType DataType { get { return _dataType; } set { _dataType = value; } }
        public object Value { get { return _value; } set { _value = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public ParamValue()
        {
            _dataType = EDataType.None;
            _value = null;
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="val"></param>
        public ParamValue(EDataType dataType, object val)
        {
            _dataType = dataType;
            _value = val;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="paramVal"></param>
        public ParamValue(ParamValue paramVal)
        {
            _dataType = paramVal._dataType;
            _value = paramVal._value;       // Shallow copy
        }

        /// <summary>
        /// Get string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = "";
            switch (_dataType)
            {
                case EDataType.Bool:
                    str = ((bool)_value).ToString(); break;
                case EDataType.Float:
                    str = ((float)_value).ToString(CultureInfo.InvariantCulture); break;
                case EDataType.LRUD:
                    str = ((ELRUDState)_value).ToString(); break;
                case EDataType.None:
                default:                
                    break;
            }

            return str;
        }
    }
}
