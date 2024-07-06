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
using System.Windows;
using System.Windows.Controls;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Sources;
using Keysticks.UI;

namespace Keysticks
{
    /// <summary>
    /// Add keyboard controls window
    /// </summary>
    public partial class AddKeyboardWindow : Window
    {
        // Fields
        private StringUtils _utils = new StringUtils();
        private AppConfig _appConfig;
        private BaseSource _source;
        private BaseSource _templatesSource;
        private Profile _previewProfile;
        private BaseSource _previewSource;
        private bool _templateChangedSinceRefresh = false;
        private NamedItemList _controlSetsList = new NamedItemList();
        private NamedItemList _templateList = new NamedItemList();
        private NamedItemList _navigationDisplayList = new NamedItemList();
        private NamedItemList _selectionDisplayList = new NamedItemList();
        private List<GeneralisedControl> _navigationControlsList;
        private List<GeneralisedControl> _selectionControlsList;

        private int _selectedTemplateModeID = Constants.NoneID;
        private GeneralisedControl _navigationControl;
        private GeneralisedControl _selectionControl;

        // Properties
        public int TemplateModeID { get { return _selectedTemplateModeID; } }
        public GeneralisedControl NavigationControl { get { return _navigationControl; } }
        public GeneralisedControl SelectionControl { get { return _selectionControl; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentWindow"></param>
        /// <param name="profile"></param>
        public AddKeyboardWindow(Window parentWindow, BaseSource source, AppConfig appConfig, BaseSource templatesSource)
        {
            InitializeComponent();

            _source = source;
            _appConfig = appConfig;
            _templatesSource = templatesSource;
            Owner = parentWindow;

            this.ProfileViewer.SetAppConfig(appConfig);
        }        

        /// <summary>
        /// Populate grid types combo
        /// </summary>
        private void PopulateTemplateList(bool filterByGridType, EGridType gridType)
        {
            _templateList.Clear();

            // Add modes with pages, plus the blank mode, from template profile
            foreach (AxisValue modeItem in _templatesSource.StateTree.SubValues)
            {
                if (modeItem.TemplateGroup == ETemplateGroup.ControlSets &&
                    (!filterByGridType || modeItem.GridType == gridType))
                {
                    _templateList.Add(modeItem);
                }
            }
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create preview profile
                _previewProfile = new Profile();
                string name = _utils.GetVirtualControllerName(_source.ID);

                // Add source with same controls
                _previewSource = new BaseSource(_source.ID, name);
                _previewSource.CopyControllerDesign(_source);
                AxisValue rootState = new AxisValue(Constants.DefaultID, Properties.Resources.String_All, StateVector.GetRootSituation());
                _previewSource.StateTree.SubValues.Add(rootState);
                _previewProfile.AddSource(_previewSource);

                // Initialise profile
                _previewProfile.Initialise(_source.Profile.KeyboardContext);
                _previewProfile.SetAppConfig(_appConfig);

                PopulateTemplateList(false, EGridType.None);

                this.TemplateCombo.ItemsSource = _templateList;
                this.NavigationControlCombo.ItemsSource = _navigationDisplayList;
                this.SelectionControlCombo.ItemsSource = _selectionDisplayList;

                // Select a template
                if (_templateList.Count != 0)
                {
                    this.TemplateCombo.SelectedIndex = 0;
                }

                // Focus the name box
                TemplateCombo.Focus();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_LoadWindow, ex);
            }
        }
                
        /// <summary>
        /// OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check the name is valid
                ClearError();
                bool isValid = true;
                NamedItem templateItem = (NamedItem)this.TemplateCombo.SelectedItem;
                int navigationControlIndex = this.NavigationControlCombo.SelectedIndex;
                int selectionControlIndex = this.SelectionControlCombo.SelectedIndex;

