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
    /// Stores the app config colour and opacity settings
    /// </summary>
    public class ColourScheme
    {
        // Fields
        private double _currentControlsOpacity;
        private double _interactiveControlsOpacity;
        private Dictionary<int, PlayerColourScheme> _playerColourSchemes;

        // Properties
        public double CurrentControlsOpacity { get { return _currentControlsOpacity; } }
        public double InteractiveControlsOpacity { get { return _interactiveControlsOpacity; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public ColourScheme()
        {
            _currentControlsOpacity = 0.01 * Constants.DefaultWindowOpacityPercent;
            _interactiveControlsOpacity = 0.01 * Constants.DefaultWindowOpacityPercent;
            InitialisePlayerColourSchemes();
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="currentControlsOpacity"></param>
        /// <param name="interactiveControlsOpacity"></param>
        /// <param name="cellColour"></param>
        /// <param name="alternateCellColour"></param>
        /// <param name="highlightColour"></param>
        /// <param name="selectionColour"></param>
        public ColourScheme(double currentControlsOpacity,
                            double interactiveControlsOpacity)
        {
            _currentControlsOpacity = currentControlsOpacity;
            _interactiveControlsOpacity = interactiveControlsOpacity;
            InitialisePlayerColourSchemes();
        }

        /// <summary>
        /// Initialise child data
        /// </summary>
        private void InitialisePlayerColourSchemes()
        {
            _playerColourSchemes = new Dictionary<int, PlayerColourScheme>();
            _playerColourSchemes[Constants.ID1] = Constants.DefaultPlayer1Colours;
            _playerColourSchemes[Constants.ID2] = Constants.DefaultPlayer2Colours;
            _playerColourSchemes[Constants.ID3] = Constants.DefaultPlayer3Colours;
            _playerColourSchemes[Constants.ID4] = Constants.DefaultPlayer4Colours;
        }

        /// <summary>
        /// Get the colour scheme for a player
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PlayerColourScheme GetPlayerColours(int id)
        {
            PlayerColourScheme playerColours = null;
            if (_playerColourSchemes.ContainsKey(id))
            {
                playerColours = _playerColourSchemes[id];
            }

            return playerColours;
        }

        /// <summary>
        /// Set the colour scheme for a player
        /// </summary>
        /// <param name="id"></param>
        /// <param name="playerColours"></param>
        public void SetPlayerColours(int id, PlayerColourScheme playerColours)
        {
            _playerColourSchemes[id] = playerColours;
        }
    }
}
