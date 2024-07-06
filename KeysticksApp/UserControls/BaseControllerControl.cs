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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Keysticks.Config;
using Keysticks.Controls;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Sources;
using Keysticks.UI;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Base class for controls which represent a virtual controller layout
    /// </summary>
    public class BaseControllerControl : KxUserControl
    {
        // Fields
        private BaseSource _source;
        private ControlAnnotationControl _selectedAnnotation;
        private ColourScheme _colourScheme;
        private DisplayData _defaultDisplayData;
        private bool _isCompactView = Constants.DefaultCompactControlsWindow;
        private double _opacity = Constants.DefaultWindowOpacityPercent * 0.01;
        private Brush _backgroundBrush = Brushes.Transparent;
        private SolidColorBrush _defaultAnnotationBackground = Brushes.White;
        private SolidColorBrush _highlightBrush;
        private SolidColorBrush _selectionBrush;
        private List<AnnotationGroup> _directionalControlAnnotations = new List<AnnotationGroup>();
        private List<ControlAnnotationControl> _annotationsList = new List<ControlAnnotationControl>();
        private Dictionary<int, ControlAnnotationControl> _annotationsIndex = new Dictionary<int, ControlAnnotationControl>();

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
            typeof(BaseControllerControl),
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
            typeof(BaseControllerControl),
            new FrameworkPropertyMetadata(false)
        );

        // Properties
        protected BaseSource Source { get { return _source; } }
        protected ControlAnnotationControl SelectedAnnotation { get { return _selectedAnnotation; } }
        protected List<AnnotationGroup> DirectionalControlAnnotations { get { return _directionalControlAnnotations; } }
        protected List<ControlAnnotationControl> AnnotationsList { get { return _annotationsList; } }
        public bool IsCompactView { get { return _isCompactView; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseControllerControl()
            :base()
        {            
            // Brushes
            _highlightBrush = new SolidColorBrush(Constants.DefaultHighlightColour);
            _selectionBrush = new SolidColorBrush(Constants.DefaultSelectionColour);
            _defaultDisplayData = new DisplayData();
        }       

        /// <summary>
        /// Set the config
        /// </summary>
        /// <param name="appConfig"></param>
        public virtual void SetAppConfig(AppConfig appConfig)
        {
            _colourScheme = appConfig.ColourScheme;
            _defaultDisplayData.FromConfig(appConfig);
            _isCompactView = appConfig.GetBoolVal(Constants.ConfigCompactControlsWindow, Constants.DefaultCompactControlsWindow);

            UpdateBrushes();
            RefreshLayout();
        }

        /// <summary>
        /// Set the opacity of the background
        /// </summary>
        /// <param name="opacity"></param>
        public void SetOpacity(double opacity)
        {
            _opacity = opacity;
        }

        /// <summary>
        /// Set the source
        /// </summary>
        /// <param name="profile"></param>
        public virtual void SetSource(BaseSource source)
        {
            _source = source;
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

                if (!_backgroundBrush.IsFrozen)
                {
                    _backgroundBrush.Opacity = _colourScheme.CurrentControlsOpacity;
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
        /// Highlight or unhighlight the annotation for an event
        /// </summary>
        /// <param name="inputEvent"></param>
        public void AnimateInputEvent(KxSourceEventArgs inputEvent)
        {
            ControlAnnotationControl annotation = FindAnnotation(inputEvent);
            if (annotation != null)
            {
                switch (inputEvent.EventReason)
                {
                    case EEventReason.Pressed:
                        annotation.IsHighlighted = true;
                        break;
                    case EEventReason.Directed:
                        if (inputEvent.LRUDState != ELRUDState.Centre)
                        {
                            annotation.IsHighlighted = true;
                        }
                        break;
                    case EEventReason.Released:
                        annotation.IsHighlighted = false;
                        break;
                    case EEventReason.Undirected:
                        if (inputEvent.LRUDState != ELRUDState.Centre)
                        {
                            annotation.IsHighlighted = false;
                        }
                        break;
                    case EEventReason.Moved:
                        if (inputEvent.Param0 != null && inputEvent.Param0.DataType == EDataType.Bool)
                        {
                            // Param 0 indicates whether moving or not
                            annotation.IsHighlighted = (bool)inputEvent.Param0.Value;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Refresh all animations to show which controls are pressed / directed
        /// </summary>
        public void RefreshAnimations()
        {            
            // Non-directional controls
            foreach (ControlAnnotationControl annotation in _annotationsList)
            {
                AnnotationData annotationData = (AnnotationData)annotation.Tag;
                if (annotationData.InputBinding != null)
                {
                    KxControlEventArgs inputControl = annotationData.InputBinding;
                    if (inputControl.LRUDState == ELRUDState.None)
                    {
                        // Trigger or button
                        BaseControl control = _source.GetVirtualControl(inputControl);
                        if (control != null)
                        {
                            annotation.IsHighlighted = control.IsPressed;
                        }
                    }
                }
            }

            // Directional controls
            foreach (AnnotationGroup group in _directionalControlAnnotations)
            {
                AnnotationData centreData = (AnnotationData)group.CentreAnnotation.Tag;
                if (centreData.InputBinding != null)
                {
                    KxControlEventArgs inputControl = centreData.InputBinding;                    
                    DirectionalControl control = _source.GetVirtualControl(inputControl) as DirectionalControl;
                    if (control != null)
                    {
                        // Loop over annotations in group excluding centre
                        foreach (ControlAnnotationControl groupAnnotation in group.Annotations)
                        {
                            AnnotationData data = (AnnotationData)groupAnnotation.Tag;
                            if (data.InputBinding.LRUDState != ELRUDState.Centre)
                            {
                                // Animate direction
                                groupAnnotation.IsHighlighted = (data.InputBinding.LRUDState == control.CurrentDirection);
                            }
                            else
                            {
                                // Animate button press
                                groupAnnotation.IsHighlighted = control.IsPressed;
                            }
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
            foreach (ControlAnnotationControl annotation in _annotationsList)
            {
                annotation.IsHighlighted = false;
            }
        }

        /// <summary>
        /// Initialise the display
        /// </summary>
        public virtual void InitialiseDisplay()
        {
        }

        /// <summary>
        /// Create the controller background and add the annotations
        /// </summary>
        protected virtual void CreateDisplay(Canvas canvas, BaseSource virtualController, bool addTitleAndMenu)
        {
            _directionalControlAnnotations.Clear();
            _annotationsList.Clear();
            _annotationsIndex.Clear();
            canvas.Children.Clear();

            if (virtualController != null)
            {
                CreateBackground(canvas, virtualController);
                CreateAnnotations(canvas, virtualController, addTitleAndMenu);
                PositionAnnotations(virtualController);
            }
        }

        /// <summary>
        /// Refresh the annotations
        /// </summary>
        public virtual void RefreshDisplay()
        {
            // Override in derived class
        }

        /// <summary>
        /// Refresh the background
        /// </summary>
        public virtual void RefreshLayout()
        {
            // Override in derived class
        }

        /// <summary>
        /// Handle selection of annotation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Annotation_Clicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Handle selection of annotation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Annotation_RightClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Start drag drop if required
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Annotation_DragStarted(object sender, MouseEventArgs e)
        {            
            e.Handled = true;
        }

        /// <summary>
        /// Handle drag drop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Annotation_DragDropped(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Find the annotation for the current situation and input event, if any
        /// </summary>
        protected ControlAnnotationControl FindAnnotation(KxControlEventArgs inputEvent)
        {
            ControlAnnotationControl annotation = null;
            if (inputEvent != null)
            {
                int id = inputEvent.ToID();
                if (_annotationsIndex.ContainsKey(id))
                {
                    annotation = _annotationsIndex[id];

                    // Don't return it if it's hidden
                    if (annotation.Visibility != System.Windows.Visibility.Visible)
                    {
                        annotation = null;
                    }
                }
            }

            return annotation;
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
        /// Set the background brush
        /// </summary>
        protected void CreateBackground(Canvas canvas, BaseSource virtualController)
        {
            Rect displayRegion;

            DisplayData display = virtualController.Display;
            if (_isCompactView)
            {
                displayRegion = virtualController.GetBoundingRect();
            }
            else
            {
                displayRegion = new Rect(0.0, 0.0, display.Width, display.Height);
            }

            // Use the default background display settings from the app config if required
            DisplayData backgroundDisplay = display.BackgroundType == EBackgroundType.Default ? _defaultDisplayData : display;

            // Load portion of bitmap
            if (backgroundDisplay.BackgroundType == EBackgroundType.Colour)
            {
                // Coloured background
                Color colour = ColourUtils.ColorFromString(backgroundDisplay.BackgroundColour, Colors.LightGray);
                Color lighterColour = Color.Multiply(colour, 1.7f);
                Color darkerColour = Color.Multiply(colour, 0.6f);

                _backgroundBrush = new LinearGradientBrush(lighterColour, darkerColour, 45.0);
                _backgroundBrush.Opacity = _opacity;
            }
            else
            {
                BitmapImage image = null;
                Uri uri;
                if (backgroundDisplay.BackgroundType == EBackgroundType.Image &&
                    !string.IsNullOrWhiteSpace(backgroundDisplay.BackgroundImageUri))
                {
                    // Custom image

                    // Prepend Profiles folder if not present
                    string filePath = backgroundDisplay.BackgroundImageUri;
                    if (!filePath.Contains("\\") && !filePath.Contains("/"))
                    {
                        filePath = System.IO.Path.Combine(AppConfig.LocalAppDataDir, "Profiles", filePath);
                    }

                    uri = new Uri(filePath);
                    try
                    {
                        image = new BitmapImage(uri);
                    }
                    catch (Exception)
                    {
                        image = null;
                    }
                }
                
                if (image == null)
                {
                    try
                    {
                        // Default image
                        uri = new Uri(Constants.DefaultControllerBackgroundUri);
                        image = new BitmapImage(uri);
                    }
                    catch (Exception)
                    {
                        image = null;
                    }
                }

                if (image != null)
                {
                    // Create image brush
                    ImageBrush imageBrush = new ImageBrush(image);
                    if (_isCompactView)
                    {
                        // Just display the image portion corresponding to the bounding rectangle
                        double x = displayRegion.X / display.Width;
                        double y = displayRegion.Y / display.Height;
                        double width = displayRegion.Width / display.Width;
                        double height = displayRegion.Height / display.Height;
                        imageBrush.Viewbox = new Rect(x, y, width, height);
                    }
                    imageBrush.Stretch = Stretch.UniformToFill;
                    imageBrush.AlignmentX = AlignmentX.Left;
                    imageBrush.AlignmentY = AlignmentY.Top;
                    imageBrush.Opacity = _opacity;
                    _backgroundBrush = imageBrush;
                }
            }

            // Set the canvas size
            canvas.Width = displayRegion.Width;
            canvas.Height = displayRegion.Height;

            // Remove existing polygon if reqd
            if (canvas.Children.Count != 0 &&
                canvas.Children[0] is Polygon)
            {
                canvas.Children.RemoveAt(0);
            }

            if (_isCompactView)
            {
                // Create polygon
                List<Point> vertices = virtualController.GetBoundingPolygon(displayRegion.TopLeft);
                Polygon polygon = new Polygon();
                polygon.Fill = _backgroundBrush;
                polygon.Points = new PointCollection(vertices);

                // Update canvas
                canvas.Background = Brushes.Transparent;
                canvas.Children.Insert(0, polygon);
            }
            else
            {
                canvas.Background = _backgroundBrush;
            }
        }

        /// <summary>
        /// Create the annotations to superimpose over the controller
        /// </summary>
        /// <param name="virtualController"></param>
        private void CreateAnnotations(Canvas canvas, BaseSource virtualController, bool addTitleAndMenu)
        {
            foreach (ControllerButton control in virtualController.Buttons)
            {
                CreateAnnotation(canvas, control.AnnotationData);
            }
            foreach (ControllerTrigger control in virtualController.Triggers)
            {
                CreateAnnotation(canvas, control.AnnotationData);
            }
            foreach (ControllerStick control in virtualController.Sticks)
            {
                CreateAnnotationGroup(canvas, control.AnnotationDataList);
            }
            foreach (ControllerDPad control in virtualController.DPads)
            {
                CreateAnnotationGroup(canvas, control.AnnotationDataList);
            }
            if (addTitleAndMenu)
            {
                DisplayData display = virtualController.Display;
                ControlAnnotationControl annotation = CreateAnnotation(canvas, display.TitleBarData);
                annotation.CurrentText = Properties.Resources.String_TitleBar;
                if (IsDesignMode)
                {
                    annotation.ToolTip = Properties.Resources.String_ClickOrDragToolTip;
                }
                annotation = CreateAnnotation(canvas, display.MenuBarData);
                annotation.CurrentText = Properties.Resources.String_MenuBar;
                if (IsDesignMode)
                {
                    annotation.ToolTip = Properties.Resources.String_ClickOrDragToolTip;
                }
            }
        }

        /// <summary>
        /// Create a group of annotations for a directional control
        /// </summary>
        /// <param name="virtualController"></param>
        /// <param name="controlGroup"></param>
        protected AnnotationGroup CreateAnnotationGroup(Canvas canvas, List<AnnotationData> annotationDataList)
        {
            List<ControlAnnotationControl> annotations = new List<ControlAnnotationControl>();
            ControlAnnotationControl centreAnnotation = null;
            foreach (AnnotationData annotationData in annotationDataList)
            {
                ControlAnnotationControl annotation = CreateAnnotation(canvas, annotationData);
                annotations.Add(annotation);

                KxControlEventArgs ev = annotationData.InputBinding;
                if (ev != null && ev.LRUDState == ELRUDState.Centre)
                {
                    centreAnnotation = annotation;
                }
            }

            // Add to directional annotations list
            AnnotationGroup annotationGroup = new AnnotationGroup(annotations, centreAnnotation);
            _directionalControlAnnotations.Add(annotationGroup);

            return annotationGroup;
        }

        /// <summary>
        /// Create a single annotation
        /// </summary>
        /// <param name="virtualController"></param>
        /// <param name="annotationData"></param>
        /// <returns></returns>
        protected ControlAnnotationControl CreateAnnotation(Canvas canvas, AnnotationData annotationData)
        {
            ControlAnnotationControl annotation = new ControlAnnotationControl();
            //annotation.Name = control.ControlName;
            annotation.Visibility = (IsDesignMode || ShowUnusedControls) ? Visibility.Visible : Visibility.Hidden;
            annotation.Width = annotationData.DisplayRect.Width;
            annotation.Height = annotationData.DisplayRect.Height;
            annotation.Tag = annotationData;
            annotation.IsDesignMode = IsDesignMode;
            annotation.SelectionBrush = _selectionBrush;
            annotation.HighlightBrush = _highlightBrush;
            annotation.AnnotationClicked += this.Annotation_Clicked;
            annotation.AnnotationRightClicked += this.Annotation_RightClicked;
            if (IsDesignMode)
            {
                annotation.DragStarted += this.Annotation_DragStarted;
                annotation.DragDropped += this.Annotation_DragDropped;
            }

            // Add to list
            _annotationsList.Add(annotation);

            // Add to annotations index if reqd
            if (annotationData.InputBinding != null)
            {
                _annotationsIndex[annotationData.InputBinding.ToID()] = annotation;
            }

            // Add to UI
            canvas.Children.Add(annotation);

            return annotation;
        }

        protected void RemoveAnnotationGroup(Canvas canvas, AnnotationGroup group)
        {
            // Remove from directional controls
            _directionalControlAnnotations.Remove(group);

            // Remove individual annotations
            foreach (ControlAnnotationControl annotation in group.Annotations)
            {
                RemoveAnnotation(canvas, annotation);
            }
        }

        /// <summary>
        /// Remove an annotation
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="annotation"></param>
        protected void RemoveAnnotation(Canvas canvas, ControlAnnotationControl annotation)
        {
            // Deselect
            if (annotation == _selectedAnnotation)
            {
                SelectAnnotation(null);
            }

            // Remove from UI
            canvas.Children.Remove(annotation);

            // Remove from lists
            _annotationsList.Remove(annotation);
            AnnotationData annotationData = (AnnotationData)annotation.Tag;
            if (annotationData.InputBinding != null)
            {
                _annotationsIndex.Remove(annotationData.InputBinding.ToID());
            }
        }

        /// <summary>
        /// Position the annotations
        /// </summary>
        /// <param name="origin"></param>
        protected void PositionAnnotations(BaseSource virtualController)
        {
            Point origin = new Point();
            if (_isCompactView)
            {
                origin = virtualController.GetBoundingRect().TopLeft;
            }

            foreach (ControlAnnotationControl annotation in _annotationsList)
            {
                AnnotationData annotationData = (AnnotationData)annotation.Tag;

                Canvas.SetLeft(annotation, annotationData.DisplayRect.X - origin.X);
                Canvas.SetTop(annotation, annotationData.DisplayRect.Y - origin.Y);
            }
        }
    }
}
