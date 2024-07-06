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
using System.Text;

namespace Keysticks.Core
{
    /// <summary>
    /// String methods
    /// </summary>
    public class StringUtils
    {
        /// <summary>
        /// Get the name of a state axis (Control set, Page, Cell)
        /// </summary>
        /// <param name="i"></param>
        public string GetAxisName(int i)
        {
            string name;
            switch (i)
            {
                case 0:
                default:
                    name = Properties.Resources.String_ControlSet; break;
                case 1:
                    name = Properties.Resources.String_Page; break;
                case 2:
                    name = Properties.Resources.String_Cell; break;
            }
            return name;
        }

        /// <summary>
        /// Get the names of the cells in a 3x3 grid
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetCellName(int i)
        {
            string name;
            switch (i) {
                case 0:
                    name = Properties.Resources.String_TopLeft; break;
                case 1:
                    name = Properties.Resources.String_TopCentre; break;
                case 2:
                    name = Properties.Resources.String_TopRight; break;
                case 3:
                    name = Properties.Resources.String_CentreLeft; break;
                case 4:
                    name = Properties.Resources.String_Centre; break;
                case 5:
                    name = Properties.Resources.String_CentreRight; break;
                case 6:
                    name = Properties.Resources.String_BottomLeft; break;
                case 7:
                    name = Properties.Resources.String_BottomCentre; break;
                case 8:
                    name = Properties.Resources.String_BottomRight; break;
                default:
                    name = Properties.Resources.String_NotKnownAbbrev; break;
            }
            return name;
        }

        /// <summary>
        /// Virtual control type to string
        /// </summary>
        /// <param name="controlType"></param>
        /// <returns></returns>
        public string ControlTypeToString(EVirtualControlType controlType)
        {
            string name;
            switch (controlType)
            {
                case EVirtualControlType.Button:
                    name = Properties.Resources.String_Button;
                    break;
                case EVirtualControlType.ButtonDiamond:
                    name = Properties.Resources.String_ButtonDiamond;
                    break;
                case EVirtualControlType.DPad:
                    name = Properties.Resources.String_DPad;
                    break;
                case EVirtualControlType.Stick:
                    name = Properties.Resources.String_Stick;
                    break;
                case EVirtualControlType.Trigger:
                    name = Properties.Resources.String_Trigger;
                    break;
                default:
                    name = Properties.Resources.String_NotKnownAbbrev;
                    break;
            }
            return name;
        }

        /// <summary>
        /// Mouse button to string
        /// </summary>
        /// <param name="eButton"></param>
        /// <returns></returns>
        public string MouseButtonToString(EMouseState eButton)
        {
            string name;
            switch (eButton)
            {
                case EMouseState.Left:
                    name = Properties.Resources.String_Left; break;
                case EMouseState.Middle:
                    name = Properties.Resources.String_Middle; break;
                case EMouseState.Right:
                    name = Properties.Resources.String_Right; break;
                case EMouseState.X1:
                    name = Properties.Resources.String_X1; break;
                case EMouseState.X2:
                    name = Properties.Resources.String_X2; break;
                case EMouseState.None:
                default:
                    name = Properties.Resources.String_None; break;
            }
            return name;
        }

        /// <summary>
        /// Button to string
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public string ButtonToString(EButtonState button)
        {
            string name;
            switch (button)
            {
                case EButtonState.A:
                        name = "A"; break;
                case EButtonState.B:
                        name = "B"; break;
                case EButtonState.X:
                        name = "X"; break;
                case EButtonState.Y:
                        name = "Y"; break;
                case EButtonState.LeftShoulder:
                        name = Properties.Resources.String_LeftShoulder; break;
                case EButtonState.RightShoulder:
                        name = Properties.Resources.String_RightShoulder; break;
                case EButtonState.Back:
                        name = Properties.Resources.String_Back; break;
                case EButtonState.Start:
                        name = Properties.Resources.String_Start; break;
                case EButtonState.LeftThumb:
                        name = Properties.Resources.String_LeftThumb; break;
                case EButtonState.RightThumb:
                        name = Properties.Resources.String_RightThumb; break;
                default:
                    name = Properties.Resources.String_NotKnownAbbrev; break;
            }
            return name;
        }

