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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using BH.Engine.Geometry;
using System;
using System.Linq;
using BH.oM.Physical.FramingProperties;
using BH.oM.Reflection;

namespace BH.Revit.Engine.Core
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
            if (!(typeof(Column).BuiltInCategories().Contains((BuiltInCategory)element.Category.Id.IntegerValue)))
                return false;

            oM.Geometry.Line columnLine = column.Location as oM.Geometry.Line;
            if (columnLine == null)
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Location has not been updated, only linear columns are allowed in Revit. Revit ElementId: {0} BHoM_Guid: {1}", element.Id, column.BHoM_Guid));
                return false;
            }

            if (columnLine.Start.Z >= columnLine.End.Z)
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Location of the column has not been updated because BHoM column has start above end. Revit ElementId: {0} BHoM_Guid: {1}", element.Id, column.BHoM_Guid));
                return false;
            }

            if (1 - columnLine.Direction().DotProduct(Vector.ZAxis) > settings.AngleTolerance && element.LookupParameterInteger(BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM) == 0)
            {
                BH.Engine.Reflection.Compute.RecordWarning(String.Format("Column style has been set to Vertical, but its driving curve is slanted. Column style changed to Slanted. Revit ElementId: {0} BHoM_Guid: {1}", element.Id, column.BHoM_Guid));
                element.SetParameter(BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM, 2);
                element.Document.Regenerate();
            }

            bool updated = false;
            if (element.IsSlantedColumn)
            {
                updated |= element.SetLocation(columnLine, settings);
                Output<double, double> extensions = element.ColumnExtensions();
                double startExtension = -extensions.Item1;
                double endExtension = -extensions.Item2;

                if (Math.Abs(startExtension) > settings.DistanceTolerance || Math.Abs(endExtension) > settings.DistanceTolerance)
                {
                    element.SetLocation(columnLine.Extend(startExtension, endExtension), settings);
                    updated = true;
                }
            }
            else
            {
                double locationZ = ((LocationPoint)element.Location).Point.Z.ToSI(UnitType.UT_Length);
                updated |= element.SetLocation(new oM.Geometry.Point { X = columnLine.Start.X, Y = columnLine.Start.Y, Z = locationZ }, settings);

                Parameter baseLevelParam = element.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM);
                Parameter topLevelParam = element.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);
                Parameter baseOffsetParam = element.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM);
                Parameter topOffsetParam = element.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM);
                Level baseLevel = element.Document.GetElement(baseLevelParam.AsElementId()) as Level;
                Level topLevel = element.Document.GetElement(topLevelParam.AsElementId()) as Level;
                double baseElevation = (baseLevel.ProjectElevation + baseOffsetParam.AsDouble()).ToSI(UnitType.UT_Length);
                double topElevation = (topLevel.ProjectElevation + topOffsetParam.AsDouble()).ToSI(UnitType.UT_Length);

                if (Math.Abs(baseElevation - columnLine.Start.Z) > settings.DistanceTolerance)
                {
                    element.SetParameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM, columnLine.Start.Z.FromSI(UnitType.UT_Length) - baseLevel.ProjectElevation, false);
                    updated = true;
                }

                if (Math.Abs(topElevation - columnLine.End.Z) > settings.DistanceTolerance)
                {
                    element.SetParameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM, columnLine.End.Z.FromSI(UnitType.UT_Length) - topLevel.ProjectElevation, false);
                    updated = true;
                }
            }
            
            double rotationDifference = element.AdjustedRotationColumn(settings) - ((ConstantFramingProperty)column.Property).OrientationAngle;
            if (Math.Abs(rotationDifference) > settings.AngleTolerance)
            {
                double rotationParamValue = element.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE);
                if (double.IsNaN(rotationParamValue))
                {
                    ElementTransformUtils.RotateElement(element.Document, element.Id, columnLine.ToRevit(), -rotationDifference);
                    updated = true;
                }
                else
                {
                    double newRotation = (rotationParamValue + rotationDifference).NormalizeAngleDomain(settings);
                    updated |= element.SetParameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE, newRotation);
                }
            }

            return updated;
        }

        /***************************************************/

        public static bool SetLocation(this Level level, BH.oM.Geometry.SettingOut.Level bHoMLevel, RevitSettings settings)
        {
            return  level.SetParameter(BuiltInParameter.LEVEL_ELEV, bHoMLevel.Elevation);
        }

        /***************************************************/

        public static bool SetLocation(this FamilyInstance element, IFramingElement framingElement, RevitSettings settings)
        {
            if (!(typeof(IFramingElement).BuiltInCategories().Contains((BuiltInCategory)element.Category.Id.IntegerValue)))
                return false;

            ConstantFramingProperty framingProperty = framingElement.Property as ConstantFramingProperty;
            if (framingProperty == null)
            {
                //TODO warning and no rotation added
            }

            //TODO: block nonuniform offsets here!

            bool updated = element.SetLocation(framingElement.Location, settings);
            element.Document.Regenerate();
            
            double rotationDifference = element.AdjustedRotationFraming(settings) - ((ConstantFramingProperty)framingElement.Property).OrientationAngle;
            if (Math.Abs(rotationDifference) > settings.AngleTolerance)
            {
                double newRotation = (element.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE) + rotationDifference).NormalizeAngleDomain(settings);
                updated |= element.SetParameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE, newRotation);
                element.Document.Regenerate();
            }

            ICurve transformedCurve = framingElement.AdjustedLocationFraming((FamilyInstance)element, settings);
            updated |= element.SetLocation(transformedCurve, settings);

            return updated;
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

        public static bool SetLocation(this Element element, BH.oM.Geometry.SettingOut.Level level, RevitSettings settings)
        {
            return true;
        }

        /***************************************************/

        public static bool SetLocation(this Grid grid, BH.oM.Geometry.SettingOut.Grid bHoMGrid, RevitSettings settings)
        {
            if (!bHoMGrid.Curve.IToRevit().IsSimilar(grid.Curve, settings))
                BH.Engine.Reflection.Compute.RecordError(String.Format("Revit does not allow changing the geometry of an existing grid programatically. Try using DeleteThenCreate PushType instead. Revit ElementId: {0} BHoM_Guid: {1}", grid.Id, bHoMGrid.BHoM_Guid));

            return false;
        }

        /***************************************************/

        public static bool SetLocation(this Element element, ICurve curve, RevitSettings settings)
        {
            LocationCurve location = element.Location as LocationCurve;
            if (location == null || location.IsReadOnly)
                return false;

            Curve bHoMCurve = curve.IToRevit();
            if (!location.Curve.IsSimilar(bHoMCurve, settings))
            {
                location.Curve = bHoMCurve;
                return true;
            }
            else
                return false;
        }

        /***************************************************/

        public static bool SetLocation(this Element element, BH.oM.Geometry.Point point, RevitSettings settings)
        {
            LocationPoint location = element.Location as LocationPoint;
            if (location == null || location.IsReadOnly)
                return false;

            XYZ bHoMPoint = point.ToRevit();
            if (!location.Point.IsAlmostEqualTo(bHoMPoint, settings.DistanceTolerance))
            {
                location.Point = bHoMPoint;
                return true;
            }

            return false;
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
