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
using BH.oM.Base;
using BH.oM.Physical.Elements;
using BH.oM.Structure.Elements;
using BH.oM.Facade.Elements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BH.oM.Reflection.Attributes;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Queries a BHoM Family for its Revit BuiltInCategory.")]
        [Input("family", "BHoM family to be queried.")]
        [Input("document", "Revit current document to be processed.")]
        [Input("caseSensitive", "Optional, whether the lookup is case sensitive or not.")]
        [Output("builtInCategories", "Resulted HashSet list of Revit BuiltInCategory.")]
        public static HashSet<BuiltInCategory> BuiltInCategories(this oM.Adapters.Revit.Elements.Family family, Document document, bool caseSensitive = true)
        {
            if (family?.PropertiesList == null)
                return null;
            
            return new HashSet<BuiltInCategory>(family.PropertiesList.Select(x => x.BuiltInCategory(document, caseSensitive)));
        }

        /***************************************************/

        [Description("Queries a BHoM generic object for its Revit BuiltInCategory.")]
        [Input("bHoMObject", "BHoM object to be queried.")]
        [Input("document", "Revit current document to be processed.")]
        [Input("caseSensitive", "Optional, whether the lookup is case sensitive or not.")]
        [Output("builtInCategories", "Resulted HashSet list of Revit BuiltInCategory.")]
        public static HashSet<BuiltInCategory> BuiltInCategories(this IBHoMObject bHoMObject, Document document, bool caseSensitive = true)
        {
            BuiltInCategory category = document.BuiltInCategory(bHoMObject.CategoryName(), caseSensitive);
            if (category != Autodesk.Revit.DB.BuiltInCategory.INVALID)
                return new HashSet<BuiltInCategory> { category };

            return bHoMObject.GetType().BuiltInCategories();
        }

        /***************************************************/

        [Description("Queries a BHoM Type for its matching Revit BuiltInCategory.")]
        [Input("bHoMType", "BHoM Type to be queried.")]
        [Output("builtInCategories", "Resulted HashSet list of Revit BuiltInCategory.")]
        public static HashSet<BuiltInCategory> BuiltInCategories(this Type bHoMType)
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

        [Description("A dictiory that relates BHoM types with Revit BuiltInCategories.")]
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
                typeof (BH.oM.Spatial.ShapeProfiles.IProfile), new BuiltInCategory[]
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
            },
            {
                typeof (BH.oM.Facade.Elements.Opening), new BuiltInCategory[]
                {
                    Autodesk.Revit.DB.BuiltInCategory.OST_Doors,
                    Autodesk.Revit.DB.BuiltInCategory.OST_Windows,
                    Autodesk.Revit.DB.BuiltInCategory.OST_CurtainWallPanels
                }
            },
            {
                typeof (FrameEdge), new BuiltInCategory[]
                {
                    Autodesk.Revit.DB.BuiltInCategory.OST_CurtainWallMullions,
                }
            },
            {
                typeof (BH.oM.MEP.System.Fittings.Fitting), new BuiltInCategory[]
                {
                    Autodesk.Revit.DB.BuiltInCategory.OST_DuctFitting,
                    Autodesk.Revit.DB.BuiltInCategory.OST_PipeFitting,
                    Autodesk.Revit.DB.BuiltInCategory.OST_CableTrayFitting,
                    Autodesk.Revit.DB.BuiltInCategory.OST_DuctAccessory,
                    Autodesk.Revit.DB.BuiltInCategory.OST_PipeAccessory,
                }
            }
        };

        /***************************************************/
    }
}

