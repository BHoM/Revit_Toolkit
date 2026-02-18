/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using Autodesk.Revit.UI;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Zooms the active view to show the specified elements.")]
        [Input("uiDoc", "The UIDocument containing the view to zoom.")]
        [Input("elements", "List of elements to zoom to.")]
        [Input("view", "Optional view to set as active before zooming. If null, the current active view will be used.")]
        public static void ZoomToElements(this UIDocument uiDoc, List<Element> elements, View view = null)
        {
            if (uiDoc == null || elements == null || !elements.Any())
                return;

            List<BoundingBoxXYZ> bboxes = elements.Select(x => x.BoundingBox()).Where(x => x != null).ToList();
            BoundingBoxXYZ bbox = bboxes.Bounds();
            bbox.Inflate(4);

            // If a specific view is provided, set it as active before zooming
            if (view != null)
                uiDoc.ActiveView = view;

            UIView uiView = uiDoc?.GetOpenUIViews()?.FirstOrDefault(x => x.ViewId.Equals(uiDoc.ActiveView.Id));
            if (uiView == null)
                return;

            uiView.ZoomAndCenterRectangle(bbox.Min, bbox.Max);
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static BoundingBoxXYZ BoundingBox(this Element element)
        {
            BoundingBoxXYZ bbox = element.get_BoundingBox(null);
            if (bbox == null)
                return null;

            if (element.Document.IsLinked)
            {
                RevitLinkInstance linkInstance = element.Document.LinkInstance();
                Transform linkTransform = linkInstance.GetTotalTransform() ?? Transform.Identity;
                bbox = bbox.BoundsOfTransformed(linkTransform);
            }

            return bbox;
        }

        /***************************************************/
    }
}