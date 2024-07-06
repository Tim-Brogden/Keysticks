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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Sources;
using Keysticks.UI;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for showing that a control set does not have an associated keyboard grid
    /// </summary>
    public partial class NoKeyboardControl : KxUserControl, IActionViewerControl
    {
        // Fields
        private bool _isLoaded;
        private IProfileEditorWindow _editorWindow;
        private AppConfig _appConfig;
        private BaseSource _source;
        private StateVector _currentSituation;

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
            typeof(NoKeyboardControl),
            new FrameworkPropertyMetadata(false)
        );

        // Properties
        public StateVector CurrentSituation { get { return _currentSituation; } }
        public KxControlEventArgs CurrentInputEvent { get { return null; } }        // Not used
        
        /// <summary>
        /// Constructor
        /// </summary>
        public NoKeyboardControl()
            :base()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Set app config
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        /// <summary>
        /// Set the current source
        /// </summary>
        /// <param name="profile"></param>
        public void SetSource(BaseSource source)
        {
            _source = source;
        }

        /// <summary>
        /// Set the profile editor window to report to
        /// </summary>
        /// <param name="profileEditor"></param>
        public void SetProfileEditor(IProfileEditorWindow editorWindow)
        {
            _editorWindow = editorWindow;
        }

        /// <summary>
        /// Focus the control
        /// </summary>
        public void SetFocus()
        {
            OuterPanel.Focus();
        }

        /// <summary>
        /// Set the current situation
        /// </summary>
        /// <param name="situation"></param>
        public void SetCurrentSituation(StateVector situation)
        {
            _currentSituation = situation;

            RefreshDisplay();
        }

        public void AnimateInputEvent(KxSourceEventArgs args)
        {
        }

        public void ClearAnimations()
        {
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;
            RefreshDisplay();
        }

        /// <summary>
        /// Refresh display
        /// </summary>
        public void RefreshDisplay()
        {
            if (_isLoaded)
            {
                bool showAdd = IsDesignMode && _currentSituation != null && _currentSituation.ModeID != Constants.DefaultID;
                if (showAdd)
                {
                    // Check that the control set doesn't already have navigation and selection controls (e.g. windowless keyboard)
                    AxisValue modeItem = _source.StateTree.GetMode(_currentSituation.ModeID);
                    if (modeItem != null &&
                        modeItem.Controls != null &&
                        (modeItem.Controls.NavigationControl != null || modeItem.Controls.SelectionControl != null))
                    {
                        showAdd = false;
                    }
                }
                AddKeyboardButton.Visibility = showAdd ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Add a keyboard to the current control set
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddKeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AxisValue modeItem = _source.StateTree.GetMode(_currentSituation.ModeID);

                // Load the templates
                string filePath = System.IO.Path.Combine(AppConfig.CommonAppDataDir, "Templates", Constants.ControlSetTemplatesName + Constants.TemplateFileExtension);
                Profile controlSetTemplatesProfile = _editorWindow.LoadTemplates(filePath);
                if (controlSetTemplatesProfile != null)
                {
                    // Assume templates are defined for player 1 only
                    BaseSource templatesSource = controlSetTemplatesProfile.GetSource(Constants.ID1);
                    if (templatesSource != null)
                    {
                        AddKeyboardWindow dialog = new AddKeyboardWindow((Window)_editorWindow, _source, _appConfig, templatesSource);
                        if (dialog.ShowDialog() == true)
                        {
                            // Get the template mode to copy
                            AxisValue templateModeItem = templatesSource.StateTree.GetMode(dialog.TemplateModeID);
                            if (templateModeItem != null)
                            {
                                // Create new controls and grid
                                EControlRestrictions restrictions = templateModeItem.Controls != null ? templateModeItem.Controls.Restrictions : EControlRestrictions.None;
                                ControlsDefinition controls = new ControlsDefinition(dialog.NavigationControl,
                                                                                        dialog.SelectionControl,
                                                                                        restrictions);
                                GridConfig grid = null;
                                GridConfig templateGrid = templateModeItem.Grid;
                                if (templateGrid != null)
                                {
                                    grid = _source.StateEditor.CreateGridConfig(templateGrid.GridType, templateGrid.NumCols, controls);
                                }

                                // Apply new controls and grid
                                modeItem.Controls = controls;
                                modeItem.Grid = grid;

                                // Copy any pages from template
                                if (templateModeItem.SubValues.Count != 0)
                                {
                                    // Remove any existing pages and their actions
                                    modeItem.SubValues.Clear();
                                    _source.Actions.Validate();

                                    int insertAfterPageID = Constants.DefaultID;
                                    AxisValue copiedPage;
                                    foreach (AxisValue pageItem in templateModeItem.SubValues)
                                    {
                                        copiedPage = _source.StateEditor.CopyPage(pageItem, pageItem.Name, modeItem.ID, insertAfterPageID);
                                        insertAfterPageID = copiedPage.ID;
                                    }
                                }

                                // Import actions
                                _source.ActionEditor.ImportActions(templatesSource,
                                                            templateModeItem.Situation,
                                                            templateModeItem.Controls,
                                                            modeItem.Situation,
                                                            modeItem.Controls);                                

                                // Update UI
                                RaiseEvent(new KxStateRoutedEventArgs(KxStatesEditedEvent, _currentSituation));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_AddKeyboard, ex);
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
