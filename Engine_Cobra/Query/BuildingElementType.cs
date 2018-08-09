using Autodesk.Revit.DB;
using BH.oM.Environment.Elements;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static BuildingElementType? BuildingElementType(this BuiltInCategory builtInCategory)
        {
            switch (builtInCategory)
            {
                case BuiltInCategory.OST_Ceilings:
                    return oM.Environment.Elements.BuildingElementType.Ceiling;
                case BuiltInCategory.OST_Floors:
                    return oM.Environment.Elements.BuildingElementType.Floor;
                case BuiltInCategory.OST_Roofs:
                    return oM.Environment.Elements.BuildingElementType.Roof;
                case BuiltInCategory.OST_Walls:
                    return oM.Environment.Elements.BuildingElementType.Wall;
                case BuiltInCategory.OST_Windows:
                    return oM.Environment.Elements.BuildingElementType.Window;
                case BuiltInCategory.OST_Doors:
                    return oM.Environment.Elements.BuildingElementType.Door;
                case BuiltInCategory.OST_CurtainWallPanels:
                    return oM.Environment.Elements.BuildingElementType.Window;
            }

            return null;
        }

        /***************************************************/

        public static BuildingElementType? BuildingElementType(this Category Category)
        {
            if (Category == null)
                return null;

            return BuildingElementType((BuiltInCategory)Category.Id.IntegerValue);
        }

        /***************************************************/
    }
}