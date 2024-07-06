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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Input;
using Keysticks.Sources;
using Keysticks.UI;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for displaying a list of inputs and their controls
    /// </summary>
    public partial class InputsListControl : KxUserControl, ISourceViewerControl
    {
        // Fields
        private bool _isLoaded = false;
        private ColourScheme _colourScheme;
        private BaseSource _source; 
        private NamedItemList _inputItems = new NamedItemList();
        private NamedItemList _availableInputs = new NamedItemList();
        private StringUtils _utils = new StringUtils();

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
            typeof(InputsListControl),
            new FrameworkPropertyMetadata(false)
        );

        // Properties
        public NamedItemList InputItems { get { return _inputItems; } }
        public NamedItemList AvailableInputs { get { return _availableInputs; } }

        // Routed events
        public static readonly RoutedEvent KxGetInputsListEvent = EventManager.RegisterRoutedEvent(
            "GetInputsList", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InputsListControl));
        public static readonly RoutedEvent KxInputsEditedEvent = EventManager.RegisterRoutedEvent(
            "InputsEdited", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InputsListControl));
        public static readonly RoutedEvent KxDeadZoneEditedEvent = EventManager.RegisterRoutedEvent(
            "DeadZoneEdited", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InputsListControl));
        public event RoutedEventHandler GetInputsList
        {
            add { AddHandler(KxGetInputsListEvent, value); }
            remove { RemoveHandler(KxGetInputsListEvent, value); }
        }
        public event RoutedEventHandler InputsEdited
        {
            add { AddHandler(KxInputsEditedEvent, value); }
            remove { RemoveHandler(KxInputsEditedEvent, value); }
        }
        public event RoutedEventHandler DeadZoneEdited
        {
            add { AddHandler(KxDeadZoneEditedEvent, value); }
            remove { RemoveHandler(KxDeadZoneEditedEvent, value); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public InputsListControl()
            : base()
        {
            InitializeComponent();

            InputsView.DataContext = this;
        }

        /// <summary>
        /// Set the colour scheme
        /// </summary>
        /// <param name="scheme"></param>
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

                Color highlightColour = ColourUtils.ColorFromString(playerColours.HighlightColour, Constants.DefaultHighlightColour);
                this.Resources["HighlightBrush"] = new SolidColorBrush(highlightColour);
            }
        }

        /// <summary>
        /// Set the source
        /// </summary>
        /// <param name="profile"></param>
        public void SetSource(BaseSource source)
        {
            _source = source;

            UpdateBrushes();
            RefreshDisplay();
        }

        /// <summary>
        /// Loaded event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;

                RefreshDisplay();
                if (IsDesignMode)
                {
                    // Raise event to request inputs list
                    RaiseEvent(new RoutedEventArgs(KxGetInputsListEvent));
                }
            }
        }

        /// <summary>
        /// Refresh the inputs list (asynchronously)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshInputsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Raise event to request inputs list
                RaiseEvent(new RoutedEventArgs(KxGetInputsListEvent));
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_RefreshInputsList, ex);
            }
        }      

        /// <summary>
        /// Set available physical inputs
        /// </summary>
        /// <param name="inputsList"></param>
        public void SetAvailableInputs(NamedItemList inputsList)
        {
            // Replace available inputs
            _availableInputs.Clear();

            // Add dummy entry
            PhysicalInput dummyInput = new PhysicalInput(Constants.NoneID, Properties.Resources.String_ChooseController, "", false, EDeviceType.Controller, Constants.NoneID);
            PhysicalInputItem dummyItem = new PhysicalInputItem(dummyInput, false, false, null, null, null);
            _availableInputs.Add(dummyItem);

            // Add specified inputs
            if (inputsList != null)
            {
                foreach (PhysicalInput input in inputsList)
                {
                    _availableInputs.Add(new PhysicalInputItem(input, false, false, null, null, null));
                }
            }

            // Refresh inputs tree
            RefreshDisplay();
        }
        
        /// <summary>
        /// Refresh the inputs list
        /// </summary>
        public void RefreshDisplay()
        {
            if (_isLoaded && _source != null)
            {
                // Set group box header
                InputsGroupBox.Header = string.Format(Properties.Resources.String_PlayerXInputs, _source.ID);

                // Add input items
                _inputItems.Clear();
                int i = 0;
                foreach (PhysicalInput input in _source.PhysicalInputs)
                {
                    AddItemForInput(input, i == 0); // Expand the first input
                    i++;
                }
                ShowOrHideAddOption();
            }
        }

        /// <summary>
        /// Add a UI item for an input
        /// </summary>
        /// <param name="input"></param>
        private void AddItemForInput(PhysicalInput input, bool isExpanded)
        {
            PhysicalInputItem treeItem = new PhysicalInputItem(input,
                                                                IsDesignMode,
                                                                isExpanded,
                                                                null,
                                                                null,
                                                                _availableInputs);
            treeItem.PropertyChanged += PhysicalInputItem_PropertyChanged;
            _inputItems.Add(treeItem);

            AddControls(treeItem, input);
        }

        /// <summary>
        /// Add physical controls to tree
        /// </summary>
        /// <param name="treeItem"></param>
        /// <param name="namedItemList"></param>
        private void AddControls(PhysicalInputItem parentItem, PhysicalInput input)
        {
            foreach (PhysicalControl control in input.Controls)
            {
                // Add control item
                PhysicalControlItem controlItem = new PhysicalControlItem(control,
                                                                        IsDesignMode,
                                                                        null,
                                                                        parentItem);
                controlItem.PropertyChanged += PhysicalControlItem_PropertyChanged;
                parentItem.Children.Add(controlItem);
            }
        }

        /// <summary>
        /// Refresh the annotation animations to show the current controller state
        /// </summary>
        public void RefreshAnimations()
        {
            // Loop over controllers
            foreach (PhysicalInputItem inputItem in _inputItems)
            {
                foreach (PhysicalControlItem controlItem in inputItem.Children)
                {
                    PhysicalControl control = (PhysicalControl)controlItem.ItemData;
                    controlItem.IsHighlighted = control.HasNonDefaultValue();
                }
            }
        }

        /// <summary>
        /// Handle change to input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PhysicalInputItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                PhysicalInputItem treeItem = (PhysicalInputItem)sender;
                if (e.PropertyName == "SelectedItem")
                {
                    PhysicalInputItem selectedInput = (PhysicalInputItem)treeItem.SelectedItem;
                    if (selectedInput != null && selectedInput.ID != Constants.NoneID)
                    {
                        // Update the profile
                        PhysicalInput currentInput = (PhysicalInput)treeItem.ItemData;
                        if (currentInput != null)
                        {
                            PhysicalInput newInput = new PhysicalInput((PhysicalInput)selectedInput.ItemData);
                            newInput.ID = currentInput.ID;

                            int index = _source.PhysicalInputs.IndexOf(currentInput);
                            if (index != -1)
                            {
                                _source.PhysicalInputs.RemoveAt(index);
                                _source.PhysicalInputs.Insert(index, newInput);
                                _source.IsModified = true;

                                // Update the UI
                                treeItem.ItemData = newInput;
                                //treeItem.Name = newInput.Name;
                                treeItem.Children.Clear();
                                AddControls(treeItem, newInput);

                                // Raise event
                                RaiseEvent(new RoutedEventArgs(KxInputsEditedEvent));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_SelectPhysicalInput, ex);
            }
        }

        /// <summary>
        /// Delete physical input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteInputButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button deleteButton = (Button)sender;
                int inputID = (int)deleteButton.Tag;
                PhysicalInputItem inputItem = _inputItems.GetItemByID(inputID) as PhysicalInputItem;
                if (inputItem != null)
                {
                    // Delete input from profile
                    PhysicalInput input = (PhysicalInput)inputItem.ItemData;
                    _source.PhysicalInputs.Remove(input);
                    _source.IsModified = true;

                    // Update UI
                    _inputItems.Remove(inputItem);
                    ShowOrHideAddOption();

                    // Raise event
                    RaiseEvent(new RoutedEventArgs(KxInputsEditedEvent));
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_DeletePhysicalInput, ex);
            }
        }

        /// <summary>
        /// Add an input for this player
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddInputButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Add input to profile
                ProfileBuilder builder = new ProfileBuilder();
                int id = _source.PhysicalInputs.GetFirstUnusedID(Constants.ID1);
                PhysicalInput input = builder.CreateDefaultPhysicalController(id, _source.ID);
                _source.PhysicalInputs.Add(input);
                _source.IsModified = true;

                // Update UI
                AddItemForInput(input, true);
                ShowOrHideAddOption();
                InputsContainer.ScrollToBottom();

                // Raise event
                RaiseEvent(new RoutedEventArgs(KxInputsEditedEvent));
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_AddPhysicalInput, ex);
            }
        }

        /// <summary>
        /// Show or hide the option to add an input
        /// </summary>
        private void ShowOrHideAddOption()
        {
            bool enableAdd = IsDesignMode && (_source.PhysicalInputs.Count < Constants.MaxInputs);
            AddInputButton.Visibility = enableAdd ? Visibility.Visible : Visibility.Collapsed;
            InputsContainer.Margin = new Thickness(0.0, 0.0, 0.0, enableAdd ? 20.0 : 0.0);
        }

        /// <summary>
        /// Handle change to control e.g. dead zone
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PhysicalControlItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DeadZone")
            {
                try
                {
                    // Update profile
                    PhysicalControlItem controlItem = (PhysicalControlItem)sender;
                    PhysicalControl physicalControl = (PhysicalControl)controlItem.ItemData;
                    physicalControl.DeadZone = (float)Math.Round(controlItem.DeadZone, 3);

                    // Flag profile as changed
                    _source.IsModified = true;

                    // Raise event
                    RaiseEvent(new RoutedEventArgs(KxDeadZoneEditedEvent));
                }
                catch (Exception ex)
                {
                    ReportError(Properties.Resources.E_ChangeDeadZone, ex);
                }
            }
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
