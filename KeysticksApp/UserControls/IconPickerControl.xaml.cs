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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Keysticks.Core;
using Keysticks.UI;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Icon picker control
    /// </summary>
    public partial class IconPickerControl : UserControl
    {
        // Fields
        private bool _isLoaded = false;
        private NamedItemList _displayableIconList = new NamedItemList();
        private EAnnotationImage _selectedIcon = EAnnotationImage.None;

        // Properties
        public EAnnotationImage SelectedIcon
        {
            get
            {
                return _selectedIcon;
            }
            set
            {                
                _selectedIcon = value;
                if (_isLoaded)
                {
                    this.IconsCombo.SelectedValue = (int)_selectedIcon;
                }
            }
        }

        // Events
        public event EventHandler SelectionChanged;

        /// <summary>
        /// Set the list of icons to display
        /// </summary>
        /// <param name="iconsList"></param>
        public void SetIconList(List<EAnnotationImage> iconsList)
        {
            _displayableIconList.Clear();
            if (iconsList != null)
            {
                StringUtils utils = new StringUtils();
                foreach (EAnnotationImage icon in iconsList)
                {
                    IconItem displayItem = new IconItem((int)icon, icon.ToString(), GUIUtils.FindIcon(icon));
                    _displayableIconList.Add(displayItem);
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public IconPickerControl()
        {
            InitializeComponent();

            IconsCombo.ItemsSource = _displayableIconList;
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;
            IconsCombo.SelectedValue = (int)_selectedIcon;
        }

        /// <summary>
        /// Selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IconsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NamedItem iconItem = (NamedItem)this.IconsCombo.SelectedItem;
            _selectedIcon = (iconItem != null) ? (EAnnotationImage)iconItem.ID : EAnnotationImage.None;

            if (SelectionChanged != null)
            {
                SelectionChanged(this, new EventArgs());
            }
        }
    }

    /// <summary>
    /// Displayable icon item
    /// </summary>
    public class IconItem : NamedItem
    {
        // Fields
        private BitmapImage _image;

        // Properties
        public BitmapImage Image { get { return _image; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="image"></param>
        public IconItem(int id, string name, BitmapImage image)
        :base(id, name)
        {
            _image = image;
        }
    }
}
