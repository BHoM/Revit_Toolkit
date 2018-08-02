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

        public static List<PanelPlanar> ToBHoMPanelPlanar(this Wall wall, bool copyCustomData = true, bool convertUnits = true)
        {
            try
            {
                string materialGrade = wall.MaterialGrade();

                IProperty2D aProperty2D = wall.WallType.ToBHoMProperty2D(copyCustomData, convertUnits, materialGrade) as IProperty2D;
                List<oM.Geometry.ICurve> outlines = wall.Outlines();

                List<PanelPlanar> aResult = BHS.Create.PanelPlanar(outlines);

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
            catch (Exception e)
            {
                throw new Exception("Failed to pull element " + wall.Id.IntegerValue.ToString() + ". Exception: " + e.Message);
            }
        }

        /***************************************************/

        public static List<PanelPlanar> ToBHoMPanelPlanar(this Floor floor, bool copyCustomData = true, bool convertUnits = true)
        {
            try
            {
                string materialGrade = floor.MaterialGrade();

                IProperty2D aProperty2D = floor.FloorType.ToBHoMProperty2D(copyCustomData, convertUnits, materialGrade) as IProperty2D;
                List<oM.Geometry.ICurve> outlines = floor.Outlines();

                List<PanelPlanar> aResult = BHS.Create.PanelPlanar(outlines);

                for (int i = 0; i < aResult.Count; i++)
                {
                    PanelPlanar panel = aResult[i] as PanelPlanar;
                    panel.Property = aProperty2D;
                    panel = Modify.SetIdentifiers(panel, floor) as PanelPlanar;

                    if (copyCustomData)
                    {
                        aResult[i] = Modify.SetCustomData(panel, floor, convertUnits) as PanelPlanar;
                    }
                }
                return aResult;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to pull element " + floor.Id.IntegerValue.ToString() + ". Exception: " + e.Message);
            }
        }

        /***************************************************/
    }
}
