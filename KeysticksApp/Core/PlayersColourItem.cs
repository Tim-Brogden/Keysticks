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

namespace Keysticks.Core
{
    /// <summary>
    /// Stores a colour name for each player
    /// </summary>
    public class PlayersColourItem : NamedItem
    {
        // Fields
        private string _player1Colour = "";
        private string _player2Colour = "";
        private string _player3Colour = "";
        private string _player4Colour = "";

        // Properties
        public string Player1Colour
        {
            get { return _player1Colour; }
            set
            {
                if (_player1Colour != value)
                {
                    _player1Colour = value;
                    NotifyPropertyChanged("Player1Colour");
                }
            }
        }
        public string Player2Colour
        {
            get { return _player2Colour; }
            set
            {
                if (_player2Colour != value)
                {
                    _player2Colour = value;
                    NotifyPropertyChanged("Player2Colour");
                }
            }
        }
        public string Player3Colour
        {
            get { return _player3Colour; }
            set
            {
                if (_player3Colour != value)
                {
                    _player3Colour = value;
                    NotifyPropertyChanged("Player3Colour");
                }
            }
        }
        public string Player4Colour
        {
            get { return _player4Colour; }
            set
            {
                if (_player4Colour != value)
                {
                    _player4Colour = value;
                    NotifyPropertyChanged("Player4Colour");
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PlayersColourItem()
            : base()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public PlayersColourItem(int id, string name)
            : base(id, name)
        {
        }
    }       
}
