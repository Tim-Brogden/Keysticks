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
using System.Text;
using Keysticks.Core;

namespace Keysticks.Config
{
    /// <summary>
    /// Identifies a logical state
    /// </summary>
    public class StateVector
    {
        // Fields
        private int[] _valueIDs = new int[3];

        // Properties
        public int ModeID { get { return _valueIDs[0]; } set { _valueIDs[0] = value; } }
        public int PageID { get { return _valueIDs[1]; } set { _valueIDs[1] = value; } }
        public int CellID { get { return _valueIDs[2]; } set { _valueIDs[2] = value; } }
        public int ID
        {
            get
            {
                return ((short)_valueIDs[0] << 16) | ((byte)_valueIDs[1] << 8) | (byte)_valueIDs[2];
            }
        }        

        /// <summary>
        /// Constructor
        /// </summary>
        public StateVector()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="modeID"></param>
        /// <param name="pageID"></param>
        /// <param name="cellID"></param>
        public StateVector(int modeID, int pageID, int cellID)
        {
            _valueIDs[0] = modeID;
            _valueIDs[1] = pageID;
            _valueIDs[2] = cellID;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="state"></param>
        public StateVector(StateVector state)
        {
            _valueIDs[0] = state._valueIDs[0];
            _valueIDs[1] = state._valueIDs[1];
            _valueIDs[2] = state._valueIDs[2];
        }

        /// <summary>
        /// Get the root situation
        /// </summary>
        /// <returns></returns>
        public static StateVector GetRootSituation()
        {
            return new StateVector(Constants.DefaultID, Constants.DefaultID, Constants.DefaultID);
        }

        /// <summary>
        /// Get the value of a particular axis
        /// </summary>
        /// <param name="axisIndex"></param>
        /// <returns></returns>
        public int GetAxisValue(int axisIndex)
        {
            return _valueIDs[axisIndex];
        }

        /// <summary>
        /// Set the value of a particular axis
        /// </summary>
        /// <param name="axisIndex"></param>
        /// <param name="valueID"></param>
        public void SetAxisValue(int axisIndex, int valueID)
        {
            _valueIDs[axisIndex] = valueID;
        }

        /// <summary>
        /// Return whether the state contains NextID, PrevID or NoneID 
        /// </summary>
        /// <returns></returns>
        public bool IsRelative()
        {
            bool isRelative = (ModeID < 1 && ModeID != Constants.DefaultID) ||
                                (PageID < 1 && PageID != Constants.DefaultID) ||
                                (CellID < 1 && CellID != Constants.DefaultID);

            return isRelative;
        }

        /// <summary>
        /// Return whether the state contains only specific IDs (> 0)
        /// </summary>
        /// <returns></returns>
        public bool IsSpecific()
        {
            return (ModeID > 0 && PageID > 0 && CellID > 0);
        }

        /// <summary>
        /// See if two states are logically the same
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool IsSameAs(StateVector state)
        {
            bool isSame = (state != null) && 
                            (state.ModeID == ModeID) &&
                            (state.PageID == PageID) &&
                            (state.CellID == CellID);
            return isSame;
        }

        /// <summary>
        /// See if this state is a super-state of the specified state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool Contains(StateVector state)
        {
            // Vector contains another if, whenever one of its axes has a specific value (i.e. not -1 or 0),
            // the other has the same value
            bool contains = (ModeID <= 0 || state.ModeID == ModeID) &&
                            (PageID <= 0 || state.PageID == PageID) &&
                            (CellID <= 0 || state.CellID == CellID);

            return contains;
        }

        public List<StateVector> GetParentStates()
        {
            List<StateVector> parentStates = new List<StateVector>();
            if (CellID != Constants.DefaultID)
            {
                // If has a page value, state has two parents
                // Put this one first as its the less specific of the two
                if (PageID != Constants.DefaultID)
                {
                    StateVector anyPageParent = new StateVector(this);
                    anyPageParent.PageID = Constants.DefaultID;
                    parentStates.Add(anyPageParent);
                }

                // Has cell value
                StateVector anyCellParent = new StateVector(this);
                anyCellParent.CellID = Constants.DefaultID;
                parentStates.Add(anyCellParent);
            }
            else if (PageID != Constants.DefaultID)
            {
                // Has page value
                StateVector parent = new StateVector(this);
                parent.PageID = Constants.DefaultID;
                parentStates.Add(parent);
            }
            else if (ModeID != Constants.DefaultID)
            {
                // Has mode value
                StateVector parent = new StateVector(this);
                parent.ModeID = Constants.DefaultID;
                parentStates.Add(parent);
            }

            return parentStates;
        }

        /// <summary>
        /// Read from Xml
        /// </summary>
        /// <param name="element"></param>
        public void FromXml(XmlElement element)
        {
            string str = element.GetAttribute("state");
            string[] tokens = str.Split(',');
            if (tokens.Length >= 3)
            {
                for (int i=0; i<3; i++)
                {
                    int id;
                    if (int.TryParse(tokens[i], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out id))
                    {
                        _valueIDs[i] = id;
                    }
                }
            }
        }

        /// <summary>
        /// Write to Xml
        /// </summary>
        /// <param name="parentElement"></param>
        /// <param name="doc"></param>
        public void ToXml(XmlElement parentElement, XmlDocument doc)
        {
            parentElement.SetAttribute("state", ToString());
        }

        /// <summary>
        /// Convert to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = string.Format("{0},{1},{2}", 
                                        _valueIDs[0].ToString(System.Globalization.CultureInfo.InvariantCulture),
                                        _valueIDs[1].ToString(System.Globalization.CultureInfo.InvariantCulture),
                                        _valueIDs[2].ToString(System.Globalization.CultureInfo.InvariantCulture));
            return str;
        }
    }
}
