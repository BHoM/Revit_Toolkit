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

        [Description("Creates elevation view referenced do elevation marker in the current Revit file.")]
        [Input("elevationMarker", "Elevation marker to which elevation view will be referenced.")]
        [Input("elevationName", "Name of the elevation view.")]
        [Input("referenceViewPlan", "ViewPlan that elevation marker and elevation views will be referenced to.")]
        [Input("elevationIndex", "The index on the Elevation Marker where the new elevation will be placed.")]
        [Input("bottomLine", "Location line of the elevation view and the bottom line for the crop region shape.")]
        [Input("depth", "Depth of the elevation view.")]
        [Input("height", "Height of the elevation view.")]
        [Input("offset", "Offset that will be added to the elevation view CropBox shape.")]
        [Input("viewTemplateId", "Optional, the View Template Id to be applied in the view.")]
        [Input("cropRegionVisible", "True if the crop region should be visible.")]
        [Input("annotationCrop", "True if the annotation crop should be visible.")]
        [Output("elevations", "The list of created elevations.")]
        public static ViewSection ElevationView(this ElevationMarker elevationMarker, string elevationName, View referenceViewPlan, int elevationIndex, Line bottomLine, double depth, double height, double offset, ElementId viewTemplateId = null, bool cropRegionVisible = false, bool annotationCrop = false)
        {
            Document doc = elevationMarker.Document;

            //create ViewSection
            ViewSection newElevation = elevationMarker.CreateElevation(doc, referenceViewPlan.Id, elevationIndex);
            newElevation.SetElevationProperties(elevationName, bottomLine, depth, height, offset, viewTemplateId, cropRegionVisible, annotationCrop);

            return newElevation;
        }

        /***************************************************/
    }
}



