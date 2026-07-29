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

        [Description("Checks whether a pile foundation family matches both the pile-cap outline and the pile layout.")]
        [Input("family", "Revit pile foundation family to compare.")]
        [Input("orientedOutline", "Oriented pile-cap outline.")]
        [Input("layout", "Explicit pile layout points.")]
        [Input("settings", "Revit adapter settings.")]
        [Output("matches", "True if outline and layout both match.")]
        public static bool IsMatchingOutlineAndLayout(this Family family, Polyline orientedOutline, ExplicitLayout layout, RevitSettings settings)
        {
            if (family == null || orientedOutline == null || layout?.Points == null)
                return false;

            settings = settings.DefaultIfNull();

            List<oM.Geometry.Point> familyPilePoints = family.PileLayoutPoints(settings);
            if (familyPilePoints == null)
                return false;

            if (familyPilePoints.Count != layout.Points.Count)
                return false;

            double tol = settings.DistanceTolerance;
            foreach (oM.Geometry.Point layoutPt in layout.Points)
            {
                oM.Geometry.Point xy = new oM.Geometry.Point { X = layoutPt.X, Y = layoutPt.Y, Z = 0 };
                if (!familyPilePoints.Any(fp => fp.Distance(xy) <= tol))
                    return false;
            }

            return true;
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static List<oM.Geometry.Point> PileLayoutPoints(this Family family, RevitSettings settings)
        {
            ISet<ElementId> symbolIds = family.GetFamilySymbolIds();
            if (symbolIds == null || symbolIds.Count == 0)
                return null;

            ElementType type = family.Document.GetElement(symbolIds.First()) as ElementType;
            if (type == null)
                return null;

            Options options = new Options()
            {
                ComputeReferences = false,
                DetailLevel = Autodesk.Revit.DB.ViewDetailLevel.Medium,
                IncludeNonVisibleObjects = false
            };

            List<Autodesk.Revit.DB.Face> faces = type.Faces(options);
            if (faces == null || faces.Count == 0)
                return null;

            List<PlanarFace> horizontal = faces.OfType<PlanarFace>()
                .Where(x => x.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ))
                .OrderByDescending(x => x.Area)
                .ToList();

            return horizontal.Skip(1)
                .Select(f =>
                {
                    oM.Geometry.Point p = f.Origin.PointFromRevit();
                    return new oM.Geometry.Point { X = p.X, Y = p.Y, Z = 0 };
                })
                .ToList();
        }

        /***************************************************/
    }
}
