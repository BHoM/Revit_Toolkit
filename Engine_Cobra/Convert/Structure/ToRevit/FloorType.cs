using Autodesk.Revit.DB;
using BH.oM.Revit;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static FloorType ToRevitFloorType(this oM.Structure.Properties.IProperty2D property2D, Document document, PushSettings pushSettings = null)
        {
            List<FloorType> aFloorTypeList = new FilteredElementCollector(document).OfClass(typeof(FloorType)).OfCategory(BuiltInCategory.OST_Floors).Cast<FloorType>().ToList();
            aFloorTypeList.AddRange(new FilteredElementCollector(document).OfClass(typeof(FloorType)).OfCategory(BuiltInCategory.OST_StructuralFoundation).Cast<FloorType>());
            if (aFloorTypeList == null || aFloorTypeList.Count < 1)
                return null;

            FloorType aFloorType = null;

            aFloorType = aFloorTypeList.Find(x => x.Name == property2D.Name);

            if (aFloorType != null)
                return aFloorType;

            return null;
        }
    }
}