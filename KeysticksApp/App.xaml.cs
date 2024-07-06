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
using System.Threading;

namespace Keysticks
{
    /// <summary>
    /// Application class
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _singleInstanceMutex = new Mutex(true, "{7A48C92E-57FB-11E1-92EA-A13F4824019B}");
        private bool _isFirstInstance = false;

        public bool IsFirstInstance { get { return _isFirstInstance; } }
        
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (_singleInstanceMutex.WaitOne(TimeSpan.Zero, true))
            {
                _isFirstInstance = true;
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (_isFirstInstance)
            {
                _singleInstanceMutex.ReleaseMutex();
            }
        }
    }
}
