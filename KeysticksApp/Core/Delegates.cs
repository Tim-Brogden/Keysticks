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
using Keysticks.Event;

namespace Keysticks.Core
{
    public delegate void KxStateChangeEventHandler(object sender, KxStateChangeEventArgs args);
    public delegate void KxInputEventHandler(object sender, KxControlEventArgs args);
    public delegate void KxEventReportHandler(object sender, KxEventArgs report);
    public delegate void WebServiceEventHandler(WebServiceMessageData message);
    //public delegate ParamValue ParamValueGetter(IStateManager stateManager, ParameterSourceArgs paramSource);
}
