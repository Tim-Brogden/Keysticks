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

namespace Keysticks.Core
{    
    /// <summary>
    /// Types of virtual control
    /// </summary>
    public enum EVirtualControlType
    {
        Button,
        ButtonDiamond,
        DPad,
        Stick,
        Trigger
    }

    /// <summary>
    /// Type of control background
    /// </summary>
    public enum EBackgroundType
    {
        Default,
        Colour,
        Image
    }

    /// <summary>
    /// Type of device
    /// </summary>
    public enum EDeviceType
    {
        Controller,
        Gamepad,
        Wheel,
        ArcadeStick,
        FlightStick,
        DancePad,
        Guitar,
        DrumKit,
        Mouse,
        Keyboard,
        Joystick
    }

    /// <summary>
    /// Types of physical control
    /// </summary>
    public enum EPhysicalControlType
    {
        POV,
        Axis,        
        Slider,
        Button
    }

    /// <summary>
    /// Options for how to interpret an axis or slider value
    /// </summary>
    public enum EPhysicalControlOption
    {
        None,
        Inverted,
        PositiveSide,
        NegativeSide
    }

    /// <summary>
    /// Configuration settings for controls
    /// </summary>
    public enum EControlSetting
    {
        None,
        DirectionMode,
        DwellAndRepeat
    }
    
    /// <summary>
    /// Reasons for raising an event
    /// Note: The order Long, Auto-repeat, Short, Release / Undirected is intentional - gets the icons / descriptions right most times
    /// </summary>
    public enum EEventReason
    {
        None,
        Directed,
        DirectedLong,
        DirectionRepeated,
        DirectedShort,
        Undirected,
        Moved,
        Pressed,
        PressedLong,
        PressRepeated,
        PressedShort,
        Released,
        Activated        
    }

    /// <summary>
    /// UI editing events
    /// </summary>
    public enum EEditingEvent
    {
        SituationChanged,
        SituationsEdited,
        InputEventChanged,
        RightClicked,
        DragDropped,
        MetaDataEdited
    }

    /// <summary>
    /// Type of web service request
    /// </summary>
    public enum EWebServiceRequestType
    {
        None,
        Licence,
        News,
        Message
    }

    /// <summary>
    /// Type of web service response
    /// </summary>
    public enum EWebServiceResponseType
    {
        None,
        Licence,
        News,
        Message
    }

    /// <summary>
    /// Profile status
    /// </summary>
    public enum EProfileStatus
    {
        None,
        Local,
        OnlineDownloaded,
        OnlinePreviouslyDownloaded,
        OnlineNotDownloaded
    }

    /// <summary>
    /// Command security options
    /// </summary>
    public enum ECommandAction
    {
        None,
        Run,
        DontRun,
        AskMe
    }

    /// <summary>
    /// Axis combinations
    /// </summary>
    public enum EAxisCombination
    {
        None,
        XOnly,
        YOnly,
        Both
    }

    /// <summary>
    /// Icons
    /// </summary>
    public enum EAnnotationImage
    {
        None,
        DontShow,
        KLogo,
        Accept,
        CentrePosition,
        LeftDirection,
        RightDirection,
        UpDirection,
        DownDirection,
        UpLeftDirection,
        UpRightDirection,
        DownLeftDirection,
        DownRightDirection,
        ChangeCell,
        LeftMouseButton,
        MiddleMouseButton,
        RightMouseButton,
        X1MouseButton,
        X2MouseButton,
        MouseWheelUp,
        MouseWheelDown,
        Mouse,
        MousePointer,
        MousePointerAbsolute,
        MousePointerFixedSpeed,
        MousePointerRelative,
        NextControlSet,
        PreviousControlSet,
        ChangeControlSet,
        NextPage,
        PreviousPage,
        ChangePage,
        NewProfile,
        LoadProfile,
        StartProgram,
        ActivateWindow,
        MaximiseWindow,
        MinimiseWindow,
        Controller,
        Wait,
        NextSuggestion,
        PreviousSuggestion,
        InsertSuggestion,
        CancelSuggestions,
        WindowsKey,
        ApplicationsKey,
        LeftArrow,
        RightArrow,
        UpArrow,
        DownArrow,
        HoldLetterKey,
        HoldNumberKey,
        HoldSymbolKey,
        HoldArrowKey,
        HoldFunctionKey,
        HoldNumpadKey,
        HoldOtherKey,
        TypeLetterKey,
        TypeShiftedLetterKey,
        TypeNumberKey,
        TypeShiftedNumberKey,
        TypeSymbolKey,
        TypeShiftedSymbolKey,
        TypeArrowKey,
        TypeFunctionKey,
        TypeNumpadKey,
        TypeOtherKey,
        AutorepeatLetterKey,
        AutorepeatNumberKey,
        AutorepeatSymbolKey,
        AutorepeatArrowKey,
        AutorepeatFunctionKey,
        AutorepeatNumpadKey,
        AutorepeatOtherKey,
        ToggleKey,
        Clock,
        DirectionMode,
        Combination,
        NextTrack,
        PreviousTrack,
        PlayPause,
        StopPlaying,
        FastForward,
        Rewind,
        Calculator,
        Mail,
        Explorer,
        Browser,
        BrowserBack,
        BrowserStop,
        BrowserFavourites,
        BrowserForward,
        BrowserHome,
        BrowserRefresh,
        BrowserSearch,
        MediaPlayer,
        VolumeUp,
        VolumeDown,
        VolumeMute,
        BulletRed,
        BulletYellow,
        BulletOrange,
        BulletGreen,
        BulletBlue,
        BulletWhite,
        OpenFile,
        OpenFolder,
        SaveFile,
        EditFile,
        Help,
        ViewText,
        Settings,
        ProgramUpdates,
        Information,
        Exit,
        DoNothing
    }