        /// <summary>
        /// Virtual control type to string
        /// </summary>
        /// <param name="controlType"></param>
        /// <returns></returns>
        public string ModifierTypeToString(EModifierKeyStates modifiers)
        {
            string name = "";
            if ((modifiers & EModifierKeyStates.AnyMenuKey) != 0)
            {
                name = Properties.Resources.String_Alt;
            }
            if ((modifiers & EModifierKeyStates.AnyControlKey) != 0)
            {
                if (name != "")
                {
                    name += "+";
                }
                name += Properties.Resources.String_Ctrl;
            }
            if ((modifiers & EModifierKeyStates.AnyShiftKey) != 0)
            {
                if (name != "")
                {
                    name += "+";
                }
                name += Properties.Resources.String_Shift;
            }
            if ((modifiers & EModifierKeyStates.AnyWinKey) != 0)
            {
                if (name != "")
                {
                    name += "+";
                }
                name += Properties.Resources.String_Win;
            }

            return name;
        }

        /// <summary>
        /// Virtual control type to string
        /// </summary>
        /// <param name="controlType"></param>
        /// <returns></returns>
        public string ToggleTypeToString(EToggleKeyStates toggles)
        {
            string name = "";
            if ((toggles & EToggleKeyStates.CapsLock) != 0)
            {
                name = Properties.Resources.String_CapsLock;
            }
            if ((toggles & EToggleKeyStates.Insert) != 0)
            {
                if (name != "")
                {
                    name += "+";
                }
                name += Properties.Resources.String_Insert;
            }
            if ((toggles & EToggleKeyStates.NumLock) != 0)
            {
                if (name != "")
                {
                    name += "+";
                }
                name += Properties.Resources.String_NumLock;
            }
            if ((toggles & EToggleKeyStates.Scroll) != 0)
            {
                if (name != "")
                {
                    name += "+";
                }
                name += Properties.Resources.String_ScrollLock;
            }

            return name;
        }

        /// <summary>
        /// Get the name for each type of action
        /// </summary>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public string ActionTypeToString(EActionType actionType)
        {
            string name;
            switch (actionType)
            {
                case EActionType.ActivateWindow:
                    name = Properties.Resources.String_ActivateWindow; break;
                case EActionType.ChangeControlSet:
                    name = Properties.Resources.String_ChangeControlSet; break;
                case EActionType.ClickMouseButton:
                    name = Properties.Resources.String_ClickMouseButton; break;
                case EActionType.ControlThePointer:
                    name = Properties.Resources.String_ControlThePointer; break;
                case EActionType.DoNothing:
                    name = Properties.Resources.String_DoNothing; break;
                case EActionType.DoubleClickMouseButton:
                    name = Properties.Resources.String_DoubleClickMouseButton; break;
                case EActionType.LoadProfile:
                    name = Properties.Resources.String_LoadProfile; break;
                case EActionType.MaximiseWindow:
                    name = Properties.Resources.String_MaximiseWindow; break;
                case EActionType.MinimiseWindow:
                    name = Properties.Resources.String_MinimiseWindow; break;
                case EActionType.MouseWheelDown:
                    name = Properties.Resources.String_MouseWheelDown; break;
                case EActionType.MouseWheelUp:
                    name = Properties.Resources.String_MouseWheelUp; break;
                case EActionType.MoveThePointer:
                    name = Properties.Resources.String_MoveThePointer; break;
                case EActionType.NavigateCells:
                    name = Properties.Resources.String_NavigateCells; break;
                case EActionType.PressDownKey:
                    name = Properties.Resources.String_PressDownKey; break;
                case EActionType.PressDownMouseButton:
                    name = Properties.Resources.String_PressDownMouseButton; break;
                case EActionType.ReleaseKey:
                    name = Properties.Resources.String_ReleaseKey; break;
                case EActionType.ReleaseMouseButton:
                    name = Properties.Resources.String_ReleaseMouseButton; break;
                case EActionType.SetDirectionMode:
                    name = Properties.Resources.String_SetDirectionMode; break;
                case EActionType.SetDwellAndAutorepeat:
                    name = Properties.Resources.String_SetDwellAndAutorepeat; break;
                case EActionType.StartProgram:
                    name = Properties.Resources.String_StartProgram; break;
                case EActionType.ToggleControlsWindow:
                    name = Properties.Resources.String_ToggleControlsWindow; break;
                case EActionType.ToggleKey:
                    name = Properties.Resources.String_ToggleKey; break;
                case EActionType.ToggleMouseButton:
                    name = Properties.Resources.String_ToggleMouseButton; break;
                case EActionType.TypeKey:
                    name = Properties.Resources.String_TypeKey; break;
                case EActionType.TypeText:
                    name = Properties.Resources.String_TypeText; break;
                case EActionType.Wait:
                    name = Properties.Resources.String_Wait; break;
                case EActionType.WordPrediction:
                    name = Properties.Resources.String_WordPrediction; break;
                default:
                    name = Properties.Resources.String_NotKnownAbbrev; break;
            }

            return name;
        }

