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
    /// Base class for keyboard grid controls
    /// </summary>
    public class BaseGridControl : KxUserControl, IActionViewerControl
    {        
        // Fields
        private bool _isLoaded = false;
        private BaseSource _source;
        private ColourScheme _colourScheme;
        private SolidColorBrush _highlightBrush;
        private SolidColorBrush _selectionBrush;
        private ControlAnnotationControl _selectedAnnotation;
        private ControlAnnotationControl _dragStartControl;
        private StateVector _currentSituation;
        private KxControlEventArgs _currentInputEvent;
        private ControlAnnotationControl[] _annotationControls;
        private Dictionary<int, AnnotationBindingsLookup> _annotationBindings;
        private Dictionary<int, KxControlEventArgs> _annotationAnimations;
        private List<AnnotationBinding> _highlightedAnnotations;
        
        // Properties
        protected ColourScheme ColourScheme { get { return _colourScheme; } }
        protected BaseSource Source { get { return _source; } }
        public StateVector CurrentSituation { get { return _currentSituation; } }
        public KxControlEventArgs CurrentInputEvent { get { return _currentInputEvent; } }

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
            typeof(BaseGridControl),
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
            typeof(BaseGridControl),
            new FrameworkPropertyMetadata(false)
        );

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseGridControl()
            :base()
        {
            _highlightBrush = new SolidColorBrush(Constants.DefaultHighlightColour);
            _selectionBrush = new SolidColorBrush(Constants.DefaultSelectionColour);
           
            // Create bindings table
            _annotationBindings = new Dictionary<int, AnnotationBindingsLookup>();

            // Create highlights array
            _annotationAnimations = new Dictionary<int, KxControlEventArgs>();
        }

        /// <summary>
        /// Set the annotations to display
        /// </summary>
        /// <param name="annotations"></param>
        protected void SetAnnotationControls(ControlAnnotationControl[] annotations)
        {
            _annotationControls = annotations;

            // Configure annotations
            for (int i = 0; i < _annotationControls.Length; i++)
            {
                ControlAnnotationControl annotation = _annotationControls[i];

                annotation.Tag = i;
                annotation.HighlightBrush = _highlightBrush;
                annotation.SelectionBrush = _selectionBrush;
            }
        }

        /// <summary>
        /// Focus the control
        /// </summary>
        public virtual void SetFocus()
        {
        }
        
        /// <summary>
        /// Set the config
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
        /// Set the current profile
        /// </summary>
        /// <param name="profile"></param>
        public void SetSource(BaseSource source)
        {
            _source = source;

            // Reset current situation and input event
            _annotationBindings.Clear();
            if (_isLoaded)
            {
                ClearAnimations();
            }
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

                if (!_highlightBrush.IsFrozen)
                {
                    _highlightBrush.Color = ColourUtils.ColorFromString(playerColours.HighlightColour, Constants.DefaultHighlightColour);
                }

                if (!_selectionBrush.IsFrozen)
                {
                    _selectionBrush.Color = ColourUtils.ColorFromString(playerColours.SelectionColour, Constants.DefaultSelectionColour);
                }

                this.Resources["BackgroundColor"] = ColourUtils.ColorFromString(playerColours.CellColour, _colourScheme.InteractiveControlsOpacity, Constants.DefaultCellColour);
            }
        }

        /// <summary>
        /// Handle change of situation
        /// </summary>
        /// <param name="args"></param>
        public virtual void SetCurrentSituation(StateVector situation)
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
                if (annotation != null/* && inputEvent is KxControllerEventArgs*/)
                {
                    //KxControllerEventArgs ev = (KxControllerEventArgs)inputEvent;                    
                    switch (inputEvent.EventReason)
                    {
                        case EEventReason.Directed:
                        case EEventReason.Pressed:
                            if (inputEvent.LRUDState != ELRUDState.Centre)
                            {
                                AnimateAnnotation(annotation, inputEvent, true);
                            }
                            break;
                        case EEventReason.Released:
                        case EEventReason.Undirected:
                            if (inputEvent.LRUDState != ELRUDState.Centre)
                            {
                                AnimateAnnotation(annotation, null, false);
                            }
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
        /// <param name="animate"></param>
        private void AnimateAnnotation(ControlAnnotationControl annotation, KxControlEventArgs inputEvent, bool animate)
        {
            int annotationIndex = (int)annotation.Tag;
            if (animate)
            {
                // Highlight and store the event
                annotation.IsHighlighted = true;
                _annotationAnimations[annotationIndex] = inputEvent;
            }
            else if (annotation.IsHighlighted)
            {
                annotation.IsHighlighted = false;
                if (_annotationAnimations.ContainsKey(annotationIndex))
                {                    
                    _annotationAnimations.Remove(annotationIndex);
                }
            }
        }

        /// <summary>
        /// Control loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
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
                _isLoaded = true;
                RefreshDisplay();
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

            RaiseEvent(new RoutedEventArgs(KxUserControl.KxQuickEditActionsEvent));

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
                AnnotationBindingsLookup annotationBindingsTable = GetGridBindings(modeID);
                List<GridBinding> annotationBindings = annotationBindingsTable.GetBindingsForAnnotation(annotationIndex);
                if (annotationBindings != null && annotationBindings.Count != 0)
                {
                    GridBinding binding = annotationBindings[0];
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
                AnnotationBindingsLookup annotationBindingsTable = GetGridBindings(modeID);
                List<GridBinding> annotationBindings = annotationBindingsTable.GetBindingsForAnnotation(annotationIndex);
                if (annotationBindings != null && annotationBindings.Count != 0)
                {
                    GridBinding binding = annotationBindings[0];
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
                                RoutedEventArgs args = new RoutedEventArgs(KxUserControl.KxActionsEditedEvent);
                                RaiseEvent(args);
                            }
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
            if (modeItem == null || (modeItem.GridType != EGridType.Keyboard && modeItem.GridType != EGridType.ActionStrip))
            {
                // Error
                return;
            }

            // Loop over annotations
            AnnotationBindingsLookup annotationBindingsTable = GetGridBindings(modeItem.ID);
            for (int i=0; i<_annotationControls.Length; i++)
            {
                ControlAnnotationControl annotation = _annotationControls[i];
                List<GridBinding> annotationBindings = annotationBindingsTable.GetBindingsForAnnotation(i);
                if (annotationBindings != null && annotationBindings.Count != 0)
                {
                    GridBinding binding = annotationBindings[0];
                    StateVector situation = _source.Utils.RelativeStateToAbsolute(binding.State, _currentSituation);

                    // Annotation applicable so get actions to bind
                    ActionSet actionSet = _source.Actions.GetActionsForInputControl(situation, binding.EventArgs, true);
                    BindAnnotationActions(annotation, actionSet);
                }
                else
                {
                    annotation.Visibility = Visibility.Collapsed;
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
        private AnnotationBindingsLookup GetGridBindings(int modeID)
        {
            AnnotationBindingsLookup gridBindings = null;

            if (_annotationBindings.ContainsKey(modeID))
            {
                // Return cached bindings
                gridBindings = _annotationBindings[modeID];
            }
            else
            {
                // Create new bindings
                gridBindings = new AnnotationBindingsLookup(_annotationControls.Length);

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
                            gridBindings.AddBinding(binding.State.CellID, (int)annotation.Tag, binding);
                        }
                    }
                }

                // Cache
                _annotationBindings[modeID] = gridBindings;
            }

            return gridBindings;
        }

        /// <summary>
        /// Drop cached grid bindings when grid design changes (e.g. number of cells)
        /// </summary>
        /// <param name="modeID"></param>
        protected void UncacheGridBindings(int modeID)
        {
            if (_annotationBindings.ContainsKey(modeID))
            {
                _annotationBindings.Remove(modeID);
            }
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
                                                                    Properties.Resources.String_ClickOrRightClickToolTip);
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
                int modeID = _currentSituation.ModeID;
                AnnotationBindingsLookup annotationBindings = GetGridBindings(modeID);
                List<AnnotationBinding> cellBindings = annotationBindings.GetBindingsForCell(_currentSituation.CellID);
                if (cellBindings != null)
                {
                    int inputID = inputEvent.ToID();
                    foreach (AnnotationBinding cellBinding in cellBindings)
                    {
                        if (cellBinding.Binding.EventArgs.ToID() == inputID)
                        {
                            // Found correct annotation
                            requiredAnnotation = _annotationControls[cellBinding.AnnotationIndex];
                            break;
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
            // Unhighlight current
            if (_highlightedAnnotations != null)
            {
                foreach (AnnotationBinding binding in _highlightedAnnotations)
                {
                    ControlAnnotationControl annotation = _annotationControls[binding.AnnotationIndex];
                    annotation.IsSelected = false;
                }
            }

            // Get new annotations to highlight
            AnnotationBindingsLookup gridBindings = GetGridBindings(_currentSituation.ModeID);
            _highlightedAnnotations = gridBindings.GetBindingsForCell(_currentSituation.CellID);
            
            // Highlight new anotations
            if (_highlightedAnnotations != null)
            {
                foreach (AnnotationBinding binding in _highlightedAnnotations)
                {
                    ControlAnnotationControl annotation = _annotationControls[binding.AnnotationIndex];
                    annotation.IsSelected = true;
                }
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
                        AnnotationBindingsLookup annotationBindings = GetGridBindings(modeID);
                        List<GridBinding> bindingsForAnnotation = annotationBindings.GetBindingsForAnnotation((int)_selectedAnnotation.Tag);
                        if (bindingsForAnnotation != null)
                        {
                            // In case the annotation is bound to more than one situation, try to choose the one for the current cell                        
                            GridBinding matchedBinding = null;
                            int currentCellID = _currentSituation.CellID;
                            foreach (GridBinding binding in bindingsForAnnotation)
                            {
                                matchedBinding = binding;
                                if (binding.State.CellID == currentCellID)
                                {
                                    break;
                                }
                            }

                            if (matchedBinding != null)
                            {
                                // Set current input
                                _currentInputEvent = matchedBinding.EventArgs;

                                // See if the annotation is for a different situation
                                StateVector newSituation = _source.Utils.RelativeStateToAbsolute(matchedBinding.State, _currentSituation);
                                if (!newSituation.IsSameAs(_currentSituation))
                                {
                                    _currentSituation = newSituation;

                                    RaiseEvent(new KxStateRoutedEventArgs(KxUserControl.KxStateChangedEvent, newSituation));
                                }
                            }
                        }
                    }
                }

                // Raise input changed event
                RaiseEvent(new KxInputControlRoutedEventArgs(KxUserControl.KxInputControlChangedEvent, _currentInputEvent));
            }
        }
    }
}