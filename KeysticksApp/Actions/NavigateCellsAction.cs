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
using Keysticks.Sources;

namespace Keysticks.Actions
{
    /// <summary>
    /// Action for changing keyboard cell
    /// </summary>
    public class NavigateCellsAction : BaseAction
    {
        // Fields
        private ELRUDState _direction = ELRUDState.Left;
        private bool _wrapAround = false;
        private StringUtils _utils = new StringUtils();

        // Properties
        public ELRUDState Direction { get { return _direction; } set { _direction = value; } }
        public bool WrapAround { get { return _wrapAround; } set { _wrapAround = value; } }

        /// <summary>
        /// Return what type of action this is
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.NavigateCells; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public NavigateCellsAction()
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
                string name = string.Format(Properties.Resources.String_MoveXOneCell, _utils.DirectionToString(_direction).ToLower());
                if (_wrapAround)
                {
                    name += " (" + Properties.Resources.String_allow_wrap + ")";
                }
                return name;
           }
        }

        public override EAnnotationImage IconRef
        {
            get
            {
                EAnnotationImage icon;
                switch (_direction)
                {
                    case ELRUDState.UpLeft:
                        icon = EAnnotationImage.UpLeftDirection;
                        break;
                    case ELRUDState.Up:
                        icon = EAnnotationImage.UpDirection;
                        break;
                    case ELRUDState.UpRight:
                        icon = EAnnotationImage.UpRightDirection;
                        break;
                    case ELRUDState.Left:
                        icon = EAnnotationImage.LeftDirection;
                        break;
                    case ELRUDState.Centre:
                        icon = EAnnotationImage.CentrePosition;
                        break;
                    case ELRUDState.Right:
                        icon = EAnnotationImage.RightDirection;
                        break;
                    case ELRUDState.DownLeft:
                        icon = EAnnotationImage.DownLeftDirection;
                        break;
                    case ELRUDState.Down:
                        icon = EAnnotationImage.DownDirection;
                        break;
                    case ELRUDState.DownRight:
                        icon = EAnnotationImage.DownRightDirection;
                        break;
                    default:
                        icon = EAnnotationImage.None;
                        break;
                }
                return icon;
            }
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _direction = (ELRUDState)Enum.Parse(typeof(ELRUDState), element.GetAttribute("direction"));
            _wrapAround = bool.Parse(element.GetAttribute("wrap"));

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("direction", _direction.ToString());
            element.SetAttribute("wrap", _wrapAround.ToString());
        }

        /// <summary>
        /// Perform the action
        /// </summary>
        /// <param name="parent"></param>
        public override void Start(IStateManager parent, KxSourceEventArgs args)
        {
            BaseSource source = parent.CurrentProfile.GetSource(args.SourceID);
            if (source != null)
            {
                StateVector currentSituation = source.CurrentState;
                int cellID = currentSituation.CellID;
                if (cellID > 0)
                {
                    // Get the mode's cell layout
                    int modeID = currentSituation.ModeID;
                    AxisValue modeItem = source.StateTree.GetMode(modeID);
                    if (modeItem != null && modeItem.GridType != EGridType.None)
                    {
                        cellID = parent.CellManager.FindCellToSelect(cellID, _direction, modeItem, _wrapAround);
                        if (cellID != Constants.NoneID)
                        {
                            StateVector newSituation = new StateVector(currentSituation);
                            newSituation.CellID = cellID;
                            source.SetCurrentState(newSituation);
                        }
                    }
                }
            }
        }
    }
}
