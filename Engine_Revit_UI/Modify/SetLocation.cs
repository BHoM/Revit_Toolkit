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
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Interface;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using BH.Engine.Geometry;
using System;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static bool SetLocation(this Element element, IInstance instance, RevitSettings settings)
        {
            if (instance.Location == null)
                return false;

            if (element.ViewSpecific && !(instance is DraftingInstance) || !element.ViewSpecific && !(instance is ModelInstance))
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Updating location of drafting elements is only allowed based on DraftingInstances, while updating location of model elements is only allowed based on ModelInstances. Revit ElementId: {0} BHoM_Guid: {1}", element.Id, instance.BHoM_Guid));
                return false;
            }

            return SetLocation(element, instance.Location as dynamic, settings);
        }

        /***************************************************/

        public static bool SetLocation(this FamilyInstance element, Column column, RevitSettings settings)
        {
            if (!typeof(Column).BuiltInCategories().ToList().Contains((BuiltInCategory)element.Category.Id.IntegerValue))
                return false;

            oM.Geometry.Line columnLine = column.Location as oM.Geometry.Line;
            if (columnLine == null)
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Location has not been updated, only linear columns are allowed in Revit. Revit ElementId: {0} BHoM_Guid: {1}", element.Id, column.BHoM_Guid));
                return false;
            }

            if (columnLine.IsVertical())
            {
                element.SetParameter(BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM, 0);
                element.SetLocation(new oM.Geometry.Point { X = columnLine.Start.X, Y = columnLine.Start.Y, Z = 0 }, settings);
                
                Level bottomLevel = columnLine.Start.BottomLevel(element.Document);
                Level topLevel = columnLine.End.BottomLevel(element.Document);

                element.SetParameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM, bottomLevel.Id);
                element.SetParameter(BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM, bottomLevel.Id);
                element.SetParameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM, topLevel.Id);
                element.SetParameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM, topLevel.Id);

                element.SetParameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM, columnLine.Start.Z.FromSI(UnitType.UT_Length) - bottomLevel.Elevation, false);
                element.SetParameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM, columnLine.End.Z.FromSI(UnitType.UT_Length) - topLevel.Elevation, false);

                return true;
            }
            else
            {
                element.SetParameter(BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM, 2);
                return element.SetLocation(columnLine, settings);
            }
        }

        /***************************************************/

        public static bool SetLocation(this FamilyInstance element, IFramingElement framingElement, RevitSettings settings)
        {
            if (!typeof(IFramingElement).BuiltInCategories().ToList().Contains((BuiltInCategory)element.Category.Id.IntegerValue))
                return false;

            return element.SetLocation(framingElement.Location, settings);
        }

        /***************************************************/

        public static bool SetLocation(this Autodesk.Revit.DB.Mechanical.Space revitSpace, Space bHoMSpace, RevitSettings settings)
        {
            Level level = bHoMSpace.Location.Z.BottomLevel(revitSpace.Document);
            if (level == null)
                return false;
            
            oM.Geometry.Point point = BH.Engine.Geometry.Create.Point(bHoMSpace.Location.X, bHoMSpace.Location.Y, level.Elevation);

            return revitSpace.SetLocation(point, settings);
        }

        /***************************************************/

        public static bool SetLocation(this Element element, ICurve curve, RevitSettings settings)
        {
            LocationCurve location = element.Location as LocationCurve;
            if (location == null || location.IsReadOnly)
                return false;

            Curve bHoMCurve = curve.IToRevit();
            if (!location.Curve.IsSimilar(bHoMCurve, settings))
                location.Curve = bHoMCurve;

            return true;
        }

        /***************************************************/

        public static bool SetLocation(this Element element, BH.oM.Geometry.Point point, RevitSettings settings)
        {
            LocationPoint location = element.Location as LocationPoint;
            if (location == null || location.IsReadOnly)
                return false;

            XYZ bHoMPoint = point.ToRevit();
            if (!location.Point.IsAlmostEqualTo(bHoMPoint, settings.DistanceTolerance))
                location.Point = bHoMPoint;

            return true;
        }

        /***************************************************/

        public static bool SetLocation(this HostObject element, BH.oM.Physical.Elements.ISurface bHoMObject, RevitSettings settings)
        {
            BH.Engine.Reflection.Compute.RecordError(String.Format("Update of location of surface-based elements is currently not supported. Revit ElementId: {0} BHoM_Guid: {1}", element.Id, bHoMObject.BHoM_Guid));
            return false;
        }


        /***************************************************/
        /****             Fallback Methods              ****/
        /***************************************************/

        public static bool SetLocation(this Element element, IGeometry geometry, RevitSettings settings)
        {
            BH.Engine.Reflection.Compute.RecordError(String.Format("Setting Revit element location based on BHoM geometry of type {0} is currently not supported. Revit ElementId: {1}", geometry.GetType(), element.Id));
            return false;
        }

        /***************************************************/

        public static bool SetLocation(this Element element, IBHoMObject bHoMObject, RevitSettings settings)
        {
            BH.Engine.Reflection.Compute.RecordError(String.Format("Unable to set location of Revit element of type {0} based on BHoM object of type {1} beacuse no suitable method could be found. Revit ElementId: {2} BHoM_Guid: {3}", element.GetType(), bHoMObject.GetType(), element.Id, bHoMObject.BHoM_Guid));
            return false;
        }


        /***************************************************/
        /****             Interface methods             ****/
        /***************************************************/

        public static bool ISetLocation(this Element element, IBHoMObject bHoMObject, RevitSettings settings)
        {
            if (element == null || bHoMObject == null)
                return false;

            return SetLocation(element as dynamic, bHoMObject as dynamic, settings);
        }
        
        /***************************************************/
    }
}
