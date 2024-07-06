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
using System.IO;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Sources;
using Keysticks.UI;

namespace Keysticks
{
    /// <summary>
    /// Profile Designer window
    /// </summary>
    public partial class ProfileDesignerWindow : Window, IProfileEditorWindow
    {
        // Fields
        private ITrayManager _parent;
        private EditActionWindow _actionEditorWindow;
        private AppConfig _appConfig;
        private Profile _profile;
        private BaseSource _source;
        private ProfileSummariser _profileSummariser;
        private StateVector _selectedState;
        private KxControlEventArgs _selectedControl;
        private StringUtils _utils = new StringUtils();

        /// <summary>
        /// Constructor
        /// </summary>
        public ProfileDesignerWindow(ITrayManager parent)
        {
            _parent = parent;
            _appConfig = new AppConfig();

            InitializeComponent();

            ProfileViewer.SetProfileEditor(this);
        }

        /// <summary>
        /// Return the parent window
        /// </summary>
        /// <returns></returns>
        public ITrayManager GetTrayManager()
        {
            return _parent;
        }

        /// <summary>
        /// Set the application configuration
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            _appConfig = appConfig;

            if (_profile != null)
            {
                _profile.SetAppConfig(appConfig);
            }
            ProfileViewer.SetAppConfig(appConfig);
        }

        /// <summary>
        /// Set the profile to edit
        /// </summary>
        /// <param name="profile">Not null</param>
        public void SetProfile(Profile profile)
        {
            // Remove event handlers for existing profile if required
            if (_profile != null)
            {
                _profile.PropertyChanged -= Profile_PropertyChanged;
            }

            // Store the profile to edit
            _profile = profile;
            _profileSummariser = new ProfileSummariser(profile);

            // Register for edit notifications
            profile.PropertyChanged += Profile_PropertyChanged;

            // Bind input viewers
            ProfileViewer.SetProfile(profile);

            // Set the title
            this.Title = string.Format("{0} - {1}", profile.Name, Constants.ProductName);
            
            // Update the data displayed if the form is loaded
            if (this.IsLoaded)
            {
                // Enable / disable Apply button
                this.ApplyButton.IsEnabled = profile.IsModified;
            }
        }

