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
using System.Windows.Media;
using Keysticks.Actions;
using Keysticks.Config;
using Keysticks.Controls;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Sources;
using Keysticks.UI;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Settings tab control - displays directionality and timing information for virtual controls
    /// </summary>
    public partial class SituationSettingsControl : KxUserControl, IActionViewerControl
    {
        // Fields
        private bool _isLoaded = false;
        private StringUtils _utils = new StringUtils();
        private ColourScheme _colourScheme;
        private BaseSource _source;
        private ControlAnnotationControl _selectedAnnotation;
        private string _defaultDirectionModeText;
        private string _defaultDwellRepeatText;
        private StateVector _currentSituation;
        private KxControlEventArgs _currentInputEvent;
        private NamedItemList _directionalityBindings;
        private NamedItemList _timingBindings;

        // Dependency properties
        public bool IsDesignMode
        {
            get { return (bool)GetValue(IsDesignModeProperty); }
            set { SetValue(IsDesignModeProperty, value); }
        }
        private static readonly DependencyProperty IsDesignModeProperty =
            DependencyProperty.Register(
            "IsDesignMode",
            typeof(bool),
            typeof(SituationSettingsControl),
            new FrameworkPropertyMetadata(false)
        );

        // Properties
        public StateVector CurrentSituation { get { return _currentSituation; } }
        public KxControlEventArgs CurrentInputEvent { get { return _currentInputEvent; } }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public SituationSettingsControl()
            : base()
        {
            InitializeComponent();

            // Bind controls
            _timingBindings = new NamedItemList();
            ControlsTable.ItemsSource = _timingBindings;
            TimingsTable.ItemsSource = _timingBindings;
            _directionalityBindings = new NamedItemList();
            DirectionModesTable.ItemsSource = _directionalityBindings;
            
            // Default text
            SetDirectionModeAction defaultDirectionModeAction = new SetDirectionModeAction();
            _defaultDirectionModeText = string.Format("({0})", defaultDirectionModeAction.TinyName);
            SetDwellAndAutorepeatAction defaultDwellRepeatAction = new SetDwellAndAutorepeatAction();
            _defaultDwellRepeatText = string.Format("({0})", defaultDwellRepeatAction.TinyName);
            
        }

        /// <summary>
        /// Acquire the focus
        /// </summary>
        public void SetFocus()
        {
            OuterPanel.Focus();
        }
        
        /// <summary>
        /// Set app config
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            // Set colours
            _colourScheme = appConfig.ColourScheme;
            UpdateBrushes();
        }

        /// <summary>
        /// Update the brush colours / opacities
        /// </summary>
        private void UpdateBrushes()
        {
            if (_colourScheme != null && _source != null)
            {
                PlayerColourScheme playerColours = _colourScheme.GetPlayerColours(_source.ID);

                Color selectionColor = ColourUtils.ColorFromString(playerColours.SelectionColour, Constants.DefaultSelectionColour);
                this.Resources["SelectionBrush"] = new SolidColorBrush(selectionColor);
            }
        }

        /// <summary>
        /// Set the current source
        /// </summary>
        /// <param name="profile"></param>
        public void SetSource(BaseSource source)
        {
            _source = source;

            UpdateBrushes();
            if (_isLoaded)
            {
                CreateDisplay();
            }
        }

        /// <summary>
        /// Handle change of situation
        /// </summary>
        /// <param name="args"></param>
        public void SetCurrentSituation(StateVector situation)
        {
            if (IsDesignMode && situation.CellID != Constants.DefaultID)
            {
                situation = new StateVector(situation);
                situation.CellID = Constants.DefaultID;

                RaiseEvent(new KxStateRoutedEventArgs(KxUserControl.KxStateChangedEvent, situation));
            }

            _currentSituation = situation;     
            RefreshDisplay();
        }        

        /// <summary>
        /// Animate input control
        /// </summary>
        /// <param name="args"></param>
        public void AnimateInputEvent(KxSourceEventArgs inputEvent)
        {
            // Do nothing
        }

        /// <summary>
        /// Unhighlight all controls
        /// </summary>
        public void ClearAnimations()
        {
            // Do nothing
        }        
        
        /// <summary>
        /// Create the annotation bindings
        /// </summary>
        public void CreateDisplay()
        {
            if (_source != null)
            {
                // Create bindings for the source
                _directionalityBindings.Clear();
                _timingBindings.Clear();
                AddBindingsForControls(_source.DPads);
                AddBindingsForControls(_source.Sticks);
                AddBindingsForControls(_source.Triggers);
                AddBindingsForControls(_source.Buttons);

                // Refresh for current situation if required
                RefreshDisplay();

                // Clear current selection if no longer valid
                if (_currentInputEvent != null && _source.GetVirtualControl(_currentInputEvent) == null)
                {
                    _currentInputEvent = null;
                    RaiseEvent(new KxInputControlRoutedEventArgs(KxInputControlChangedEvent, _currentInputEvent));
                }
            }
        }

        /// <summary>
        /// Add the directionality and timing bindings for the controls in a list
        /// </summary>
        /// <param name="controlsList"></param>
        private void AddBindingsForControls(NamedItemList controlsList)
        {
            foreach (BaseControl control in controlsList)
            {
                switch (control.ControlType)
                {
                    case EVirtualControlType.DPad:
                    case EVirtualControlType.Stick:
                        {
                            KxControlEventArgs args = new KxControlEventArgs(control.ControlType, control.ID, EControlSetting.DwellAndRepeat, ELRUDState.None);
                            SettingDisplayItem displayItem = new SettingDisplayItem(args.ToID(), control.Name, args, "", null, IsDesignMode);
                            _timingBindings.Add(displayItem);

                            args = new KxControlEventArgs(control.ControlType, control.ID, EControlSetting.DirectionMode, ELRUDState.None);
                            displayItem = new SettingDisplayItem(args.ToID(), control.Name, args, "", null, IsDesignMode);
                            _directionalityBindings.Add(displayItem);
                        }
                        break;
                    case EVirtualControlType.Button:
                    case EVirtualControlType.Trigger:
                        {
                            KxControlEventArgs args = new KxControlEventArgs(control.ControlType, control.ID, EControlSetting.DwellAndRepeat, ELRUDState.None);
                            SettingDisplayItem displayItem = new SettingDisplayItem(args.ToID(), control.Name, args, "", null, IsDesignMode);
                            _timingBindings.Add(displayItem);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;
                CreateDisplay();
            }
        }
        
        /// <summary>
        /// Handle selection of annotation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Annotation_Clicked(object sender, RoutedEventArgs e)
        {
            if (IsDesignMode)
            {
                // Set the keyboard focus
                SetFocus();
                SelectAnnotation((ControlAnnotationControl)sender);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle selection of annotation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Annotation_RightClicked(object sender, RoutedEventArgs e)
        {
            if (IsDesignMode)
            {
                // Set the keyboard focus
                SetFocus();
                SelectAnnotation((ControlAnnotationControl)sender);

                RaiseEvent(new RoutedEventArgs(KxQuickEditActionsEvent));
            }

            e.Handled = true;
        }
        
        /// <summary>
        /// Refresh the annotations
        /// </summary>
        public void RefreshDisplay()
        {
            if (_isLoaded && _source != null && _currentSituation != null)
            {
                ActionMappingTable actionMappings = _source.Actions.GetActionsForState(_currentSituation, true);

                // Loop over timing settings
                foreach (SettingDisplayItem displayItem in _timingBindings)
                {
                    ActionSet actionSet = actionMappings.GetActions(displayItem.ID, true);
                    BindAnnotationActions(displayItem, actionSet, _defaultDwellRepeatText);
                }

                // Loop over directionality settings
                foreach (SettingDisplayItem displayItem in _directionalityBindings)
                {
                    ActionSet actionSet = actionMappings.GetActions(displayItem.ID, true);
                    BindAnnotationActions(displayItem, actionSet, _defaultDirectionModeText);
                }
            }
        }

        /// <summary>
        /// Bind actions to an annotation
        /// </summary>
        /// <param name="annotation"></param>
        /// <param name="actionSet"></param>
        private void BindAnnotationActions(SettingDisplayItem displayItem, ActionSet actionSet, string defaultText)
        {
            displayItem.Text = actionSet != null ? actionSet.Info.TinyDescription : defaultText;
            if (IsDesignMode)
            {
                displayItem.ToolTip = string.Format("{0}{1}{2}",
                                                                actionSet != null ? actionSet.Info.ShortDescription : Properties.Resources.String_DefaultSettings,
                                                                Environment.NewLine,
                                                                Properties.Resources.String_ClickOrRightClickToolTip);
            }
            else
            {
                displayItem.ToolTip = actionSet != null ? actionSet.Info.ShortDescription : Properties.Resources.String_DefaultSettings;                    
            }
        }
        
        /// <summary>
        /// Set the selected annotation and raise an input event for it
        /// </summary>
        /// <param name="annotation"></param>
        private void SelectAnnotation(ControlAnnotationControl annotation)
        {
            if (annotation != _selectedAnnotation)
            {
                // Selection changed

                // Clear current selection
                if (_selectedAnnotation != null)
                {
                    _selectedAnnotation.IsSelected = false;
                }

                // Set new selection
                _selectedAnnotation = annotation;
                if (_selectedAnnotation != null)
                {
                    _selectedAnnotation.IsSelected = true;

                    // Raise event
                    SettingDisplayItem displayItem = (SettingDisplayItem)annotation.DataContext;
                    _currentInputEvent = displayItem.InputControl;

                    RaiseEvent(new KxInputControlRoutedEventArgs(KxInputControlChangedEvent, _currentInputEvent));
                }
            }
        }
    }

    /// <summary>
    /// Holds data to bind to an annotation
    /// </summary>
    public class SettingDisplayItem : NamedItem
    {
        private KxControlEventArgs _inputControl;
        private string _text = "";
        private string _toolTip = "";
        private bool _isDesignMode = false;

        // Properties
        public KxControlEventArgs InputControl
        {
            get { return _inputControl; }
            set
            {
                if (_inputControl != value)
                {
                    _inputControl = value;
                    NotifyPropertyChanged("InputControl");
                }
            }
        }
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    NotifyPropertyChanged("Text");
                }
            }
        }
        public string ToolTip
        {
            get { return _toolTip; }
            set
            {
                if (_toolTip != value)
                {
                    _toolTip = value;
                    NotifyPropertyChanged("ToolTip");
                }
            }
        }
        public bool IsDesignMode
        {
            get { return _isDesignMode; }
            set
            {
                if (_isDesignMode != value)
                {
                    _isDesignMode = value;
                    NotifyPropertyChanged("IsDesignMode");
                }
            }
        }        


        public SettingDisplayItem()
            : base()
        {
        }

        public SettingDisplayItem(int id, string name, KxControlEventArgs inputControl, string text, string toolTip, bool isDesignMode)
        :base(id, name)
        {
            _inputControl = inputControl;
            _text = text;
            _toolTip = toolTip;
            _isDesignMode = isDesignMode;
        }
    }
}
