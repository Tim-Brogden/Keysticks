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
using System.Windows.Input;
using System.Windows.Media;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Sources;
using Keysticks.UI;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for square keyboard grid layouts
    /// </summary>
    public partial class SquareGridControl : KxUserControl, IActionViewerControl
    {      
        // Fields
        private bool _isLoaded = false;
        private BaseSource _source;
        private ControlAnnotationControl _selectedAnnotation;
        private ControlAnnotationControl _dragStartControl;
        private Panel _selectedPanel;
        private ColourScheme _colourScheme;
        private Brush _defaultCellBrush;
        private SolidColorBrush _selectedCellBrush;
        private SolidColorBrush _highlightBrush;
        private SolidColorBrush _selectionBrush;
        private StateVector _currentSituation;
        private KxControlEventArgs _currentInputEvent;
        private ControlAnnotationControl[] _annotationControls;
        private Dictionary<int, GridBinding[]> _annotationBindings;
        private Dictionary<int, KxControlEventArgs> _annotationAnimations;
        private Panel[] _panelArray;

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
            typeof(SquareGridControl),
            new FrameworkPropertyMetadata(false)
        );
        public bool ShowUnusedControls
        {
            get { return (bool)GetValue(ShowUnusedControlsProperty); }
            set { SetValue(ShowUnusedControlsProperty, value); }
        }
        private static readonly DependencyProperty ShowUnusedControlsProperty =
            DependencyProperty.Register(
            "ShowUnusedControls",
            typeof(bool),
            typeof(SquareGridControl),
            new FrameworkPropertyMetadata(false)
        );

        // Properties
        public StateVector CurrentSituation { get { return _currentSituation; } }
        public KxControlEventArgs CurrentInputEvent { get { return _currentInputEvent; } }
       
        /// <summary>
        /// Constructor
        /// </summary>
        public SquareGridControl()
            : base()
        {
            InitializeComponent();

            // Colours
            _highlightBrush = new SolidColorBrush(Constants.DefaultHighlightColour);
            _selectionBrush = new SolidColorBrush(Constants.DefaultSelectionColour);
            _selectedCellBrush = new SolidColorBrush(Constants.DefaultSelectionColour);
            _selectedCellBrush.Opacity = 0.01 * Constants.DefaultWindowOpacityPercent;

            // Note: this order matters
            _annotationControls = new ControlAnnotationControl[]
            { 
                // Row 0
                A0000, A0001, A0002,
                A0010, A0011, A0012,
                A0020, A0021, A0022,

                A0100, A0101, A0102,
                A0110, A0111, A0112,
                A0120, A0121, A0122,
                
                A0200, A0201, A0202,
                A0210, A0211, A0212,
                A0220, A0221, A0222,

                // Row 1
                A1000, A1001, A1002,
                A1010, A1011, A1012,
                A1020, A1021, A1022,

                A1100, A1101, A1102,
                A1110, A1111, A1112,
                A1120, A1121, A1122,
                
                A1200, A1201, A1202,
                A1210, A1211, A1212,
                A1220, A1221, A1222,

                // Row 2
                A2000, A2001, A2002,
                A2010, A2011, A2012,
                A2020, A2021, A2022,

                A2100, A2101, A2102,
                A2110, A2111, A2112,
                A2120, A2121, A2122,
                
                A2200, A2201, A2202,
                A2210, A2211, A2212,
                A2220, A2221, A2222
            };

            _panelArray = new Panel[] { TopLeftGrid, TopCentreGrid, TopRightGrid,
                                            CentreLeftGrid, CentreGrid, CentreRightGrid,
                                            BottomLeftGrid, BottomCentreGrid, BottomRightGrid};

            for (int i = 0; i < _annotationControls.Length; i++)
            {
                ControlAnnotationControl annotation = _annotationControls[i];

                // Tag annotations with their index in the list, and set colours
                annotation.Tag = i;

                // Set brushes
                annotation.HighlightBrush = _highlightBrush;
                annotation.SelectionBrush = _selectionBrush;
            }

            // Create bindings table
            _annotationBindings = new Dictionary<int, GridBinding[]>();

            // Create highlights array
            _annotationAnimations = new Dictionary<int, KxControlEventArgs>();
        }

        /// <summary>
        /// Acquire the focus
        /// </summary>
        public void SetFocus()
        {
            OuterPanel.Focus();
        }
        
        /// <summary>
        /// Apply program options
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            // Make sure the highlights are cleared in case a control is currently pressed / directed
            bool animateControls = appConfig.GetBoolVal(Constants.ConfigAnimateControls, Constants.DefaultAnimateControls);
            if (_isLoaded && !animateControls)
            {
                ClearAnimations();
            }

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

                this.Resources["CellColor"] = ColourUtils.ColorFromString(playerColours.CellColour, _colourScheme.InteractiveControlsOpacity, Constants.DefaultCellColour);
                this.Resources["AlternateCellColor"] = ColourUtils.ColorFromString(playerColours.AlternateCellColour, _colourScheme.InteractiveControlsOpacity, Constants.DefaultAlternateCellColour);
                if (!_selectedCellBrush.IsFrozen)
                {
                    _selectedCellBrush.Color = ColourUtils.ColorFromString(playerColours.SelectionColour, _colourScheme.InteractiveControlsOpacity, Constants.DefaultSelectionColour);
                }

                if (!_highlightBrush.IsFrozen)
                {
                    _highlightBrush.Color = ColourUtils.ColorFromString(playerColours.HighlightColour, Constants.DefaultHighlightColour);
                }

                if (!_selectionBrush.IsFrozen)
                {
                    _selectionBrush.Color = ColourUtils.ColorFromString(playerColours.SelectionColour, Constants.DefaultSelectionColour);
                }
            }
        }

        /// <summary>
        /// Set the source
        /// </summary>
        /// <param name="profile"></param>
        public void SetSource(BaseSource source)
        {
            // Reset current situation and input event            
            _annotationBindings.Clear();
            if (_isLoaded)
            {
                ClearAnimations();
            }

            _source = source;

            UpdateBrushes();            
        }

        /// <summary>
        /// Handle change of situation
        /// </summary>
        /// <param name="args"></param>
        public void SetCurrentSituation(StateVector situation)
        {
            // Only update the annotations if the mode or page has changed
            bool updateAnnotationsRequired = (_currentSituation == null) ||
                                             (_currentSituation.ModeID != situation.ModeID) ||
                                             (_currentSituation.PageID != situation.PageID);
            if (IsDesignMode &&
                    situation.CellID == Constants.DefaultID &&
                    _currentSituation != null &&
                    _currentSituation.CellID != situation.CellID)
            {
                situation = new StateVector(situation);
                situation.CellID = _currentSituation.CellID;

                RaiseEvent(new KxStateRoutedEventArgs(KxUserControl.KxStateChangedEvent, situation));
            }

            _currentSituation = situation;                        
            if (_isLoaded && _source != null)
            {
                if (updateAnnotationsRequired)
                {
                    UpdateAnnotations();
                }

                // Refresh the highlights when the situation changes
                RefreshAnimations();

                if (IsDesignMode)
                {
                    ControlAnnotationControl annotationToSelect = FindAnnotation(_currentInputEvent);
                    SelectAnnotation(annotationToSelect);
                }
                else
                {
                    HighlightCurrentSituation();
                }
            }
        }

        /// <summary>
        /// Highlight or unhighlight the annotation for an event
        /// </summary>
        /// <param name="inputEvent"></param>
        public void AnimateInputEvent(KxSourceEventArgs inputEvent)
        {
            if (_isLoaded && _source != null && _currentSituation != null)
            {
                ControlAnnotationControl annotation = FindAnnotation(inputEvent);
                if (annotation != null)
                {
                    switch (inputEvent.EventReason)
                    {
                        case EEventReason.Directed:
                        case EEventReason.Pressed:
                            AnimateAnnotation(annotation, inputEvent, true);
                            break;
                        case EEventReason.Released:
                        case EEventReason.Undirected:
                            AnimateAnnotation(annotation, null, false);
                            break;
                        case EEventReason.Moved:                            
                            if (inputEvent.Param0 != null && inputEvent.Param0.DataType == EDataType.Bool)
                            {
                                // Param 0 indicates whether moving or not
                                AnimateAnnotation(annotation, inputEvent, (bool)inputEvent.Param0.Value);
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Move any highlights when the situation changes
        /// </summary>
        private void RefreshAnimations()
        {
            if (_annotationAnimations.Count != 0)
            {
                int[] annotationIndices = new int[_annotationAnimations.Count];
                _annotationAnimations.Keys.CopyTo(annotationIndices, 0);
                foreach (int index in annotationIndices)
                {
                    ControlAnnotationControl annotation = _annotationControls[index];
                    KxControlEventArgs inputEvent = _annotationAnimations[index];
                    ControlAnnotationControl newAnnotation = FindAnnotation(inputEvent);
                    if (newAnnotation != annotation)
                    {
                        AnimateAnnotation(annotation, null, false);
                        if (newAnnotation != null)
                        {
                            AnimateAnnotation(newAnnotation, inputEvent, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Unhighlight all controls
        /// </summary>
        public void ClearAnimations()
        {
            int[] annotationIndices = new int[_annotationAnimations.Count];
            _annotationAnimations.Keys.CopyTo(annotationIndices, 0);
            foreach (int index in annotationIndices)
            {
                ControlAnnotationControl annotation = _annotationControls[index];
                AnimateAnnotation(annotation, null, false);
            }
        }

        /// <summary>
        /// Highlight or unhighlight an annotation
        /// </summary>
        /// <param name="annotation">Not null</param>
        /// <param name="highlight"></param>
        private void AnimateAnnotation(ControlAnnotationControl annotation, KxControlEventArgs inputEvent, bool highlight)
        {
            int annotationIndex = (int)annotation.Tag;
            if (highlight)
            {
                // Highlight and store the event
                annotation.IsHighlighted = true;
                _annotationAnimations[annotationIndex] = inputEvent;
            }
            else if (annotation.IsHighlighted)
            {
                // Unhighlight
                annotation.IsHighlighted = false;
                if (_annotationAnimations.ContainsKey(annotationIndex))
                {
                    _annotationAnimations.Remove(annotationIndex);
                }
            }
        }

        /// <summary>
        /// Refresh the annotations
        /// </summary>
        public void RefreshDisplay()
        {
            if (_isLoaded && _source != null && _currentSituation != null)
            {
                // Clear the annotation bindings cache
                _annotationBindings.Clear();

                UpdateAnnotations();
                if (!IsDesignMode)
                {
                    HighlightCurrentSituation();
                }
                //if (IsDesignMode)
                //{
                //    ControlAnnotationControl annotationToSelect = FindCurrentAnnotation();
                //    SelectAnnotation(annotationToSelect);
                //}
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

                // Apply design mode to child controls if required
                if (IsDesignMode)
                {
                    foreach (ControlAnnotationControl annotation in _annotationControls)
                    {
                        annotation.IsDesignMode = true;
                        annotation.AnnotationClicked += this.Annotation_Clicked;
                        annotation.AnnotationRightClicked += this.Annotation_RightClicked;
                        annotation.DragStarted += this.Annotation_DragStarted;
                        annotation.DragDropped += this.Annotation_DragDropped;
                    }
                }

                RefreshDisplay();
            }
        }

        /// <summary>
        /// Handle selection of annotation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Annotation_Clicked(object sender, RoutedEventArgs e)
        {
            // Set the keyboard focus
            SetFocus();
            SelectAnnotation((ControlAnnotationControl)sender);

            e.Handled = true;
        }

        /// <summary>
        /// Handle selection of annotation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Annotation_RightClicked(object sender, RoutedEventArgs e)
        {
            // Set the keyboard focus
            SetFocus();
            SelectAnnotation((ControlAnnotationControl)sender);

            RaiseEvent(new RoutedEventArgs(KxQuickEditActionsEvent));

            e.Handled = true;
        }

        /// <summary>
        /// Start drag drop if required
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Annotation_DragStarted(object sender, MouseEventArgs e)
        {
            if (_currentSituation != null)
            {
                // See if starting drag is allowed
                ControlAnnotationControl sourceAnnotation = (ControlAnnotationControl)sender;
                int annotationIndex = (int)sourceAnnotation.Tag;

                // Get the grid bindings for this mode
                int modeID = _currentSituation.ModeID;
                GridBinding[] annotationBindings = GetGridBindings(modeID);

                // Raise situation change and input event
                GridBinding binding = annotationBindings[annotationIndex];
                if (binding != null)
                {
                    StateVector sourceSituation = _source.Utils.RelativeStateToAbsolute(binding.State, _currentSituation); 
                    
                    KxControlEventArgs inputControl = binding.EventArgs;
                    ActionSet actionSet = _source.Actions.GetActionsForInputControl(sourceSituation, inputControl, true);
                    if (actionSet != null && actionSet.LogicalState.ModeID != Constants.DefaultID)
                    {
                        // Annotation has actions that are for this grid so allow drag drop
                        _dragStartControl = (ControlAnnotationControl)sender;                        

                        DataObject dragData = new DataObject(typeof(ActionSet).FullName, actionSet);
                        DragDrop.DoDragDrop(sourceAnnotation, dragData, DragDropEffects.Move);
                    }
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle drag drop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Annotation_DragDropped(object sender, DragEventArgs e)
        {
            if ((e.Effects & DragDropEffects.Copy | DragDropEffects.Move) != 0 &&
                e.Data.GetDataPresent(typeof(ActionSet).FullName) &&
                sender != _dragStartControl)
            {
                // Get the source actions
                ActionSet sourceActionSet = e.Data.GetData(typeof(ActionSet).FullName) as ActionSet;
                KxControlEventArgs sourceControl = sourceActionSet.EventArgs;
                StateVector sourceSituation = sourceActionSet.LogicalState;
                EDirectionMode sourceDirectionality = _source.Utils.GetActiveDirectionMode(sourceSituation, sourceControl);
                
                // Get the destination annotation
                ControlAnnotationControl destAnnotation = (ControlAnnotationControl)sender;
                int annotationIndex = (int)destAnnotation.Tag;

                // Get the grid bindings for this mode
                int modeID = sourceSituation.ModeID;
                GridBinding[] annotationBindings = GetGridBindings(modeID);
                GridBinding binding = annotationBindings[annotationIndex];
                if (binding != null)
                {
                    KxControlEventArgs destControl = binding.EventArgs;
                    StateVector destSituation = _source.Utils.RelativeStateToAbsolute(binding.State, sourceSituation);

                    EDirectionMode destDirectionality = _source.Utils.GetActiveDirectionMode(destSituation, destControl);
                    ActionSet destActionSet = _source.Actions.GetActionsForInputControl(destSituation, destControl, false);    // May be null

                    // Source and destination are compatible if they are either both discrete or both continuous
                    bool compatible = (sourceDirectionality == EDirectionMode.Continuous) == (destDirectionality == EDirectionMode.Continuous);
                    if (compatible)
                    {
                        // Import the source actions to the destination control and vice versa
                        GeneralisedControl fromSource = new GeneralisedControl(EDirectionMode.NonDirectional, sourceControl);
                        GeneralisedControl toDest = new GeneralisedControl(destDirectionality, destControl);
                        GeneralisedControl fromDest = new GeneralisedControl(EDirectionMode.NonDirectional, destControl);
                        GeneralisedControl toSource = new GeneralisedControl(sourceDirectionality, sourceControl);
                        compatible = _source.ActionEditor.CanConvertActionSet(sourceActionSet, fromSource, destSituation, toDest);
                        if (compatible && destActionSet != null)
                        {
                            compatible = _source.ActionEditor.CanConvertActionSet(destActionSet, fromDest, sourceSituation, toSource);
                        }

                        // Check whether the respective controls support each other's actions
                        if (compatible)
                        {
                            // Remove the current actions
                            _source.Actions.RemoveActionSet(sourceActionSet);
                            if (destActionSet != null)
                            {
                                _source.Actions.RemoveActionSet(destActionSet);
                            }

                            // Import the source actions to the destination control
                            _source.ActionEditor.ConvertActionSet(sourceActionSet, fromSource, destSituation, toDest);
                            sourceActionSet.LogicalState = destSituation;
                            _source.Actions.AddActionSet(sourceActionSet);

                            // Import any destination actions to the source control
                            if (destActionSet != null)
                            {
                                _source.ActionEditor.ConvertActionSet(destActionSet, fromDest, sourceSituation, toSource);
                                destActionSet.LogicalState = sourceSituation;
                                _source.Actions.AddActionSet(destActionSet);
                            }

                            // Tell the parent to refresh the display
                            RaiseEvent(new RoutedEventArgs(KxActionsEditedEvent));
                        }
                    }
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Refresh the annotations on the grid cells
        /// </summary>
        private void UpdateAnnotations()
        {
            StringUtils utils = new StringUtils();
            
            // Find the current mode and its type of grid
            int modeID = _currentSituation.ModeID;
            AxisValue modeItem = _source.StateTree.GetMode(modeID);
            if (modeItem == null || (modeItem.GridType != EGridType.Square4x4 && modeItem.GridType != EGridType.Square8x4))
            {
                // Error
                return;
            }

            // Work out what the selection controls are for this mode
            KxControlEventArgs actionSelectorControl = modeItem.Controls.SelectionControl.ReferenceControl;

            // Decide whether the centre annotation is applicable
            bool showCentre = actionSelectorControl.ControlType == EVirtualControlType.DPad ||
                                actionSelectorControl.ControlType == EVirtualControlType.Stick;            

            // Loop over cells
            GridBinding[] annotationBindings = GetGridBindings(modeItem.ID);
            for (int i = 0; i < 9; i++)
            {
                StateVector situation = new StateVector(_currentSituation);
                situation.CellID = Constants.TopLeftCellID + i;
                ActionMappingTable actionMappings = _source.Actions.GetActionsForState(situation, true);
                EDirectionMode cellDirectionMode = _source.Utils.GetActiveDirectionMode(situation, actionSelectorControl);                

                // Loop over annotations in cell
                for (int j = 0; j < 9; j++)
                {
                    int controlIndex = 9 * i + j;

                    ActionSet actionSet = null;
                    bool annotationApplicable = false;
                    GridBinding binding = annotationBindings[controlIndex];
                    if (binding != null)
                    {
                        // See if the direction is relevant to the selection control
                        if (binding.EventArgs.LRUDState == ELRUDState.Centre)
                        {
                            annotationApplicable = showCentre;
                        }
                        else
                        {
                            annotationApplicable = _source.Utils.IsDirectionValid(Constants.SquareGridDirections[j], cellDirectionMode/*, true*/);
                        }

                        if (annotationApplicable)
                        {
                            // Annotation applicable so get actions to bind
                            actionSet = actionMappings.GetActions(binding.EventArgs.ToID(), true);

                            // Ignore the action set if it is inherited from a parent situation with a different directionality
                            if (actionSet != null && !actionSet.LogicalState.IsSameAs(situation))
                            {
                                EDirectionMode inheritedDirectionality = _source.Utils.GetActiveDirectionMode(actionSet.LogicalState, actionSet.EventArgs);
                                if ((cellDirectionMode == EDirectionMode.Continuous) != (inheritedDirectionality == EDirectionMode.Continuous))
                                {
                                    actionSet = null;
                                }
                            }

                            BindAnnotationActions(_annotationControls[controlIndex], actionSet);
                        }
                    }
                    if (!annotationApplicable)
                    {
                        _annotationControls[controlIndex].Visibility = Visibility.Hidden;
                    }
                }
            }

            // If the selected annotation has been hidden, deselect it
            if (_selectedAnnotation != null && _selectedAnnotation.Visibility != Visibility.Visible)
            {
                SelectAnnotation(null);
            }
        }

        /// <summary>
        /// Store some tables of control bindings for efficiency
        /// </summary>
        private GridBinding[] GetGridBindings(int modeID)
        {
            GridBinding[] gridBindings = null;

            if (_annotationBindings.ContainsKey(modeID))
            {
                // Return cached bindings
                gridBindings = _annotationBindings[modeID];
            }
            else
            {
                // Create new bindings
                gridBindings = new GridBinding[81];
                
                // Get the grid for this mode
                AxisValue modeItem = _source.StateTree.GetMode(modeID);
                if (modeItem != null && modeItem.Grid != null)
                {
                    // Populate the bindings array
                    foreach (GridBinding binding in modeItem.Grid.Bindings)
                    {
                        ControlAnnotationControl annotation = (ControlAnnotationControl)FindName(binding.UIControlName);
                        if (annotation != null)
                        {
                            int annotationIndex = (int)annotation.Tag;
                            gridBindings[annotationIndex] = binding;
                        }
                    }
                }

                // Cache
                _annotationBindings[modeID] = gridBindings;
            }

            return gridBindings;
        }

        /// <summary>
        /// Bind actions to an annotation
        /// </summary>
        /// <param name="annotation"></param>
        /// <param name="actionSet"></param>
        private void BindAnnotationActions(ControlAnnotationControl annotation, ActionSet actionSet)
        {
            bool actionsShown = false;
            if (actionSet != null)
            {
                if (IsDesignMode)
                {
                    annotation.CurrentText = actionSet.Info.TinyDescription;
                    annotation.ToolTip = string.Format("{0}{1}{2}",
                                                                    actionSet.Info.ShortDescription,
                                                                    Environment.NewLine,
                                                                    "(Click to select, right-click for quick-edit options)");
                    annotation.IconRef = actionSet.Info.IconRef;

                    // Decide whether to show or hide the annotation
                    annotation.Visibility = Visibility.Visible;
                    actionsShown = true;
                }
                else if (actionSet.Info.IconRef != EAnnotationImage.DontShow && actionSet.Info.IconRef != EAnnotationImage.DoNothing)
                {
                    annotation.CurrentText = actionSet.Info.TinyDescription;
                    annotation.ToolTip = actionSet.Info.ShortDescription != "" ? actionSet.Info.ShortDescription : null;
                    annotation.IconRef = actionSet.Info.IconRef;

                    // Decide whether to show or hide the annotation
                    annotation.Visibility = Visibility.Visible;
                    actionsShown = true;
                }
            }

            if (!actionsShown)
            {
                annotation.CurrentText = null;
                annotation.ToolTip = IsDesignMode ? Properties.Resources.String_ClickOrRightClickToolTip : null;
                annotation.IconRef = EAnnotationImage.None;

                annotation.Visibility = (IsDesignMode || ShowUnusedControls) ? Visibility.Visible : Visibility.Hidden;
            }
        }

        /// <summary>
        /// Find the annotation for the current situation and input event, if any
        /// </summary>
        private ControlAnnotationControl FindAnnotation(KxControlEventArgs inputEvent)
        {
            // Find the annotation to select
            ControlAnnotationControl requiredAnnotation = null;
            if (inputEvent != null)
            {
                int inputID = inputEvent.ToID();
                int modeID = _currentSituation.ModeID;
                GridBinding[] annotationBindings = GetGridBindings(modeID);

                int startAnnotationIndex = 9 * (int)(_currentSituation.CellID - Constants.TopLeftCellID);
                if (startAnnotationIndex > -1 && startAnnotationIndex < annotationBindings.Length - 9)
                {
                    for (int i = startAnnotationIndex; i < startAnnotationIndex + 9; i++)
                    {
                        // Don't return annotation if it's hidden
                        ControlAnnotationControl annotation = _annotationControls[i];
                        if (annotation.Visibility == System.Windows.Visibility.Visible)
                        {
                            GridBinding binding = annotationBindings[i];
                            if (binding != null && binding.EventArgs.ToID() == inputID)
                            {
                                // Found correct annotation
                                requiredAnnotation = annotation;
                                break;
                            }
                        }
                    }
                }
            }

            return requiredAnnotation;
        }

        /// <summary>
        /// Highlight the current page of controls
        /// </summary>
        private void HighlightCurrentSituation()
        {
            // Find the cell to highlight
            Panel panelToSelect = null;
            int panelIndex = (int)(_currentSituation.CellID - Constants.TopLeftCellID);
            if (panelIndex > -1 && panelIndex < 9)
            {
                panelToSelect = _panelArray[panelIndex];
            }

            // See if the current cell has changed
            if (panelToSelect != _selectedPanel)
            {
                // Unhighlight current cell
                if (_selectedPanel != null)
                {
                    _selectedPanel.Background = _defaultCellBrush;
                }

                // Highlight new cell
                if (panelToSelect != null)
                {
                    _defaultCellBrush = panelToSelect.Background;
                    panelToSelect.Background = _selectedCellBrush;
                }

                _selectedPanel = panelToSelect;
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
                _currentInputEvent = null;
                if (_selectedAnnotation != null)
                {
                    _selectedAnnotation.IsSelected = true;

                    if (_currentSituation != null)
                    {
                        // Get the grid bindings for this mode
                        int modeID = _currentSituation.ModeID;
                        GridBinding[] annotationBindings = GetGridBindings(modeID);

                        // Raise situation change and input event
                        int annotationIndex = (int)_selectedAnnotation.Tag;
                        GridBinding binding = annotationBindings[annotationIndex];
                        if (binding != null)
                        {
                            // Set current input
                            _currentInputEvent = binding.EventArgs;

                            // See if the annotation is for a different situation
                            StateVector newSituation = _source.Utils.RelativeStateToAbsolute(binding.State, _currentSituation);
                            if (!newSituation.IsSameAs(_currentSituation))
                            {
                                _currentSituation = newSituation;                                
                                //HighlightCurrentSituation();                                

                                RaiseEvent(new KxStateRoutedEventArgs(KxStateChangedEvent, newSituation));
                            }
                        }
                    }
                }

                RaiseEvent(new KxInputControlRoutedEventArgs(KxInputControlChangedEvent, _currentInputEvent));
            }
        }
    }
}
