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
using System;
using System.ComponentModel;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.UI;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Graphics;
using ViewPlan = Autodesk.Revit.DB.ViewPlan;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Creates elevation marker in the current Revit file.")]
        [Input("document", "Revit current document to be processed.")]
        [Input("elevationMarkerLocation", "Location point of the elevation marker to be placed.")]
        [Input("referenceViewPlan", "ViewPlan that elevation marker is referenced to.")]
        [Output("elevationMarker", "Elevation Marker that has been created.")]
        public static ElevationMarker ElevationMarker(this XYZ elevationMarkerLocation, View referenceViewPlan)
        {
            Document doc = referenceViewPlan.Document;
            var vft = Query.ViewFamilyType(doc, ViewFamily.Elevation);
            var elevationMarker = Autodesk.Revit.DB.ElevationMarker.CreateElevationMarker(doc, vft.Id, elevationMarkerLocation, referenceViewPlan.Scale);

            return elevationMarker;
        }

        /***************************************************/

        [Description("Creates elevation views referenced do elevation marker in the current Revit file.")]
        [Input("elevationMarker", "Elevation marker to which elevation views will be referenced.")]
        [Input("elevationName", "Name of the elevation view. Elevation index will be added to the value (from 1 to 4).")]
        [Input("referenceViewPlan", "ViewPlan that elevation marker and elevation views will be referenced to.")]
        [Input("elevationIndex", "The index on the Elevation Marker where the new elevation will be placed.")]
        [Input("bottomLine", "Location line of the elevation view and the bottom line for the crop region shape.")]
        [Input("depth", "Depth of the elevation view.")]
        [Input("height", "Height of the elevation view.")]
        [Input("viewTemplateId", "Optional, the View Template Id to be applied in the view.")]
        [Input("cropRegionVisible", "True if the crop region should be visible.")]
        [Input("annotationCrop", "True if the annotation crop should be visible.")]
        [Output("elevations", "The list of created elevations.")]
        public static ViewSection ElevationView(this ElevationMarker elevationMarker, string elevationName, View referenceViewPlan, int elevationIndex, Line bottomLine, double depth, double height, double offset, ElementId viewTemplateId = null, bool cropRegionVisible = false, bool annotationCrop = false)
        {
            Document doc = elevationMarker.Document;

            //create ViewSection
            ViewSection newElevation = elevationMarker.CreateElevation(doc, referenceViewPlan.Id, elevationIndex);

            //set name
            newElevation.SetViewName(elevationName + " " + (elevationIndex + 1).ToString(), doc);

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
                    newElevation.ViewTemplateId = viewTemplateId;
                }
                catch (Exception)
                {
                    BH.Engine.Base.Compute.RecordWarning($"Could not apply the View Template of Id '{viewTemplateId}'. Please check if it's a valid ElementId.");
                }
            }

            //set crop box shape
            var curveLoop = ElevationCropRegionShape(bottomLine, height, offset);
            newElevation.GetCropRegionShapeManager().SetCropShape(curveLoop);

            //set depth
            newElevation.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR).Set(depth);

            //set crop region visibility
            if (!newElevation.get_Parameter(BuiltInParameter.VIEWER_CROP_REGION_VISIBLE).Set(cropRegionVisible ? 1 : 0))
                BH.Engine.Base.Compute.RecordWarning($"Could not set the crop region visibility in the view. Parameter is ready-only.");

            //set annotation crop visibility
            if (!newElevation.get_Parameter(BuiltInParameter.VIEWER_ANNOTATION_CROP_ACTIVE).Set(annotationCrop ? 1 : 0))
                BH.Engine.Base.Compute.RecordWarning($"Could not set the annotation crop in the view. Parameter is ready-only.");

            return newElevation;

        }

        /***************************************************/

        [Description("Get the shape of the elevation crop region aligned to the given line.")]
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



