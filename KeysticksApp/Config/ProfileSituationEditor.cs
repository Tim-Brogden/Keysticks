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
using Keysticks.Core;
using Keysticks.Event;
using Keysticks.Sources;

namespace Keysticks.Config
{
    /// <summary>
    /// Utility class for editing the control set hierarchy
    /// </summary>
    public class ProfileSituationEditor
    {
        private BaseSource _source;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source"></param>
        public ProfileSituationEditor(BaseSource source)
        {
            _source = source;
        }

        /// <summary>
        /// Add a new mode
        /// </summary>
        /// <param name="name"></param>
        /// <param name="insertAfterID"></param>
        /// <param name="controls"></param>
        /// <param name="gridConfig"></param>
        /// <param name="templateGroup"></param>
        /// <returns></returns>
        public AxisValue AddMode(string name, 
                                    int insertAfterID,
                                    ControlsDefinition controls,                                    
                                    GridConfig gridConfig,
                                    ETemplateGroup templateGroup)
        {
            // Create situation for mode
            int newID = _source.StateTree.SubValues.GetFirstUnusedID(Constants.ID1);
            StateVector situation = new StateVector(newID, Constants.DefaultID, Constants.DefaultID);
            
            // Create mode
            AxisValue newItem = new AxisValue(newID, 
                                                name, 
                                                situation, 
                                                controls, 
                                                gridConfig,
                                                templateGroup);
            _source.StateTree.SubValues.InsertAfterID(insertAfterID, newItem);
            _source.IsModified = true;

            return newItem;
        }

        /// <summary>
        /// Rename an existing mode
        /// </summary>
        /// <param name="modeID"></param>
        /// <param name="newName"></param>
        public void RenameMode(int modeID, string newName)
        {
            AxisValue modeItem = _source.StateTree.GetMode(modeID);
            if (modeItem != null && modeItem.ID > 0)
            {
                modeItem.Name = newName;
                _source.IsModified = true;
                _source.Actions.Validate();
            }
        }

        /// <summary>
        /// Copy the specified mode
        /// </summary>
        /// <param name="modeID"></param>
        /// <param name="copyName"></param>
        /// <returns></returns>
        public AxisValue CopyMode(AxisValue modeItem, string copyName, int insertAfterID)
        {
            AxisValue copiedMode = null;
            if (modeItem != null)
            {
                // Copy controls
                ControlsDefinition controlsCopy = null;
                GridConfig gridCopy = null;
                if (modeItem.Controls != null)
                {
                    controlsCopy = new ControlsDefinition(modeItem.Controls);                    

                    // Create grid for mode if applicable
                    GridConfig grid = modeItem.Grid;
                    if (grid != null)
                    {
                        gridCopy = CreateGridConfig(grid.GridType, grid.NumCols, controlsCopy);
                    }
                }

                // Create mode of same type
                copiedMode = AddMode(copyName,
                                        insertAfterID, 
                                        controlsCopy,
                                        gridCopy,
                                        modeItem.TemplateGroup);

                if (copiedMode != null)
                {
                    // Copy pages
                    int insertAfterPageID = Constants.DefaultID;
                    AxisValue copiedPage;
                    foreach (AxisValue pageItem in modeItem.SubValues)
                    {
                        copiedPage = CopyPage(pageItem, pageItem.Name, copiedMode.ID, insertAfterPageID);
                        insertAfterPageID = copiedPage.ID;
                    }
                }
            }

            return copiedMode;
        }

        /// <summary>
        /// Delete a mode
        /// </summary>
        /// <param name="modeID"></param>
        public void DeleteMode(int modeID)
        {
            if (modeID > 0)
            {
                AxisValue modeItem = (AxisValue)_source.StateTree.GetMode(modeID);
                if (modeItem != null && _source.StateTree.SubValues.Remove(modeItem))
                {
                    // Validate
                    _source.AutoActivations.Validate();
                    _source.Actions.Validate();
                    _source.IsModified = true;
                }
            }
        }

        /// <summary>
        /// Clear all states except the root state
        /// </summary>
        public void DeleteAllModes()
        {
            int i = 0;
            while (i < _source.StateTree.SubValues.Count)
            {
                AxisValue modeItem = (AxisValue)_source.StateTree.SubValues[i];
                if (modeItem.ID != Constants.DefaultID)
                {
                    // Remove mode
                    _source.StateTree.SubValues.RemoveAt(i);

                    // Validate
                    _source.IsModified = true;
                }
                else
                {
                    i++;
                }
            }

            _source.AutoActivations.Validate();
            _source.Actions.Validate();
        }

