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

namespace Keysticks.UI
{
    /// <summary>
    /// Data object to bind to a visual element
    /// </summary>
    public class CustomBindingItem : NamedItem
    {
        // Fields
        private NamedItem _itemData;
        private bool _isSelected;
        private bool _isHighlighted;
        private bool _isExpanded;
        private bool _isDesignMode;
        private bool _isBeingRenamed;
        private string _toolTipText;
        private CustomBindingItem _parent;
        private NamedItemList _children = new NamedItemList();

        // Properties
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set
            {
                if (_isHighlighted != value)
                {
                    _isHighlighted = value;
                    NotifyPropertyChanged("IsHighlighted");
                }
            }
        }
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    NotifyPropertyChanged("IsExpanded");

                    // Expand recursively up the tree if necessary
                    if (_isExpanded && _parent != null)
                    {
                        _parent.IsExpanded = true;
                    }
                }
            }
        }
        public bool IsDesignMode
        {
            get { return _isDesignMode; }
            set
            {
                if (_isDesignMode != value)
                {
                    _isDesignMode = value;
                    NotifyPropertyChanged("IsDesignMode");
                }
            }
        }
        public bool IsBeingRenamed
        {
            get { return _isBeingRenamed; }
            set
            {
                if (_isBeingRenamed != value)
                {
                    _isBeingRenamed = value;
                    NotifyPropertyChanged("IsBeingRenamed");
                }
            }
        }
        public NamedItem ItemData 
        { 
            get { return _itemData; } 
            set
            {
                if (_itemData != value)
                {
                    _itemData = value;
                    NotifyPropertyChanged("ItemData");
                }
            } 
        }
        public string ToolTipText { get { return _toolTipText; } }
        public CustomBindingItem Parent { get { return _parent; } }
        public NamedItemList Children { get { return _children; } }   

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public CustomBindingItem(NamedItem itemData, bool isDesignMode, bool isExpanded, string toolTipText, CustomBindingItem parent)     
            :base(itemData)
        {
            _itemData = itemData;
            _isDesignMode = isDesignMode;
            _isExpanded = isExpanded;
            _toolTipText = toolTipText;
            _parent = parent;
        }
    }
}
