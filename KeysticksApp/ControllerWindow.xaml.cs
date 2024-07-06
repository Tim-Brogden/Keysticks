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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using Keysticks.Core;
using Keysticks.Config;
using Keysticks.Event;
using Keysticks.Sources;

namespace Keysticks
{
    /// <summary>
    /// Virtual controller window
    /// </summary>
    public partial class ControllerWindow : Window, IViewerWindow
    {
        // Fields
        private bool _isLoaded = false;
        private IMainWindow _parentWindow;
        private EStandardWindow _windowRef = EStandardWindow.Controller;
        private AppConfig _appConfig;
        private BaseSource _source;
        private int _currentModeID = Constants.NoneID;
        private int _currentPageID = Constants.NoneID;
        private bool _animateControls = Constants.DefaultAnimateControls;
        private DoubleValItem _zoomFactor = new DoubleValItem(1.0);
        private string _currentTitleBarText = "";
        private DispatcherTimer _titleBarTimer;

        // Properties
        public BaseSource Source { get { return _source; } }
        public DoubleValItem ZoomFactor { get { return _zoomFactor; } }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ControllerWindow(IMainWindow parentWindow)
        {
            try
            {
                _parentWindow = parentWindow;

                InitializeComponent();
                
                this.DataContext = this;
                _titleBarTimer = new DispatcherTimer();
                _titleBarTimer.Tick += TitleBarTimer_Tick;
                _titleBarTimer.Interval = TimeSpan.FromMilliseconds(Constants.TitleBarAnimationDuration);                                
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_CreateControllerWindow, ex);
            }
        }

        /// <summary>
        /// Drag window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();

