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
using System.IO;
using System.Reflection;
using WordPredictorLib;

namespace WordPredictionDemo
{
    /// <summary>
    /// Wrapper for the WordPredictor COM component
    /// </summary>
    class WordPredictor : IDisposable
    {
        public const string DefaultBaseDirectory = "..\\..\\..\\data\\base";
        public const string DefaultInstalledDictionary = "enggb";
        private const int REQUEST_RESET_INPUT = 10;
        private const int REQUEST_INSERT_STRING = 11;
        private const int REQUEST_MOVE_CURSOR = 12;
        private const int REQUEST_REMOVE_CHARS = 13;
        private const int REQUEST_INSERT_SUGGESTION = 14;
        private const int REQUEST_CONFIGURE_LEARNING = 15;
        private const int REQUEST_SET_CURSOR = 16;
        private const int REQUEST_GET_SUGGESTIONS = 17;
        private const int REQUEST_INSTALL_PACKAGES = 18;
        private const int REQUEST_UNINSTALL_PACKAGES = 19;
        private const int REQUEST_SET_ACTIVE_DICTIONARIES = 20;

        private const int RESPONSE_ERROR_UNRECOGNISED_MSG_TYPE = 200;
        private const int RESPONSE_ERROR_BUFFER_OVERFLOW = 201;
        private const int RESPONSE_ERROR_RESET = 210;
        private const int RESPONSE_ERROR_INSERT_STRING = 211;
        private const int RESPONSE_ERROR_MOVE_CURSOR = 212;
        private const int RESPONSE_ERROR_REMOVE_CHARS = 213;
        private const int RESPONSE_ERROR_INSERT_SUGGESTION = 214;
        private const int RESPONSE_ERROR_CONFIGURE_LEARNING = 215;
        private const int RESPONSE_ERROR_SET_CURSOR = 216;
        private const int RESPONSE_ERROR_GET_SUGGESTIONS = 217;
        private const int RESPONSE_ERROR_INSTALL_PACKAGES = 218;
        private const int RESPONSE_ERROR_UNINSTALL_PACKAGES = 219;
        private const int RESPONSE_ERROR_SET_ACTIVE_DICTIONARIES = 220;

        private string _basePath;
        private IWordPredictorCom _framework;
        private List<string> _currentSuggestionsList;
        private string _currentPrefix;
        private string _currentSuffix;
        private int _currentIndex;
        private string[] _dummyArray = new string[0];

        // Properties
        public string BasePath { get { return _basePath; } }
        public List<string> CurrentSuggestionsList { get { return _currentSuggestionsList; } }
        public string CurrentPrefix { get { return _currentPrefix; } }
        public string CurrentSuffix { get { return _currentSuffix; } }
        public int CurrentIndex { get { return _currentIndex; } }

        // Events
        public event EventHandler SuggestionsReceived;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="suggestionsList"></param>
        public WordPredictor()
        {
            _currentSuggestionsList = new List<string>();
            _currentPrefix = null;
            _currentSuffix = null;
            _currentIndex = 0;

            string exeDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            _basePath = Path.Combine(exeDir, DefaultBaseDirectory);

            WordPredictorCom co = new WordPredictorCom();            
            _framework = (IWordPredictorCom)co;
            _framework.Create(_basePath);
        }

        public void Dispose()
        {
            _framework.Destroy();
        }

        /// <summary>
        /// Insert a string into the word prediction buffer
        /// </summary>
        /// <param name="text"></param>
        public void SendText(string text)
        {
            byte[] requestMeta = new byte[] {
                REQUEST_INSERT_STRING,
                REQUEST_GET_SUGGESTIONS
            };
            string[] requestData = new string[] { text };
            string[] responseData = new string[0];

            int result = _framework.ProcessRequest(requestMeta, requestData, ref responseData);
            HandleResponse(result, responseData);

            _currentIndex += text.Length;
        }

        /// <summary>
        /// Reset the word prediction buffer
        /// </summary>
        public void SendReset()
        {
            byte[] requestMeta = new byte[] {
                REQUEST_RESET_INPUT,
                REQUEST_GET_SUGGESTIONS
            };
            string[] responseData = new string[0];

            int result = _framework.ProcessRequest(requestMeta, _dummyArray, ref responseData);
            HandleResponse(result, responseData);

            _currentIndex = 0;
        }

