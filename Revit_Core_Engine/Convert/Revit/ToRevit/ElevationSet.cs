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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts BH.oM.Adapters.Revit.Elements.ElevationSet to a Revit ElevationMarker.")]
        [Input("elevationSet", "BH.oM.Adapters.Revit.Elements.ElevationSet to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("m_ElevationMarker", "Revit Elevation Marker resulting from converting the input BH.oM.Adapters.Revit.Elements.ElevationSet.")]
        public static ElevationMarker ToRevitElevationSet(this oM.Adapters.Revit.Elements.ElevationSet elevationSet, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (elevationSet == null)
                return null;

            ElevationMarker revitViewPlan = refObjects.GetValue<ElevationMarker>(document, elevationSet.BHoM_Guid);
            if (revitViewPlan != null)
                return revitViewPlan;

            settings = settings.DefaultIfNull();

            var elevationMarkerLocation = elevationSet.MarkerLocation.ToRevit();
            var refViewPlan = document.GetElement(new ElementId(elevationSet.ReferenceViewPlanId)) as View;

            var revitElevationMarker = Create.ElevationMarker(document, elevationMarkerLocation, refViewPlan);

            //unfinished - need check if LeftElevation.Line is oriented to the left, if TopELevation.Line is oriented to the top etc...

            elevationSet.LeftElevation.ToRevit(document, revitElevationMarker, refViewPlan, 0, settings, refObjects);
            elevationSet.TopElevation.ToRevit(document, revitElevationMarker, refViewPlan, 1, settings, refObjects);
            elevationSet.RightElevation.ToRevit(document, revitElevationMarker, refViewPlan, 2, settings, refObjects);
            elevationSet.BottomElevation.ToRevit(document, revitElevationMarker, refViewPlan, 3, settings, refObjects);

            return revitElevationMarker;
        }

        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static ViewSection ToRevit(this oM.Adapters.Revit.Elements.ElevationView elevationView, Document document, ElevationMarker elevationMarker, View referenceViewPlan, int elevationIndex, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (elevationView == null)
                return null;

            ViewSection revitViewPlan = refObjects.GetValue<ViewSection>(document, elevationView.BHoM_Guid);
            if (revitViewPlan != null)
                return revitViewPlan;

            settings = settings.DefaultIfNull();

            var elevName = elevationView.Name;
            var elevLine = elevationView.BottomLine.ToRevit();
            double elevHeight = elevationView.Heigth.FromSI(SpecTypeId.Length);
            double elevDepth = elevationView.Depth.FromSI(SpecTypeId.Length);
            bool cropRegionVisible = elevationView.CropRegionVisible;
            bool annotationCropVisible = elevationView.AnnotationCropVisible;

            ElementId viewTemplateId = ElementId.InvalidElementId;

            if (!string.IsNullOrWhiteSpace(elevationView.TemplateName))
            {
                IEnumerable<View> viewPlans = new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>();
                View viewPlanTemplate = viewPlans.FirstOrDefault(x => x.IsTemplate && elevationView.TemplateName == x.Name);
                if (viewPlanTemplate == null)
                    Compute.ViewTemplateNotExistsWarning(elevationView);
                else
                    viewTemplateId = viewPlanTemplate.Id;
            }

            var revitElevationView = Create.ElevationView(document, elevName, elevationMarker, referenceViewPlan, elevationIndex, elevLine, elevDepth, elevHeight, viewTemplateId, cropRegionVisible, annotationCropVisible);

            // Copy parameters from BHoM object to Revit element
            revitViewPlan.CopyParameters(elevationView, settings);
            refObjects.AddOrReplace(elevationView, revitElevationView);

            return revitElevationView;
        }

        /***************************************************/

    }
}



