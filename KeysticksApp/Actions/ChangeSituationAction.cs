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
using System.Xml;
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Config;

namespace Keysticks.Actions
{
    /// <summary>
    /// Action for changing control set / page / keyboard cell
    /// </summary>
    public class ChangeSituationAction : BaseAction
    {
        // Fields
        private StateVector _newSituation = new StateVector();
        private string _situationName = "";

        // Properties
        public StateVector NewSituation { get { return _newSituation; } set { _newSituation = value; } }
        public string SituationName { get { return _situationName; } set { _situationName = value; } }

        /// <summary>
        /// Return what type of action this is
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.ChangeControlSet; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ChangeSituationAction()
            : base()
        {
        }

        /// <summary>
        /// Return the name of the action
        /// </summary>
        /// <returns></returns>
        public override string Name
        {
           get 
           {
               return string.Format(Properties.Resources.String_GoToX, _situationName);
           }
        }

        /// <summary>
        /// Short name
        /// </summary>
        public override string ShortName
        {
            get
            {
                return _situationName;
            }
        }

        public override EAnnotationImage IconRef
        {
            get
            {
                EAnnotationImage icon = EAnnotationImage.ChangeControlSet;
                switch (_newSituation.ModeID)
                {
                    case Constants.NextID:
                        icon = EAnnotationImage.NextControlSet;
                        break;
                    case Constants.PreviousID:
                        icon = EAnnotationImage.PreviousControlSet;
                        break;
                    case Constants.NoneID:
                        switch (_newSituation.PageID)
                        {
                            case Constants.NextID:
                                icon = EAnnotationImage.NextPage;
                                break;
                            case Constants.PreviousID:
                                icon = EAnnotationImage.PreviousPage;
                                break;
                            case Constants.NoneID:
                                icon = GetIconForCellID(_newSituation.CellID);
                                break;
                            default:
                                icon = EAnnotationImage.ChangePage;
                                break;
                        }
                        break;
                    default:
                        icon = EAnnotationImage.ChangeControlSet;
                        break;
                }
                return icon;
            }
        }

        /// <summary>
        /// Get an image that represents the cell ID
        /// </summary>
        /// <param name="cellID"></param>
        /// <returns></returns>
        private EAnnotationImage GetIconForCellID(int cellID)
        {
            EAnnotationImage icon;
            switch (cellID)
            {
                case Constants.NextID:
                    icon = EAnnotationImage.RightDirection;
                    break;
                case Constants.PreviousID:
                    icon = EAnnotationImage.LeftDirection;
                    break;
                case Constants.TopLeftCellID:
                    icon = EAnnotationImage.UpLeftDirection;
                    break;
                case Constants.TopCentreCellID:
                    icon = EAnnotationImage.UpDirection;
                    break;
                case Constants.TopRightCellID:
                    icon = EAnnotationImage.UpRightDirection;
                    break;
                case Constants.CentreLeftCellID:
                    icon = EAnnotationImage.LeftDirection;
                    break;
                case Constants.CentreCellID:
                    icon = EAnnotationImage.CentrePosition;
                    break;
                case Constants.CentreRightCellID:
                    icon = EAnnotationImage.RightDirection;
                    break;
                case Constants.BottomLeftCellID:
                    icon = EAnnotationImage.DownLeftDirection;
                    break;
                case Constants.BottomCentreCellID:
                    icon = EAnnotationImage.DownDirection;
                    break;
                case Constants.BottomRightCellID:
                    icon = EAnnotationImage.DownRightDirection;
                    break;
                default:
                    icon = EAnnotationImage.ChangeCell;
                    break;
            }

            return icon;
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _newSituation = new StateVector();
            _newSituation.FromXml(element);
            _situationName = element.GetAttribute("situationname");

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            _newSituation.ToXml(element, doc);
            element.SetAttribute("situationname", _situationName);
        }

        /// <summary>
        /// Perform the action
        /// </summary>
        /// <param name="parent"></param>
        public override void Start(IStateManager parent, KxSourceEventArgs args)
        {
            parent.SetCurrentState(args.SourceID, _newSituation);
        }
    }
}
