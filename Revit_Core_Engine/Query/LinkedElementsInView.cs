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
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Return elements from link instance located in given view. Method returns all instance elements in the view scope (including hidden elements)")]
        [Input("view", "ViewPlan to get the elements from.")]
        [Input("linkInstance", "Revit Link Instance to get the elements from.")]
        [Input("elementFilters", "Additional filters for the element collector. If null, no additional filters will be applied.")]
        [Output("elements", "Elements of link instance in given view.")]
        public static List<Element> LinkedElementsInView(this ViewPlan view, RevitLinkInstance linkInstance, List<ElementFilter> elementFilters = null)
        {
            if (view == null || linkInstance == null || elementFilters.Count == 0)
                return null;

            Document linkDoc = linkInstance.GetLinkDocument();
            Transform linkTransform = linkInstance.GetTotalTransform();
            Solid viewSolid = view.Solid(linkTransform);

            ElementIntersectsSolidFilter solidFilter = new ElementIntersectsSolidFilter(viewSolid);

            if (elementFilters == null)
                return new FilteredElementCollector(linkDoc).WherePasses(solidFilter).WhereElementIsNotElementType().ToElements().ToList();

            LogicalOrFilter elementFilter = new LogicalOrFilter(elementFilters);
            LogicalAndFilter elementSolidFilter = new LogicalAndFilter(elementFilter, solidFilter);

            return new FilteredElementCollector(linkDoc).WherePasses(elementSolidFilter).WhereElementIsNotElementType().ToElements().ToList();
        }

        /***************************************************/
    }
}
