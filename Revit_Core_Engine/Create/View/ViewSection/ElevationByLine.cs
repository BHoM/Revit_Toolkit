/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using Autodesk.Revit.DB;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Creates a single Elevation view based on given line. Elevation marker will be placed in the middle of the line.")]
        [Input("elevationLine", "Location line of the elevation view and the bottom line for the crop region shape.")]
        [Input("elevationName", "Name of the elevation view.")]
        [Input("referenceViewPlan", "ViewPlan that elevation marker is referenced to.")]
        [Input("depth", "Depth of the elevation view.")]
        [Input("height", "Height of the elevation view.")]
        [Input("offset", "Offset that will be added to the elevation view CropBox shape.")]
        [Input("elevationMarker", "Elevation marker that has been created (out parameter).")]
        [Input("viewTemplateId", "Optional, the View Template Id to be applied in the view.")]
        [Input("cropRegionVisible", "True if the crop region should be visible.")]
        [Input("annotationCrop", "True if the annotation crop should be visible.")]
        [Output("elevationView", "Elevation view that has been created.")]
        public static ViewSection ElevationByLine(this Line elevationLine, string elevationName, View referenceViewPlan, double depth, double height, double offset, ElementId viewTemplateId = null, bool cropRegionVisible = false, bool annotationCrop = false)
        {
            Document doc = referenceViewPlan.Document;

            var elevationMarkerLocation = elevationLine.Evaluate(0.5, true);
            var lineDirection = elevationLine.Direction.Normalize();

            if (lineDirection.DotProduct(XYZ.BasisZ) < Tolerance.Angle)
            {
                //warning and make horizontal
            }

            var markerDirection = lineDirection.VectorFromRevit().Rotate(-Math.PI / 2, BH.oM.Geometry.Vector.ZAxis).ToRevit();
            var angle = -markerDirection.AngleTo(XYZ.BasisX);

            if (Math.Abs(angle) < Tolerance.Angle)
                angle = 0;

            var axis = Line.CreateUnbound(elevationMarkerLocation, XYZ.BasisZ);

            var elevationMarker = ElevationMarker(elevationMarkerLocation, referenceViewPlan);
            var elevationView = elevationMarker.CreateElevation(doc, referenceViewPlan.Id, 0);

            ElementTransformUtils.RotateElement(doc, elevationMarker.Id, axis, angle);

            elevationView.SetElevationProperties(elevationName, elevationLine, depth, height, offset, viewTemplateId, cropRegionVisible, annotationCrop);

            return elevationView;
        }

        /***************************************************/
    }
}



