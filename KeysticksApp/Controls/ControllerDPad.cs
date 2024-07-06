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
//using System.Diagnostics;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Input;
using Keysticks.Event;

namespace Keysticks.Controls
{
    /// <summary>
    /// Represents a DPad control
    /// </summary>
    public class ControllerDPad : DirectionalControl
    {
        // Config
        private List<AnnotationData> _annotationDataList = new List<AnnotationData>();

        // State
        //private int _lastPacketNo = -1;
        //private GamePadDPad _dpadState;
        //private List<ParameterSourceArgs> _supportedVariables;
        private bool _isBindingValid;
        
        // Misc
        private StringUtils _utils = new StringUtils();

        // Properties
        public List<AnnotationData> AnnotationDataList { get { return _annotationDataList; } }        
        public override EVirtualControlType ControlType { get { return EVirtualControlType.DPad; } }
        protected override bool IsPressableControl { get { return false; } }     

        /// <summary>
        /// Get the event reasons supported by this control
        /// </summary>
        public override List<EEventReason> GetSupportedEventReasons(KxControlEventArgs args/*, EDirectionMode directionMode*/)
        {
            List<EEventReason> supportedReasons = new List<EEventReason>();
            foreach (EEventReason reason in Enum.GetValues(typeof(EEventReason)))
            {
                if (IsReasonSupported(args, reason))
                {
                    supportedReasons.Add(reason);
                }
            }

            return supportedReasons;
        }

        /// <summary>
        /// Get whether the specified reason is supported
        /// </summary>
        /// <param name="args"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public override bool IsReasonSupported(KxControlEventArgs args, /*EDirectionMode directionMode,*/ EEventReason reason)
        {
            bool supported = false;
            if (args.SettingType != EControlSetting.None)
            {
                supported = (reason == EEventReason.Activated);
            }
            else
            {
                switch (reason)
                {
                    case EEventReason.Directed:
                    case EEventReason.Undirected:
                        supported = true;/*Parent.Utils.IsDirectionValid(direction, directionMode, true);*/ break;
                    case EEventReason.DirectedShort:
                    case EEventReason.DirectedLong:
                    case EEventReason.DirectionRepeated:
                        supported = (args.LRUDState != ELRUDState.Centre);/* && Parent.Utils.IsDirectionValid(direction, directionMode, true);*/ break;
                }
            }

            return supported;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent"></param>
        public ControllerDPad()
            : base()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        public ControllerDPad(int id, string name, List<AnnotationData> annotationDataList)
            : base(id, name)
        {
            _annotationDataList = annotationDataList;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="control"></param>
        public ControllerDPad(ControllerDPad control)
            :base(control)
        {
            if (control._annotationDataList != null)
            {
                foreach (AnnotationData data in control._annotationDataList)
                {
                    _annotationDataList.Add(new AnnotationData(data));
                }
            }
        }

        /// <summary>
        /// Read from Xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            base.FromXml(element);

            // Annotation data
            XmlNodeList annotationElements = element.SelectNodes("annotations/annotation");
            foreach (XmlElement annotationElement in annotationElements)
            {
                AnnotationData annotationData = new AnnotationData();
                annotationData.FromXml(annotationElement);
                _annotationDataList.Add(annotationData);
            }
        }

        /// <summary>
        /// Write to Xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            // Annotation data
            XmlElement annotationsElement = doc.CreateElement("annotations");
            foreach (AnnotationData annotationData in _annotationDataList)
            {
                XmlElement annotationElement = doc.CreateElement("annotation");
                annotationData.ToXml(annotationElement, doc);
                annotationsElement.AppendChild(annotationElement);
            }
            element.AppendChild(annotationsElement);
        }
        
        /// <summary>
        /// Handle new input bindings
        /// </summary>
        public override void SetBoundControls(PhysicalControl[] boundControls)
        {
            base.SetBoundControls(boundControls);

            _isBindingValid = false;
            if (boundControls != null && boundControls.Length == 1 && boundControls[0] != null)
            {
                _isBindingValid = true;
            }
        }

        /// <summary>
        /// Update the control's state from the physical inputs
        /// </summary>
        public override void UpdateState()
        {
            if (_isBindingValid)
            {
                CurrentDirection = BoundControls[0].GetDirectionVal();
            }
        }
    }
}
