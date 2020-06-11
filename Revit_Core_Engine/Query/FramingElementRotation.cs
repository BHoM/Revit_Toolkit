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
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using BH.oM.Physical.FramingProperties;
using System;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static double FramingElementRotation(this FamilyInstance familyInstance, RevitSettings settings)
        {
            double rotation = double.NaN;
            Location location = familyInstance.Location;

            if (location is LocationPoint)
                rotation = Math.PI * 0.5 + (location as LocationPoint).Rotation;
            else if (location is LocationCurve)
            {
                BH.oM.Geometry.ICurve locationCurve = (location as LocationCurve).Curve.IFromRevit();
                if (locationCurve is BH.oM.Geometry.Line)
                {
                    Transform transform = familyInstance.GetTotalTransform();
                    if ((locationCurve as BH.oM.Geometry.Line).IsVertical())
                    {
                        if (familyInstance.IsSlantedColumn)
                            rotation = XYZ.BasisY.AngleOnPlaneTo(transform.BasisX, transform.BasisZ);
                        else
                            rotation = XYZ.BasisY.AngleOnPlaneTo(transform.BasisY, transform.BasisX);
                    }
                    else
                    {
                        if (familyInstance.IsSlantedColumn)
                            rotation = XYZ.BasisZ.AngleOnPlaneTo(transform.BasisY, transform.BasisZ);
                        else
                            rotation = XYZ.BasisZ.AngleOnPlaneTo(transform.BasisZ, transform.BasisX);
                    }
                }
                else
                {
                    if (IsVerticalNonLinearCurve((location as LocationCurve).Curve, settings))
                        rotation = Math.PI * 0.5 - familyInstance.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE);
                    else
                        rotation = -familyInstance.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE);

                    if (familyInstance.Mirrored)
                        rotation *= -1;
                }
            }

            return rotation;
        }

        /***************************************************/

        //public static double AdjustedRotationFraming(this FamilyInstance familyInstance, IFramingElement framingElement, bool local, RevitSettings settings = null)
        //{
        //    settings = settings.DefaultIfNull();

        //    double rotation;
        //    //Curve location = ((LocationCurve)familyInstance.Location).Curve;

        //    //TODO: global rotation as a separate method?

        //    BH.oM.Geometry.ICurve locationCurve = framingElement?.Location;
        //    if (locationCurve == null)
        //        locationCurve = ((LocationCurve)familyInstance.Location).Curve.IFromRevit();

        //    if (locationCurve is BH.oM.Geometry.Line)
        //    {
        //        Transform transform = familyInstance.GetTotalTransform();
        //        XYZ axis;
        //        if (local)
        //            axis = transform.BasisX;
        //        else
        //            axis = ((BH.oM.Geometry.Line)locationCurve).Direction().ToRevit().Normalize();

        //        if (1 - Math.Abs(axis.DotProduct(XYZ.BasisZ)) <= settings.AngleTolerance)
        //            rotation = XYZ.BasisY.AngleOnPlaneTo(transform.BasisY, axis);
        //        else
        //            rotation = XYZ.BasisZ.AngleOnPlaneTo(transform.BasisZ, axis);

        //        if (framingElement?.Property as ConstantFramingProperty != null)
        //            rotation = familyInstance.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE) - ((ConstantFramingProperty)framingElement.Property).OrientationAngle + rotation;
        //    }
        //    else
        //    {
        //        if (locationCurve.IsVerticalNonLinearCurve(settings))
        //            rotation = Math.PI * 0.5 - familyInstance.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE);
        //        else
        //            rotation = -familyInstance.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE);

        //        if (framingElement?.Property as ConstantFramingProperty != null)
        //            rotation = familyInstance.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE) - ((ConstantFramingProperty)framingElement.Property).OrientationAngle + rotation;

        //        if (familyInstance.Mirrored)
        //            rotation *= -1;
        //    }

        //    return rotation.NormalizeAngleDomain();
        //}

        /***************************************************/

        //TODO: THIS WILL NEED TO BE MADE PRIVATE AND MANAGED BY A DISPATCHER FOR ANALYTICALS!
        public static double AdjustedRotation(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            double rotation;
            Curve locationCurve = ((LocationCurve)familyInstance.Location).Curve;

            if (locationCurve is Autodesk.Revit.DB.Line)
            {
                Transform transform = familyInstance.GetTotalTransform();
                if (1 - Math.Abs(transform.BasisX.DotProduct(XYZ.BasisZ)) <= settings.AngleTolerance)
                    rotation = XYZ.BasisY.AngleOnPlaneTo(transform.BasisY, transform.BasisX);
                else
                    rotation = XYZ.BasisZ.AngleOnPlaneTo(transform.BasisZ, transform.BasisX);
            }
            else
            {
                if (locationCurve.IsVerticalNonLinearCurve(settings))
                    rotation = Math.PI * 0.5 - familyInstance.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE);
                else
                    rotation = -familyInstance.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE);

                if (familyInstance.Mirrored)
                    rotation *= -1;
            }

            return rotation.NormalizeAngleDomain();
        }

        public static double AdjustedRotation(this IFramingElement framingElement, FamilyInstance familyInstance, RevitSettings settings = null)
        {
            double rotation = familyInstance.AdjustedRotation(settings);
            rotation = familyInstance.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE) - ((ConstantFramingProperty)framingElement.Property).OrientationAngle + rotation;
            return rotation.NormalizeAngleDomain();
        }

        /***************************************************/

        private static bool IsVerticalNonLinearCurve(this ICurve curve, RevitSettings settings)
        {
            BH.oM.Geometry.Plane plane = curve.IFitPlane();
            if (plane == null)
                return false;

            return Math.Abs(plane.Normal.DotProduct(Vector.ZAxis)) <= settings.AngleTolerance;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        //public static double ToRevitOrientationAngleColumn(this double bhomOrientationAngle, BH.oM.Geometry.Line centreLine)
        //{
        //    //For vertical columns orientation angles are following similar rules between Revit and BHoM but flipped 90 degrees.
        //    if (centreLine.IsVertical())
        //        return CheckOrientationAngleDomain((Math.PI * 0.5 - bhomOrientationAngle));
        //    else
        //        return CheckOrientationAngleDomain(-bhomOrientationAngle);
        //}

        ///***************************************************/

        //public static double ToRevitOrientationAngleBeams(this double bhomOrientationAngle)
        //{
        //    return CheckOrientationAngleDomain(-bhomOrientationAngle);
        //}

        ///***************************************************/

        private static double NormalizeAngleDomain(this double orientationAngle)
        {
            orientationAngle = orientationAngle % (2 * Math.PI);

            if (orientationAngle - BH.oM.Geometry.Tolerance.Angle < -Math.PI * 2)
                orientationAngle += Math.PI * 2;
            else if (orientationAngle + BH.oM.Geometry.Tolerance.Angle > Math.PI * 2)
                orientationAngle -= Math.PI * 2;

            return orientationAngle;
        }

        /***************************************************/
    }
}
