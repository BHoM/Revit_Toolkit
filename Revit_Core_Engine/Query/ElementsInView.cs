/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using Autodesk.Revit.DB.Analysis;
using BH.Engine.Adapters.Revit;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns an element from a Revit document that matches a given Revit unique Id and optionally link unique Id.")]
        [Input("document", "Revit document to search for the element.")]
        [Input("uniqueId", "Revit unique Id to find a matching element for.")]
        [Input("linkUniqueId", "Revit unique Id of a Revit link instance. If not null, the link instance under this unique Id will be searched instead of the host document.")]
        [Output("element", "Revit element matching the input Revit unique Ids.")]
        public static List<Element> ElementsInView(this ViewPlan view, List<Document> documents, List<ElementFilter> elementFilters)
        {

            //view bbox filter
            BoundingBoxXYZ viewBbox = view.ViewBBox();
            Outline viewOutline = new Outline(viewBbox.Min, viewBbox.Max);
            BoundingBoxIsInsideFilter bboxInsideFilter = new BoundingBoxIsInsideFilter(viewOutline);
            BoundingBoxIntersectsFilter bboxIntersectFilter = new BoundingBoxIntersectsFilter(viewOutline);
            LogicalOrFilter bboxFilter = new LogicalOrFilter(bboxInsideFilter, bboxIntersectFilter);

            LogicalOrFilter elementFilter = new LogicalOrFilter(elementFilters);

            List<Element> elemets = new List<Element>();
            foreach (Document doc in documents)
            {
                IList<Element> filteredElements = new FilteredElementCollector(doc).WherePasses(new LogicalAndFilter(elementFilter, bboxFilter)).WhereElementIsNotElementType().ToElements();
                elemets.AddRange(filteredElements);
            }

            return elemets;

        }

        /***************************************************/

        [Description("Bounding Box of the View Plan.")]
        [Input("view", "ViewPlan to get the bounding box from.")]
        [Output("bbox", "Bounding Box of the view.")]
        private static BoundingBoxXYZ ViewBBox(this ViewPlan view)
        {
            //bbox of current view
            Document doc = view.Document;
            PlanViewRange viewRange = view.GetViewRange();
            BoundingBoxXYZ viewCropBox = view.CropBox;
            Transform viewTransform = viewCropBox.Transform;

            //topElevation (from the CutPlane)
            Level topLevel = doc.GetElement(viewRange.GetLevelId(PlanViewPlane.CutPlane)) as Level;
            double topElevation;
            if (topLevel == null)
            {
                topElevation = 1e6;
            }
            else
            {
                double topOffset = viewRange.GetOffset(PlanViewPlane.CutPlane);
                topElevation = topLevel.ProjectElevation + topOffset;
            }

            //bottomElevation
            Level bottomLevel = doc.GetElement(viewRange.GetLevelId(PlanViewPlane.BottomClipPlane)) as Level;
            double bottomElevation;
            if (bottomLevel == null)
            {
                bottomElevation = -1e6;
            }
            else
            {
                double bottomOffset = viewRange.GetOffset(PlanViewPlane.BottomClipPlane);
                bottomElevation = bottomLevel.ProjectElevation + bottomOffset;
            }

            BoundingBoxXYZ viewBbox = new BoundingBoxXYZ();
            viewBbox.Min = new XYZ(viewCropBox.Min.X, viewCropBox.Min.Y, bottomElevation);
            viewBbox.Max = new XYZ(viewCropBox.Max.X, viewCropBox.Max.Y, topElevation);
            viewBbox.Transform = viewTransform;

            return viewBbox;
        }
    }
}