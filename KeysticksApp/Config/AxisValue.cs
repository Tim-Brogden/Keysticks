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
using Keysticks.Core;

namespace Keysticks.Config
{
    /// <summary>
    /// Defines a state in the control set hierarchy
    /// </summary>
    public class AxisValue : NamedItem
    {
        // Fields
        private StateVector _situation = new StateVector();
        private ControlsDefinition _controls = null;
        private GridConfig _gridConfig = null;
        private ETemplateGroup _templateGroup = ETemplateGroup.None;
        private NamedItemList _subValues = new NamedItemList();

        // Properties
        public EGridType GridType { get { return (_gridConfig != null) ? _gridConfig.GridType : EGridType.None; } }
        public ControlsDefinition Controls { get { return _controls; } set { _controls = value; } }
        public GridConfig Grid { get { return _gridConfig; } set { _gridConfig = value; } }
        public ETemplateGroup TemplateGroup { get { return _templateGroup; } }
        public StateVector Situation { get { return _situation; } }
        public NamedItemList SubValues { get { return _subValues; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        public AxisValue()
        :base()
        {
        }

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="situation"></param>
        public AxisValue(int id, string name, StateVector situation)
            :base(id, name)
        {
            _situation = situation;
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public AxisValue(int id, 
                            string name, 
                            StateVector situation, 
                            ControlsDefinition controls,
                            GridConfig gridConfig,
                            ETemplateGroup templateGroup)
        :base(id, name)
        {
            _situation = situation;
            _controls = controls;
            _gridConfig = gridConfig;
            _templateGroup = templateGroup;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="axisValue"></param>
        public AxisValue(AxisValue axisValue)
            :base(axisValue)
        {
            _situation = new StateVector(axisValue._situation);
            if (axisValue._controls != null)
            {
                _controls = new ControlsDefinition(axisValue._controls);
            }
            if (axisValue._gridConfig != null)
            {
                _gridConfig = new GridConfig(axisValue._gridConfig);
            }
            _templateGroup = axisValue._templateGroup;

            foreach (AxisValue subValue in axisValue._subValues)
            {
                _subValues.Add(new AxisValue(subValue));
            }
        }

        public override void FromXml(XmlElement element)
        {
            base.FromXml(element);

            // Situation
            if (element.HasAttribute("state"))
            {
                _situation = new StateVector();
                _situation.FromXml(element);
            }

            // Template group
            if (element.HasAttribute("templategroup"))
            {
                _templateGroup = (ETemplateGroup)Enum.Parse(typeof(ETemplateGroup), element.GetAttribute("templategroup"));
            }

            // Control templates
            XmlElement controlsElement = (XmlElement)element.SelectSingleNode("controls");
            if (controlsElement != null)
            {
                _controls = new ControlsDefinition();
                _controls.FromXml(controlsElement);
            }

            // Grid
            XmlElement gridElement = (XmlElement)element.SelectSingleNode("grid");
            if (gridElement != null)
            {
                _gridConfig = new GridConfig();
                _gridConfig.FromXml(gridElement);
            }

            // Sub values
            XmlNodeList valueNodes = element.SelectNodes("value");
            foreach (XmlElement valueElement in valueNodes)
            {
                AxisValue axisValue = new AxisValue();
                axisValue.FromXml(valueElement);
                _subValues.Add(axisValue);
            }
        }

        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            // Situation
            _situation.ToXml(element, doc);

            // Template group
            if (_templateGroup != ETemplateGroup.None)
            {
                element.SetAttribute("templategroup", _templateGroup.ToString());
            }

            // Control templates
            if (_controls != null)
            {
                XmlElement controlsElement = doc.CreateElement("controls");
                _controls.ToXml(controlsElement, doc);
                element.AppendChild(controlsElement);
            }            

            // Grid
            if (_gridConfig != null)
            {
                XmlElement gridElement = doc.CreateElement("grid");
                _gridConfig.ToXml(gridElement, doc);
                element.AppendChild(gridElement);
            }

            // Sub values
            foreach (AxisValue subValue in _subValues)
            {
                XmlElement valueElement = doc.CreateElement("value");
                subValue.ToXml(valueElement, doc);
                element.AppendChild(valueElement);
            }
        }
    }
}
