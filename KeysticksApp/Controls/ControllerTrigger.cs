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
using System.Collections.Generic;
using System.Xml;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Input;

namespace Keysticks.Controls
{
    /// <summary>
    /// Represents a trigger / slider control
    /// </summary>
    public class ControllerTrigger : BaseControl
    {
        // Config
        private bool _useDefaultSensitivity = Constants.DefaultUseDefaultSensitivity;
        private float _userDeadZone;
        private float _defaultDeadZone;
        private float _deadZone = Constants.DefaultTriggerDeadZoneFraction;
        private AnnotationData _annotationData = new AnnotationData();

        // State
        private float _value;
        private bool _isBindingValid;

        // Misc
        private List<EEventReason> _supportedReasons = new List<EEventReason> { EEventReason.Pressed, 
                                                                                EEventReason.PressedShort,
                                                                                EEventReason.PressedLong,
                                                                                EEventReason.PressRepeated,
                                                                                EEventReason.Released };
        private List<EEventReason> _supportedSettingReasons = new List<EEventReason> { EEventReason.Activated };

        // Properties
        public AnnotationData AnnotationData { get { return _annotationData; } }
        public override EVirtualControlType ControlType { get { return EVirtualControlType.Trigger; } }
        protected override bool IsPressableControl { get { return true; } }     

        /// <summary>
        /// Get the event reasons supported by this control
        /// </summary>
        public override List<EEventReason> GetSupportedEventReasons(KxControlEventArgs args/*, EDirectionMode directionMode*/)
        {
            return args.SettingType == EControlSetting.None ? _supportedReasons : _supportedSettingReasons;
        }

        /// <summary>
        /// Get whether reason is supported
        /// </summary>
        /// <param name="args"></param>
        /// <param name="directionMode"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public override bool IsReasonSupported(KxControlEventArgs args, /*EDirectionMode directionMode,*/ EEventReason reason)
        {
            return args.SettingType == EControlSetting.None ? _supportedReasons.Contains(reason) : _supportedSettingReasons.Contains(reason);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ControllerTrigger()
            : base()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        public ControllerTrigger(int id, string name, AnnotationData annotationData)
            : base(id, name)
        {
            _annotationData = annotationData;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="control"></param>
        public ControllerTrigger(ControllerTrigger control)
            : base(control)
        {
            if (control._annotationData != null)
            {
                _annotationData = new AnnotationData(control._annotationData);
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
            XmlElement annotationElement = (XmlElement)element.SelectSingleNode("annotations/annotation");
            _annotationData.FromXml(annotationElement);
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
            XmlElement annotationElement = doc.CreateElement("annotation");
            _annotationData.ToXml(annotationElement, doc);
            annotationsElement.AppendChild(annotationElement);
            element.AppendChild(annotationsElement);
        }

        /// <summary>
        /// Handle change of app config
        /// </summary>
        /// <param name="appConfig"></param>
        public override void SetAppConfig(AppConfig appConfig)
        {
            // Deadzone
            _useDefaultSensitivity = appConfig.GetBoolVal(Constants.ConfigUseDefaultSensitivity, Constants.DefaultUseDefaultSensitivity);
            _userDeadZone = appConfig.GetFloatVal(Constants.ConfigTriggerDeadZoneFraction, Constants.DefaultTriggerDeadZoneFraction);

            _deadZone = _useDefaultSensitivity ? _defaultDeadZone : _userDeadZone;            
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
                // Set dead zone if required
                _defaultDeadZone = boundControls[0].DeadZone;
                if (_useDefaultSensitivity)
                {
                    _deadZone = _defaultDeadZone;
                }

                _isBindingValid = true;
            }
        }

        /// <summary>
        /// Update the control's state from the physical inputs
        /// </summary>
        /// <param name="newState"></param>
        public override void UpdateState()
        {
            if (_isBindingValid)
            {
                // Get the current trigger value
                _value = BoundControls[0].GetFloatVal(InputMappings[0].Options);

                // Apply deadzone
                if (_value < _deadZone)
                {
                    _value = 0f;
                }
                else
                {
                    // Rescale to remove deadzone
                    _value = (_value - _deadZone) / (1f - _deadZone);
                }

                IsPressed = _value != 0f;
            }
        }        
    }
}
