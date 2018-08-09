using Autodesk.Revit.DB;
using BH.oM.Base;
using BH.oM.Revit;
using BH.oM.Structural.Elements;
using BH.oM.Structural.Properties;
using System.Collections.Generic;
using System.Linq;
using BHS = BH.Engine.Structure;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static List<PanelPlanar> ToBHoMPanelPlanar(this Wall wall, PullSettings pullSettings = null)
        {
            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            IProperty2D aProperty2D = null;
            if (pullSettings.RefObjects != null)
            {
                List<IBHoMObject> aBHoMObjectList = new List<IBHoMObject>();
                if (pullSettings.RefObjects.TryGetValue(wall.WallType.Id.IntegerValue, out aBHoMObjectList))
                    if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                        aProperty2D = aBHoMObjectList.First() as IProperty2D;
            }

            if (aProperty2D == null)
            {
                aProperty2D = wall.WallType.ToBHoMProperty2D(pullSettings) as IProperty2D;
                if (pullSettings.RefObjects != null)
                    pullSettings.RefObjects.Add(wall.WallType.Id.IntegerValue, new List<IBHoMObject>(new IBHoMObject[] { aProperty2D }));
            }
            
            List<oM.Geometry.ICurve> outlines = wall.Outlines();
            List<PanelPlanar> aResult = outlines != null ? BHS.Create.PanelPlanar(outlines) : new List<PanelPlanar> { new PanelPlanar { ExternalEdges = new List<oM.Structural.Elements.Edge>(), Openings = new List<oM.Structural.Elements.Opening>(), Property = aProperty2D } };

            for (int i = 0; i < aResult.Count; i++)
            {
                PanelPlanar panel = aResult[i] as PanelPlanar;
                panel.Property = aProperty2D;

                panel = Modify.SetIdentifiers(panel, wall) as PanelPlanar;
                if (pullSettings.CopyCustomData)
                    panel = Modify.SetCustomData(panel, wall, pullSettings.ConvertUnits) as PanelPlanar;
            }

            return aResult;
        }

        /***************************************************/

        internal static List<PanelPlanar> ToBHoMPanelPlanar(this Floor floor, PullSettings pullSettings = null)
        {
            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            IProperty2D aProperty2D = null;
            if (pullSettings.RefObjects != null)
            {
                List<IBHoMObject> aBHoMObjectList = new List<IBHoMObject>();
                if (pullSettings.RefObjects.TryGetValue(floor.FloorType.Id.IntegerValue, out aBHoMObjectList))
                    if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                        aProperty2D = aBHoMObjectList.First() as IProperty2D;
            }

            if (aProperty2D == null)
            {
                aProperty2D = floor.FloorType.ToBHoMProperty2D(pullSettings) as IProperty2D;
                if (pullSettings.RefObjects != null)
                    pullSettings.RefObjects.Add(floor.FloorType.Id.IntegerValue, new List<IBHoMObject>(new IBHoMObject[] { aProperty2D }));
            }

            List<oM.Geometry.ICurve> outlines = floor.Outlines();
            List<PanelPlanar> aResult = outlines != null ? BHS.Create.PanelPlanar(outlines) : new List<PanelPlanar> { new PanelPlanar { ExternalEdges = new List<oM.Structural.Elements.Edge>(), Openings = new List<oM.Structural.Elements.Opening>(), Property = aProperty2D } };

            for (int i = 0; i < aResult.Count; i++)
            {
                PanelPlanar panel = aResult[i] as PanelPlanar;
                panel.Property = aProperty2D;

                panel = Modify.SetIdentifiers(panel, floor) as PanelPlanar;
                if (pullSettings.CopyCustomData)
                    panel = Modify.SetCustomData(panel, floor, pullSettings.ConvertUnits) as PanelPlanar;
            }

            return aResult;
        }

        /***************************************************/
    }
}