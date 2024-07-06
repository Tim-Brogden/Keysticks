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

namespace Keysticks.UserControls
{
    /// <summary>
    /// QWERTY-style keyboard layout control
    /// </summary>
    public partial class KeyboardControl : BaseGridControl
    {            
        /// <summary>
        /// Constructor
        /// </summary>
        public KeyboardControl()
            : base()
        {
            InitializeComponent();

            // Create array of annotations
            ControlAnnotationControl[] annotationControls = new ControlAnnotationControl[] 
            {
                Escape, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, Insert, PrintScreen, PageUp,
                Backtick, D1, D2, D3, D4, D5, D6, D7, D8, D9, D0, MinusSign, EqualsSign, Backspace, Home, End,
                Tab, Q, W, E, R, T, Y, U, I, O, P, LeftBracket, RightBracket, Hash, Delete, PageDown,
                CapsLock, A, S, D, F, G, H, J, K, L, Semicolon, Apostrophe, Return, Up,
                LShiftKey, Backslash, Z, X, C, V, B, N, M, Comma, Fullstop, Slash, RShiftKey, Left, Right,
                LControlKey, LWin, LMenu, Spacebar, RMenu, RWin, Apps, RControlKey, Down
            };
            SetAnnotationControls(annotationControls);
        }

        /// <summary>
        /// Acquire the focus
        /// </summary>
        public override void SetFocus()
        {
            OuterPanel.Focus();
        }
    }
}