        /// <summary>
        /// Get the name of a template group
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public string TemplateGroupToString(ETemplateGroup group)
        {
            string name;
            switch (group)
            {
                case ETemplateGroup.AutorepeatLetterKey:
                    name = Properties.Resources.TemplateGroup_LetterKey; break;
                case ETemplateGroup.AutorepeatNumberKey:
                    name = Properties.Resources.TemplateGroup_NumberKey; break;
                case ETemplateGroup.AutorepeatSymbolKey:
                    name = Properties.Resources.TemplateGroup_SymbolKey; break;
                case ETemplateGroup.AutorepeatArrowKey:
                    name = Properties.Resources.TemplateGroup_ArrowKey; break;
                case ETemplateGroup.AutorepeatFunctionKey:
                    name = Properties.Resources.TemplateGroup_FunctionKey; break;
                case ETemplateGroup.AutorepeatNumpadKey:
                    name = Properties.Resources.TemplateGroup_NumpadKey; break;
                case ETemplateGroup.AutorepeatOtherKey:
                    name = Properties.Resources.TemplateGroup_OtherKey; break;
                case ETemplateGroup.BrowserKey:
                    name = Properties.Resources.TemplateGroup_BrowserKey; break;
                case ETemplateGroup.ChangeControlSet:
                    name = Properties.Resources.TemplateGroup_ChangeControlSet; break;
                case ETemplateGroup.Combination:
                    name = Properties.Resources.TemplateGroup_Combination; break;
                case ETemplateGroup.ControlSets:
                    name = Properties.Resources.TemplateGroup_ControlSets; break;
                case ETemplateGroup.DirectionMode:
                    name = Properties.Resources.TemplateGroup_DirectionMode; break;
                case ETemplateGroup.HoldLetterKey:
                    name = Properties.Resources.TemplateGroup_LetterKey; break;
                case ETemplateGroup.HoldNumberKey:
                    name = Properties.Resources.TemplateGroup_NumberKey; break;
                case ETemplateGroup.HoldSymbolKey:
                    name = Properties.Resources.TemplateGroup_SymbolKey; break;
                case ETemplateGroup.HoldArrowKey:
                    name = Properties.Resources.TemplateGroup_ArrowKey; break;
                case ETemplateGroup.HoldFunctionKey:
                    name = Properties.Resources.TemplateGroup_FunctionKey; break;
                case ETemplateGroup.HoldNumpadKey:
                    name = Properties.Resources.TemplateGroup_NumpadKey; break;
                case ETemplateGroup.HoldOtherKey:
                    name = Properties.Resources.TemplateGroup_OtherKey; break;
                case ETemplateGroup.MediaKey:
                    name = Properties.Resources.TemplateGroup_MediaKey; break;
                case ETemplateGroup.Mouse:
                    name = Properties.Resources.TemplateGroup_Mouse; break;
                case ETemplateGroup.None:
                    name = Properties.Resources.String_None; break;
                case ETemplateGroup.Other:
                    name = Properties.Resources.String_Other; break;
                case ETemplateGroup.Timing:
                    name = Properties.Resources.TemplateGroup_Timing; break;
                case ETemplateGroup.ToggleKey:
                    name = Properties.Resources.TemplateGroup_ToggleKey; break;
                case ETemplateGroup.TypeLetterKey:
                    name = Properties.Resources.TemplateGroup_LetterKey; break;
                case ETemplateGroup.TypeShiftedLetterKey:
                    name = Properties.Resources.TemplateGroup_ShiftedLetterKey; break;
                case ETemplateGroup.TypeNumberKey:
                    name = Properties.Resources.TemplateGroup_NumberKey; break;
                case ETemplateGroup.TypeShiftedNumberKey:
                    name = Properties.Resources.TemplateGroup_ShiftedNumberKey; break;
                case ETemplateGroup.TypeSymbolKey:
                    name = Properties.Resources.TemplateGroup_SymbolKey; break;
                case ETemplateGroup.TypeShiftedSymbolKey:
                    name = Properties.Resources.TemplateGroup_ShiftedSymbolKey; break;
                case ETemplateGroup.TypeArrowKey:
                    name = Properties.Resources.TemplateGroup_ArrowKey; break;
                case ETemplateGroup.TypeFunctionKey:
                    name = Properties.Resources.TemplateGroup_FunctionKey; break;
                case ETemplateGroup.TypeNumpadKey:
                    name = Properties.Resources.TemplateGroup_NumpadKey; break;
                case ETemplateGroup.TypeOtherKey:
                    name = Properties.Resources.TemplateGroup_OtherKey; break;
                case ETemplateGroup.WindowAction:
                    name = Properties.Resources.TemplateGroup_WindowAction; break;
                case ETemplateGroup.WindowsShortcut:
                    name = Properties.Resources.TemplateGroup_WindowsShortcut; break;
                case ETemplateGroup.WordPrediction:
                    name = Properties.Resources.TemplateGroup_WordPrediction; break;
                default:
                    name = Properties.Resources.String_NotKnownAbbrev; break;
            }

            return name;
        }

