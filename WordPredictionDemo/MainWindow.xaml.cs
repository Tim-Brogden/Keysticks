/******************************************************************************
 *
 * Copyright 2019 Tim Brogden
 * All rights reserved. This program and the accompanying materials   
 * are made available under the terms of the Eclipse Public License v1.0  
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *           
 * Contributors: 
 * Tim Brogden - WordPredictor COM component with demo application
 *
 *****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.IO;

namespace WordPredictionDemo
{
    /// <summary>
    /// Demonstrates how to add word prediction powered by the OpenAdaptxt API to a C#.NET application
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<string> _currentSuggestionsList;
        private ObservableCollection<DictionaryItem> _dictionaryList;
        private WordPredictor _wordPredictor;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Window loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Create word predictor
            _wordPredictor = new WordPredictor();
            _wordPredictor.SuggestionsReceived += new EventHandler(WordPredictor_SuggestionsReceived);

            // Create suggestions list
            _currentSuggestionsList = new ObservableCollection<string>();
            WordSuggestionsList.ItemsSource = _currentSuggestionsList;

            // Create dictionary list
            _dictionaryList = new ObservableCollection<DictionaryItem>();
            DictionariesTable.ItemsSource = _dictionaryList;
        }

        /// <summary>
        /// Populate the list of dictionaries based upon the package files found, plus the default dictionary
        /// </summary>
        private void RefreshDictionariesList()
        {
            // Remember which dictionaries are currently inactive
            List<string> disabledDictionaries = new List<string>();
            foreach (DictionaryItem item in _dictionaryList)
            {
                if (!item.IsEnabled)
                {
                    disabledDictionaries.Add(item.Name);
                }
            }

            // Add the default dictionary
            _dictionaryList.Clear();
            _dictionaryList.Add(new DictionaryItem(WordPredictor.DefaultInstalledDictionary, true));

            // Add any packages
            string packagesPath = Path.Combine(_wordPredictor.BasePath, "Packages");
            DirectoryInfo packagesDir = new DirectoryInfo(packagesPath);
            FileInfo[] packagesList = packagesDir.GetFiles("*.atp");
            foreach (FileInfo packageFile in packagesList)
            {
                string lang = packageFile.Name.Replace(".atp", "");
                if (lang != WordPredictor.DefaultInstalledDictionary)
                {
                    _dictionaryList.Add(new DictionaryItem(lang, true));
                }
            }

            // Preserve inactive dictionary states
            foreach (string name in disabledDictionaries)
            {
                foreach (DictionaryItem item in _dictionaryList)
                {
                    if (item.Name == name)
                    {
                        item.IsEnabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Receive a new suggestions list from the Word Prediction Server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WordPredictor_SuggestionsReceived(object sender, EventArgs e)
        {
            // Update the suggestions list
            _currentSuggestionsList.Clear();
            foreach (string suggestion in _wordPredictor.CurrentSuggestionsList)
            {
                _currentSuggestionsList.Add(suggestion);
            }

            // Select first suggestion if possible
            if (_currentSuggestionsList.Count > 0)
            {
                this.WordSuggestionsList.SelectedIndex = 0;
                InsertButton.IsEnabled = true;
            }
            else
            {
                InsertButton.IsEnabled = false;
            }
        }
        
        /// <summary>
        /// Install new packages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Text = "";
                _wordPredictor.SendInstallPackages();
                RefreshDictionariesList();
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = ex.Message;
            }
        }

        /// <summary>
        /// Uninstall all packages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UninstallButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Text = "";
                _wordPredictor.SendUninstallPackages();

                // Remove non-default dictionaries from list
                int i = 1;
                while (i < _dictionaryList.Count)
                {
                    _dictionaryList.RemoveAt(i);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = ex.Message;
            }
        }

        /// <summary>
        /// Set active dictionaries
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetDictionariesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Text = "";
                StringBuilder sb = new StringBuilder();
                foreach (DictionaryItem item in _dictionaryList)
                {
                    if (item.IsEnabled)
                    {
                        sb.Append(item.Name);
                        sb.Append(',');
                    }
                }
                string dictionariesStr = sb.ToString().TrimEnd(',');

                _wordPredictor.SendActiveDictionariesList(dictionariesStr);
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = ex.Message;
            }
        }

        /// <summary>
        /// Turn learning on or off
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LearningCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorMessage.Text = "";
                _wordPredictor.SendConfigureLearning(EnableLearningCheckbox.IsChecked == true);
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = ex.Message;
            }
        }

        /// <summary>
        /// Window closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _wordPredictor.Dispose();
        }

        /// <summary>
        /// Input text changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ErrorMessage.Text = "";
                if (InputTextBox.Text != "")
                {
                    foreach (TextChange change in e.Changes)
                    {
                        _wordPredictor.SendSetCursor((uint)change.Offset);
                        if (change.RemovedLength != 0)
                        {
                            _wordPredictor.SendDelete((uint)change.RemovedLength);
                        }
                        if (change.AddedLength != 0)
                        {
                            string addedText = InputTextBox.Text.Substring(change.Offset, change.AddedLength);
                            _wordPredictor.SendText(addedText);
                        }
                    }
                    _wordPredictor.SendGetSuggestions();
                }
                else
                {
                    _wordPredictor.SendReset();
                }                
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = ex.Message;
            }

            e.Handled = true;
        }

        /// <summary>
        /// Clicked in the input text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputTextBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_wordPredictor.CurrentIndex != InputTextBox.CaretIndex)
            {
                try
                {
                    _wordPredictor.SendSetCursor((uint)InputTextBox.CaretIndex);
                    _wordPredictor.SendGetSuggestions();
                }
                catch (Exception ex)
                {
                    ErrorMessage.Text = ex.Message;
                }
            }
        }

        /// <summary>
        /// Typed in the input text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                bool requestSuggestions = true;
                switch (e.Key)
                {
                    case Key.Left:
                        if (_wordPredictor.CurrentIndex > 0)
                        {
                            _wordPredictor.SendLeftCursor(1);
                        }
                        break;
                    case Key.Right:
                        if (_wordPredictor.CurrentIndex < InputTextBox.Text.Length - 1)
                        {
                            _wordPredictor.SendRightCursor(1);
                        }
                        break;
                    case Key.Home:
                        _wordPredictor.SendSetCursor(0u);
                        break;
                    case Key.End:
                        _wordPredictor.SendSetCursor((uint)InputTextBox.Text.Length);
                        break;
                    default:
                        requestSuggestions = false;
                        break;
                }

                if (requestSuggestions)
                {
                    _wordPredictor.SendGetSuggestions();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = ex.Message;
            }

            e.Handled = true;
        }

        /// <summary>
        /// Double-clicked a suggestion to insert it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WordSuggestionsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            InsertButton_Click(sender, e);
        }

        /// <summary>
        /// Insert button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Text = "";

            int selectedIndex = WordSuggestionsList.SelectedIndex;
            if (selectedIndex != -1 && selectedIndex < _currentSuggestionsList.Count)
            {
                try
                {
                    int numBackspaces = 0;
                    string suggestion = _currentSuggestionsList[selectedIndex];

                    string prefix = _wordPredictor.CurrentPrefix;
                    if (prefix != null)
                    {
                        if (suggestion.StartsWith(prefix))
                        {
                            suggestion = suggestion.Remove(0, prefix.Length);
                        }
                        else
                        {
                            numBackspaces = prefix.Length;
                        }
                    }

                    int numDeletes = 0;
                    string suffix = _wordPredictor.CurrentSuffix;
                    if (suffix != null)
                    {
                        if (suggestion.EndsWith(suffix))
                        {
                            suggestion = suggestion.Substring(0, suggestion.Length - suffix.Length);
                        }
                        else
                        {
                            numDeletes = suffix.Length;
                        }
                    }

                    if (suggestion.Length != 0)
                    {
                        if (suffix == null)
                        {
                            suggestion += " ";  // Auto-insert space
                        }

                        string displayedText = InputTextBox.Text;
                        int offset = InputTextBox.CaretIndex - numBackspaces;
                        if (numBackspaces != 0)
                        {
                            displayedText = displayedText.Remove(offset, numBackspaces);
                        }
                        if (numDeletes != 0)
                        {
                            displayedText = displayedText.Remove(offset, numDeletes);
                        }

                        InputTextBox.Text = displayedText.Insert(offset, suggestion);

                        offset += suggestion.Length;
                        InputTextBox.CaretIndex = offset;
                        InputTextBox.Focus();

                        if (_wordPredictor.CurrentIndex != offset)
                        {
                            _wordPredictor.SendSetCursor((uint)offset);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Text = ex.Message;
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            e.Handled = true;
        }

    }
}