                if (templateItem == null)
                {
                    this.ErrorMessage.Show(Properties.Resources.E_SelectTemplate, null);
                    isValid = false;
                }
                else if (navigationControlIndex == -1 ||
                        navigationControlIndex >= _navigationControlsList.Count ||
                        selectionControlIndex == -1 ||
                        selectionControlIndex >= _selectionControlsList.Count)
                {
                    this.ErrorMessage.Show(Properties.Resources.E_SelectControls, null);
                    isValid = false;
                }

                if (isValid)
                {
                    _selectedTemplateModeID = templateItem.ID;
                    _navigationControl = _navigationControlsList[navigationControlIndex];
                    _selectionControl = _selectionControlsList[selectionControlIndex];
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_InvalidDetails, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Cancelled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ClearError();
            this.Close();

            e.Handled = true;
        }
        
        /// <summary>
        /// Grid type changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TemplateCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ClearError();

                AxisValue templateItem = (AxisValue)this.TemplateCombo.SelectedItem;
                if (templateItem != null)
                {
                    _templateChangedSinceRefresh = true;

                    // Decide which generalised controls can be used for navigating and selecting in this control set
                    EDirectionMode navigationDirectionality = EDirectionMode.None;
                    int defaultID = 0;
                    ControlsDefinition templateControls = templateItem.Controls;
                    if (templateControls != null)
                    {
                        navigationDirectionality = templateControls.NavigationControl.DirectionMode;
                        defaultID = templateControls.NavigationControl.ToID();
                    }
                    _navigationControlsList = _source.Utils.GetControlsWithDirectionType(navigationDirectionality, null);

                    // Populate navigation combo
                    _navigationDisplayList.Clear();
                    foreach (GeneralisedControl genControl in _navigationControlsList)
                    {
                        string name = _source.Utils.GetGeneralControlName(genControl);
                        _navigationDisplayList.Add(new NamedItem(genControl.ToID(), name));
                    }

                    // Select default control
                    if (_navigationDisplayList.GetItemByID(defaultID) != null)
                    {
                        this.NavigationControlCombo.SelectedValue = defaultID;
                    }
                    else if (_navigationControlsList.Count != 0)
                    {
                        this.NavigationControlCombo.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_InvalidTemplate, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Navigation control changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigationControlCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ClearError();

                // Force navigation and selection to be the same or different if reqd
                AxisValue templateItem = (AxisValue)this.TemplateCombo.SelectedItem;
                int navigationIndex = NavigationControlCombo.SelectedIndex;
                if (navigationIndex != -1 && templateItem != null)
                {
                    EDirectionMode selectionDirectionality = EDirectionMode.None;
                    int defaultID = 0;
                    EControlRestrictions controlRestrictions = EControlRestrictions.None;
                    ControlsDefinition templateControls = templateItem.Controls;
                    if (templateControls != null)
                    {
                        selectionDirectionality = templateControls.SelectionControl.DirectionMode;
                        defaultID = templateControls.SelectionControl.ToID();                         
                        controlRestrictions = templateControls.Restrictions;
                    }

                    GeneralisedControl navigationControl = _navigationControlsList[navigationIndex];
                    if (controlRestrictions == EControlRestrictions.BothSame)
                    {
                        _selectionControlsList = new List<GeneralisedControl>();
                        _selectionControlsList.Add(new GeneralisedControl(navigationControl));
                        defaultID = navigationControl.ToID();
                    }
                    else if (controlRestrictions == EControlRestrictions.BothDifferent)
                    {
                        _selectionControlsList = _source.Utils.GetControlsWithDirectionType(selectionDirectionality, navigationControl);
                    }
                    else
                    {
                        _selectionControlsList = _source.Utils.GetControlsWithDirectionType(selectionDirectionality, null);
                    }

                    // Populate selection control combo
                    _selectionDisplayList.Clear();
                    foreach (GeneralisedControl genControl in _selectionControlsList)
                    {
                        string name = _source.Utils.GetGeneralControlName(genControl); 
                        _selectionDisplayList.Add(new NamedItem(genControl.ToID(), name));
                    }

                    // Select default value, or first item if not valid
                    if (_selectionDisplayList.GetItemByID(defaultID) != null)
                    {
                        SelectionControlCombo.SelectedValue = defaultID;
                    }
                    else if (_selectionDisplayList.Count != 0)
                    {
                        SelectionControlCombo.SelectedIndex = 0;
                    }                  
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_SelectionChange, ex);
            }
        }

