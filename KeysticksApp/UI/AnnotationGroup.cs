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
using Keysticks.UserControls;

namespace Keysticks.UI
{
    /// <summary>
    /// Stores the annotations for displaying a direction control or button group
    /// </summary>
    public class AnnotationGroup
    {
        // Fields
        private List<ControlAnnotationControl> _annotations;
        private ControlAnnotationControl _centreAnnotation;

        // Properties
        public List<ControlAnnotationControl> Annotations { get { return _annotations; } }
        public ControlAnnotationControl CentreAnnotation { get { return _centreAnnotation; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="annotations"></param>
        /// <param name="centreAnnotation"></param>
        public AnnotationGroup(List<ControlAnnotationControl> annotations, ControlAnnotationControl centreAnnotation)
        {
            _annotations = annotations;
            _centreAnnotation = centreAnnotation;

            // Mark the annotations as members of this group
            foreach (ControlAnnotationControl annotation in annotations)
            {
                annotation.Group = this;
            }
        }
    }
}
