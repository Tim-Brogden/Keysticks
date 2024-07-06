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
using Keysticks.Config;

namespace Keysticks.Core
{
    /// <summary>
    /// Keyboard grid layout utility methods
    /// </summary>
    public class CellUtils
    {
        // Fields
        //private Dictionary<int, int> _cellLookup;
        private EKeyboardKey[] _keyLeftWrap = new EKeyboardKey[]
        {
        EKeyboardKey.None,
        EKeyboardKey.Delete,EKeyboardKey.Escape, EKeyboardKey.F1, EKeyboardKey.F2, EKeyboardKey.F3, EKeyboardKey.F4, EKeyboardKey.F5, EKeyboardKey.F6, EKeyboardKey.F7, EKeyboardKey.F8, EKeyboardKey.F9, EKeyboardKey.F10, EKeyboardKey.F11, EKeyboardKey.F12, EKeyboardKey.Insert, EKeyboardKey.PrintScreen, 
        EKeyboardKey.Home,EKeyboardKey.Backtick, EKeyboardKey.D1, EKeyboardKey.D2, EKeyboardKey.D3, EKeyboardKey.D4, EKeyboardKey.D5, EKeyboardKey.D6, EKeyboardKey.D7, EKeyboardKey.D8, EKeyboardKey.D9, EKeyboardKey.D0, EKeyboardKey.MinusSign, EKeyboardKey.EqualsSign, EKeyboardKey.Backspace, 
        EKeyboardKey.PageUp,EKeyboardKey.Tab, EKeyboardKey.Q, EKeyboardKey.W, EKeyboardKey.E, EKeyboardKey.R, EKeyboardKey.T, EKeyboardKey.Y, EKeyboardKey.U, EKeyboardKey.I, EKeyboardKey.O, EKeyboardKey.P, EKeyboardKey.LeftBracket, EKeyboardKey.RightBracket, EKeyboardKey.Hash, 
        EKeyboardKey.PageDown,EKeyboardKey.CapsLock, EKeyboardKey.A, EKeyboardKey.S, EKeyboardKey.D, EKeyboardKey.F, EKeyboardKey.G, EKeyboardKey.H, EKeyboardKey.J, EKeyboardKey.K, EKeyboardKey.L, EKeyboardKey.Semicolon, EKeyboardKey.Apostrophe, EKeyboardKey.Return, 
        EKeyboardKey.End,EKeyboardKey.LShiftKey, EKeyboardKey.Backslash, EKeyboardKey.Z, EKeyboardKey.X, EKeyboardKey.C, EKeyboardKey.V, EKeyboardKey.B, EKeyboardKey.N, EKeyboardKey.M, EKeyboardKey.Comma, EKeyboardKey.Fullstop, EKeyboardKey.Slash, EKeyboardKey.RShiftKey, EKeyboardKey.Up, 
        EKeyboardKey.Right, EKeyboardKey.LControlKey, EKeyboardKey.LWin, EKeyboardKey.LMenu, EKeyboardKey.Spacebar, EKeyboardKey.RMenu, EKeyboardKey.RWin, EKeyboardKey.Apps, EKeyboardKey.RControlKey, EKeyboardKey.Left, EKeyboardKey.Down
        };
        private EKeyboardKey[] _keyLeftNoWrap = new EKeyboardKey[]
        {
        EKeyboardKey.None,
        EKeyboardKey.None,EKeyboardKey.Escape, EKeyboardKey.F1, EKeyboardKey.F2, EKeyboardKey.F3, EKeyboardKey.F4, EKeyboardKey.F5, EKeyboardKey.F6, EKeyboardKey.F7, EKeyboardKey.F8, EKeyboardKey.F9, EKeyboardKey.F10, EKeyboardKey.F11, EKeyboardKey.F12, EKeyboardKey.Insert, EKeyboardKey.PrintScreen, 
        EKeyboardKey.None,EKeyboardKey.Backtick, EKeyboardKey.D1, EKeyboardKey.D2, EKeyboardKey.D3, EKeyboardKey.D4, EKeyboardKey.D5, EKeyboardKey.D6, EKeyboardKey.D7, EKeyboardKey.D8, EKeyboardKey.D9, EKeyboardKey.D0, EKeyboardKey.MinusSign, EKeyboardKey.EqualsSign, EKeyboardKey.Backspace, 
        EKeyboardKey.None,EKeyboardKey.Tab, EKeyboardKey.Q, EKeyboardKey.W, EKeyboardKey.E, EKeyboardKey.R, EKeyboardKey.T, EKeyboardKey.Y, EKeyboardKey.U, EKeyboardKey.I, EKeyboardKey.O, EKeyboardKey.P, EKeyboardKey.LeftBracket, EKeyboardKey.RightBracket, EKeyboardKey.Hash, 
        EKeyboardKey.None,EKeyboardKey.CapsLock, EKeyboardKey.A, EKeyboardKey.S, EKeyboardKey.D, EKeyboardKey.F, EKeyboardKey.G, EKeyboardKey.H, EKeyboardKey.J, EKeyboardKey.K, EKeyboardKey.L, EKeyboardKey.Semicolon, EKeyboardKey.Apostrophe, EKeyboardKey.Return, 
        EKeyboardKey.None,EKeyboardKey.LShiftKey, EKeyboardKey.Backslash, EKeyboardKey.Z, EKeyboardKey.X, EKeyboardKey.C, EKeyboardKey.V, EKeyboardKey.B, EKeyboardKey.N, EKeyboardKey.M, EKeyboardKey.Comma, EKeyboardKey.Fullstop, EKeyboardKey.Slash, EKeyboardKey.RShiftKey, EKeyboardKey.Up, 
        EKeyboardKey.None, EKeyboardKey.LControlKey, EKeyboardKey.LWin, EKeyboardKey.LMenu, EKeyboardKey.Spacebar, EKeyboardKey.RMenu, EKeyboardKey.RWin, EKeyboardKey.Apps, EKeyboardKey.RControlKey, EKeyboardKey.Left, EKeyboardKey.Down
        };
        private EKeyboardKey[] _keyRightWrap = new EKeyboardKey[]
        {
        EKeyboardKey.None,
        EKeyboardKey.F1, EKeyboardKey.F2, EKeyboardKey.F3, EKeyboardKey.F4, EKeyboardKey.F5, EKeyboardKey.F6, EKeyboardKey.F7, EKeyboardKey.F8, EKeyboardKey.F9, EKeyboardKey.F10, EKeyboardKey.F11, EKeyboardKey.F12, EKeyboardKey.Insert, EKeyboardKey.PrintScreen, EKeyboardKey.Delete,EKeyboardKey.Escape, 
        EKeyboardKey.D1, EKeyboardKey.D2, EKeyboardKey.D3, EKeyboardKey.D4, EKeyboardKey.D5, EKeyboardKey.D6, EKeyboardKey.D7, EKeyboardKey.D8, EKeyboardKey.D9, EKeyboardKey.D0, EKeyboardKey.MinusSign, EKeyboardKey.EqualsSign, EKeyboardKey.Backspace, EKeyboardKey.Home,EKeyboardKey.Backtick, 
        EKeyboardKey.Q, EKeyboardKey.W, EKeyboardKey.E, EKeyboardKey.R, EKeyboardKey.T, EKeyboardKey.Y, EKeyboardKey.U, EKeyboardKey.I, EKeyboardKey.O, EKeyboardKey.P, EKeyboardKey.LeftBracket, EKeyboardKey.RightBracket, EKeyboardKey.Hash, EKeyboardKey.PageUp,EKeyboardKey.Tab, 
        EKeyboardKey.A, EKeyboardKey.S, EKeyboardKey.D, EKeyboardKey.F, EKeyboardKey.G, EKeyboardKey.H, EKeyboardKey.J, EKeyboardKey.K, EKeyboardKey.L, EKeyboardKey.Semicolon, EKeyboardKey.Apostrophe, EKeyboardKey.Return, EKeyboardKey.PageDown,EKeyboardKey.CapsLock, 
        EKeyboardKey.Backslash, EKeyboardKey.Z, EKeyboardKey.X, EKeyboardKey.C, EKeyboardKey.V, EKeyboardKey.B, EKeyboardKey.N, EKeyboardKey.M, EKeyboardKey.Comma, EKeyboardKey.Fullstop, EKeyboardKey.Slash, EKeyboardKey.RShiftKey, EKeyboardKey.Up, EKeyboardKey.End,EKeyboardKey.LShiftKey, 
        EKeyboardKey.LWin, EKeyboardKey.LMenu, EKeyboardKey.Spacebar, EKeyboardKey.RMenu, EKeyboardKey.RWin, EKeyboardKey.Apps, EKeyboardKey.RControlKey, EKeyboardKey.Left, EKeyboardKey.Down, EKeyboardKey.Right,EKeyboardKey.LControlKey
        };
        private EKeyboardKey[] _keyRightNoWrap = new EKeyboardKey[]
        {
        EKeyboardKey.None,
        EKeyboardKey.F1, EKeyboardKey.F2, EKeyboardKey.F3, EKeyboardKey.F4, EKeyboardKey.F5, EKeyboardKey.F6, EKeyboardKey.F7, EKeyboardKey.F8, EKeyboardKey.F9, EKeyboardKey.F10, EKeyboardKey.F11, EKeyboardKey.F12, EKeyboardKey.Insert, EKeyboardKey.PrintScreen, EKeyboardKey.Delete,EKeyboardKey.None, 
        EKeyboardKey.D1, EKeyboardKey.D2, EKeyboardKey.D3, EKeyboardKey.D4, EKeyboardKey.D5, EKeyboardKey.D6, EKeyboardKey.D7, EKeyboardKey.D8, EKeyboardKey.D9, EKeyboardKey.D0, EKeyboardKey.MinusSign, EKeyboardKey.EqualsSign, EKeyboardKey.Backspace, EKeyboardKey.Home,EKeyboardKey.None, 
        EKeyboardKey.Q, EKeyboardKey.W, EKeyboardKey.E, EKeyboardKey.R, EKeyboardKey.T, EKeyboardKey.Y, EKeyboardKey.U, EKeyboardKey.I, EKeyboardKey.O, EKeyboardKey.P, EKeyboardKey.LeftBracket, EKeyboardKey.RightBracket, EKeyboardKey.Hash, EKeyboardKey.PageUp,EKeyboardKey.None, 
        EKeyboardKey.A, EKeyboardKey.S, EKeyboardKey.D, EKeyboardKey.F, EKeyboardKey.G, EKeyboardKey.H, EKeyboardKey.J, EKeyboardKey.K, EKeyboardKey.L, EKeyboardKey.Semicolon, EKeyboardKey.Apostrophe, EKeyboardKey.Return, EKeyboardKey.PageDown,EKeyboardKey.None, 
        EKeyboardKey.Backslash, EKeyboardKey.Z, EKeyboardKey.X, EKeyboardKey.C, EKeyboardKey.V, EKeyboardKey.B, EKeyboardKey.N, EKeyboardKey.M, EKeyboardKey.Comma, EKeyboardKey.Fullstop, EKeyboardKey.Slash, EKeyboardKey.RShiftKey, EKeyboardKey.Up, EKeyboardKey.End,EKeyboardKey.None, 
        EKeyboardKey.LWin, EKeyboardKey.LMenu, EKeyboardKey.Spacebar, EKeyboardKey.RMenu, EKeyboardKey.RWin, EKeyboardKey.Apps, EKeyboardKey.RControlKey, EKeyboardKey.Left, EKeyboardKey.Down, EKeyboardKey.Right,EKeyboardKey.None
        };
        private EKeyboardKey[] _keyDownWrap = new EKeyboardKey[]
        {
        EKeyboardKey.None,
        EKeyboardKey.Backtick, EKeyboardKey.D1, EKeyboardKey.D2, EKeyboardKey.D3, EKeyboardKey.D4, EKeyboardKey.D5, EKeyboardKey.D6, EKeyboardKey.D7, EKeyboardKey.D8, EKeyboardKey.D9, EKeyboardKey.D0, EKeyboardKey.MinusSign, EKeyboardKey.EqualsSign, EKeyboardKey.Backspace, EKeyboardKey.Backspace, EKeyboardKey.Home,
        EKeyboardKey.Tab, EKeyboardKey.Tab, EKeyboardKey.Q, EKeyboardKey.W, EKeyboardKey.E, EKeyboardKey.R, EKeyboardKey.T, EKeyboardKey.Y, EKeyboardKey.U, EKeyboardKey.I, EKeyboardKey.O, EKeyboardKey.P, EKeyboardKey.LeftBracket, EKeyboardKey.RightBracket, EKeyboardKey.PageUp,
        EKeyboardKey.CapsLock, EKeyboardKey.A, EKeyboardKey.S, EKeyboardKey.D, EKeyboardKey.F, EKeyboardKey.G, EKeyboardKey.H, EKeyboardKey.J, EKeyboardKey.K, EKeyboardKey.L, EKeyboardKey.Semicolon, EKeyboardKey.Apostrophe, EKeyboardKey.Return, EKeyboardKey.Return, EKeyboardKey.PageDown,
        EKeyboardKey.LShiftKey, EKeyboardKey.Z, EKeyboardKey.X, EKeyboardKey.C, EKeyboardKey.V, EKeyboardKey.B, EKeyboardKey.N, EKeyboardKey.M, EKeyboardKey.Comma, EKeyboardKey.Fullstop, EKeyboardKey.Slash, EKeyboardKey.RShiftKey, EKeyboardKey.Up, EKeyboardKey.End,
        EKeyboardKey.LControlKey, EKeyboardKey.LControlKey, EKeyboardKey.LWin, EKeyboardKey.LMenu, EKeyboardKey.Spacebar, EKeyboardKey.Spacebar, EKeyboardKey.Spacebar, EKeyboardKey.Spacebar, EKeyboardKey.Spacebar, EKeyboardKey.RMenu, EKeyboardKey.RWin, EKeyboardKey.Apps, EKeyboardKey.RControlKey, EKeyboardKey.Down, EKeyboardKey.Right,
        EKeyboardKey.Escape, EKeyboardKey.F2, EKeyboardKey.F3, EKeyboardKey.F6, EKeyboardKey.F9, EKeyboardKey.F10, EKeyboardKey.F11, EKeyboardKey.F12, EKeyboardKey.Insert, EKeyboardKey.PrintScreen, EKeyboardKey.Delete
        };
        private EKeyboardKey[] _keyDownNoWrap = new EKeyboardKey[]
        {
        EKeyboardKey.None,
        EKeyboardKey.Backtick, EKeyboardKey.D1, EKeyboardKey.D2, EKeyboardKey.D3, EKeyboardKey.D4, EKeyboardKey.D5, EKeyboardKey.D6, EKeyboardKey.D7, EKeyboardKey.D8, EKeyboardKey.D9, EKeyboardKey.D0, EKeyboardKey.MinusSign, EKeyboardKey.EqualsSign, EKeyboardKey.Backspace, EKeyboardKey.Backspace, EKeyboardKey.Home,
        EKeyboardKey.Tab, EKeyboardKey.Tab, EKeyboardKey.Q, EKeyboardKey.W, EKeyboardKey.E, EKeyboardKey.R, EKeyboardKey.T, EKeyboardKey.Y, EKeyboardKey.U, EKeyboardKey.I, EKeyboardKey.O, EKeyboardKey.P, EKeyboardKey.LeftBracket, EKeyboardKey.RightBracket, EKeyboardKey.PageUp,
        EKeyboardKey.CapsLock, EKeyboardKey.A, EKeyboardKey.S, EKeyboardKey.D, EKeyboardKey.F, EKeyboardKey.G, EKeyboardKey.H, EKeyboardKey.J, EKeyboardKey.K, EKeyboardKey.L, EKeyboardKey.Semicolon, EKeyboardKey.Apostrophe, EKeyboardKey.Return, EKeyboardKey.Return, EKeyboardKey.PageDown,
        EKeyboardKey.LShiftKey, EKeyboardKey.Z, EKeyboardKey.X, EKeyboardKey.C, EKeyboardKey.V, EKeyboardKey.B, EKeyboardKey.N, EKeyboardKey.M, EKeyboardKey.Comma, EKeyboardKey.Fullstop, EKeyboardKey.Slash, EKeyboardKey.RShiftKey, EKeyboardKey.Up, EKeyboardKey.End,
        EKeyboardKey.LControlKey, EKeyboardKey.LControlKey, EKeyboardKey.LWin, EKeyboardKey.LMenu, EKeyboardKey.Spacebar, EKeyboardKey.Spacebar, EKeyboardKey.Spacebar, EKeyboardKey.Spacebar, EKeyboardKey.Spacebar, EKeyboardKey.RMenu, EKeyboardKey.RWin, EKeyboardKey.Apps, EKeyboardKey.RControlKey, EKeyboardKey.Down, EKeyboardKey.Right,
        EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None
        };
        private EKeyboardKey[] _keyUpWrap = new EKeyboardKey[]
        {
        EKeyboardKey.None,
        EKeyboardKey.LControlKey, EKeyboardKey.LControlKey, EKeyboardKey.LWin, EKeyboardKey.LMenu, EKeyboardKey.Spacebar, EKeyboardKey.Spacebar, EKeyboardKey.Spacebar, EKeyboardKey.Spacebar, EKeyboardKey.Spacebar, EKeyboardKey.RMenu, EKeyboardKey.RWin, EKeyboardKey.Apps, EKeyboardKey.RControlKey, EKeyboardKey.Left, EKeyboardKey.Down, EKeyboardKey.Right,
        EKeyboardKey.Escape, EKeyboardKey.F1, EKeyboardKey.F2, EKeyboardKey.F3, EKeyboardKey.F4, EKeyboardKey.F5, EKeyboardKey.F6, EKeyboardKey.F7, EKeyboardKey.F8, EKeyboardKey.F9, EKeyboardKey.F10, EKeyboardKey.F11, EKeyboardKey.F12, EKeyboardKey.Insert, EKeyboardKey.Delete,
        EKeyboardKey.Backtick, EKeyboardKey.D2, EKeyboardKey.D3, EKeyboardKey.D4, EKeyboardKey.D5, EKeyboardKey.D6, EKeyboardKey.D7, EKeyboardKey.D8, EKeyboardKey.D9, EKeyboardKey.D0, EKeyboardKey.MinusSign, EKeyboardKey.EqualsSign, EKeyboardKey.Backspace, EKeyboardKey.Backspace, EKeyboardKey.Home,
        EKeyboardKey.Tab, EKeyboardKey.Q, EKeyboardKey.W, EKeyboardKey.E, EKeyboardKey.R, EKeyboardKey.T, EKeyboardKey.Y, EKeyboardKey.U, EKeyboardKey.I, EKeyboardKey.O, EKeyboardKey.P, EKeyboardKey.LeftBracket, EKeyboardKey.RightBracket, EKeyboardKey.PageUp,
        EKeyboardKey.CapsLock, EKeyboardKey.CapsLock, EKeyboardKey.A, EKeyboardKey.S, EKeyboardKey.D, EKeyboardKey.F, EKeyboardKey.G, EKeyboardKey.H, EKeyboardKey.J, EKeyboardKey.K, EKeyboardKey.L, EKeyboardKey.Semicolon, EKeyboardKey.Apostrophe, EKeyboardKey.Return, EKeyboardKey.PageDown,
        EKeyboardKey.LShiftKey, EKeyboardKey.Z, EKeyboardKey.X, EKeyboardKey.B, EKeyboardKey.Comma, EKeyboardKey.Fullstop, EKeyboardKey.Slash, EKeyboardKey.RShiftKey, EKeyboardKey.RShiftKey, EKeyboardKey.Up, EKeyboardKey.End
        };
        private EKeyboardKey[] _keyUpNoWrap = new EKeyboardKey[]
        {
        EKeyboardKey.None,
        EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None, EKeyboardKey.None,
        EKeyboardKey.Escape, EKeyboardKey.F1, EKeyboardKey.F2, EKeyboardKey.F3, EKeyboardKey.F4, EKeyboardKey.F5, EKeyboardKey.F6, EKeyboardKey.F7, EKeyboardKey.F8, EKeyboardKey.F9, EKeyboardKey.F10, EKeyboardKey.F11, EKeyboardKey.F12, EKeyboardKey.Insert, EKeyboardKey.Delete,
        EKeyboardKey.Backtick, EKeyboardKey.D2, EKeyboardKey.D3, EKeyboardKey.D4, EKeyboardKey.D5, EKeyboardKey.D6, EKeyboardKey.D7, EKeyboardKey.D8, EKeyboardKey.D9, EKeyboardKey.D0, EKeyboardKey.MinusSign, EKeyboardKey.EqualsSign, EKeyboardKey.Backspace, EKeyboardKey.Backspace, EKeyboardKey.Home,
        EKeyboardKey.Tab, EKeyboardKey.Q, EKeyboardKey.W, EKeyboardKey.E, EKeyboardKey.R, EKeyboardKey.T, EKeyboardKey.Y, EKeyboardKey.U, EKeyboardKey.I, EKeyboardKey.O, EKeyboardKey.P, EKeyboardKey.LeftBracket, EKeyboardKey.RightBracket, EKeyboardKey.PageUp,
        EKeyboardKey.CapsLock, EKeyboardKey.CapsLock, EKeyboardKey.A, EKeyboardKey.S, EKeyboardKey.D, EKeyboardKey.F, EKeyboardKey.G, EKeyboardKey.H, EKeyboardKey.J, EKeyboardKey.K, EKeyboardKey.L, EKeyboardKey.Semicolon, EKeyboardKey.Apostrophe, EKeyboardKey.Return, EKeyboardKey.PageDown,
        EKeyboardKey.LShiftKey, EKeyboardKey.Z, EKeyboardKey.X, EKeyboardKey.B, EKeyboardKey.Comma, EKeyboardKey.Fullstop, EKeyboardKey.Slash, EKeyboardKey.RShiftKey, EKeyboardKey.RShiftKey, EKeyboardKey.Up, EKeyboardKey.End
        };