        /// <summary>
        /// Add a new page
        /// </summary>
        /// <param name="modeID"></param>
        /// <param name="pageName"></param>
        public AxisValue AddPageToMode(AxisValue modeItem, int pageID, string pageName, int insertAfterID)
        {
            StateVector situation = new StateVector(modeItem.Situation);
            situation.PageID = pageID;

            AxisValue newPageItem = new AxisValue(pageID, pageName, situation);
            modeItem.SubValues.InsertAfterID(insertAfterID, newPageItem);

            _source.IsModified = true;

            return newPageItem;
        }

        /// <summary>
        /// Rename a page
        /// </summary>
        /// <param name="modeID"></param>
        /// <param name="pageID"></param>
        /// <param name="newName"></param>
        public void RenamePage(int modeID, int pageID, string newName)
        {
            AxisValue modeItem = _source.StateTree.GetMode(modeID);
            if (modeItem != null)
            {
                AxisValue pageItem = (AxisValue)modeItem.SubValues.GetItemByID(pageID);
                if (pageItem != null && pageItem.ID > 0)
                {
                    pageItem.Name = newName;
                    _source.IsModified = true;                    
                    _source.Actions.Validate();
                }
            }
        }

        /// <summary>
        /// Copy a page
        /// </summary>
        /// <param name="modeID"></param>
        /// <param name="pageID"></param>
        /// <param name="newName"></param>
        public AxisValue CopyPage(AxisValue pageItem, string pageCopyName, int copyToModeID, int insertAfterPageID)
        {
            AxisValue newPageItem;
            AxisValue destinationMode = _source.StateTree.GetMode(copyToModeID);
            
            // Create new page                    
            if (pageItem.ID == Constants.DefaultID)
            {
                newPageItem = AddDefaultPageToMode(destinationMode);
            }
            else
            {
                // Try to use the same ID as the existing page - this is required when copying a whole mode
                int newPageID = pageItem.ID;
                if (destinationMode.SubValues.GetItemByID(newPageID) != null)
                {
                    // Page already exists (probably because we're copying a single page), so need to generate a new ID
                    newPageID = destinationMode.SubValues.GetFirstUnusedID(1);
                }
                newPageItem = AddPageToMode(destinationMode, newPageID, pageCopyName, insertAfterPageID);                        
            }

            // Copy the cells if there are any
            CopyCells(pageItem, newPageItem);

            return newPageItem;
        }

        /// <summary>
        /// Copy the cells from one page to another
        /// </summary>
        /// <param name="pageItem"></param>
        /// <param name="newPageItem"></param>
        public void CopyCells(AxisValue pageItem, AxisValue newPageItem)
        {
            foreach (AxisValue cellItem in pageItem.SubValues)
            {
                StateVector situation = new StateVector(newPageItem.Situation);
                situation.CellID = cellItem.ID;

                newPageItem.SubValues.Add(new AxisValue(cellItem.ID, cellItem.Name, situation));
            }
            _source.IsModified = true;
        }

        /// <summary>
        /// Delete a page
        /// </summary>
        /// <param name="modeID"></param>
        /// <param name="pageID"></param>
        public void DeletePage(int modeID, int pageID)
        {
            AxisValue modeItem = _source.StateTree.GetMode(modeID);
            if (modeItem != null && pageID > 0)
            {
                AxisValue pageItem = (AxisValue)modeItem.SubValues.GetItemByID(pageID);
                if (pageItem != null && modeItem.SubValues.Remove(pageItem))
                {
                    // Validate action sets
                    _source.AutoActivations.Validate();
                    _source.Actions.Validate();
                    _source.IsModified = true;
                }                
            }
        }

        /// <summary>
        /// Add a default page to a mode
        /// </summary>
        /// <param name="modeItem"></param>
        public AxisValue AddDefaultPageToMode(AxisValue modeItem)
        {
            // Add default page
            StateVector situation = new StateVector(modeItem.Situation);
            AxisValue newPageItem = new AxisValue(Constants.DefaultID, Properties.Resources.String_AnyPage, situation);
            modeItem.SubValues.Insert(0, newPageItem);

            _source.IsModified = true;

            return newPageItem;
        }