        /// <summary>
        /// Control restrictions to string
        /// </summary>
        /// <param name="restrictions"></param>
        /// <returns></returns>
        public string RestrictionsToString(EControlRestrictions restrictions) {
            string name;
            switch (restrictions)
            {
                case EControlRestrictions.BothDifferent:
                    name = Properties.Resources.String_BothDifferent; break;
                case EControlRestrictions.BothSame:
                    name = Properties.Resources.String_BothTheSame; break;
                case EControlRestrictions.None:
                default:
                    name = Properties.Resources.String_None; break;
            }
            return name;
        }

        /// <summary>
        /// Device type to string
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public string DeviceTypeToString(EDeviceType deviceType)
        {
            string name;
            switch (deviceType)
            {
                case EDeviceType.ArcadeStick:
                    name = Properties.Resources.String_ArcadeStick; break;
                case EDeviceType.Controller:
                    name = Properties.Resources.String_Controller; break;
                case EDeviceType.DancePad:
                    name = Properties.Resources.String_DancePad; break;
                case EDeviceType.DrumKit:
                    name = Properties.Resources.String_DrumKit; break;
                case EDeviceType.FlightStick:
                    name = Properties.Resources.String_FlightStick; break;
                case EDeviceType.Gamepad:
                    name = Properties.Resources.String_Gamepad; break;
                case EDeviceType.Guitar:
                    name = Properties.Resources.String_Guitar; break;
                case EDeviceType.Joystick:
                    name = Properties.Resources.String_Joystick; break;
                case EDeviceType.Keyboard:
                    name = Properties.Resources.String_Keyboard; break;
                case EDeviceType.Mouse:
                    name = Properties.Resources.String_Mouse; break;
                case EDeviceType.Wheel:
                    name = Properties.Resources.String_Wheel; break;
                default:
                    name = Properties.Resources.String_NotKnownAbbrev; break;
            }
            return name;
        }

