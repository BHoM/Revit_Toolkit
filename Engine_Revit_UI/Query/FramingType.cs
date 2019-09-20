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
using Autodesk.Revit.DB.Structure.StructuralSections;

using BH.oM.Environment.Elements;
using BH.oM.Environment.Fragments;
using BH.oM.Geometry.ShapeProfiles;
using BH.oM.Structure.SurfaceProperties;
using BH.oM.Adapters.Revit.Elements;

namespace BH.UI.Revit.Engine
{
    /***************************************************/
    /**** Public Methods                            ****/
    /***************************************************/

    public static partial class Query
    {
        public static Type FramingType(this BuiltInCategory category)
        {
            switch (category)
            {
                //case BuiltInCategory.OST_StructuralFoundation:
                case Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns:
                case Autodesk.Revit.DB.BuiltInCategory.OST_Columns:
                    return typeof(BH.oM.Physical.Elements.Column);
                case Autodesk.Revit.DB.BuiltInCategory.OST_VerticalBracing:
                case Autodesk.Revit.DB.BuiltInCategory.OST_HorizontalBracing:
                    return typeof(BH.oM.Physical.Elements.Bracing);
                case Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming:
                case Autodesk.Revit.DB.BuiltInCategory.OST_Truss:
                case Autodesk.Revit.DB.BuiltInCategory.OST_StructuralTruss:
                case Autodesk.Revit.DB.BuiltInCategory.OST_Purlin:
                case Autodesk.Revit.DB.BuiltInCategory.OST_Joist:
                case Autodesk.Revit.DB.BuiltInCategory.OST_Girder:
                case Autodesk.Revit.DB.BuiltInCategory.OST_StructuralStiffener:
                case Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingOther:
                    return typeof(BH.oM.Physical.Elements.Beam);
            }

            return null;
        }

        /***************************************************/
    }
}