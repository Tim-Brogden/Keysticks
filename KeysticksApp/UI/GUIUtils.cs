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
using System.Windows.Media.Imaging;
using Keysticks.Core;
using System.IO;

namespace Keysticks.UI
{
    /// <summary>
    /// GUI utility methods
    /// </summary>
    public static class GUIUtils
    {
        // Fields
        private static Dictionary<EAnnotationImage, BitmapImage> _iconLookup = new Dictionary<EAnnotationImage, BitmapImage>();        
        
        /// <summary>
        /// Set the list of items for a combo box
        /// </summary>
        public static void PopulateDisplayableListWithNamedItems(NamedItemList displayableItems,
                                                                    NamedItemList namedItems,
                                                                    bool addNoneItem,
                                                                    string noneItemText,
                                                                    bool addDefaultItem,
                                                                    string defaultItemText,
                                                                    bool addShortCutItems)
        {
            displayableItems.Clear();

            int numSpecialItems = 0;

            if (addNoneItem)
            {
                displayableItems.Add(new NamedItem(Constants.NoneID, noneItemText));
                numSpecialItems++;
            }

            if (addDefaultItem)
            {
                displayableItems.Add(new NamedItem(Constants.DefaultID, defaultItemText));
                numSpecialItems++;
            }

            if (namedItems != null)
            {
                foreach (NamedItem item in namedItems)
                {
                    // Ignore None or Default items here
                    if (item.ID > 0)
                    {
                        displayableItems.Add(new NamedItem(item.ID, item.Name));
                    }
                }
            }

            if (addShortCutItems)
            {
                displayableItems.Insert(numSpecialItems++, new NamedItem(Constants.NextID, "(" + Properties.Resources.String_Next + ")"));
                displayableItems.Insert(numSpecialItems++, new NamedItem(Constants.PreviousID, "(" + Properties.Resources.String_Previous + ")"));
            }
        }

        /// <summary>
        /// Set the list of keys for a combo box
        /// </summary>
        /// <param name="displayableItems"></param>
        public static void PopulateDisplayableListWithKeys(NamedItemList displayableItems, IKeyboardContext context)
        {
            // Get the keyboard layout data
            VirtualKeyData[] virtualKeyData = KeyUtils.GetVirtualKeysByKeyCode(context.KeyboardHKL);

            // Sort the names into alphabetical order for readability
            List<int> idList = new List<int>();
            List<string> keyNamesList = new List<string>();
            for (uint i = 0; i < virtualKeyData.Length; i++)
            {
                VirtualKeyData vk = virtualKeyData[i];
                if (vk != null)
                {
                    // Use either scan code or virtual key code, depending upon whether we want to adapt to the user's keyboard layout
                    int id = virtualKeyData[i].WindowsScanCode | ((int)virtualKeyData[i].KeyCode << 16);
                    idList.Add(id);
                    keyNamesList.Add(virtualKeyData[i].Name);
                }
            }
            int[] keyIDs = idList.ToArray();
            string[] keyNames = keyNamesList.ToArray();
            Array.Sort<string, int>(keyNames, keyIDs);

            // Add key-name pairs to the displayable list
            // Add the "length 1" items first, sorted alpabetically for readability
            displayableItems.Clear();
            for (int i = 0; i < keyNames.Length; i++)
            {
                if (keyNames[i].Length == 1)
                {
                    displayableItems.Add(new NamedItem(keyIDs[i], keyNames[i]));
                }
            }
            // Now add the remaining named keys, sorted alpabetically
            for (int i = 0; i < keyNames.Length; i++)
            {
                if (keyNames[i].Length > 1)
                {
                    displayableItems.Add(new NamedItem(keyIDs[i], keyNames[i]));
                }
            }
        }

        /// <summary>
        /// Crop and append ellipsis to long strings
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLen"></param>
        /// <returns></returns>
        public static string CapDisplayStringLength(string str, int maxLen)
        {
            string result;
            if (str.Length > maxLen)
            {
                result = str.Substring(0, maxLen - 3) + "...";
            }
            else
            {
                result = str;
            }

            return result;
        }

        /// <summary>
        /// Get the icon to use for each profile status
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static EAnnotationImage GetIconForProfileStatus(EProfileStatus status)
        {
            EAnnotationImage icon = EAnnotationImage.None;
            switch (status)
            {
                case EProfileStatus.Local:
                    icon = EAnnotationImage.KLogo; break;
                case EProfileStatus.OnlineDownloaded:
                    icon = EAnnotationImage.BulletGreen; break;
                case EProfileStatus.OnlineNotDownloaded:
                    icon = EAnnotationImage.BulletYellow; break;
                case EProfileStatus.OnlinePreviouslyDownloaded:
                    icon = EAnnotationImage.BulletBlue; break;
            }

            return icon;
        }

