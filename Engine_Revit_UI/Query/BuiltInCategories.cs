/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Environment.Elements;
using BH.oM.Structure.Elements;
using BH.oM.Data.Requests;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IEnumerable<BuiltInCategory> BuiltInCategories(this Type type)
        {
            if (type == null)
                return null;

            List<BuiltInCategory> aBuiltInCategoryList = new List<BuiltInCategory>();

            if (type == typeof(BH.oM.Architecture.Elements.Room))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Rooms);
            }

            if (type == typeof(BH.oM.Environment.Elements.Space))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MEPSpaces);
            }


            if (type == typeof(BH.oM.Physical.Elements.Wall))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Walls);
            }
            if (type == typeof(BH.oM.Physical.Elements.Floor))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Floors);
            }
            if (type == typeof(BH.oM.Physical.Elements.Roof))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Roofs);
            }
            if (type == typeof(BH.oM.Physical.Elements.Window))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Windows);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurtainWallPanels);
            }
            if (type == typeof(BH.oM.Physical.Elements.Door))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Doors);
            }
            if (type == typeof(BH.oM.Physical.Elements.IFramingElement))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming);
                //aBuiltInCategoryList.Add(BuiltInCategory.OST_StructuralFoundation);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Columns);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_VerticalBracing);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Truss);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralTruss);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HorizontalBracing);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Purlin);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Joist);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Girder);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralStiffener);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingOther);
            }
            if (type == typeof(BH.oM.Structure.Elements.Bar))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming);
                //aBuiltInCategoryList.Add(BuiltInCategory.OST_StructuralFoundation);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Columns);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_VerticalBracing);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Truss);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralTruss);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HorizontalBracing);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Purlin);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Joist);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Girder);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralStiffener);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingOther);
            }
            if (type == typeof(BH.oM.Structure.Elements.Panel))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Floors);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Walls);
            }
            if (type == typeof(BH.oM.Physical.Elements.Column))
            {

                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Columns);
            }
            if (type == typeof(BH.oM.Physical.Elements.Bracing))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_VerticalBracing);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HorizontalBracing);
            }
            if (type == typeof(BH.oM.Physical.Elements.Pile))
            {
                BH.Engine.Reflection.Compute.RecordError("Pulling of piles explicitly not currently supported. Try pulling IFramingElement");
            }
            if (type == typeof(BH.oM.Physical.Elements.Cable))
            {
                BH.Engine.Reflection.Compute.RecordError("Pulling of cables explicitly not currently supported. Try pulling IFramingElement");
            }
            if (type == typeof(BH.oM.Physical.Elements.Beam))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Truss);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralTruss);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Purlin);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Joist);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Girder);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralStiffener);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingOther);
            }
            if (type == typeof(oM.Environment.Elements.Panel))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Doors);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Windows);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Walls);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Floors);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Ceilings);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Roofs);
            }
            if(type == typeof(oM.Adapters.Revit.Elements.Sheet))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Sheets);
            }
            if (type == typeof(oM.Adapters.Revit.Elements.Viewport))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Viewports);
            }
            if (type == typeof(oM.Adapters.Revit.Elements.ViewPlan))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Views);
            }
            if (type == typeof(oM.Architecture.Elements.Ceiling))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Ceilings);
            }
            if (type == typeof(oM.Geometry.ShapeProfiles.ISectionProfile))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_VerticalBracing);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HorizontalBracing);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Purlin);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Joist);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Girder);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralStiffener);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingOther);
            }

            return aBuiltInCategoryList;
        }

        /***************************************************/

        public static IEnumerable<BuiltInCategory> BuiltInCategories(IEnumerable<Element> elements)
        {
            if (elements == null)
                return null;

            List<BuiltInCategory> aBuiltInCategoryList = new List<BuiltInCategory>();
            foreach (Element aElement in elements)
                if (aElement.Category != null)
                {
                    BuiltInCategory aBuiltInCategory = (BuiltInCategory)aElement.Category.Id.IntegerValue;
                    if (!aBuiltInCategoryList.Contains(aBuiltInCategory))
                        aBuiltInCategoryList.Add(aBuiltInCategory);
                }

            return aBuiltInCategoryList;
        }

        /***************************************************/

        public static IEnumerable<BuiltInCategory> BuiltInCategories(IEnumerable<FilterRequest> filterRequests, Document document)
        {
            if (filterRequests == null || document == null)
                return null;

            List<BuiltInCategory> aBuiltInCategories = new List<BuiltInCategory>();
            foreach (FilterRequest aFilterRequest in filterRequests)
            {
                BuiltInCategory aBuiltInCategory = Query.BuiltInCategory(aFilterRequest, document);
                if (aBuiltInCategory != Autodesk.Revit.DB.BuiltInCategory.INVALID)
                    aBuiltInCategories.Add(aBuiltInCategory);
            }
            return aBuiltInCategories;
        }
    }
}