        /// <summary>
        /// Create the grid configuration that accompanies a mode
        /// </summary>
        /// <param name="windowType"></param>
        /// <returns></returns>
        public GridConfig CreateGridConfig(EGridType gridType, int numCols, ControlsDefinition controls)
        {
            GridConfig gridConfig = null;

            if (controls != null)
            {
                // Add bindings if required
                switch (gridType)
                {
                    case EGridType.Keyboard:
                        gridConfig = CreateKeyboardGridConfig(gridType, controls.SelectionControl);
                        break;
                    case EGridType.ActionStrip:
                        gridConfig = CreateActionStripGridConfig(gridType, numCols, controls.SelectionControl);
                        break;
                    case EGridType.Square4x4:
                    case EGridType.Square8x4:
                        gridConfig = CreateSquareGridConfig(gridType, controls.SelectionControl);
                        break;
                }
            }

            return gridConfig;
        }


        /// <summary>
        /// Add the window bindings for a keyboard window
        /// </summary>
        /// <param name="gridType"></param>
        /// <param name="eGeneralisedControl"></param>
        /// <returns></returns>
        private GridConfig CreateKeyboardGridConfig(EGridType gridType, GeneralisedControl selectionControl)
        {
            GridConfig gridConfig = new GridConfig(gridType);

            StringUtils utils = new StringUtils();
            KxControlEventArgs inputControl = selectionControl.ReferenceControl;
            foreach (EKeyboardKey keyCell in Enum.GetValues(typeof(EKeyboardKey)))
            {
                // Create situation for this cell
                StateVector situation = new StateVector(Constants.NoneID, Constants.NoneID, (int)keyCell);

                // Create cell binding
                GridBinding binding = new GridBinding(keyCell.ToString(),
                                                    situation, 
                                                    new KxControlEventArgs(inputControl));
                gridConfig.Bindings.Add(binding);
            }

            return gridConfig;
        }

        /// <summary>
        /// Add window bindings for an action strip grid
        /// </summary>
        /// <param name="gridType"></param>
        /// <param name="eGeneralisedControl"></param>
        /// <returns></returns>
        private GridConfig CreateActionStripGridConfig(EGridType gridType, int numCols, GeneralisedControl selectionControl)
        {
            // If default cols is requested, populate with default value
            if (numCols == Constants.DefaultID)
            {
                numCols = Constants.ActionStripDefaultNumCells;
            }

            // Create grid
            GridConfig gridConfig = new GridConfig(gridType, numCols);

            // Add bindings
            StringUtils utils = new StringUtils();
            KxControlEventArgs inputControl = selectionControl.ReferenceControl;
            for (int i = 0; i < numCols; i++)
            {
                // Create situation for this cell
                StateVector situation = new StateVector(Constants.NoneID, Constants.NoneID, i + 1);

                // Create cell binding
                GridBinding binding = new GridBinding("A" + i.ToString(System.Globalization.CultureInfo.InvariantCulture),
                                                    situation,
                                                    new KxControlEventArgs(inputControl));
                gridConfig.Bindings.Add(binding);
            }

            return gridConfig;
        }

        /// <summary>
        /// Add the window bindings for a square grid window
        /// </summary>
        /// <param name="gridConfig"></param>
        private GridConfig CreateSquareGridConfig(EGridType gridType, GeneralisedControl selectionControl)
        {
            GridConfig gridConfig = new GridConfig(gridType);

            EDirectionMode navigationDirectionality;
            EDirectionMode selectionDirectionality;
            switch (gridType)
            {
                case EGridType.Square4x4:
                    navigationDirectionality = EDirectionMode.FourWay;
                    selectionDirectionality = EDirectionMode.FourWay;
                    break;
                case EGridType.Square8x4:
                    navigationDirectionality = EDirectionMode.EightWay;
                    selectionDirectionality = EDirectionMode.FourWay;
                    break;
                default:
                    navigationDirectionality = EDirectionMode.EightWay;
                    selectionDirectionality = EDirectionMode.EightWay;
                    break;
            }

            // Loop over cells
            StringUtils utils = new StringUtils();
            for (int i = 0; i < 9; i++)
            {
                // See if this cell is required
                if (!_source.Utils.IsDirectionValid(Constants.SquareGridDirections[i], navigationDirectionality/*, true*/))
                {
                    continue;
                }

                // Create situation for this cell
                StateVector situation = new StateVector(Constants.NoneID, Constants.NoneID, Constants.SquareGridCellIDs[i]);

                // Create bindings for controls in the cell
                for (int j = 0; j < 9; j++)
                {
                    // See if this binding is required
                    ELRUDState direction = Constants.SquareGridDirections[j];
                    if (!_source.Utils.IsDirectionValid(direction, selectionDirectionality/*, true*/))
                    {
                        continue;
                    }

                    string controlName = string.Format("A{0}{1}{2}{3}", i / 3, i % 3, j / 3, j % 3);
                    KxControlEventArgs args = _source.Utils.GetSpecificInputControl(selectionControl, EControlSetting.None, direction);
                    if (args != null)
                    {
                        GridBinding binding = new GridBinding(controlName, new StateVector(situation), args);
                        gridConfig.Bindings.Add(binding);
                    }
                }
            }

            return gridConfig;
        }