        /// <summary>
        /// Get the tooltip to use for each profile status
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static string GetCaptionForProfileStatus(EProfileStatus status)
        {
            string caption = null;
            switch (status)
            {
                case EProfileStatus.Local:
                    caption = Properties.Resources.String_On_your_computer; break;
                case EProfileStatus.OnlineDownloaded:
                    caption = Properties.Resources.String_Downloaded; break;
                case EProfileStatus.OnlineNotDownloaded:
                    caption = Properties.Resources.String_Not_downloaded; break;
                case EProfileStatus.OnlinePreviouslyDownloaded:
                    caption = Properties.Resources.String_Previously_downloaded; break;
            }

            return caption;
        }

        /// <summary>
        /// Get an icon for some of the template groups
        /// </summary>
        /// <param name="templateGroup"></param>
        /// <returns></returns>
        public static EAnnotationImage GetIconForTemplateGroup(ETemplateGroup templateGroup)
        {
            EAnnotationImage icon = EAnnotationImage.None;
            switch (templateGroup)
            {
                case ETemplateGroup.HoldLetterKey:
                    icon = EAnnotationImage.HoldLetterKey; break;
                case ETemplateGroup.HoldNumberKey:
                    icon = EAnnotationImage.HoldNumberKey; break;
                case ETemplateGroup.HoldSymbolKey:
                    icon = EAnnotationImage.HoldSymbolKey; break;
                case ETemplateGroup.HoldArrowKey:
                    icon = EAnnotationImage.HoldArrowKey; break;
                case ETemplateGroup.HoldFunctionKey:
                    icon = EAnnotationImage.HoldFunctionKey; break;
                case ETemplateGroup.HoldNumpadKey:
                    icon = EAnnotationImage.HoldNumpadKey; break;
                case ETemplateGroup.HoldOtherKey:
                    icon = EAnnotationImage.HoldOtherKey; break;

                case ETemplateGroup.TypeLetterKey:
                    icon = EAnnotationImage.TypeLetterKey; break;
                case ETemplateGroup.TypeShiftedLetterKey:
                    icon = EAnnotationImage.TypeShiftedLetterKey; break;
                case ETemplateGroup.TypeNumberKey:
                    icon = EAnnotationImage.TypeNumberKey; break;
                case ETemplateGroup.TypeShiftedNumberKey:
                    icon = EAnnotationImage.TypeShiftedNumberKey; break;
                case ETemplateGroup.TypeSymbolKey:
                    icon = EAnnotationImage.TypeSymbolKey; break;
                case ETemplateGroup.TypeShiftedSymbolKey:
                    icon = EAnnotationImage.TypeShiftedSymbolKey; break;
                case ETemplateGroup.TypeArrowKey:
                    icon = EAnnotationImage.TypeArrowKey; break;
                case ETemplateGroup.TypeFunctionKey:
                    icon = EAnnotationImage.TypeFunctionKey; break;
                case ETemplateGroup.TypeNumpadKey:
                    icon = EAnnotationImage.TypeNumpadKey; break;
                case ETemplateGroup.TypeOtherKey:
                    icon = EAnnotationImage.TypeOtherKey; break;
                
                case ETemplateGroup.AutorepeatLetterKey:
                    icon = EAnnotationImage.AutorepeatLetterKey; break;
                case ETemplateGroup.AutorepeatNumberKey:
                    icon = EAnnotationImage.AutorepeatNumberKey; break;
                case ETemplateGroup.AutorepeatSymbolKey:
                    icon = EAnnotationImage.AutorepeatSymbolKey; break;
                case ETemplateGroup.AutorepeatArrowKey:
                    icon = EAnnotationImage.AutorepeatArrowKey; break;
                case ETemplateGroup.AutorepeatFunctionKey:
                    icon = EAnnotationImage.AutorepeatFunctionKey; break;
                case ETemplateGroup.AutorepeatNumpadKey:
                    icon = EAnnotationImage.AutorepeatNumpadKey; break;
                case ETemplateGroup.AutorepeatOtherKey:
                    icon = EAnnotationImage.AutorepeatOtherKey; break;

                case ETemplateGroup.ToggleKey:
                    icon = EAnnotationImage.ToggleKey; break;

                case ETemplateGroup.MediaKey:
                    icon = EAnnotationImage.MediaPlayer; break;
                case ETemplateGroup.BrowserKey:
                    icon = EAnnotationImage.Browser; break;
                case ETemplateGroup.WindowsShortcut:
                    icon = EAnnotationImage.WindowsKey; break;
                case ETemplateGroup.Mouse:
                    icon = EAnnotationImage.Mouse; break;
                case ETemplateGroup.WordPrediction:
                    icon = EAnnotationImage.InsertSuggestion; break;
                case ETemplateGroup.DirectionMode:
                    icon = EAnnotationImage.DirectionMode; break;
                case ETemplateGroup.Timing:
                    icon = EAnnotationImage.Clock; break;
                case ETemplateGroup.Combination:
                    icon = EAnnotationImage.Combination; break;
                case ETemplateGroup.ChangeControlSet:
                    icon = EAnnotationImage.ChangeControlSet; break;
                case ETemplateGroup.WindowAction:
                    icon = EAnnotationImage.ActivateWindow; break;
            }

            return icon;
        }