    /// <summary>
    /// Ways in which a button or directional control can be pressed
    /// </summary>
    public enum EPressType
    {
        Any,
        Short,
        Long,
        AutoRepeat
    }

    /// <summary>
    /// Buttons
    /// </summary>
    [Flags]
    public enum EButtonState
    {
        //None = 0,
        A = 1,
        B = 2,
        X = 3,
        Y = 4,
        LeftShoulder = 5,
        RightShoulder = 6,
        Back = 7,
        Start = 8,
        LeftThumb = 9,
        RightThumb = 10
    }

    /// <summary>
    /// DirectInput axes
    /// </summary>
    public enum EAxis
    {
        None = 0,
        X1 = 1,
        //PositionAxes = X1,
        Y1 = 2,
        Z1 = 3,
        X2 = 4,
        Y2 = 5,
        Z2 = 6,
        X3 = 7,
        //VelocityAxes = X3,
        Y3 = 8,
        Z3 = 9,
        X4 = 10,
        Y4 = 11,
        Z4 = 12,
        X5 = 13,
        //AccelerationAxes = X5,
        Y5 = 14,
        Z5 = 15,
        X6 = 16,
        Y6 = 17,
        Z6 = 18,
        X7 = 19,
        //ForceAxes = X7,
        Y7 = 20,
        Z7 = 21,
        X8 = 22,
        Y8 = 23,
        Z8 = 24,
        Count = 25
    }
    
    /// <summary>
    /// DirectInput slider types
    /// </summary>
    public enum ESliderType
    {
        None,
        Position,
        Velocity,
        Acceleration,
        Force
    }

    /// <summary>
    /// Mouse buttons
    /// </summary>
    [Flags]
    public enum EMouseState
    {
        None = 0,
        Left = 1,
        Middle = 2,
        Right = 4,
        X1 = 8,
        X2 = 16
    }

    /// <summary>
    /// Left-right-up-down states
    /// </summary>
    [Flags]
    public enum ELRUDState
    {
        None = 0,
        Centre = 1,
        Left = 2,
        Right = 4,
        Up = 8,
        Down = 16,
        UpLeft = Up | Left,
        UpRight = Up | Right,
        DownLeft = Down | Left,
        DownRight = Down | Right
    }

    /// <summary>
    /// Types of action that can be performed
    /// </summary>
    public enum EActionType
    {
        TypeKey,
        PressDownKey,
        ReleaseKey,
        ToggleKey,
        TypeText,
        ClickMouseButton,
        DoubleClickMouseButton,
        PressDownMouseButton,
        ReleaseMouseButton,
        ToggleMouseButton,
        MouseWheelUp,
        MouseWheelDown,
        MoveThePointer,
        ControlThePointer,
        ChangeControlSet,
        NavigateCells,
        WordPrediction,
        LoadProfile,
        StartProgram,
        ActivateWindow,
        MaximiseWindow,
        MinimiseWindow,
        ToggleControlsWindow,
        SetDirectionMode,
        SetDwellAndAutorepeat,
        Wait,
        DoNothing
    }