        /// <summary>
        /// Add the cells to a page according to its window type
        /// </summary>
        /// <param name="modeItem"></param>
        /// <param name="pageItem"></param>
        public void AddCellsToPage(AxisValue pageItem, GridConfig grid)
        {
            StringUtils utils = new StringUtils();

            if (grid != null)
            {
                switch (grid.GridType)
                {
                    case EGridType.Keyboard:
                        {
                            // Add default cell
                            AxisValue defaultCellItem = new AxisValue(Constants.DefaultID, Properties.Resources.String_AnyKey, new StateVector(pageItem.Situation));
                            pageItem.SubValues.Add(defaultCellItem);

                            // Add keyboard keys
                            for (int i = 1; i < Constants.KeyboardCellNames.Length; i++)
                            {
                                int cellID = i;
                                StateVector situation = new StateVector(pageItem.Situation);
                                situation.CellID = cellID;
                                string stateName = string.Format("{0} " + Properties.Resources.String_Key.ToLower(), Constants.KeyboardCellNames[i]);
                                AxisValue cellItem = new AxisValue(cellID, stateName, situation);
                                pageItem.SubValues.Add(cellItem);
                            }
                        }
                        break;
                    case EGridType.ActionStrip:
                        {
                            AddActionStripCells(pageItem, grid.NumCols);
                        }
                        break;
                    case EGridType.Square8x4:
                        {
                            // Add default cell
                            AxisValue defaultCellItem = new AxisValue(Constants.DefaultID, Properties.Resources.String_AnyCell, new StateVector(pageItem.Situation));
                            pageItem.SubValues.Add(defaultCellItem);

                            // Add 9 cells of square grid
                            for (int i = 0; i < 9; i++)
                            {
                                int cellID = Constants.TopLeftCellID + i;
                                StateVector situation = new StateVector(pageItem.Situation);
                                situation.CellID = cellID;
                                string stateName = string.Format("{0} {1}", utils.GetCellName(i), Properties.Resources.String_Cell.ToLower());
                                AxisValue cellItem = new AxisValue(cellID, stateName, situation);
                                pageItem.SubValues.Add(cellItem);
                            }
                        }
                        break;
                    case EGridType.Square4x4:
                        {
                            // Add default cell
                            AxisValue defaultCellItem = new AxisValue(Constants.DefaultID, Properties.Resources.String_AnyCell, new StateVector(pageItem.Situation));
                            pageItem.SubValues.Add(defaultCellItem);

                            // Add 5 LRUD/centre cells of square grid
                            for (int i = 0; i < 9; i++)
                            {
                                if (_source.Utils.IsDirectionValid(Constants.SquareGridDirections[i], EDirectionMode.FourWay/*, true*/))
                                {
                                    int cellID = Constants.TopLeftCellID + i;
                                    StateVector situation = new StateVector(pageItem.Situation);
                                    situation.CellID = cellID;
                                    string stateName = string.Format("{0} {1}", utils.GetCellName(i), Properties.Resources.String_Cell.ToLower());
                                    AxisValue cellItem = new AxisValue(cellID, stateName, situation);
                                    pageItem.SubValues.Add(cellItem);
                                }
                            }
                        }
                        break;
                }
                _source.IsModified = true;
            }
        }

        /// <summary>
        /// Add cells to an action strip
        /// </summary>
        /// <param name="pageItem"></param>
        /// <param name="gridType"></param>
        /// <param name="numCells"></param>
        public void AddActionStripCells(AxisValue pageItem, int numCells)
        {
            // Interpret default value
            if (numCells == Constants.DefaultID)
            {
                numCells = Constants.ActionStripDefaultNumCells;
            }

            // Add default cell
            AxisValue defaultCellItem = new AxisValue(Constants.DefaultID, Properties.Resources.String_AnyCell, new StateVector(pageItem.Situation));
            pageItem.SubValues.Add(defaultCellItem);

            // Add action cells
            for (int i = 1; i <= numCells; i++)
            {
                StateVector situation = new StateVector(pageItem.Situation);
                situation.CellID = i;
                string stateName = string.Format(Properties.Resources.String_Cell + " {0}", i);
                AxisValue cellItem = new AxisValue(i, stateName, situation);
                pageItem.SubValues.Add(cellItem);
            }

            _source.IsModified = true;
        }
    }
}
