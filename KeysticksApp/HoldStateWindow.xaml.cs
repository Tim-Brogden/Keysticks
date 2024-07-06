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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Diagnostics;
using Keysticks.Core;
using Keysticks.Config;
using Keysticks.Event;
using Keysticks.Sys;

namespace Keysticks
{
    /// <summary>
    /// Desktop overlay window for showing which modifiers keys are currently held
    /// </summary>
    public partial class HoldStateWindow : Window
    {
        // Fields
        private IMainWindow _parentWindow;
        private EStandardWindow _windowRef = EStandardWindow.HoldState;
        private AppConfig _appConfig;
        private bool _showModifierKeys = Constants.DefaultShowHeldModifierKeys;
        private bool _showMouseButtons= Constants.DefaultShowHeldMouseButtons;
        private EModifierKeyStates _modifierKeyState = EModifierKeyStates.None;
        private EMouseState _mouseButtonState = EMouseState.None;
        private SolidColorBrush _defaultBrush;
        private SolidColorBrush _animatedBrush;
        private DispatcherTimer _timer;
        private bool _isLoaded = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentWindow"></param>
        public HoldStateWindow(IMainWindow parentWindow)
        {
            try
            {
                _parentWindow = parentWindow;

                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromMilliseconds(Constants.DefaultHoldStateDelayMS);
                _timer.Tick += HandleTimeout;

                InitializeComponent();

                _defaultBrush = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
                _animatedBrush = new SolidColorBrush(Constants.DefaultSelectionColour);
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_CreateHoldStateWindow, ex);
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
                _appConfig = appConfig;

                _showModifierKeys = appConfig.GetBoolVal(Constants.ConfigShowHeldModifierKeys, Constants.DefaultShowHeldModifierKeys);
                _showMouseButtons = appConfig.GetBoolVal(Constants.ConfigShowHeldMouseButtons, Constants.DefaultShowHeldMouseButtons);

                // Colours
                if (!_animatedBrush.IsFrozen)
                {
                    // Note: Using player 1 selection colour
                    string colourName = appConfig.GetPlayerVal(Constants.ID1, Constants.ConfigSelectionColour, Constants.DefaultPlayer1Colours.SelectionColour);
                    Color color = ColourUtils.ColorFromString(colourName, Constants.DefaultSelectionColour);
                    _animatedBrush.Color = color;
                }

                // Make sure the display is reset next time there's an update if an option is turned off
                if (!_showModifierKeys)
                {
                    _modifierKeyState = EModifierKeyStates.None;
                }
                if (!_showMouseButtons)
                {
                    _mouseButtonState = EMouseState.None;
                }

                // Refresh the display
                if (_isLoaded)
                {
                    RefreshDisplay();
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_ConfigureHoldStateWindow, ex);
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
                    _appConfig.SetStringVal(positionKey, windowPosition.ToString(CultureInfo.InvariantCulture));
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_PositionHoldStateWindow, ex);
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
                // Ensure that window doesn't appear in Alt-Tab task switcher
                WindowInteropHelper wndHelper = new WindowInteropHelper(this);
                int exStyle = (int)WindowsAPI.GetWindowLong(wndHelper.Handle, WindowsAPI.GWL_EXSTYLE);
                exStyle |= WindowsAPI.WS_EX_TOOLWINDOW;
                WindowsAPI.SetWindowLong(wndHelper.Handle, WindowsAPI.GWL_EXSTYLE, (IntPtr)exStyle);

                _isLoaded = true;

                // Position window
                PositionWindow();
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_LoadHoldStateWindow, ex);
            }
        }

        /// <summary>
        /// Closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _parentWindow.ChildWindowClosing(this);
        }

        /// <summary>
        /// Position the window
        /// </summary>
        private void PositionWindow()
        {
            string positionKey = _windowRef.ToString();
            if (System.Windows.Forms.SystemInformation.MonitorCount > 1)
            {
                positionKey += "Multiscreen";
            }
            string posStr = _appConfig.GetStringVal(positionKey, null);

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
                // Default position
                x = 0.5 * (SystemParameters.WorkArea.Width - this.Width);
                y = 0.0;
            }

            this.Left = x;
            this.Top = y;
        }              

        /// <summary>
        /// Show which modifier keys are held
        /// </summary>
        /// <param name="args"></param>
        public void HandleKeyboardStateEvent(KxKeyboardStateEventArgs args)
        {           
            // Store new modifier state
            _modifierKeyState = args.ModifierState;

            if (_showModifierKeys)
            {
                _timer.IsEnabled = true;
            }
        }

        /// <summary>
        /// Show which mouse buttons are held
        /// </summary>
        /// <param name="feetUpMouseButtonStateEventArgs"></param>
        public void HandleMouseButtonStateEvent(KxMouseButtonStateEventArgs args)
        {
            // Store new button state
            _mouseButtonState = args.MouseButton;

            if (_showMouseButtons)
            {
                _timer.IsEnabled = true;
            }
        }

        /// <summary>
        /// Perform a GUI update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HandleTimeout(object sender, EventArgs args)
        {
            try
            {
                // Stop the timer
                _timer.Stop();

                RefreshDisplay();
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_ShowHeldKeysAndButtons, ex);
            }
        }

        /// <summary>
        /// Refresh display
        /// </summary>
        private void RefreshDisplay()
        {
            // Show the key state if one of the modifiers is set
            //Trace.WriteLine(_modifierKeyState);
            if (_modifierKeyState != EModifierKeyStates.None)
            {
                ShiftBorder.Background = (_modifierKeyState & EModifierKeyStates.AnyShiftKey) != 0 ? _animatedBrush : _defaultBrush;
                CtrlBorder.Background = (_modifierKeyState & EModifierKeyStates.AnyControlKey) != 0 ? _animatedBrush : _defaultBrush;
                WinBorder.Background = (_modifierKeyState & EModifierKeyStates.AnyWinKey) != 0 ? _animatedBrush : _defaultBrush;
                AltBorder.Background = (_modifierKeyState & EModifierKeyStates.AnyMenuKey) != 0 ? _animatedBrush : _defaultBrush;

                KeysPanel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                KeysPanel.Visibility = Visibility.Collapsed;
            }

            // Show the mouse button state if one of the buttons is pressed
            if (_mouseButtonState != EMouseState.None)
            {
                LeftMouseButton.Background = (_mouseButtonState & EMouseState.Left) != 0 ? _animatedBrush : _defaultBrush;
                MiddleMouseButton.Background = (_mouseButtonState & EMouseState.Middle) != 0 ? _animatedBrush : _defaultBrush;
                RightMouseButton.Background = (_mouseButtonState & EMouseState.Right) != 0 ? _animatedBrush : _defaultBrush;
                X1MouseButton.Background = (_mouseButtonState & EMouseState.X1) != 0 ? _animatedBrush : _defaultBrush;
                X2MouseButton.Background = (_mouseButtonState & EMouseState.X2) != 0 ? _animatedBrush : _defaultBrush;

                MousePanel.Visibility = Visibility.Visible;
            }
            else
            {
                MousePanel.Visibility = Visibility.Collapsed;
            }

            // Show or hide the window
            if (_modifierKeyState != EModifierKeyStates.None || _mouseButtonState != EMouseState.None)
            {
                if (!_isLoaded)
                {
                    this.Show();
                }
                else
                {
                    this.Visibility = Visibility.Visible;
                }
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
            }
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
