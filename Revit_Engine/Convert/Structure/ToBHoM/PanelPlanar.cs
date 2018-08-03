using Autodesk.Revit.DB;
using BH.oM.Base;
using BH.oM.Structural.Elements;
using BH.oM.Structural.Properties;
using System;
using System.Collections.Generic;
using BHS = BH.Engine.Structure;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static List<PanelPlanar> ToBHoMPanelPlanar(this Wall wall, bool copyCustomData = true, bool convertUnits = true)
        {
            string materialGrade = wall.MaterialGrade();
            IProperty2D aProperty2D = wall.WallType.ToBHoMProperty2D(materialGrade, copyCustomData, convertUnits) as IProperty2D;

            List<oM.Geometry.ICurve> outlines = wall.Outlines();

            //TODO: Remove this hack when Nurbs are properly implemented!
            bool isNurb = false;
            foreach (oM.Geometry.ICurve c in outlines)
            {
                if (c is oM.Geometry.NurbCurve)
                {
                    isNurb = true;
                    Reflection.Compute.RecordError(string.Format("The panel outline contains a nurbs curve, which is not supported in BHoM 2.0, it is returned with empty geometry. Element Id: {0}", wall.Id.IntegerValue));
                    break;
                }
            }

            List<PanelPlanar> aResult = isNurb ? new List<PanelPlanar> { new PanelPlanar { ExternalEdges = new List<oM.Structural.Elements.Edge>(), Openings = new List<oM.Structural.Elements.Opening>(), Property = aProperty2D } } : BHS.Create.PanelPlanar(outlines);

            for (int i = 0; i < aResult.Count; i++)
            {
                PanelPlanar panel = aResult[i] as PanelPlanar;
                panel.Property = aProperty2D;

                panel = Modify.SetIdentifiers(panel, wall) as PanelPlanar;
                if (copyCustomData)
                {
                    panel = Modify.SetCustomData(panel, wall, convertUnits) as PanelPlanar;
                }
            }

            return aResult;
        }

        /***************************************************/

        internal static List<PanelPlanar> ToBHoMPanelPlanar(this Floor floor, bool copyCustomData = true, bool convertUnits = true)
        {
            string materialGrade = floor.MaterialGrade();
            IProperty2D aProperty2D = floor.FloorType.ToBHoMProperty2D(copyCustomData, convertUnits, materialGrade) as IProperty2D;

            List<oM.Geometry.ICurve> outlines = floor.Outlines();

            //TODO: Remove this hack when Nurbs are properly implemented!
            bool isNurb = false;
            foreach (oM.Geometry.ICurve c in outlines)
            {
                if (c is oM.Geometry.NurbCurve)
                {
                    isNurb = true;
                    Reflection.Compute.RecordError(string.Format("The panel outline contains a nurbs curve, which is not supported in BHoM 2.0, it is returned with empty geometry. Element Id: {0}", floor.Id.IntegerValue));
                    break;
                }
            }

            List<PanelPlanar> aResult = isNurb ? new List<PanelPlanar> { new PanelPlanar { ExternalEdges = new List<oM.Structural.Elements.Edge>(), Openings = new List<oM.Structural.Elements.Opening>(), Property = aProperty2D } } : BHS.Create.PanelPlanar(outlines);

            for (int i = 0; i < aResult.Count; i++)
            {
                PanelPlanar panel = aResult[i] as PanelPlanar;
                panel.Property = aProperty2D;

                panel = Modify.SetIdentifiers(panel, floor) as PanelPlanar;
                if (copyCustomData)
                {
                    panel = Modify.SetCustomData(panel, floor, convertUnits) as PanelPlanar;
                }
            }

            return aResult;
        }

        /***************************************************/
    }
}
