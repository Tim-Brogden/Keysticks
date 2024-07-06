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
using System.Text;
using System.Windows;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Sources;
using Keysticks.UI;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for displaying and editing control set auto-activations
    /// </summary>
    public partial class AutoActivationControl : KxUserControl, ISourceViewerControl
    {
        // Fields
        private bool _isLoaded;
        private BaseSource _source;
        private StateVector _selectedState;

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
            typeof(AutoActivationControl),
            new FrameworkPropertyMetadata(false)
        );

        // Events
        public event RoutedEventHandler ActivationChanged
        {
            add { AddHandler(KxActivationChangedEvent, value); }
            remove { RemoveHandler(KxActivationChangedEvent, value); }
        }
        public static readonly RoutedEvent KxActivationChangedEvent = EventManager.RegisterRoutedEvent(
            "ActivationChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AutoActivationControl));

        /// <summary>
        /// Constructor
        /// </summary>
        public AutoActivationControl()
            :base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set the app config
        /// </summary>
        /// <param name="scheme"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
        }

        /// <summary>
        /// Set the source
        /// </summary>
        /// <param name="profile"></param>
        public void SetSource(BaseSource source)
        {
            _source = source;

            RefreshDisplay();
        }

        /// <summary>
        /// Set the current state
        /// </summary>
        /// <param name="state"></param>
        public void SetState(StateVector state)
        {            
            _selectedState = state;

            RefreshDisplay();
        }

        /// <summary>
        /// Control loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;

            RefreshDisplay();
        }

        /// <summary>
        /// Refresh the display
        /// </summary>
        public void RefreshDisplay()
        {
            if (_isLoaded && _source != null && _selectedState != null)
            {
                // Caption
                string stateTitle = _source.Utils.GetModeOrPageName(_selectedState);                
                string caption;
                if (!string.IsNullOrEmpty(stateTitle))
                {
                    caption = string.Format(Properties.Resources.String_ActivateX, stateTitle);
                }
                else
                {
                    caption = Properties.Resources.String_ActivateControlSet;
                }
                ActivationGroupBox.Header = caption;

                // Enable in design mode except for root state
                ActivationGrid.IsEnabled = IsDesignMode && (_selectedState.ModeID != Constants.DefaultID);

                // Show activation for current state if any
                string activationsSummary = "";
                string tooltip = null;
                StateVector defaultState = _source.AutoActivations.DefaultState;
                if (defaultState != null && _selectedState.ID == defaultState.ID)
                {
                    DefaultRadioButton.IsChecked = true;
                }
                else
                {
                    NamedItemList activations = _source.AutoActivations.GetActivations(_selectedState);
                    if (activations.Count != 0)
                    {
                        ProgramRadioButton.IsChecked = true;
                        bool first = true;
                        StringBuilder sbText = new StringBuilder();
                        StringBuilder sbTooltip = new StringBuilder();
                        foreach (AutoActivation activation in activations)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                            {
                                sbText.Append("; ");
                            }
                            sbText.Append(activation.Name);
                            sbTooltip.AppendLine(activation.Name);
                        }
                        activationsSummary = sbText.ToString();
                        tooltip = sbTooltip.ToString();
                    }
                    else
                    {
                        ManualRadioButton.IsChecked = true;
                    }
                }
                ActivationSummary.Text = activationsSummary;
                ActivationSummary.ToolTip = tooltip;
                EditActivationsButton.Visibility = ProgramRadioButton.IsChecked == true && IsDesignMode ? Visibility.Visible : Visibility.Hidden;
            }
        }

        /// <summary>
        /// Auto-activate radio button checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (_isLoaded && _source != null && _selectedState != null)
            {
                try
                {
                    bool isProgramOption = ProgramRadioButton.IsChecked == true;
                    if (!isProgramOption)
                    {
                        _source.AutoActivations.ClearActivations(_selectedState);
                        ActivationSummary.Text = "";
                        ActivationSummary.ToolTip = null;
                    }
                    EditActivationsButton.Visibility = isProgramOption && IsDesignMode ? Visibility.Visible : Visibility.Hidden;

                    if (DefaultRadioButton.IsChecked == true)
                    {
                        if (_source.AutoActivations.DefaultState == null ||
                            _source.AutoActivations.DefaultState.ID != _selectedState.ID)
                        {
                            // Set new default state
                            _source.AutoActivations.DefaultState = _selectedState;
                        }
                    }
                    else
                    {
                        if (_source.AutoActivations.DefaultState != null &&
                            _source.AutoActivations.DefaultState.ID == _selectedState.ID)
                        {
                            // Unset default state
                            _source.AutoActivations.DefaultState = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ReportError(Properties.Resources.E_ChangeAutoActivation, ex);
                }
            }
        }

        /// <summary>
        /// Edit activations (Choose) button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditActivationsButton_Click(object sender, RoutedEventArgs e)
        {
            EditActivationsWindow activationsWindow = new EditActivationsWindow(_source, _selectedState);
            if (activationsWindow.ShowDialog() == true)
            {
                // Update activations for selected state
                NamedItemList activations = activationsWindow.ActivationList;
                _source.AutoActivations.SetActivations(_selectedState, activations);

                RefreshDisplay();
                RaiseEvent(new RoutedEventArgs(KxActivationChangedEvent));
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