        /// <summary>
        /// Selection control changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectionControlCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ClearError();
                                 
                // Refresh the preview
                RefreshTemplatePreview();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_SelectionChange, ex);
            }
        }

        /// <summary>
        /// Display a preview of a template control set
        /// </summary>
        /// <param name="modeItem"></param>
        private void RefreshTemplatePreview()
        {
            // Get the selections
            AxisValue templateMode = (AxisValue)this.TemplateCombo.SelectedItem;
            int navigationControlIndex = this.NavigationControlCombo.SelectedIndex;
            int selectionControlIndex = this.SelectionControlCombo.SelectedIndex;
            if (templateMode != null && navigationControlIndex != -1 && selectionControlIndex != -1)
            {
                ProfileViewer.Visibility = Visibility.Visible;
                InvalidSelectionTextBlock.Visibility = Visibility.Collapsed;

                GeneralisedControl navigationControl = _navigationControlsList[navigationControlIndex];
                GeneralisedControl selectionControl = _selectionControlsList[selectionControlIndex];

                // Clear the preview profile
                _previewSource.Actions.Clear();
                _previewSource.StateEditor.DeleteAllModes();

                // Create new controls and grid
                ControlsDefinition templateControls = templateMode.Controls != null ? templateMode.Controls : new ControlsDefinition();
                ControlsDefinition controls = new ControlsDefinition(navigationControl,
                                                                        selectionControl,
                                                                        templateControls.Restrictions);
                GridConfig grid = null;
                GridConfig templateGrid = templateMode.Grid;
                if (templateGrid != null)
                {
                    grid = _previewSource.StateEditor.CreateGridConfig(templateGrid.GridType, templateGrid.NumCols, controls);
                }

                // Copy mode and apply new controls and grid
                AxisValue newModeItem = _previewSource.StateEditor.CopyMode(templateMode, templateMode.Name, Constants.DefaultID);
                newModeItem.Controls = controls;
                newModeItem.Grid = grid;

                // Import actions
                _previewSource.ActionEditor.ImportActions(_templatesSource,
                                                            templateMode.Situation,
                                                            templateMode.Controls,
                                                            newModeItem.Situation,
                                                            newModeItem.Controls);                

                // Decide which situation to select
                StateVector situation;
                if (_templateChangedSinceRefresh || ProfileViewer.SelectedState == null)
                {
                    // Select a sensible default situation
                    if (newModeItem.SubValues.Count > 1)
                    {
                        AxisValue pageItem = (AxisValue)newModeItem.SubValues[1];
                        situation = pageItem.Situation;
                    }
                    else
                    {
                        situation = newModeItem.Situation;
                    }
                }
                else
                {
                    // Select the same situation
                    situation = ProfileViewer.SelectedState;
                }

                // Bind the preview controls
                this.ProfileViewer.SetProfile(_previewProfile);

                // Select situation
                this.ProfileViewer.SetCurrentSituation(situation);

                _templateChangedSinceRefresh = false;
            }
            else
            {
                ProfileViewer.Visibility = Visibility.Collapsed;
                InvalidSelectionTextBlock.Visibility = Visibility.Visible;
            }
        }
        
        /// <summary>
        /// Handle an error from a child control
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        private void HandleError(object sender, KxErrorRoutedEventArgs e)
        {
            ErrorMessage.Show(e.Error.Message, e.Error.InnerException);
        }

        private void ClearError()
        {
            ErrorMessage.Clear();
        }
    }
}

