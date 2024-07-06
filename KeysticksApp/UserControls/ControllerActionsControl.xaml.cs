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
using System.Windows.Input;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Sources;
using Keysticks.UI;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for displaying the layout of a virtual controller and the actions assigned to its controls
    /// </summary>
    public partial class ControllerActionsControl : BaseControllerControl, IActionViewerControl
    {
        // Fields
        private bool _isLoaded = false;
        private StringUtils _utils = new StringUtils();
        private StateVector _currentSituation;
        private KxControlEventArgs _currentInputEvent;
        private ControlAnnotationControl _dragStartControl;

        // Properties
        public StateVector CurrentSituation { get { return _currentSituation; } }
        public KxControlEventArgs CurrentInputEvent { get { return _currentInputEvent; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public ControllerActionsControl()
            : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Acquire the focus
        /// </summary>
        public void SetFocus()
        {
            OuterPanel.Focus();
        }

        /// <summary>
        /// Set the profile
        /// </summary>
        /// <param name="profile"></param>
        public override void SetSource(BaseSource source)
        {
            base.SetSource(source);

            if (_isLoaded)
            {
                InitialiseDisplay();
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
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;

                InitialiseDisplay();
            }
        }

        /// <summary>
        /// Create the display
        /// </summary>
        public override void InitialiseDisplay()
        {            
            if (Source != null)
            {
                CreateDisplay(ControllerCanvas, Source, false);
                RefreshDisplay();

                // Clear current selection if no longer valid
                if (_isLoaded && _currentInputEvent != null && Source.GetVirtualControl(_currentInputEvent) == null)
                {
                    SetCurrentAnnotation(null);
                }
            }
        }
 
        /// <summary>
        /// Refresh the annotations
        /// </summary>
        public override void RefreshDisplay()
        {
            if (_isLoaded && Source != null && _currentSituation != null)
            {
                // Display actions
                ActionMappingTable actionMappings = Source.Actions.GetActionsForState(_currentSituation, true);
                foreach (ControlAnnotationControl annotation in AnnotationsList)
                {
                    AnnotationData control = (AnnotationData)annotation.Tag;
                    ActionSet actionSet = actionMappings.GetActions(control.InputBinding.ToID(), true);

                    // Ignore the action set if it is inherited from a parent situation with a different directionality
                    if (actionSet != null && !actionSet.LogicalState.IsSameAs(_currentSituation))
                    {
                        EDirectionMode thisDirectionality = Source.Utils.GetActiveDirectionMode(_currentSituation, actionSet.EventArgs);
                        EDirectionMode inheritedDirectionality = Source.Utils.GetActiveDirectionMode(actionSet.LogicalState, actionSet.EventArgs);
                        if ((thisDirectionality == EDirectionMode.Continuous) != (inheritedDirectionality == EDirectionMode.Continuous))
                        {
                            actionSet = null;
                        }
                    }

                    BindAnnotationActions(annotation, actionSet);
                }

                // Hide directional annotations that don't apply
                foreach (AnnotationGroup annotationGroup in DirectionalControlAnnotations)
                {
                    HideDirectionalAnnotations(annotationGroup);
                }

                // When it's a keyboard situation but there's no current cell, show which control is used to select in the keyboard
                if (_currentSituation.CellID == Constants.DefaultID)
                {
                    AxisValue modeItem = Source.Utils.GetModeItem(_currentSituation);
                    if (modeItem != null && modeItem.GridType != EGridType.None && modeItem.Controls != null)
                    {
                        GeneralisedControl selectionControl = modeItem.Controls.SelectionControl;
                        foreach (ControlAnnotationControl annotation in AnnotationsList)
                        {
                            AnnotationData control = (AnnotationData)annotation.Tag;
                            if (annotation.CurrentText == null && 
                                annotation.IconRef == EAnnotationImage.None &&
                                Source.Utils.IsGeneralTypeOfControl(control.InputBinding, selectionControl))
                            {
                                // Show when a control is a keyboard selection control, provided it doesn't have any actions to show
                                annotation.CurrentText = null;
                                annotation.ToolTip = Properties.Resources.String_PerformActionsForCellToolTip;
                                annotation.IconRef = EAnnotationImage.Accept;

                                annotation.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }

                // If the selected annotation has been hidden, deselect it
                if (SelectedAnnotation != null && SelectedAnnotation.Visibility != Visibility.Visible)
                {
                    SetCurrentAnnotation(null);
                }
            }
        }

        /// <summary>
        /// Refresh the controller background and layout
        /// </summary>
        public override void RefreshLayout()
        {
            if (_isLoaded && Source != null)
            {
                CreateBackground(ControllerCanvas, Source);
                PositionAnnotations(Source);
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
        /// Handle selection of annotation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void Annotation_Clicked(object sender, RoutedEventArgs e)
        {
            if (IsDesignMode)
            {
                // Select the annotation
                SetCurrentAnnotation((ControlAnnotationControl)sender);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle selection of annotation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void Annotation_RightClicked(object sender, RoutedEventArgs e)
        {
            if (IsDesignMode)
            {
                // Select the annotation
                SetCurrentAnnotation((ControlAnnotationControl)sender);

                // Raise quick edit event
                RaiseEvent(new RoutedEventArgs(KxQuickEditActionsEvent));
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle selection of an annotation
        /// </summary>
        /// <param name="annotation">Can be null</param>
        private void SetCurrentAnnotation(ControlAnnotationControl annotation)
        {
            SelectAnnotation(annotation);

            KxControlEventArgs inputControl = null;
            if (annotation != null)
            {
                // Set the keyboard focus
                SetFocus();

                AnnotationData data = (AnnotationData)annotation.Tag;
                inputControl = data.InputBinding;
            }
            _currentInputEvent = inputControl;
            RaiseEvent(new KxInputControlRoutedEventArgs(KxInputControlChangedEvent, _currentInputEvent));
        }

        /// <summary>
        /// Start drag drop if required
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void Annotation_DragStarted(object sender, MouseEventArgs e)
        {
            if (_currentSituation != null)
            {                
                // See if starting drag is allowed
                ControlAnnotationControl sourceAnnotation = (ControlAnnotationControl)sender;
                AnnotationData control = (AnnotationData)sourceAnnotation.Tag;
                ActionSet actionSet = Source.Actions.GetActionsForInputControl(_currentSituation, control.InputBinding, true);
                if (actionSet != null)
                {
                    // Annotation has actions so allow drag drop
                    _dragStartControl = (ControlAnnotationControl)sender;

                    DataObject dragData = new DataObject(typeof(ActionSet).FullName, actionSet);
                    DragDrop.DoDragDrop(sourceAnnotation, dragData, DragDropEffects.Move);
                }                    
            }

            e.Handled = true;
        }
        
        /// <summary>
        /// Handle drag drop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected  override void Annotation_DragDropped(object sender, DragEventArgs e)
        {
            if ((e.Effects & DragDropEffects.Copy | DragDropEffects.Move) != 0 &&
                e.Data.GetDataPresent(typeof(ActionSet).FullName) &&
                sender != _dragStartControl)
            {
                // Get the source actions
                ActionSet sourceActionSet = e.Data.GetData(typeof(ActionSet).FullName) as ActionSet;
                StateVector situation = sourceActionSet.LogicalState;
                KxControlEventArgs sourceControl = sourceActionSet.EventArgs;
                EDirectionMode sourceDirectionality = Source.Utils.GetActiveDirectionMode(situation, sourceControl);

                // Get the destination actions
                ControlAnnotationControl destAnnotation = (ControlAnnotationControl)sender;
                AnnotationData control = (AnnotationData)destAnnotation.Tag;
                KxControlEventArgs destControl = control.InputBinding;
                EDirectionMode destDirectionality = Source.Utils.GetActiveDirectionMode(situation, destControl);
                ActionSet destActionSet = Source.Actions.GetActionsForInputControl(situation, destControl, false);    // May be null

                // Source and destination are compatible if they are either both discrete or both continuous
                bool compatible = (sourceDirectionality == EDirectionMode.Continuous) == (destDirectionality == EDirectionMode.Continuous);
                if (compatible)
                {
                    // Import the source actions to the destination control and vice versa
                    GeneralisedControl fromSource = new GeneralisedControl(EDirectionMode.NonDirectional, sourceControl);
                    GeneralisedControl toDest = new GeneralisedControl(destDirectionality, destControl);
                    GeneralisedControl fromDest = new GeneralisedControl(EDirectionMode.NonDirectional, destControl);
                    GeneralisedControl toSource = new GeneralisedControl(sourceDirectionality, sourceControl);
                    compatible = Source.ActionEditor.CanConvertActionSet(sourceActionSet, fromSource, situation, toDest);
                    if (compatible && destActionSet != null)
                    {
                        compatible = Source.ActionEditor.CanConvertActionSet(destActionSet, fromDest, situation, toSource);
                    }

                    // Check whether the respective controls support each other's actions
                    if (compatible)
                    {
                        // Remove the current actions
                        Source.Actions.RemoveActionSet(sourceActionSet);
                        if (destActionSet != null)
                        {
                            Source.Actions.RemoveActionSet(destActionSet);
                        }

                        // Import the source actions to the destination control
                        Source.ActionEditor.ConvertActionSet(sourceActionSet, fromSource, situation, toDest);
                        sourceActionSet.LogicalState = situation;
                        Source.Actions.AddActionSet(sourceActionSet);

                        // Import any destination actions to the source control
                        if (destActionSet != null)
                        {
                            Source.ActionEditor.ConvertActionSet(destActionSet, fromDest, situation, toSource);
                            destActionSet.LogicalState = situation;
                            Source.Actions.AddActionSet(destActionSet);
                        }

                        // Tell the parent
                        RaiseEvent(new RoutedEventArgs(KxActionsEditedEvent));
                    }
                }
            }

            e.Handled = true;
        }
        
        /// <summary>
        /// Show the right annotations for the direction mode of the controls for this page
        /// </summary>
        private void HideDirectionalAnnotations(AnnotationGroup annotationGroup)
        {
            // Get the centre control
            if (annotationGroup.CentreAnnotation != null)
            {
                // Get the direction style of the control in this situation
                AnnotationData centreControl = (AnnotationData)annotationGroup.CentreAnnotation.Tag;
                EDirectionMode directionMode = Source.Utils.GetActiveDirectionMode(_currentSituation, centreControl.InputBinding);

                // Loop over annotations for control
                foreach (ControlAnnotationControl annotation in annotationGroup.Annotations)
                {
                    AnnotationData control = (AnnotationData)annotation.Tag;
                    KxControlEventArgs args = control.InputBinding;
                    if (!Source.Utils.IsDirectionValid(args.LRUDState, directionMode/*, true*/))
                    {
                        annotation.Visibility = Visibility.Hidden;                                                   
                    }
                }
            }
        }
    }
}
