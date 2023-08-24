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
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates the callout visible in the referenced view plan.")]
        [Input("referenceViewPlan", "View plan on which the callout will be visible.")]
        [Input("boundingBoxXyz", "Bounds of the callout.")]
        [Input("calloutName", "Name of the callout view. If null, default name will be set.")]
        [Input("calloutTemplateId", "Id of the template to be applied to the callout view. If null, no template will be set.")]
        [Output("callout", "New callout view.")]
        public static View Callout(this ViewPlan referenceViewPlan, BoundingBoxXYZ boundingBoxXyz, string calloutName = null, ElementId calloutTemplateId = null)
        {
            if (referenceViewPlan == null)
            {
                BH.Engine.Base.Compute.RecordError("Could not create a callout view based on null reference view plan.");
                return null;
            }
            else if (boundingBoxXyz == null)
            {
                BH.Engine.Base.Compute.RecordError("Could not create a callout view based on a null boudning box.");
                return null;
            }

            Document document = referenceViewPlan.Document;
            ViewFamilyType detailViewFamilyType = Query.ViewFamilyType(document, ViewFamily.Detail);

            View callout = Autodesk.Revit.DB.ViewSection.CreateCallout(document, referenceViewPlan.Id, detailViewFamilyType.Id, boundingBoxXyz.Min, boundingBoxXyz.Max);

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

