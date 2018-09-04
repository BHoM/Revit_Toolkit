using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Environment.Elements;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BuiltInCategory BuiltInCategory(this BuildingElementType buildingElementType)
        {
            switch (buildingElementType)
            {
                case (oM.Environment.Elements.BuildingElementType.Ceiling):
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Ceilings;
                case (oM.Environment.Elements.BuildingElementType.Door):
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Doors;
                case (oM.Environment.Elements.BuildingElementType.Floor):
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Floors;
                case (oM.Environment.Elements.BuildingElementType.Roof):
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Roofs;
                case (oM.Environment.Elements.BuildingElementType.Wall):
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Walls;
                case (oM.Environment.Elements.BuildingElementType.Window):
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Windows;
                case (oM.Environment.Elements.BuildingElementType.Undefined):
                    return Autodesk.Revit.DB.BuiltInCategory.INVALID;
                default:
                    return Autodesk.Revit.DB.BuiltInCategory.INVALID;
            }
        }

        /***************************************************/

        public static BuiltInCategory BuiltInCategory(this BHoMObject bHoMObject, Document document)
        {
            if (bHoMObject == null || document == null)
                return Autodesk.Revit.DB.BuiltInCategory.INVALID;

            string aCategoryName = BH.Engine.Revit.Query.CategoryName(bHoMObject);
            if (string.IsNullOrEmpty(aCategoryName))
                return Autodesk.Revit.DB.BuiltInCategory.INVALID;

            return BuiltInCategory(document, aCategoryName);
        }

        /***************************************************/

        public static BuiltInCategory BuiltInCategory(this Document document, string Name)
        {
            if (document == null || string.IsNullOrEmpty(Name)|| document.Settings == null || document.Settings.Categories == null)
                return Autodesk.Revit.DB.BuiltInCategory.INVALID;


            foreach (Category aCategory in document.Settings.Categories)
                if (aCategory.Name == Name)
                    return (BuiltInCategory)aCategory.Id.IntegerValue;

            return Autodesk.Revit.DB.BuiltInCategory.INVALID;
        }

        /***************************************************/
    }
}
