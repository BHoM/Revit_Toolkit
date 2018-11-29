using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Structure.Elements;
using BH.oM.Structure.Properties;
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
            pullSettings = pullSettings.DefaultIfNull();

            List<PanelPlanar> aResult = pullSettings.FindRefObjects<PanelPlanar>(wall.Id.IntegerValue);
            if (aResult != null && aResult.Count > 0)
                return aResult;

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
            
            List<oM.Geometry.ICurve> outlines = wall.Outlines(pullSettings);
            aResult = outlines != null ? BHS.Create.PanelPlanar(outlines) : new List<PanelPlanar> { new PanelPlanar { ExternalEdges = new List<oM.Structure.Elements.Edge>(), Openings = new List<oM.Structure.Elements.Opening>(), Property = aProperty2D } };

            for (int i = 0; i < aResult.Count; i++)
            {
                PanelPlanar panel = aResult[i] as PanelPlanar;
                panel.Property = aProperty2D;

                panel = Modify.SetIdentifiers(panel, wall) as PanelPlanar;
                if (pullSettings.CopyCustomData)
                    panel = Modify.SetCustomData(panel, wall, pullSettings.ConvertUnits) as PanelPlanar;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(panel);

                aResult[i] = panel;
            }

            return aResult;
        }

        /***************************************************/

        internal static List<PanelPlanar> ToBHoMPanelPlanar(this Floor floor, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<PanelPlanar> aResult = pullSettings.FindRefObjects<PanelPlanar>(floor.Id.IntegerValue);
            if (aResult != null && aResult.Count > 0)
                return aResult;

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

            List<oM.Geometry.ICurve> outlines = floor.Outlines(pullSettings);
            aResult = outlines != null ? BHS.Create.PanelPlanar(outlines) : new List<PanelPlanar> { new PanelPlanar { ExternalEdges = new List<oM.Structure.Elements.Edge>(), Openings = new List<oM.Structure.Elements.Opening>(), Property = aProperty2D } };

            for (int i = 0; i < aResult.Count; i++)
            {
                PanelPlanar panel = aResult[i] as PanelPlanar;
                panel.Property = aProperty2D;

                panel = Modify.SetIdentifiers(panel, floor) as PanelPlanar;
                if (pullSettings.CopyCustomData)
                    panel = Modify.SetCustomData(panel, floor, pullSettings.ConvertUnits) as PanelPlanar;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(panel);

                aResult[i] = panel;
            }

            return aResult;
        }

        /***************************************************/
    }
}