        private int[] _squareLeftWrap = new int[]
        {
            Constants.TopRightCellID, Constants.TopLeftCellID, Constants.TopCentreCellID,
            Constants.CentreRightCellID, Constants.CentreLeftCellID, Constants.CentreCellID,
            Constants.BottomRightCellID, Constants.BottomLeftCellID, Constants.BottomCentreCellID
        };
        private int[] _squareLeftNoWrap = new int[]
        {
            Constants.NoneID, Constants.TopLeftCellID, Constants.TopCentreCellID,
            Constants.NoneID, Constants.CentreLeftCellID, Constants.CentreCellID,
            Constants.NoneID, Constants.BottomLeftCellID, Constants.BottomCentreCellID
        };
        private int[] _squareRightWrap = new int[]
        {
            Constants.TopCentreCellID, Constants.TopRightCellID, Constants.TopLeftCellID, 
            Constants.CentreCellID,Constants.CentreRightCellID, Constants.CentreLeftCellID, 
            Constants.BottomCentreCellID,Constants.BottomRightCellID, Constants.BottomLeftCellID
        };
        private int[] _squareRightNoWrap = new int[]
        {
            Constants.TopCentreCellID, Constants.TopRightCellID, Constants.NoneID, 
            Constants.CentreCellID,Constants.CentreRightCellID, Constants.NoneID, 
            Constants.BottomCentreCellID,Constants.BottomRightCellID, Constants.NoneID
        };
        private int[] _squareDownWrap = new int[]
        {
            Constants.CentreLeftCellID, Constants.CentreCellID,Constants.CentreRightCellID, 
            Constants.BottomLeftCellID, Constants.BottomCentreCellID,Constants.BottomRightCellID,
            Constants.TopLeftCellID, Constants.TopCentreCellID, Constants.TopRightCellID
        };
        private int[] _squareDownNoWrap = new int[]
        {
            Constants.CentreLeftCellID, Constants.CentreCellID,Constants.CentreRightCellID, 
            Constants.BottomLeftCellID, Constants.BottomCentreCellID,Constants.BottomRightCellID,
            Constants.NoneID, Constants.NoneID, Constants.NoneID
        };
        private int[] _squareUpWrap = new int[]
        {
            Constants.BottomLeftCellID, Constants.BottomCentreCellID,Constants.BottomRightCellID,
            Constants.TopLeftCellID, Constants.TopCentreCellID, Constants.TopRightCellID,
            Constants.CentreLeftCellID, Constants.CentreCellID,Constants.CentreRightCellID
        };
        private int[] _squareUpNoWrap = new int[]
        {
            Constants.NoneID, Constants.NoneID,Constants.NoneID,
            Constants.TopLeftCellID, Constants.TopCentreCellID, Constants.TopRightCellID,
            Constants.CentreLeftCellID, Constants.CentreCellID,Constants.CentreRightCellID
        };

