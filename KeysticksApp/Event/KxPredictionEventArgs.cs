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
using Keysticks.Core;

namespace Keysticks.Event
{
    /// <summary>
    /// Word prediction event data
    /// </summary>
    public class KxPredictionEventArgs : KxEventArgs
    {
        // Fields
        public EWordPredictionEventType PredictionEventType;
        public WordPredictionResponse PredictionResponse;

        // Properties
        public override EEventType EventType { get { return EEventType.WordPrediction; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="suggestionsList"></param>
        public KxPredictionEventArgs(EWordPredictionEventType predictionEventType)
            : base()
        {
            PredictionEventType = predictionEventType;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="args"></param>
        public KxPredictionEventArgs(KxPredictionEventArgs args)
            :base(args)
        {
            PredictionEventType = args.PredictionEventType;
            if (args.PredictionResponse != null)
            {
                PredictionResponse = new WordPredictionResponse(args.PredictionResponse);
            }
        }

        /// <summary>
        /// Get string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringUtils utils = new StringUtils();
            string str = string.Format("{0} {1}", 
                utils.PredictionEventToString(PredictionEventType), 
                PredictionResponse != null ? PredictionResponse.ToString() : "");

            return str;
        }
    }
}
