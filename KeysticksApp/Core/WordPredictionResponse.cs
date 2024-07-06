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

namespace Keysticks.Core
{
    /// <summary>
    /// Stores a response from the word prediction server
    /// </summary>
    public class WordPredictionResponse
    {
        // Fields
        private NamedItemList _suggestionsList;
        private string _currentWordPrefix;
        private string _currentWordSuffix;

        // Properties
        public NamedItemList SuggestionsList { get { return _suggestionsList; } }
        public string CurrentWordPrefix { get { return _currentWordPrefix; } }
        public string CurrentWordSuffix { get { return _currentWordSuffix; } }
        public int NumSuggestions { get { return _suggestionsList != null ? _suggestionsList.Count : 0; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        public WordPredictionResponse()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="suggestionsList"></param>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        public WordPredictionResponse(NamedItemList suggestionsList, string prefix, string suffix)
        {
            _suggestionsList = suggestionsList;
            _currentWordPrefix = prefix;
            _currentWordSuffix = suffix;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="response"></param>
        public WordPredictionResponse(WordPredictionResponse response)
        {
            _suggestionsList = response._suggestionsList;
            _currentWordPrefix = response._currentWordPrefix;
            _currentWordSuffix = response._currentWordSuffix;
        }

        /// <summary>
        /// Empty the suggestions data
        /// </summary>
        public void Clear()
        {
            _suggestionsList = null;
            _currentWordPrefix = null;
            _currentWordSuffix = null;
        }

        /// <summary>
        /// Get string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = string.Format("{0}- {1} -{2}",
                _currentWordPrefix != null ? _currentWordPrefix : "",
                _suggestionsList != null ? _suggestionsList.ToString() : "",
                _currentWordSuffix != null ? _currentWordSuffix : "");
            return str;
        }
    }
}
