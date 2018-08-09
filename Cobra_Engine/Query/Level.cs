using System.Collections.Generic;

using Autodesk.Revit.DB;
using BH.oM.Base;
using System.Linq;
using BH.oM.Adapters.Revit;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Query Methods
    /// </summary>
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        /// <summary>
        /// Gets Level of Revit element
        /// </summary>
        /// <param name="element">Revit Element</param>
        /// <param name="pullSettings">BHoM Pull Settings</param>
        /// <returns name="Level">BHoM Level</returns>
        /// <search>
        /// Query, Level, Revit, Level, element
        /// </search>
        static public oM.Architecture.Elements.Level Level(Element element, PullSettings pullSettings = null)
        {
            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            oM.Architecture.Elements.Level aLevel = null;
            if (pullSettings.RefObjects != null)
            {
                List<IBHoMObject> aBHoMObjectList = new List<IBHoMObject>();
                if (pullSettings.RefObjects.TryGetValue(element.LevelId.IntegerValue, out aBHoMObjectList))
                    if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                        aLevel = aBHoMObjectList.First() as oM.Architecture.Elements.Level;
            }

            if (aLevel == null)
            {
                aLevel = (element.Document.GetElement(element.LevelId) as Level).ToBHoM(pullSettings) as oM.Architecture.Elements.Level;
                if (pullSettings.RefObjects != null)
                    pullSettings.RefObjects.Add(element.LevelId.IntegerValue, new List<IBHoMObject>(new BHoMObject[] { aLevel }));
            }

            return aLevel;
        }

        /***************************************************/
    }
}
