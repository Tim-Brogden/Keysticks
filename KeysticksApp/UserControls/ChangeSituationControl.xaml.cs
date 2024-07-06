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
using System.Collections.ObjectModel;
using System.ComponentModel;
using Keysticks.Core;
using Keysticks.Actions;
using Keysticks.Config;
using Keysticks.Sources;
using Keysticks.UI;

namespace Keysticks.UserControls
{    
    /// <summary>
    /// Control for editing change control set actions
    /// </summary>
    public partial class ChangeSituationControl : UserControl
    {
        // Fields
        private ChangeSituationAction _currentAction = new ChangeSituationAction();
        private ObservableCollection<AxisRowTemplate> _axisRows = new ObservableCollection<AxisRowTemplate>();
        private BaseSource _source;
        private StateVector _situation;

        /// <summary>
        ///  Constructor
        /// </summary>
        public ChangeSituationControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Control loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshDisplay();
        }

        /// <summary>
        /// Set the axis data in order to populate the combo boxes
        /// </summary>
        /// <param name="rootAxis"></param>
        public void SetSourceAndSituation(BaseSource source, StateVector situation)
        {
            // Store profile
            _source = source;
            _situation = situation;

            // Initialise combo boxes without values
            StringUtils utils = new StringUtils();
            _axisRows.Clear();
            for (int i=0; i<3; i++)     // Loop over three axes: Control set, page, cell
            {
                AxisRowTemplate rowItem = new AxisRowTemplate();
                rowItem.RowIndex = i;
                rowItem.AxisItem = new NamedItem(i, utils.GetAxisName(i) + ":");
                rowItem.Values = new NamedItemList();
                rowItem.ValueID = Constants.NoneID;
                _axisRows.Add(rowItem);
            }

            // Populate the first combo box
            PopulateNthComboBoxItems(0);

            // Bind list
            StateSelectionList.DataContext = _axisRows;
        }

        /// <summary>
        /// Combo box selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RowCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            int rowIndex = (int)combo.Tag;
            if (rowIndex < _axisRows.Count - 1)
            {
                PopulateNthComboBoxItems(rowIndex + 1);
            }

            e.Handled = true;
        }
        
        /// <summary>
        /// Populate the list of values for the n-th axis
        /// </summary>
        /// <param name="rowIndex"></param>
        private void PopulateNthComboBoxItems(int rowIndex)
        {
            if (_source == null || _situation == null)
            {
                return;
            }

            // Get the currently selected state
            int idToSelect = _axisRows[rowIndex].ValueID;
            StateVector selectedState = new StateVector();
            for (int i = 0; i < 3; i++)
            {
                selectedState.SetAxisValue(i, _axisRows[i].ValueID);
            }

            // Interpret the state given the current situation
            selectedState = _source.Utils.RelativeStateToAbsolute(selectedState, _situation);

            // Get the valid values for required axis in the selected state
            NamedItemList valueList = _source.StateTree.SubValues;
            for (int i = 0; i < rowIndex; i++)
            {
                int valueID = selectedState.GetAxisValue(i);

                // If any "no change" values remain, use the same values as for default ID
                if (valueID == Constants.NoneID)
                {
                    valueID = Constants.DefaultID;
                }

                AxisValue axisValue = (AxisValue)valueList.GetItemByID(valueID);
                if (axisValue == null)
                {
                    // No items
                    valueList = null;
                    break;
                }

                valueList = axisValue.SubValues;
            }

            // Update the combo box items
            GUIUtils.PopulateDisplayableListWithNamedItems(_axisRows[rowIndex].Values, valueList, true, Properties.Resources.String_NoChange, false, "", true);

            // Restore selection, but reset to "no change" if it's no longer valid
            _axisRows[rowIndex].ValueID = Constants.DefaultID;  // Something not in list
            if (_axisRows[rowIndex].Values.GetItemByID(idToSelect) == null)
            {
                idToSelect = Constants.NoneID;
            }
            _axisRows[rowIndex].ValueID = idToSelect;            
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is ChangeSituationAction)
            {
                _currentAction = (ChangeSituationAction)action;
                RefreshDisplay();
            }
        }

        /// <summary>
        /// Redisplay the action
        /// </summary>
        private void RefreshDisplay()
        {
            if (IsLoaded && _currentAction != null && _situation != null)
            {
                StateVector state = _currentAction.NewSituation;
                if (state != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        _axisRows[i].ValueID = state.GetAxisValue(i);
                    }
                }
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {           
            _currentAction = null;

            StateVector newSituation = new StateVector(_axisRows[0].ValueID, _axisRows[1].ValueID, _axisRows[2].ValueID);
            if (!newSituation.IsSameAs(new StateVector()))
            {
                _currentAction = new ChangeSituationAction();
                _currentAction.NewSituation = newSituation;
                _currentAction.SituationName = _source.Utils.GetRelativeSituationName(_currentAction.NewSituation, _situation);
            }

            return _currentAction;
        }

    }

    public class AxisRowTemplate : INotifyPropertyChanged
    {
        private int _valueID;
        private NamedItemList _valueList;

        public event PropertyChangedEventHandler PropertyChanged;

        public int RowIndex { get; set; }
        public NamedItem AxisItem { get; set; }
        public NamedItemList Values 
        {
            get { return _valueList; }
            set
            {
                if (value != _valueList)
                {
                    _valueList = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Values"));
                    }
                }
            }
        }
        public int ValueID
        {
            get { return _valueID; }
            set
            {
                if (value != _valueID)
                {
                    _valueID = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("ValueID"));
                    }
                }
            }
        }

    }
}
