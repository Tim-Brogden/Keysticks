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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.UI;
using Keysticks.Sources;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Control for displaying the control set hierarchy
    /// </summary>
    public partial class StateTreeControl : KxUserControl, ISourceViewerControl
    {
        // Fields
        private bool _isLoaded = false;
        private bool _expandPageLevel = Constants.DefaultExpandPagesInSituationTree;
        private AppConfig _appConfig;
        private BaseSource _source;
        private NamedItemList _treeItems = new NamedItemList();
        private CustomBindingItem _selectedTreeItem;
        private StateVector _selectedState;

        // Dependency properties
        public bool IsDesignMode
        {
            get { return (bool)GetValue(IsDesignModeProperty); }
            set { SetValue(IsDesignModeProperty, value); }
        }
        private static readonly DependencyProperty IsDesignModeProperty =
            DependencyProperty.Register(
            "IsDesignMode",
            typeof(bool),
            typeof(StateTreeControl),
            new FrameworkPropertyMetadata(false)
        );

        // Properties
        public NamedItemList TreeItems { get { return _treeItems; } }
        
        // Commands
        public static RoutedCommand RenameCommand = new RoutedCommand();

        // Routed events
        public static readonly RoutedEvent KxStateEditEvent = EventManager.RegisterRoutedEvent(
            "StateEdit", RoutingStrategy.Bubble, typeof(KxStateRoutedEventHandler), typeof(StateTreeControl));
        public static readonly RoutedEvent KxStateRenameEvent = EventManager.RegisterRoutedEvent(
            "StateRename", RoutingStrategy.Bubble, typeof(KxStateRoutedEventHandler), typeof(StateTreeControl));
        public static readonly RoutedEvent KxStateDeleteEvent = EventManager.RegisterRoutedEvent(
            "StateDelete", RoutingStrategy.Bubble, typeof(KxStateRoutedEventHandler), typeof(StateTreeControl));
        public event KxStateRoutedEventHandler StateEdit
        {
            add { AddHandler(KxStateEditEvent, value); }
            remove { RemoveHandler(KxStateEditEvent, value); }
        }
        public event KxStateRoutedEventHandler StateRename
        {
            add { AddHandler(KxStateRenameEvent, value); }
            remove { RemoveHandler(KxStateRenameEvent, value); }
        }
        public event KxStateRoutedEventHandler StateDelete
        {
            add { AddHandler(KxStateDeleteEvent, value); }
            remove { RemoveHandler(KxStateDeleteEvent, value); }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public StateTreeControl()
            : base()
        {
            InitializeComponent();
            
            this.StateTreeView.DataContext = this;
        }

        /// <summary>
        /// Set the colour scheme
        /// </summary>
        /// <param name="scheme"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            _appConfig = appConfig;
            _expandPageLevel = appConfig.GetBoolVal(Constants.ConfigExpandPagesInSituationTree, Constants.DefaultExpandPagesInSituationTree);
        }

        /// <summary>
        /// Set the source
        /// </summary>
        /// <param name="profile"></param>
        public void SetSource(BaseSource source)
        {
            _source = source;

            RefreshDisplay();
        }

        /// <summary>
        /// Refresh the state tree
        /// </summary>
        public void RefreshDisplay()
        {
            if (_isLoaded && _source != null)
            {
                // State tree group box title
                StateTreeGroupBox.Header = string.Format("{0} {1} {2}", Properties.Resources.String_Player, _source.ID, Properties.Resources.String_control_sets);

                // Rebuild the tree
                _selectedTreeItem = null;
                _treeItems.Clear();
                AddTreeItems(null, 0, _source.StateTree);
                
                // Select tree item if possible
                if (_selectedState != null)
                {
                    if (!SelectTreeItemForState(_selectedState))
                    {
                        // Selected state no longer valid, so select root state
                        _selectedState = _source.StateTree.GetInitialState();
                        SelectTreeItemForState(_selectedState);
                    }
                }
            }
        }

        /// <summary>
        /// Set the current state
        /// </summary>
        /// <param name="state"></param>
        public void SetState(StateVector state)
        {
            try
            {                
                _selectedState = state;

                SelectTreeItemForState(state);
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_SetState, ex);
            }
        }

        /// <summary>
        /// Select the tree item corresponding to the selected state
        /// </summary>
        private bool SelectTreeItemForState(StateVector state)
        {
            // Find the item to select
            CustomBindingItem matchedItem = null;
            if (state != null)
            {
                foreach (CustomBindingItem treeItem in _treeItems)
                {
                    matchedItem = FindMatch(treeItem, state);
                    if (matchedItem != null)
                    {
                        break;
                    }
                }
            }

            // Deselect current state
            if (_selectedTreeItem != null)
            {
                _selectedTreeItem.IsSelected = false;
            }
            // Select the new item
            if (matchedItem != null)
            {
                matchedItem.IsSelected = true;

                // Make sure selected item is visible
                if (matchedItem.Parent != null)
                {
                    matchedItem.Parent.IsExpanded = true;
                }
            }

            return matchedItem != null;
        }

        /// <summary>
        /// Look for the first tree item with the specified state
        /// </summary>
        /// <param name="stateTreeItem"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private CustomBindingItem FindMatch(CustomBindingItem stateTreeItem, StateVector state)
        {
            CustomBindingItem matchedItem = null;

            StateVector stateTreeItemData = ((AxisValue)stateTreeItem.ItemData).Situation;
            if (stateTreeItemData.Contains(state))
            {
                matchedItem = stateTreeItem;
                if (!stateTreeItemData.IsSameAs(state))
                {
                    foreach (CustomBindingItem childItem in stateTreeItem.Children)
                    {
                        CustomBindingItem item = FindMatch(childItem, state);
                        if (item != null)
                        {
                            matchedItem = item;
                            StateVector matchedItemData = ((AxisValue)matchedItem.ItemData).Situation;
                            if (matchedItemData.IsSameAs(state))
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return matchedItem;
        }

        /// <summary>
        /// Control loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;

                // Show or hide edit controls sets button
                if (IsDesignMode)
                {
                    EditControlSetsButton.Visibility = Visibility.Visible;
                    Grid.SetRowSpan(StateTreeView, 1);
                }
                else
                {
                    EditControlSetsButton.Visibility = Visibility.Collapsed;
                    Grid.SetRowSpan(StateTreeView, 2);
                }

                RefreshDisplay();
            }
        }

        /// <summary>
        /// Selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StateTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (e.NewValue is CustomBindingItem && e.NewValue != _selectedTreeItem)
                {
                    // Get the current state
                    StateVector currentState = null;
                    if (_selectedTreeItem != null)
                    {
                        currentState = ((AxisValue)_selectedTreeItem.ItemData).Situation;
                    }

                    // Store new selection
                    _selectedTreeItem = (CustomBindingItem)e.NewValue;
                    _selectedState = ((AxisValue)_selectedTreeItem.ItemData).Situation;

                    // Raise event
                    RaiseEvent(new KxStateRoutedEventArgs(KxStateChangedEvent, _selectedState));
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_OnStateSelection, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Build the item tree recursively
        /// </summary>
        /// <param name="_rootItem"></param>
        /// <param name="axis"></param>
        private void AddTreeItems(CustomBindingItem parentItem, int level, AxisValue axisValue)
        {
            try
            {
                // Add a tree item for this state if required
                bool addItem = axisValue.ID != Constants.DefaultID || (IsDesignMode && level == 0);
                if (addItem)
                {
                    bool canEdit = IsDesignMode;
                    string toolTipText = Properties.Resources.String_ClickToView;
                    if (canEdit)
                    {
                        toolTipText += ", " + Properties.Resources.String_right_click_to_edit;
                    }

                    CustomBindingItem treeItem = new CustomBindingItem(axisValue,
                                                canEdit,
                                                true,
                                                toolTipText,
                                                parentItem);
                    if (parentItem != null)
                    {
                        parentItem.Children.Add(treeItem);
                    }
                    else
                    {
                        _treeItems.Add(treeItem);
                    }

                    parentItem = treeItem;
                }
                
                // Recurse for child states
                if (level < 2)
                {
                    foreach (AxisValue childValue in axisValue.SubValues)
                    {
                        AddTreeItems(parentItem, level + 1, childValue);
                    }
                }           
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_PopulateStateTree, ex);
            }
        }

        /// <summary>
        /// Handle right-click on tree item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Get the tree item that was right-clicked
                FrameworkElement control = (FrameworkElement)sender;
                CustomBindingItem treeItem = (CustomBindingItem)control.DataContext;

                // If the item isn't selected, select it
                if (treeItem != _selectedTreeItem)
                {
                    if (_selectedTreeItem != null)
                    {
                        _selectedTreeItem.IsSelected = false;
                    }
                    treeItem.IsSelected = true;
                }

                // Raise event
                KxStateRoutedEventArgs args = new KxStateRoutedEventArgs(KxStateEditEvent, ((AxisValue)treeItem.ItemData).Situation);
                RaiseEvent(args);
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_OnRightClickState, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Edit control sets button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditControlSetsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedState != null)
                {
                    // Raise event
                    KxStateRoutedEventArgs args = new KxStateRoutedEventArgs(KxStateEditEvent, _selectedState);
                    RaiseEvent(args);
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_OpenEditControlSetsMenu, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handle delete command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {                
                // Request a delete
                StateVector state = ((AxisValue)_selectedTreeItem.ItemData).Situation;
                if (state.ModeID != Constants.DefaultID)
                {
                    KxStateRoutedEventArgs args = new KxStateRoutedEventArgs(KxStateDeleteEvent, state);
                    RaiseEvent(args);
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_DeleteControlSet, ex);
            }

            e.Handled = true;
        }

        // Decide whether delete can execute
        private void DeleteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsDesignMode &&
                             (_selectedTreeItem != null) &&
                             (((AxisValue)_selectedTreeItem.ItemData).Situation.CellID == Constants.DefaultID);
            e.Handled = true;
        }

        /// <summary>
        /// Handle delete command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RenameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {                
                // Request a rename
                StateVector state = ((AxisValue)_selectedTreeItem.ItemData).Situation;
                if (state.ModeID != Constants.DefaultID)
                {
                    KxStateRoutedEventArgs args = new KxStateRoutedEventArgs(KxStateRenameEvent, state);
                    RaiseEvent(args);
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_RenameControlSet, ex);
            }

            e.Handled = true;
        }

        // Decide whether delete can execute
        public void RenameCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsDesignMode && 
                            (_selectedTreeItem != null) &&
                            (((AxisValue)_selectedTreeItem.ItemData).Situation.CellID == Constants.DefaultID);
            e.Handled = true;
        }

        /// <summary>
        /// Report an error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        private void ReportError(string message, Exception ex)
        {
            KxErrorRoutedEventArgs args = new KxErrorRoutedEventArgs(KxErrorEvent, new KxException(message, ex));
            RaiseEvent(args);
        }
    }
}
