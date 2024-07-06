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
using System.Xml;
using Keysticks.Core;
using Keysticks.Event;

namespace Keysticks.Actions
{
    /// <summary>
    /// Word prediction action
    /// </summary>
    public class WordPredictionAction : BaseAction
    {
        // Fields
        private EWordPredictionEventType _predictionEventType = EWordPredictionEventType.InsertSuggestion;

        // Properties
        public EWordPredictionEventType PredictionEventType { get { return _predictionEventType; } set { _predictionEventType = value; } }

        /// <summary>
        /// Return the type of action
        /// </summary>
        public override EActionType ActionType
        {
            get 
            {
                return EActionType.WordPrediction;
            }
        }

        /// <summary>
        /// Return the name of the action
        /// </summary>
        /// <returns></returns>
        public override string Name
        {
            get
            {
                StringUtils utils = new StringUtils();
                return utils.PredictionEventToString(_predictionEventType);
            }
        }

        /// <summary>
        /// Get the icon to use
        /// </summary>
        public override EAnnotationImage IconRef
        {
            get
            {
                EAnnotationImage icon = EAnnotationImage.None;
                switch (_predictionEventType)
                {
                    case EWordPredictionEventType.NextSuggestion:
                        icon = EAnnotationImage.NextSuggestion; break;
                    case EWordPredictionEventType.PreviousSuggestion:
                        icon = EAnnotationImage.PreviousSuggestion; break;
                    case EWordPredictionEventType.InsertSuggestion:
                        icon = EAnnotationImage.InsertSuggestion; break;
                    case EWordPredictionEventType.CancelSuggestions:
                        icon = EAnnotationImage.CancelSuggestions; break;
                }
                return icon;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public WordPredictionAction()
            : base()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="actionType"></param>
        public WordPredictionAction(EWordPredictionEventType predictionEventType)
            : base()
        {
            _predictionEventType = predictionEventType;
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _predictionEventType = (EWordPredictionEventType)Enum.Parse(typeof(EWordPredictionEventType), element.GetAttribute("predictioneventtype"));

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("predictioneventtype", _predictionEventType.ToString());
        }

        /// <summary>
        /// Start the action
        /// </summary>
        public override void Start(IStateManager parent, KxSourceEventArgs args)
        {            
            // If it's a cancel action, tell the state manager to ignore the current word
            // Otherwise, let the UI decide what needs to be done
            if (_predictionEventType == EWordPredictionEventType.CancelSuggestions)
            {
                parent.WordPredictionManager.CancelSuggestions();
            }
            else
            {
                parent.ThreadManager.SubmitUIEvent(new KxPredictionEventArgs(/*args.SourceID,*/ _predictionEventType));
            }

            IsOngoing = false;
        }
    }
}
