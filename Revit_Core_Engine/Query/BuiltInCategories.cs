/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using BH.oM.Physical.Elements;
using BH.oM.Structure.Elements;
using System;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static IEnumerable<BuiltInCategory> BuiltInCategories(this Type bHoMType)
        {
            HashSet<BuiltInCategory> result = new HashSet<BuiltInCategory>();
            foreach (Type key in BuiltInCategoryTable.Keys)
            {
                if (bHoMType.IsAssignableFrom(key))
                {
                    foreach (BuiltInCategory category in BuiltInCategoryTable[key])
                        result.Add(category);
                }
            }
            

            return result;
        }


        /***************************************************/
        /****             Public dictionary             ****/
        /***************************************************/

        public static readonly Dictionary<Type, BuiltInCategory[]> BuiltInCategoryTable = new Dictionary<Type, BuiltInCategory[]>
        {
            {
                typeof (Bar), new BuiltInCategory[]
                {
                    //Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFoundation,
                    Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming,
                    Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns,
                    Autodesk.Revit.DB.BuiltInCategory.OST_Columns,
                    Autodesk.Revit.DB.BuiltInCategory.OST_VerticalBracing,
                    Autodesk.Revit.DB.BuiltInCategory.OST_Truss,
                    Autodesk.Revit.DB.BuiltInCategory.OST_StructuralTruss,
                    Autodesk.Revit.DB.BuiltInCategory.OST_HorizontalBracing,
                    Autodesk.Revit.DB.BuiltInCategory.OST_Purlin,
                    Autodesk.Revit.DB.BuiltInCategory.OST_Joist,
                    Autodesk.Revit.DB.BuiltInCategory.OST_Girder,
                    Autodesk.Revit.DB.BuiltInCategory.OST_StructuralStiffener,
                    Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingOther
                }
            },
            {
                typeof (Column), new BuiltInCategory[]
                {
                    Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns,
                    Autodesk.Revit.DB.BuiltInCategory.OST_Columns
                }
            },
            {
                typeof (Bracing), new BuiltInCategory[]
                {
                    Autodesk.Revit.DB.BuiltInCategory.OST_VerticalBracing,
                    Autodesk.Revit.DB.BuiltInCategory.OST_HorizontalBracing
                }
            },
            {
                typeof (Beam), new BuiltInCategory[]
                {
                Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming,
                Autodesk.Revit.DB.BuiltInCategory.OST_Truss,
                Autodesk.Revit.DB.BuiltInCategory.OST_StructuralTruss,
                Autodesk.Revit.DB.BuiltInCategory.OST_Purlin,
                Autodesk.Revit.DB.BuiltInCategory.OST_Joist,
                Autodesk.Revit.DB.BuiltInCategory.OST_Girder,
                Autodesk.Revit.DB.BuiltInCategory.OST_StructuralStiffener,
                Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingOther
                }
            },
            {
                typeof (Window), new BuiltInCategory[]
                {
                    Autodesk.Revit.DB.BuiltInCategory.OST_Windows,
                    Autodesk.Revit.DB.BuiltInCategory.OST_CurtainWallPanels
                }
            },
            {
                typeof (Door), new BuiltInCategory[]
                {
                    Autodesk.Revit.DB.BuiltInCategory.OST_Doors
                }
            },
            {
                typeof (BH.oM.Environment.Elements.Panel), new BuiltInCategory[]
                {
                    Autodesk.Revit.DB.BuiltInCategory.OST_Doors,
                    Autodesk.Revit.DB.BuiltInCategory.OST_Windows,
                }
            },
            {
                typeof (BH.oM.Geometry.ShapeProfiles.IProfile), new BuiltInCategory[]
                {
                    Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming,
                    Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns,
                    Autodesk.Revit.DB.BuiltInCategory.OST_Columns,
                    Autodesk.Revit.DB.BuiltInCategory.OST_VerticalBracing,
                    Autodesk.Revit.DB.BuiltInCategory.OST_Truss,
                    Autodesk.Revit.DB.BuiltInCategory.OST_StructuralTruss,
                    Autodesk.Revit.DB.BuiltInCategory.OST_HorizontalBracing,
                    Autodesk.Revit.DB.BuiltInCategory.OST_Purlin,
                    Autodesk.Revit.DB.BuiltInCategory.OST_Joist,
                    Autodesk.Revit.DB.BuiltInCategory.OST_Girder,
                    Autodesk.Revit.DB.BuiltInCategory.OST_StructuralStiffener,
                    Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingOther
                }
            }
        };

        /***************************************************/
    }
}
