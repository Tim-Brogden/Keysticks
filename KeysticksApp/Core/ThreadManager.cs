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
using System.Collections.Generic;
using System.Threading;
using Keysticks.Config;
using Keysticks.Event;

namespace Keysticks.Core
{
    /// <summary>
    /// Manages the state manager and word prediction threads and communication between them and the UI
    /// </summary>
    public class ThreadManager : IThreadManager
    {
        // Thread managers
        private StateManager _stateManager;
        private WordPredictionEngine _predictionEngine;

        // Inter-thread comms
        private EventReportingBuffer _stateEventBuffer = new EventReportingBuffer();
        private EventReportingBuffer _uiEventBuffer = new EventReportingBuffer();
        private EventReportingBuffer _predictionEventBuffer = new EventReportingBuffer();
        private Profile _stateManagerProfile;
        private AppConfig _stateManagerAppConfig;
        private object _lockStateManager = new object();
        private bool _stateManagerConfigChanged = false;
        private AppConfig _predictionManagerAppConfig;
        private object _lockPredictionManager = new object();
        private bool _predictionManagerConfigChanged = false;

        // Threading
        private static Thread _stateManagerThread;
        private static Thread _predictionThread;
        private delegate void UpdateCallback();
        private bool _continueStateManager = false;
        private bool _continueWordPrediction = false;

        // Properties
        public bool ContinueStateManager { get { return _continueStateManager; } set { _continueStateManager = value; } }
        public bool ContinueWordPrediction { get { return _continueWordPrediction; } set { _continueWordPrediction = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public ThreadManager()
        {
        }

        /// <summary>
        /// Set a new app config
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            lock (_lockPredictionManager)
            {
                _predictionManagerAppConfig = new AppConfig(appConfig);
                _predictionManagerConfigChanged = true;
            }

            lock (_lockStateManager)
            {
                _stateManagerAppConfig = new AppConfig(appConfig);
                _stateManagerConfigChanged = true;
            }
        }

        /// <summary>
        /// Set a new configuration profile and logical state
        /// </summary>
        /// <param name="profile"></param>
        public void SetProfile(Profile profile)
        {
            // Start the word prediction thread if required
            if (!_continueWordPrediction && profile.HasActionsOfType(EActionType.WordPrediction))
            {
                _continueWordPrediction = true;

                ThreadStart threadStart = new ThreadStart(RunWordPrediction);
                _predictionThread = new Thread(threadStart);
                _predictionThread.Start();
            }

            lock (_lockStateManager)
            {
                _stateManagerProfile = new Profile(profile);
                _stateManagerConfigChanged = true;
            }

            // Start state manager thread if required
            if (!_continueStateManager)
            {
                _continueStateManager = true;

                ThreadStart threadStart = new ThreadStart(RunStateManager);
                _stateManagerThread = new Thread(threadStart);
                _stateManagerThread.Start();
            }
        }
        
        /// <summary>
        /// Allow the state manager to check for profile and logical state updates
        /// </summary>
        /// <param name="stateHandler"></param>
        public void ReceiveStateManagerConfig(StateManager stateHandler)
        {
            if (_stateManagerConfigChanged)
            {
                lock (_lockStateManager)
                {
                    if (_stateManagerAppConfig != null)
                    {
                        stateHandler.SetAppConfig(_stateManagerAppConfig);
                        _stateManagerAppConfig = null;
                    }
                    if (_stateManagerProfile != null)
                    {
                        stateHandler.SetProfile(_stateManagerProfile);
                        _stateManagerProfile = null;
                    }
                    _stateManagerConfigChanged = false;
                }
            }
        }

        /// <summary>
        /// Allow the prediction manager to receive its configuration
        /// </summary>
        /// <param name="predictionEngine"></param>
        public void ReceivePredictionManagerConfig(WordPredictionEngine predictionEngine)
        {
            if (_predictionManagerConfigChanged)
            {
                lock (_lockPredictionManager)
                {
                    predictionEngine.SetAppConfig(_predictionManagerAppConfig);
                    _predictionManagerConfigChanged = false;
                }
            }
        }

        /// <summary>
        /// Stop polling input sources
        /// </summary>
        public void StopThreads()
        {
            _continueStateManager = false;
            _continueWordPrediction = false;
        }

        /// <summary>
        /// Start the word prediction thread
        /// </summary>
        private void RunWordPrediction()
        {
            // This blocks until the word prediction thread exits
            _predictionEngine = new WordPredictionEngine(this);
            _predictionEngine.Run();
        }

        /// <summary>
        /// Start the input polling (state manager) thread
        /// </summary>
        private void RunStateManager()
        {
            // This blocks until the state handler exits
            _stateManager = new StateManager(this);
            _stateManager.Run();
        }

        /// <summary>
        /// Submit an event to the UI
        /// </summary>
        public void SubmitUIEvent(KxEventArgs report)
        {
            _uiEventBuffer.SubmitEvent(report);
        }

        /// <summary>
        /// Receive new UI events
        /// </summary>
        public List<KxEventArgs> ReceiveUIEvents()
        {
            return _uiEventBuffer.ReceiveEvents();
        }

        /// <summary>
        /// Submit an event to the state thread
        /// </summary>
        public void SubmitStateEvent(KxEventArgs report)
        {
            _stateEventBuffer.SubmitEvent(report);
        }

        /// <summary>
        /// Receive new state events
        /// </summary>
        public List<KxEventArgs> ReceiveStateEvents()
        {
            return _stateEventBuffer.ReceiveEvents();
        }

        /// <summary>
        /// Submit an event to the prediction thread
        /// </summary>
        public void SubmitPredictionEvent(KxEventArgs report)
        {
            _predictionEventBuffer.SubmitEvent(report);
        }

        /// <summary>
        /// Receive new prediction events
        /// </summary>
        public List<KxEventArgs> ReceivePredictionEvents()
        {
            return _predictionEventBuffer.ReceiveEvents();
        }
    }
}
