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
using BH.oM.Adapters.Revit.Enums;
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

        [Description("Returns ElementIds of insert objects to be ignored in convert of a Revit panel element to given engineering discipline.")]
        [Input("hostObject", "Revit HostObject (panel element) to find the inserts that should be ignored on convert.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Output("ids", "ElementIds of insert objects to be ignored in convert of a Revit panel element to the input engineering discipline.")]
        public static IList<ElementId> InsertsToIgnore(this HostObject hostObject, Discipline discipline)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return hostObject.FindInserts(true, true, true, true);
                case Discipline.Structural:
                    return null;
                case Discipline.Facade:
                case Discipline.Physical:
                case Discipline.Architecture:
                    {
                        if (hostObject is Ceiling || hostObject.CurtainGrids().Count != 0)
                            return null;

                        BoundingBoxXYZ bbox = hostObject.get_BoundingBox(null);
                        if (bbox == null)
                            return null;

                        IList<ElementId> insertIds = hostObject.FindInserts(true, true, true, true);
                        List<Element> inserts = new List<Element>();
                        if (insertIds.Count != 0)
                        {
                            BoundingBoxIntersectsFilter bbif = new BoundingBoxIntersectsFilter(new Outline(bbox.Min, bbox.Max));
                            inserts = new FilteredElementCollector(hostObject.Document, insertIds).WherePasses(bbif).ToList();
                        }

                        return inserts.Where(x => x is FamilyInstance && (x.Category.Id.IntegerValue == (int)Autodesk.Revit.DB.BuiltInCategory.OST_Windows || x.Category.Id.IntegerValue == (int)Autodesk.Revit.DB.BuiltInCategory.OST_Doors)).Select(x => x.Id).ToList();
                    }
            }

            return null;
        }

        /***************************************************/
    }
}
