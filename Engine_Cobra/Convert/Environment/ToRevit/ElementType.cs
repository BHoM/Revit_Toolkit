using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Environment.Properties;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static ElementType ToRevitElementType(this BuildingElementProperties buildingElementProperties, Document document, PushSettings pushSettings = null)
        {
            if (buildingElementProperties == null || document == null)
                return null;

            ElementType aElementType = pushSettings.FindRefObject<ElementType>(document, buildingElementProperties.BHoM_Guid);
            if (aElementType != null)
                return aElementType;

            pushSettings.DefaultIfNull();

            BuiltInCategory aBuiltInCategory = buildingElementProperties.BuildingElementType.BuiltInCategory();
            if (aBuiltInCategory == BuiltInCategory.INVALID)
                return null;

            aElementType = buildingElementProperties.ElementType(document, aBuiltInCategory, pushSettings.FamilyLoadSettings);
            if(aElementType == null)
            {
                List<ElementType> aElementTypeList = new FilteredElementCollector(document).OfCategory(aBuiltInCategory).WhereElementIsElementType().Cast<ElementType>().ToList();
                if (aElementTypeList == null || aElementTypeList.Count < 1)
                    return null;

                aElementType = aElementTypeList.First() as ElementType;
                aElementType = aElementType.Duplicate(buildingElementProperties.Name);
            }

            aElementType.CheckIfNullPush(buildingElementProperties);
            if (aElementType == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aElementType, buildingElementProperties, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(buildingElementProperties, aElementType);

            return aElementType;
        }

        /***************************************************/
    }
}