        /// <summary>
        /// Constructor
        /// </summary>
        public CellUtils()
        {
            //_cellLookup = new Dictionary<int, int>();
        }

        /// <summary>
        /// Find the adjacent cell to move to, according to the current cell, window type and move direction
        /// </summary>
        /// <param name="cellID"></param>
        /// <param name="_direction"></param>
        /// <param name="gridType"></param>
        /// <returns></returns>
        public int FindCellToSelect(int currentCellID, ELRUDState direction, AxisValue modeItem, bool allowWrapAround)
        {
            int newCellID = Constants.NoneID;

            //int key = (int)currentCellID |
            //            ((int)direction << 8) |
            //            ((int)modeItem.GridType << 16) |
            //            (allowWrapAround ? 1 << 24 : 0);
            //if (_cellLookup.ContainsKey(key))
            //{
            //    // Use cached value
            //    newCellID = _cellLookup[key];
            //}
            //else
            //{
                switch (modeItem.GridType)
                {
                    case EGridType.Keyboard:
                        newCellID = FindKeyboardCell(currentCellID, direction, allowWrapAround);
                        break;
                    case EGridType.ActionStrip:
                        newCellID = FindActionStripCell(currentCellID, direction, modeItem, allowWrapAround);
                        break;
                    case EGridType.Square4x4:
                        newCellID = FindPlusGridCell(currentCellID, direction, allowWrapAround);
                        break;
                    case EGridType.Square8x4:
                        newCellID = FindSquareGridCell(currentCellID, direction, allowWrapAround);
                        break;
                }

                // Cache for next time
                //_cellLookup[key] = newCellID;
            //}

            return newCellID;
        }

