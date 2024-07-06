/*
Alt Controller
--------------
Copyright 2011 T C Brogden Limited
www.tcbrogden.co.uk

Description
-----------
A free program for mapping computer inputs, such as pointer movements and button presses, 
to actions, such as key presses. The aim of this program is to help make third-party programs,
such as computer games, more accessible to users with physical difficulties.

License
-------
This file is part of Alt Controller. 
Alt Controller is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Alt Controller is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Alt Controller.  If not, see <http://www.gnu.org/licenses/>.
*/
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
using FeetUpCore.Config;
using FeetUpCore.Core;
using FeetUpCore.Actions;
using FeetUpCore.Input;

namespace FeetUp.UserControls
{
    public partial class ShowOrHideControlsControl : UserControl
    {
        // Members
        private ShowOrHideControlsAction _currentAction = new ShowOrHideControlsAction();

        /// <summary>
        /// Constructor
        /// </summary>
        public ShowOrHideControlsControl()
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
            RefreshDisplay();
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is ShowOrHideControlsAction)
            {
                _currentAction = (ShowOrHideControlsAction)action;
                RefreshDisplay();
            }
        }

        private void RefreshDisplay()
        {
            if (IsLoaded && _currentAction != null)
            {
                switch (_currentAction.WindowState)
                {
                    case EWindowEventType.Show:
                        this.ShowRadioButton.IsChecked = true; break;
                    case EWindowEventType.Hide:
                        this.HideRadioButton.IsChecked = true; break;
                    //case EWindowEventType.Toggle:
                    //default:
                    //    this.ToggleRadioButton.IsChecked = true; break;
                }
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = new ShowOrHideControlsAction();
            if (this.ShowRadioButton.IsChecked == true)
            {
                _currentAction.WindowState = EWindowEventType.Show;
            }
            else if (this.HideRadioButton.IsChecked == true)
            {
                _currentAction.WindowState = EWindowEventType.Hide;
            }
            //else
            //{
            //    _currentAction.WindowState = EWindowEventType.Toggle;
            //}

            return _currentAction;
        }

    }
}