        /// <summary>
        /// Backspace in the word prediction buffer
        /// </summary>
        /// <param name="count"></param>
        public void SendBackspace(uint count)
        {
            byte[] requestMeta = new byte[] {
                REQUEST_REMOVE_CHARS,
                REQUEST_GET_SUGGESTIONS,
                (byte)count,             // Backspace
                0                       // Delete
            };
            string[] responseData = new string[0];

            int result = _framework.ProcessRequest(requestMeta, _dummyArray, ref responseData);
            HandleResponse(result, responseData);

            _currentIndex -= (int)count;
        }

        /// <summary>
        /// Delete in the word prediction buffer
        /// </summary>
        /// <param name="count"></param>
        public void SendDelete(uint count)
        {
            byte[] requestMeta = new byte[] {
                REQUEST_REMOVE_CHARS,
                REQUEST_GET_SUGGESTIONS,
                0,                      // Backspace
                (byte)count             // Delete
            };
            string[] responseData = new string[0];

            int result = _framework.ProcessRequest(requestMeta, _dummyArray, ref responseData);
            HandleResponse(result, responseData);
        }

        /// <summary>
        /// Move left in the word prediction buffer
        /// </summary>
        /// <param name="count"></param>
        public void SendLeftCursor(uint count)
        {
            byte[] requestMeta = new byte[] {
                REQUEST_MOVE_CURSOR,
                REQUEST_GET_SUGGESTIONS,
                (byte)count,              // positions left
                0                         // positions right
            };
            string[] responseData = new string[0];

            int result = _framework.ProcessRequest(requestMeta, _dummyArray, ref responseData);
            HandleResponse(result, responseData);

            _currentIndex -= (int)count;
        }

        /// <summary>
        /// Move right in the word prediction buffer
        /// </summary>
        /// <param name="count"></param>
        public void SendRightCursor(uint count)
        {
            byte[] requestMeta = new byte[] {
                REQUEST_MOVE_CURSOR,
                REQUEST_GET_SUGGESTIONS,
                0,                  // positions left
                (byte)count         // positions right
            };
            string[] responseData = new string[0];

            int result = _framework.ProcessRequest(requestMeta, _dummyArray, ref responseData);
            HandleResponse(result, responseData);

            _currentIndex += (int)count;
        }

        /// <summary>
        /// Set the cursor position in the word prediction buffer
        /// </summary>
        /// <param name="index"></param>
        public void SendSetCursor(uint index)
        {
            byte[] requestMeta = new byte[] {
                REQUEST_SET_CURSOR,
                REQUEST_GET_SUGGESTIONS,
                (byte)index            // cursor index
            };
            string[] responseData = new string[0];

            //string[] testInput = new string[] { "Test", "Test 2" };
            //int testResult = _framework.TestIn(testInput);

            int result = _framework.ProcessRequest(requestMeta, _dummyArray, ref responseData);
            HandleResponse(result, responseData);

            _currentIndex = (int)index;
        }

        /// <summary>
        /// Retrieve the current list of suggestions from the word prediction server
        /// </summary>
        public void SendGetSuggestions()
        {
            byte[] requestMeta = new byte[] {
                REQUEST_GET_SUGGESTIONS
            };
            string[] responseData = new string[0];

            int result = _framework.ProcessRequest(requestMeta, _dummyArray, ref responseData);
            HandleResponse(result, responseData);
        }

        /// <summary>
        /// Enable or disable learning
        /// </summary>
        /// <param name="enable"></param>
        public void SendConfigureLearning(bool enable)
        {
            byte[] requestMeta = new byte[] {
                REQUEST_CONFIGURE_LEARNING,
                (byte)(enable ? 1 : 0)
            };
            string[] responseData = new string[0];

            int result = _framework.ProcessRequest(requestMeta, _dummyArray, ref responseData);
            HandleResponse(result, responseData);
        }