        /// <summary>
        /// Find an icon
        /// </summary>
        /// <param name="iconRef"></param>
        /// <returns></returns>
        public static BitmapImage FindIcon(EAnnotationImage iconRef)
        {
            BitmapImage icon = null;

            if (_iconLookup.ContainsKey(iconRef))
            {
                icon = _iconLookup[iconRef];
            }
            else
            {
                string key = null;
                switch (iconRef)
                {
                    case EAnnotationImage.DontShow:
                        key = "image_dont_show"; break;
                    case EAnnotationImage.Accept:
                        key = "image_accept"; break;
                    case EAnnotationImage.KLogo:
                        key = "image_klogo"; break;
                    case EAnnotationImage.CentrePosition:
                        key = "image_yellow_arrow_centre"; break; 
                    case EAnnotationImage.LeftDirection:
                        key = "image_yellow_arrow_left"; break;
                    case EAnnotationImage.RightDirection:
                        key = "image_yellow_arrow_right"; break;
                    case EAnnotationImage.UpDirection:
                        key = "image_yellow_arrow_up"; break;
                    case EAnnotationImage.DownDirection:
                        key = "image_yellow_arrow_down"; break;
                    case EAnnotationImage.UpLeftDirection:
                        key = "image_yellow_arrow_up_left"; break;
                    case EAnnotationImage.UpRightDirection:
                        key = "image_yellow_arrow_up_right"; break;
                    case EAnnotationImage.DownLeftDirection:
                        key = "image_yellow_arrow_down_left"; break;
                    case EAnnotationImage.DownRightDirection:
                        key = "image_yellow_arrow_down_right"; break;
                    case EAnnotationImage.LeftMouseButton:
                        key = "image_mouse_left_click"; break;
                    case EAnnotationImage.MiddleMouseButton:
                        key = "image_mouse_middle_click"; break;
                    case EAnnotationImage.RightMouseButton:
                        key = "image_mouse_right_click"; break;
                    case EAnnotationImage.X1MouseButton:
                        key = "image_mouse_X1_click"; break;
                    case EAnnotationImage.X2MouseButton:
                        key = "image_mouse_X2_click"; break;
                    case EAnnotationImage.MouseWheelUp:
                        key = "image_mouse_wheel_up"; break;
                    case EAnnotationImage.MouseWheelDown:
                        key = "image_mouse_wheel_down"; break;
                    case EAnnotationImage.Mouse:
                        key = "image_mouse"; break;
                    case EAnnotationImage.MousePointer:
                        key = "image_cursor"; break;
                    case EAnnotationImage.MousePointerAbsolute:
                        key = "image_cursor_absolute"; break;
                    case EAnnotationImage.MousePointerFixedSpeed:
                        key = "image_cursor_fixed_speed"; break;
                    case EAnnotationImage.MousePointerRelative:
                        key = "image_cursor_relative"; break;
                    case EAnnotationImage.NextControlSet:
                        key = "image_mode_next"; break;
                    case EAnnotationImage.PreviousControlSet:
                        key = "image_mode_previous"; break;
                    case EAnnotationImage.ChangeControlSet:
                        key = "image_mode_star"; break;
                    case EAnnotationImage.NextPage:
                        key = "image_page_white_go"; break;
                    case EAnnotationImage.PreviousPage:
                        key = "image_page_white_previous"; break;
                    case EAnnotationImage.ChangePage:
                        key = "image_page_white_star"; break;
                    case EAnnotationImage.ChangeCell:
                        key = "image_yellow_arrow_other"; break;
                    case EAnnotationImage.NewProfile:
                        key = "image_page"; break;
                    case EAnnotationImage.LoadProfile:
                        key = "image_folder_page"; break;
                    case EAnnotationImage.StartProgram:
                        key = "image_start_program"; break;
                    case EAnnotationImage.MaximiseWindow:
                        key = "image_maximise_window"; break;
                    case EAnnotationImage.MinimiseWindow:
                        key = "image_minimise_window"; break;
                    case EAnnotationImage.Controller:
                        key = "image_controller"; break;
                    case EAnnotationImage.ActivateWindow:
                        key = "image_application_double"; break;                    
                    case EAnnotationImage.Wait: 
                        key = "image_hourglass"; break;
                    case EAnnotationImage.NextSuggestion:
                        key = "image_suggestion_next"; break;
                    case EAnnotationImage.PreviousSuggestion:
                        key = "image_suggestion_previous"; break;
                    case EAnnotationImage.InsertSuggestion:
                        key = "image_suggestion_insert"; break;
                    case EAnnotationImage.CancelSuggestions:
                        key = "image_suggestion_cancel"; break;
                    case EAnnotationImage.WindowsKey:
                        key = "image_start_button"; break;
                    case EAnnotationImage.ApplicationsKey:
                        key = "image_apps_key"; break;
                    case EAnnotationImage.LeftArrow:
                        key = "image_left_cursor"; break;
                    case EAnnotationImage.RightArrow:
                        key = "image_right_cursor"; break;
                    case EAnnotationImage.UpArrow:
                        key = "image_up_cursor"; break;
                    case EAnnotationImage.DownArrow:
                        key = "image_down_cursor"; break;
                    //case EAnnotationImage.BulletArrowRight:
                    //    key = "image_bullet_arrow_right"; break;
                    //case EAnnotationImage.BulletPoint:
                    //    key = "image_bullet_grey"; break;
                    
                    case EAnnotationImage.HoldLetterKey:
                        key = "image_hold_letter_key"; break;
                    case EAnnotationImage.HoldNumberKey:
                        key = "image_hold_number_key"; break;
                    case EAnnotationImage.HoldSymbolKey:
                        key = "image_hold_symbol_key"; break;
                    case EAnnotationImage.HoldArrowKey:
                        key = "image_hold_arrow_key"; break;
                    case EAnnotationImage.HoldFunctionKey:
                        key = "image_hold_function_key"; break;
                    case EAnnotationImage.HoldNumpadKey:
                        key = "image_hold_num_pad_key"; break;
                    case EAnnotationImage.HoldOtherKey:
                        key = "image_hold_other_key"; break;

                    case EAnnotationImage.TypeLetterKey:
                        key = "image_type_letter_key"; break;
                    case EAnnotationImage.TypeShiftedLetterKey:
                        key = "image_type_shifted_letter_key"; break;
                    case EAnnotationImage.TypeNumberKey:
                        key = "image_type_number_key"; break;
                    case EAnnotationImage.TypeShiftedNumberKey:
                        key = "image_type_shifted_number_key"; break;
                    case EAnnotationImage.TypeSymbolKey:
                        key = "image_type_symbol_key"; break;
                    case EAnnotationImage.TypeShiftedSymbolKey:
                        key = "image_type_shifted_symbol_key"; break;
                    case EAnnotationImage.TypeArrowKey:
                        key = "image_type_arrow_key"; break;
                    case EAnnotationImage.TypeFunctionKey:
                        key = "image_type_function_key"; break;
                    case EAnnotationImage.TypeNumpadKey:
                        key = "image_type_num_pad_key"; break;
                    case EAnnotationImage.TypeOtherKey:
                        key = "image_type_other_key"; break;

                    case EAnnotationImage.AutorepeatLetterKey:
                        key = "image_autorepeat_letter_key"; break;
                    case EAnnotationImage.AutorepeatNumberKey:
                        key = "image_autorepeat_number_key"; break;
                    case EAnnotationImage.AutorepeatSymbolKey:
                        key = "image_autorepeat_symbol_key"; break;
                    case EAnnotationImage.AutorepeatArrowKey:
                        key = "image_autorepeat_arrow_key"; break;
                    case EAnnotationImage.AutorepeatFunctionKey:
                        key = "image_autorepeat_function_key"; break;
                    case EAnnotationImage.AutorepeatNumpadKey:
                        key = "image_autorepeat_num_pad_key"; break;
                    case EAnnotationImage.AutorepeatOtherKey:
                        key = "image_autorepeat_other_key"; break;

                    case EAnnotationImage.ToggleKey:
                        key = "image_toggle_key"; break;

                    case EAnnotationImage.Clock:
                        key = "image_clock"; break;
                    case EAnnotationImage.DirectionMode:
                        key = "image_direction_mode"; break;
                    case EAnnotationImage.Combination:
                        key = "image_combinations"; break;
                    case EAnnotationImage.NextTrack:
                        key = "image_control_end_blue"; break;
                    case EAnnotationImage.PreviousTrack:
                        key = "image_control_start_blue"; break;
                    case EAnnotationImage.PlayPause:
                        key = "image_control_play_blue"; break;
                    case EAnnotationImage.StopPlaying:
                        key = "image_control_stop_blue"; break;
                    case EAnnotationImage.FastForward:
                        key = "image_control_fastforward_blue"; break;
                    case EAnnotationImage.Rewind:
                        key = "image_control_rewind_blue"; break;
                    case EAnnotationImage.Calculator:
                        key = "image_calculator"; break;
                    case EAnnotationImage.Mail:
                        key = "image_email"; break;
                    case EAnnotationImage.Explorer:
                        key = "image_folder"; break;
                    case EAnnotationImage.Browser:
                        key = "image_browser"; break;
                    case EAnnotationImage.BrowserBack:
                        key = "image_browser_back"; break;
                    case EAnnotationImage.BrowserFavourites:
                        key = "image_browser_favourites"; break;
                    case EAnnotationImage.BrowserForward:
                        key = "image_browser_forward"; break;
                    case EAnnotationImage.BrowserHome:
                        key = "image_browser_home"; break;
                    case EAnnotationImage.BrowserRefresh:
                        key = "image_browser_refresh"; break;
                    case EAnnotationImage.BrowserSearch:
                        key = "image_browser_search"; break;
                    case EAnnotationImage.BrowserStop:
                        key = "image_browser_stop"; break;
                    case EAnnotationImage.MediaPlayer:
                        key = "image_music_notes"; break;
                    case EAnnotationImage.VolumeUp:
                        key = "image_sound"; break;
                    case EAnnotationImage.VolumeDown:
                        key = "image_sound_low"; break;
                    case EAnnotationImage.VolumeMute:
                        key = "image_sound_mute"; break;
                    case EAnnotationImage.BulletRed:
                        key = "image_bullet_red"; break;
                    case EAnnotationImage.BulletOrange:
                        key = "image_bullet_orange"; break;
                    case EAnnotationImage.BulletYellow:
                        key = "image_bullet_yellow"; break;
                    case EAnnotationImage.BulletGreen:
                        key = "image_bullet_green"; break;
                    case EAnnotationImage.BulletBlue:
                        key = "image_bullet_blue"; break;
                    case EAnnotationImage.BulletWhite:
                        key = "image_bullet_white"; break;
                    case EAnnotationImage.OpenFile:
                        key = "image_folder_page"; break;
                    case EAnnotationImage.OpenFolder:
                        key = "image_folder_explore"; break;
                    case EAnnotationImage.SaveFile:
                        key = "image_page_save"; break;
                    case EAnnotationImage.EditFile:
                        key = "image_application_form_edit"; break;
                    case EAnnotationImage.Help:
                        key = "image_help"; break;
                    case EAnnotationImage.ViewText:
                        key = "image_page_white_text"; break;
                    case EAnnotationImage.Settings:
                        key = "image_cog"; break;
                    case EAnnotationImage.ProgramUpdates:
                        key = "image_shield_go"; break;
                    case EAnnotationImage.Information:
                        key = "image_information"; break;
                    case EAnnotationImage.Exit:
                        key = "image_cross"; break;
                    case EAnnotationImage.DoNothing:
                        key = "image_lock"; break;
                }

                if (key != null)
                {
                    icon = (BitmapImage)App.Current.FindResource(key);
                    //Trace.WriteLine(string.Format("{0}: {1} x {2}", key, icon.Width, icon.Height));
                }

                // Cache
                _iconLookup[iconRef] = icon;
            }


            return icon;
        }

        /// <summary>
        /// Convert between image formats
        /// </summary>
        /// <param name="bitmapImage"></param>
        /// <returns></returns>
        public static System.Drawing.Bitmap ConvertImage(BitmapImage bitmapImage)
        {
            System.Drawing.Bitmap bitmap = null;
            if (bitmapImage != null)
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    PngBitmapEncoder enc = new PngBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                    enc.Save(outStream);
                    bitmap = new System.Drawing.Bitmap(outStream);

                    //bitmap = new System.Drawing.Bitmap(bitmap);
                }
            }

            return bitmap;
        }        

    }
}