    /// <summary>
    /// Types of mouse button action
    /// </summary>
    public enum EMouseButtonActionType
    {
        Click,
        DoubleClick,
        PressDown,
        Release,
        Toggle
    }

    /// <summary>
    /// Types of word suggestion action that can be performed
    /// </summary>
    public enum EWordPredictionEventType
    {
        None,
        NextSuggestion,
        PreviousSuggestion,
        InsertSuggestion,
        CancelSuggestions,
        SuggestionsList,
        Enable,
        Disable
    }

    /// <summary>
    /// Window visibility events
    /// </summary>
    public enum EWindowEventType
    {
        Restore = 9,    // SW_RESTORE
        Maximise = 3,   // SW_MAXIMISE
        Minimise = 6,   // SW_MINIMISE
        Show = 5        // SW_SHOW
    }

    /// <summary>
    /// Type of match for window title
    /// </summary>
    public enum EMatchType
    {
        Equals,
        StartsWith,
        EndsWith
    }

    /// <summary>
    /// Types of event that can be raised
    /// </summary>
    public enum EEventType
    {
        Unknown,
        Control,
        Source,
        StateChange,
        AppChange,
        LoadProfile,
        Key,
        RepeatKey,
        Text,
        MouseButtonState,
        KeyboardState,
        WordPrediction,
        ErrorMessage,
        StartProgram,
        ToggleControls,
        KeyboardLayoutChange,
        LanguagePackages,
        LogMessage
    }

    /// <summary>
    /// Data types for event state data
    /// </summary>
    public enum EDataType
    {
        None,
        Bool,
        Float,
        LRUD
    }

    /// <summary>
    /// Type of highlighting
    /// </summary>
    public enum EHighlightType
    {
        None = 0,
        Default = 1,
        Configured = 2,
        Selected = 3
    }

    /// <summary>
    /// Type of key press event
    /// </summary>
    [Flags]
    public enum EKeyEventType
    {
        None = 0,
        Press = 1,
        Release = 2,
        Type = Press | Release
    }

    /// <summary>
    /// Which toggle keys are on
    /// </summary>
    [Flags]
    public enum EToggleKeyStates
    {
        None = 0,
        CapsLock = 1,
        NumLock = 2,
        Scroll = 4,
        Insert = 8,
        All = 15
    }

    /// <summary>
    /// Which modifier keys are pressed
    /// </summary>
    [Flags]
    public enum EModifierKeyStates
    {
        None = 0,
        LShiftKey = 1,
        RShiftKey = 2,
        ShiftKey = 4,
        AnyShiftKey = LShiftKey | RShiftKey | ShiftKey,
        LControlKey = 8,
        RControlKey = 16,
        ControlKey = 32,
        AnyControlKey = LControlKey | RControlKey | ControlKey,
        LMenu = 64,
        RMenu = 128,
        Menu = 256,
        AnyMenuKey = LMenu | RMenu | Menu,
        LWin = 512,
        RWin = 1024,
        AnyWinKey = LWin | RWin,
        All = 2047
    }

    /// <summary>
    /// Type of window
    /// </summary>
    public enum EStandardWindow
    {
        HoldState,
        Controller,
        Grids
    }

    /// <summary>
    /// Tabs in the profile viewer control
    /// </summary>
    public enum EProfileViewerTab
    {
        Controls,
        Keyboard,
        Settings,
        Inputs,
        Summary
    }

    /// <summary>
    /// Type of grid
    /// </summary>
    public enum EGridType
    {
        None,
        Keyboard,
        ActionStrip,
        Square4x4,
        Square8x4
    }

    /// <summary>
    /// Type of controller control or group of controls
    /// </summary>
    public enum EGeneralisedControl
    {
        None,
        LeftThumb,
        LeftThumbDirection,
        RightThumb,
        RightThumbDirection,
        DPad,
        DPadDirection,
        Buttons,
        ABXYButtons,
        A,
        B,
        X,
        Y,
        Back,
        Start,
        LeftShoulder,
        RightShoulder,
        LeftTrigger,
        RightTrigger
    }
  
    /// <summary>
    /// Direction capability
    /// </summary>
    [Flags]
    public enum EDirectionMode
    {
        None = 0,
        NonDirectional = 1,
        TwoWay = 2,
        FourWay = 4,
        EightWay = 8,
        AxisStyle = 16,
        Continuous = 32
    }
    
