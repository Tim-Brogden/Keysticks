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
using System.Windows;
using System.Xml;
using Keysticks.Config;
using Keysticks.Controls;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Input;

namespace Keysticks.Sources
{
    /// <summary>
    /// Represents a virtual input source (virtual controller)
    /// </summary>
    public class BaseSource : NamedItem
    {
        // Fields
        private Profile _profile;
        private IStateManager _stateManager;
        private DisplayData _displayData;
        private List<BaseControl> _inputControls = new List<BaseControl>();
        private StateCollection _states;
        private AutoActivationList _autoActivations;
        private NamedItemList _physicalInputs = new NamedItemList();
        private ActionSetCollection _actions;
        private NamedItemList _buttons = new NamedItemList();
        private NamedItemList _triggers = new NamedItemList();
        private NamedItemList _sticks = new NamedItemList();
        private NamedItemList _dpads = new NamedItemList();
        private NamedItemList _buttonDiamonds = new NamedItemList();
        private StringUtils _utils = new StringUtils();
        private SourceUtils _sourceUtils;
        private ProfileSituationEditor _stateEditor;
        private ProfileActionEditor _actionEditor;
        private StateVector _currentState;
        private bool _inputStateChanged;
        private ActionMappingTable _currentActionMappings;
        private bool _autoActivatedState;

