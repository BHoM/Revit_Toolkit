/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates the callout view visible on the referenced view.")]
        [Input("referenceView", "View on which the callout will be visible. Callouts can be created on view plans, sections and detail views.")]
        [Input("boundingBoxXyz", "Bounds of the callout.")]
        [Input("calloutName", "Name of the callout view. If null, default name will be set.")]
        [Input("viewFamily", "ViewFamily to be used by the callout view. Detail view family set as default can be used in all views except CeilingPlans and DraftingViews (in which it is recommended to use the same view family as the reference view).")]
        [Input("calloutTemplateId", "Id of the template to be applied to the callout view. If null, no template will be set.")]
        [Output("callout", "New callout view.")]
        public static View Callout(this View referenceView, BoundingBoxXYZ boundingBoxXyz, string calloutName = null, ViewFamily viewFamily = ViewFamily.Detail, ElementId calloutTemplateId = null)
        {
            if (referenceView == null)
            {
                BH.Engine.Base.Compute.RecordError("Could not create a callout view based on null reference view.");
                return null;
            }
            else if (boundingBoxXyz == null)
            {
                BH.Engine.Base.Compute.RecordError("Could not create a callout view based on a null bounding box.");
                return null;
            }

            Document document = referenceView.Document;
            ViewFamilyType viewFamilyType = Query.ViewFamilyType(document, viewFamily);

            View callout = Autodesk.Revit.DB.ViewSection.CreateCallout(document, referenceView.Id, viewFamilyType.Id, boundingBoxXyz.Min, boundingBoxXyz.Max);

            callout.SetViewTemplate(calloutTemplateId);

            if (!string.IsNullOrEmpty(calloutName))
            {
                callout.SetViewName(calloutName);
            }

            return callout;
        }

        /***************************************************/

    }
}



