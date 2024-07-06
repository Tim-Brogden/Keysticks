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
using System.Xml;
using Keysticks.Event;

namespace Keysticks.Config
{
    /// <summary>
    /// Describes a binding from a window control to a state-event pair 
    /// </summary>
    public class GridBinding
    {
        // Fields
        private string _uiControlName;
        private StateVector _state;
        private KxControlEventArgs _eventArgs;

        // Properties
        public string UIControlName { get { return _uiControlName; } set { _uiControlName = value; } }
        public StateVector State { get { return _state; } set { _state = value; } }
        public KxControlEventArgs EventArgs { get { return _eventArgs; } set { _eventArgs = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public GridBinding()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="uiControlName"></param>
        /// <param name="state"></param>
        /// <param name="args"></param>
        public GridBinding(string uiControlName, StateVector state, KxControlEventArgs args)
        {
            _uiControlName = uiControlName;
            _state = state;
            _eventArgs = args;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="binding"></param>
        public GridBinding(GridBinding binding)
        {
            _uiControlName = string.Copy(binding.UIControlName);
            _state = new StateVector(binding.State);
            if (binding.EventArgs != null)
            {
                _eventArgs = new KxControlEventArgs(binding.EventArgs);
            }
        }

        /// <summary>
        /// Read from Xml
        /// </summary>
        /// <param name="element"></param>
        public void FromXml(XmlElement element)
        {
            _uiControlName = element.GetAttribute("uicontrolname");

            // Could make state vector optional (e.g. not used for controller window)
            _state = new StateVector();
            _state.FromXml(element);

            KxControlEventArgs args = new KxControlEventArgs();
            if (args.FromXml(element))
            {
                _eventArgs = args;
            }
        }

        /// <summary>
        /// Write to Xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public void ToXml(XmlElement element, XmlDocument doc)
        {
            element.SetAttribute("uicontrolname", _uiControlName);

            _state.ToXml(element, doc);

            if (_eventArgs != null)
            {
                _eventArgs.ToXml(element, doc);
            }
        }
    }
}
