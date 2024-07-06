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
    /// Controller layout and input mappings control
    /// </summary>
    public partial class ControllerDesignControl : BaseControllerControl, ISourceViewerControl
    {
        // Fields
        private bool _isLoaded = false;
        private IProfileEditorWindow _editorWindow;
        private AppConfig _appConfig;
        private KxControlEventArgs _currentInputEvent;
        private bool _dragInProgress;
        private Point _dragStartPoint = new Point();

        // Routed events
        public static readonly RoutedEvent KxEditBackgroundEvent = EventManager.RegisterRoutedEvent(
            "EditBackground", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ControllerDesignControl));
        public static readonly RoutedEvent KxLayoutEditedEvent = EventManager.RegisterRoutedEvent(
            "LayoutEdited", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ControllerDesignControl));
        public static readonly RoutedEvent KxControlsEditedEvent = EventManager.RegisterRoutedEvent(
            "ControlsEdited", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ControllerDesignControl));
        public event KxStateRoutedEventHandler EditBackground
        {
            add { AddHandler(KxEditBackgroundEvent, value); }
            remove { RemoveHandler(KxEditBackgroundEvent, value); }
        }
        public event RoutedEventHandler LayoutEdited
        {
            add { AddHandler(KxLayoutEditedEvent, value); }
            remove { RemoveHandler(KxLayoutEditedEvent, value); }
        }
        public event RoutedEventHandler ControlsEdited
        {
            add { AddHandler(KxControlsEditedEvent, value); }
            remove { RemoveHandler(KxControlsEditedEvent, value); }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public ControllerDesignControl()
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
        /// Set the profile editor window to report to
        /// </summary>
        /// <param name="profileEditor"></param>
        public void SetProfileEditor(IProfileEditorWindow editorWindow)
        {
            _editorWindow = editorWindow;
        }

        /// <summary>
        /// Set the config
        /// </summary>
        /// <param name="appConfig"></param>
        public override void SetAppConfig(AppConfig appConfig)
        {
            _appConfig = appConfig;
            base.SetAppConfig(appConfig);
        }

        /// <summary>
        /// Set the source
        /// </summary>
        /// <param name="profile"></param>
        public override void SetSource(BaseSource source)
        {
            base.SetSource(source);

            if (_isLoaded)
            {
                InitialiseDisplay();
                RefreshDisplay();
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

                // Show / hide design controls
                DesignOptionsPanel.Visibility = IsDesignMode ? Visibility.Visible : Visibility.Hidden;
                if (IsDesignMode)
                {
                    DesignCanvas.Drop += DesignCanvas_Drop;
                    DesignCanvas.AllowDrop = true;
                    LayoutBorder.BorderThickness = new Thickness(1);
                }

                InitialiseDisplay();
                RefreshDisplay();
            }
        }

        /// <summary>
        /// Create the display
        /// </summary>
        public override void InitialiseDisplay()
        {
            if (Source != null)
            {
                DesignCanvas.Width = Source.Display.Width;
                DesignCanvas.Height = Source.Display.Height;
                PositionControllerCanvas(Source);
                CreateDisplay(ControllerCanvas, Source, true);
                foreach (ControlAnnotationControl annotation in AnnotationsList)
                {
                    annotation.Cursor = Cursors.Hand;
                }
                SetCurrentAnnotation(null);
            }
        }

        /// <summary>
        /// Position the controller canvas inside the outer canvas
        /// </summary>
        /// <param name="source"></param>
        private void PositionControllerCanvas(BaseSource source)
        {
            if (IsCompactView)
            {
                Rect displayRegion = source.GetBoundingRect();
                Canvas.SetLeft(ControllerCanvas, displayRegion.X);
                Canvas.SetTop(ControllerCanvas, displayRegion.Y);
            }
            else
            {
                Canvas.SetLeft(ControllerCanvas, 0.0);
                Canvas.SetTop(ControllerCanvas, 0.0);
            }
        }

        /// <summary>
        /// Refresh the annotations
        /// </summary>
        public override void RefreshDisplay()
        {
            if (_isLoaded && Source != null)
            {
                // Display input mappings
                foreach (ControlAnnotationControl annotation in AnnotationsList)
                {
                    if (annotation.InputBindings != null)
                    {
                        BindAnnotation(annotation);
                    }
                }             
            }
        }

        /// <summary>
        /// Handle selection of annotation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void Annotation_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                SetCurrentAnnotation((ControlAnnotationControl)sender);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_SelectionInControllerDesign, ex);
            }
        }

        /// <summary>
        /// Handle right click of annotation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void Annotation_RightClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                SetCurrentAnnotation((ControlAnnotationControl)sender);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_RightClickInControllerDesign, ex);
            }
        }

        /// <summary>
        /// Set the current annotation
        /// </summary>
        /// <param name="annotation">Can be null</param>
        private void SetCurrentAnnotation(ControlAnnotationControl annotation)
        {
            // Highlight the annotation
            SelectAnnotation(annotation);

            // Set current control
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
        /// Handle start of drag drop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void Annotation_DragStarted(object sender, MouseEventArgs e)
        {
            ControlAnnotationControl annotation = (ControlAnnotationControl)sender;
            
            _dragStartPoint = e.GetPosition(DesignCanvas);
            _dragInProgress = true;
            DataObject dragData = new DataObject(typeof(ControlAnnotationControl).FullName, annotation);
            try
            {
                DesignCanvas.DragOver += new DragEventHandler(DesignCanvas_DragOver);
                DragDrop.DoDragDrop(annotation, dragData, DragDropEffects.Move);
            }
            finally
            {
                DesignCanvas.DragOver -= DesignCanvas_DragOver;
                if (_dragInProgress)
                {
                    // User cancelled drag-drop
                    RefreshLayout();
                    _dragInProgress = false;
                }
            }
            
            e.Handled = true;
        }

        /// <summary>
        /// Position controls during drag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DesignCanvas_DragOver(object sender, DragEventArgs e)
        {
            if ((e.Effects & DragDropEffects.Move) != 0 &&
                e.Data.GetDataPresent(typeof(ControlAnnotationControl).FullName))
            {
                try
                {
                    // Get the source data
                    ControlAnnotationControl annotation = (ControlAnnotationControl)e.Data.GetData(typeof(ControlAnnotationControl).FullName);
                    Point mousePos = e.GetPosition(DesignCanvas);
                    Vector diff = mousePos - _dragStartPoint;
                    if (Source != null)
                    {
                        // Offset the annotation(s) being dragged but don't update the source data until the drag-drop is completed
                        Point origin = new Point();
                        if (IsCompactView)
                        {
                            origin = Source.GetBoundingRect().TopLeft;
                        }
                        origin -= diff;

                        AnnotationGroup group = annotation.Group;
                        if (group != null)
                        {
                            // Move whole group of direction controls (stick / DPad)                                               
                            foreach (ControlAnnotationControl member in group.Annotations)
                            {
                                AnnotationData memberData = (AnnotationData)member.Tag;
                                Canvas.SetLeft(member, memberData.DisplayRect.X - origin.X);
                                Canvas.SetTop(member, memberData.DisplayRect.Y - origin.Y);
                            }
                        }
                        else
                        {
                            // Move single annotation for button, trigger, menu bar or title bar
                            AnnotationData data = (AnnotationData)annotation.Tag;
                            Canvas.SetLeft(annotation, data.DisplayRect.X - origin.X);
                            Canvas.SetTop(annotation, data.DisplayRect.Y - origin.Y);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ReportError(Properties.Resources.E_DragInControllerDesign, ex);
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// Handle drag drop over annotation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void Annotation_DragDropped(object sender, DragEventArgs e)
        {
            // Don't handle event here
        }

        /// <summary>
        /// Handle drag drop completion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DesignCanvas_Drop(object sender, DragEventArgs e)
        {
            if ((e.Effects & DragDropEffects.Move) != 0 &&
                e.Data.GetDataPresent(typeof(ControlAnnotationControl).FullName))
            {
                try
                {
                    // Get the source data
                    ControlAnnotationControl annotation = (ControlAnnotationControl)e.Data.GetData(typeof(ControlAnnotationControl).FullName);
                    Point dragEndPoint = e.GetPosition(DesignCanvas);
                    Vector difference = dragEndPoint - _dragStartPoint;

                    if (Source != null)
                    {
                        AnnotationGroup group = annotation.Group;
                        if (group != null)
                        {
                            // Move whole group of direction controls (stick / DPad)
                            ControlAnnotationControl centreAnnotation = group.CentreAnnotation;
                            AnnotationData data = (AnnotationData)centreAnnotation.Tag;
                            double newX = Math.Max(Constants.DefaultAnnotationWidth, Math.Min(data.DisplayRect.X + difference.X, Source.Display.Width - 2 * Constants.DefaultAnnotationWidth));
                            double newY = Math.Max(Constants.DefaultAnnotationHeight, Math.Min(data.DisplayRect.Y + difference.Y, Source.Display.Height - 2 * Constants.DefaultAnnotationHeight));
                            difference = new Vector(newX - data.DisplayRect.X, newY - data.DisplayRect.Y);
                            foreach (ControlAnnotationControl member in group.Annotations)
                            {
                                AnnotationData memberData = (AnnotationData)member.Tag;
                                memberData.DisplayRect = new Rect(Point.Add(memberData.DisplayRect.Location, difference), memberData.DisplayRect.Size);
                            }
                        }
                        else
                        {
                            // Move single annotation for button, trigger, menu bar or title bar
                            AnnotationData data = (AnnotationData)annotation.Tag;
                            double newX = Math.Max(0, Math.Min(data.DisplayRect.X + difference.X, Source.Display.Width - data.DisplayRect.Width));
                            double newY = Math.Max(0, Math.Min(data.DisplayRect.Y + difference.Y, Source.Display.Height - data.DisplayRect.Height));
                            data.DisplayRect = new Rect(newX, newY, data.DisplayRect.Width, data.DisplayRect.Height);
                        }

                        // Update UI
                        Source.IsModified = true;
                        _dragInProgress = false;
                        RefreshLayout();

                        // Tell other controls
                        RaiseEvent(new RoutedEventArgs(KxLayoutEditedEvent));
                    }
                }
                catch (Exception ex)
                {
                    ReportError(Properties.Resources.E_PositioningInControllerDesign, ex);
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Open the context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DesignOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContextMenu contextMenu = this.Resources["DesignContextMenu"] as ContextMenu;
                if (contextMenu != null)
                {
                    MenuItem menuItem = (MenuItem)LogicalTreeHelper.FindLogicalNode(contextMenu, "RemoveControlButton");
                    if (menuItem != null)
                    {
                        menuItem.IsEnabled = _currentInputEvent != null;
                    }

                    contextMenu.PlacementTarget = DesignOptionsButton;
                    contextMenu.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_OpenMenuInControllerDesign, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Show or hide context menu items on opening
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            FrameworkElement control = (FrameworkElement)sender;
            if (control.ContextMenu != null)
            {
                MenuItem menuItem = (MenuItem)LogicalTreeHelper.FindLogicalNode(control.ContextMenu, "RemoveControlButton");
                if (menuItem != null)
                {
                    menuItem.IsEnabled = _currentInputEvent != null;
                }
            }
        }

        /// <summary>
        /// Add a virtual POV control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddPOVButton_Click(object sender, RoutedEventArgs e)
        {
            if (Source != null)
            {
                try
                {
                    // Create control
                    int id = Source.DPads.GetFirstUnusedID(Constants.ID1);
                    if (id <= Constants.MaxControls)
                    {
                        string name = Properties.Resources.String_POV + " " + id.ToString();
                        List<AnnotationData> annotationDataList = new List<AnnotationData>();
                        annotationDataList.Add(new AnnotationData(new Point(Constants.DefaultAnnotationWidth, Constants.DefaultAnnotationHeight), new KxControlEventArgs(EVirtualControlType.DPad, id, EControlSetting.None, ELRUDState.Centre)));
                        annotationDataList.Add(new AnnotationData(new Point(0, Constants.DefaultAnnotationHeight), new KxControlEventArgs(EVirtualControlType.DPad, id, EControlSetting.None, ELRUDState.Left)));
                        annotationDataList.Add(new AnnotationData(new Point(0, 0), new KxControlEventArgs(EVirtualControlType.DPad, id, EControlSetting.None, ELRUDState.UpLeft)));
                        annotationDataList.Add(new AnnotationData(new Point(Constants.DefaultAnnotationWidth, 0), new KxControlEventArgs(EVirtualControlType.DPad, id, EControlSetting.None, ELRUDState.Up)));
                        annotationDataList.Add(new AnnotationData(new Point(2 * Constants.DefaultAnnotationWidth, 0), new KxControlEventArgs(EVirtualControlType.DPad, id, EControlSetting.None, ELRUDState.UpRight)));
                        annotationDataList.Add(new AnnotationData(new Point(2 * Constants.DefaultAnnotationWidth, Constants.DefaultAnnotationHeight), new KxControlEventArgs(EVirtualControlType.DPad, id, EControlSetting.None, ELRUDState.Right)));
                        annotationDataList.Add(new AnnotationData(new Point(2 * Constants.DefaultAnnotationWidth, 2 * Constants.DefaultAnnotationHeight), new KxControlEventArgs(EVirtualControlType.DPad, id, EControlSetting.None, ELRUDState.DownRight)));
                        annotationDataList.Add(new AnnotationData(new Point(Constants.DefaultAnnotationWidth, 2 * Constants.DefaultAnnotationHeight), new KxControlEventArgs(EVirtualControlType.DPad, id, EControlSetting.None, ELRUDState.Down)));
                        annotationDataList.Add(new AnnotationData(new Point(0, 2 * Constants.DefaultAnnotationHeight), new KxControlEventArgs(EVirtualControlType.DPad, id, EControlSetting.None, ELRUDState.DownLeft)));
                        ControllerDPad control = new ControllerDPad(id, name, annotationDataList);
                        InputMapping[] inputMappings = new InputMapping[] { new InputMapping(Constants.ID1, EPhysicalControlType.POV, id - 1, EPhysicalControlOption.None) };
                        control.SetInputMappings(inputMappings);
                        Source.AddControl(control);

                        // Add control to UI
                        AnnotationGroup group = CreateAnnotationGroup(ControllerCanvas, annotationDataList);
                        RefreshLayout();
                        RefreshDisplay();

                        // Select control
                        SetCurrentAnnotation(group.CentreAnnotation);

                        // Tell other controls
                        RaiseEvent(new RoutedEventArgs(KxControlsEditedEvent));
                    }
                }
                catch (Exception ex)
                {
                    ReportError(Properties.Resources.E_AddControlInControllerDesign, ex);
                }
            }
            e.Handled = true;
        }

        /// <summary>
        /// Add a virtual stick control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddStickButton_Click(object sender, RoutedEventArgs e)
        {
            if (Source != null)
            {
                try
                {
                    // Create control
                    int id = Source.Sticks.GetFirstUnusedID(Constants.ID1);
                    if (id <= Constants.MaxControls)
                    {
                        string name = Properties.Resources.String_Stick + " " + id.ToString();
                        List<AnnotationData> annotationDataList = new List<AnnotationData>();
                        annotationDataList.Add(new AnnotationData(new Point(Constants.DefaultAnnotationWidth, Constants.DefaultAnnotationHeight), new KxControlEventArgs(EVirtualControlType.Stick, id, EControlSetting.None, ELRUDState.Centre)));
                        annotationDataList.Add(new AnnotationData(new Point(0, Constants.DefaultAnnotationHeight), new KxControlEventArgs(EVirtualControlType.Stick, id, EControlSetting.None, ELRUDState.Left)));
                        annotationDataList.Add(new AnnotationData(new Point(0, 0), new KxControlEventArgs(EVirtualControlType.Stick, id, EControlSetting.None, ELRUDState.UpLeft)));
                        annotationDataList.Add(new AnnotationData(new Point(Constants.DefaultAnnotationWidth, 0), new KxControlEventArgs(EVirtualControlType.Stick, id, EControlSetting.None, ELRUDState.Up)));
                        annotationDataList.Add(new AnnotationData(new Point(2 * Constants.DefaultAnnotationWidth, 0), new KxControlEventArgs(EVirtualControlType.Stick, id, EControlSetting.None, ELRUDState.UpRight)));
                        annotationDataList.Add(new AnnotationData(new Point(2 * Constants.DefaultAnnotationWidth, Constants.DefaultAnnotationHeight), new KxControlEventArgs(EVirtualControlType.Stick, id, EControlSetting.None, ELRUDState.Right)));
                        annotationDataList.Add(new AnnotationData(new Point(2 * Constants.DefaultAnnotationWidth, 2 * Constants.DefaultAnnotationHeight), new KxControlEventArgs(EVirtualControlType.Stick, id, EControlSetting.None, ELRUDState.DownRight)));
                        annotationDataList.Add(new AnnotationData(new Point(Constants.DefaultAnnotationWidth, 2 * Constants.DefaultAnnotationHeight), new KxControlEventArgs(EVirtualControlType.Stick, id, EControlSetting.None, ELRUDState.Down)));
                        annotationDataList.Add(new AnnotationData(new Point(0, 2 * Constants.DefaultAnnotationHeight), new KxControlEventArgs(EVirtualControlType.Stick, id, EControlSetting.None, ELRUDState.DownLeft)));
                        ControllerStick control = new ControllerStick(id, name, annotationDataList);
                        InputMapping[] inputMappings = new InputMapping[] { new InputMapping(Constants.ID1, EPhysicalControlType.Axis, 2 * (id - 1), EPhysicalControlOption.None),
                                                                    new InputMapping(Constants.ID1, EPhysicalControlType.Axis, 2 * (id - 1) + 1, EPhysicalControlOption.None),
                                                                    new InputMapping(Constants.NoneID, EPhysicalControlType.Button, Constants.Index0, EPhysicalControlOption.None) }; // Button not mapped
                        control.SetInputMappings(inputMappings);
                        Source.AddControl(control);

                        // Add control to UI
                        AnnotationGroup group = CreateAnnotationGroup(ControllerCanvas, annotationDataList);
                        RefreshLayout();
                        RefreshDisplay();

                        // Select control
                        SetCurrentAnnotation(group.CentreAnnotation);

                        // Tell other controls
                        RaiseEvent(new RoutedEventArgs(KxControlsEditedEvent));
                    }
                }
                catch (Exception ex)
                {
                    ReportError(Properties.Resources.E_AddControlInControllerDesign, ex);
                }
            }
            e.Handled = true;
        }

        /// <summary>
        /// Add a virtual slider control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddSliderButton_Click(object sender, RoutedEventArgs e)
        {
            if (Source != null)
            {
                try
                {
                    // Create control
                    int id = Source.Triggers.GetFirstUnusedID(Constants.ID1);
                    if (id <= Constants.MaxControls)
                    {
                        string name = Properties.Resources.String_Slider + " " + id.ToString();
                        Point displayPos = new Point(0, 0);
                        AnnotationData annotationData = new AnnotationData(displayPos, new KxControlEventArgs(EVirtualControlType.Trigger, id, EControlSetting.None, ELRUDState.None));
                        ControllerTrigger control = new ControllerTrigger(id, name, annotationData);
                        InputMapping[] inputMappings = new InputMapping[] { new InputMapping(Constants.ID1, EPhysicalControlType.Slider, id - 1, EPhysicalControlOption.None) };
                        control.SetInputMappings(inputMappings);
                        Source.AddControl(control);

                        // Add control to UI
                        ControlAnnotationControl annotation = CreateAnnotation(ControllerCanvas, annotationData);
                        RefreshLayout();
                        RefreshDisplay();

                        // Select control
                        SetCurrentAnnotation(annotation);

                        // Tell other controls
                        RaiseEvent(new RoutedEventArgs(KxControlsEditedEvent));
                    }
                }
                catch (Exception ex)
                {
                    ReportError(Properties.Resources.E_AddControlInControllerDesign, ex);
                }
            }
            e.Handled = true;
        }

        /// <summary>
        /// Add a virtual button control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButtonButton_Click(object sender, RoutedEventArgs e)
        {
            if (Source != null)
            {
                try
                {
                    // Create control
                    int id = Source.Buttons.GetFirstUnusedID(Constants.ID1);
                    if (id <= Constants.MaxControls)
                    {
                        string name = Properties.Resources.String_Button + " " + id.ToString();
                        Point displayPos = new Point(0, 0);
                        AnnotationData annotationData = new AnnotationData(displayPos, new KxControlEventArgs(EVirtualControlType.Button, id, EControlSetting.None, ELRUDState.None));
                        ControllerButton control = new ControllerButton(id, name, annotationData);
                        InputMapping[] inputMappings = new InputMapping[] { new InputMapping(Constants.ID1, EPhysicalControlType.Button, id - 1, EPhysicalControlOption.None) };
                        control.SetInputMappings(inputMappings);
                        Source.AddControl(control);

                        // Add control to UI
                        ControlAnnotationControl annotation = CreateAnnotation(ControllerCanvas, annotationData);
                        RefreshLayout();
                        RefreshDisplay();

                        // Select control
                        SetCurrentAnnotation(annotation);

                        // Tell other controls
                        RaiseEvent(new RoutedEventArgs(KxControlsEditedEvent));
                    }
                }
                catch (Exception ex)
                {
                    ReportError(Properties.Resources.E_AddControlInControllerDesign, ex);
                }
            }
            e.Handled = true;
        }

        /// <summary>
        /// Handle delete command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                DeleteSelectedControl();
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_RemoveControlFromControllerDesign, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// See if delete can execute
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            bool canDelete = false;
            if (IsDesignMode)
            {
                ControlAnnotationControl annotation = SelectedAnnotation;
                if (annotation != null)
                {
                    AnnotationData data = (AnnotationData)annotation.Tag;
                    if (data.InputBinding != null)
                    {
                        canDelete = true;
                    }
                }
            }
            e.CanExecute = canDelete;
            e.Handled = true;
        }

        /// <summary>
        /// Remove a virtual control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveControlButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteSelectedControl();
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_RemoveControlFromControllerDesign, ex);
            }
            e.Handled = true;
        }

        /// <summary>
        /// Delete the selected virtual control
        /// </summary>
        private void DeleteSelectedControl()
        {
            ControlAnnotationControl annotation = SelectedAnnotation;
            if (annotation != null)
            {
                AnnotationData data = (AnnotationData)annotation.Tag;
                if (data.InputBinding != null)
                {
                    KxControlEventArgs ev = data.InputBinding;                    
                    BaseControl virtualControl = Source.GetVirtualControl(ev);
                    if (virtualControl != null)
                    {
                        // Confirm if required
                        bool confirmed = _appConfig.GetBoolVal(Constants.ConfigAutoConfirmDeleteVirtualControl, Constants.DefaultAutoConfirmDeleteVirtualControl);
                        if (!confirmed)
                        {
                            CustomMessageBox messageBox = new CustomMessageBox((Window)_editorWindow, Properties.Resources.Q_DeleteControlMessage, Properties.Resources.Q_DeleteControl, MessageBoxButton.OKCancel, true, true);
                            if (messageBox.ShowDialog() == true)
                            {
                                confirmed = true;
                                if (messageBox.DontAskAgain)
                                {
                                    _appConfig.SetBoolVal(Constants.ConfigAutoConfirmDeleteVirtualControl, true);
                                }
                            }
                        }
                                  
                        if (confirmed)
                        {
                            // Remove from UI
                            if (annotation.Group != null)
                            {
                                // Remove group
                                RemoveAnnotationGroup(ControllerCanvas, annotation.Group);
                            }
                            else
                            {
                                // Remove single annotation
                                RemoveAnnotation(ControllerCanvas, annotation);
                            }

                            // Remove from source
                            Source.RemoveControl(virtualControl);                            

                            // Update UI
                            RefreshLayout();
                            SetCurrentAnnotation(null);

                            // Tell other controls
                            RaiseEvent(new RoutedEventArgs(KxControlsEditedEvent));
                        }
                    }                    
                }
            }
        }

        /// <summary>
        /// Show the background editor window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditBackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(KxEditBackgroundEvent));
            
            e.Handled = true;
        }

        /// <summary>
        /// Import virtual controller design from profile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            ImportWindow dialog = new ImportWindow(Source);
            dialog.ItemName = "controller design";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Get the profile to import from
                    Profile fromProfile;
                    if (dialog.IsFromFile)
                    {
                        fromProfile = new Profile();
                        fromProfile.FromFile(dialog.FilePath);
                    }
                    else
                    {
                        fromProfile = Source.Profile;
                    }

                    int fromSourceID = dialog.FromSourceID;
                    BaseSource fromSource = fromProfile.GetSource(fromSourceID);
                    if (fromSource != null)
                    {
                        // Import controller design
                        Source.CopyControllerDesign(fromSource);

                        // Recreate the layout
                        InitialiseDisplay();
                        RefreshDisplay(); 
                        
                        // Tell other controls
                        RaiseEvent(new RoutedEventArgs(KxControlsEditedEvent));
                    }
                    else
                    {
                        string message = string.Format(Properties.Resources.E_NoDesignFound, fromProfile.Name, fromSourceID);
                        ReportError(message, null);
                    }
                }
                catch (Exception ex)
                {
                    ReportError(Properties.Resources.E_ImportControlSets, ex);
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle updates to the controller background
        /// </summary>
        public override void RefreshLayout()
        {
            if (_isLoaded && Source != null)
            {
                DesignCanvas.Width = Source.Display.Width;
                DesignCanvas.Height = Source.Display.Height; 
                PositionControllerCanvas(Source);
                CreateBackground(ControllerCanvas, Source);
                PositionAnnotations(Source);
            }
        }

        /// <summary>
        /// Set a description of the input mappings for a virtual control
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        private void BindAnnotation(ControlAnnotationControl annotation)
        {
            AnnotationData data = (AnnotationData)annotation.Tag;
            KxControlEventArgs inputControl = data.InputBinding;
            if (inputControl != null)
            {
                string displayText = "";
                string toolTip = null;
                //EAnnotationImage icon = EAnnotationImage.None;

                // Decide which mapping to use, if any
                int mappingIndex = 0;
                if (inputControl.ControlType == EVirtualControlType.Stick)
                {
                    switch (inputControl.LRUDState)
                    {
                        case ELRUDState.Left:
                        case ELRUDState.Right:
                            mappingIndex = 0; break;
                        case ELRUDState.Up:
                        case ELRUDState.Down:
                            mappingIndex = 1; break;
                        case ELRUDState.Centre:
                            mappingIndex = 2; break;
                        default:
                            mappingIndex = -1; break;
                    }
                }
                else if (inputControl.ControlType == EVirtualControlType.DPad)
                {
                    if (inputControl.LRUDState != ELRUDState.Centre)
                    {
                        mappingIndex = -1;
                    }
                }

                if (mappingIndex != -1)
                {
                    BaseControl control = Source.GetVirtualControl(inputControl);
                    if (control != null)
                    {
                        InputMapping mapping = control.GetInputMapping(mappingIndex);
                        if (mapping != null)
                        {
                            PhysicalControl physicalControl = null;
                            PhysicalInput input = (PhysicalInput)Source.PhysicalInputs.GetItemByID(mapping.InputID);
                            if (input != null)
                            {
                                physicalControl = (PhysicalControl)input.GetControl(mapping.ControlType, mapping.ControlIndex);
                                if (physicalControl != null)
                                {
                                    displayText = physicalControl.ShortName;
                                    switch (mapping.Options)
                                    {
                                        case EPhysicalControlOption.Inverted:
                                            displayText += "^"; break;
                                        case EPhysicalControlOption.PositiveSide:
                                            displayText += "+"; break;
                                        case EPhysicalControlOption.NegativeSide:
                                            displayText += "-"; break;
                                    }
                                }
                            }

                            if (physicalControl == null)
                            {
                                // Not mapped
                                displayText = "---";
                            }
                        }
                    }                   

                    toolTip = control.Name;
                    if (IsDesignMode)
                    {
                        toolTip += Environment.NewLine + Properties.Resources.String_ClickOrDragToolTip;
                    }
                }

                annotation.CurrentText = displayText;
                annotation.ToolTip = toolTip;
                //annotation.IconRef = icon;
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