        // Properties
        public Profile Profile { get { return _profile; } set { _profile = value; } }
        public DisplayData Display { get { return _displayData; } set { _displayData = value; } }
        public List<BaseControl> InputControls { get { return _inputControls; } }
        public StateCollection StateTree { get { return _states; } }
        public AutoActivationList AutoActivations { get { return _autoActivations; } }
        public NamedItemList PhysicalInputs { get { return _physicalInputs; } }
        public ActionSetCollection Actions { get { return _actions; } }
        public NamedItemList Buttons { get { return _buttons; } }
        public NamedItemList DPads { get { return _dpads; } }
        public NamedItemList Sticks { get { return _sticks; } }
        public NamedItemList Triggers { get { return _triggers; } }
        public NamedItemList ButtonDiamonds { get { return _buttonDiamonds; } }
        public StateVector CurrentState { get { return _currentState; } }
        public SourceUtils Utils { get { return _sourceUtils; } }
        public ProfileSituationEditor StateEditor { get { return _stateEditor; } }
        public ProfileActionEditor ActionEditor { get { return _actionEditor; } }
        public bool IsStateChanged { get { return _inputStateChanged; } }
        public bool IsModified
        {
            set
            {
                if (_profile != null)
                {
                    _profile.IsModified = value;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node"></param>
        public BaseSource()
            : base()
        {
            _displayData = new DisplayData();
            _sourceUtils = new SourceUtils(this);
            _stateEditor = new ProfileSituationEditor(this);
            _actionEditor = new ProfileActionEditor(this);            
            _states = new StateCollection(this);
            _autoActivations = new AutoActivationList(this);
            _actions = new ActionSetCollection(this);
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public BaseSource(int id, string name)
            : base(id, name)
        {
            _displayData = new DisplayData();
            _sourceUtils = new SourceUtils(this);
            _stateEditor = new ProfileSituationEditor(this);
            _actionEditor = new ProfileActionEditor(this);
            _states = new StateCollection(this);
            _autoActivations = new AutoActivationList(this);
            _actions = new ActionSetCollection(this);
        }

        /// <summary>
        /// Handle keyboard layout change
        /// </summary>
        public void Initialise(IKeyboardContext context)
        {
            _actions.Initialise(context);
        }        

        /// <summary>
        /// Handle change of app config
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            foreach (BaseControl control in _inputControls)
            {
                control.SetAppConfig(appConfig);
            }
            _actionEditor.SetAppConfig(appConfig);
            _actions.SetAppConfig(appConfig);
        }

        /// <summary>
        /// Set the state manager
        /// </summary>
        /// <param name="stateManager"></param>
        public void SetStateManager(IStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        /// <summary>
        /// Add a control
        /// </summary>
        /// <param name="button"></param>
        public void AddControl(BaseControl control)
        {
            control.SetParent(this);
            _inputControls.Add(control);
            switch (control.ControlType)
            {
                case EVirtualControlType.Button:
                    _buttons.Add((ControllerButton)control); break;
                case EVirtualControlType.ButtonDiamond:
                    _buttonDiamonds.Add((ControllerButtonDiamond)control); break;
                case EVirtualControlType.DPad:
                    _dpads.Add((ControllerDPad)control); break;
                case EVirtualControlType.Stick:
                    _sticks.Add((ControllerStick)control); break;
                case EVirtualControlType.Trigger:
                    _triggers.Add((ControllerTrigger)control); break;
            }
            IsModified = true;
        }

        /// <summary>
        /// Remove a control
        /// </summary>
        /// <param name="control"></param>
        public void RemoveControl(BaseControl control)
        {
            switch (control.ControlType)
            {
                case EVirtualControlType.Button:
                    _buttons.Remove(control); break;
                case EVirtualControlType.ButtonDiamond:
                    _buttonDiamonds.Remove(control); break;
                case EVirtualControlType.DPad:
                    _dpads.Remove(control); break;
                case EVirtualControlType.Stick:
                    _sticks.Remove(control); break;
                case EVirtualControlType.Trigger:
                    _triggers.Remove(control); break;
            }
            _inputControls.Remove(control);
            IsModified = true;
            
            // Validate modes with grids
            Validate();
        }

        /// <summary>
        /// Import controls from another source
        /// </summary>
        /// <param name="fromSource"></param>
        public void CopyControllerDesign(BaseSource fromSource)
        {
            // Copy the display settings
            _displayData = new DisplayData(fromSource.Display);

            // Copy the controls
            _inputControls.Clear();
            _buttons.Clear();
            _buttonDiamonds.Clear();
            _dpads.Clear();
            _sticks.Clear();
            _triggers.Clear();
            foreach (BaseControl control in fromSource.InputControls)
            {
                switch (control.ControlType)
                {
                    case EVirtualControlType.Button:
                        AddControl(new ControllerButton((ControllerButton)control)); break;
                    case EVirtualControlType.ButtonDiamond:
                        AddControl(new ControllerButtonDiamond((ControllerButtonDiamond)control)); break;
                    case EVirtualControlType.DPad:
                        AddControl(new ControllerDPad((ControllerDPad)control)); break;
                    case EVirtualControlType.Stick:
                        AddControl(new ControllerStick((ControllerStick)control)); break;
                    case EVirtualControlType.Trigger:
                        AddControl(new ControllerTrigger((ControllerTrigger)control)); break;
                }
            }
            IsModified = true;

            // Validate
            Validate();
        }
        
        /// <summary>
        /// Get the specified input control
        /// </summary>
        /// <param name="inputControl"></param>
        /// <returns></returns>
        public BaseControl GetVirtualControl(KxControlEventArgs ev)
        {
            BaseControl control = null;
            switch (ev.ControlType)
            {
                case EVirtualControlType.Button:
                    control = (BaseControl)_buttons.GetItemByID(ev.ControlID);
                    break;
                case EVirtualControlType.DPad:
                    control = (BaseControl)_dpads.GetItemByID(ev.ControlID);
                    break;
                case EVirtualControlType.Stick:
                    control = (BaseControl)_sticks.GetItemByID(ev.ControlID);
                    break;
                case EVirtualControlType.Trigger:
                    control = (BaseControl)_triggers.GetItemByID(ev.ControlID);
                    break;
                case EVirtualControlType.ButtonDiamond:
                    control = (BaseControl)_buttonDiamonds.GetItemByID(ev.ControlID); ;
                    break;
            }            

            return control;
        }

        /// <summary>
        /// Calculate the bounding rectangle containing the all the virtual control annotations
        /// </summary>
        /// <returns></returns>
        public Rect GetBoundingRect()
        {
            List<AnnotationData> allAnnotations = GetAnnotationDataList();
            return _displayData.GetBoundingRect(allAnnotations);
        }

        /// <summary>
        /// Calculate enclosing polygon
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public List<Point> GetBoundingPolygon(Point origin)
        {
            List<AnnotationData> allAnnotations = GetAnnotationDataList();
            return _displayData.GetBoundingPolygon(allAnnotations, origin);
        }

        /// <summary>
        /// Get the annotation data for all controls
        /// </summary>
        /// <returns></returns>
        private List<AnnotationData> GetAnnotationDataList()
        {
            List<AnnotationData> allAnnotationData = new List<AnnotationData>();

            // Add the virtual control annotation data
            foreach (ControllerTrigger control in _triggers)
            {
                allAnnotationData.Add(control.AnnotationData);
            }
            foreach (ControllerButton control in _buttons)
            {
                allAnnotationData.Add(control.AnnotationData);
            }
            foreach (ControllerDPad control in _dpads)
            {
                allAnnotationData.AddRange(control.AnnotationDataList);
            }
            foreach (ControllerStick control in _sticks)
            {
                allAnnotationData.AddRange(control.AnnotationDataList);
            }

            // Add the title bar and menu annotation data
            allAnnotationData.Add(_displayData.TitleBarData);
            allAnnotationData.Add(_displayData.MenuBarData);

            return allAnnotationData;
        }

        /// <summary>
        /// Set the active process and window
        /// </summary>
        /// <param name="processNameLower">lower case reqd</param>
        /// <param name="windowTitleLower">lower case reqd</param>
        public void SetCurrentWindow(string processNameLower, string windowTitleLower)
        {
            StateVector stateToActivate = _autoActivations.GetActivation(processNameLower, windowTitleLower);
            if (stateToActivate != null)
            {
                // Auto-activate state
                SetCurrentState(stateToActivate);
                _autoActivatedState = true;
            }
            else
            {
                if (_autoActivatedState && _autoActivations.DefaultState != null)
                {
                    // Restore default state
                    SetCurrentState(_autoActivations.DefaultState);
                }
                _autoActivatedState = false;
            }
        }

        /// <summary>
        /// Update the current logical state of the source
        /// </summary>
        /// <param name="relativeState"></param>
        public void SetCurrentState(StateVector relativeState)
        {
            StateVector oldState = _currentState;
            StateVector newState = _sourceUtils.RelativeStateToAbsolute(relativeState, oldState);
            _sourceUtils.MakeStateSpecific(newState);

            // Check whether the new state is different
            if (oldState == null || newState == null || !newState.IsSameAs(oldState))
            {
                // Update the state
                _currentState = newState;

                // Deactivate any controls that no longer apply
                if (_currentActionMappings != null)
                {
                    DeactivateControls(_currentActionMappings, newState);
                    _currentActionMappings = null;
                }

                if (newState != null)
                {
                    // Report state change
                    KxStateChangeEventArgs args = new KxStateChangeEventArgs(ID, new StateVector(newState));
                    _stateManager.ThreadManager.SubmitUIEvent(args);

                    // Calculate current action set
                    _currentActionMappings = _actions.GetActionsForState(newState, true);

                    // Activate new action set
                    ActivateControls(_currentActionMappings, oldState);
                }
            }
        }

        /// <summary>
        /// Update the state of the source from its physical inputs
        /// </summary>
        /// <param name="stateManager"></param>
        public bool UpdateState()
        {
            bool success = true;

            // Update physical inputs
            _inputStateChanged = false;
            foreach (PhysicalInput input in _physicalInputs)
            {
                success &= input.UpdateState();
                _inputStateChanged |= input.IsStateChanged;
            }

            // Update controls
            if (_inputStateChanged)
            {
                foreach (BaseControl control in _inputControls)
                {
                    control.UpdateState();
                }
            }

            return success;
        }

        /// <summary>
        /// Handle new state and generate any events
        /// </summary>
        /// <param name="stateManager"></param>
        public void RaiseEvents(IStateManager stateManager)
        {
            foreach (BaseControl control in _inputControls)
            {
                if (control.IsActive)
                {
                    control.RaiseEvents(stateManager);                    
                }
            }
        }

        /// <summary>
        /// Handle an event triggered by an input source and perform the appropriate actions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="eventTypeID"></param>
        public void HandleInputEvent(object sender, KxSourceEventArgs args)
        {
            // Report certain event types
            switch (args.EventReason)
            {
                case EEventReason.Directed:
                case EEventReason.Undirected:
                case EEventReason.Pressed:
                case EEventReason.Released:
                    _stateManager.ThreadManager.SubmitUIEvent(new KxSourceEventArgs(args));
                    break;
            }

            // Perform actions required
            bool ongoing = false;
            if (_currentActionMappings != null)
            {
                ActionSet actionSet = _currentActionMappings.GetActions(args.ToID(), true);
                if (actionSet != null && actionSet.IsActive)
                {
                    ActionList actionList = actionSet.GetActions(args.EventReason);
                    if (actionList != null)
                    {
                        // Start the action list
                        if (actionList.IsOngoing)
                        {
                            // Already started - restart
                            actionList.CurrentEvent.InUse = false;
                            //Trace.WriteLine("Free - restarted");
                            actionList.Start(_stateManager, args);
                        }
                        else
                        {
                            actionList.Start(_stateManager, args);

                            // Add to ongoing actions if required
                            if (actionList.IsOngoing)
                            {
                                _stateManager.AddOngoingActions(actionList);
                            }
                        }
                        ongoing = actionList.IsOngoing;
                    }
                }
            }

            if (!ongoing)
            {
                // Free up this event object
                //Trace.WriteLine("Free - not ongoing");
                args.InUse = false;
            }
        }

        /// <summary>
        /// Read from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            base.FromXml(element);

            // Display data
            XmlElement displayElement = (XmlElement)element.SelectSingleNode("display");
            if (displayElement != null)
            {
                _displayData = new DisplayData();
                _displayData.FromXml(displayElement);
            }

            // DPads
            XmlNodeList dpadElementList = element.SelectNodes("dpads/dpad");
            foreach (XmlElement dpadElement in dpadElementList)
            {
                ControllerDPad dpad = new ControllerDPad();
                dpad.FromXml(dpadElement);
                AddControl(dpad);
            }

            // Sticks
            XmlNodeList stickElementList = element.SelectNodes("sticks/stick");
            foreach (XmlElement stickElement in stickElementList)
            {
                ControllerStick stick = new ControllerStick();
                stick.FromXml(stickElement);
                AddControl(stick);
            }

            // Triggers
            XmlNodeList triggerElementList = element.SelectNodes("triggers/trigger");
            foreach (XmlElement triggerElement in triggerElementList)
            {
                ControllerTrigger trigger = new ControllerTrigger();
                trigger.FromXml(triggerElement);
                AddControl(trigger);
            }

            // Buttons
            XmlNodeList buttonElementList = element.SelectNodes("buttons/button");
            foreach (XmlElement buttonElement in buttonElementList)
            {
                ControllerButton button = new ControllerButton();
                button.FromXml(buttonElement);
                AddControl(button);
            }

            // Button diamonds
            XmlNodeList diamondElementList = element.SelectNodes("diamonds/diamond");
            foreach (XmlElement diamondElement in diamondElementList)
            {
                ControllerButtonDiamond diamond = new ControllerButtonDiamond();
                diamond.FromXml(diamondElement);
                AddControl(diamond);
            }

            // Read physical inputs
            XmlNodeList inputNodes = element.SelectNodes("inputs/input");
            foreach (XmlElement inputNode in inputNodes)
            {
                PhysicalInput input = new PhysicalInput();
                input.FromXml(inputNode);
                _physicalInputs.Add(input);
            }

            // Read state tree
            XmlElement stateTreeElement = (XmlElement)element.SelectSingleNode("statetree");
            _states.FromXml(stateTreeElement);

            // Read auto-activations
            XmlElement activationsElement = (XmlElement)element.SelectSingleNode("activations");
            _autoActivations.FromXml(activationsElement);

            // Read action sets
            XmlElement actionSetsNode = (XmlElement)element.SelectSingleNode("actionsets");
            _actions.FromXml(actionSetsNode);
        }

        /// <summary>
        /// Write to xml
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            XmlElement displayElement = doc.CreateElement("display");
            _displayData.ToXml(displayElement, doc);
            element.AppendChild(displayElement);

            // Buttons
            XmlElement buttonsElement = doc.CreateElement("buttons");
            foreach (ControllerButton button in _buttons)
            {
                XmlElement buttonElement = doc.CreateElement("button");
                button.ToXml(buttonElement, doc);
                buttonsElement.AppendChild(buttonElement);
            }
            element.AppendChild(buttonsElement);

            // Triggers
            XmlElement triggersElement = doc.CreateElement("triggers");
            foreach (ControllerTrigger trigger in _triggers)
            {
                XmlElement triggerElement = doc.CreateElement("trigger");
                trigger.ToXml(triggerElement, doc);
                triggersElement.AppendChild(triggerElement);
            }
            element.AppendChild(triggersElement);

            // DPads
            XmlElement dpadsElement = doc.CreateElement("dpads");
            foreach (ControllerDPad dpad in _dpads)
            {
                XmlElement dpadElement = doc.CreateElement("dpad");
                dpad.ToXml(dpadElement, doc);
                dpadsElement.AppendChild(dpadElement);
            }
            element.AppendChild(dpadsElement);

            // Sticks
            XmlElement sticksElement = doc.CreateElement("sticks");
            foreach (ControllerStick stick in _sticks)
            {
                XmlElement stickElement = doc.CreateElement("stick");
                stick.ToXml(stickElement, doc);
                sticksElement.AppendChild(stickElement);
            }
            element.AppendChild(sticksElement);

            // Button diamonds
            XmlElement diamondsElement = doc.CreateElement("diamonds");
            foreach (ControllerButtonDiamond diamond in _buttonDiamonds)
            {
                XmlElement diamondElement = doc.CreateElement("diamond");
                diamond.ToXml(diamondElement, doc);
                diamondsElement.AppendChild(diamondElement);
            }
            element.AppendChild(diamondsElement);

            // Physical inputs
            XmlElement inputsElement = doc.CreateElement("inputs");
            foreach (PhysicalInput input in _physicalInputs)
            {
                XmlElement inputElement = doc.CreateElement("input");
                input.ToXml(inputElement, doc);
                inputsElement.AppendChild(inputElement);
            }
            element.AppendChild(inputsElement);

            // State tree
            XmlElement stateTreeElement = doc.CreateElement("statetree");
            _states.ToXml(stateTreeElement, doc);
            element.AppendChild(stateTreeElement);

            // Auto-activations
            XmlElement activationsElement = doc.CreateElement("activations");
            _autoActivations.ToXml(activationsElement, doc);
            element.AppendChild(activationsElement);

            // Action sets
            XmlElement actionSetsElement = doc.CreateElement("actionsets");
            _actions.ToXml(actionSetsElement, doc);
            element.AppendChild(actionSetsElement);
        }

        /// <summary>
        /// Activate actions
        /// </summary>
        /// <param name="actionMappings"></param>
        /// <param name="previousState"></param>
        private void ActivateControls(ActionMappingTable actionMappings, StateVector previousState)
        {
            // See if we're entering the state for these controls i.e. they're not currently applicable
            if (previousState == null || !actionMappings.State.Contains(previousState))
            {
                // Apply more general parent controls first, so that they can be overridden if appropriate
                if (actionMappings.ParentTablesList != null)
                {
                    foreach (ActionMappingTable table in actionMappings.ParentTablesList)
                    {
                        ActivateControls(table, previousState);
                    }
                }

                // Activate these controls
                Dictionary<int, ActionSet>.Enumerator eActionList = actionMappings.GetEnumerator();
                while (eActionList.MoveNext())
                {
                    ActionSet actionSet = eActionList.Current.Value;
                    if (!actionSet.IsActive)
                    {
                        // Activate action set
                        KxSourceEventArgs args = new KxSourceEventArgs(ID, EEventReason.None, actionSet.EventArgs);
                        actionSet.Activate(_stateManager, args);

                        // Enable or disable certain input events
                        BaseControl control = GetVirtualControl(actionSet.EventArgs);
                        if (control != null)
                        {
                            control.EnableInputEvents(actionSet, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deactivate actions
        /// </summary>
        /// <param name="actionMappings"></param>
        /// <param name="newState"></param>
        private void DeactivateControls(ActionMappingTable actionMappings, StateVector newState)
        {
            // See if we're entering the state for these controls i.e. they're not currently applicable
            if (newState == null || !actionMappings.State.Contains(newState))
            {
                // Deactivate these controls
                Dictionary<int, ActionSet>.Enumerator eActionList = actionMappings.GetEnumerator();
                while (eActionList.MoveNext())
                {
                    // Update whether action list is active or not
                    ActionSet actionSet = eActionList.Current.Value;
                    if (actionSet.IsActive)
                    {
                        // Deactivate action set
                        KxSourceEventArgs args = new KxSourceEventArgs(ID, EEventReason.None, actionSet.EventArgs);
                        actionSet.Deactivate(_stateManager, args);

                        // Disable certain input events
                        BaseControl control = GetVirtualControl(actionSet.EventArgs);
                        if (control != null)
                        {
                            control.EnableInputEvents(actionSet, false);
                        }
                    }
                }

                // Deactivate more general parent controls second, in the reverse order in which they were activated
                if (actionMappings.ParentTablesList != null)
                {
                    for (int i = actionMappings.ParentTablesList.Count - 1; i != -1; i--)
                    {
                        DeactivateControls(actionMappings.ParentTablesList[i], newState);
                    }
                }
            }
        }

        /// <summary>
        /// Validate the source by removing invalid states and actions
        /// </summary>
        /// <param name="control"></param>
        public void Validate()
        {
            _states.Validate();
            _actions.Validate();
        }
    }
}
