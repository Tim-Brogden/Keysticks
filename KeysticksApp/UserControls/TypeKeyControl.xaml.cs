using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Keysticks.Core;
using Keysticks.Actions;
using Keysticks.UI;

namespace Keysticks.UserControls
{
    public partial class TypeKeyControl : UserControl
    {
        // Members
        private NamedItemList _keyListItems = new NamedItemList();
        private TypeKeyAction _currentAction = new TypeKeyAction();

        /// <summary>
        /// Constructor
        /// </summary>
        public TypeKeyControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Control loaded event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GUIUtils.PopulateDisplayableListWithKeys(_keyListItems);
            this.KeyboardKeyCombo.ItemsSource = _keyListItems;

            RefreshDisplay();
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is TypeKeyAction)
            {
                _currentAction = (TypeKeyAction)action;
                RefreshDisplay();
            }
        }

        private void RefreshDisplay()
        {
            if (IsLoaded && _currentAction != null)
            {
                this.KeyboardKeyCombo.SelectedValue = _currentAction.UniqueKeyID;
                this.AltCheck.IsChecked = _currentAction.IsAltModifierSet;
                this.ControlCheck.IsChecked = _currentAction.IsControlModifierSet;
                this.ShiftCheck.IsChecked = _currentAction.IsShiftModifierSet;
                this.WinCheck.IsChecked = _currentAction.IsWinModifierSet;
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = null;

            NamedItem keyboardKey = (NamedItem)this.KeyboardKeyCombo.SelectedItem;
            if (keyboardKey != null)
            {
                _currentAction = new TypeKeyAction();
                _currentAction.UniqueKeyID = keyboardKey.ID;
                _currentAction.IsAltModifierSet = (this.AltCheck.IsChecked == true);
                _currentAction.IsControlModifierSet = (this.ControlCheck.IsChecked == true);
                _currentAction.IsShiftModifierSet = (this.ShiftCheck.IsChecked == true);
                _currentAction.IsWinModifierSet = (this.WinCheck.IsChecked == true);
            }

            return _currentAction;
        }

    }
}