        private int FindKeyboardCell(int currentCellID, ELRUDState direction, bool allowWrapAround)
        {
            int newCellID = Constants.NoneID;

            EKeyboardKey[] lookupArray = null;
            switch (direction)
            {
                case ELRUDState.Left:
                    lookupArray = allowWrapAround ? _keyLeftWrap : _keyLeftNoWrap; break;
                case ELRUDState.Right:
                    lookupArray = allowWrapAround ? _keyRightWrap : _keyRightNoWrap; break;
                case ELRUDState.Up:
                    lookupArray = allowWrapAround ? _keyUpWrap : _keyUpNoWrap; break;
                case ELRUDState.Down:
                    lookupArray = allowWrapAround ? _keyDownWrap : _keyDownNoWrap; break;
            }

            if (lookupArray != null && currentCellID > 0 && currentCellID < lookupArray.Length)
            {
                newCellID = (int)lookupArray[currentCellID];
            }

            return newCellID;
        }

        private int FindActionStripCell(int currentCellID, ELRUDState direction, AxisValue modeItem, bool allowWrapAround)
        {
            int newCellID = Constants.NoneID;

            // Get the number of cells in the strip
            int maxID = Constants.ActionStripMaxCells;
            if (modeItem.SubValues.Count != 0)
            {
                AxisValue pageItem = (AxisValue)modeItem.SubValues[0];
                maxID = pageItem.SubValues.Count - 1;    // Cell IDs are 1,...,N plus default cell (-1)
            }

            switch (direction)
            {
                // Backwards
                case ELRUDState.Left:
                case ELRUDState.Up:
                    if (currentCellID > 1)
                    {
                        newCellID = currentCellID - 1;
                    }
                    else if (allowWrapAround)
                    {
                        // Wrap first cell
                        newCellID = maxID;
                    }
                    break;
                // Forwards
                case ELRUDState.Right:
                case ELRUDState.Down:
                    if (currentCellID < maxID)
                    {
                        newCellID = currentCellID + 1;
                    }
                    else if (allowWrapAround)
                    {
                        // Wrap last cell
                        newCellID = 1;
                    }
                    break;
            }

            return newCellID;
        }        

