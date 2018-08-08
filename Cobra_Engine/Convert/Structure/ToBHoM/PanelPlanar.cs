using Autodesk.Revit.DB;
using BH.oM.Structural.Elements;
using BH.oM.Structural.Properties;
using System.Collections.Generic;
using BHS = BH.Engine.Structure;
using BH.oM.Base;
using System.Linq;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static List<PanelPlanar> ToBHoMPanelPlanar(this Wall wall, Dictionary<ElementId, List<IBHoMObject>> objects = null, bool copyCustomData = true, bool convertUnits = true)
        {
            IProperty2D aProperty2D = null;
            if (objects != null)
            {
                List<IBHoMObject> aBHoMObjectList = new List<IBHoMObject>();
                if (objects.TryGetValue(wall.WallType.Id, out aBHoMObjectList))
                    if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                        aProperty2D = aBHoMObjectList.First() as IProperty2D;
            }

            if (aProperty2D == null)
            {
                aProperty2D = wall.WallType.ToBHoMProperty2D(objects, copyCustomData, convertUnits) as IProperty2D;
                if (objects != null)
                    objects.Add(wall.WallType.Id, new List<IBHoMObject>(new IBHoMObject[] { aProperty2D }));
            }
            
            List<oM.Geometry.ICurve> outlines = wall.Outlines();
            List<PanelPlanar> aResult = outlines != null ? BHS.Create.PanelPlanar(outlines) : new List<PanelPlanar> { new PanelPlanar { ExternalEdges = new List<oM.Structural.Elements.Edge>(), Openings = new List<oM.Structural.Elements.Opening>(), Property = aProperty2D } };

            for (int i = 0; i < aResult.Count; i++)
            {
                PanelPlanar panel = aResult[i] as PanelPlanar;
                panel.Property = aProperty2D;

                panel = Modify.SetIdentifiers(panel, wall) as PanelPlanar;
                if (copyCustomData)
                    panel = Modify.SetCustomData(panel, wall, convertUnits) as PanelPlanar;
            }

            return aResult;
        }

        /***************************************************/

        internal static List<PanelPlanar> ToBHoMPanelPlanar(this Floor floor, Dictionary<ElementId, List<IBHoMObject>> objects = null, bool copyCustomData = true, bool convertUnits = true)
        {
            IProperty2D aProperty2D = null;
            if (objects != null)
            {
                List<IBHoMObject> aBHoMObjectList = new List<IBHoMObject>();
                if (objects.TryGetValue(floor.FloorType.Id, out aBHoMObjectList))
                    if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                        aProperty2D = aBHoMObjectList.First() as IProperty2D;
            }

            if (aProperty2D == null)
            {
                aProperty2D = floor.FloorType.ToBHoMProperty2D(objects, copyCustomData, convertUnits) as IProperty2D;
                if (objects != null)
                    objects.Add(floor.FloorType.Id, new List<IBHoMObject>(new IBHoMObject[] { aProperty2D }));
            }

            List<oM.Geometry.ICurve> outlines = floor.Outlines();
            List<PanelPlanar> aResult = outlines != null ? BHS.Create.PanelPlanar(outlines) : new List<PanelPlanar> { new PanelPlanar { ExternalEdges = new List<oM.Structural.Elements.Edge>(), Openings = new List<oM.Structural.Elements.Opening>(), Property = aProperty2D } };

            for (int i = 0; i < aResult.Count; i++)
            {
                PanelPlanar panel = aResult[i] as PanelPlanar;
                panel.Property = aProperty2D;

                panel = Modify.SetIdentifiers(panel, floor) as PanelPlanar;
                if (copyCustomData)
                    panel = Modify.SetCustomData(panel, floor, convertUnits) as PanelPlanar;
            }

            return aResult;
        }

        /***************************************************/
    }
}
