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
using System.Windows;
using Keysticks.Config;
using Keysticks.Core;
using Keysticks.Sources;
using Keysticks.UI;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Context menu for editing the control set hierarchy
    /// </summary>
    public partial class EditSituationsContextMenu : KxUserControl
    {
        // Fields
        private IProfileEditorWindow _editorWindow;
        private BaseSource _source;
        private AppConfig _appConfig;
        private StateVector _state;
        
        public EditSituationsContextMenu()
            : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set the app configuration
        /// </summary>
        /// <param name="appConfig"></param>
        public void SetAppConfig(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        /// <summary>
        /// Set the source to edit
        /// </summary>
        /// <param name="profile"></param>
        public void SetSource(BaseSource source)
        {
            _source = source;
        }

        /// <summary>
        /// Set the profile editor window to report to
        /// </summary>
        /// <param name="profileEditor"></param>
        public void SetProfileEditor(IProfileEditorWindow editorWindow)
        {
            _editorWindow = editorWindow;
        }

        /// <summary>
        /// Set the situation to edit
        /// </summary>
        /// <param name="situation"></param>
        public void SetState(StateVector state)
        {
            _state = state;
        }

        /// <summary>
        /// Menu opening
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ShowContextMenu(UIElement placementControl)
        {
            // Check that the control has been configured
            if (_state == null)
            {
                // Error
                return;
            }

            AxisValue modeItem = _source.Utils.GetModeItem(_state);
            AxisValue pageItem = _source.Utils.GetPageItem(_state);
            bool isModeLevel = (_state.PageID == Constants.DefaultID);

            // Hide buttons as required
            if (isModeLevel)
            {
                CreateModeButton.Visibility = Visibility.Visible;
                ReorderModesButton.Visibility = (_source.StateTree.NumModes > 2) ? Visibility.Visible : Visibility.Collapsed;
                if (modeItem != null && modeItem.ID != Constants.DefaultID)
                {
                    RenameModeButton.Header = string.Format(Properties.Resources.String_RenameX + "...", modeItem.Name);
                    RenameModeButton.Visibility = Visibility.Visible;
                    CopyModeButton.Header = string.Format(Properties.Resources.String_CopyX + "...", modeItem.Name);
                    CopyModeButton.Visibility = Visibility.Visible;
                    bool autoConfirm = _appConfig.GetBoolVal(Constants.ConfigAutoConfirmDeleteControlSet, Constants.DefaultAutoConfirmDeleteControlSet); 
                    DeleteModeButton.Header = string.Format(Properties.Resources.String_DeleteX, modeItem.Name) + (autoConfirm ? "" : "...");
                    DeleteModeButton.Visibility = Visibility.Visible;
                }
                else
                {
                    RenameModeButton.Visibility = Visibility.Collapsed;
                    CopyModeButton.Visibility = Visibility.Collapsed;
                    DeleteModeButton.Visibility = Visibility.Collapsed;
                }
                RenamePageButton.Visibility = Visibility.Collapsed;
                CopyPageButton.Visibility = Visibility.Collapsed;
                DeletePageButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                CreateModeButton.Visibility = Visibility.Collapsed;
                ReorderModesButton.Visibility = Visibility.Collapsed;
                RenameModeButton.Visibility = Visibility.Collapsed;
                CopyModeButton.Visibility = Visibility.Collapsed;
                DeleteModeButton.Visibility = Visibility.Collapsed;
                if (pageItem != null && pageItem.ID != Constants.DefaultID)
                {
                    RenamePageButton.Header = string.Format(Properties.Resources.String_RenameX + "...", pageItem.Name);
                    RenamePageButton.Visibility = Visibility.Visible;
                    CopyPageButton.Header = string.Format(Properties.Resources.String_CopyX + "...", pageItem.Name);
                    CopyPageButton.Visibility = Visibility.Visible;
                    bool autoConfirm = _appConfig.GetBoolVal(Constants.ConfigAutoConfirmDeletePage, Constants.DefaultAutoConfirmDeletePage);
                    DeletePageButton.Header = string.Format(Properties.Resources.String_DeleteX, pageItem.Name) + (autoConfirm ? "" : "...");
                    DeletePageButton.Visibility = Visibility.Visible;
                }
                else
                {
                    RenamePageButton.Visibility = Visibility.Collapsed;
                    CopyPageButton.Visibility = Visibility.Collapsed;
                    DeletePageButton.Visibility = Visibility.Collapsed;
                }
            }

            if (modeItem != null && modeItem.ID != Constants.DefaultID)
            {
                ModesSeparator.Visibility = isModeLevel ? Visibility.Visible : Visibility.Collapsed;
                CreatePageButton.Header = string.Format(Properties.Resources.String_AddNewPageToX, modeItem.Name);
                CreatePageButton.Visibility = Visibility.Visible;
            }
            else
            {
                ModesSeparator.Visibility = Visibility.Collapsed;
                CreatePageButton.Visibility = Visibility.Collapsed;
            }

            if (modeItem != null && modeItem.SubValues.Count > 2)
            {
                ReorderPagesButton.Header = string.Format(Properties.Resources.String_ReorderPagesForX, modeItem.Name);
                ReorderPagesButton.Visibility = Visibility.Visible;
            }
            else
            {
                ReorderPagesButton.Visibility = Visibility.Collapsed;
            }

            // Open the menu
            this.TheContextMenu.PlacementTarget = placementControl;
            this.TheContextMenu.IsOpen = true;
        }

        /// <summary>
        /// Add control set clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateModeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_source.Profile.IsTemplate)
                {
                    CreateTemplateMode();
                }
                else
                {
                    CreateNewMode();
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_AddControlSet, ex);
            }

            e.Handled = true;
        }        

        /// <summary>
        /// Create a standard control set
        /// </summary>
        /// <param name="defaultInsertAfterID"></param>
        private void CreateNewMode()
        {
            int defaultInsertAfterID = _state.ModeID;
            AddControlSetWindow dialog = new AddControlSetWindow((Window)_editorWindow, Properties.Resources.String_ControlSet, _source.StateTree.SubValues, defaultInsertAfterID);
            if (dialog.ShowDialog() == true)
            {
                AxisValue newModeItem = _source.StateEditor.AddMode(dialog.ItemName, dialog.InsertAfterID, null, null, ETemplateGroup.None);
                
                RaiseEvent(new KxStateRoutedEventArgs(KxStatesEditedEvent, newModeItem.Situation));
            }
        }

        /// <summary>
        /// Create a new template control set
        /// </summary>
        private void CreateTemplateMode()
        {
            int defaultInsertAfterID = _state.ModeID;
            CreateTemplateWindow dialog = new CreateTemplateWindow((Window)_editorWindow, _source, defaultInsertAfterID);
            if (dialog.ShowDialog() == true)
            {
                // Create grid
                GridConfig grid = null;
                if (dialog.GridType != EGridType.None)
                {
                    grid = _source.StateEditor.CreateGridConfig(dialog.GridType, Constants.DefaultID, dialog.TemplateControls);
                }

                // Create mode
                AxisValue newModeItem = _source.StateEditor.AddMode(dialog.ModeName,
                                                                dialog.InsertAfterID,
                                                                dialog.TemplateControls,
                                                                grid,
                                                                dialog.TemplateGroup);

                // Add default page if it's a grid control set (required, so that the control set can have cells)
                if (grid != null)
                {
                    AxisValue pageItem = _source.StateEditor.AddDefaultPageToMode(newModeItem);
                    _source.StateEditor.AddCellsToPage(pageItem, grid);
                }

                // Update UI
                RaiseEvent(new KxStateRoutedEventArgs(KxStatesEditedEvent, newModeItem.Situation));
            }
        }

        /// <summary>
        /// Rename control set clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RenameModeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RenameMode();
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_RenameControlSet, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Rename control set
        /// </summary>
        public void RenameMode()
        {
            AxisValue modeItem = _source.Utils.GetModeItem(_state);
            if (modeItem != null && modeItem.ID != Constants.DefaultID)
            {
                ChooseNameWindow dialog = new ChooseNameWindow((Window)_editorWindow, Properties.Resources.String_ControlSet, modeItem.Name, _source.StateTree.SubValues);
                if (dialog.ShowDialog() == true)
                {
                    _source.StateEditor.RenameMode(modeItem.ID, dialog.SelectedName);

                    RaiseEvent(new KxStateRoutedEventArgs(KxStatesEditedEvent, null));
                }
            }
        }
      
        /// <summary>
        /// Copy control set clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyModeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CopyMode();
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_CopyControlSet, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Copy control set
        /// </summary>
        public void CopyMode()
        {
            AxisValue modeItem = _source.Utils.GetModeItem(_state);
            if (modeItem != null && modeItem.ID != Constants.DefaultID)
            {
                string suggestedName = _source.StateTree.SubValues.GetFirstUnusedName(modeItem.Name, false, true, 2);
                ChooseNameWindow dialog = new ChooseNameWindow((Window)_editorWindow, Properties.Resources.String_ControlSet, suggestedName, _source.StateTree.SubValues);
                if (dialog.ShowDialog() == true)
                {
                    AxisValue modeCopy = _source.StateEditor.CopyMode(modeItem, dialog.SelectedName, modeItem.ID);
                    if (modeCopy != null)
                    {
                        // Copy actions for mode
                        _source.ActionEditor.CopyActions(_source, modeItem.Situation, modeCopy.Situation);

                        // Select copied mode
                        RaiseEvent(new KxStateRoutedEventArgs(KxStatesEditedEvent, modeCopy.Situation));
                    }
                }
            }
        }

        /// <summary>
        /// Delete control set clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteModeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteMode();
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_DeleteControlSet, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Delete control set
        /// </summary>
        public void DeleteMode()
        {
            AxisValue modeItem = _source.Utils.GetModeItem(_state);
            if (modeItem != null && modeItem.ID != Constants.DefaultID)
            {
                // Confirm if required
                bool confirmed = _appConfig.GetBoolVal(Constants.ConfigAutoConfirmDeleteControlSet, Constants.DefaultAutoConfirmDeleteControlSet);
                if (!confirmed)
                {
                    CustomMessageBox messageBox = new CustomMessageBox((Window)_editorWindow, Properties.Resources.Q_DeleteControlSetMessage, Properties.Resources.Q_DeleteControlSet, MessageBoxButton.OKCancel, true, true);
                    if (messageBox.ShowDialog() == true)
                    {
                        confirmed = true;
                        if (messageBox.DontAskAgain)
                        {
                            _appConfig.SetBoolVal(Constants.ConfigAutoConfirmDeleteControlSet, true);
                        }
                    }
                }

                if (confirmed)
                {
                    // Remove the mode
                    _source.StateEditor.DeleteMode(modeItem.ID);

                    RaiseEvent(new KxStateRoutedEventArgs(KxStatesEditedEvent, null));
                }
            }
        }

        /// <summary>
        /// Reorder control sets clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReorderModesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReorderItemsWindow dialog = new ReorderItemsWindow((Window)_editorWindow, Properties.Resources.String_ControlSet, _source.StateTree.SubValues);
                if (dialog.ShowDialog() == true)
                {
                    _source.IsModified = true;
                    RaiseEvent(new KxStateRoutedEventArgs(KxStatesEditedEvent, null));
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_ReorderControlSets, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Add page (sub-control set) clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreatePageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CreatePage();
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_AddPage, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Add a page (sub-control set)
        /// </summary>
        public void CreatePage()
        {
            AxisValue modeItem = _source.Utils.GetModeItem(_state);
            if (modeItem != null && modeItem.ID != Constants.DefaultID)
            {
                int insertAfterID = _state.PageID;
                AddControlSetWindow dialog = new AddControlSetWindow((Window)_editorWindow, Properties.Resources.String_Page, modeItem.SubValues, insertAfterID);
                if (dialog.ShowDialog() == true)
                {
                    // If the mode doesn't have any pages yet, create the default page too
                    AxisValue newPageItem;
                    if (modeItem.SubValues.Count == 0)
                    {
                        newPageItem = _source.StateEditor.AddDefaultPageToMode(modeItem);
                        _source.StateEditor.AddCellsToPage(newPageItem, modeItem.Grid);
                    }

                    int newPageID = modeItem.SubValues.GetFirstUnusedID(1);
                    newPageItem = _source.StateEditor.AddPageToMode(modeItem, newPageID, dialog.ItemName, dialog.InsertAfterID);
                    _source.StateEditor.AddCellsToPage(newPageItem, modeItem.Grid);

                    RaiseEvent(new KxStateRoutedEventArgs(KxStatesEditedEvent, newPageItem.Situation));
                }
            }
        }

        /// <summary>
        /// Rename a page (sub-control set) clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RenamePageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RenamePage();               
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_RenamePage, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Rename page (sub-control set)
        /// </summary>
        public void RenamePage()
        {
            AxisValue modeItem = _source.Utils.GetModeItem(_state);
            AxisValue pageItem = _source.Utils.GetPageItem(_state);
            if (modeItem != null && pageItem != null && pageItem.ID != Constants.DefaultID)
            {
                ChooseNameWindow dialog = new ChooseNameWindow((Window)_editorWindow, Properties.Resources.String_Page, pageItem.Name, modeItem.SubValues);
                if (dialog.ShowDialog() == true)
                {
                    _source.StateEditor.RenamePage(modeItem.ID, pageItem.ID, dialog.SelectedName);

                    RaiseEvent(new KxStateRoutedEventArgs(KxStatesEditedEvent, null));
                }
            }
        }

        /// <summary>
        /// Copy page (sub-control set) clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyPageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CopyPage();
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_CopyPage, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Copy page (sub-control set)
        /// </summary>
        public void CopyPage()
        {
            AxisValue modeItem = _source.Utils.GetModeItem(_state);
            AxisValue pageItem = _source.Utils.GetPageItem(_state);
            if (modeItem != null && pageItem != null && pageItem.ID != Constants.DefaultID)
            {
                string suggestedName = modeItem.SubValues.GetFirstUnusedName(pageItem.Name, false, true, 2);
                ChooseNameWindow dialog = new ChooseNameWindow((Window)_editorWindow, Properties.Resources.String_Page, suggestedName, modeItem.SubValues);
                if (dialog.ShowDialog() == true)
                {
                    AxisValue pageCopy = _source.StateEditor.CopyPage(pageItem, dialog.SelectedName, modeItem.ID, pageItem.ID);
                    if (pageCopy != null)
                    {
                        // Copy actions for page
                        _source.ActionEditor.CopyActions(_source, pageItem.Situation, pageCopy.Situation);                    

                        RaiseEvent(new KxStateRoutedEventArgs(KxStatesEditedEvent, pageCopy.Situation));
                    }
                }
            }
        }

        /// <summary>
        /// Delete page (sub-control set) clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeletePageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeletePage();
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_DeletePage, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Delete page (sub-control set)
        /// </summary>
        public void DeletePage()
        {
            AxisValue modeItem = _source.Utils.GetModeItem(_state);
            AxisValue pageItem = _source.Utils.GetPageItem(_state);
            if (modeItem != null && pageItem != null && pageItem.ID != Constants.DefaultID)
            {
                // Confirm if required
                bool confirmed = _appConfig.GetBoolVal(Constants.ConfigAutoConfirmDeletePage, Constants.DefaultAutoConfirmDeletePage);
                if (!confirmed)
                {
                    CustomMessageBox messageBox = new CustomMessageBox((Window)_editorWindow, Properties.Resources.Q_DeletePageMessage, Properties.Resources.Q_DeletePage, MessageBoxButton.OKCancel, true, true);
                    if (messageBox.ShowDialog() == true)
                    {
                        confirmed = true;
                        if (messageBox.DontAskAgain)
                        {
                            _appConfig.SetBoolVal(Constants.ConfigAutoConfirmDeletePage, true);
                        }
                    }
                }

                if (confirmed)
                {
                    _source.StateEditor.DeletePage(modeItem.ID, pageItem.ID);

                    RaiseEvent(new KxStateRoutedEventArgs(KxStatesEditedEvent, null));
                }
            }
        }

        /// <summary>
        /// Reorder pages clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReorderPagesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AxisValue modeItem = _source.Utils.GetModeItem(_state);
                if (modeItem != null && modeItem.ID != Constants.DefaultID)
                {
                    ReorderItemsWindow dialog = new ReorderItemsWindow((Window)_editorWindow, Properties.Resources.String_Page, modeItem.SubValues);
                    if (dialog.ShowDialog() == true)
                    {
                        _source.IsModified = true;
                        RaiseEvent(new KxStateRoutedEventArgs(KxStatesEditedEvent, null));
                    }
                }
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_ReorderPages, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Import control sets clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ImportControlSets();
            }
            catch (Exception ex)
            {
                ReportError(Properties.Resources.E_ImportControlSets, ex);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Import control sets from another profile / player
        /// </summary>
        private void ImportControlSets()
        {
            ImportWindow dialog = new ImportWindow(_source);
            dialog.ItemName = Properties.Resources.String_control_sets;
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Get the profile to import from
                    Profile fromProfile;
                    if (dialog.IsFromFile)
                    {
                        fromProfile = new Profile();
                        fromProfile.FromFile(dialog.FilePath);
                    }
                    else
                    {
                        fromProfile = _source.Profile;
                    }

                    int fromSourceID = dialog.FromSourceID;
                    BaseSource fromSource = fromProfile.GetSource(fromSourceID);
                    if (fromSource != null)
                    {
                        // Loop over modes in the source
                        int insertAfterID = _source.StateTree.SubValues.FindPreviousID(Constants.DefaultID); // This gets the last ID
                        IEnumerator<NamedItem> eMode = fromSource.StateTree.SubValues.GetEnumerator();
                        while (eMode.MoveNext())
                        {
                            // Don't copy the 'All' control set
                            AxisValue fromMode = (AxisValue)eMode.Current;
                            AxisValue toMode;
                            if (fromMode.ID != Constants.DefaultID)
                            {
                                // Copy mode
                                toMode = _source.StateEditor.CopyMode(fromMode, fromMode.Name, insertAfterID);
                                if (toMode != null)
                                {
                                    insertAfterID = toMode.ID;

                                    // Copy actions
                                    _source.ActionEditor.CopyActions(fromSource,
                                                                        fromMode.Situation,
                                                                        toMode.Situation);
                                }
                            }                            
                        }

                        // Copy actions for 'All' control set
                        _source.ActionEditor.CopyRootActions(fromSource);
                        
                        // Validate
                        _source.Validate();

                        RaiseEvent(new KxStateRoutedEventArgs(KxStatesEditedEvent, null));
                    }
                    else
                    {
                        string message = string.Format(Properties.Resources.E_NoControlsFound, fromProfile.Name, fromSourceID);
                        ReportError(message, null);
                    }
                }
                catch (Exception ex)
                {
                    ReportError(Properties.Resources.E_ImportControlSets, ex);
                }
            }
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
