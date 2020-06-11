﻿/*
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
using BH.oM.Physical.Elements;
using System;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static double OrientationAngle(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();
            double rotation = double.NaN;

            if (typeof(Column).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                rotation = familyInstance.OrientationAngleColumn(settings);
            else if (typeof(IFramingElement).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                rotation = familyInstance.OrientationAngleFraming(settings);

            return rotation;
        }
        
        /***************************************************/

        public static double OrientationAngleColumn(this FamilyInstance familyInstance, RevitSettings settings)
        {
            double rotation = double.NaN;
            Location location = familyInstance.Location;

            if (location is LocationPoint)
                rotation = Math.PI * 0.5 + (location as LocationPoint).Rotation;
            else if (location is LocationCurve)
            {
                BH.oM.Geometry.ICurve locationCurve = (location as LocationCurve).Curve.IFromRevit();
                Transform transform = familyInstance.GetTotalTransform();
                if ((locationCurve as BH.oM.Geometry.Line).IsVertical())
                    rotation = XYZ.BasisY.AngleOnPlaneTo(transform.BasisX, transform.BasisZ);
                else
                    rotation = XYZ.BasisZ.AngleOnPlaneTo(transform.BasisY, transform.BasisZ);
            }

            return rotation;
        }

        /***************************************************/
        
        public static double OrientationAngleFraming(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
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

            return rotation.NormalizeAngleDomain(settings);
        }
        
        /***************************************************/
    }
}