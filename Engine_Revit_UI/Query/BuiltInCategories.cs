/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
        /****              Public methods               ****/
        /***************************************************/

        public static IEnumerable<BuiltInCategory> BuiltInCategories(this Type type)
        {
            if (type == null)
                return null;

            List<BuiltInCategory> builtInCategories = new List<BuiltInCategory>();

            if (type == typeof(BH.oM.Architecture.Elements.Room))
            {
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Rooms);
            }

            if (type == typeof(BH.oM.Environment.Elements.Space))
            {
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_MEPSpaces);
            }

            if (type == typeof(BH.oM.Physical.Elements.Wall))
            {
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Walls);
            }
            if (type == typeof(BH.oM.Physical.Elements.Floor))
            {
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Floors);
            }
            if (type == typeof(BH.oM.Physical.Elements.Roof))
            {
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Roofs);
            }
            if (type == typeof(BH.oM.Physical.Elements.Window))
            {
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Windows);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_CurtainWallPanels);
            }
            if (type == typeof(BH.oM.Physical.Elements.Door))
            {
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Doors);
            }
            if (type == typeof(BH.oM.Physical.Elements.ISurface))
            {
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Roofs);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Floors);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Walls);
            }

            if (type == typeof(BH.oM.Physical.Elements.IFramingElement))
            {
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming);
                //aBuiltInCategoryList.Add(BuiltInCategory.OST_StructuralFoundation);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Columns);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_VerticalBracing);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Truss);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralTruss);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HorizontalBracing);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Purlin);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Joist);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Girder);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralStiffener);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingOther);
            }
            if (type == typeof(BH.oM.Structure.Elements.Bar))
            {
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming);
                //aBuiltInCategoryList.Add(BuiltInCategory.OST_StructuralFoundation);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Columns);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_VerticalBracing);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Truss);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralTruss);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HorizontalBracing);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Purlin);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Joist);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Girder);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralStiffener);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingOther);
            }
            if (type == typeof(BH.oM.Structure.Elements.Panel))
            {
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Floors);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Walls);
            }
            if (type == typeof(BH.oM.Physical.Elements.Column))
            {

                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Columns);
            }
            if (type == typeof(BH.oM.Physical.Elements.Bracing))
            {
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_VerticalBracing);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HorizontalBracing);
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
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Truss);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralTruss);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Purlin);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Joist);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Girder);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralStiffener);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingOther);
            }
            if (type == typeof(oM.Environment.Elements.Panel))
            {
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Doors);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Windows);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Walls);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Floors);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Ceilings);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Roofs);
            }
            if(type == typeof(oM.Adapters.Revit.Elements.Sheet))
            {
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Sheets);
            }
            if (type == typeof(oM.Adapters.Revit.Elements.Viewport))
            {
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Viewports);
            }
            if (type == typeof(oM.Adapters.Revit.Elements.ViewPlan))
            {
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Views);
            }
            if (type == typeof(oM.Architecture.Elements.Ceiling))
            {
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Ceilings);
            }
            if (type == typeof(oM.Geometry.ShapeProfiles.ISectionProfile))
            {
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_VerticalBracing);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HorizontalBracing);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Purlin);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Joist);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Girder);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralStiffener);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingOther);
            }
            if (type == typeof(oM.Geometry.SettingOut.Grid))
            {
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Grids);
                builtInCategories.Add(Autodesk.Revit.DB.BuiltInCategory.OST_GridChains);
            }

            return builtInCategories;
        }

        /***************************************************/

        public static IEnumerable<BuiltInCategory> BuiltInCategories(IEnumerable<Element> elements)
        {
            if (elements == null)
                return null;

            List<BuiltInCategory> builtInCategories = new List<BuiltInCategory>();
            foreach (Element element in elements)
                if (element.Category != null)
                {
                    BuiltInCategory builtInCategory = (BuiltInCategory)element.Category.Id.IntegerValue;
                    if (!builtInCategories.Contains(builtInCategory))
                        builtInCategories.Add(builtInCategory);
                }

            return builtInCategories;
        }

        /***************************************************/
    }
}