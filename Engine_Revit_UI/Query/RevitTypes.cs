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

using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Structure.Elements;


namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static IEnumerable<System.Type> RevitTypes(System.Type type)
        {
            if (type == null)
                return null;

            if (!BH.Engine.Adapters.Revit.Query.IsAssignableFromByFullName(type, typeof(BHoMObject)))
                return null;

            List<System.Type> aResult = new List<System.Type>();
            if (type == typeof(oM.Environment.Elements.Panel))
            {
                aResult.Add(typeof(Floor));
                aResult.Add(typeof(Wall));
                aResult.Add(typeof(Ceiling));
                aResult.Add(typeof(RoofBase));
                aResult.Add(typeof(FamilyInstance));
                return aResult;
            }

            if (type == typeof(oM.Structure.Elements.Panel))
            {
                aResult.Add(typeof(Floor));
                aResult.Add(typeof(Wall));
                return aResult;
            }
            
            if (type == typeof(FramingElement))
            {
                aResult.Add(typeof(FamilyInstance));
                return aResult;
            }

            if (type == typeof(Building))
            {
                aResult.Add(typeof(ProjectInfo));
                return aResult;
            }

            if (type == typeof(oM.Architecture.Elements.Level))
            {
                aResult.Add(typeof(Level));
                return aResult;
            }

            if (type == typeof(Space))
            {
                aResult.Add(typeof(SpatialElement));
                return aResult;
            }

            if (type == typeof(oM.Architecture.Elements.Grid))
            {
                aResult.Add(typeof(Grid));
                return aResult;
            }

            if (type == typeof(Sheet))
            {
                aResult.Add(typeof(ViewSheet));
                return aResult;
            }

            if (type == typeof(oM.Adapters.Revit.Elements.ViewPlan))
            {
                aResult.Add(typeof(Autodesk.Revit.DB.ViewPlan));
                return aResult;
            }

            if (type == typeof(DraftingInstance))
            {
                aResult.Add(typeof(FamilyInstance));
                return aResult;
            }

            if (type == typeof(oM.Adapters.Revit.Elements.Viewport))
            {
                aResult.Add(typeof(Autodesk.Revit.DB.Viewport));
                return aResult;
            }

            if (type == typeof(oM.Adapters.Revit.Generic.RevitFilePreview))
            {
                aResult.Add(typeof(Autodesk.Revit.DB.Family));
                return aResult;
            }

            return null;
        }

        /***************************************************/
    }
}
