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
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Sources;
using System.Windows.Threading;

namespace Keysticks
{
    /// <summary>
    /// Keyboard grid window
    /// </summary>
    public partial class GridsWindow : Window, IViewerWindow
    {
        // Fields
        private IMainWindow _parentWindow;
        private EStandardWindow _windowRef = EStandardWindow.Grids;
        private AppConfig _appConfig;
        private BaseSource _source;
        private StringUtils _utils = new StringUtils();
        private bool _showTitleBar = Constants.DefaultShowInteractiveControlsTitleBar;
        private bool _showFooter = Constants.DefaultShowInteractiveControlsFooter;
        private bool _animateControls = Constants.DefaultAnimateControls;
        private bool _allowSuggestions = false;
        private bool _showSuggestions = false;
        private DoubleValItem _zoomFactor = new DoubleValItem(1.0);
        private bool _isLoaded = false;
        private int _currentModeID = Constants.NoneID;
        private int _currentPageID = Constants.NoneID;
        private EGridType _currentGridType = EGridType.None;
        private List<IActionViewerControl> _actionViewerControls;
        private IActionViewerControl _visibleGrid = null;

        // Properties
        public BaseSource Source { get { return _source; } }
        public DoubleValItem ZoomFactor { get { return _zoomFactor; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public GridsWindow(IMainWindow parentWindow)
        {
            try
            {
                InitializeComponent();

                _actionViewerControls = new List<IActionViewerControl> { KeyboardViewer,
                                                                        ActionStripViewer,
                                                                        SquareGridViewer };
                _parentWindow = parentWindow;
                SuggestionsControl.SetMainWindow(parentWindow);

                this.DataContext = this;
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_CreateKeyboardWindow, ex);
            }
        }

        /// <summary>
        /// Set new configuration options
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            try
            {
                // Reset
                _currentModeID = Constants.NoneID;
                _currentPageID = Constants.NoneID;

                _appConfig = appConfig;

                // Opacity
                double interactiveControlsOpacity = 0.01 * appConfig.GetIntVal(Constants.ConfigInteractiveControlsOpacityPercent, Constants.DefaultWindowOpacityPercent);
                LinearGradientBrush brush = (LinearGradientBrush)this.FindResource("TitleBarBrush");
                if (brush != null && !brush.IsFrozen)
                {
                    brush.Opacity = interactiveControlsOpacity;
                }

                // Show or hide title bar
                _showTitleBar = appConfig.GetBoolVal(Constants.ConfigShowInteractiveControlsTitleBar, Constants.DefaultShowInteractiveControlsTitleBar);
                _showFooter = appConfig.GetBoolVal(Constants.ConfigShowInteractiveControlsFooter, Constants.DefaultShowInteractiveControlsFooter);
                if (_isLoaded)
                {
                    this.TitleBar.Visibility = _showTitleBar ? Visibility.Visible : Visibility.Collapsed;
                    this.Footer.Visibility = _showFooter ? Visibility.Visible : Visibility.Collapsed;
                }

                // Suggestion options
                _allowSuggestions = appConfig.GetBoolVal(Constants.ConfigEnableWordPrediction, Constants.DefaultEnableWordPrediction);
                SuggestionsControl.SetAppConfig(appConfig);

                // Animations
                _animateControls = appConfig.GetBoolVal(Constants.ConfigAnimateControls, Constants.DefaultAnimateControls);

                // Grid viewer options
                foreach (IActionViewerControl viewer in _actionViewerControls)
                {
                    viewer.SetAppConfig(appConfig);
                }

                // Zoom
                ZoomFactor.Value = 0.01 * appConfig.GetIntVal(Constants.ConfigZoomFactorPercent, Constants.DefaultZoomFactorPercent);
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_ConfigureKeyboardWindow, ex);
            }
        }

