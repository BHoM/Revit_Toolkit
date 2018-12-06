using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Environment.Properties;
using BH.oM.Adapters.Revit.Settings;
using System;

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

            List<BuiltInCategory> aBuiltInCategoryList = null;
            BuiltInCategory aBuiltInCategory = buildingElementProperties.BuildingElementType.BuiltInCategory();
            if(aBuiltInCategory == BuiltInCategory.INVALID)
                aBuiltInCategoryList = Enum.GetValues(typeof(oM.Environment.Elements.BuildingElementType)).Cast<oM.Environment.Elements.BuildingElementType>().ToList().ConvertAll(x => Query.BuiltInCategory(x));
            else
                aBuiltInCategoryList = new List<BuiltInCategory>() { aBuiltInCategory};

            if (aBuiltInCategoryList == null || aBuiltInCategoryList.Count == 0)
                return null;

            aElementType = buildingElementProperties.ElementType(document, aBuiltInCategoryList, pushSettings.FamilyLoadSettings, true);

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