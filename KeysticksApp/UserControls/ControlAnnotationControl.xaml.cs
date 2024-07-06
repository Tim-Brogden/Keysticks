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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Keysticks.Core;
using Keysticks.UI;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for displaying a virtual controller control (or control direction)
    /// </summary>
    public partial class ControlAnnotationControl : UserControl
    {
        // Fields
        private bool _isLoaded;
        private string _defaultText;
        private bool _isDesignMode;
        private string _currentText = "";
        private EAnnotationImage _currentIcon = EAnnotationImage.None;
        private AnnotationGroup _annotationGroup;
        //private bool _autoFontSize = true;
        private bool _dragStarting = false;
        private Point _dragStartPoint;
        private SolidColorBrush _defaultBrush;

        // Dependency properties
        public static readonly DependencyProperty IsDesignModeProperty =
             DependencyProperty.Register("IsDesignMode", typeof(bool),
             typeof(ControlAnnotationControl), new FrameworkPropertyMetadata(false, DesignModeChanged));
        public static readonly DependencyProperty IsSelectedProperty =
             DependencyProperty.Register("IsSelected", typeof(bool),
             typeof(ControlAnnotationControl), new FrameworkPropertyMetadata(false, BackgroundChanged));
        public static readonly DependencyProperty IsHighlightedProperty =
             DependencyProperty.Register("IsHighlighted", typeof(bool),
             typeof(ControlAnnotationControl), new FrameworkPropertyMetadata(false, BackgroundChanged));
        public static readonly DependencyProperty HighlightBrushProperty =
             DependencyProperty.Register("HighlightBrush", typeof(Brush),
             typeof(ControlAnnotationControl), new FrameworkPropertyMetadata(new SolidColorBrush(Constants.DefaultHighlightColour)));
        public static readonly DependencyProperty SelectionBrushProperty =
             DependencyProperty.Register("SelectionBrush", typeof(Brush),
             typeof(ControlAnnotationControl), new FrameworkPropertyMetadata(new SolidColorBrush(Constants.DefaultSelectionColour)));
        public static readonly DependencyProperty CurrentTextProperty =
             DependencyProperty.Register("CurrentText", typeof(string),
             typeof(ControlAnnotationControl), new FrameworkPropertyMetadata("", CurrentTextChanged));

        // Dependency property wrappers
        public bool IsDesignMode
        {
            get { return (bool)GetValue(IsDesignModeProperty); }
            set { SetValue(IsDesignModeProperty, value); }
        }
        public bool IsHighlighted
        {
            get { return (bool)GetValue(IsHighlightedProperty); }
            set { SetValue(IsHighlightedProperty, value); }
        }
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public Brush HighlightBrush
        {
            get { return (Brush)GetValue(HighlightBrushProperty); }
            set { SetValue(HighlightBrushProperty, value); }
        }
        public Brush SelectionBrush
        {
            get { return (Brush)GetValue(SelectionBrushProperty); }
            set { SetValue(SelectionBrushProperty, value); }
        }
        public string CurrentText
        {
            get { return (string)GetValue(CurrentTextProperty); }
            set { SetValue(CurrentTextProperty, value); }
        }
        
        // Properties
        public string DefaultText 
        { 
            get { return _defaultText; } 
            set 
            {
                if (_defaultText != value)
                {
                    _defaultText = value;
                    UpdateTextOrImage();
                }
            } 
        }
        public EAnnotationImage IconRef 
        { 
            get { return _currentIcon; }
            set
            {
                if (_currentIcon != value)
                {
                    _currentIcon = value;
                    UpdateTextOrImage();
                }
            }
        }
        public AnnotationGroup Group { get { return _annotationGroup; } set { _annotationGroup = value; } }

        // Events
        public event MouseEventHandler DragStarted;
        public event DragEventHandler DragDropped;
        public event RoutedEventHandler AnnotationClicked
        {
            add { AddHandler(KxAnnotationClickedEvent, value); }
            remove { RemoveHandler(KxAnnotationClickedEvent, value); }
        }
        public event RoutedEventHandler AnnotationRightClicked
        {
            add { AddHandler(KxAnnotationRightClickedEvent, value); }
            remove { RemoveHandler(KxAnnotationRightClickedEvent, value); }
        }

        public static readonly RoutedEvent KxAnnotationClickedEvent = EventManager.RegisterRoutedEvent(
            "AnnotationClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ControlAnnotationControl));
        public static readonly RoutedEvent KxAnnotationRightClickedEvent = EventManager.RegisterRoutedEvent(
            "AnnotationRightClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ControlAnnotationControl));

        /// <summary>
        /// Constructor
        /// </summary>
        public ControlAnnotationControl()
            : base()
        {
            InitializeComponent();

            _defaultBrush = new SolidColorBrush(Colors.White);
        }

        /// <summary>
        /// Set whether design mode or not
        /// </summary>
        /// <param name="isDesignMode"></param>
        public void SetIsDesignMode(bool isDesignMode)
        {
            // Make sure value has changed
            if (isDesignMode != _isDesignMode)
            {
                _isDesignMode = isDesignMode;
                if (isDesignMode)
                {
                    CustomWindowBorder.Cursor = Cursors.Hand;
                    this.PreviewMouseLeftButtonDown += this.ControlAnnotationControl_PreviewMouseLeftButtonDown;
                    this.PreviewMouseMove += ControlAnnotationControl_PreviewMouseMove;
                    this.Drop += ControlAnnotationControl_Drop;
                    this.AllowDrop = true;
                }
                else
                {
                    CustomWindowBorder.Cursor = Cursors.Arrow;
                    this.PreviewMouseLeftButtonDown -= this.ControlAnnotationControl_PreviewMouseLeftButtonDown;
                    this.PreviewMouseMove -= ControlAnnotationControl_PreviewMouseMove;
                    this.Drop -= ControlAnnotationControl_Drop;
                    this.AllowDrop = false;
                }
            }
        }

        /// <summary>
        /// Set new text
        /// </summary>
        /// <param name="currentText"></param>
        public void SetCurrentText(string currentText)
        {
            _currentText = currentText;
            UpdateTextOrImage();
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;
            UpdateTextOrImage();
        }

        /// <summary>
        /// Record mouse position for drag drop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlAnnotationControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DragStarted != null)
            {
                _dragStartPoint = e.GetPosition(null);
                _dragStarting = true;
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle user starting to drag from control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlAnnotationControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (DragStarted != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed && _dragStarting)
                {
                    // Get the current mouse position
                    Point mousePos = e.GetPosition(null);
                    Vector diff = _dragStartPoint - mousePos;

                    if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                    {
                        _dragStarting = false;
                        DragStarted(this, e);
                    }
                }
                else
                {
                    _dragStarting = false;
                }
            }

            e.Handled = true;
        }
        
        /// <summary>
        /// Drag dropped over annotation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlAnnotationControl_Drop(object sender, DragEventArgs e)
        {
            if (DragDropped != null)
            {
                DragDropped(this, e);
            }
        }

        /// <summary>
        /// Design mode changed
        /// </summary>
        private static void DesignModeChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ControlAnnotationControl annotation = source as ControlAnnotationControl;
            annotation.SetIsDesignMode((bool)e.NewValue);
        }

        /// <summary>
        /// Background changed
        /// </summary>
        private static void BackgroundChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ControlAnnotationControl annotation = source as ControlAnnotationControl;
            annotation.UpdateBackground();
        }

        /// <summary>
        /// Text changed
        /// </summary>
        private static void CurrentTextChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ControlAnnotationControl annotation = source as ControlAnnotationControl;
            annotation.SetCurrentText((string)e.NewValue);
        }

        /// <summary>
        /// Display text or image
        /// </summary>
        private void UpdateTextOrImage()
        {
            if (_isLoaded)
            {
                string caption = _currentText != null ? _currentText : _defaultText;
                if (!string.IsNullOrEmpty(caption))
                {
                    CaptionTextBlock.Text = caption;
                    CaptionImage.Source = null;                    
                }
                else
                {
                    CaptionTextBlock.Text = "";
                    CaptionImage.Source = GUIUtils.FindIcon(_currentIcon);
                }
            }
        }

        /// <summary>
        /// Update background brush
        /// </summary>
        public void UpdateBackground()
        {
            if (IsHighlighted)
            {
                this.CustomWindowBorder.Background = HighlightBrush;
            }
            else if (IsSelected)
            {
                this.CustomWindowBorder.Background = SelectionBrush;
            }
            else
            {
                this.CustomWindowBorder.Background = _defaultBrush;
            }            
        }

        /// <summary>
        /// Handle click on annotation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomWindowBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(KxAnnotationClickedEvent));
            
            // Don't mark event as handled
        }

        /// <summary>
        /// Handle right-click on annotation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomWindowBorder_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(KxAnnotationRightClickedEvent));

            // Don't mark event as handled
        }

    }
}
