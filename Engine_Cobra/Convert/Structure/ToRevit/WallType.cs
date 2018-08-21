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

        public static WallType ToRevitWallType(this oM.Structure.Properties.IProperty2D property2D, Document document, PushSettings pushSettings = null)
        {
            List<WallType> aWallTypeList = new FilteredElementCollector(document).OfClass(typeof(WallType)).OfCategory(BuiltInCategory.OST_Walls).Cast<WallType>().ToList();
            if (aWallTypeList == null || aWallTypeList.Count < 1)
                return null;

            WallType aWallType = null;

            aWallType = aWallTypeList.Find(x => x.Name == property2D.Name);

            if (aWallType != null)
                return aWallType;

            return null;
        }
    }
}