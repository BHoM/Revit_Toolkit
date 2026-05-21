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
using Extrusion = Autodesk.Revit.DB.Extrusion;

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
            Document document = padFoundationFamily.Document;
            Document famDoc = null;
            double tol = settings.DistanceTolerance;
            try
            {
                famDoc = document.EditFamily(padFoundationFamily);
                if (famDoc == null)
                    return false;

                Polyline familyOutline = document.ExtrusionOutline(settings);
                if (familyOutline == null || familyOutline.ControlPoints.Count == 0)
                    return false;

                if (familyOutline.ControlPoints.Count != orientedOutline.ControlPoints.Count)
                    return false;

                List<BH.oM.Geometry.Line> bhomEdges = orientedOutline.SubParts().Where(x => x != null && x.Length() > tol).ToList();
                List<BH.oM.Geometry.Line> revitEdges = familyOutline.SubParts().Where(x => x != null && x.Length() > tol).ToList();

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
            finally
            {
                if (famDoc != null && famDoc.IsValidObject)
                    famDoc.Close(false);
            }
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static Polyline ExtrusionOutline(this Document familyDocument, RevitSettings settings)
        {
            Extrusion extrusion = new FilteredElementCollector(familyDocument).OfClass(typeof(Extrusion)).FirstElement() as Extrusion;
            if (extrusion?.Sketch?.Profile?.Size != 1)
                return null;

            CurveArray curveArray = extrusion.Sketch.Profile.get_Item(0);
            List<ICurve> segments = curveArray.FromRevit();
            List<BH.oM.Geometry.Line> lines = segments.OfType<BH.oM.Geometry.Line>().ToList();
            if (segments.Count != lines.Count)
                return null;

            List<Polyline> polylines = lines.Join(settings.DistanceTolerance);
            if (polylines.Count != 1)
                return null;

            return polylines[0];
        }
    }

}
