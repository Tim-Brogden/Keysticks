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
using System.Windows;
using System.Windows.Controls;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.UI;

namespace Keysticks.UserControls
{
    /// <summary>
    /// 'Keyboard' grid control consisting of a single row of cells
    /// </summary>
    public partial class ActionStripControl : BaseGridControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ActionStripControl()
            : base()
        {
            InitializeComponent();

            // Populate number of cells combo
            NamedItemList numCellsList = new NamedItemList();
            for (int i = 1; i <= 16; i++)
            {
                numCellsList.Add(new NamedItem(i, i.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            }
            NumCellsComboBox.ItemsSource = numCellsList;

            // Create array of annotations
            ControlAnnotationControl[] annotationControls = new ControlAnnotationControl[] 
            {
                A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14, A15
            };
            SetAnnotationControls(annotationControls);
        }

        /// <summary>
        /// Acquire the focus
        /// </summary>
        public override void SetFocus()
        {
            OuterPanel.Focus();
        }        

        /// <summary>
        /// Handle change of situation
        /// </summary>
        /// <param name="args"></param>
        public override void SetCurrentSituation(StateVector situation)
        {
            base.SetCurrentSituation(situation);

            if (IsDesignMode && IsLoaded && Source != null)
            {
                bool modeChanged = (CurrentSituation == null) || (CurrentSituation.ModeID != situation.ModeID);
                if (modeChanged)
                {
                    // Mode changed
                    RefreshNumCellsCombo(situation);
                }
            }
        }

        /// <summary>
        /// Show the annotations for the cells that are in use for this control set
        /// </summary>
        /// <param name="modeItem"></param>
        private void RefreshNumCellsCombo(StateVector situation)
        {
            AxisValue modeItem = Source.Utils.GetModeItem(situation);
            if (modeItem.GridType == EGridType.ActionStrip && modeItem.SubValues.Count != 0)
            {
                AxisValue pageItem = (AxisValue)modeItem.SubValues[0];
                int numCells = pageItem.SubValues.Count - 1;

                // Show the number of annotations in use
                this.NumCellsComboBox.SelectedValue = numCells;                
            }
        }       

        /// <summary>
        /// Control loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            base.UserControl_Loaded(sender, e);

            this.OptionsGroupBox.Visibility = IsDesignMode ? Visibility.Visible : Visibility.Collapsed;
            if (IsDesignMode && CurrentSituation != null && Source != null)
            {
                RefreshNumCellsCombo(CurrentSituation);
            }
        }

        /// <summary>
        /// Number of cells in action strip changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumCellsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int numCells = NumCellsComboBox.SelectedIndex + 1;
            if (numCells > 0 && numCells <= Constants.ActionStripMaxCells)
            {
                AxisValue modeItem = Source.Utils.GetModeItem(CurrentSituation);
                if (modeItem != null && modeItem.GridType == EGridType.ActionStrip && modeItem.SubValues.Count != 0)
                {
                    AxisValue defaultPage = (AxisValue)modeItem.SubValues[0];
                    int currentCells = defaultPage.SubValues.Count - 1;
                    if (numCells != currentCells)
                    {
                        // Delete actions for deleted cells
                        foreach (AxisValue pageItem in modeItem.SubValues)
                        {
                            StateVector deletedSituation = new StateVector(pageItem.Situation);
                            for (int cellID = numCells + 1; cellID <= currentCells; cellID++)
                            {
                                AxisValue cellItem = (AxisValue)pageItem.SubValues.GetItemByID(cellID);
                                if (cellItem != null)
                                {
                                    pageItem.SubValues.Remove(cellItem);
                                }
                            }
                        }

                        // Replace cells collections
                        foreach (AxisValue pageItem in modeItem.SubValues)
                        {
                            pageItem.SubValues.Clear();
                            Source.StateEditor.AddActionStripCells(pageItem, numCells);
                        }

                        // Replace grid bindings
                        GridConfig newGrid = Source.StateEditor.CreateGridConfig(modeItem.GridType, numCells, modeItem.Controls);
                        modeItem.Grid = newGrid;

                        // Uncache grid manager's bindings for this mode
                        UncacheGridBindings(modeItem.ID);

                        // Validate action sets
                        Source.Actions.Validate();
                        Source.IsModified = true;

                        // Refresh the display and deselect cell if it has been deleted
                        StateVector situationToSelect = new StateVector(CurrentSituation);
                        int selectedCellID = situationToSelect.CellID;
                        if (selectedCellID > numCells)
                        {
                            situationToSelect.CellID = Constants.DefaultID;
                        }
                        RaiseEvent(new KxStateRoutedEventArgs(KxStatesEditedEvent, situationToSelect));                        
                    }
                }
            }
            e.Handled = true;
        }
    }
}
