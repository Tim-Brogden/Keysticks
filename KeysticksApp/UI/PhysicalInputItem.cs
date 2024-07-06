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
using Keysticks.Core;
using Keysticks.Input;

namespace Keysticks.UI
{
    /// <summary>
    /// UI data for displaying a physical input
    /// </summary>
    public class PhysicalInputItem : CustomBindingItem
    {
        // Fields
        private NamedItemList _availableInputs;
        private NamedItem _selectedItem;

        // Properties
        public string FullName
        { 
            get 
            { 
                string fullName = ItemData.Name;
                PhysicalInput input = ItemData as PhysicalInput;
                if (input != null && input.Description != "")
                {
                    fullName += " - " + input.Description;
                }
                return fullName;
            }
        }
        public NamedItemList AvailableInputs { get { return _availableInputs; } }
        public NamedItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    NotifyPropertyChanged("SelectedItem");
                }
            }
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="itemData"></param>
        /// <param name="isDesignMode"></param>
        /// <param name="toolTipText"></param>
        /// <param name="parent"></param>
        /// <param name="availableInputs"></param>
        public PhysicalInputItem(PhysicalInput itemData,
                                        bool isDesignMode,
                                        bool isExpanded,
                                        string toolTipText, 
                                        CustomBindingItem parent,
                                        NamedItemList availableInputs)
            : base(itemData, isDesignMode, isExpanded, toolTipText, parent)
        {
            _availableInputs = availableInputs;
            if (availableInputs != null && availableInputs.Count != 0)
            {
                _selectedItem = availableInputs[0];
            }
        }
    }
}
