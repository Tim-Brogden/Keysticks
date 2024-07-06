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
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Sources;
using Keysticks.UI;

namespace Keysticks
{
    /// <summary>
    /// Window for editing the size and background of a virtual controller
    /// </summary>
    public partial class EditBackgroundWindow : Window
    {
        // Fields
        private bool _isLoaded;
        private IProfileDesignerControl _parentControl;
        private AppConfig _appConfig;
        private BaseSource _source;
        private bool _refreshingDisplay;
        private DispatcherTimer _timer;
        private const int _timerIntervalMS = 500;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent"></param>
        public EditBackgroundWindow(IProfileDesignerControl parent)
        {
            _parentControl = parent;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(_timerIntervalMS);
            _timer.Tick += HandleTimeout;

            InitializeComponent();
        }

        /// <summary>
        /// Set the app config
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        /// <summary>
        /// Set the source
        /// </summary>
        /// <param name="source"></param>
        public void SetSource(BaseSource source)
        {
            _source = source as BaseSource;

            if (_isLoaded)
            {
                RefreshDisplay();
            }
        }
        
        /// <summary>
        /// Window loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;
            
            RefreshDisplay();            
        }
        
        /// <summary>
        /// Window closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timer.IsEnabled = false;
            _parentControl.ChildWindowClosing(this);
        }

        /// <summary>
        /// Refresh window
        /// </summary>
        public void RefreshDisplay()
        {
            _refreshingDisplay = true;
            if (_source != null)
            {
                ErrorMessage.Clear();
                DisplayData display = _source.Display;
                Rect displayRegion = _source.GetBoundingRect();

                // Make sure the display region always includes the current annotations
                WidthSlider.Minimum = displayRegion.Right;
                HeightSlider.Minimum = displayRegion.Bottom;

                // Size
                WidthSlider.Value = display.Width;
                HeightSlider.Value = display.Height;

                // Background
                ColourPicker.SelectedColour = display.BackgroundColour;
                FilePathTextBox.Text = display.BackgroundImageUri;
                switch (display.BackgroundType)
                {
                    case EBackgroundType.Colour:
                        ColourRadioButton.IsChecked = true; break;
                    case EBackgroundType.Image:
                        ImageRadioButton.IsChecked = true; break;
                    default:
                        DefaultRadioButton.IsChecked = true; break;
                }
            }
            _refreshingDisplay = false;
        }

        /// <summary>
        /// Width or height changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_ValueChanged(object sender, KxDoubleValRoutedEventArgs e)
        {
            if (_isLoaded && _source != null)
            {
                try
                {
                    ErrorMessage.Clear();

                    double width = WidthSlider.Value;
                    double height = HeightSlider.Value;
                    if (width != _source.Display.Width ||
                        height != _source.Display.Height)
                    {
                        _source.Display.SetSize(width, height);
                        _source.IsModified = true;
                        ScheduleAsyncUpdate();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_BackgroundChange, ex);
                }
            }
            e.Handled = true;
        }

        /// <summary>
        /// Radio button checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (_isLoaded && _source != null)
            {
                try
                {
                    ErrorMessage.Clear();

                    EBackgroundType backgroundType;
                    if (sender == ColourRadioButton)
                    {
                        backgroundType = EBackgroundType.Colour;
                        ColourPicker.IsEnabled = true;
                        FileChoiceGrid.IsEnabled = false;
                    }
                    else if (sender == ImageRadioButton)
                    {
                        backgroundType = EBackgroundType.Image;
                        ColourPicker.IsEnabled = false;
                        FileChoiceGrid.IsEnabled = true;
                    }
                    else
                    {
                        backgroundType = EBackgroundType.Default;
                        ColourPicker.IsEnabled = false;
                        FileChoiceGrid.IsEnabled = false;
                    }

                    if (backgroundType != _source.Display.BackgroundType)
                    {
                        _source.Display.BackgroundType = backgroundType;
                        _source.IsModified = true;
                        _parentControl.RefreshBackground();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Show(Properties.Resources.E_BackgroundChange, ex);
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Colour changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ColourPickerControl_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isLoaded && _source != null && ColourPicker.SelectedColour != null)
                {
                    ErrorMessage.Clear();

                    string newColour = ColourPicker.SelectedColour;
                    if (newColour != null && newColour != _source.Display.BackgroundColour)
                    {
                        _source.Display.BackgroundColour = ColourPicker.SelectedColour;
                        _source.IsModified = true;
                        ScheduleAsyncUpdate();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_BackgroundChange, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// File path entered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilePathTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();
                HandleFilePathChange();
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_FileSelection, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Browse for image file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                string initialDir = Path.Combine(AppConfig.LocalAppDataDir, "Profiles");            
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.AddExtension = false;
                dialog.CheckFileExists = true;
                dialog.Filter = Properties.Resources.String_ImageFiles + " (*.png, *.jpg, *.jpeg, *.gif, *.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|" + Properties.Resources.String_AllFiles + " (*.*)|*.*";
                dialog.Multiselect = false;
                dialog.ShowReadOnly = false;
                dialog.Title = Properties.Resources.String_ChooseAnImageFile;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Remove the folder path if it's the same as the user's profiles folder
                    string fileName = dialog.FileName;
                    if (fileName.StartsWith(initialDir))
                    {
                        FileInfo fi = new FileInfo(fileName);
                        fileName = fi.Name;
                    }
                    FilePathTextBox.Text = fileName;

                    HandleFilePathChange();
                }                
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_FileSelection, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle new image file path
        /// </summary>
        private void HandleFilePathChange()
        {
            string filePath = FilePathTextBox.Text;
            if (_source != null && filePath != _source.Display.BackgroundImageUri)
            {
                _source.Display.BackgroundImageUri = filePath;
                _source.IsModified = true;
                _parentControl.RefreshBackground();
            }
            else
            {
                ErrorMessage.Show(Properties.Resources.E_FileNotFound, null);
            }
        }

        /// <summary>
        /// Set a new default background image or colour, 
        /// or clear any custom default if this button is clicked when the 'Default' radio button is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetAsDefaultBackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Clear();

                // Get the user's selections
                DisplayData defaultDisplayData = new DisplayData();
                if (ColourRadioButton.IsChecked == true)
                {
                    defaultDisplayData.BackgroundType = EBackgroundType.Colour;

                    string colour = ColourPicker.SelectedColour;
                    if (colour != null)
                    {
                        defaultDisplayData.BackgroundColour = colour;
                    }
                }
                else if (ImageRadioButton.IsChecked == true)
                {
                    defaultDisplayData.BackgroundType = EBackgroundType.Image;

                    string filePath = FilePathTextBox.Text;
                    defaultDisplayData.BackgroundImageUri = filePath;
                }
                
                // Ask the user to confirm
                string caption;
                string message;
                if (defaultDisplayData.BackgroundType == EBackgroundType.Default)
                {
                    caption = Properties.Resources.Q_ResetBackground;
                    message = Properties.Resources.Q_ResetBackgroundMessage;
                }
                else
                {
                    caption = Properties.Resources.Q_SetDefaultBackground;
                    message = Properties.Resources.Q_SetDefaultBackgroundMessage;
                }
                CustomMessageBox messageBox = new CustomMessageBox(this, message, caption, MessageBoxButton.OKCancel, true, false);
                if (messageBox.ShowDialog() == true)
                {
                    // Update the app config
                    defaultDisplayData.ToConfig(_appConfig);

                    // Reapply the app config
                    _parentControl.GetEditorWindow().GetTrayManager().ApplyAppConfig(_appConfig);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Show(Properties.Resources.E_SetDefaultBackground, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Schedule an async config apply
        /// </summary>
        private void ScheduleAsyncUpdate()
        {
            if (_isLoaded && !_refreshingDisplay && !_timer.IsEnabled)
            {
                _timer.Start();
            }
        }

        /// <summary>
        /// Perform a GUI update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HandleTimeout(object sender, EventArgs args)
        {
            _timer.Stop();
            _parentControl.RefreshBackground();
        }
    }
}
