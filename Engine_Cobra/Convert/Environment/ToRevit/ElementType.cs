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

            Type aType = Query.RevitType(buildingElementProperties.BuildingElementType);

            List<ElementType> aElementTypeList = new FilteredElementCollector(document).OfClass(aType).Cast<ElementType>().ToList();
            if (aElementTypeList == null || aElementTypeList.Count < 1)
                return null;

            pushSettings.DefaultIfNull();

            ElementType aElementType = null;
            aElementType = aElementTypeList.First() as ElementType;
            aElementType = aElementType.Duplicate(buildingElementProperties.Name);

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aElementType, buildingElementProperties, null, pushSettings.ConvertUnits);


            return aElementType;
        }

        /***************************************************/
    }
}