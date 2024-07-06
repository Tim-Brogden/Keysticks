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
using Keysticks.Core;
using Keysticks.Input;

namespace Keysticks.UI
{
    /// <summary>
    /// UI data for displaying a physical control
    /// </summary>
    public class PhysicalControlItem : CustomBindingItem
    {
        // Fields
        private Visibility _deadZoneVisibility = Visibility.Collapsed;
        private double _deadZone;

        // Properties
        public Visibility DeadZoneVisibility { get { return _deadZoneVisibility; } }
        public double DeadZone
        {
            get { return _deadZone; }
            set
            {
                if (_deadZone != value && value >= 0.0 && value <= 1.0)
                {
                    _deadZone = value;
                    NotifyPropertyChanged("DeadZone");
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="isDesignMode"
        /// <param name="toolTipText"></param>
        /// <param name="tag"></param>
        /// <param name="parent"></param>
        /// <param name="controlType"></param>
        /// <param name="deadZone"></param>
        public PhysicalControlItem(PhysicalControl control, bool isDesignMode, string toolTipText, CustomBindingItem parent)
            :base(control, isDesignMode, true, toolTipText, parent)
        {
            _deadZone = Math.Round(control.DeadZone, 3);
            if (control.ControlType == EPhysicalControlType.Axis ||
                control.ControlType == EPhysicalControlType.Slider)
            {
                _deadZoneVisibility = Visibility.Visible;
            }
        }
    }
}
