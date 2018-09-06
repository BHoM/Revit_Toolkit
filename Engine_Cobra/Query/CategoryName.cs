using Autodesk.Revit.DB;
using BH.oM.Base;
using BH.oM.Environment.Elements;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static string CategoryName(this BuiltInCategory builtInCategory, Document document)
        {
            if (document == null || document.Settings == null || document.Settings.Categories == null)
                return null;

            foreach (Category aCategory in document.Settings.Categories)
                if (aCategory.Id.IntegerValue == (int)builtInCategory)
                    return aCategory.Name;

            return null;
        }

        /***************************************************/

        public static string CategoryName(this Document document, string familyName)
        {
            if (document == null || string.IsNullOrEmpty(familyName))
                return null;

            List<ElementType> aElementTypeList = new FilteredElementCollector(document).OfClass(typeof(ElementType)).Cast<ElementType>().ToList();

            ElementType aElementType = aElementTypeList.Find(x => x.FamilyName == familyName && x.Category != null);

            if (aElementType == null)
                return null;

            return aElementType.Category.Name;
        }

        /***************************************************/
    }
}
