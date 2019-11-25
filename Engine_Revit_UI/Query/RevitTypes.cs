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
        /****              Public methods               ****/
        /***************************************************/
        
        public static IEnumerable<System.Type> RevitTypes(System.Type type)
        {
            if (type == null)
                return null;

            if (!typeof(IBHoMObject).IsAssignableFrom(type))//BH.Engine.Adapters.Revit.Query.IsAssignableFromByFullName(type, typeof(BHoMObject)))
                return null;

            List<System.Type> result = new List<System.Type>();
            if (type == typeof(oM.Environment.Elements.Panel))
            {
                result.Add(typeof(Floor));
                result.Add(typeof(Wall));
                result.Add(typeof(Ceiling));
                result.Add(typeof(RoofBase));
                result.Add(typeof(FamilyInstance));
                return result;
            }

            if (type == typeof(oM.Physical.Elements.Wall))
            {
                result.Add(typeof(Wall));
                return result;
            }

            if (type == typeof(oM.Physical.Elements.Floor))
            {
                result.Add(typeof(Floor));
                return result;
            }

            if (type == typeof(oM.Physical.Elements.Roof))
            {
                result.Add(typeof(RoofBase));
                return result;
            }


            if (type == typeof(oM.Physical.Elements.Window))
            {
                result.Add(typeof(FamilyInstance));
                //aResult.Add(typeof(Wall));
                //DO NOT ADD: Autodesk.Revit.DB.Panel -> does not work with FilteredElementCollector
                //aResult.Add(typeof(Autodesk.Revit.DB.Panel));
                return result;
            }


            if (type == typeof(oM.Physical.Elements.Door))
            {
                result.Add(typeof(FamilyInstance));
                return result;
            }

            if(typeof(oM.Physical.Elements.ISurface).IsAssignableFrom(type))
            {
                result.Add(typeof(Wall));
                result.Add(typeof(Floor));
                result.Add(typeof(RoofBase));
                return result;
            }

            if (type == typeof(oM.Structure.Elements.Panel))
            {
                result.Add(typeof(Floor));
                result.Add(typeof(Wall));
                return result;
            }

            if (type == typeof(oM.Structure.Elements.Bar))
            {
                result.Add(typeof(FamilyInstance));
                return result;
            }

            if (type == typeof(BH.oM.Physical.Elements.Cable) || type == typeof(BH.oM.Physical.Elements.Pile))
                return result;

            if (typeof(BH.oM.Physical.Elements.IFramingElement).IsAssignableFrom(type))
            {
                result.Add(typeof(FamilyInstance));
                return result;
            }

            if (type == typeof(Building))
            {
                result.Add(typeof(ProjectInfo));
                return result;
            }

            if (type == typeof(oM.Geometry.SettingOut.Level))
            {
                result.Add(typeof(Level));
                return result;
            }

            if (type == typeof(Space))
            {
                result.Add(typeof(SpatialElement));
                return result;
            }

            if (type == typeof(oM.Architecture.Elements.Room))
            {
                result.Add(typeof(SpatialElement));
                return result;
            }

            if (type == typeof(oM.Geometry.SettingOut.Grid))
            {
                result.Add(typeof(Grid));
                return result;
            }

            if (type == typeof(Sheet))
            {
                result.Add(typeof(ViewSheet));
                return result;
            }

            if (type == typeof(oM.Adapters.Revit.Elements.ViewPlan))
            {
                result.Add(typeof(Autodesk.Revit.DB.ViewPlan));
                return result;
            }

            if (type == typeof(DraftingInstance))
            {
                result.Add(typeof(FamilyInstance));
                result.Add(typeof(CurveElement));
                return result;
            }

            if (type == typeof(ModelInstance))
            {
                result.Add(typeof(FamilyInstance));
                result.Add(typeof(CurveElement));
                return result;
            }

            if (type == typeof(oM.Adapters.Revit.Properties.InstanceProperties))
            {
                result.Add(typeof(ElementType));
                result.Add(typeof(GraphicsStyle));
                return result;
            }

            if (type == typeof(oM.Adapters.Revit.Elements.Viewport))
            {
                result.Add(typeof(Autodesk.Revit.DB.Viewport));
                return result;
            }

            if (type == typeof(oM.Adapters.Revit.Generic.RevitFilePreview))
            {
                result.Add(typeof(Autodesk.Revit.DB.Family));
                return result;
            }

            if (type == typeof(oM.Architecture.Elements.Ceiling))
            {
                result.Add(typeof(Ceiling));
                return result;
            }

            if (type == typeof(oM.Geometry.ShapeProfiles.ISectionProfile))
            {
                result.Add(typeof(FamilySymbol));
                return result;
            }

            return null;
        }

        /***************************************************/
    }
}
