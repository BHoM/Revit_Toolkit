using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.Engine.Environment;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System.Linq;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static List<oM.Environment.Elements.Panel> ToBHoMPanels(this Element element, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> aResult = pullSettings.FindRefObjects<oM.Environment.Elements.Panel>(element.Id.IntegerValue);
            if (aResult != null && aResult.Count > 0)
                return aResult;

            aResult = Query.Panels(element.get_Geometry(new Options()), pullSettings);
            if (aResult == null || aResult.Count == 0)
                return aResult;

            for(int i=0; i < aResult.Count; i++)
            {
                aResult[i] = Modify.SetIdentifiers(aResult[i], element) as oM.Environment.Elements.Panel;
                if (pullSettings.CopyCustomData)
                    aResult[i] = Modify.SetCustomData(aResult[i], element, pullSettings.ConvertUnits) as oM.Environment.Elements.Panel;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aResult[i]);
            }

            return aResult;
        }

        /***************************************************/

        internal static List<oM.Environment.Elements.Panel> ToBHoMPanels(this RoofBase roofBase, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> aResult = pullSettings.FindRefObjects<oM.Environment.Elements.Panel>(roofBase.Id.IntegerValue);
            if (aResult != null && aResult.Count > 0)
                return aResult;

            aResult = Query.Panels(roofBase.get_Geometry(new Options()), pullSettings);
            if (aResult == null || aResult.Count == 0)
                return aResult;

            for (int i = 0; i < aResult.Count; i++)
            {
                aResult[i] = Modify.SetIdentifiers(aResult[i], roofBase) as oM.Environment.Elements.Panel;
                if (pullSettings.CopyCustomData)
                    aResult[i] = Modify.SetCustomData(aResult[i], roofBase, pullSettings.ConvertUnits) as oM.Environment.Elements.Panel;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aResult[i]);
            }

            return aResult;
        }

        /***************************************************/

        internal static List<oM.Environment.Elements.Panel> ToBHoMPanels(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> aResult = new List<oM.Environment.Elements.Panel>();

            oM.Environment.Elements.Panel aPanel = pullSettings.FindRefObject<oM.Environment.Elements.Panel>(familyInstance.Id.IntegerValue);
            if (aPanel != null)
            {
                aResult.Add(aPanel);
                return aResult;
            }

            aPanel = Create.Panel(new oM.Geometry.Polyline[] { familyInstance.VerticalBounds(pullSettings)});
            if (aPanel != null)
                aResult.Add(aPanel);

            aPanel = Modify.SetIdentifiers(aPanel, familyInstance) as oM.Environment.Elements.Panel;
            if (pullSettings.CopyCustomData)
                aPanel = Modify.SetCustomData(aPanel, familyInstance, pullSettings.ConvertUnits) as oM.Environment.Elements.Panel;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aPanel);

            return aResult;
        }

        /***************************************************/


    }
}