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

        [Description("Creates and returns a new Plan view in the current Revit file.")]
        [Input("document", "Revit current document to be processed.")]
        [Input("level", "The level that the created Floor Plan refers to.")]
        [Input("viewName", "Name of the new view.")]
        [Input("viewFamily", "View family type of the new view.")]
        [Input("cropBox", "Optional, the crop region to attempt to apply to the newly created view.")]
        [Input("viewTemplateId", "Optional, the View Template Id to be applied in the view.")]
        [Input("viewDetailLevel", "Optional, the Detail Level of the view.")]
        [Input("cropRegionVisible", "True if the crop region should be visible.")]
        [Input("annotationCrop", "True if the annotation crop should be visible.")]
        [Output("newView", "The new view.")]
        public static List<ViewSection> ViewElevations(this Document document, string elevationName, XYZ elevationMarkerLocation, ViewPlan referenceViewPlan, int numberOfElevations = 4, ElementId viewTemplateId = null, bool cropRegionVisible = false, bool annotationCrop = false)
        {
            if (numberOfElevations < 1 || numberOfElevations > 4)
            {
                BH.Engine.Base.Compute.RecordWarning($"Number of elevation views should be between 1 and 4.");
                return null;
            }

            var vft = Core.Query.ViewFamilyType(document, ViewFamily.Elevation);
            var elevationMarker = ElevationMarker.CreateElevationMarker(document, vft.Id, elevationMarkerLocation, referenceViewPlan.Scale);

            var elevations = new List<ViewSection>();

            for (int i = 0; i < numberOfElevations; i++)
            {
                ViewSection newElevation = elevationMarker.CreateElevation(document, referenceViewPlan.Id, i);
                newElevation.SetViewName(elevationName + " - " + (i + 1).ToString(), document);

                if (viewTemplateId != null)
                {
                    if (!(document.GetElement(viewTemplateId) as View).IsTemplate)
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

                if (!newElevation.get_Parameter(BuiltInParameter.VIEWER_CROP_REGION_VISIBLE).Set(cropRegionVisible ? 1 : 0))
                    BH.Engine.Base.Compute.RecordWarning($"Could not set the crop region visibility in the view. Parameter is ready-only.");

                if (!newElevation.get_Parameter(BuiltInParameter.VIEWER_ANNOTATION_CROP_ACTIVE).Set(annotationCrop ? 1 : 0))
                    BH.Engine.Base.Compute.RecordWarning($"Could not set the annotation crop in the view. Parameter is ready-only.");

                elevations.Add(newElevation);
            }

            return elevations;

        }

        /***************************************************/

    }
}