        /// <summary>
        /// Event type to string
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public string EventTypeToString(EEventType eventType)
        {
            string name;
            switch (eventType) {
                case EEventType.Control:
                    name = Properties.Resources.String_Control; break;
                case EEventType.Source:
                    name = Properties.Resources.String_Source; break;
                case EEventType.StateChange:
                    name = Properties.Resources.String_StateChange; break;
                case EEventType.AppChange:
                    name = Properties.Resources.String_AppChange; break;
                case EEventType.LoadProfile:
                    name = Properties.Resources.String_LoadProfile; break;
                case EEventType.Key:
                    name = Properties.Resources.String_Key; break;
                case EEventType.RepeatKey:
                    name = Properties.Resources.String_RepeatKey; break;
                case EEventType.Text:
                    name = Properties.Resources.String_Text; break;
                case EEventType.MouseButtonState:
                    name = Properties.Resources.String_MouseButtonState; break;
                case EEventType.KeyboardState:
                    name = Properties.Resources.String_KeyboardState; break;
                case EEventType.WordPrediction:
                    name = Properties.Resources.String_WordPrediction; break;
                case EEventType.ErrorMessage:
                    name = Properties.Resources.String_ErrorMessage; break;
                case EEventType.StartProgram:
                    name = Properties.Resources.String_StartProgram; break;
                case EEventType.ToggleControls:
                    name = Properties.Resources.String_ToggleControls; break;
                case EEventType.KeyboardLayoutChange:
                    name = Properties.Resources.String_KeyboardLayoutChange; break;
                case EEventType.LanguagePackages:
                    name = Properties.Resources.String_LanguagePackages; break;
                case EEventType.LogMessage:
                    name = Properties.Resources.String_LogMessage; break;
                case EEventType.Unknown:
                default:
                    name = Properties.Resources.String_NotKnownAbbrev; break;
                }
            return name;
        }

        /// <summary>
        /// Language code to friendly name (in English)
        /// </summary>
        /// <param name="langCode"></param>
        /// <returns></returns>
        public string LanguageCodeToString(ELanguageCode langCode)
        {
            string name;
            switch (langCode)
            {
                case ELanguageCode.araeg: name = "Arabic"; break;
                case ELanguageCode.baqes: name = "Basque"; break;
                case ELanguageCode.belby: name = "Belarusian"; break;
                case ELanguageCode.bulbg: name = "Bulgarian"; break;
                case ELanguageCode.cates: name = "Catalan"; break;
                case ELanguageCode.czecz: name = "Czech"; break;
                case ELanguageCode.dandk: name = "Danish"; break;
                case ELanguageCode.dutnl: name = "Dutch"; break;
                case ELanguageCode.enggb: name = "English (UK)"; break;
                case ELanguageCode.engus: name = "English (US)"; break;
                case ELanguageCode.estee: name = "Estonian"; break;
                case ELanguageCode.finfi: name = "Finnish"; break;
                //case ELanguageCode.freca: name = "French (Canadian)"; break;
                case ELanguageCode.frefr: name = "French"; break;                
                case ELanguageCode.gerde: name = "German"; break;
                case ELanguageCode.gleie: name = "Irish"; break;
                case ELanguageCode.glges: name = "Galician"; break;
                case ELanguageCode.glvim: name = "Manx"; break;
                case ELanguageCode.gregr: name = "Greek"; break;                
                case ELanguageCode.hebil: name = "Hebrew"; break;
                case ELanguageCode.hinin: name = "Hindi"; break;
                case ELanguageCode.hrvhr: name = "Croatian"; break;
                case ELanguageCode.hunhu: name = "Hungarian"; break;
                case ELanguageCode.iceis: name = "Icelandic"; break;
                case ELanguageCode.indid: name = "Indonesian"; break;
                case ELanguageCode.itait: name = "Italian"; break;
                case ELanguageCode.kanin: name = "Kannada"; break;
                case ELanguageCode.lavlv: name = "Latvian"; break;
                case ELanguageCode.litlt: name = "Lithuanian"; break;
                case ELanguageCode.malin: name = "Malayalam"; break;
                case ELanguageCode.marin: name = "Marathi"; break;
                case ELanguageCode.maymy: name = "Malay"; break;
                case ELanguageCode.norno: name = "Norwegian"; break;
                case ELanguageCode.perir: name = "Persian"; break;
                case ELanguageCode.polpl: name = "Polish"; break;
                case ELanguageCode.porbr: name = "Portuguese (Brazilian)"; break;
                case ELanguageCode.porpt: name = "Portuguese (European)"; break;
                case ELanguageCode.rumro: name = "Romanian"; break;
                case ELanguageCode.rusru: name = "Russian"; break;
                case ELanguageCode.slosk: name = "Slovak"; break;
                case ELanguageCode.slvsi: name = "Slovenian"; break;
                case ELanguageCode.spaes: name = "Spanish (European)"; break;
                case ELanguageCode.spamx: name = "Spanish (Latin American)"; break;
                case ELanguageCode.srprs: name = "Serbian"; break;
                case ELanguageCode.swese: name = "Swedish"; break;
                case ELanguageCode.tamin: name = "Tamil"; break;
                case ELanguageCode.telin: name = "Telugu"; break;
                case ELanguageCode.tglph: name = "Filipino"; break;
                case ELanguageCode.turtr: name = "Turkish"; break;
                case ELanguageCode.ukrua: name = "Ukrainian"; break;
                case ELanguageCode.urdpk: name = "Urdu"; break;
                case ELanguageCode.vievn: name = "Vietnamese"; break;
                default:
                    name = Properties.Resources.String_NotKnownAbbrev; break;
            }

            return name;
        }        

