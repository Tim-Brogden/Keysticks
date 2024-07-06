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
using SlimDX;
//using System.Diagnostics;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Input;
using Keysticks.Event;

namespace Keysticks.Controls
{
    /// <summary>
    /// Represents an analogue stick control
    /// </summary>
    public class ControllerStick : DirectionalControl
    {
        // Config
        private List<AnnotationData> _annotationDataList = new List<AnnotationData>();
        private bool _useDefaultSensitivity = Constants.DefaultUseDefaultSensitivity;
        private float _userDeadZone;
        private float _defaultDeadZone;
        private float _defaultDeadZoneSquared;
        private float _deadZone = Constants.DefaultStickDeadZoneFraction;
        private float _deadZoneSquared = Constants.DefaultStickDeadZoneFraction * Constants.DefaultStickDeadZoneFraction;
        private KxSourceEventFactory _moveEventFactory;

        // State
        //private int _lastPacketNo = -1;
        private Vector2 _oldValue = new Vector2();
        private Vector2 _value = new Vector2();
        private bool _isBindingValid;

        // Misc
        private float _root2Plus1 = (float)Math.Sqrt(2) + 1f;
        private float _root2Minus1 = (float)Math.Sqrt(2) - 1f;
        private StringUtils _utils = new StringUtils();
        
