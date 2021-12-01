/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Reflection.Attributes;
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

        [Description("Looks for a Revit element that contains the given XYZ point.")]
        [Input("point", "XYZ point to find the containing element for.")]
        [Input("document", "Revit document to be searched for the containing element.")]
        [Input("categories", "Revit categories to be taken into account when performing the search. If null, elements of all categories will be checked.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("host", "First Revit element that contains the input XYZ point.")]
        public static Element ContainingElement(this XYZ point, Document document, IEnumerable<BuiltInCategory> categories, RevitSettings settings = null)
        {
            if (point == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Host element could not be found for a null point.");
                return null;
            }

            if (document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Host element could not be found in a null Revit document.");
                return null;
            }

            settings = settings.DefaultIfNull();

            FilteredElementCollector collector = new FilteredElementCollector(document).WhereElementIsNotElementType();
            if (categories != null)
            {
                if (categories.Any())
                {
                    LogicalOrFilter filter = new LogicalOrFilter(new List<ElementFilter>(categories.Select(x => new ElementCategoryFilter(x))));
                    collector = collector.WherePasses(filter);
                }
                else
                    return null;
            }

            return collector.FirstOrDefault(x => point.IsInside(x, settings));
        }

        /***************************************************/
    }
}


