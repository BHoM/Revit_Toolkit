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
using System;

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
                    if (IsVerticalNonLinearCurve((location as LocationCurve).Curve))
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

        public static double AdjustedRotationFraming(this FamilyInstance familyInstance, double bhomRotation = double.NaN, bool inverse = false, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            double rotation;
            Curve location = ((LocationCurve)familyInstance.Location).Curve;

            BH.oM.Geometry.ICurve locationCurve = location.IFromRevit();
            if (locationCurve is BH.oM.Geometry.Line)
            {
                Transform transform = familyInstance.GetTotalTransform();
                if (1 - Math.Abs(transform.BasisX.DotProduct(XYZ.BasisZ)) <= settings.AngleTolerance)
                    rotation = XYZ.BasisY.AngleOnPlaneTo(transform.BasisY, transform.BasisX);
                else
                    rotation = XYZ.BasisZ.AngleOnPlaneTo(transform.BasisZ, transform.BasisX);
                
                if (inverse && !double.IsNaN(bhomRotation))
                    rotation = familyInstance.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE) - bhomRotation + rotation;
            }
            else
            {
                if (location.IsVerticalNonLinearCurve())
                    rotation = Math.PI * 0.5 - familyInstance.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE);
                else
                    rotation = -familyInstance.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE);
                
                if (inverse && !double.IsNaN(bhomRotation))
                    rotation = familyInstance.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE) - bhomRotation + rotation;

                if (familyInstance.Mirrored)
                    rotation *= -1;
            }

            return rotation.CheckOrientationAngleDomain();
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

        private static double CheckOrientationAngleDomain(this double orientationAngle)
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
