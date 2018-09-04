using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.Engine.Revit;
using BH.oM.Revit;


namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        static public ElementType ElementType(this BHoMObject bHoMObject, IEnumerable<ElementType> elementTypes)
        {
            if (elementTypes == null || bHoMObject == null)
                return null;

            string aTypeName = bHoMObject.TypeName();
            if (string.IsNullOrEmpty(aTypeName))
                aTypeName = bHoMObject.Name;

            string aFamilyName = bHoMObject.FamilyName();

            ElementType aResult = null;
            if (!string.IsNullOrEmpty(aTypeName))
            {
                foreach (ElementType aElementType in elementTypes)
                {
                    if ((aElementType.FamilyName == aFamilyName && aElementType.Name == aTypeName) || (string.IsNullOrEmpty(aFamilyName) && aElementType.Name == aTypeName))
                    {
                        aResult = aElementType;
                        break;
                    }
                }
            }

            return aResult;
        }

        /***************************************************/

        static public ElementType ElementType(this BHoMObject bHoMObject, Document document, BuiltInCategory builtInCategory, FamilyLibrary familyLibrary = null)
        {
            if (bHoMObject == null || document == null)
                return null;

            List<ElementType> aElementTypeList = new FilteredElementCollector(document).OfClass(typeof(ElementType)).OfCategory(builtInCategory).Cast<ElementType>().ToList();

            ElementType aElementType = bHoMObject.ElementType(aElementTypeList);
            if (aElementType != null)
                return aElementType;

            if(familyLibrary != null)
            {
                string aCategoryName = builtInCategory.CategoryName(document);
                if (string.IsNullOrEmpty(aCategoryName))
                    aCategoryName = bHoMObject.CategoryName();

                string aTypeName = bHoMObject.TypeName();
                if (string.IsNullOrEmpty(aTypeName))
                    aTypeName = bHoMObject.Name;

                string aFamilyName = bHoMObject.FamilyName();

                return familyLibrary.LoadFamilySymbol(document, aCategoryName, aFamilyName, aTypeName);
            }

            return aElementType;
        }

        /***************************************************/
    }
}
