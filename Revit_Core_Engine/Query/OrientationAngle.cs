/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using BH.oM.Physical.Elements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts a BHoM-representative orientation angle from a given Revit family instance.")]
        [Input("familyInstance", "Revit family instance to extract the orientation angle from.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("angle", "BHoM-representative orientation angle extracted from the input Revit family instance.")]
        public static double OrientationAngle(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();
            double rotation = double.NaN;

            if (familyInstance is Mullion)
                return familyInstance.OrientationAngleMullion(settings);

            BuiltInCategory category = (BuiltInCategory)familyInstance.Category.Id.Value();
            if (typeof(Column).BuiltInCategories().Contains(category))
                rotation = familyInstance.OrientationAngleColumn(settings);
            else if (typeof(Pile).BuiltInCategories().Contains(category))
                rotation = familyInstance.OrientationAnglePile(settings);
            else if (familyInstance is Mullion || typeof(IFramingElement).BuiltInCategories().Contains(category))
                rotation = familyInstance.OrientationAngleFraming(settings);

            return rotation;
        }

        /***************************************************/

        [Description("Query a Revit MEPCurve to extract its orientation angle.")]
        [Input("mepCurve", "Revit MEPCurve (cable trays, ducts and pipes) to be queried.")]
        [Input("settings", "Revit adapter settings.")]
        [Output("double", "Orientation angle of a MEPCurve in radians extracted from a Revit MEPCurve.")]
        public static double OrientationAngle(this MEPCurve mepCurve, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            Curve curve = (mepCurve.Location as LocationCurve)?.Curve;
            if (curve == null)
            {
                BH.Engine.Base.Compute.RecordError($"Cannot query orientation of a MEP element without a valid location curve. ElementId: {mepCurve.Id.Value()}");
                return double.NaN;
            }

            BH.oM.Geometry.ICurve bhomCurve = curve.IFromRevit(); // Convert to a BHoM curve

            List<Connector> endConnectors = mepCurve.ConnectorsOfType(ConnectorType.End);
            if (endConnectors.Count != 2)
            {
                BH.Engine.Base.Compute.RecordError($"Cannot query orientation of a MEP element without two end connectors. ElementId: {mepCurve.Id.Value()}");
                return double.NaN;
            }

            XYZ endPoint = curve.GetEndPoint(0);
            endConnectors = endConnectors.OrderBy(x => x.Origin.DistanceTo(endPoint)).ToList();
            Connector endConnector = endConnectors[0];

            // Coordinate system of the connector
            Transform transform = endConnector.CoordinateSystem;

            // Get the rotation
            double rotation;
            if ((bhomCurve as BH.oM.Geometry.Line).IsVertical()) // Is the duct vertical?
                rotation = transform.BasisX.AngleOnPlaneTo(XYZ.BasisY, transform.BasisZ);
            else
            {
                rotation = XYZ.BasisZ.AngleOnPlaneTo(transform.BasisY, transform.BasisZ);

                bool positiveDir;
                double xDifference = endConnectors[1].Origin.X - endConnectors[0].Origin.X;
                if (Math.Abs(xDifference) < 1e-6)
                    positiveDir = endConnectors[1].Origin.Y > endConnectors[0].Origin.Y;
                else
                    positiveDir = xDifference > 0;

                if (positiveDir)
                    rotation *= -1;
            }

            return rotation.NormalizeAngleDomain();
        }

        /***************************************************/

        [Description("Extracts a BHoM-representative column orientation angle from a given Revit family instance.")]
        [Input("familyInstance", "Revit family instance to extract the orientation angle from.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("angle", "BHoM-representative column orientation angle extracted from the input Revit family instance.")]
        public static double OrientationAngleColumn(this FamilyInstance familyInstance, RevitSettings settings)
        {
            settings = settings.DefaultIfNull();

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

        [Description("Extracts a BHoM-representative pile orientation angle from a given Revit family instance.")]
        [Input("familyInstance", "Revit family instance to extract the orientation angle from.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("angle", "BHoM-representative pile orientation angle extracted from the input Revit family instance.")]
        public static double OrientationAnglePile(this FamilyInstance familyInstance, RevitSettings settings)
        {
            return (familyInstance.Location as LocationPoint).Rotation.NormalizeAngleDomain();
        }

        /***************************************************/

        [Description("Extracts a BHoM-representative framing orientation angle from a given Revit family instance.")]
        [Input("familyInstance", "Revit family instance to extract the orientation angle from.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("angle", "BHoM-representative framing orientation angle extracted from the input Revit family instance.")]
        public static double OrientationAngleFraming(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
            double rotation;
            Curve locationCurve = (familyInstance.Location as LocationCurve)?.Curve;
            if (locationCurve == null)
                return double.NaN;

            settings = settings.DefaultIfNull();

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

        [Description("Extracts a BHoM-representative mullion orientation angle from a given Revit family instance.")]
        [Input("familyInstance", "Revit family instance to extract the orientation angle from.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("angle", "BHoM-representative mullion orientation angle extracted from the input Revit family instance.")]
        public static double OrientationAngleMullion(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
            double rotation;
            settings = settings.DefaultIfNull();

            Transform transform = familyInstance.GetTotalTransform();
            if (1 - Math.Abs(transform.BasisZ.DotProduct(XYZ.BasisZ)) <= settings.AngleTolerance)
                rotation = XYZ.BasisY.AngleOnPlaneTo(transform.BasisY, transform.BasisZ);
            else
                rotation = XYZ.BasisZ.AngleOnPlaneTo(transform.BasisX, transform.BasisZ);

            return rotation.NormalizeAngleDomain();
        }

        /***************************************************/
    }
}