        /// <summary>
        /// Handle property change for profile e.g. when it's been marked as edited
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Profile_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsModified")
            {
                if (this.IsLoaded && _profile != null)
                {
                    this.ApplyButton.IsEnabled = _profile.IsModified;
                }
            }
        }

        /// <summary>
        /// Handle window loaded event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Position top left
                this.Top = 0;
                this.Left = 0;

                if (_profile != null)
                {
                    // Enable / disable Apply button
                    this.ApplyButton.IsEnabled = _profile.IsModified;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_LoadWindow, ex);
            }
        }

        /// <summary>
        /// Tell the parent form that the window is closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_actionEditorWindow != null)
            {
                _actionEditorWindow.Close();
            }
            _parent.ChildWindowClosing(this);
        }

        /// <summary>
        /// Handle child window closing
        /// </summary>
        /// <param name="window"></param>
        public void ChildWindowClosing(Window window)
        {
            ClearError();
            if (window == _actionEditorWindow)
            {
                _actionEditorWindow = null;
            }            
        }

        /// <summary>
        /// Get the templates profile to import quick-edit actions from
        /// </summary>
        /// <returns></returns>
        public Profile LoadTemplates(string filePath)
        {
            // Load templates if required
            Profile templatesProfile = null;
            if (File.Exists(filePath))
            {
                try
                {
                    templatesProfile = new Profile(true);
                    templatesProfile.FromFile(filePath);
                    templatesProfile.Initialise(_parent.InputContext);
                    templatesProfile.SetAppConfig(_appConfig);

                    // Update mode names according to current input language
                    templatesProfile.SetKeyboardSpecificControlSetNames(_parent.InputContext);
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_LoadTemplates, ex);
                    templatesProfile = null;
                }
            }

            return templatesProfile;
        }

        /// <summary>
        /// Handle input language change
        /// </summary>
        public void KeyboardLayoutChanged(KxKeyboardChangeEventArgs args)
        {
            try
            {
                // Update the profile
                _profile.Initialise(args);

                // Refresh UI
                ProfileViewer.KeyboardLayoutChanged(args);
                if (_actionEditorWindow != null)
                {
                    _actionEditorWindow.KeyboardLayoutChanged(args);
                }
                ProfileViewer.RefreshDisplay();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_KeyboardLayoutChange, ex);
            }
        }

        /// <summary>
        /// OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearError();
                ApplyAnyChanges();
                Close();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_ApplyChanges, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Apply button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearError();
                ApplyAnyChanges();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_ApplyChanges, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Apply any changes to the profile being edited
        /// </summary>
        private void ApplyAnyChanges()
        {
            if (_profile.IsModified)
            {
                // Apply changes
                Profile profile = new Profile(_profile);
                _parent.ApplyProfile(profile);

                // Record that changes have been applied
                _profile.IsModified = false;
            }
        }

        /// <summary>
        /// Cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

            e.Handled = true;
        }
     
        /// <summary>
        /// Handle selection of different state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_SituationChanged(object sender, KxStateRoutedEventArgs e)
        {
            try
            {
                ClearError();
                _selectedState = e.State;

                // Refresh action editor
                RefreshActionEditor();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_DisplayControlSet, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Player radio button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_PlayerChanged(object sender, KxIntValRoutedEventArgs e)
        {
            try
            {
                ClearError();

                _source = _profile.GetSource(e.Value);
                if (_actionEditorWindow != null)
                {
                    _actionEditorWindow.SetSource(_source);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_SelectionChange, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle change of selected control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Control_InputEventChanged(object sender, KxInputControlRoutedEventArgs e)
        {
            try
            {
                ClearError();

                _selectedControl = e.Control;
                RefreshActionEditor();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_DisplayActionsForControl, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Edit the actions for the specified situation
        /// </summary>
        /// <param name="situation"></param>
        private void Control_EditActions(object sender, KxStateRoutedEventArgs e)
        {
            try
            {
                ClearError();

                if (e.State != null &&
                    _selectedControl != null)
                {                    
                    // Open the action if reqd
                    if (_actionEditorWindow == null)
                    {
                        _actionEditorWindow = new EditActionWindow(this);
                        _actionEditorWindow.SetAppConfig(_appConfig);
                        _actionEditorWindow.SetSource(_source);
                        _actionEditorWindow.SetData(e.State, _selectedControl);
                        _actionEditorWindow.Show();

                        // Position alongside this window
                        _actionEditorWindow.Top = this.Top;
                        _actionEditorWindow.Left = Math.Min(this.Left + this.Width + 10, SystemParameters.VirtualScreenWidth - _actionEditorWindow.Width);
                    }
                    else
                    {
                        // Set the data to edit
                        _actionEditorWindow.SetData(e.State, _selectedControl);

                        _actionEditorWindow.WindowState = WindowState.Normal;
                        _actionEditorWindow.Activate();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_ConfigureActionEditor, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Redisplay the current selection when its actions have changed
        /// </summary>
        public void ActionsEdited()
        {
            // Refresh the profile summary
            _profileSummariser.UpdateMetaData();

            // Refresh control set viewers
            ProfileViewer.RefreshActionViewers();
        }

        /// <summary>
        /// Refresh the action editor if it is open
        /// </summary>
        public void RefreshActionEditor()
        {
            if (_actionEditorWindow != null && _source != null)
            {
                StateVector state = _selectedState;
                if (_selectedState != null && _selectedControl != null)
                {
                    // Get actions for this control
                    ActionSet actionSet = _source.Actions.GetActionsForInputControl(_selectedState, _selectedControl, true);
                    if (actionSet != null)
                    {
                        state = actionSet.LogicalState;
                    }
                }

                _actionEditorWindow.SetData(state, _selectedControl);
            }
        }

        /// <summary>
        /// Enable or disable the action editor window
        /// </summary>
        /// <param name="enable"></param>
        public void EnableActionEditor(bool enable)
        {
            if (_actionEditorWindow != null)
            {
                _actionEditorWindow.IsEnabled = enable;
            }
        }

        /// <summary>
        /// Handle an error
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleError(object sender, KxErrorRoutedEventArgs e)
        {
            ErrorMessage.Show(e.Error.Message, e.Error.InnerException);
            e.Handled = true;
        }

        /// <summary>
        /// Clear any error message
        /// </summary>
        private void ClearError()
        {
            ErrorMessage.Clear();
        }
    }
}
