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
    public partial class ToggleKeyControl : UserControl
    {
        // Members
        private NamedItemList _keyListItems = new NamedItemList();
        private ToggleKeyAction _currentAction = new ToggleKeyAction();

        /// <summary>
        /// Constructor
        /// </summary>
        public ToggleKeyControl()
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
            GUIUtils.PopulateDisplayableListWithKeys(_keyListItems, true);
            this.KeyToToggleCombo.ItemsSource = _keyListItems;

            RefreshDisplay();
        }

        /// <summary>
        /// Handle change of input language
        /// </summary>
        public void InputLanguageChanged()
        {
            GUIUtils.PopulateDisplayableListWithKeys(_keyListItems, true);
            RefreshDisplay();
        }
        
        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is ToggleKeyAction)
            {
                _currentAction = (ToggleKeyAction)action;
                RefreshDisplay();
            }
        }

        private void RefreshDisplay()
        {
            if (IsLoaded && _currentAction != null)
            { 
                this.KeyToToggleCombo.SelectedValue = _currentAction.UniqueKeyID;
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = null;

            NamedItem keyToToggle = (NamedItem)this.KeyToToggleCombo.SelectedItem;
            if (keyToToggle != null)
            {
                _currentAction = new ToggleKeyAction();
                _currentAction.UniqueKeyID = keyToToggle.ID;
            }

            return _currentAction;
        }

    }
}
