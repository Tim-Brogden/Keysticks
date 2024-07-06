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
using System.Threading;
using System.IO;
using System.Text;
using Keysticks.Config;
using Keysticks.Event;
using Keysticks.Sys;

namespace Keysticks.Core
{
    /// <summary>
    /// Proxy for sending and receiving messages to / from the word prediction component
    /// </summary>
    public class WordPredictionEngine
    {
        // Fields
        private ThreadManager _parent;
        private AppConfig _appConfig = new AppConfig();
        private IWordPredictorCom _framework;
        private bool _predictionsEnabled = false;
        private int _pollingIntervalMS = Constants.DefaultWordPredictionPollingIntervalMS;
        private string[] _dummyArray = new string[0];

        /// <summary>
        /// Constructor
        /// </summary>
        public WordPredictionEngine(ThreadManager parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Configure the prediction engine
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            try
            {
                // Configure word prediction server if it's enabled and settings have changed
                _predictionsEnabled = appConfig.GetBoolVal(Constants.ConfigEnableWordPrediction, Constants.DefaultEnableWordPrediction);
                if (_predictionsEnabled)
                {
                    if (_framework == null)
                    {
                        string basePath = Path.Combine(AppConfig.CommonAppDataDir, "base");

                        _framework = (IWordPredictorCom)new WordPredictorCom();
                        _framework.Create(basePath);
                    }

                    string predictionLanguagesList = appConfig.GetStringVal(Constants.ConfigWordPredictionInstalledLanguages, Constants.DefaultWordPredictionInstalledLanguages);
                    if (predictionLanguagesList != _appConfig.GetStringVal(Constants.ConfigWordPredictionInstalledLanguages, Constants.DefaultWordPredictionInstalledLanguages))
                    {
                        // Install / uninstall packages
                        SendInstallPackages();

                        // Set active dictionaries
                        ConfigureLanguages(predictionLanguagesList);
                    }

                    bool learnNewWords = appConfig.GetBoolVal(Constants.ConfigLearnNewWords, Constants.DefaultLearnNewWords);
                    if (learnNewWords != _appConfig.GetBoolVal(Constants.ConfigLearnNewWords, Constants.DefaultLearnNewWords))
                    {
                        ConfigureLearning(learnNewWords);
                    }

                    // Store new config
                    _appConfig = appConfig;
                }
            }
            catch (Exception ex)
            {
                _parent.SubmitUIEvent(new KxErrorMessageEventArgs(Properties.Resources.E_ConfigureWordPrediction, ex));
            }
        }