        /// <summary>
        /// Send a request to install any new packages
        /// </summary>
        public void SendInstallPackages()
        {
            byte[] requestMeta = new byte[] {
                REQUEST_INSTALL_PACKAGES
            };
            string[] responseData = new string[0];

            int result = _framework.ProcessRequest(requestMeta, _dummyArray, ref responseData);
            HandleResponse(result, responseData);
        }

        /// <summary>
        /// Send a request to uninstall all packages
        /// </summary>
        public void SendUninstallPackages()
        {
            byte[] requestMeta = new byte[] {
                REQUEST_UNINSTALL_PACKAGES
            };
            string[] responseData = new string[0];

            int result = _framework.ProcessRequest(requestMeta, _dummyArray, ref responseData);
            HandleResponse(result, responseData);
        }

        /// <summary>
        /// Set the list of active dictionaries in priority order (highest priority first)
        /// </summary>
        /// <param name="activeDictionaryList">E.g. enggb,frefr,lavlv</param>
        public void SendActiveDictionariesList(string activeDictionaryList)
        {
            byte[] requestMeta = new byte[] {
                REQUEST_SET_ACTIVE_DICTIONARIES
            };
            string[] requestData = new string[] { activeDictionaryList };
            string[] responseData = new string[0];

            int result = _framework.ProcessRequest(requestMeta, requestData, ref responseData);
            HandleResponse(result, responseData);
        }

        /// <summary>
        /// Handle the response of the prediction component
        /// </summary>
        private void HandleResponse(int responseCode, string[] responseData)
        {
            if (responseCode == 0)
            {
                RefreshSuggestions(responseData);
            }
            else
            {
                HandleError(responseCode);
            }
        }

        private void HandleError(int errorCode)
        {
            switch (errorCode)
            {
                case RESPONSE_ERROR_BUFFER_OVERFLOW:
                    throw new Exception("Word prediction error - buffer overflow");
                case RESPONSE_ERROR_GET_SUGGESTIONS:
                    throw new Exception("Word prediction error while getting suggestions");
                case RESPONSE_ERROR_INSERT_STRING:
                    throw new Exception("Word prediction error while inserting text");
                case RESPONSE_ERROR_INSERT_SUGGESTION:
                    throw new Exception("Word prediction error while inserting suggestion");
                case RESPONSE_ERROR_MOVE_CURSOR:
                    throw new Exception("Word prediction error while moving cursor");
                case RESPONSE_ERROR_REMOVE_CHARS:
                    throw new Exception("Word prediction error while deleting text");
                case RESPONSE_ERROR_RESET:
                    throw new Exception("Word prediction error while resetting buffer");
                case RESPONSE_ERROR_CONFIGURE_LEARNING:
                    throw new Exception("Word prediction error while configuring learning");
                case RESPONSE_ERROR_SET_CURSOR:
                    throw new Exception("Word prediction error while setting cursor position");
                case RESPONSE_ERROR_INSTALL_PACKAGES:
                    throw new Exception("Word prediction error while installing new packages");
                case RESPONSE_ERROR_UNINSTALL_PACKAGES:
                    throw new Exception("Word prediction error while uninstalling packages");
                case RESPONSE_ERROR_SET_ACTIVE_DICTIONARIES:
                    throw new Exception("Word prediction error while setting active dictionaries");
                case RESPONSE_ERROR_UNRECOGNISED_MSG_TYPE:
                    throw new Exception("Word prediction error - unrecognised message type");
            }
        }

        /// <summary>
        /// Extract word suggestions list from a buffer
        /// </summary>
        /// <param name="receiveBuffer"></param>
        /// <param name="bytesRead"></param>
        private void RefreshSuggestions(string[] responseData)
        {
            // Extract suggestions
            if (responseData.Length > 1)
            {
                _currentPrefix = responseData[0];
                _currentSuffix = responseData[1];

                _currentSuggestionsList.Clear();
                for (int i = 2; i < responseData.Length; i++) 
                {
                    _currentSuggestionsList.Add(responseData[i]);
                }

                // Report to the owner
                if (SuggestionsReceived != null)
                {
                    SuggestionsReceived(this, new EventArgs());
                }
            }
        }        
    }
}
