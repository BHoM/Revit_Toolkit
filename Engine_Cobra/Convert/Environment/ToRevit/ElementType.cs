using Autodesk.Revit.DB;
using BH.oM.Environment.Properties;
using BH.oM.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static ElementType ToRevit(this BuildingElementProperties buildingElementProperties, Document document, PushSettings pushSettings = null)
        {
            if (buildingElementProperties == null || document == null)
                return null;

            BuiltInCategory aBuiltInCategory = buildingElementProperties.BuildingElementType.BuiltInCategory();
            if (aBuiltInCategory == BuiltInCategory.INVALID)
                return null;

            pushSettings.DefaultIfNull();

            ElementType aElementType = buildingElementProperties.ElementType(document, aBuiltInCategory, pushSettings.FamilyLibrary);
            if(aElementType == null)
            {
                List<ElementType> aElementTypeList = new FilteredElementCollector(document).OfCategory(aBuiltInCategory).WhereElementIsElementType().Cast<ElementType>().ToList();
                if (aElementTypeList == null || aElementTypeList.Count < 1)
                    return null;

                aElementType = aElementTypeList.First() as ElementType;
                aElementType = aElementType.Duplicate(buildingElementProperties.Name);
            }

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aElementType, buildingElementProperties, null, pushSettings.ConvertUnits);

            return aElementType;
        }

        /***************************************************/
    }
}