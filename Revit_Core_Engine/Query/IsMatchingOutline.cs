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
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
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

        [Description("Checks whether the given BHoM Polyline outline matches the outline of the padFoundationFamily freeform extrusion in Revit.")]
        [Input("padFoundationFamily", "Revit padFoundationFamily whose extrusion outline should be compared.")]
        [Input("orientedOutline", "BHoM polyline outline to match against the padFoundationFamily outline.")]
        [Input("settings", "Revit adapter settings providing distance tolerance for the matching.")]
        [Output("matches", "True if the padFoundationFamily extrusion outline matches the input outline.")]
        public static bool IsMatchingOutline(this Family padFoundationFamily, Polyline orientedOutline, RevitSettings settings)
        {
            double tol = settings.DistanceTolerance;
            try
            {
                List<BH.oM.Geometry.Line> bhomEdges = orientedOutline.SubParts().Where(x => x != null && x.Length() > tol).ToList();
                List<BH.oM.Geometry.Line> revitEdges = padFoundationFamily.ExtrusionEdges(settings);

                if (bhomEdges.Count != revitEdges.Count)
                    return false;

                for (int i = 0; i < bhomEdges.Count; i++)
                {
                    BH.oM.Geometry.Line bhomEdge = bhomEdges[i];
                    bool edgeMatch = false;
                    foreach (oM.Geometry.Line revitEdge in revitEdges)
                    {
                        oM.Geometry.Point bhomStart = bhomEdge.Start;
                        oM.Geometry.Point bhomEnd = bhomEdge.End;
                        oM.Geometry.Point revitStart = revitEdge.Start;
                        oM.Geometry.Point revitEnd = revitEdge.End;
                        if ((bhomStart.Distance(revitStart) <= tol && bhomEnd.Distance(revitEnd) <= tol)
                            || (bhomStart.Distance(revitEnd) <= tol && bhomEnd.Distance(revitStart) <= tol))
                        {
                            edgeMatch = true;
                            break;
                        }
                    }

                    if (!edgeMatch)
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static List<BH.oM.Geometry.Line> ExtrusionEdges(this Family family, RevitSettings settings)
        {
            ISet<ElementId> symbolIds = family.GetFamilySymbolIds();
            if (symbolIds == null || symbolIds.Count == 0)
                return null;

            ElementType type = family.Document.GetElement(symbolIds.First()) as ElementType;

            Options options = new Options()
            {
                ComputeReferences = false,
                DetailLevel = Autodesk.Revit.DB.ViewDetailLevel.Medium,
                IncludeNonVisibleObjects = false
            };

            List<Autodesk.Revit.DB.Face> faces = type.Faces(options);
            List<PlanarFace> tops = faces.OfType<PlanarFace>().Where(x => x.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ)).ToList();
            if (tops == null || tops.Count != 1)
                return null;

            IList<CurveLoop> outlines = tops[0].GetEdgesAsCurveLoops();
            if (outlines == null || outlines.Count != 1)
                return null;

            List<ICurve> edges = outlines[0].FromRevit().SubParts();
            List<oM.Geometry.Line> lines = edges.OfType<BH.oM.Geometry.Line>().ToList();
            if (edges.Count != lines.Count)
                return null;

            return lines.Where(x => x.Length() > settings.DistanceTolerance).Select(x => x.Project(new oM.Geometry.Plane())).ToList();
        }
    }
}
