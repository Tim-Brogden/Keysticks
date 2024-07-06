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

namespace Keysticks.UI
{
    // Routed event handlers
    public delegate void KxDoubleValRoutedEventHandler(object sender, KxDoubleValRoutedEventArgs args);
    public delegate void KxIntValRoutedEventHandler(object sender, KxIntValRoutedEventArgs args);
    public delegate void KxStateRoutedEventHandler(object sender, KxStateRoutedEventArgs args);
    public delegate void KxInputControlRoutedEventHandler(object sender, KxInputControlRoutedEventArgs args);
    public delegate void KxErrorRoutedEventHandler(object sender, KxErrorRoutedEventArgs args);
}
