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
using System.Diagnostics;
using Keysticks.Actions;
using Keysticks.Core;
using Keysticks.Sys;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for editing Activate Window actions
    /// </summary>
    public partial class ActivateWindowControl : UserControl
    {
        // Fields
        private ActivateWindowAction _currentAction = new ActivateWindowAction();
        private Dictionary<int, string> _processNames = new Dictionary<int, string>();
        private Dictionary<string, List<string>> _windowTable = new Dictionary<string, List<string>>();
        private CallBackPtr _enumWindowsCallBack;
        private string _processBeingEnumerated = null;
        private NamedItemList _programList = new NamedItemList();
        private NamedItemList _windowList = new NamedItemList();
        private string _selectedProgram = "";

        // Dependency properties
        public bool ShowOptions
        {
            get { return (bool)GetValue(ShowOptionsProperty); }
            set { SetValue(ShowOptionsProperty, value); }
        }
        private static readonly DependencyProperty ShowOptionsProperty =
            DependencyProperty.Register(
            "ShowOptions",
            typeof(bool),
            typeof(ActivateWindowControl),
            new FrameworkPropertyMetadata(true)
        );

        public bool RequireTitle
        {
            get { return (bool)GetValue(RequireTitleProperty); }
            set { SetValue(RequireTitleProperty, value); }
        }
        private static readonly DependencyProperty RequireTitleProperty =
            DependencyProperty.Register(
            "RequireTitle",
            typeof(bool),
            typeof(ActivateWindowControl),
            new FrameworkPropertyMetadata(true)
        );

        /// <summary>
        /// Constructor
        /// </summary>
        public ActivateWindowControl()
        {
            InitializeComponent();

            RestoreIfMinimisedCheckBox.Visibility = ShowOptions ? Visibility.Visible : Visibility.Collapsed;
            MinimiseIfActiveCheckBox.Visibility = ShowOptions ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Control loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateWindowTable();
            PopulateProgramsComboBox();

            ProgramNameComboBox.ItemsSource = _programList;
            WindowTitleComboBox.ItemsSource = _windowList;

            RefreshDisplay();
        }

        /// <summary>
        /// Refresh the lists of running programs and windows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedProgram = ProgramNameComboBox.Text;
            string selectedWindow = WindowTitleComboBox.Text;

            PopulateWindowTable();
            PopulateProgramsComboBox();
            HandleProgramChange();

            ProgramNameComboBox.Text = selectedProgram;
            WindowTitleComboBox.Text = selectedWindow;
        }

        /// <summary>
        /// Populate the dictionary of window titles by process name
        /// </summary>
        private void PopulateWindowTable()
        {
            _processNames.Clear();
            _windowTable.Clear();
            _enumWindowsCallBack = new CallBackPtr(this.EnumWindowsCallback);
            WindowsAPI.EnumWindows(_enumWindowsCallBack, IntPtr.Zero);
        }

        /// <summary>
        /// Callback for EnumWindows / EnumChildWindows calls
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private bool EnumWindowsCallback(IntPtr hWnd, IntPtr hParentWindow)
        {
            try
            {
                // Exclude non-standard windows
                uint style = WindowsAPI.GetWindowStyle(hWnd);
                bool include = WindowsAPI.IsStandardWindowStyle(style);
                if (include)
                {
                    if (hParentWindow == IntPtr.Zero)
                    {
                        // Top level window
                        _processBeingEnumerated = null;

                        // Get window's process ID
                        int processID;
                        if (0u != WindowsAPI.GetWindowThreadProcessId(hWnd, out processID))
                        {
                            // Get process name
                            string processName = null;
                            if (_processNames.ContainsKey(processID))
                            {
                                processName = _processNames[processID];
                            }
                            else
                            {
                                Process process = Process.GetProcessById(processID);
                                if (process != null)
                                {
                                    processName = process.ProcessName;
                                    _processNames[processID] = processName;
                                }
                            }

                            if (!string.IsNullOrEmpty(processName))
                            {
                                _processBeingEnumerated = processName;
                            }
                        }
                    }

                    if (_processBeingEnumerated != null)
                    {
                        // Get the window title
                        string title = WindowsAPI.GetTitleBarText(hWnd);
                        if (!string.IsNullOrWhiteSpace(title))
                        {
                            //Console.WriteLine(string.Format("Style {0:X8} Title: {1}", style, title));

                            // Add to list of windows for this process
                            List<string> windowsForProcess;
                            if (_windowTable.ContainsKey(_processBeingEnumerated))
                            {
                                windowsForProcess = _windowTable[_processBeingEnumerated];
                            }
                            else
                            {
                                windowsForProcess = new List<string>();
                                _windowTable[_processBeingEnumerated] = windowsForProcess;
                            }

                            if (!windowsForProcess.Contains(title))
                            {
                                windowsForProcess.Add(title);
                            }
                        }

                        // Recurse through child windows
                        WindowsAPI.EnumChildWindows(hWnd, _enumWindowsCallBack, hWnd);
                    }
                }         
            }
            catch (Exception)
            {
                // Ignore errors
            }

            return true;
        }

        /// <summary>
        /// Populate the program list
        /// </summary>
        private void PopulateProgramsComboBox()
        {
            _programList.Clear();
            int i = 0;
            foreach (string programName in _windowTable.Keys)
            {
                _programList.Add(new NamedItem(i++, programName));
            }
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is ActivateWindowAction)
            {
                _currentAction = (ActivateWindowAction)action;
                RefreshDisplay();
            }
        }

        /// <summary>
        /// Display the current action
        /// </summary>
        private void RefreshDisplay()
        {
            if (IsLoaded && _currentAction != null)
            {
                ProgramNameComboBox.Text = _currentAction.ProgramName;
                switch (_currentAction.MatchType)
                {
                    case EMatchType.Equals:
                        EqualsRadioButton.IsChecked = true; break;
                    case EMatchType.StartsWith:
                        StartsWithRadioButton.IsChecked = true; break;
                    case EMatchType.EndsWith:
                        EndsWithRadioButton.IsChecked = true; break;
                }
                WindowTitleComboBox.Text = _currentAction.WindowTitle;
                RestoreIfMinimisedCheckBox.IsChecked = _currentAction.RestoreIfMinimised;
                MinimiseIfActiveCheckBox.IsChecked = _currentAction.MinimiseIfActive;
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = null;
            string programName = this.ProgramNameComboBox.Text;
            string windowName = this.WindowTitleComboBox.Text;
            if (windowName != "" || (!RequireTitle && programName != ""))
            {
                _currentAction = new ActivateWindowAction();
                _currentAction.ProgramName = this.ProgramNameComboBox.Text;
                EMatchType matchType;
                if (StartsWithRadioButton.IsChecked == true)
                {
                    matchType = EMatchType.StartsWith;
                }
                else if (EndsWithRadioButton.IsChecked == true)
                {
                    matchType = EMatchType.EndsWith;
                }
                else
                {
                    matchType = EMatchType.Equals;
                }
                _currentAction.WindowTitle = windowName;
                _currentAction.MatchType = matchType;
                _currentAction.RestoreIfMinimised = RestoreIfMinimisedCheckBox.IsChecked == true;
                _currentAction.MinimiseIfActive = MinimiseIfActiveCheckBox.IsChecked == true;
            }

            return _currentAction;
        }

        /// <summary>
        /// Handle selection of program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgramNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NamedItem selectedItem = (NamedItem)ProgramNameComboBox.SelectedItem;
            if (selectedItem != null)
            {
                _selectedProgram = selectedItem.Name;
                HandleProgramChange();
            }
        }

        /// <summary>
        /// Handle loss of keyboard focus for case where user manually types a program name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgramNameComboBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!_selectedProgram.Equals(ProgramNameComboBox.Text))
            {
                _selectedProgram = ProgramNameComboBox.Text;
                HandleProgramChange();
            }
        }

        /// <summary>
        /// Handle change of program
        /// </summary>
        private void HandleProgramChange()
        {
            // Update window list
            _windowList.Clear();
            if (_windowTable.ContainsKey(_selectedProgram))
            {
                int i = 0;
                foreach (string windowName in _windowTable[_selectedProgram])
                {
                    _windowList.Add(new NamedItem(i++, windowName));
                }
            }
        }
    }
}
