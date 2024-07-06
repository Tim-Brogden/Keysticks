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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Sources;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for displaying a list of word suggestions while typing
    /// </summary>
    public partial class SuggestionListControl : UserControl
    {
        // Fields
        private IMainWindow _mainWindow;
        private bool _provideWordSuggestions = false;
        private bool _autoInsertSpaces = Constants.DefaultAutoInsertSpaces;
        private NamedItemList _suggestionsList = new NamedItemList();
        private WordPredictionResponse _prediction;
        private ColourScheme _colourScheme;
        private BaseSource _source;

        /// <summary>
        /// Constructor
        /// </summary>
        public SuggestionListControl()
        {
            InitializeComponent();

            this.SuggestionsListBox.DataContext = _suggestionsList;
        }

        /// <summary>
        /// Store a reference to the main window
        /// </summary>
        /// <param name="window"></param>
        public void SetMainWindow(IMainWindow window)
        {
            _mainWindow = window;
        }

        /// <summary>
        /// Set the config
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            // Set options
            _provideWordSuggestions = appConfig.GetBoolVal(Constants.ConfigEnableWordPrediction, Constants.DefaultEnableWordPrediction);
            _autoInsertSpaces = appConfig.GetBoolVal(Constants.ConfigAutoInsertSpaces, Constants.DefaultAutoInsertSpaces);

            // Set colours
            _colourScheme = appConfig.ColourScheme;
            UpdateBrushes();
        }

        /// <summary>
        /// Set the source
        /// </summary>
        /// <param name="profile"></param>
        public void SetSource(BaseSource source)
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

                SolidColorBrush backgroundBrush = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
                backgroundBrush.Opacity = _colourScheme.InteractiveControlsOpacity;
                this.Resources["BackgroundBrush"] = backgroundBrush;

                Color selectionColour = ColourUtils.ColorFromString(playerColours.SelectionColour, Constants.DefaultSelectionColour);
                this.Resources["SelectionBrush"] = new SolidColorBrush(selectionColour);
            }
        }
        
        /// <summary>
        /// Handle word prediction event
        /// </summary>
        /// <param name="args"></param>
        public void HandlePredictionEvent(KxPredictionEventArgs args)
        {
            switch (args.PredictionEventType)
            {
                case EWordPredictionEventType.SuggestionsList:
                    BindSuggestions(args.PredictionResponse);
                    break;
                case EWordPredictionEventType.NextSuggestion:
                    if (_prediction != null)
                    {
                        if (this.SuggestionsListBox.SelectedIndex < _prediction.NumSuggestions - 1)
                        {
                            this.SuggestionsListBox.SelectedIndex++;
                        }
                        else
                        {
                            // For one turn in the cycle, clear the selection (when going forwards)
                            this.SuggestionsListBox.SelectedIndex = -1;
                        }                        
                    }
                    else if (_provideWordSuggestions)
                    {
                        // Re-enable suggestions
                        _mainWindow.ThreadManager.SubmitStateEvent(new KxPredictionEventArgs(/*_source.ID, */EWordPredictionEventType.Enable));
                    }
                    break;
                case EWordPredictionEventType.PreviousSuggestion:
                    if (_prediction != null)
                    {
                        if (this.SuggestionsListBox.SelectedIndex > 0)
                        {
                            this.SuggestionsListBox.SelectedIndex--;
                        }
                        else if (_prediction.NumSuggestions > 0)
                        {
                            // Wrap
                            this.SuggestionsListBox.SelectedIndex = _prediction.NumSuggestions - 1;
                        }
                    }
                    break;
                case EWordPredictionEventType.CancelSuggestions:
                    BindSuggestions(null);
                    break;
                case EWordPredictionEventType.InsertSuggestion:                    
                    if (_prediction != null)
                    {
                        InsertSuggestion(_mainWindow);
                    }
                    else if (_provideWordSuggestions)
                    {
                        // Re-enable suggestions
                        _mainWindow.ThreadManager.SubmitStateEvent(new KxPredictionEventArgs(/*_source.ID, */ EWordPredictionEventType.Enable));
                    }
                    break;
            }
        }

        /// <summary>
        /// Insert the selected suggestion
        /// </summary>
        private void InsertSuggestion(IMainWindow parent)
        {
            NamedItem selectedItem = (NamedItem)this.SuggestionsListBox.SelectedItem;
            if (this.Visibility == Visibility.Visible && selectedItem != null)
            {
                string suggestion = selectedItem.Name;

                // If there's a prefix, remove it if it's different to the start of the suggestion
                if (!string.IsNullOrEmpty(_prediction.CurrentWordPrefix))
                {
                    //Trace.WriteLine(string.Format("Prefix: '{0}'", _prediction.CurrentWordPrefix));
                    if (suggestion.StartsWith(_prediction.CurrentWordPrefix))
                    {
                        // Prefix is the start of the suggestion
                        //Trace.WriteLine("Matched prefix");
                        suggestion = suggestion.Substring(_prediction.CurrentWordPrefix.Length);
                    }
                    else
                    {
                        // Prefix doesn't match the suggestion, so delete it
                        //Trace.WriteLine(string.Format("Backspace {0} chars", _prediction.CurrentWordPrefix.Length));
                        KxRepeatKeyEventArgs args = new KxRepeatKeyEventArgs(System.Windows.Forms.Keys.Back, (uint)_prediction.CurrentWordPrefix.Length);
                        parent.ThreadManager.SubmitStateEvent(args);
                    }
                }

                // If there's a suffix, delete it if it's different to the start of the suggestion
                if (!string.IsNullOrEmpty(_prediction.CurrentWordSuffix))
                {
                    //Trace.WriteLine(string.Format("Suffix: '{0}'", _prediction.CurrentWordSuffix));
                    if (suggestion.StartsWith(_prediction.CurrentWordSuffix))
                    {
                        // Suffix is the start of the suggestion portion, so move to the end of the suffix
                        //Trace.WriteLine("Matched suffix");
                        //Trace.WriteLine(string.Format("Move right {0} chars", _prediction.CurrentWordSuffix.Length));
                        KxRepeatKeyEventArgs args = new KxRepeatKeyEventArgs(System.Windows.Forms.Keys.Right, (uint)_prediction.CurrentWordSuffix.Length);
                        parent.ThreadManager.SubmitStateEvent(args);

                        suggestion = suggestion.Substring(_prediction.CurrentWordSuffix.Length);
                    }
                    else
                    {
                        // Suffix doesn't match the suggestion, so delete it
                        //Trace.WriteLine(string.Format("Delete {0} chars", _prediction.CurrentWordSuffix.Length));
                        KxRepeatKeyEventArgs args = new KxRepeatKeyEventArgs(System.Windows.Forms.Keys.Delete, (uint)_prediction.CurrentWordSuffix.Length);
                        parent.ThreadManager.SubmitStateEvent(args);
                    }
                }

                // Type the suggestion (or remaining portion of it)
                if (suggestion.Length != 0)
                {
                    string textToInsert = suggestion;
                    // If we're not editing in the middle of a word (i.e. if there is no suffix), auto-insert a space if reqd
                    if (_autoInsertSpaces && string.IsNullOrEmpty(_prediction.CurrentWordSuffix))
                    {
                        textToInsert += " ";
                    }
                    //Trace.WriteLine(string.Format("Type '{0}'", textToInsert));
                    KxTextEventArgs ev = new KxTextEventArgs(textToInsert);
                    parent.ThreadManager.SubmitStateEvent(ev);
                }
            }
        }

        /// <summary>
        /// Bind the suggestions and select the first item if possible
        /// </summary>
        /// <param name="suggestionsList"></param>
        private void BindSuggestions(WordPredictionResponse predictionResponse)
        {
            _prediction = predictionResponse;
            _suggestionsList.Clear();
            if (_prediction != null && _prediction.NumSuggestions > 0)
            {                
                foreach (NamedItem suggestion in _prediction.SuggestionsList)
                {
                    _suggestionsList.Add(suggestion);
                }
                this.SuggestionsListBox.SelectedIndex = 0;
                this.PrefixTextBlock.Text = !string.IsNullOrEmpty(_prediction.CurrentWordPrefix) ? _prediction.CurrentWordPrefix : "";
                this.SuffixTextBlock.Text = !string.IsNullOrEmpty(_prediction.CurrentWordSuffix) ? _prediction.CurrentWordSuffix : "";
                this.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.Visibility = System.Windows.Visibility.Hidden;
            }
        }
    }
}
