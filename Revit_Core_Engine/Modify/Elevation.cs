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
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Modify existing elevation view.")]
        [Input("elevation", "Elevation view to be modified.")]
        [Input("elevationName", "Name of the elevation view.")]
        [Input("elevationLine", "Location line of the elevation view and the bottom line for the crop region shape.")]
        [Input("depth", "Depth of the elevation view.")]
        [Input("height", "Height of the elevation view.")]
        [Input("offset", "Offset that will be added to the elevation view CropBox shape.")]
        [Input("viewTemplateId", "Optional, the View Template Id to be applied in the view.")]
        [Input("cropRegionVisible", "True if the crop region should be visible.")]
        [Input("annotationCrop", "True if the annotation crop should be visible.")]
        [Output("elevationView", "Elevation view that has been modified.")]
        public static ViewSection ModifyElevation(this ViewSection elevation, string elevationName, Line bottomLine, double depth, double height, double offset, ElementId viewTemplateId = null, bool cropRegionVisible = false, bool annotationCrop = false)
        {
            Document doc = elevation.Document;

            //set name
            elevation.SetViewName(elevationName);

            //set template
            if (viewTemplateId != null && viewTemplateId.IntegerValue != -1)
            {
                if (!(doc.GetElement(viewTemplateId) as View).IsTemplate)
                {
                    BH.Engine.Base.Compute.RecordWarning($"Could not apply the View Template of Id '{viewTemplateId}'. Please check if it's a valid View Template.");
                    return null;
                }

                try
                {
                    elevation.ViewTemplateId = viewTemplateId;
                }
                catch (Exception)
                {
                    BH.Engine.Base.Compute.RecordWarning($"Could not apply the View Template of Id '{viewTemplateId}'. Please check if it's a valid ElementId.");
                }
            }

            //set crop box shape
            var curveLoop = ElevationCropRegionShape(bottomLine, height, offset);
            elevation.GetCropRegionShapeManager().SetCropShape(curveLoop);

            //set depth
            elevation.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR).Set(depth);

            //set crop region visibility
            if (!elevation.get_Parameter(BuiltInParameter.VIEWER_CROP_REGION_VISIBLE).Set(cropRegionVisible ? 1 : 0))
                BH.Engine.Base.Compute.RecordWarning($"Could not set the crop region visibility in the view. Parameter is ready-only.");

            //set annotation crop visibility
            if (!elevation.get_Parameter(BuiltInParameter.VIEWER_ANNOTATION_CROP_ACTIVE).Set(annotationCrop ? 1 : 0))
                BH.Engine.Base.Compute.RecordWarning($"Could not set the annotation crop in the view. Parameter is ready-only.");

            return elevation;

        }

        /***************************************************/
        /****             Private methods               ****/
        /***************************************************/

        [Description("Get the shape of the elevation crop region based on given line and increased by the offset.")]
        private static CurveLoop ElevationCropRegionShape(Line line, double height, double offset)
        {
            var pt1 = line.GetEndPoint(0);
            var pt4 = line.GetEndPoint(1);
            var pt2 = pt1.Add(new XYZ(0, 0, height));
            var pt3 = pt4.Add(new XYZ(0, 0, height));

            var lines = new List<Curve>
            {
                Line.CreateBound(pt1, pt2),
                Line.CreateBound(pt2, pt3),
                Line.CreateBound(pt3, pt4),
                Line.CreateBound(pt4, pt1)
            };

            var curveLoop = CurveLoop.Create(lines);

            return CurveLoop.CreateViaOffset(curveLoop, offset, curveLoop.GetPlane().Normal);
        }

        /***************************************************/
    }
}