        /// <summary>
        /// Direction mode to string
        /// </summary>
        /// <param name="directionMode"></param>
        /// <returns></returns>
        public string DirectionModeToString(EDirectionMode directionMode)
        {
            string str;
            switch (directionMode)
            {
                case EDirectionMode.Continuous:
                    str = Properties.Resources.String_Continuous; break;
                case EDirectionMode.EightWay:
                    str = Properties.Resources.String_EightWay; break;
                case EDirectionMode.FourWay:
                    str = Properties.Resources.String_FourWay; break;
                case EDirectionMode.TwoWay:
                    str = Properties.Resources.String_TwoWay; break;
                case EDirectionMode.AxisStyle:
                    str = Properties.Resources.String_AxisStyle; break;
                case EDirectionMode.NonDirectional:
                    str = Properties.Resources.String_Nondirectional; break;
                case EDirectionMode.None:
                default:
                    str = Properties.Resources.String_None; break;
            }

            return str;
        }

        /// <summary>
        /// Event reason to string
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public string EventReasonToString(EEventReason reason)
        {
            string str;
            switch (reason)
            {
                case EEventReason.Activated:
                    str = Properties.Resources.String_InThisControlSet;
                    break;
                case EEventReason.Moved:
                    str = Properties.Resources.String_Moved; break;
                case EEventReason.Pressed:
                    str = Properties.Resources.String_Pressed; break;
                case EEventReason.Released:
                    str = Properties.Resources.String_Released; break;
                case EEventReason.Directed:
                    str = Properties.Resources.String_Directed;
                    break;
                case EEventReason.Undirected:
                    str = Properties.Resources.String_Undirected;
                    break;
                case EEventReason.DirectedLong:
                    str = string.Format("{0} ({1})",
                        Properties.Resources.String_Directed,
                        Properties.Resources.String_Long.ToLower());
                    break;
                case EEventReason.DirectedShort:
                    str = string.Format("{0} ({1})",
                        Properties.Resources.String_Directed,
                        Properties.Resources.String_Short.ToLower());
                    break;
                case EEventReason.DirectionRepeated:
                    str = string.Format("{0} ({1})",
                        Properties.Resources.String_Directed,
                        Properties.Resources.String_AutoRepeat.ToLower());
                    break;
                case EEventReason.PressedLong:
                    str = string.Format("{0} ({1})",
                                            Properties.Resources.String_Pressed,
                                            Properties.Resources.String_Long.ToLower());
                    break;
                case EEventReason.PressedShort:
                    str = string.Format("{0} ({1})",
                                            Properties.Resources.String_Pressed,
                                            Properties.Resources.String_Short.ToLower());
                    break;
                case EEventReason.PressRepeated:
                    str = string.Format("{0} ({1})",
                                            Properties.Resources.String_Pressed,
                                            Properties.Resources.String_AutoRepeat.ToLower());
                    break;
                default:
                    str = "";
                    break;
            }

            return str;
        }

