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
using Keysticks.Config;

namespace Keysticks.Core
{
    /// <summary>
    /// Stores the grid bindings for a mode
    /// Bindings are stored as a list of (index, binding) pairs for each cell, 
    /// where index is the index of the annotation that is bound
    /// </summary>
    public class AnnotationBindingsLookup
    {
        // Fields
        private Dictionary<int, List<AnnotationBinding>> _cellBindingsTable;
        private List<GridBinding>[] _annotationBindingsTable;

        /// <summary>
        /// Constructor
        /// </summary>
        public AnnotationBindingsLookup(int numAnnotations)
        {
            _cellBindingsTable = new Dictionary<int, List<AnnotationBinding>>();
            _annotationBindingsTable = new List<GridBinding>[numAnnotations];
        }

        public Dictionary<int, List<AnnotationBinding>>.Enumerator GetCellBindingsEnumerator()
        {
            return _cellBindingsTable.GetEnumerator();
        }        

        /// <summary>
        /// Get the bindings for a particular cell
        /// </summary>
        /// <param name="cellID"></param>
        /// <returns></returns>
        public List<AnnotationBinding> GetBindingsForCell(int cellID)
        {
            List<AnnotationBinding> cellBindings = null;
            if (_cellBindingsTable.ContainsKey(cellID))
            {
                cellBindings = _cellBindingsTable[cellID];
            }

            return cellBindings;
        }

        /// <summary>
        /// Get the bindings for an annotation
        /// </summary>
        /// <param name="annotationIndex"></param>
        /// <returns></returns>
        public List<GridBinding> GetBindingsForAnnotation(int annotationIndex)
        {
            List<GridBinding> annotationBindings = null;
            if (annotationIndex > -1 && annotationIndex < _annotationBindingsTable.Length)
            {
                annotationBindings = _annotationBindingsTable[annotationIndex];
            }

            return annotationBindings;
        }

        /// <summary>
        /// Add a grid binding for a cell
        /// </summary>
        /// <param name="cellID"></param>
        /// <param name="binding"></param>
        public void AddBinding(int cellID, int annotationIndex, GridBinding binding)
        {
            // Get the bindings for this cell, creating if necessary
            List<AnnotationBinding> cellBindingsList = GetBindingsForCell(cellID);
            if (cellBindingsList == null)
            {
                cellBindingsList = new List<AnnotationBinding>();
                _cellBindingsTable[cellID] = cellBindingsList;
            }

            // Add the binding
            cellBindingsList.Add(new AnnotationBinding(annotationIndex, binding));

            // Get the bindings for this annotation, creating if necessary
            List<GridBinding> annotationBindingsList = GetBindingsForAnnotation(annotationIndex);
            if (annotationBindingsList == null)
            {
                annotationBindingsList = new List<GridBinding>();
                _annotationBindingsTable[annotationIndex] = annotationBindingsList;
            }

            // Add the binding
            annotationBindingsList.Add(binding);
        }
    }
}