    /// <summary>
    /// Restrictions for pairs of controls
    /// </summary>
    public enum EControlRestrictions
    {
        None,
        BothSame,
        BothDifferent
    }

    /// <summary>
    /// Template groups
    /// </summary>
    public enum ETemplateGroup
    {
        None,
        HoldLetterKey,
        HoldNumberKey,
        HoldSymbolKey,
        HoldArrowKey,
        HoldFunctionKey,
        HoldNumpadKey,
        HoldOtherKey,
        TypeLetterKey,
        TypeShiftedLetterKey,
        TypeNumberKey,
        TypeShiftedNumberKey,
        TypeSymbolKey,
        TypeShiftedSymbolKey,
        TypeArrowKey,
        TypeFunctionKey,
        TypeNumpadKey,
        TypeOtherKey,
        AutorepeatLetterKey,
        AutorepeatNumberKey,
        AutorepeatSymbolKey,
        AutorepeatArrowKey,
        AutorepeatFunctionKey,
        AutorepeatNumpadKey,
        AutorepeatOtherKey,
        ToggleKey,
        MediaKey,
        BrowserKey,
        WindowsShortcut,
        Mouse,
        WordPrediction,
        ChangeControlSet,
        WindowAction,
        DirectionMode,
        Timing,
        Combination,
        ControlSets,
        Other
    }    

    /// <summary>
    /// Colour scheme items
    /// </summary>
    public enum EColourSchemeItem
    {
        CellColour,
        AlternateCellColour
    }

    /// <summary>
    /// Language identifiers
    /// </summary>
    public enum ELanguageCode
    {
        araeg,
        baqes,
        belby,
        bulbg,
        cates,
        czecz,
        dandk,
        dutnl,
        enggb,
        engus,
        estee,
        finfi,
        //freca,
        frefr,
        gerde,
        //glagb,
        gleie,
        glges,
        glvim,
        gregr,
        //haung,
        hebil,
        hinin,
        hrvhr,
        hunhu,
        iceis,
        indid,
        itait,
        kanin,
        lavlv,
        litlt,
        malin,
        marin,
        maymy,
        norno,
        perir,
        polpl,
        porbr,
        porpt,
        rumro,
        rusru,
        slosk,
        slvsi,
        spaes,
        spamx,
        srprs,
        swese,
        tamin,
        telin,
        tglph,
        turtr,
        ukrua,
        urdpk,
        vievn
    }   

    /// <summary>
    /// Names of keyboard cells
    /// </summary>
    public enum EKeyboardKey
    {
        None,
        Escape, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, Insert, PrintScreen, Delete,
        Backtick, D1, D2, D3, D4, D5, D6, D7, D8, D9, D0, MinusSign, EqualsSign, Backspace, Home,
        Tab, Q, W, E, R, T, Y, U, I, O, P, LeftBracket, RightBracket, Hash, PageUp,
        CapsLock, A, S, D, F, G, H, J, K, L, Semicolon, Apostrophe, Return, PageDown,
        LShiftKey, Backslash, Z, X, C, V, B, N, M, Comma, Fullstop, Slash, RShiftKey, Up, End,
        LControlKey, LWin, LMenu, Spacebar, RMenu, RWin, Apps, RControlKey, Left, Down, Right
    }

    public enum EMessageType
    {
        None,
        WebServiceError,
        LoginSuccess,
        LoginError,
        GetProgramUpdates,
        ProgramUpdates,
        CheckForumUser,
        GetProfileData,
        ProfileData,
        GetProfilesList,
        ProfilesList,
        SubmitProfile,
        ProfileSubmitted,
        DeleteProfile,
        ProfileDeleted,
        GetWordPredictionLanguagePack,
        WordPredictionLanguagePack
    }

    public enum EMetaDataItem
    {
        None,
        ID,
        Name,
        ShortName,
        FileVersion,
        AppVersion,
        AddedBy,
        AddedDate,
        LastModifiedDate,
        TargetApp,
        TargetAppURL,
        NumPlayers,
        ControlSets,
        KeyboardTypes,
        ProgramActions,
        AutoActivations,
        Title,
        Description,
        Url,
        Username,
        Password,
        Notes,
        Downloads,
        Likes,
        IsLiked,
        IsDeleted,
        IsAdmin,
        LicenceChanged,
        DownloadUpdates,
        Index,
        Count
    }

}
