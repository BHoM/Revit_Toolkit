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

        [Description("Checks whether the given BHoM Polyline outline matches the outline of the family freeform extrusion in Revit.")]
        [Input("family", "Revit family whose extrusion outline should be compared.")]
        [Input("orientedOutline", "BHoM polyline outline to match against the family outline.")]
        [Input("settings", "Revit adapter settings providing distance tolerance for the matching.")]
        [Output("matches", "True if the family extrusion outline matches the input outline.")]
        public static bool IsMatchingOutline(this Family family, Polyline orientedOutline, RevitSettings settings)
        {
            Document document = family.Document;
            Document famDoc = null;
            double tol = settings.DistanceTolerance;
            try
            {
                famDoc = document.EditFamily(family);
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