        /// <summary>
        /// Find adjacent grid cell in the specified direction, for a square 3x3 grid
        /// </summary>
        /// <param name="currentCellID"></param>
        /// <param name="direction"></param>
        /// <param name="allowWrapAround"></param>
        /// <returns></returns>
        private int FindSquareGridCell(int currentCellID, ELRUDState direction, bool allowWrapAround)
        {
            int newCellID = Constants.NoneID;

            int[] lookupArray = null;
            switch (direction)
            {
                case ELRUDState.Left:
                    lookupArray = allowWrapAround ? _squareLeftWrap : _squareLeftNoWrap; break;
                case ELRUDState.Right:
                    lookupArray = allowWrapAround ? _squareRightWrap : _squareRightNoWrap; break;
                case ELRUDState.Up:
                    lookupArray = allowWrapAround ? _squareUpWrap : _squareUpNoWrap; break;
                case ELRUDState.Down:
                    lookupArray = allowWrapAround ? _squareDownWrap : _squareDownNoWrap; break;
            }

            int cellIndex = currentCellID - Constants.TopLeftCellID;
            if (lookupArray != null && cellIndex > -1 && cellIndex < lookupArray.Length)
            {
                newCellID = lookupArray[cellIndex];
            }   

            return newCellID;
        }

        /// <summary>
        /// Find adjacent grid cell in the specified direction, for a 3x3 "Plus" grid (i.e. centre plus LRUD cells only)
        /// </summary>
        /// <param name="currentCellID"></param>
        /// <param name="direction"></param>
        /// <param name="allowWrapAround"></param>
        /// <returns></returns>
        private int FindPlusGridCell(int currentCellID, ELRUDState direction, bool allowWrapAround)
        {
            int newCellID = Constants.NoneID;

            // Adjust the new cell to make sure it is one of the "Plus cells"
            switch (currentCellID)
            {
                case Constants.CentreCellID:
                    switch (direction)
                    {
                        case ELRUDState.Left:
                        case ELRUDState.Right:
                        case ELRUDState.Up:
                        case ELRUDState.Down:
                            newCellID = FindSquareGridCell(currentCellID, direction, allowWrapAround); break;
                    }
                    break;
                case Constants.CentreLeftCellID:
                    switch (direction)
                    {
                        case ELRUDState.Left:
                        case ELRUDState.Right:
                            newCellID = FindSquareGridCell(currentCellID, direction, allowWrapAround); break;
                        case ELRUDState.Up:
                        //case ELRUDState.UpRight:
                            newCellID = Constants.TopCentreCellID; break;
                        case ELRUDState.Down:
                        //case ELRUDState.DownRight:
                            newCellID = Constants.BottomCentreCellID; break;
                    }
                    break;
                case Constants.CentreRightCellID:
                    switch (direction)
                    {
                        case ELRUDState.Left:
                        case ELRUDState.Right:
                            newCellID = FindSquareGridCell(currentCellID, direction, allowWrapAround); break;
                        case ELRUDState.Up:
                        //case ELRUDState.UpLeft:
                            newCellID = Constants.TopCentreCellID; break;
                        case ELRUDState.Down:
                        //case ELRUDState.DownLeft:
                            newCellID = Constants.BottomCentreCellID; break;
                    }
                    break;
                case Constants.TopCentreCellID:
                    switch (direction)
                    {
                        case ELRUDState.Left:
                        //case ELRUDState.DownLeft:
                            newCellID = Constants.CentreLeftCellID; break;
                        case ELRUDState.Right:
                        //case ELRUDState.DownRight:
                            newCellID = Constants.CentreRightCellID; break;
                        case ELRUDState.Up:
                        case ELRUDState.Down:
                            newCellID = FindSquareGridCell(currentCellID, direction, allowWrapAround); break;
                    }
                    break;
                case Constants.BottomCentreCellID:
                    switch (direction)
                    {
                        case ELRUDState.Left:
                        //case ELRUDState.UpLeft:
                            newCellID = Constants.CentreLeftCellID; break;
                        case ELRUDState.Right:
                        //case ELRUDState.UpRight:
                            newCellID = Constants.CentreRightCellID; break;
                        case ELRUDState.Up:
                        case ELRUDState.Down:
                            newCellID = FindSquareGridCell(currentCellID, direction, allowWrapAround); break;
                    }
                    break;
            }

            return newCellID;
        }
    }
}
