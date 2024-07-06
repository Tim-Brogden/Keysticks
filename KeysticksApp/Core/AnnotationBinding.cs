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
using Keysticks.Config;

namespace Keysticks.Core
{
    /// <summary>
    /// Stores a single keyboard cell binding
    /// </summary>
    public class AnnotationBinding
    {
        // Fields
        private int _annotationIndex;
        private GridBinding _binding;

        // Properties
        public int AnnotationIndex { get { return _annotationIndex; } }
        public GridBinding Binding { get { return _binding; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="annotationIndex"></param>
        /// <param name="binding"></param>
        public AnnotationBinding(int annotationIndex, GridBinding binding)
        {
            _annotationIndex = annotationIndex;
            _binding = binding;
        }
    }
}
