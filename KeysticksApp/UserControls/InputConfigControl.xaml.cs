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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Keysticks.Config;
using Keysticks.Controls;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Input;
using Keysticks.Sources;
using Keysticks.UI;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Displays the configuration of a virtual control (name, input mappings, etc.)
    /// </summary>
    public partial class InputConfigControl : KxUserControl, ISourceViewerControl
    {
        // Fields
        private bool _isLoaded;
        private BaseSource _source;
        private KxControlEventArgs _selectedControl;
        private ControlAnnotationControl _selectedAnnotation;
        private ColourScheme _colourScheme;
        private SolidColorBrush _selectionBrush;
        private NamedItemList _inputControlsList = new NamedItemList();

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
            typeof(InputConfigControl),
            new FrameworkPropertyMetadata(false/*, new PropertyChangedCallback(OnDependencyPropertyChanged)*/)
        );

        // Routed events
        public static readonly RoutedEvent KxMappingEditedEvent = EventManager.RegisterRoutedEvent(
            "MappingEdited", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InputConfigControl));
        public static readonly RoutedEvent KxControlRenamedEvent = EventManager.RegisterRoutedEvent(
            "ControlRenamed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InputConfigControl));
        public event RoutedEventHandler MappingEdited
        {
            add { AddHandler(KxMappingEditedEvent, value); }
            remove { RemoveHandler(KxMappingEditedEvent, value); }
        }
        public event RoutedEventHandler ControlRenamed
        {
            add { AddHandler(KxControlRenamedEvent, value); }
            remove { RemoveHandler(KxControlRenamedEvent, value); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public InputConfigControl()
            :base()
        {
            InitializeComponent();

            // Brushes
            _selectionBrush = new SolidColorBrush(Constants.DefaultSelectionColour);
            NoneAnnotation.SelectionBrush = _selectionBrush;
            LeftAnnotation.SelectionBrush = _selectionBrush;
            RightAnnotation.SelectionBrush = _selectionBrush;
            UpAnnotation.SelectionBrush = _selectionBrush;
            DownAnnotation.SelectionBrush = _selectionBrush;
        }

        /// <summary>
        /// Set the application configuration
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {            
            // Set colours
            _colourScheme = appConfig.ColourScheme;
            UpdateBrushes();
        }

        /// <summary>
        /// Update the brush colours
        /// </summary>
        private void UpdateBrushes()
        {
            if (_colourScheme != null && _source != null)
            {
                PlayerColourScheme playerColours = _colourScheme.GetPlayerColours(_source.ID);
                _selectionBrush.Color = ColourUtils.ColorFromString(playerColours.SelectionColour, Constants.DefaultSelectionColour);
            }
        }

        /// <summary>
        /// Set the source
        /// </summary>
        /// <param name="profile"></param>
        public void SetSource(BaseSource source)
        {
            _source = source;
            _selectedControl = null;

            UpdateBrushes();            
        }

        /// <summary>
        /// Set the virtual control to display
        /// </summary>
        /// <param name="control"></param>
        public void SetCurrentInputEvent(KxControlEventArgs control)
        {
             _selectedControl = control;

            RefreshDisplay();
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KxUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;

                // Add event handlers
                if (IsDesignMode)
                {
                    NoneAnnotation.AnnotationClicked += DirectionAnnotation_Clicked;
                    LeftAnnotation.AnnotationClicked += DirectionAnnotation_Clicked;
                    RightAnnotation.AnnotationClicked += DirectionAnnotation_Clicked;
                    UpAnnotation.AnnotationClicked += DirectionAnnotation_Clicked;
                    DownAnnotation.AnnotationClicked += DirectionAnnotation_Clicked;
                }

                // Make controls read only if necessary
                ControlNameTextBox.IsEnabled = IsDesignMode;
                InputComboBox.IsEnabled = IsDesignMode;
                OptionCheckBox.IsEnabled = IsDesignMode;
                ButtonDirectionalityGrid.IsEnabled = IsDesignMode;

                // Bind controls
                InputComboBox.ItemsSource = _inputControlsList;

                RefreshDisplay();
            }
        }
                
        /// <summary>
        /// Refresh
        /// </summary>
        public void RefreshDisplay()
        {
            if (_isLoaded)
            {
                BaseControl control = null;
                int mappingIndex = -1;
                bool showOptions = false;
                bool showButtonDirections = false;
                if (_selectedControl != null)
                {
                    control = _source.GetVirtualControl(_selectedControl);
                    ControlNameTextBox.Text = control.Name;
                    ControlNameTextBox.IsEnabled = IsDesignMode;

                    // Determine which mapping and type of input is required
                    EVirtualControlType inputType = _selectedControl.ControlType;
                    if (_selectedControl.ControlType == EVirtualControlType.Stick)
                    {
                        switch (_selectedControl.LRUDState)
                        {
                            case ELRUDState.Left:
                            case ELRUDState.Right:
                                mappingIndex = 0; break;
                            case ELRUDState.Up:
                            case ELRUDState.Down:
                                mappingIndex = 1; break;
                            case ELRUDState.Centre:
                                mappingIndex = 2;
                                inputType = EVirtualControlType.Button;     // Stick button
                                break;
                        }
                    }
                    else if (control.ControlType == EVirtualControlType.DPad)
                    {
                        if (_selectedControl.LRUDState == ELRUDState.Centre)
                        {
                            mappingIndex = 0;
                        }
                    }
                    else
                    {
                        mappingIndex = 0;
                    }

                    if (mappingIndex != -1)
                    {
                        // Refresh combo box
                        PopulateInputControlsList(inputType);

                        // Select combo box values
                        InputMapping mapping = control.GetInputMapping(mappingIndex);
                        if (mapping != null)
                        {
                            int id = mapping.ToID(false);
                            if (_inputControlsList.GetItemByID(id) != null)
                            {
                                InputComboBox.SelectedValue = id;
                            }
                        }

                        // Refresh options checkbox
                        if (inputType == EVirtualControlType.Stick || inputType == EVirtualControlType.Trigger)
                        {
                            RefreshOptionsCheckbox(control, mappingIndex);
                            showOptions = true;
                        }

                        // Refresh button directionality
                        if (control.ControlType == EVirtualControlType.Button)
                        {
                            RefreshButtonDirectionality(control);
                            showButtonDirections = true;
                        }
                    }
                }
                else
                {                    
                    ControlNameTextBox.Text = "(No control selected)";
                    ControlNameTextBlock.IsEnabled = false;
                }

                InputTextBlock.Visibility = (mappingIndex != -1) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
                InputComboBox.Visibility = (mappingIndex != -1) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
                OptionCheckBox.Visibility = showOptions ? Visibility.Visible : Visibility.Collapsed;
                ButtonDirectionalityGrid.Visibility = showButtonDirections ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Set the text and value of the option checkbox
        /// </summary>
        /// <param name="options"></param>
        /// <param name="mappingIndex"></param>
        private void RefreshOptionsCheckbox(BaseControl control, int mappingIndex)
        {                        
            InputMapping mapping = control.GetInputMapping(mappingIndex);
            if (mapping != null)
            {
                bool fullOptionsEnabled = control.ControlType == EVirtualControlType.Stick || mapping.ControlType == EPhysicalControlType.Slider;
                if (fullOptionsEnabled)
                {
                    OptionCheckBox.Content = Properties.Resources.String_Inverted;
                    OptionCheckBox.IsChecked = (mapping.Options == EPhysicalControlOption.Inverted);
                }
                else
                {
                    OptionCheckBox.Content = Properties.Resources.String_NegativeSide;
                    OptionCheckBox.IsChecked = (mapping.Options == EPhysicalControlOption.NegativeSide);
                }
            }
        }

        /// <summary>
        /// Show the directionality of the selected button
        /// </summary>
        /// <param name="control"></param>
        private void RefreshButtonDirectionality(BaseControl control)
        {
            // Get the button diamond for this virtual controller
            KxControlEventArgs args = new KxControlEventArgs(EVirtualControlType.ButtonDiamond, Constants.ID1, EControlSetting.None, ELRUDState.None);
            ControllerButtonDiamond buttonDiamond = _source.GetVirtualControl(args) as ControllerButtonDiamond;
            
            // Determine the directionality of this button, if any
            ELRUDState direction = ELRUDState.None; 
            if (buttonDiamond != null)
            {
                direction = buttonDiamond.GetDirectionForControlID(control.ID);
            }

            // Select the appropriate annotation
            switch (direction)
            {
                case ELRUDState.Left:
                    SelectAnnotation(LeftAnnotation); break;
                case ELRUDState.Right:
                    SelectAnnotation(RightAnnotation); break;
                case ELRUDState.Up:
                    SelectAnnotation(UpAnnotation); break;
                case ELRUDState.Down:
                    SelectAnnotation(DownAnnotation); break;
                default:
                    SelectAnnotation(NoneAnnotation); break;
            }
        }

        /// <summary>
        /// Set the selected annotation and raise an input event for it
        /// </summary>
        /// <param name="annotation"></param>
        protected void SelectAnnotation(ControlAnnotationControl annotation)
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
                }
            }
        }

        /// <summary>
        /// Populate input controls list
        /// </summary>
        /// <param name="profile"></param>
        private void PopulateInputControlsList(EVirtualControlType controlType)
        {
            _inputControlsList.Clear();

            // Add a dummy entry
            EPhysicalControlType dummyControlType;
            switch (controlType)
            {
                case EVirtualControlType.Button:
                default:
                    dummyControlType = EPhysicalControlType.Button; break;
                case EVirtualControlType.DPad:
                    dummyControlType = EPhysicalControlType.POV; break;
                case EVirtualControlType.Stick:
                    dummyControlType = EPhysicalControlType.Axis; break;
                case EVirtualControlType.Trigger:
                    dummyControlType = EPhysicalControlType.Slider; break;
            }
            InputMapping dummy = new InputMapping(Constants.NoneID, dummyControlType, Constants.Index0, EPhysicalControlOption.None);
            _inputControlsList.Add(new NamedItem(dummy.ToID(false), Properties.Resources.String_None));

            // Add input controls
            foreach (PhysicalInput physicalInput in _source.PhysicalInputs)
            {
                foreach (PhysicalControl physicalControl in physicalInput.Controls)
                {
                    bool add = false;
                    switch (controlType)
                    {
                        case EVirtualControlType.DPad:
                            add = physicalControl.ControlType == EPhysicalControlType.POV;
                            break;
                        case EVirtualControlType.Stick:
                            add = physicalControl.ControlType == EPhysicalControlType.Axis;
                            break;
                        case EVirtualControlType.Trigger:
                            add = (physicalControl.ControlType == EPhysicalControlType.Axis) ||
                                    (physicalControl.ControlType == EPhysicalControlType.Slider);
                            break;
                        case EVirtualControlType.Button:
                            add = physicalControl.ControlType == EPhysicalControlType.Button;
                            break;
                    }

                    if (add)
                    {
                        InputMapping mapping = new InputMapping(physicalInput.ID, physicalControl.ControlType, physicalControl.ControlIndex, EPhysicalControlOption.None);
                        string name = string.Format("{0} - {1}", physicalInput.Name, physicalControl.Name);
                        _inputControlsList.Add(new NamedItem(mapping.ToID(false), name));
                    }
                }
            }
        }

        /// <summary>
        /// Control name changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlNameTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                BaseControl control = null;
                string name = ControlNameTextBox.Text;
                if (name != "" && _selectedControl != null)
                {
                    control = _source.GetVirtualControl(_selectedControl);
                    if (control != null && control.Name != name)
                    {
                        control.Name = name;
                        _source.IsModified = true;
                        RaiseEvent(new RoutedEventArgs(KxControlRenamedEvent));
                    }
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_UpdateControlName, ex);
            }
        }

        /// <summary>
        /// Input control mapping changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox comboBox = (ComboBox)sender;
                NamedItem selectedItem = (NamedItem)comboBox.SelectedItem;
                if (selectedItem != null)
                {
                    BaseControl control = _source.GetVirtualControl(_selectedControl);
                    if (control != null)
                    {
                        int mappingIndex = 0;
                        if (_selectedControl.ControlType == EVirtualControlType.Stick)
                        {
                            switch (_selectedControl.LRUDState)
                            {
                                case ELRUDState.Up:
                                case ELRUDState.Down:
                                    mappingIndex = 1;
                                    break;
                                case ELRUDState.Centre:
                                    mappingIndex = 2;
                                    break;
                            }
                        }

                        InputMapping mapping = control.GetInputMapping(mappingIndex);
                        if (mapping != null)
                        {
                            int oldID = mapping.ToID(false);
                            if (oldID != selectedItem.ID)
                            {
                                InputMapping newMapping = InputMapping.FromID(selectedItem.ID);
                                if (newMapping != null)
                                {
                                    mapping.InputID = newMapping.InputID;
                                    mapping.ControlType = newMapping.ControlType;
                                    mapping.ControlIndex = newMapping.ControlIndex;

                                    // Ensure that the options value is valid
                                    if (control.ControlType == EVirtualControlType.Trigger)
                                    {
                                        if (mapping.ControlType == EPhysicalControlType.Slider)
                                        {
                                            // Trigger mapped to slider - allow Normal / Inverted
                                            if (mapping.Options == EPhysicalControlOption.PositiveSide)
                                            {
                                                mapping.Options = EPhysicalControlOption.None;
                                            }
                                            else if (mapping.Options == EPhysicalControlOption.NegativeSide)
                                            {
                                                mapping.Options = EPhysicalControlOption.Inverted;
                                            }
                                        }
                                        else if (mapping.ControlType == EPhysicalControlType.Axis)
                                        {
                                            // Trigger mapped to axis - allow +/- side
                                            if (mapping.Options == EPhysicalControlOption.None)
                                            {
                                                mapping.Options = EPhysicalControlOption.PositiveSide;
                                            }
                                            else if (mapping.Options == EPhysicalControlOption.Inverted)
                                            {
                                                mapping.Options = EPhysicalControlOption.NegativeSide;
                                            }
                                        }
                                        RefreshOptionsCheckbox(control, mappingIndex);
                                    }
                                }

                                _source.IsModified = true;
                                RaiseEvent(new RoutedEventArgs(KxMappingEditedEvent));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_UpdateInputMapping, ex);
            }
        }

        /// <summary>
        /// Handle change of checkbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OptionCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox checkBox = (CheckBox)sender;
                BaseControl control = _source.GetVirtualControl(_selectedControl);
                if (control != null)
                {
                    int mappingIndex = 0;
                    if (_selectedControl.ControlType == EVirtualControlType.Stick)
                    {
                        switch (_selectedControl.LRUDState)
                        {
                            case ELRUDState.Up:
                            case ELRUDState.Down:
                                mappingIndex = 1;
                                break;
                            case ELRUDState.Centre:
                                mappingIndex = 2;
                                break;
                        }
                    }

                    InputMapping mapping = control.GetInputMapping(mappingIndex);
                    if (mapping != null)
                    {
                        // Interpret the check box value as an option value
                        EPhysicalControlOption options;
                        bool fullOptionsEnabled = control.ControlType == EVirtualControlType.Stick || mapping.ControlType == EPhysicalControlType.Slider;
                        if (fullOptionsEnabled)
                        {
                            options = (checkBox.IsChecked == true) ? EPhysicalControlOption.Inverted : EPhysicalControlOption.None;
                        }
                        else
                        {
                            options = (checkBox.IsChecked == true) ? EPhysicalControlOption.NegativeSide : EPhysicalControlOption.PositiveSide;
                        }

                        if (mapping.Options != options)
                        {
                            mapping.Options = options;
                            _source.IsModified = true;
                            RaiseEvent(new RoutedEventArgs(KxMappingEditedEvent));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_UpdateInversionOption, ex);
            }
        }

        /// <summary>
        /// Button direction annotation clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DirectionAnnotation_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                BaseControl control = _source.GetVirtualControl(_selectedControl);
                if (control != null && control.ControlType == EVirtualControlType.Button)
                {
                    // Determine which direction the user has selected
                    string directionString = ((ControlAnnotationControl)sender).Tag as string;
                    ELRUDState direction = (ELRUDState)Enum.Parse(typeof(ELRUDState), directionString);

                    // Get the button diamond for this virtual controller
                    KxControlEventArgs args = new KxControlEventArgs(EVirtualControlType.ButtonDiamond, Constants.ID1, EControlSetting.None, ELRUDState.None);
                    ControllerButtonDiamond buttonDiamond = _source.GetVirtualControl(args) as ControllerButtonDiamond;
                    if (buttonDiamond != null)
                    {
                        int index = -1;
                        switch (direction)
                        {
                            case ELRUDState.Left:
                                index = 0; break;
                            case ELRUDState.Right:
                                index = 1; break;
                            case ELRUDState.Up:
                                index = 2; break;
                            case ELRUDState.Down:
                                index = 3; break;
                        }

                        for (int i = 0; i < buttonDiamond.LRUDControlIDs.Length; i++)
                        {
                            if (i == index)
                            {
                                // Set new direction
                                if (buttonDiamond.LRUDControlIDs[i] != control.ID)
                                {
                                    buttonDiamond.LRUDControlIDs[i] = control.ID;
                                    _source.IsModified = true;
                                }
                            }
                            else
                            {
                                // Unset current direction
                                if (buttonDiamond.LRUDControlIDs[i] == control.ID)
                                {
                                    buttonDiamond.LRUDControlIDs[i] = Constants.NoneID;
                                    _source.IsModified = true;
                                }
                            }
                        }
                    }
                }

                // Select the annotation that was clicked
                SelectAnnotation((ControlAnnotationControl)sender);
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_UpdateButtonDirectionality, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Report an error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        private void ReportError(string message, Exception ex)
        {
            KxErrorRoutedEventArgs args = new KxErrorRoutedEventArgs(KxErrorEvent, new KxException(message, ex));
            RaiseEvent(args);
        }
    }
}