        // Properties
        public List<AnnotationData> AnnotationDataList { get { return _annotationDataList; } }
        public override EVirtualControlType ControlType { get { return EVirtualControlType.Stick; } }
        protected override bool IsPressableControl { get { return InputMappings != null && InputMappings.Length == 3 && InputMappings[2].InputID != Constants.NoneID; } }
        protected override KxSourceEventArgs TemplateEvent
        {
            get
            {
                KxSourceEventArgs args = new KxSourceEventArgs(Parent.ID,
                                                ControlType,
                                                ID,
                                                EControlSetting.None,
                                                EEventReason.None,
                                                ELRUDState.Centre);
                args.Param0 = new ParamValue(EDataType.Float, 0f);
                args.Param1 = new ParamValue(EDataType.Float, 0f);
                return args;
            }
        }

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
        /// <param name="directionMode"></param>
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
                    case EEventReason.Moved:
                        supported = (args.LRUDState == ELRUDState.Centre); break;
                    case EEventReason.Pressed:
                    case EEventReason.PressedShort:
                    case EEventReason.PressedLong:
                    case EEventReason.PressRepeated:
                    case EEventReason.Released:
                        supported = (args.LRUDState == ELRUDState.Centre) && IsPressableControl; break;
                    case EEventReason.Directed:
                    case EEventReason.Undirected:
                        supported = true; /*Parent.Utils.IsDirectionValid(direction, directionMode, true);*/ break;
                    case EEventReason.DirectedShort:
                    case EEventReason.DirectedLong:
                    case EEventReason.DirectionRepeated:
                        supported = (args.LRUDState != ELRUDState.Centre); /* && Parent.Utils.IsDirectionValid(direction, directionMode, true);*/ break;
                }
            }

            return supported;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ControllerStick()
            :base()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        public ControllerStick(int id, string name, List<AnnotationData> annotationDataList)
            : base(id, name)
        {
            _annotationDataList = annotationDataList;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="control"></param>
        public ControllerStick(ControllerStick control)
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
        /// Initialisation
        /// </summary>
        protected override void Initialise()
        {
            base.Initialise();

            _moveEventFactory = new KxSourceEventFactory(TemplateEvent);
        }

        /// <summary>
        /// Handle change of app config
        /// </summary>
        /// <param name="appConfig"></param>
        public override void SetAppConfig(AppConfig appConfig)
        {
            // Set thumbstick sensitivity
            _useDefaultSensitivity = appConfig.GetBoolVal(Constants.ConfigUseDefaultSensitivity, Constants.DefaultUseDefaultSensitivity);
            _userDeadZone = appConfig.GetFloatVal(Constants.ConfigThumbstickDeadZoneFraction, Constants.DefaultStickDeadZoneFraction);

            _deadZone = _useDefaultSensitivity ? _defaultDeadZone : _userDeadZone;
            _deadZoneSquared = _deadZone * _deadZone;
        }

        /// <summary>
        /// Handle new input bindings
        /// </summary>
        public override void SetBoundControls(PhysicalControl[] boundControls)
        {
 	        base.SetBoundControls(boundControls);

            _isBindingValid = false;
            if (boundControls != null && boundControls.Length == 3 &&
                (boundControls[0] != null || boundControls[1] != null || boundControls[2] != null))
            {
                // Set default dead zone
                if (boundControls[0] != null)
                {
                    if (boundControls[1] != null)
                    {
                        _defaultDeadZoneSquared = boundControls[0].DeadZone * boundControls[1].DeadZone;
                    }
                    else
                    {
                        _defaultDeadZoneSquared = boundControls[0].DeadZone * boundControls[0].DeadZone;
                    }
                }
                else if (boundControls[1] != null)
                {
                    _defaultDeadZoneSquared = boundControls[1].DeadZone * boundControls[1].DeadZone;
                }
                else
                {
                    _defaultDeadZoneSquared = Constants.DefaultStickDeadZoneFraction * Constants.DefaultStickDeadZoneFraction;
                }

                _defaultDeadZone = _defaultDeadZoneSquared > 1E-6 ? (float)Math.Sqrt(_defaultDeadZoneSquared) : 0f;

                // Apply dead zone if required
                if (_useDefaultSensitivity)
                {
                    _deadZone = _defaultDeadZone;
                    _deadZoneSquared = _defaultDeadZoneSquared;
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
                // Get the current thumbstick co-ords
                // Input mapping 0 specifies X axis, mapping 1 specifies Y axis                
                _value.X = (BoundControls[0] != null) ? BoundControls[0].GetFloatVal(InputMappings[0].Options) : 0f;
                _value.Y = (BoundControls[1] != null) ? BoundControls[1].GetFloatVal(InputMappings[1].Options) : 0f;

                // Apply deadzone
                float lenSquared = _value.LengthSquared();                
                if (lenSquared < _deadZoneSquared)
                {
                    _value.X = 0f;
                    _value.Y = 0f;
                    CurrentDirection = ELRUDState.Centre;
                }
                else
                {
                    // Rescale to remove deadzone, if non-zero
                    if (lenSquared > 1E-6 && _deadZone < 1f)
                    {
                        float radius = (float)Math.Sqrt(lenSquared);
                        float newRadius = Math.Min(1f, (radius - _deadZone) / (1f - _deadZone));
                        float enlargementFactor = newRadius / radius;
                        _value.X *= enlargementFactor;
                        _value.Y *= enlargementFactor;
                    }

                    CurrentDirection = VectorToDirections(ref _value);
                }                

                // Get the current button state
                IsPressed = (BoundControls[2] != null) ? BoundControls[2].GetBoolVal() : false;
            }
        }

        /// <summary>
        /// Handle new state and generate any events
        /// </summary>
        /// <param name="newState"></param>
        public override void RaiseEvents(IStateManager stateManager)
        {
            // Raise button and direction events if reqd
            base.RaiseEvents(stateManager);
            
            // Raise move event if reqd
            if (DirectionMode == EDirectionMode.Continuous && IsEventTypeEnabled(EEventReason.Moved))
            {
                KxSourceEventArgs args;
                if (!_value.Equals(_oldValue))
                {                
                    args = _moveEventFactory.Create();
                    args.EventReason = EEventReason.Moved;
                    args.Param0.Value = _value.X;
                    args.Param1.Value = _value.Y;

                    RaiseInputEvent(args);
                }

                _oldValue.X = _value.X;
                _oldValue.Y = _value.Y;
            }
        }

        /// <summary>
        /// Get a left-right-up-down direction from an x-y vector
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private ELRUDState VectorToDirections(ref Vector2 value)
        {
            ELRUDState directions = ELRUDState.Centre;

            // See whether 4-way or 8-way directions are required
            float absY = Math.Abs(value.Y);
            float absX = Math.Abs(value.X);
            if (DirectionMode == EDirectionMode.FourWay)
            {
                // 4-way

                if (absY < absX)
                {
                    // Left or right
                    directions = (value.X > 0f) ? ELRUDState.Right : ELRUDState.Left;
                }
                else
                {
                    // Up or down
                    directions = (value.Y > 0f) ? ELRUDState.Up : ELRUDState.Down;
                }
            }
            else
            {
                // 8-way or Axis-style

                // Tan 22.5 deg = root(2) - 1
                // Tan 67.5 deg = root(2) + 1
                if (absY < absX * _root2Minus1)
                {
                    // Left or right
                    directions = (value.X > 0f) ? ELRUDState.Right : ELRUDState.Left;
                }
                else if (absY > absX * _root2Plus1)
                {
                    // Up or down
                    directions = (value.Y > 0f) ? ELRUDState.Up : ELRUDState.Down;
                }
                else
                {
                    // Diagonal
                    directions = (value.X > 0f) ? ELRUDState.Right : ELRUDState.Left;
                    directions |= (value.Y > 0f) ? ELRUDState.Up : ELRUDState.Down;
                }
            }            

            //Trace.WriteLine(directions.ToString());

            return directions;
        }

    }
}
