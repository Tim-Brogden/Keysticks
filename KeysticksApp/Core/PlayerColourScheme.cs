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
    /// Stores the app config colour settings for a player
    /// </summary>
    public class PlayerColourScheme
    {
        // Fields
        private string _cellColour;
        private string _alternateCellColour;
        private string _highlightColour;
        private string _selectionColour;

        // Properties
        public string CellColour { get { return _cellColour; } }
        public string AlternateCellColour { get { return _alternateCellColour; } }
        public string HighlightColour { get { return _highlightColour; } }
        public string SelectionColour { get { return _selectionColour; } }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="cellColour"></param>
        /// <param name="alternateCellColour"></param>
        /// <param name="highlightColour"></param>
        /// <param name="selectionColour"></param>
        public PlayerColourScheme(string cellColour,
                                    string alternateCellColour,
                                    string highlightColour,
                                    string selectionColour)
        {
            _cellColour = cellColour;
            _alternateCellColour = alternateCellColour;
            _highlightColour = highlightColour;
            _selectionColour = selectionColour;
        }
    }
}
