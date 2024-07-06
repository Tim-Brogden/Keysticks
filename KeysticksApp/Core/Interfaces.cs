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
using System.IO;
using Keysticks.Config;
using Keysticks.Event;
using Keysticks.Sources;
using Keysticks.Sys;

namespace Keysticks.Core
{
    public interface IWindowParent
    {
        void ChildWindowClosing(Window window);
    }

    public interface IErrorHandler
    {
        void HandleError(string message, Exception ex);
        void ClearError();
    }

    public interface IThreadManager
    {
        void SubmitUIEvent(KxEventArgs args);
        void SubmitStateEvent(KxEventArgs args);
        void SubmitPredictionEvent(KxEventArgs args);
    }

    public interface IMainWindow : IWindowParent, IErrorHandler
    {
        IKeyboardContext OutputContext { get; }
        IThreadManager ThreadManager { get; }

        Profile GetProfile();
        ITrayManager GetTrayManager();
        MessageLogger GetMessageLogger();

        bool ApplyProfile(Profile profile);
        void SetAppConfig(AppConfig appConfig);

        ControllerWindow GetControllerWindow(int playerID);
        void ShowControllerWindows();
        void ShowControllerWindow(int playerID);
        void ShowHelpAboutWindow();

        void Close();
    }

    public interface ITrayManager : IWindowParent, IErrorHandler
    {
        IKeyboardContext InputContext { get; }
        IThreadManager ThreadManager { get; }

        void FileOpen_Click(object sender, EventArgs e);
        void EditProfile_Click(object sender, EventArgs e);
        void ToolsOptions_Click(object sender, EventArgs e);
        void ViewHelp_Click(object sender, EventArgs e);
        void ViewLogFile_Click(object sender, EventArgs e);

        bool LoadProfile(string filePath, bool isTemplate, bool checkForSave);
        void ApplyAppConfig(AppConfig appConfig);
        bool ApplyProfile(Profile profile);
        void KeyboardLayoutChanged(KxKeyboardChangeEventArgs args);
        bool PromptToSaveAndExit();
        void ProfileRenamed(FileInfo fromFile, FileInfo toFile);
        void ShowTemporaryMessage(string message, string caption, System.Windows.Forms.ToolTipIcon icon);
        void ShowContextMenu();
    }

    public interface IProfileEditorWindow : IWindowParent
    {
        Profile LoadTemplates(string filePath);
        void ActionsEdited();
        void RefreshActionEditor();
        void EnableActionEditor(bool enable);
        ITrayManager GetTrayManager();
    }

    /// <summary>
    /// Viewer window interface
    /// </summary>
    public interface IViewerWindow
    {        
        void SetAppConfig(AppConfig appConfig);
        void SetSource(BaseSource source);
        void KeyboardLayoutChanged(KxKeyboardChangeEventArgs args);
        void Close();               
    }

    /// <summary>
    /// Profile viewer tab control
    /// </summary>
    public interface IProfileDesignerControl : IWindowParent
    {
        void RefreshBackground();
        IProfileEditorWindow GetEditorWindow();
    }

    /// <summary>
    /// Interface for state manager
    /// </summary>
    public interface IStateManager
    {
        Profile CurrentProfile { get; }
        KeyPressManager KeyStateManager { get; }
        MouseManager MouseStateManager { get; }
        WordPredictionManager WordPredictionManager { get; }
        IThreadManager ThreadManager { get; }
        CellUtils CellManager { get; }
        IKeyboardContext KeyboardContext { get; }
        int PollingIntervalMS { get; }
        Rect CurrentWindowRect { get; }

        void SetCurrentState(int playerID, StateVector newState);

        void HandleKeyEvent(KxKeyEventArgs args);
        void HandleTextEvent(KxTextEventArgs args);
        void HandleKeyboardChangeEvent(KxKeyboardChangeEventArgs args);

        void AddOngoingActions(ActionList actionList);
    }

    public interface IKeyboardContext
    {
        IntPtr KeyboardHKL { get; }
    }

    /// <summary>
    /// Interface for controls which display a view of a profile
    /// </summary>
    public interface ISourceViewerControl
    {
        void SetAppConfig(AppConfig appConfig);
        void SetSource(BaseSource profile);
        void RefreshDisplay();
    }

    /// <summary>
    /// Interface for controls which display actions
    /// </summary>
    public interface IActionViewerControl : ISourceViewerControl
    {
        StateVector CurrentSituation { get; }
        KxControlEventArgs CurrentInputEvent { get; }

        void SetFocus();
        void SetCurrentSituation(StateVector situation);
        void AnimateInputEvent(KxSourceEventArgs args);
        void ClearAnimations();
    }
}
