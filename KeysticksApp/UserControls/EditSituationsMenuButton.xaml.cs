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
using FeetUpCore.Config;
using FeetUpCore.Core;
using FeetUpCore.Event;

namespace FeetUp.UserControls
{
    /// <summary>
    /// Interaction logic for EditSituationsMenuButton.xaml
    /// </summary>
    public partial class EditSituationsMenuButton : UserControl
    {
        // Members
        private IProfileEditorWindow _parentWindow;

        /// <summary>
        /// Constructor
        /// </summary>
        public EditSituationsMenuButton()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set the current situation
        /// </summary>
        /// <param name="situation"></param>
        public void SetParent(IProfileEditorWindow parent)
        {
            _parentWindow = parent;
        }

        /// <summary>
        /// Menu opening
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditSituationsContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            Profile profile = _parentWindow.GetProfile();
            StateVector situation = _parentWindow.GetSelectedState();
            if (profile == null || situation == null)
            {
                this.EditSituationsContextMenu.IsOpen = false;
                return;
            }

            AxisValue modeItem = GetCurrentMode(profile, situation);
            AxisValue pageItem = GetCurrentPage(profile, situation);
            
            // Hide buttons as required
            if (modeItem != null)
            {
                this.RenameModeButton.Header = string.Format("Rename '{0}' mode", modeItem.Name);
                this.DeleteModeButton.Header = string.Format("Delete '{0}' mode", modeItem.Name);
                this.FirstSeparator.Visibility = Visibility.Visible;
                this.RenameModeButton.Visibility = Visibility.Visible;
                this.DeleteModeButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.FirstSeparator.Visibility = Visibility.Collapsed;
                this.RenameModeButton.Visibility = Visibility.Collapsed;
                this.DeleteModeButton.Visibility = Visibility.Collapsed;
            }

            bool createPageAllowed = (modeItem != null) && (modeItem.GridType != EGridType.None);
            if (createPageAllowed)
            {
                this.CreatePageButton.Header = string.Format("Add a page to '{0}' mode", modeItem.Name);
                this.CreatePageButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.CreatePageButton.Visibility = Visibility.Collapsed;
            }

            if (pageItem != null)
            {
                this.RenamePageButton.Header = string.Format("Rename '{0}'", pageItem.Name);
                this.DeletePageButton.Header = string.Format("Delete '{0}'", pageItem.Name);
                this.SecondSeparator.Visibility = Visibility.Visible;
                this.RenamePageButton.Visibility = Visibility.Visible;
                this.DeletePageButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.SecondSeparator.Visibility = Visibility.Collapsed;
                this.RenamePageButton.Visibility = Visibility.Collapsed;
                this.DeletePageButton.Visibility = Visibility.Collapsed;
            }

            if (!situation.IsSameAs(profile.InitialState))
            {
                this.ThirdSeparator.Visibility = Visibility.Visible;
                this.StartupSituationButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.ThirdSeparator.Visibility = Visibility.Collapsed;
                this.StartupSituationButton.Visibility = Visibility.Collapsed;
            }

            e.Handled = true;
        }

        /// <summary>
        /// Get the current mode ('Any' mode not included)
        /// </summary>
        /// <returns></returns>
        private AxisValue GetCurrentMode(Profile profile, StateVector situation)
        {
            AxisValue modeItem = null;
            if (situation != null)
            {
                AxisValue modeAxisValue = (AxisValue)profile.StateTree.GetItemByID(situation.GetAxisValue(Constants.ModeAxisID));
                if (modeAxisValue != null && modeAxisValue.ID > 0)
                {
                    modeItem = modeAxisValue;
                }
            }

            return modeItem;
        }

        /// <summary>
        /// Get the current page  ('Any' page not included)
        /// </summary>
        /// <returns></returns>
        private AxisValue GetCurrentPage(Profile profile, StateVector situation)
        {
            AxisValue pageItem = null;
            AxisValue modeAxisValue = (AxisValue)GetCurrentMode(profile, situation);
            if (modeAxisValue != null)
            {
                AxisValue pageAxisValue = (AxisValue)modeAxisValue.SubValues.GetItemByID(situation.GetAxisValue(Constants.PageAxisID));
                if (pageAxisValue != null && pageAxisValue.ID > 0)
                {
                    pageItem = pageAxisValue;
                }
            }

            return pageItem;
        }

        /// <summary>
        /// Allow menu to be opened using left-click too
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditSituationsButton_Click(object sender, RoutedEventArgs e)
        {
            this.EditSituationsContextMenu.PlacementTarget = this;
            this.EditSituationsContextMenu.IsOpen = true;
        }

