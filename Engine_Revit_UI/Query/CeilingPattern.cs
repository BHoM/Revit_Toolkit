using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using BH.Engine.Geometry;
using BH.oM.Geometry;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        public static List<Polyline> CeilingPattern(this Ceiling ceiling, PlanarSurface surface)
        {
            CeilingType ceilingType = ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType;

            CompoundStructure comStruct = ceilingType.GetCompoundStructure();

            double lowestX = surface.ExternalBoundary.IControlPoints().Min(x => x.X);
            double lowestY = surface.ExternalBoundary.IControlPoints().Min(x => x.Y);
            double highestX = surface.ExternalBoundary.IControlPoints().Max(x => x.X);
            double highestY = surface.ExternalBoundary.IControlPoints().Max(x => x.Y);
            double z = surface.ExternalBoundary.IControlPoints().Max(x => x.Z);

            List<Polyline> patterns = new List<Polyline>();

            List<ElementId> materialIds = ceiling.GetMaterialIds(false).ToList();

            //foreach (CompoundStructureLayer layer in comStruct.GetLayers())
            foreach(ElementId e in materialIds)
            {
                //Material revitMaterial = ceiling.Document.GetElement(layer.MaterialId) as Material;
                Material revitMaterial = ceiling.Document.GetElement(e) as Material;

                FillPatternElement fillPatternElement = revitMaterial.Document.GetElement(revitMaterial.SurfaceForegroundPatternId) as FillPatternElement;

                if(fillPatternElement != null)
                {
                    FillPattern fillPattern = fillPatternElement.GetFillPattern();
                    if (fillPattern == null || fillPattern.IsSolidFill /*|| fillPattern.Target == FillPatternTarget.Drafting*/)
                        continue; //Skip solid filled patterns or drafting patterns

                    IList<FillGrid> fillGridList = fillPattern.GetFillGrids();
                    foreach(FillGrid grid in fillGridList)
                    {
                        double offset = grid.Offset;

                        double currentY = lowestY;

                        while((currentY + offset) < highestY)
                        {
                            BH.oM.Geometry.Point pt = new oM.Geometry.Point { X = lowestX, Y = currentY + offset, Z = z };
                            BH.oM.Geometry.Point pt2 = new oM.Geometry.Point { X = highestX, Y = currentY + offset, Z = z };

                            Polyline pline = new Polyline { ControlPoints = new List<oM.Geometry.Point> { pt, pt2 } };

                            pline = pline.Rotate(pt, Vector.ZAxis, grid.Angle);

                            patterns.Add(pline);

                            currentY += offset;
                        }
                    }
                }
            }

            return patterns;
        }
    }
}
