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
using System.Windows;
using System.Windows.Controls;
using Keysticks.UI;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Base class for custom user controls
    /// </summary>
    public class KxUserControl : UserControl
    {
        // Custom routed events
        public static readonly RoutedEvent KxStateChangedEvent = EventManager.RegisterRoutedEvent(
            "StateChanged", RoutingStrategy.Bubble, typeof(KxStateRoutedEventHandler), typeof(KxUserControl));
        public static readonly RoutedEvent KxStatesEditedEvent = EventManager.RegisterRoutedEvent(
            "StatesEdited", RoutingStrategy.Bubble, typeof(KxStateRoutedEventHandler), typeof(KxUserControl));
        public static readonly RoutedEvent KxInputControlChangedEvent = EventManager.RegisterRoutedEvent(
            "InputControlChanged", RoutingStrategy.Bubble, typeof(KxInputControlRoutedEventHandler), typeof(KxUserControl));
        public static readonly RoutedEvent KxQuickEditActionsEvent = EventManager.RegisterRoutedEvent(
            "QuickEditActions", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(KxUserControl));
        public static readonly RoutedEvent KxEditActionsEvent = EventManager.RegisterRoutedEvent(
            "EditActions", RoutingStrategy.Bubble, typeof(KxStateRoutedEventHandler), typeof(KxUserControl));
        public static readonly RoutedEvent KxActionsEditedEvent = EventManager.RegisterRoutedEvent(
            "ActionsEdited", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(KxUserControl));
        public static readonly RoutedEvent KxErrorEvent = EventManager.RegisterRoutedEvent(
            "KxError", RoutingStrategy.Bubble, typeof(KxErrorRoutedEventHandler), typeof(KxUserControl));

        // Routed events
        public event KxStateRoutedEventHandler StateChanged
        {
            add { AddHandler(KxStateChangedEvent, value); }
            remove { RemoveHandler(KxStateChangedEvent, value); }
        }
        public event KxStateRoutedEventHandler StatesEdited
        {
            add { AddHandler(KxStatesEditedEvent, value); }
            remove { RemoveHandler(KxStatesEditedEvent, value); }
        }
        public event KxInputControlRoutedEventHandler InputControlChanged
        {
            add { AddHandler(KxInputControlChangedEvent, value); }
            remove { RemoveHandler(KxInputControlChangedEvent, value); }
        }
        public event RoutedEventHandler QuickEditActions
        {
            add { AddHandler(KxQuickEditActionsEvent, value); }
            remove { RemoveHandler(KxQuickEditActionsEvent, value); }
        }
        public event KxStateRoutedEventHandler EditActions
        {
            add { AddHandler(KxEditActionsEvent, value); }
            remove { RemoveHandler(KxEditActionsEvent, value); }
        }
        public event RoutedEventHandler ActionsEdited
        {
            add { AddHandler(KxActionsEditedEvent, value); }
            remove { RemoveHandler(KxActionsEditedEvent, value); }
        }
        public event KxErrorRoutedEventHandler KxError
        {
            add { AddHandler(KxErrorEvent, value); }
            remove { RemoveHandler(KxErrorEvent, value); }
        }
    }
}
