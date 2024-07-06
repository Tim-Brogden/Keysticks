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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Reflection;
using Keysticks.Core;

namespace Keysticks.UserControls
{
    /// <summary>
    /// Colour picker control
    /// </summary>
    public partial class ColourPickerControl : UserControl
    {
        // Fields
        private List<NamedItem> _colourItems;

        // Properties
        public string SelectedColour
        {
            get { return (string)GetValue(SelectedColourProperty); }
            set { SetValue(SelectedColourProperty, value); }
        }
        private static readonly DependencyProperty SelectedColourProperty =
            DependencyProperty.Register(
            "SelectedColour",
            typeof(string),
            typeof(ColourPickerControl),
            new FrameworkPropertyMetadata(SelectedColourPropertyChanged)
        );
        public List<NamedItem> ColourItems { get { return _colourItems; } }

        // Routed events
        public static readonly RoutedEvent KxSelectedColourChangedEvent = EventManager.RegisterRoutedEvent(
            "SelectedColourChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ColourPickerControl));
        public event RoutedEventHandler SelectedColourChanged
        {
            add { AddHandler(KxSelectedColourChangedEvent, value); }
            remove { RemoveHandler(KxSelectedColourChangedEvent, value); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ColourPickerControl()
        {
            PopulateColourList();
            
            InitializeComponent();

            ColoursCombo.DataContext = this;
        }

        /// <summary>
        /// Design mode changed
        /// </summary>
        private static void SelectedColourPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ColourPickerControl control = source as ColourPickerControl;
            if (control.IsLoaded)
            {
                control.RaiseEvent(new RoutedEventArgs(KxSelectedColourChangedEvent));
            }
        }

        /// <summary>
        /// Control loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Make sure a colour is selected
            if (SelectedColour == null && _colourItems.Count != 0)
            {
                SelectedColour = _colourItems[0].Name;
            }
        }

        /// <summary>
        /// Populate the list of colours
        /// </summary>
        private void PopulateColourList()
        {
            // Get standard colours
            _colourItems = new List<NamedItem>();
            foreach (PropertyInfo colourInfo in typeof(Colors).GetProperties())
            {
                if (colourInfo.Name != "Transparent")
                {
                    Color colour = (Color)ColorConverter.ConvertFromString(colourInfo.Name);
                    System.Drawing.Color sdc = System.Drawing.Color.FromArgb(colour.R, colour.G, colour.B);
                    int hue = (int)sdc.GetHue();    // 0 - 360
                    int brightness = (int)(255 * sdc.GetBrightness());
                    int id = (hue << 8) + brightness;
                    _colourItems.Add(new NamedItem(id, colourInfo.Name));
                }
            }

            // Sort by ID
            _colourItems.Sort((x, y) => x.ID.CompareTo(y.ID));
        }
    }
}
