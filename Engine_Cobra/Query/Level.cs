using Autodesk.Revit.DB;
using BH.oM.Base;
using BH.oM.Revit;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        static public oM.Architecture.Elements.Level Level(this Element element, PullSettings pullSettings = null)
        {
            pullSettings.DefaultIfNull();

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