        /// <summary>
        /// Poll the word prediction server
        /// </summary>
        public void Run()
        {
            // Read and ignore any events before the thread was started
            List<KxEventArgs> eventsReceived = _parent.ReceivePredictionEvents();
            while (_parent.ContinueWordPrediction)
            {
                // Check for config updates
                _parent.ReceivePredictionManagerConfig(this);

                // Handle any events submitted from other threads
                eventsReceived = _parent.ReceivePredictionEvents();
                if (eventsReceived != null && _predictionsEnabled)
                {
                    try
                    {
                        for (int i = 0; i < eventsReceived.Count; i++)
                        {
                            KxEventArgs args = eventsReceived[i];
                            switch (args.EventType)
                            {
                                case EEventType.Text:
                                    ProcessTextEvent((KxTextEventArgs)args);
                                    break;
                                case EEventType.Key:
                                    // Batch up multiple presses of the same key into a repeat key event
                                    KxKeyEventArgs keyArgs = (KxKeyEventArgs)args;
                                    System.Windows.Forms.Keys key = keyArgs.Key;
                                    uint count = 1;
                                    while (++i < eventsReceived.Count)
                                    {
                                        args = eventsReceived[i];
                                        if (args.EventType == EEventType.Key &&
                                            ((KxKeyEventArgs)args).Key == keyArgs.Key)
                                        {
                                            count++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    i--;
                                    ProcessRepeatKeyEvent(key, count);
                                    break;
                                case EEventType.RepeatKey:
                                    KxRepeatKeyEventArgs repArgs = (KxRepeatKeyEventArgs)args;
                                    ProcessRepeatKeyEvent(repArgs.Key, repArgs.Count);
                                    break;
                                case EEventType.LanguagePackages:
                                    ProcessLanguagePackagesEvent();
                                    break;
                            }                            
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                Thread.Sleep(_pollingIntervalMS);
            }

            // Clean up before exiting
            DestroyFramework();
        }

        /// <summary>
        /// Release COM component resources
        /// </summary>
        private void DestroyFramework()
        {
            if (_framework != null)
            {
                _framework.Destroy();
                _framework = null;
            }
        }

        /// <summary>
        /// Synchronise the word prediction server after language packages have been added or deleted
        /// </summary>
        private void ProcessLanguagePackagesEvent()
        {
            // Install / uninstall packages
            SendInstallPackages();

            // Configure active languages
            string predictionLanguagesList = _appConfig.GetStringVal(Constants.ConfigWordPredictionInstalledLanguages, Constants.DefaultWordPredictionInstalledLanguages);
            ConfigureLanguages(predictionLanguagesList);            
        }

        /// <summary>
        /// Process a repeat key event
        /// </summary>
        /// <param name="args"></param>
        private void ProcessRepeatKeyEvent(System.Windows.Forms.Keys key, uint count)
        {
            // Interpret special keys
            switch (key)
            {
                case System.Windows.Forms.Keys.None:
                    SendReset();
                    break;
                case System.Windows.Forms.Keys.Left:
                    SendLeftCursor(count);
                    break;
                case System.Windows.Forms.Keys.Right:
                    SendRightCursor(count);
                    break;
                case System.Windows.Forms.Keys.Back:
                    SendBackspace(count);
                    break;
                case System.Windows.Forms.Keys.Delete:
                    SendDelete(count);
                    break;                
            }
        }

        /// <summary>
        /// Process a text event
        /// </summary>
        /// <param name="args"></param>
        private void ProcessTextEvent(KxTextEventArgs args)
        {
            byte[] requestMeta = new byte[] {
                Constants.REQUEST_INSERT_STRING,
                Constants.REQUEST_GET_SUGGESTIONS
            };
            string[] requestData = new string[] { args.Text };
            string[] responseData = new string[0];

            int result = _framework.ProcessRequest(requestMeta, requestData, ref responseData);
            HandleResponse(result, responseData);                        
        }        

        /// <summary>
        /// Reset the word prediction buffer
        /// </summary>
        public void SendReset()
        {
            byte[] requestMeta = new byte[] {
                Constants.REQUEST_RESET_INPUT,
                Constants.REQUEST_GET_SUGGESTIONS
            };
            string[] responseData = new string[0];

            int result = _framework.ProcessRequest(requestMeta, _dummyArray, ref responseData);
            HandleResponse(result, responseData);
        }

        /// <summary>
        /// Backspace in the word prediction buffer
        /// </summary>
        /// <param name="count"></param>
        public void SendBackspace(uint count)
        {
            byte[] requestMeta = new byte[] {
                Constants.REQUEST_REMOVE_CHARS,
                Constants.REQUEST_GET_SUGGESTIONS,
                (byte)count,             // Backspace
                0                        // Delete
            };
            string[] responseData = new string[0];

            int result = _framework.ProcessRequest(requestMeta, _dummyArray, ref responseData);
            HandleResponse(result, responseData);
        }

        /// <summary>
        /// Delete in the word prediction buffer
        /// </summary>
        /// <param name="count"></param>
        public void SendDelete(uint count)
        {
            byte[] requestMeta = new byte[] {
                Constants.REQUEST_REMOVE_CHARS,
                Constants.REQUEST_GET_SUGGESTIONS,
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
                Constants.REQUEST_MOVE_CURSOR,
                Constants.REQUEST_GET_SUGGESTIONS,
                (byte)count,              // positions left
                0                         // positions right
            };
            string[] responseData = new string[0];

            int result = _framework.ProcessRequest(requestMeta, _dummyArray, ref responseData);
            HandleResponse(result, responseData);
        }

        /// <summary>
        /// Move right in the word prediction buffer
        /// </summary>
        /// <param name="count"></param>
        public void SendRightCursor(uint count)
        {
            byte[] requestMeta = new byte[] {
                Constants.REQUEST_MOVE_CURSOR,
                Constants.REQUEST_GET_SUGGESTIONS,
                0,                  // positions left
                (byte)count         // positions right
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
                Constants.REQUEST_INSTALL_PACKAGES
            };
            string[] responseData = new string[0];

            int result = _framework.ProcessRequest(requestMeta, _dummyArray, ref responseData);
            HandleResponse(result, responseData);
        }        

        /// <summary>
        /// Tell the word prediction server which languages to activate
        /// </summary>
        /// <param name="installedLanguagesStr"></param>
        private void ConfigureLanguages(string installedLanguagesStr)
        {
            StringBuilder sb = new StringBuilder();
            StringUtils utils = new StringUtils();
            NamedItemList langList = utils.LanguageListFromString(installedLanguagesStr);

            bool isFirst = true;
            foreach (OptionalNamedItem langItem in langList)
            {
                if (langItem.IsEnabled)
                {
                    if (!isFirst)
                    {
                        sb.Append(',');
                    }
                    isFirst = false;

                    ELanguageCode langCode = (ELanguageCode)langItem.ID;
                    sb.Append(langCode.ToString());
                }
            }
            string activeDictionaryList = sb.ToString();

            byte[] requestMeta = new byte[] {
                Constants.REQUEST_SET_ACTIVE_DICTIONARIES
            };
            string[] requestData = new string[] { activeDictionaryList };
            string[] responseData = new string[0];

            int result = _framework.ProcessRequest(requestMeta, requestData, ref responseData);
            HandleResponse(result, responseData);
        }

        /// <summary>
        /// Enable or disable word learning
        /// </summary>
        /// <param name="enable"></param>
        public void ConfigureLearning(bool enable)
        {
            byte[] requestMeta = new byte[] {
                Constants.REQUEST_CONFIGURE_LEARNING,
                (byte)(enable ? 1 : 0)
            };
            string[] responseData = new string[0];

            int result = _framework.ProcessRequest(requestMeta, _dummyArray, ref responseData);
            HandleResponse(result, responseData);
        }

        /// <summary>
        /// Handle the response from the word prediction server
        /// </summary>
        private void HandleResponse(int responseCode, string[] responseData)
        {
            if (responseCode == 0)
            {
                HandleSuggestions(responseData);
            }
            else
            {
                HandleError(responseCode);
            }
        }

        /// <summary>
        /// Extract word suggestions list from a buffer
        /// </summary>
        /// <param name="_receiveBuffer"></param>
        /// <param name="bytesRead"></param>
        private void HandleSuggestions(string[] responseData)
        {
            if (responseData.Length > 1)
            {
                string prefix = (string)responseData[0];
                string suffix = (string)responseData[1];

                NamedItemList suggestionsList = null;
                if (responseData.Length > 2)
                {
                    int suggestionIndex = 0;
                    suggestionsList = new NamedItemList();
                    for (int i = 2; i < responseData.Length; i++)
                    {
                        suggestionsList.Add(new NamedItem(suggestionIndex++, responseData[i]));
                    }
                }

                WordPredictionResponse predictionResponse = new WordPredictionResponse(suggestionsList, prefix, suffix);

                // Send suggestions to UI
                KxPredictionEventArgs args = new KxPredictionEventArgs(EWordPredictionEventType.SuggestionsList);
                args.PredictionResponse = predictionResponse;
                _parent.SubmitUIEvent(args);
            }
        }

        /// <summary>
        /// Handle an error
        /// </summary>
        /// <param name="errorCode"></param>
        private void HandleError(int errorCode)
        {
            switch (errorCode)
            {
                case Constants.RESPONSE_ERROR_INSTALL_PACKAGES:
                    _parent.SubmitUIEvent(new KxErrorMessageEventArgs(Properties.Resources.E_WordPredictionInstallLanguages, null));
                    break;
                case Constants.RESPONSE_ERROR_UNINSTALL_PACKAGES:
                    _parent.SubmitUIEvent(new KxErrorMessageEventArgs(Properties.Resources.E_WordPredictionUninstallLanguages, null));
                    break;
                case Constants.RESPONSE_ERROR_SET_ACTIVE_DICTIONARIES:
                    _parent.SubmitUIEvent(new KxErrorMessageEventArgs(Properties.Resources.E_WordPredictionSetDictionaries, null));
                    break;
                case Constants.RESPONSE_ERROR_CONFIGURE_LEARNING:
                    _parent.SubmitUIEvent(new KxErrorMessageEventArgs(Properties.Resources.E_WordPredictionConfigureLearning, null));
                    break;
            }
        }
    }
}
