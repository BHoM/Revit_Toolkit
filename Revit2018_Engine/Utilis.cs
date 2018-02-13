using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using BH.oM.Environmental.Elements;
using BH.oM.Base;

namespace BH.Engine.Revit
{
    public static class Utilis
    {
        public static class Revit
        {
            public static Type GetType(BuildingElementType BuildingElementType)
            {
                switch (BuildingElementType)
                {
                    case BuildingElementType.Ceiling:
                        return typeof(CeilingType);
                    case BuildingElementType.Floor:
                        return typeof(FloorType);
                    case BuildingElementType.Roof:
                        return typeof(RoofType);
                    case BuildingElementType.Wall:
                        return typeof(WallType);
                }

                return null;
            }
        }

        public static class BHoM
        {
            public static BuildingElementType? GetBuildingElementType(BuiltInCategory BuiltInCategory)
            {
                switch(BuiltInCategory)
                {
                    case BuiltInCategory.OST_Ceilings:
                        return BuildingElementType.Ceiling;
                    case BuiltInCategory.OST_Floors:
                        return BuildingElementType.Floor;
                    case BuiltInCategory.OST_Roofs:
                        return BuildingElementType.Roof;
                    case BuiltInCategory.OST_Walls:
                        return BuildingElementType.Wall;
                }

                return null;
            }

            public static void AssignIdentifiers(BHoMObject BHoMObject, Element Element)
            {
                BHoMObject.CustomData.Add("ElementId", Element.Id.IntegerValue);
                BHoMObject.CustomData.Add("UniqueId", Element.UniqueId);
            }
        }
    }
}