            e.Handled = true;
        }

        /// <summary>
        /// Handle right-click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControllerViewControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _parentWindow.GetTrayManager().ShowContextMenu();            
        }

        /// <summary>
        /// Set new configuration options
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            try
            {
                _appConfig = appConfig;

                // Stay on top
                this.Topmost = appConfig.GetBoolVal(Constants.ConfigKeepControlsWindowOnTop, Constants.DefaultKeepControlsWindowOnTop);

                // Animations
                _animateControls = appConfig.GetBoolVal(Constants.ConfigAnimateControls, Constants.DefaultAnimateControls);
                if (_isLoaded && !_animateControls)
                {
                    // Make sure the highlights are cleared in case a control is currently pressed / directed
                    this.ControllerViewControl.ClearAnimations();
                }

                // Opacity
                double currentControlsOpacity = 0.01 * appConfig.GetIntVal(Constants.ConfigCurrentControlsOpacityPercent, Constants.DefaultWindowOpacityPercent);
                LinearGradientBrush brush = (LinearGradientBrush)this.FindResource("TitleBarBrush");
                if (brush != null && !brush.IsFrozen)
                {
                    brush.Opacity = currentControlsOpacity;
                }

                // Zoom
                ZoomFactor.Value = 0.01 * appConfig.GetIntVal(Constants.ConfigZoomFactorPercent, Constants.DefaultZoomFactorPercent);

                // Compact view option
                bool isCompactView = appConfig.GetBoolVal(Constants.ConfigCompactControlsWindow, Constants.DefaultCompactControlsWindow);
                if (isCompactView != ControllerViewControl.IsCompactView)
                {                    
                    if (_isLoaded && _source != null)
                    {
                        // Reposition controls if compact view option has changed
                        Rect displayRegion = GetDisplayRegion();
                        PositionControls(_source, displayRegion.TopLeft);
                    }
                }

                // Controller options
                this.ControllerViewControl.SetOpacity(currentControlsOpacity);
                this.ControllerViewControl.SetAppConfig(appConfig);
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_ConfigureControllerWindow, ex);
            }
        }

        /// <summary>
        /// Set the source to display
        /// </summary>
        /// <param name="profile"></param>
        public void SetSource(BaseSource source)
        {
            try
            {
                // Reset current mode and page
                _currentModeID = Constants.NoneID;
                _currentPageID = Constants.NoneID;

                _source = source;
                ControllerViewControl.SetSource(source);

                // Window title
                this.Title = string.Format("{0} {1} {2} - {3}", 
                    Properties.Resources.String_Player, _source.ID, Properties.Resources.String_Controls, Constants.ProductName);

                if (_isLoaded)
                {
                    Rect displayRegion = GetDisplayRegion();
                    PositionControls(_source, displayRegion.TopLeft);
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_ConfigureControllerWindow, ex);
            }
        }

        /// <summary>
        /// Handle input language change
        /// </summary>
        public void KeyboardLayoutChanged(KxKeyboardChangeEventArgs args)
        {
            this.ControllerViewControl.RefreshDisplay();
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
                    string positionKey = _windowRef.ToString();
                    if (System.Windows.Forms.SystemInformation.MonitorCount > 1)
                    {
                        positionKey += "Multiscreen";
                    } 
                    _appConfig.SetPlayerVal(_source.ID, positionKey, windowPosition.ToString(CultureInfo.InvariantCulture));
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_PositionControllerWindow, ex);
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
                _isLoaded = true;

                if (_source != null)
                {
                    Rect displayRegion = GetDisplayRegion();
                    PositionWindow(displayRegion);
                    PositionControls(_source, displayRegion.TopLeft);
                }
            }
            catch (Exception ex)
            {
                HandleError(Properties.Resources.E_LoadControllerWindow, ex);
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
                HideOrExit();
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
            HideOrExit();
            e.Handled = true;
        }

        /// <summary>
        /// Either close the window, or prompt the user to save and exit, according to whether background running is allowed
        /// </summary>
        private void HideOrExit()
        {
            // When closing the main controller window, show a message to indicate still running
            bool onlyControllerWindow = true;
            for (int id = Constants.ID1; id <= Constants.ID4; id++)
            {
                ControllerWindow window = _parentWindow.GetControllerWindow(id);
                if (window != null && window != this && window.Visibility == Visibility.Visible)
                {
                    onlyControllerWindow = false;
                    break;
                }
            }

            ITrayManager trayManager = _parentWindow.GetTrayManager();
            bool allowBackgroundRunning = _appConfig.GetBoolVal(Constants.ConfigAllowBackgroundRunning, Constants.DefaultAllowBackgroundRunning);
            if (!onlyControllerWindow || allowBackgroundRunning)
            {
                // Hide the window
                this.Visibility = Visibility.Hidden;

                if (onlyControllerWindow)
                {
                    // Show running in background
                    string message = Properties.Resources.String_RunningInBackground;
                    string caption = "";
                    Profile profile = _parentWindow.GetProfile();
                    if (profile != null)
                    {
                        caption = string.Format("({0})", profile.Name);
                    }
                    trayManager.ShowTemporaryMessage(message, caption, System.Windows.Forms.ToolTipIcon.Info);
                }
            }
            else
            {
                // Let the user save, then exit the program
                trayManager.PromptToSaveAndExit();
            }
        }
                  
        /// <summary>
        /// Window closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _titleBarTimer.Stop();

            _parentWindow.ChildWindowClosing(this);            
        }

        /// <summary>
        /// Get the rectangular portion of the co-ordinate space to display
        /// </summary>
        /// <returns></returns>
        private Rect GetDisplayRegion()
        {
            Rect displayRegion;
            if (_source != null)
            {
                bool isCompactView = _appConfig.GetBoolVal(Constants.ConfigCompactControlsWindow, Constants.DefaultCompactControlsWindow);
                if (isCompactView)
                {
                    displayRegion = _source.GetBoundingRect();
                }
                else
                {
                    displayRegion = new Rect(0.0, 0.0, _source.Display.Width, _source.Display.Height);
                }
            }
            else
            {
                displayRegion = new Rect(0.0, 0.0, Constants.DefaultControllerWidth, Constants.DefaultControllerHeight);
            }

            return displayRegion;
        }

        /// <summary>
        /// Position the window
        /// </summary>
        private void PositionWindow(Rect displayRegion)
        {
            string positionKey = _windowRef.ToString();
            if (System.Windows.Forms.SystemInformation.MonitorCount > 1)
            {
                positionKey += "Multiscreen";
            }
            string posStr = _appConfig.GetPlayerVal(_source.ID, positionKey, null);

            double x = 0.0;
            double y = 0.0;
            bool positionSet = false;
            if (posStr != null)
            {
                // Stored position
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
                // As the default, position the window in a corner of the screen according to player ID
                double indent = 10.0;
                switch (_source.ID)
                {
                    case Constants.ID1:
                    default:
                        // Bottom right
                        x = SystemParameters.WorkArea.Width - displayRegion.Width - indent;
                        y = SystemParameters.WorkArea.Height - displayRegion.Height;
                        break;
                    case Constants.ID2:
                        // Bottom left
                        x = indent;
                        y = SystemParameters.WorkArea.Height - displayRegion.Height;
                        break;
                    case Constants.ID3:
                        // Top left
                        x = indent;
                        y = 0.0;
                        break;
                    case Constants.ID4:
                        // Top right
                        x = SystemParameters.WorkArea.Width - displayRegion.Width - indent;
                        y = 0.0;
                        break;
                }
            }

            this.Left = x;
            this.Top = y;            
        }

        /// <summary>
        /// Position the title bar and menu according for the current virtual controller
        /// </summary>
        /// <param name="virtualControllerData"></param>
        private void PositionControls(BaseSource virtualController, Point origin)
        {
            if (virtualController != null)
            {
                this.TitleBar.Margin = new Thickness(_source.Display.TitleBarData.DisplayRect.X - origin.X, _source.Display.TitleBarData.DisplayRect.Y - origin.Y, 0.0, 0.0);
                this.MenuBar.Margin = new Thickness(_source.Display.MenuBarData.DisplayRect.X - origin.X, _source.Display.MenuBarData.DisplayRect.Y - origin.Y, 0.0, 0.0);
            }
        }

        /// <summary>
        /// Set the current situation
        /// </summary>
        /// <param name="situation"></param>
        private void SetCurrentSituation(StateVector situation)
        {            
            // Update the title
            int modeID = situation.ModeID;
            AxisValue modeItem = _source.StateTree.GetMode(modeID);
            if (modeItem != null)
            {
                int pageID = situation.PageID;
                if (modeID != _currentModeID || pageID != _currentPageID)
                {
                    string controlSetName;
                    AxisValue pageItem;
                    if (pageID != Constants.DefaultID && (pageItem = (AxisValue)modeItem.SubValues.GetItemByID(pageID)) != null)
                    {
                        controlSetName = pageItem.Name;
                    }
                    else
                    {
                        controlSetName = modeItem.Name;
                    }

                    SetTitleBarText(controlSetName, false);
                }

                _currentModeID = modeID;
                _currentPageID = pageID;
            }

            // Update child control
            this.ControllerViewControl.SetCurrentSituation(situation);
        }

        /// <summary>
        /// Handle a controller event 
        /// </summary>
        /// <param name="ev"></param>
        public void HandleControllerEvent(KxSourceEventArgs args)
        {
            if (_animateControls)
            {
                this.ControllerViewControl.AnimateInputEvent(args);
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
        /// Open (profile browser) button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            _parentWindow.GetTrayManager().FileOpen_Click(sender, e);
        }

        /// <summary>
        /// Edit button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditProfile_Click(object sender, RoutedEventArgs e)
        {
            _parentWindow.GetTrayManager().EditProfile_Click(sender, e);
        }

        /// <summary>
        /// Menu button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            _parentWindow.GetTrayManager().ShowContextMenu();            
        }

        /// <summary>
        /// Program options button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolsOptions_Click(object sender, RoutedEventArgs e)
        {
            _parentWindow.GetTrayManager().ToolsOptions_Click(sender, e);
        }

        /// <summary>
        /// Help button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewHelp_Click(object sender, RoutedEventArgs e)
        {
            _parentWindow.GetTrayManager().ViewHelp_Click(sender, e);
        }

        /// <summary>
        /// Warning icon clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WarningIconImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                _parentWindow.GetTrayManager().ViewLogFile_Click(sender, e);
            }
        }

        /// <summary>
        /// Set the title bar text
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void SetTitleBarText(string message, bool isTemporary)
        {
            if (_isLoaded)
            {
                if (isTemporary)
                {
                    this.TitleTextBlock.Text = message;

                    _titleBarTimer.IsEnabled = true;
                }
                else
                {
                    _currentTitleBarText = message;

                    // Don't overwrite a temporary title
                    if (!_titleBarTimer.IsEnabled)
                    {
                        this.TitleTextBlock.Text = message;
                    }
                }
            }
        }

        /// <summary>
        /// Set the title bar tooltip text
        /// </summary>
        /// <param name="toolTip"></param>
        public void SetTitleBarToolTip(string toolTip)
        {
            if (_isLoaded)
            {
                this.TitleTextBlock.ToolTip = toolTip;
            }
        }

        /// <summary>
        /// Show warning icon
        /// </summary>
        public void ShowWarningIcon()
        {
            if (_isLoaded)
            {
                Grid.SetColumnSpan(TitleTextBlock, 1);
                this.WarningIconImage.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Restore the original title bar text after a temporary animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TitleBarTimer_Tick(object sender, EventArgs e)
        {
            _titleBarTimer.IsEnabled = false;

            this.TitleTextBlock.Text = _currentTitleBarText;
        }

        /// <summary>
        /// Report an error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        private void HandleError(string message, Exception ex)
        {
            _parentWindow.HandleError(message, ex);
        }

        /// <summary>
        /// Clear errors icon
        /// </summary>
        public void ClearError()
        {
            if (_isLoaded)
            {
                this.WarningIconImage.Visibility = Visibility.Collapsed;
                Grid.SetColumnSpan(TitleTextBlock, 2);
            }
        }
    }
}