        private void CreateModeButton_Click(object sender, RoutedEventArgs e)
        {
            Profile profile = _parentWindow.GetProfile();
            StateVector situation = _parentWindow.GetSelectedState();
            CreateModeWindow dialog = new CreateModeWindow(profile.StateTree);
            if (dialog.ShowDialog() == true)
            {
                ProfileSituationEditor profileEditor = new ProfileSituationEditor(profile);
                profileEditor.AddMode(dialog.SelectedName, dialog.SelectedInsertPos, dialog.SelectedPageType);

                _parentWindow.SituationsEdited();
            }
        }

        private void RenameModeButton_Click(object sender, RoutedEventArgs e)
        {
            Profile profile = _parentWindow.GetProfile();
            StateVector situation = _parentWindow.GetSelectedState();
            AxisValue modeItem = GetCurrentMode(profile, situation);
            if (modeItem != null)
            {
                ChooseNameWindow dialog = new ChooseNameWindow("mode", profile.StateTree);
                if (dialog.ShowDialog() == true)
                {
                    ProfileSituationEditor profileEditor = new ProfileSituationEditor(profile);
                    profileEditor.RenameMode(modeItem.ID, dialog.SelectedName);

                    _parentWindow.SituationsEdited();
                }
            }
        }

        private void DeleteModeButton_Click(object sender, RoutedEventArgs e)
        {
            Profile profile = _parentWindow.GetProfile();
            StateVector situation = _parentWindow.GetSelectedState();
            AxisValue modeItem = GetCurrentMode(profile, situation);
            if (modeItem != null)
            {
                // Confirm with user
                MessageBoxResult result =
                    System.Windows.MessageBox.Show("Delete this mode and all its pages of controls?", "Delete mode?", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.OK)
                {
                    // Remove the mode
                    ProfileSituationEditor profileEditor = new ProfileSituationEditor(profile);
                    profileEditor.DeleteMode(modeItem.ID);

                    _parentWindow.SituationsEdited();
                }
            }
        }

        private void CreatePageButton_Click(object sender, RoutedEventArgs e)
        {
            Profile profile = _parentWindow.GetProfile();
            StateVector situation = _parentWindow.GetSelectedState();
            AxisValue modeItem = GetCurrentMode(profile, situation);
            if (modeItem != null)
            {
                CreatePageWindow dialog = new CreatePageWindow(modeItem.SubValues);
                if (dialog.ShowDialog() == true)
                {
                    ProfileSituationEditor profileEditor = new ProfileSituationEditor(profile);
                    profileEditor.AddPageToMode(modeItem.ID, dialog.SelectedName, dialog.SelectedInsertPos);

                    _parentWindow.SituationsEdited();
                }
            }
        }
        
        private void RenamePageButton_Click(object sender, RoutedEventArgs e)
        {
            Profile profile = _parentWindow.GetProfile();
            StateVector situation = _parentWindow.GetSelectedState();
            AxisValue modeItem = GetCurrentMode(profile, situation);
            AxisValue pageItem = GetCurrentPage(profile, situation);
            if (modeItem != null && pageItem != null)
            {
                ChooseNameWindow dialog = new ChooseNameWindow("page", modeItem.SubValues);
                if (dialog.ShowDialog() == true)
                {
                    ProfileSituationEditor profileEditor = new ProfileSituationEditor(profile);
                    profileEditor.RenamePage(modeItem.ID, pageItem.ID, dialog.SelectedName);

                    _parentWindow.SituationsEdited();
                }
            }
        }

        private void DeletePageButton_Click(object sender, RoutedEventArgs e)
        {
            Profile profile = _parentWindow.GetProfile();
            StateVector situation = _parentWindow.GetSelectedState();
            AxisValue modeItem = GetCurrentMode(profile, situation);
            AxisValue pageItem = GetCurrentPage(profile, situation);
            if (modeItem != null && pageItem != null)
            {
                // Confirm with user
                MessageBoxResult result =
                    System.Windows.MessageBox.Show("Delete this page of controls?", "Delete page?", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.OK)
                {                    
                    ProfileSituationEditor profileEditor = new ProfileSituationEditor(profile);
                    profileEditor.DeletePage(modeItem.ID, pageItem.ID);

                    _parentWindow.SituationsEdited();
                }
            }
        }

        /// <summary>
        /// Set which situation to start the profile in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartupSituationButton_Click(object sender, RoutedEventArgs e)
        {
            Profile profile = _parentWindow.GetProfile();
            StateVector situation = _parentWindow.GetSelectedState();
            ProfileSituationEditor profileEditor = new ProfileSituationEditor(profile);
            profileEditor.SetInitialState(new StateVector(situation));

            _parentWindow.InitialStateEdited();
        }
    }
}
