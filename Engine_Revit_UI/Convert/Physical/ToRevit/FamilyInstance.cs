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

using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Physical.Elements;

using BH.Engine.Structure;
using BH.Engine.Geometry;

using System;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static FamilyInstance ToRevitFamilyInstance(this IFramingElement framingElement, Document document, PushSettings pushSettings = null)
        {
            if (framingElement == null || document == null)
                return null;

            if (framingElement is Column)
            {
                return ToRevitFamilyInstance_Column(framingElement, document, pushSettings);
            }
            else if (framingElement is Beam || framingElement is Bracing || framingElement is Cable)
            {
                return ToRevitFamilyInstance_Framing(framingElement, document, pushSettings);
            }
            else if (framingElement is Pile)
            {
                BH.Engine.Reflection.Compute.RecordError(string.Format("Push of pile foundations is not supported in current version of BHoM. BHoM element Guid: {0}", framingElement.BHoM_Guid));
                return null;
            }
            else
            {
                BH.Engine.Reflection.Compute.RecordError(string.Format("Push of {0} is not supported in current version of BHoM. BHoM element Guid: {1}",framingElement.GetType(), framingElement.BHoM_Guid));
                return null;
            }

        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static FamilyInstance ToRevitFamilyInstance_Column(this IFramingElement framingElement, Document document, PushSettings pushSettings = null)
        {
            if (framingElement == null || document == null)
                return null;

            FamilyInstance aFamilyInstance = pushSettings.FindRefObject<FamilyInstance>(document, framingElement.BHoM_Guid);
            if (aFamilyInstance != null)
                return aFamilyInstance;

            pushSettings.DefaultIfNull();

            object aCustomDataValue = null;

            //Check that the curve works for revit
            if (!CheckLocationCurveColumns(framingElement))
                return null;
            
            Line columnLine = framingElement.Location.ToRevitCurve(pushSettings) as Line;
            Level aLevel = null;

            aCustomDataValue = framingElement.CustomDataValue("Base Level");
            if (aCustomDataValue != null && aCustomDataValue is int)
            {
                ElementId aElementId = new ElementId((int)aCustomDataValue);
                aLevel = document.GetElement(aElementId) as Level;
            }

            if (aLevel == null)
                aLevel = Query.BottomLevel(framingElement.Location, document, pushSettings.ConvertUnits);

            FamilySymbol aFamilySymbol = framingElement.Property.ToRevitFamilySymbol_Column(document, pushSettings);

            if (aFamilySymbol == null)
            {
                aFamilySymbol = Query.ElementType(framingElement, document, BuiltInCategory.OST_StructuralColumns, pushSettings.FamilyLoadSettings) as FamilySymbol;

                if (aFamilySymbol == null)
                {
                    Compute.ElementTypeNotFoundWarning(framingElement);
                    return null;
                }
            }

            FamilyPlacementType aFamilyPlacementType = aFamilySymbol.Family.FamilyPlacementType;
            if (aFamilyPlacementType != FamilyPlacementType.CurveBased && aFamilyPlacementType != FamilyPlacementType.CurveBasedDetail && aFamilyPlacementType != FamilyPlacementType.CurveDrivenStructural && aFamilyPlacementType != FamilyPlacementType.TwoLevelsBased)
            {
                Compute.InvalidFamilyPlacementTypeWarning(framingElement, aFamilySymbol);
                return null;
            }

            aFamilyInstance = document.Create.NewFamilyInstance(columnLine, aFamilySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.Column);

            aFamilyInstance.CheckIfNullPush(framingElement);
            if (aFamilyInstance == null)
                return null;

            oM.Physical.FramingProperties.ConstantFramingProperty barProperty = framingElement.Property as oM.Physical.FramingProperties.ConstantFramingProperty;
            if (barProperty != null)
            {
                double orientationAngle = ToRevitOrientationAngleColumn(barProperty.OrientationAngle, framingElement.Location as oM.Geometry.Line);
                Parameter aParameter = aFamilyInstance.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE);
                if (aParameter != null && !aParameter.IsReadOnly)
                    aParameter.Set(orientationAngle);

                //TODO: if the material does not get assigned an error should be thrown?
                if (barProperty.Material != null)
                {
                    Autodesk.Revit.DB.Material material = document.GetElement(new ElementId(BH.Engine.Adapters.Revit.Query.ElementId(barProperty.Material))) as Autodesk.Revit.DB.Material;
                    if (material != null)
                        aFamilyInstance.StructuralMaterialId = material.Id;
                }
            }
            
            if (1 - Math.Abs(columnLine.Direction.DotProduct(XYZ.BasisZ)) < BH.oM.Geometry.Tolerance.Angle)
            {
                document.Regenerate();
                aFamilyInstance.TrySetParameter(BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM, 0);

                aFamilyInstance.TrySetParameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM, columnLine.Origin.Z - aLevel.Elevation);
                aFamilyInstance.TrySetParameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM, columnLine.Origin.Z + columnLine.Length - aLevel.Elevation);
                document.Regenerate();
            }
            

            if (pushSettings.CopyCustomData)
            {
                BuiltInParameter[] paramsToIgnore = new BuiltInParameter[]
                {
                    //BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM,
                    //BuiltInParameter.FAMILY_BASE_LEVEL_PARAM,
                    //BuiltInParameter.SCHEDULE_LEVEL_PARAM,
                    //BuiltInParameter.FAMILY_TOP_LEVEL_PARAM,
                    //BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM,
                    BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM,
                    BuiltInParameter.SCHEDULE_BASE_LEVEL_OFFSET_PARAM,
                    BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM,
                    BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM,
                    BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM,
                    BuiltInParameter.ELEM_FAMILY_PARAM,
                    BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
                    BuiltInParameter.ELEM_TYPE_PARAM,
                    BuiltInParameter.ALL_MODEL_IMAGE,
                    BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE,
                    BuiltInParameter.STRUCTURAL_MATERIAL_PARAM
                };
                Modify.SetParameters(aFamilyInstance, framingElement, paramsToIgnore, pushSettings.ConvertUnits);
            }

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(framingElement, aFamilyInstance);

            return aFamilyInstance;
        }

        /***************************************************/

        private static bool CheckLocationCurveColumns(IFramingElement framingElement)
        {

            if ((framingElement.Location is BH.oM.Geometry.Line))
            {
                BH.oM.Geometry.Line line = framingElement.Location as BH.oM.Geometry.Line;

                if (line.Start.Z >= line.End.Z)
                {
                    BH.Engine.Reflection.Compute.RecordError(string.Format("Start point of revit columns need to have a lower elevation than the end point. Have a look at flipping your location curves. Failing for object with BHoMGuid: {0}", framingElement.BHoM_Guid));
                    return false;
                }
                return true;
            }
            else
            {
                BH.Engine.Reflection.Compute.RecordError(string.Format("Revit does only support Line based columns. Try pushing your element as a beam instead. Failing for object with BHoMGuid: {0}", framingElement.BHoM_Guid));
                return false;
            }
        }

        /***************************************************/

        private static double ToRevitOrientationAngleColumn(double bhomOrientationAngle, BH.oM.Geometry.Line centreLine)
        {
            //For vertical columns orientation angles are following similar rules between Revit and BHoM but flipped 90 degrees
            if (BH.Engine.Structure.Query.IsVertical(centreLine))
            {
                return CheckOrientationAngleDomain((Math.PI * 0.5 - bhomOrientationAngle));
            }
            else
            {
                return CheckOrientationAngleDomain(-bhomOrientationAngle);

            }
        }

        /***************************************************/

        private static FamilyInstance ToRevitFamilyInstance_Framing(this IFramingElement framingElement, Document document, PushSettings pushSettings = null)
        {
            if (framingElement == null || document == null)
                return null;

            FamilyInstance aFamilyInstance = pushSettings.FindRefObject<FamilyInstance>(document, framingElement.BHoM_Guid);
            if (aFamilyInstance != null)
                return aFamilyInstance;

            pushSettings.DefaultIfNull();

            object aCustomDataValue = null;

            //Update justification based on custom data
            BH.oM.Geometry.ICurve adjustedLocation = BH.Engine.Adapters.Revit.Query.AdjustedLocation(framingElement);
            Curve revitCurve = adjustedLocation.ToRevitCurve(pushSettings);

            //TODO: Replace the line below with line above, remember about updating justification params 
            //Curve revitCurve = framingElement.Location.ToRevitCurve(pushSettings);




            bool isVertical, isLinear;
            //Check if curve is planar, and if so, if it is vertical. This is used to determine if the orientation angle needs
            //To be subtracted by 90 degrees or not.
            if (!CheckLocationCurveBeams(framingElement, revitCurve, out isVertical, out isLinear))
                return null;

            Level aLevel = null;

            aCustomDataValue = framingElement.CustomDataValue("Reference Level");
            if (aCustomDataValue != null && aCustomDataValue is int)
            {
                ElementId aElementId = new ElementId((int)aCustomDataValue);
                aLevel = document.GetElement(aElementId) as Level;
            }

            if (aLevel == null)
                aLevel = Query.BottomLevel(framingElement.Location, document, pushSettings.ConvertUnits);

            FamilySymbol aFamilySymbol = framingElement.Property.ToRevitFamilySymbol_Framing(document, pushSettings);

            if (aFamilySymbol == null)
            {
                aFamilySymbol = Query.ElementType(framingElement, document, BuiltInCategory.OST_StructuralFraming, pushSettings.FamilyLoadSettings) as FamilySymbol;

                if (aFamilySymbol == null)
                {
                    Compute.ElementTypeNotFoundWarning(framingElement);
                    return null;
                }
            }

            FamilyPlacementType aFamilyPlacementType = aFamilySymbol.Family.FamilyPlacementType;
            if (aFamilyPlacementType != FamilyPlacementType.CurveBased && aFamilyPlacementType != FamilyPlacementType.CurveBasedDetail && aFamilyPlacementType != FamilyPlacementType.CurveDrivenStructural && aFamilyPlacementType != FamilyPlacementType.TwoLevelsBased)
            {
                Compute.InvalidFamilyPlacementTypeWarning(framingElement, aFamilySymbol);
                return null;
            }

            if (framingElement is Beam)
                aFamilyInstance = document.Create.NewFamilyInstance(revitCurve, aFamilySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
            else if (framingElement is Bracing || framingElement is Cable)
                aFamilyInstance = document.Create.NewFamilyInstance(revitCurve, aFamilySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.Brace);
            else
                aFamilyInstance = document.Create.NewFamilyInstance(revitCurve, aFamilySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.UnknownFraming);

            aFamilyInstance.CheckIfNullPush(framingElement);
            if (aFamilyInstance == null)
                return null;

            oM.Physical.FramingProperties.ConstantFramingProperty barProperty = framingElement.Property as oM.Physical.FramingProperties.ConstantFramingProperty;
            if (barProperty != null)
            {
                double orientationAngle = ToRevitOrientationAngleBeams(barProperty.OrientationAngle, isVertical, isLinear);
                Parameter aParameter = aFamilyInstance.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE);
                if (aParameter != null && !aParameter.IsReadOnly)
                    aParameter.Set(orientationAngle);

                //TODO: if the material does not get assigned an error should be thrown?
                if (barProperty.Material != null)
                {
                    Autodesk.Revit.DB.Material material = document.GetElement(new ElementId(BH.Engine.Adapters.Revit.Query.ElementId(barProperty.Material))) as Autodesk.Revit.DB.Material;
                    if (material != null)
                        aFamilyInstance.StructuralMaterialId = material.Id;
                }
            }

            //Sets the insertion point to the centroid. 
            //TODO: Remove this once FramingElement.AdjustedLocation() query method is added.
            Parameter zJustification = aFamilyInstance.get_Parameter(BuiltInParameter.Z_JUSTIFICATION);
            if (zJustification != null && !zJustification.IsReadOnly)
                zJustification.Set((int)Autodesk.Revit.DB.Structure.ZJustification.Origin);

            if (pushSettings.CopyCustomData)
            {
                BuiltInParameter[] paramsToIgnore = new BuiltInParameter[]
                {
                    BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION,
                    BuiltInParameter.STRUCTURAL_BEAM_END1_ELEVATION,
                    BuiltInParameter.ELEM_FAMILY_PARAM,
                    BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
                    BuiltInParameter.ELEM_TYPE_PARAM,
                    BuiltInParameter.ALL_MODEL_IMAGE,
                    BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE,
                    BuiltInParameter.STRUCTURAL_MATERIAL_PARAM,
                    BuiltInParameter.Y_OFFSET_VALUE,
                    BuiltInParameter.Z_OFFSET_VALUE,
                    BuiltInParameter.START_Y_OFFSET_VALUE,
                    BuiltInParameter.END_Y_OFFSET_VALUE,
                    BuiltInParameter.START_Z_OFFSET_VALUE,
                    BuiltInParameter.END_Z_OFFSET_VALUE,

                    //TODO: remove the above once FramingElement.AdjustedLocation() query method is added.
                    //BuiltInParameter.YZ_JUSTIFICATION,
                    //BuiltInParameter.Y_JUSTIFICATION,
                    //BuiltInParameter.Z_JUSTIFICATION,
                    //BuiltInParameter.START_Y_JUSTIFICATION,
                    //BuiltInParameter.END_Y_JUSTIFICATION,
                    //BuiltInParameter.START_Z_JUSTIFICATION,
                    //BuiltInParameter.END_Z_JUSTIFICATION
                };
                Modify.SetParameters(aFamilyInstance, framingElement, paramsToIgnore, pushSettings.ConvertUnits);
            }

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(framingElement, aFamilyInstance);

            return aFamilyInstance;
        }

        /***************************************************/

        private static bool CheckLocationCurveBeams(IFramingElement framingElement, Curve revitCurve, out bool isVertical, out bool isLinear)
        {
            isLinear = framingElement.Location is BH.oM.Geometry.Line;

            //Line-based elements are handled slightly differently for orientation angles check used later
            if (isLinear)
            {
                isVertical = BH.Engine.Structure.Query.IsVertical(framingElement.Location as BH.oM.Geometry.Line);
                return true;
            }
            else
            {
                //Revit framing elements can only handle planar curves.
                CurveLoop curveLoop = CurveLoop.Create(new Curve[] { revitCurve });
                if (curveLoop.HasPlane())
                {
                    Plane curvePlane = curveLoop.GetPlane();
                    //Orientation angles are handled slightly differently for framing elements that have a curve fits in a plane that contains the z-vector
                    isVertical = Math.Abs(curvePlane.Normal.DotProduct(XYZ.BasisZ)) < BH.oM.Geometry.Tolerance.Angle;
                    return true;
                }
                else
                {
                    isVertical = false;
                    BH.Engine.Reflection.Compute.RecordError(string.Format("Revit framing elements does only support planar curves. Failing for object with BHoMGuid: {0}", framingElement.BHoM_Guid));
                    return false;
                }
            }
        }

        /***************************************************/

        private static double ToRevitOrientationAngleBeams(double bhomOrientationAngle, bool isVertical, bool isLinear)
        {
            return CheckOrientationAngleDomain(-bhomOrientationAngle);
        }

        /***************************************************/

        private static double CheckOrientationAngleDomain(double orientationAngle)
        {
            //Fixes orientation angle excedening +- 2 PI
            orientationAngle = orientationAngle % (2 * Math.PI);

            //The above should be enough, but bue to some tolerance issues going into revit it can sometimes still give errors.
            //The below is added as an extra saftey check
            if (orientationAngle - BH.oM.Geometry.Tolerance.Angle < -Math.PI * 2)
                return orientationAngle + Math.PI * 2;
            else if (orientationAngle + BH.oM.Geometry.Tolerance.Angle > Math.PI * 2)
                return orientationAngle - Math.PI * 2;

            return orientationAngle;
        }

        /***************************************************/
    }
}