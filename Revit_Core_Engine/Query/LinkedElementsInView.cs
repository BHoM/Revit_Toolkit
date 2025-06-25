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
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Return elements from the revit link instance located in the view scope (including hidden elements)")]
        [Input("view", "View to get visible elements from. The view needs to belong to the host document.")]
        [Input("linkInstance", "Revit link instance to get the elements from.")]
        [Input("elementFilters", "Additional filters for the element collector. If null, no additional filters will be applied.")]
        [Output("elements", "Elements of link instance in given view.")]
        public static IEnumerable<ElementId> LinkedElementsInView(this View view, RevitLinkInstance linkInstance, List<ElementFilter> elementFilters = null)
        {
            if (view == null || linkInstance == null)
                return null;

            if (!(view is ViewPlan || view is View3D || view is ViewSection))
            {
                BH.Engine.Base.Compute.RecordWarning("Cannot get linked elements because provided view is not a ViewPlan, View3D, or ViewSection.");
                return new List<ElementId>();
            }

            Document linkDoc = linkInstance.GetLinkDocument();
            Solid viewSolid = view.TransformedViewSolid(linkInstance);
            ElementIntersectsSolidFilter solidFilter = new ElementIntersectsSolidFilter(viewSolid);
            
            IEnumerable<ElementId> elementIds; 
            if (elementFilters == null)
                elementIds = new FilteredElementCollector(linkDoc).WherePasses(solidFilter).WhereElementIsNotElementType().ToElementIds();
            else
            {
                LogicalOrFilter elementFilter = new LogicalOrFilter(elementFilters);
                LogicalAndFilter elementSolidFilter = new LogicalAndFilter(elementFilter, solidFilter);
                elementIds = new FilteredElementCollector(linkDoc).WherePasses(elementSolidFilter).WhereElementIsNotElementType().ToElementIds();
            }

            return elementIds;
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        [Description("Return view solid transformed by inverse link instnace transform.")]
        private static Solid TransformedViewSolid(this View view, RevitLinkInstance linkInstance)
        {
            Solid viewSolid = view.ViewSolid(true);

            Transform linkTransform = linkInstance.GetTotalTransform();
            if (linkTransform.IsIdentity)
                return viewSolid;

            return SolidUtils.CreateTransformed(viewSolid, linkTransform.Inverse);
        }

        /***************************************************/
    }
}


