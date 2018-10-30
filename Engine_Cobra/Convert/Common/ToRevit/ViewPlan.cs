using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;


namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Internal Methods                          ****/
        /***************************************************/

        internal static ViewPlan ToRevitViewPlan(this oM.Adapters.Revit.Elements.ViewPlan floorPlan, Document document, PushSettings pushSettings = null)
        {
            if (floorPlan == null || string.IsNullOrEmpty(floorPlan.LevelName) || string.IsNullOrEmpty(floorPlan.Name))
                return null;

            ElementId aElementId_Level = null;

            List<Level> aLevelList = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
            if (aLevelList == null || aLevelList.Count < 1)
                return null;

            Level aLevel = aLevelList.Find(x => x.Name == floorPlan.LevelName);
            if (aLevel == null)
                return null;

            aElementId_Level = aLevel.Id;

            ElementId aElementId_ViewFamilyType = ElementId.InvalidElementId;

            IEnumerable<ElementType> aViewFamilyTypes = new FilteredElementCollector(document).OfClass(typeof(ViewFamilyType)).Cast<ElementType>();

            ElementType aElementType = floorPlan.ElementType(aViewFamilyTypes);
            if (aElementType == null)
                return null;

            foreach (ViewFamilyType aViewFamilyType in aViewFamilyTypes)
            {
                if(aViewFamilyType.FamilyName == "Floor Plan")
                {
                    aElementId_ViewFamilyType = aViewFamilyType.Id;
                    break;
                }
            }

            if (aElementId_ViewFamilyType == ElementId.InvalidElementId)
                return null;

            Autodesk.Revit.DB.ViewPlan aViewPlan = Autodesk.Revit.DB.ViewPlan.Create(document, aElementId_ViewFamilyType, aElementId_Level);
            aViewPlan.ViewName = floorPlan.Name;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aViewPlan, floorPlan, null, pushSettings.ConvertUnits);

            return aViewPlan;
        }

        /***************************************************/
    }
}