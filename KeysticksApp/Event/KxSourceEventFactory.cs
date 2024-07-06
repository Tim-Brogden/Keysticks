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
using System.Collections.Generic;

namespace Keysticks.Event
{
    /// <summary>
    /// Factory class for input source event data objects
    /// </summary>
    public class KxSourceEventFactory
    {
        // Fields
        private List<KxSourceEventArgs> _eventList;
        private KxSourceEventArgs _templateEvent;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="templateEvent"></param>
        public KxSourceEventFactory(KxSourceEventArgs templateEvent)
        {
            _templateEvent = templateEvent;
            _eventList = new List<KxSourceEventArgs>();
            _eventList.Add(new KxSourceEventArgs(templateEvent));
        }

        /// <summary>
        /// Return an input event
        /// </summary>
        /// <returns></returns>
        public KxSourceEventArgs Create()
        {
            KxSourceEventArgs ev;

            // Try to find an event that isn't in use
            int i = 0;
            do
            {
                ev = _eventList[i];
            }
            while (ev.InUse && ++i < _eventList.Count);

            // See if an event was found
            if (ev.InUse)
            {
                // None found so add a new event
                ev = new KxSourceEventArgs(_templateEvent);
                _eventList.Add(ev);

                //Trace.WriteLine(string.Format("Factory: {0}", _eventList.Count));
            }            

            // Record that this event is being used
            ev.InUse = true;

            return ev;
        }
    }
}
