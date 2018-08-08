using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Environment.Properties;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static ElementType ToRevit(this BuildingElementProperties buildingElementProperties, Document document, bool copyCustomData = true, bool convertUnits = true)
        {
            if (buildingElementProperties == null || document == null)
                return null;

            Type aType = Query.RevitType(buildingElementProperties.BuildingElementType);

            List<ElementType> aElementTypeList = new FilteredElementCollector(document).OfClass(aType).Cast<ElementType>().ToList();
            if (aElementTypeList == null || aElementTypeList.Count < 1)
                return null;

            ElementType aElementType = null;
            aElementType = aElementTypeList.First() as ElementType;
            aElementType = aElementType.Duplicate(buildingElementProperties.Name);

            if (copyCustomData)
                Modify.SetParameters(aElementType, buildingElementProperties, null, convertUnits);


            return aElementType;
        }

        /***************************************************/
    }
}
