/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using BH.oM.Geometry;
using BH.oM.Spatial.Layouts;
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

        [Description("Checks whether a pile foundation family matches the pile-cap outline, pile layout and pile diameter.")]
        [Input("family", "Revit pile foundation family to compare.")]
        [Input("orientedOutline", "Oriented pile-cap outline.")]
        [Input("layout", "Explicit pile layout points.")]
        [Input("diameter", "Pile diameter to match against the family.")]
        [Input("settings", "Revit adapter settings.")]
        [Output("matches", "True if outline, layout and diameter all match.")]
        public static bool IsMatchingOutlineAndLayout(this Family family, Polyline orientedOutline, ExplicitLayout layout, double diameter, RevitSettings settings)
        {
            if (family == null || orientedOutline == null || layout?.Points == null || diameter <= 0)
                return false;

            settings = settings.DefaultIfNull();
            double tol = settings.DistanceTolerance;

            if (!family.IsMatchingOutline(orientedOutline, settings))
                return false;

            if (!family.IsMatchingPileLayout(layout, tol))
                return false;

            if (!family.IsMatchingPileDiameter(diameter, tol))
                return false;

            return true;
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static bool IsMatchingPileLayout(this Family family, ExplicitLayout layout, double tol)
        {
            List<oM.Geometry.Point> familyPilePoints = family.PileLayoutPoints();
            if (familyPilePoints == null || familyPilePoints.Count != layout.Points.Count)
                return false;

            foreach (oM.Geometry.Point layoutPt in layout.Points)
            {
                oM.Geometry.Point xy = new oM.Geometry.Point { X = layoutPt.X, Y = layoutPt.Y, Z = 0 };
                if (!familyPilePoints.Any(fp => fp.Distance(xy) <= tol))
                    return false;
            }

            return true;
        }

        /***************************************************/

        private static bool IsMatchingPileDiameter(this Family family, double diameter, double tol)
        {
            Document doc = family.Document;
            FamilyInstance host = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>().FirstOrDefault(fi => fi.Symbol?.Family?.Id == family.Id);
            if (host == null)
                return false;
            FamilyInstance nest = host.GetSubComponentIds().Select(id => doc.GetElement(id) as FamilyInstance).FirstOrDefault(fi => fi != null);
            FamilySymbol nestSymbol = nest?.Symbol;
            if (nestSymbol == null)
                return false;
            Parameter radiusParam = nestSymbol.Parameters.Cast<Parameter>().FirstOrDefault(p => p.Definition.Name.EndsWith("Radius"));
            if (radiusParam != null && radiusParam.HasValue)
            {
                double radius = radiusParam.AsDouble().ToSI(radiusParam.Definition.GetDataType());
                return Math.Abs(radius * 2.0 - diameter) <= tol;
            }

            Parameter diameterParam = nestSymbol.Parameters.Cast<Parameter>().FirstOrDefault(p => p.Definition.Name.EndsWith("Diameter"));
            if (diameterParam != null && diameterParam.HasValue)
            {
                double nestDiameter = diameterParam.AsDouble().ToSI(diameterParam.Definition.GetDataType());
                return Math.Abs(nestDiameter - diameter) <= tol;
            }
            return false;
        }

        /***************************************************/

        private static List<oM.Geometry.Point> PileLayoutPoints(this Family family)
        {
            Document doc = family.Document;

            FamilyInstance host = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .FirstOrDefault(fi => fi.Symbol?.Family?.Id == family.Id);

            if (host == null)
                return null;

            Transform toFamily = host.GetTotalTransform().Inverse;
            List<oM.Geometry.Point> points = new List<oM.Geometry.Point>();

            foreach (ElementId id in host.GetSubComponentIds())
            {
                FamilyInstance nest = doc.GetElement(id) as FamilyInstance;
                XYZ projectPt = (nest?.Location as LocationPoint)?.Point;
                if (projectPt == null)
                    continue;

                XYZ local = toFamily.OfPoint(projectPt);
                oM.Geometry.Point p = local.PointFromRevit();
                points.Add(new oM.Geometry.Point { X = p.X, Y = p.Y, Z = 0 });
            }

            return points.Count > 0 ? points : null;
        }

        /***************************************************/
    }
}