        /// <summary>
        /// Event type to string
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public string GetEventDescription(EEventReason reason, ELRUDState direction)
        {
            string str;
            switch (reason)
            {
                case EEventReason.Activated:
                    str = Properties.Resources.String_InThisControlSet;
                    break;
                //case EEventReason.Deactivated:
                //    str = "Leaving this control set";
                //    break;
                case EEventReason.Moved:
                    str = Properties.Resources.String_Moved; break;
                case EEventReason.Pressed:
                    str = Properties.Resources.String_Pressed; break;
                case EEventReason.Released:
                    str = Properties.Resources.String_Released; break;
                case EEventReason.Directed:
                    if (direction == ELRUDState.Centre)
                    {
                        str = Properties.Resources.String_Centred;
                    }
                    else
                    {
                        str = string.Format("{0} {1}",
                            Properties.Resources.String_Directed,
                            DirectionToString(direction).ToLower());
                    }
                    break;
                case EEventReason.Undirected:
                    if (direction == ELRUDState.Centre)
                    {
                        str = Properties.Resources.String_Uncentred;
                    }
                    else
                    {
                        str = string.Format("{0} {1}",
                        Properties.Resources.String_Undirected,
                        DirectionToString(direction).ToLower());
                    }
                    break;
                case EEventReason.DirectedLong:
                    str = string.Format("{0} {1} ({2})", 
                        Properties.Resources.String_Directed, 
                        DirectionToString(direction).ToLower(),
                        Properties.Resources.String_Long.ToLower());
                    break;
                case EEventReason.DirectedShort:
                    str = string.Format("{0} {1} ({2})",
                        Properties.Resources.String_Directed,
                        DirectionToString(direction).ToLower(),
                        Properties.Resources.String_Short.ToLower());
                    break;
                case EEventReason.DirectionRepeated:
                    str = string.Format("{0} {1} ({2})",
                        Properties.Resources.String_Directed,
                        DirectionToString(direction).ToLower(),
                        Properties.Resources.String_AutoRepeat.ToLower());
                    break;
                case EEventReason.PressedLong:
                    str = string.Format("{0} ({1})",
                                            Properties.Resources.String_Pressed,
                                            Properties.Resources.String_Long.ToLower()); 
                    break;
                case EEventReason.PressedShort:
                    str = string.Format("{0} ({1})",
                                            Properties.Resources.String_Pressed,
                                            Properties.Resources.String_Short.ToLower());
                    break;
                case EEventReason.PressRepeated:
                    str = string.Format("{0} ({1})",
                                            Properties.Resources.String_Pressed,
                                            Properties.Resources.String_AutoRepeat.ToLower());
                    break;
                default:
                    str = "";
                    break;
            }

            return str;
        }

        /// <summary>
        /// Direction to string
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public string DirectionToString(ELRUDState direction)
        {
            string str;

            switch (direction)
            {
                case ELRUDState.Centre:
                    str = Properties.Resources.String_Centre; break;
                case ELRUDState.Up:
                    str = Properties.Resources.String_Up; break;
                case ELRUDState.Down:
                    str = Properties.Resources.String_Down; break;
                case ELRUDState.Left:
                    str = Properties.Resources.String_Left; break;
                case ELRUDState.Right:
                    str = Properties.Resources.String_Right; break;
                case ELRUDState.UpLeft:
                    str = Properties.Resources.String_UpLeft; break;
                case ELRUDState.UpRight:
                    str = Properties.Resources.String_UpRight; break;
                case ELRUDState.DownLeft:
                    str = Properties.Resources.String_DownLeft; break;
                case ELRUDState.DownRight:
                    str = Properties.Resources.String_DownRight; break;
                default:
                    str = ""; break;
            }

            return str;
        }

        /// <summary>
        /// Convert a grid type to a string
        /// </summary>
        /// <param name="gridType"></param>
        /// <returns></returns>
        public string GridTypeToString(EGridType gridType)
        {
            string str;
            switch (gridType)
            {
                case EGridType.None:
                    str = Properties.Resources.String_None; break;
                case EGridType.Keyboard:
                    str = Properties.Resources.GridType_StandardKeyboard; break;
                case EGridType.Square8x4:
                    str = Properties.Resources.GridType_Full; break;
                case EGridType.Square4x4:
                    str = Properties.Resources.GridType_Half; break;
                case EGridType.ActionStrip:
                    str = Properties.Resources.GridType_ActionStrip; break;
                default:
                    str = Properties.Resources.String_NotKnownAbbrev; break;
            }

            return str;
        }        