        /// <summary>
        /// Set the source
        /// </summary>
        /// <param name="profile"></param>
        public void SetSource(BaseSource source)
        {
            try
            {
                // Reset
                _currentModeID = Constants.NoneID;
                _currentPageID = Constants.NoneID;

                _source = source;
                KeyboardViewer.SetSource(source);
                ActionStripViewer.SetSource(source);
                SquareGridViewer.SetSource(source);
                SuggestionsControl.SetSource(source);

                // Window title
                this.Title = string.Format("{0} {1} {2} - {3}", 
                    Properties.Resources.String_Player, _source.ID, Properties.Resources.String_Keyboard, Constants.ProductName);
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_SetProfileKeyboardWindow, ex);
            }
        }

        /// <summary>
        /// Drag to move window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();

            e.Handled = true;
        }

        /// <summary>
        /// Handle repositioning of window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            try
            {
                if (_isLoaded && !double.IsNaN(Width) && !double.IsNaN(Height) &&
                    Width > 0.0 && Height > 0.0 &&
                    Left > -Width && Left < SystemParameters.VirtualScreenWidth &&
                    Top > -Height && Top < SystemParameters.VirtualScreenHeight)
                {
                    Point windowPosition = new Point(this.Left / SystemParameters.WorkArea.Width,
                                                        this.Top / SystemParameters.WorkArea.Height);
                    string positionKey = string.Format("{0}{1}{2}", 
                                                        _windowRef, 
                                                        _currentGridType,
                                                        System.Windows.Forms.SystemInformation.MonitorCount > 1 ? "Multiscreen" : "");
                    _appConfig.SetPlayerVal(_source.ID, positionKey, windowPosition.ToString(CultureInfo.InvariantCulture));
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_PositionKeyboardWindow, ex);
            }
        }

        /// <summary>
        /// Handle window resize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {                
                PositionWindow(e.NewSize);
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_ResizeKeyboardWindow, ex);
            }
        }

        /// <summary>
        /// Minimise window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinimiseButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;

            e.Handled = true;
        }

        /// <summary>
        /// Handle double-click on Keysticks icon, to perform same as close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeysticksIconImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                HideWindow();
                RestoreInitialState();
            }
            e.Handled = true;
        }

        /// <summary>
        /// Close the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            HideWindow();
            RestoreInitialState();
            e.Handled = true;
        }

        /// <summary>
        /// Hide the window and try to restore the initial state
        /// </summary>
        private void RestoreInitialState()
        {
            // Restore the initial control set, provided it wouldn't result in opening this window again
            StateVector initialState = new StateVector(_source.StateTree.GetInitialState());
            AxisValue modeItem = _source.Utils.GetModeItem(initialState);
            if (modeItem != null && modeItem.GridType == EGridType.None)
            {
                _parentWindow.ThreadManager.SubmitStateEvent(new KxStateChangeEventArgs(_source.ID, initialState));
            }
        }

        /// <summary>
        /// Handle input language change
        /// </summary>
        public void KeyboardLayoutChanged(KxKeyboardChangeEventArgs args)
        {
            foreach (IActionViewerControl viewer in _actionViewerControls)
            {
                viewer.RefreshDisplay();
            }
        }

        /// <summary>
        /// Decide whether to turn word prediction on or off
        /// </summary>
        private void EnableOrDisableSuggestions(StateVector situation)
        {
            // See if this mode has word prediction actions
            bool enable = false;
            if (_allowSuggestions)
            {
                AxisValue modeItem = _source.Utils.GetModeItem(situation);
                if (_source.Actions.HasWordPredictionInMode(modeItem.ID))
                {
                    enable = true;
                }
            }

            if (enable != _showSuggestions)
            {
                _showSuggestions = enable;
                if (_showSuggestions)
                {
                    _parentWindow.ThreadManager.SubmitStateEvent(new KxPredictionEventArgs(/*_source.ID,*/ EWordPredictionEventType.Enable));
                }
                else
                {
                    SuggestionsControl.Visibility = Visibility.Hidden;
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
            _isLoaded = true;

            // Show or hide the title bar
            this.TitleBar.Visibility = _showTitleBar ? Visibility.Visible : Visibility.Collapsed;
            this.Footer.Visibility = _showFooter ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Window closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RestoreInitialState();
            _parentWindow.ChildWindowClosing(this);
        }

        /// <summary>
        /// Position the window
        /// </summary>
        private void PositionWindow(Size windowSize)
        {
            // Check that the size is set and non-zero
            if (!double.IsNaN(windowSize.Width) && !double.IsNaN(windowSize.Height) &&
                windowSize.Width > 0.0 && windowSize.Height > 0.0)
            {
                string positionKey = string.Format("{0}{1}{2}", 
                                                    _windowRef, 
                                                    _currentGridType, 
                                                    System.Windows.Forms.SystemInformation.MonitorCount > 1 ? "Multiscreen" : "");
                string posStr = _appConfig.GetPlayerVal(_source.ID, positionKey, null);
                             ;
                double x = 0.0;
                double y = 0.0;
                bool positionSet = false;
                if (posStr != null)
                {
                    // Saved position
                    Point windowPos = Point.Parse(posStr);
                    x = windowPos.X * SystemParameters.WorkArea.Width;
                    y = windowPos.Y * SystemParameters.WorkArea.Height;
                    if (x < SystemParameters.VirtualScreenWidth &&
                        y < SystemParameters.VirtualScreenHeight)
                    {
                        positionSet = true;
                    }
                }

                if (!positionSet)
                {
                    // As the default, position the window bottom, left, top or right according to player ID (1, 2, 3, 4)
                    double indent = 10.0;
                    switch (_source.ID)
                    {
                        case Constants.ID1:
                        default:
                            // Bottom
                            x = 0.5 * (SystemParameters.WorkArea.Width - windowSize.Width);
                            y = SystemParameters.WorkArea.Height - windowSize.Height;
                            break;
                        case Constants.ID2:
                            // Left
                            x = indent;
                            y = 0.5 * (SystemParameters.WorkArea.Height - windowSize.Height);
                            break;
                        case Constants.ID3:
                            // Top
                            x = 0.5 * (SystemParameters.WorkArea.Width - windowSize.Width);
                            y = 0.0;
                            break;
                        case Constants.ID4:
                            // Right
                            x = SystemParameters.WorkArea.Width - windowSize.Width - indent;
                            y = 0.5 * (SystemParameters.WorkArea.Height - windowSize.Height);
                            break;
                    }
                }

                this.Left = x;
                this.Top = y;
            }
        }

        /// <summary>
        /// Handle a controller event
        /// </summary>
        /// <param name="args"></param>
        public void HandleControllerEvent(KxSourceEventArgs args)
        {
            if (_visibleGrid != null && _animateControls)
            {
                _visibleGrid.AnimateInputEvent(args);
            }
        }

        /// <summary>
        /// Handle a state change event
        /// </summary>
        /// <param name="args"></param>
        public void HandleStateChangeEvent(KxStateChangeEventArgs args)
        {
            SetCurrentSituation(args.LogicalState);
        }

        /// <summary>
        /// Handle a word prediction event
        /// </summary>
        /// <param name="args"></param>
        public void HandlePredictionEvent(KxPredictionEventArgs args)
        {
            if (_showSuggestions)
            {
                this.SuggestionsControl.HandlePredictionEvent(args);
            }
        }

        /// <summary>
        /// Show / hide the window and show the correct grid according to the current situation
        /// </summary>
        /// <param name="situation"></param>
        private void SetCurrentSituation(StateVector newSituation)
        {
            int modeID = newSituation.ModeID;
            AxisValue modeItem = _source.StateTree.GetMode(modeID);
            if (modeItem != null)
            {
                // See if the mode has changed
                bool modeChanged = false;
                if (modeID != _currentModeID)
                {
                    // Mode changed
                    modeChanged = true;
                    _currentModeID = modeID;
                    _currentGridType = modeItem.GridType;

                    // Show or hide suggestions
                    EnableOrDisableSuggestions(newSituation);

                    // Get the grid to show if any   
                    if (_currentGridType != EGridType.None)
                    {
                        // Show this grid
                        switch (_currentGridType)
                        {
                            case EGridType.Keyboard:
                                ShowInputControl(this.KeyboardViewer);
                                break;
                            case EGridType.ActionStrip:
                                ShowInputControl(this.ActionStripViewer);
                                break;
                            case EGridType.Square4x4:
                            case EGridType.Square8x4:
                                ShowInputControl(this.SquareGridViewer);
                                break;
                            // case ... Other grid types
                        }

                        ShowWindow();
                    }
                    else
                    {
                        HideWindow();
                    }
                }

                // Update the footer info if needed
                if (modeChanged)
                {
                    string footer;
                    if (modeItem.Controls != null)
                    {
                        string navigateControlName = _source.Utils.GetGeneralControlName(modeItem.Controls.NavigationControl);
                        string selectControlName = _source.Utils.GetGeneralControlName(modeItem.Controls.SelectionControl);
                        if (!string.Equals(navigateControlName, selectControlName))
                        {
                            footer = string.Format("{0}: {1}, {2}: {3}", 
                                Properties.Resources.String_Navigate, navigateControlName, Properties.Resources.String_Select, selectControlName);
                        }
                        else
                        {
                            footer = string.Format("{0}/{1}: {2}", Properties.Resources.String_Navigate, Properties.Resources.String_Select, navigateControlName);
                        }
                    }
                    else
                    {
                        footer = "";
                    }
                    this.FooterTextBlock.Text = footer;
                }

                // Get the current page
                int pageID = newSituation.PageID;
                if (modeChanged || (pageID != _currentPageID))
                {
                    string controlSetName;
                    AxisValue pageItem;
                    if (pageID != Constants.DefaultID && (pageItem = (AxisValue)modeItem.SubValues.GetItemByID(pageID)) != null)
                    {
                        controlSetName = string.Format("{0} - {1}", modeItem.Name, pageItem.Name);
                    }
                    else
                    {
                        controlSetName = modeItem.Name;
                    }

                    this.TitleTextBlock.Text = controlSetName;
                    this.TitleTextBlock.ToolTip = string.Format("{0} - {1} {2}", 
                        controlSetName, Properties.Resources.String_Player, _source.ID);
                }
                _currentPageID = pageID;
            }

            // Update child control
            if (_visibleGrid != null)
            {
                _visibleGrid.SetCurrentSituation(newSituation);
            }
        }

        /// <summary>
        /// Select which input control to show
        /// </summary>
        /// <param name="controlToShow"></param>
        private void ShowInputControl(IActionViewerControl controlToShow)
        {
            if (_visibleGrid != null)
            {
                ((FrameworkElement)_visibleGrid).Visibility = Visibility.Collapsed;
            }

            _visibleGrid = controlToShow;

            if (_visibleGrid != null)
            {
                ((FrameworkElement)_visibleGrid).Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Show window and enable word prediction
        /// </summary>
        private void ShowWindow()
        {
            if (_isLoaded)
            {
                this.Visibility = Visibility.Visible;
                if (this.WindowState == WindowState.Minimized)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                                                new Action(delegate ()
                                                                {
                                                                    this.WindowState = WindowState.Normal;
                                                                }));
                }
            }
            else
            {
                this.Show();
            }
        }

        /// <summary>
        /// Hide window and disable word prediction
        /// </summary>
        private void HideWindow()
        {            
            // Hide window
            this.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Report an error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        private void ReportError(string message, Exception ex)
        {
            if (_parentWindow is IErrorHandler)
            {
                ((IErrorHandler)_parentWindow).HandleError(message, ex);
            }
        }
    }
}
