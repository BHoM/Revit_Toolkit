﻿using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        static public oM.Architecture.Elements.Level Level(this Element element, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

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

        static public Level Level(this Document document, double Elevation, bool convertUnits = true)
        {
            List<Level> aLevelList = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
            if (aLevelList == null || aLevelList.Count == 0)
                return null;

            aLevelList.Sort((x, y) => x.ProjectElevation.CompareTo(y.ProjectElevation));

            double aElevation = aLevelList.First().ProjectElevation;
            if (convertUnits)
                aElevation = UnitUtils.ConvertFromInternalUnits(aElevation, DisplayUnitType.DUT_METERS);

            if (Elevation <= aElevation)
                return aLevelList.First();

            for (int i = 1; i < aLevelList.Count; i++)
            {
                aElevation = aLevelList[i].ProjectElevation;
                if (convertUnits)
                    aElevation = UnitUtils.ConvertFromInternalUnits(aElevation, DisplayUnitType.DUT_METERS);

                if (Elevation <= aElevation)
                    return aLevelList[i];
            }


            return aLevelList.Last();
        }

        /***************************************************/
    }
}