        /// <summary>
        /// Read a list of language items from a string
        /// </summary>
        /// <param name="langListStr"></param>
        /// <returns></returns>
        public NamedItemList LanguageListFromString(string langListStr)
        {
            NamedItemList langList = new NamedItemList();
            string[] tokens = langListStr.Split(',');
            int i = 0;
            while (i < tokens.Length - 1)
            {
                string langCodeStr = tokens[i++];
                string enabledStr = tokens[i++];
                ELanguageCode langCode;
                bool enabled;
                if (Enum.TryParse<ELanguageCode>(langCodeStr, out langCode) &&
                    bool.TryParse(enabledStr, out enabled))
                {
                    OptionalNamedItem langItem = new OptionalNamedItem((int)langCode, LanguageCodeToString(langCode), enabled);
                    langList.Add(langItem);
                }
            }

            return langList;
        }

        /// <summary>
        /// Write a list of language items to a string
        /// </summary>
        /// <param name="langList"></param>
        /// <returns></returns>
        public string LanguageListToString(NamedItemList langList)
        {
            StringBuilder sb = new StringBuilder();
            bool isFirst = true;
            foreach (OptionalNamedItem langItem in langList)
            {
                if (!isFirst)
                {
                    sb.Append(',');
                }
                isFirst = false;

                ELanguageCode langCode = (ELanguageCode)langItem.ID;
                sb.Append(langCode.ToString());
                sb.Append(',');
                sb.Append(langItem.IsEnabled);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get short button names
        /// </summary>
        /// <param name="buttonID"></param>
        /// <returns></returns>
        public string EButtonStateToShortName(EButtonState buttonID)
        {
            string shortName;
            switch (buttonID)
            {
                case EButtonState.A:
                    shortName = "A"; break;
                case EButtonState.B:
                    shortName = "B"; break;
                case EButtonState.X:
                    shortName = "X"; break;
                case EButtonState.Y:
                    shortName = "Y"; break;
                case EButtonState.LeftShoulder:
                    shortName = "LS"; break;
                case EButtonState.RightShoulder:
                    shortName = "RS"; break;
                case EButtonState.Back:
                    shortName = Properties.Resources.String_Back; break;
                case EButtonState.Start:
                    shortName = Properties.Resources.String_Start; break;
                case EButtonState.LeftThumb:
                    shortName = "LT"; break;
                case EButtonState.RightThumb:
                    shortName = "RT"; break;
                default:
                    shortName = "NK"; break;
            }

            return shortName;
        }

        /// <summary>
        /// Get a display name for a virtual input source
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetVirtualControllerName(int id)
        {
            string name;
            switch (id)
            {
                case Constants.DefaultID:
                    name = Properties.Resources.String_Internal; break;
                case Constants.ID1:
                    name = Properties.Resources.String_Player1; break;
                case Constants.ID2:
                    name = Properties.Resources.String_Player2; break;
                case Constants.ID3:
                    name = Properties.Resources.String_Player3; break;
                case Constants.ID4:
                    name = Properties.Resources.String_Player4; break;
                default:
                    name = Properties.Resources.String_NotKnownAbbrev; break;
            }

            return name;
        }

        /// <summary>
        /// Prediction event type to string
        /// </summary>
        /// <param name="predictionEvent"></param>
        /// <returns></returns>
        public string PredictionEventToString(EWordPredictionEventType predictionEvent)
        {
            string name;
            switch (predictionEvent)
            {
                case EWordPredictionEventType.NextSuggestion:
                    name = Properties.Resources.String_NextSuggestion; break;
                case EWordPredictionEventType.PreviousSuggestion:
                    name = Properties.Resources.String_PreviousSuggestion; break;
                case EWordPredictionEventType.InsertSuggestion:
                    name = Properties.Resources.String_InsertSuggestion; break;
                case EWordPredictionEventType.CancelSuggestions:
                    name = Properties.Resources.String_CancelSuggestions; break;
                case EWordPredictionEventType.Enable:
                    name = Properties.Resources.String_Enable; break;
                case EWordPredictionEventType.Disable:
                    name = Properties.Resources.String_Disable; break;
                case EWordPredictionEventType.SuggestionsList:
                    name = Properties.Resources.String_SuggestionsList; break;
                case EWordPredictionEventType.None:
                default:
                    name = Properties.Resources.String_None; break;
            }
            return name;
        }
    }
}
