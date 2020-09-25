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
using Autodesk.Revit.DB.Mechanical;
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Physical.Elements;
using BH.oM.Reflection.Attributes;
using System;
using System.ComponentModel;
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

        [Description("Query a Revit duct to extract its orientation angle.")]
        [Input("duct", "Revit duct to be queried.")]
        [Input("settings", "Revit adapter settings.")]
        [Output("double", "Orientation angle of a duct in radians extracted from a Revit duct.")]
        public static double OrientationAngle(this Duct duct, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            double rotation;

            Location location = duct.Location;

            LocationCurve locationCurve = location as LocationCurve;
            Curve curve = locationCurve.Curve;

            BH.oM.Geometry.ICurve bhomCurve = curve.IFromRevit(); // Convert to a BHoM curve

            // Get the duct connector
            Connector connector = null;
            foreach (Connector conn in duct.ConnectorManager.Connectors)
            {
                // Get the End connector for this duct
                if (conn.ConnectorType == ConnectorType.End)
                {
                    connector = conn;
                    break;
                }
            }

            // Coordinate system of the duct connector
            Transform transform = connector.CoordinateSystem;

            // Get the rotation
            if ((bhomCurve as BH.oM.Geometry.Line).IsVertical()) // Is the duct vertical?
                rotation = XYZ.BasisY.AngleOnPlaneTo(transform.BasisX, transform.BasisZ);
            else
                rotation = Math.PI + XYZ.BasisZ.AngleOnPlaneTo(transform.BasisY, transform.BasisZ);

            return rotation.NormalizeAngleDomain();
        }

        /***************************************************/
        
        [Description("Query a Revit cable tray to extract its orientation angle.")]
        [Input("cableTray", "Revit cable tray to be queried.")]
        [Input("settings", "Revit adapter settings.")]
        [Output("double", "Orientation angle of a cable tray in radians extracted from a Revit cable tray.")]
        public static double OrientationAngle(this Autodesk.Revit.DB.Electrical.CableTray cableTray, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            double rotation;

            Location location = cableTray.Location;

            LocationCurve locationCurve = location as LocationCurve;
            Curve curve = locationCurve.Curve;

            BH.oM.Geometry.ICurve bhomCurve = curve.IFromRevit(); // Convert to a BHoM curve

            // Get the cable tray connector
            Connector connector = null;
            foreach (Connector conn in cableTray.ConnectorManager.Connectors)
            {
                // Get the End connector for this cable tray
                if (conn.ConnectorType == ConnectorType.End)
                {
                    connector = conn;
                    break;
                }
            }

            // Coordinate system of the cable tray connector
            Transform transform = connector.CoordinateSystem;

            // Get the rotation
            if ((bhomCurve as BH.oM.Geometry.Line).IsVertical()) // Is the cable tray vertical?
                rotation = XYZ.BasisY.AngleOnPlaneTo(transform.BasisX, transform.BasisZ);
            else
                rotation = Math.PI + XYZ.BasisZ.AngleOnPlaneTo(transform.BasisY, transform.BasisZ);

            return rotation.NormalizeAngleDomain();
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

            return rotation.NormalizeAngleDomain();
        }

        /***************************************************/

        public static double OrientationAngleFraming(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
            double rotation;
            Curve locationCurve = (familyInstance.Location as LocationCurve)?.Curve;
            if (locationCurve == null)
                return double.NaN;

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

        /***************************************************/
    }
}
