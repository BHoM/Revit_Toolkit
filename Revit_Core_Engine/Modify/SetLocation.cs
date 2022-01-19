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
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using BH.oM.Physical.FramingProperties;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Sets the location of a given Revit Element based on a given BHoM IInstance.")]
        [Input("element", "Revit Element to be modified.")]
        [Input("instance", "BHoM IInsance acting as a source of information about the new location.")]
        [Input("settings", "Revit adapter settings to be used while performing the operation.")]
        [Output("success", "True if location of the input Revit Element has been successfully set.")]
        public static bool SetLocation(this Element element, IInstance instance, RevitSettings settings)
        {
            if (instance.Location == null)
                return false;

            return SetLocation(element, instance.Location as dynamic, settings);
        }

        /***************************************************/

        [Description("Sets the location of a given Revit FamilyInstance based on a given BHoM builders work Opening.")]
        [Input("element", "Revit FamilyInstance to be modified.")]
        [Input("opening", "BHoM builders work Opening acting as a source of information about the new location.")]
        [Input("settings", "Revit adapter settings to be used while performing the operation.")]
        [Output("success", "True if location of the input Revit FamilyInstance has been successfully set.")]
        public static bool SetLocation(this FamilyInstance element, BH.oM.Architecture.BuildersWork.Opening opening, RevitSettings settings)
        {
            if (element == null || opening?.CoordinateSystem == null)
                return false;

            return element.SetLocation(opening.CoordinateSystem.Origin, new Basis(opening.CoordinateSystem.X, opening.CoordinateSystem.Y, opening.CoordinateSystem.Z), settings);
        }

        /***************************************************/

        [Description("Sets the location of a given Revit FamilyInstance based on a given BHoM IInstance.")]
        [Input("element", "Revit FamilyInstance to be modified.")]
        [Input("instance", "BHoM IInsance acting as a source of information about the new location.")]
        [Input("settings", "Revit adapter settings to be used while performing the operation.")]
        [Output("success", "True if location of the input Revit FamilyInstance has been successfully set.")]
        public static bool SetLocation(this FamilyInstance element, IInstance instance, RevitSettings settings)
        {
            if (instance.Location == null)
                return false;

            bool success = false;
            if (instance.Location is BH.oM.Geometry.Point)
                success = element.SetLocation((BH.oM.Geometry.Point)instance.Location, instance.Orientation, settings);
            else if (instance.Location is ICurve && element.Host != null)
            {
                LocationCurve location = element.Location as LocationCurve;
                BH.oM.Geometry.Line l = instance.Location as BH.oM.Geometry.Line;
                if (location == null || l == null)
                    return false;

                XYZ start = l.Start.ToRevit();
                XYZ end = l.End.ToRevit();
                Transform instanceTransform = null;
                if (element.Host is FamilyInstance && !((FamilyInstance)element.Host).HasModifiedGeometry())
                {
                    instanceTransform = ((FamilyInstance)element.Host).GetTotalTransform();
                    start = (instanceTransform.Inverse.OfPoint(start));
                    end = (instanceTransform.Inverse.OfPoint(end));
                }

                Autodesk.Revit.DB.Face face = element.Host.GetGeometryObjectFromReference(element.HostFace) as Autodesk.Revit.DB.Face;
                IntersectionResult ir1 = face?.Project(start);
                IntersectionResult ir2 = face?.Project(end);
                if (ir1 == null || ir2 == null)
                {
                    BH.Engine.Base.Compute.RecordError($"Location update failed: the new location line used on update of a family instance could not be placed on its host face. BHoM_Guid: {instance.BHoM_Guid} ElementId: {element.Id.IntegerValue}");
                    return false;
                }

                if (ir1.Distance > settings.DistanceTolerance || ir2.Distance > settings.DistanceTolerance)
                    BH.Engine.Base.Compute.RecordWarning($"The location line used on update of a family instance has been snapped to its host face. BHoM_Guid: {instance.BHoM_Guid} ElementId: {element.Id.IntegerValue}");

                start = ir1.XYZPoint;
                end = ir2.XYZPoint;
                if (instanceTransform != null)
                {
                    start = instanceTransform.OfPoint(start);
                    end = instanceTransform.OfPoint(end);
                }

                Autodesk.Revit.DB.Line newLocation = Autodesk.Revit.DB.Line.CreateBound(start, end);
                if (!newLocation.IsSimilar(location.Curve, settings))
                {
                    location.Curve = newLocation;
                    success = true;
                }
            }
            else
                success = SetLocation(element, instance.Location as dynamic, settings);

            return success;
        }

        /***************************************************/

        [Description("Sets the location of a given Revit FamilyInstance based on a given BHoM Column.")]
        [Input("element", "Revit FamilyInstance to be modified.")]
        [Input("column", "BHoM Column acting as a source of information about the new location.")]
        [Input("settings", "Revit adapter settings to be used while performing the operation.")]
        [Output("success", "True if location of the input Revit FamilyInstance has been successfully set.")]
        public static bool SetLocation(this FamilyInstance element, Column column, RevitSettings settings)
        {
            if (!(typeof(Column).BuiltInCategories().Contains((BuiltInCategory)element.Category.Id.IntegerValue)))
                return false;

            oM.Geometry.Line columnLine = column.Location as oM.Geometry.Line;
            if (columnLine == null)
            {
                BH.Engine.Base.Compute.RecordError(String.Format("Location has not been updated, only linear columns are allowed in Revit. Revit ElementId: {0} BHoM_Guid: {1}", element.Id, column.BHoM_Guid));
                return false;
            }

            if (columnLine.Start.Z >= columnLine.End.Z)
            {
                BH.Engine.Base.Compute.RecordError(String.Format("Location of the column has not been updated because BHoM column has start above end. Revit ElementId: {0} BHoM_Guid: {1}", element.Id, column.BHoM_Guid));
                return false;
            }

            if (1 - columnLine.Direction().DotProduct(Vector.ZAxis) > settings.AngleTolerance && element.LookupParameterInteger(BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM) == 0)
            {
                BH.Engine.Base.Compute.RecordWarning(String.Format("Column style has been set to Vertical, but its driving curve is slanted. Column style changed to Slanted. Revit ElementId: {0} BHoM_Guid: {1}", element.Id, column.BHoM_Guid));
                element.SetParameter(BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM, 2);
                element.Document.Regenerate();
            }

            bool updated = false;
            if (element.IsSlantedColumn)
            {
                updated |= element.SetLocation(columnLine, settings);
                Output<double, double> extensions = element.ColumnExtensions(settings);
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
                double locationZ = ((LocationPoint)element.Location).Point.Z.ToSI(SpecTypeId.Length);
                updated |= element.SetLocation(new oM.Geometry.Point { X = columnLine.Start.X, Y = columnLine.Start.Y, Z = locationZ }, settings);

                Parameter baseLevelParam = element.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM);
                Parameter topLevelParam = element.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);
                Parameter baseOffsetParam = element.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM);
                Parameter topOffsetParam = element.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM);
                Level baseLevel = element.Document.GetElement(baseLevelParam.AsElementId()) as Level;
                Level topLevel = element.Document.GetElement(topLevelParam.AsElementId()) as Level;
                double baseElevation = (baseLevel.ProjectElevation + baseOffsetParam.AsDouble()).ToSI(SpecTypeId.Length);
                double topElevation = (topLevel.ProjectElevation + topOffsetParam.AsDouble()).ToSI(SpecTypeId.Length);

                if (Math.Abs(baseElevation - columnLine.Start.Z) > settings.DistanceTolerance)
                {
                    element.SetParameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM, columnLine.Start.Z.FromSI(SpecTypeId.Length) - baseLevel.ProjectElevation, false);
                    updated = true;
                }

                if (Math.Abs(topElevation - columnLine.End.Z) > settings.DistanceTolerance)
                {
                    element.SetParameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM, columnLine.End.Z.FromSI(SpecTypeId.Length) - topLevel.ProjectElevation, false);
                    updated = true;
                }
            }

            double rotation = 0;
            ConstantFramingProperty framingProperty = column.Property as ConstantFramingProperty;
            if (framingProperty == null)
                BH.Engine.Base.Compute.RecordWarning(String.Format("BHoM object's property is not a ConstantFramingProperty, therefore its orientation angle could not be retrieved. BHoM_Guid: {0}", column.BHoM_Guid));
            else
                rotation = ((ConstantFramingProperty)column.Property).OrientationAngle;

            double rotationDifference = element.OrientationAngleColumn(settings) - rotation;
            if (Math.Abs(rotationDifference) > settings.AngleTolerance)
            {
                double rotationParamValue = element.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE);
                if (double.IsNaN(rotationParamValue))
                {
                    ElementTransformUtils.RotateElement(element.Document, element.Id, columnLine.ToRevit(), -rotationDifference.NormalizeAngleDomain());
                    updated = true;
                }
                else
                {
                    double newRotation = (rotationParamValue + rotationDifference).NormalizeAngleDomain();
                    updated |= element.SetParameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE, newRotation);
                }
            }

            return updated;
        }

        /***************************************************/

        [Description("Sets the location of a given Revit FamilyInstance based on a given BHoM IFramingElement.")]
        [Input("element", "Revit FamilyInstance to be modified.")]
        [Input("framingElement", "BHoM IFramingElement acting as a source of information about the new location.")]
        [Input("settings", "Revit adapter settings to be used while performing the operation.")]
        [Output("success", "True if location of the input Revit FamilyInstance has been successfully set.")]
        public static bool SetLocation(this FamilyInstance element, IFramingElement framingElement, RevitSettings settings)
        {
            if (!(typeof(IFramingElement).BuiltInCategories().Contains((BuiltInCategory)element.Category.Id.IntegerValue)))
                return false;

            double rotation = 0;
            ConstantFramingProperty framingProperty = framingElement.Property as ConstantFramingProperty;
            if (framingProperty == null)
                BH.Engine.Base.Compute.RecordWarning(String.Format("BHoM object's property is not a ConstantFramingProperty, therefore its orientation angle could not be retrieved. BHoM_Guid: {0}", framingElement.BHoM_Guid));
            else
                rotation = ((ConstantFramingProperty)framingElement.Property).OrientationAngle;

            if (element.LookupParameterInteger(BuiltInParameter.YZ_JUSTIFICATION) == 1)
            {
                BH.Engine.Base.Compute.RecordWarning(String.Format("Pushing of framing elements with non-uniform offsets at ends is currently not supported. yz Justification parameter has been set to Uniform. Revit ElementId: {0}", element.Id));
                element.SetParameter(BuiltInParameter.YZ_JUSTIFICATION, 0);
                element.Document.Regenerate();
            }

            bool updated = element.SetLocation(framingElement.Location, settings);
            element.Document.Regenerate();
            
            double rotationDifference = element.OrientationAngleFraming(settings) - rotation;
            if (Math.Abs(rotationDifference) > settings.AngleTolerance)
            {
                double newRotation = (element.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE) + rotationDifference).NormalizeAngleDomain();
                updated |= element.SetParameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE, newRotation);
                element.Document.Regenerate();
            }

            ICurve transformedCurve = framingElement.AdjustedLocationCurveFraming((FamilyInstance)element, settings);
            updated |= element.SetLocation(transformedCurve, settings);

            return updated;
        }

        /***************************************************/

        [Description("Sets the location of a given Revit Space based on a given BHoM Space.")]
        [Input("revitSpace", "Revit Space to be modified.")]
        [Input("bHoMSpace", "BHoM Space acting as a source of information about the new location.")]
        [Input("settings", "Revit adapter settings to be used while performing the operation.")]
        [Output("success", "True if location of the input Revit Space has been successfully set.")]
        public static bool SetLocation(this Autodesk.Revit.DB.Mechanical.Space revitSpace, Space bHoMSpace, RevitSettings settings)
        {
            Level level = revitSpace.Document.LevelBelow(bHoMSpace.Location, settings);
            if (level == null)
                return false;

            oM.Geometry.Point point = BH.Engine.Geometry.Create.Point(bHoMSpace.Location.X, bHoMSpace.Location.Y, level.Elevation);

            return revitSpace.SetLocation(point, settings);
        }

        /***************************************************/

        [Description("Sets the location of a given Revit Level based on a given BHoM Level.")]
        [Input("level", "Revit Level to be modified.")]
        [Input("bHoMLevel", "BHoM Level acting as a source of information about the new location.")]
        [Input("settings", "Revit adapter settings to be used while performing the operation.")]
        [Output("success", "True if location of the input Revit Level has been successfully set.")]
        public static bool SetLocation(this Level level, BH.oM.Spatial.SettingOut.Level bHoMLevel, RevitSettings settings)
        {
            return level.SetParameter(BuiltInParameter.LEVEL_ELEV, bHoMLevel.Elevation);
        }

        /***************************************************/

        [Description("Sets the location of a given Revit Grid based on a given BHoM Grid.")]
        [Input("grid", "Revit Grid to be modified.")]
        [Input("bHoMGrid", "BHoM Grid acting as a source of information about the new location.")]
        [Input("settings", "Revit adapter settings to be used while performing the operation.")]
        [Output("success", "True if location of the input Revit Grid has been successfully set.")]
        public static bool SetLocation(this Grid grid, BH.oM.Spatial.SettingOut.Grid bHoMGrid, RevitSettings settings)
        {
            if (!bHoMGrid.Curve.IToRevit().IsSimilar(grid.Curve, settings))
                BH.Engine.Base.Compute.RecordError(String.Format("Revit does not allow changing the geometry of an existing grid programatically. Try using DeleteThenCreate PushType instead. Revit ElementId: {0} BHoM_Guid: {1}", grid.Id, bHoMGrid.BHoM_Guid));

            return false;
        }

        /***************************************************/

        [Description("Sets the location of a given Revit Element based on a given BHoM ICurve.")]
        [Input("element", "Revit Element to be modified.")]
        [Input("curve", "BHoM ICurve acting as a source of information about the new location.")]
        [Input("settings", "Revit adapter settings to be used while performing the operation.")]
        [Output("success", "True if location of the input Revit Element has been successfully set.")]
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

        [Description("Sets the location of a given Revit Element based on a given BHoM Point.")]
        [Input("element", "Revit Element to be modified.")]
        [Input("point", "BHoM Point acting as a source of information about the new location.")]
        [Input("settings", "Revit adapter settings to be used while performing the operation.")]
        [Output("success", "True if location of the input Revit Element has been successfully set.")]
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

        [Description("Sets the location of a given Revit HostObject based on a given BHoM ISurface.")]
        [Input("element", "Revit HostObject to be modified.")]
        [Input("bHoMObject", "BHoM ISurface acting as a source of information about the new location.")]
        [Input("settings", "Revit adapter settings to be used while performing the operation.")]
        [Output("success", "True if location of the input Revit HostObject has been successfully set.")]
        public static bool SetLocation(this HostObject element, BH.oM.Physical.Elements.ISurface bHoMObject, RevitSettings settings)
        {
            BH.Engine.Base.Compute.RecordWarning(String.Format("Update of location of surface-based elements is currently not supported. Possibly DeleteThenCreate PushType could be used instead. Revit ElementId: {0} BHoM_Guid: {1}", element.Id, bHoMObject.BHoM_Guid));
            return false;
        }


        /***************************************************/
        /****             Fallback Methods              ****/
        /***************************************************/

        private static bool SetLocation(this Element element, IGeometry geometry, RevitSettings settings)
        {
            Type type = element.GetType();
            if (AbstractRevitTypes.All(x => !x.IsAssignableFrom(type)))
                BH.Engine.Base.Compute.RecordError(String.Format("Setting Revit element location based on BHoM geometry of type {0} is currently not supported. Only parameters were updated. Revit ElementId: {1}", geometry.GetType(), element.Id));

            return false;
        }

        /***************************************************/

        private static bool SetLocation(this Element element, IBHoMObject bHoMObject, RevitSettings settings)
        {
            Type type = element.GetType();
            if (AbstractRevitTypes.All(x => !x.IsAssignableFrom(type)))
                BH.Engine.Base.Compute.RecordError(String.Format("Unable to set location of Revit element of type {0} based on BHoM object of type {1} beacuse no suitable method could be found. Only parameters were updated. Revit ElementId: {2} BHoM_Guid: {3}", element.GetType(), bHoMObject.GetType(), element.Id, bHoMObject.BHoM_Guid));

            return false;
        }


        /***************************************************/
        /****             Interface methods             ****/
        /***************************************************/

        [Description("Sets the location of a given Revit Element based on a given BHoM IBHoMObject.")]
        [Input("element", "Revit Element to be modified.")]
        [Input("bHoMObject", "BHoM IBHoMObject acting as a source of information about the new location.")]
        [Input("settings", "Revit adapter settings to be used while performing the operation.")]
        [Output("success", "True if location of the input Revit Element has been successfully set.")]
        public static bool ISetLocation(this Element element, IBHoMObject bHoMObject, RevitSettings settings)
        {
            if (element == null || bHoMObject == null)
                return false;

            return SetLocation(element as dynamic, bHoMObject as dynamic, settings);
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static bool SetLocation(this FamilyInstance element, BH.oM.Geometry.Point location, Basis orientation, RevitSettings settings)
        {
            LocationPoint elementLocation = element.Location as LocationPoint;
            if (elementLocation == null)
                return false;

            bool success = false;
            XYZ newLocation = location.ToRevit();

            if (elementLocation.Point.DistanceTo(newLocation) > settings.DistanceTolerance)
            {
                if (element.Host != null)
                {
                    if (element.HostFace == null)
                    {
                        List<Solid> hostSolids = element.Host.Solids(new Options());
                        if (hostSolids != null && hostSolids.All(x => !x.IsContaining(newLocation, settings.DistanceTolerance)))
                            BH.Engine.Base.Compute.RecordWarning($"The new location point used to update the location of a family instance was outside of the host solid, the point has been snapped to the host. ElementId: {element.Id.IntegerValue}");
                    }
                    else
                    {
                        if (element.Host is ReferencePlane)
                        {
                            Autodesk.Revit.DB.Plane p = ((ReferencePlane)element.Host).GetPlane();
                            if (p.Origin.DistanceTo(newLocation) > settings.DistanceTolerance && Math.Abs((p.Origin - newLocation).Normalize().DotProduct(p.Normal)) > settings.AngleTolerance)
                            {
                                BH.Engine.Base.Compute.RecordError($"Location update failed: the new location point used on update of a family instance does not lie in plane with its reference plane. ElementId: {element.Id.IntegerValue}");
                                return false;
                            }
                        }
                        else
                        {
                            Transform linkTransform = null;
                            Element hostElement = element.Host;
                            Reference reference = element.HostFace;
                            if (element.Host is RevitLinkInstance)
                            {
                                hostElement = ((RevitLinkInstance)element.Host).GetLinkDocument().GetElement(element.HostFace.LinkedElementId);
                                reference = element.HostFace.CreateReferenceInLink();
                                linkTransform = ((RevitLinkInstance)element.Host).GetTotalTransform();
                                newLocation = linkTransform.Inverse.OfPoint(newLocation);
                            }

                            Autodesk.Revit.DB.Face face = hostElement.GetGeometryObjectFromReference(reference) as Autodesk.Revit.DB.Face;
                        
                            XYZ toProject = newLocation;
                            Transform instanceTransform = null;
                            if (hostElement is FamilyInstance && !((FamilyInstance)hostElement).HasModifiedGeometry())
                            {
                                instanceTransform = ((FamilyInstance)hostElement).GetTotalTransform();
                                toProject = (instanceTransform.Inverse.OfPoint(newLocation));
                            }

                            IntersectionResult ir = face?.Project(toProject);
                            if (ir == null)
                            {
                                BH.Engine.Base.Compute.RecordError($"Location update failed: the new location point used on update of a family instance could not be placed on its host face. ElementId: {element.Id.IntegerValue}");
                                return false;
                            }

                            newLocation = ir.XYZPoint;
                            if (instanceTransform != null)
                                newLocation = instanceTransform.OfPoint(newLocation);

                            if (linkTransform != null)
                                newLocation = linkTransform.OfPoint(newLocation);

                            if (ir.Distance > settings.DistanceTolerance)
                                BH.Engine.Base.Compute.RecordWarning($"The location point used on update of a family instance has been snapped to its host face. ElementId: {element.Id.IntegerValue}");
                        }
                    }
                }

                elementLocation.Point = newLocation;
                success = true;
            }

            if (orientation?.X != null)
            {
                Transform transform = element.GetTotalTransform();
                XYZ newX = orientation.X.ToRevit().Normalize();
                if (1 - Math.Abs(transform.BasisX.DotProduct(newX)) > settings.AngleTolerance)
                {
                    XYZ revitNormal;
                    XYZ bHoMNormal;
                    FamilySymbol fs = element.Document.GetElement(element.GetTypeId()) as FamilySymbol;
                    if (fs != null && fs.Family.FamilyPlacementType == FamilyPlacementType.OneLevelBasedHosted)
                    {
                        revitNormal = transform.BasisY;
                        bHoMNormal = orientation.Y.ToRevit().Normalize();
                    }
                    else
                    {
                        revitNormal = transform.BasisZ;
                        bHoMNormal = orientation.Z.ToRevit().Normalize();
                    }

                    if (1 - Math.Abs(revitNormal.DotProduct(bHoMNormal)) > settings.AngleTolerance)
                        BH.Engine.Base.Compute.RecordWarning($"The orientation applied to the family instance on update has different normal than the original one. Only in-plane rotation has been applied, the orientation out of plane has been ignored. ElementId: {element.Id.IntegerValue}");

                    double angle = transform.BasisX.AngleOnPlaneTo(newX, revitNormal);
                    if (Math.Abs(angle) > settings.AngleTolerance)
                    {
                        ElementTransformUtils.RotateElement(element.Document, element.Id, Autodesk.Revit.DB.Line.CreateBound(newLocation, newLocation + transform.BasisZ), angle);
                        success = true;
                    }
                }
            }

            return success;
        }


        /***************************************************/
        /****            Private collections            ****/
        /***************************************************/

        private static readonly Type[] AbstractRevitTypes = new Type[]
        {
            typeof(Autodesk.Revit.DB.Family),
            typeof(ElementType),
            typeof(Material),
            typeof(View)
        };

        /***************************************************/
    }
}


