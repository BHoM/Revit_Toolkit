using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit;


namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        static public ElementType ElementType(this IBHoMObject bHoMObject, IEnumerable<ElementType> elementTypes)
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

        static public ElementType ElementType(this IBHoMObject bHoMObject, Document document, BuiltInCategory builtInCategory, FamilyLibrary familyLibrary = null, bool DuplicateTypeIfNotExists = true)
        {
            if (bHoMObject == null || document == null)
                return null;

            List<ElementType> aElementTypeList = new FilteredElementCollector(document).OfClass(typeof(ElementType)).OfCategory(builtInCategory).Cast<ElementType>().ToList();

            //Find Existing ElementType in Document
            ElementType aElementType = bHoMObject.ElementType(aElementTypeList);
            if (aElementType != null)
                return aElementType;

            //Find ElementType in FamilyLibrary
            if (familyLibrary != null)
            {
                string aCategoryName = builtInCategory.CategoryName(document);
                if (string.IsNullOrEmpty(aCategoryName))
                    aCategoryName = bHoMObject.CategoryName();

                string aTypeName = bHoMObject.TypeName();
                if (string.IsNullOrEmpty(aTypeName))
                    aTypeName = bHoMObject.Name;

                string aFamilyName = bHoMObject.FamilyName();
                aElementType = familyLibrary.LoadFamilySymbol(document, aCategoryName, aFamilyName, aTypeName);
            }

            //Duplicate if not exists
            if (aElementType == null && DuplicateTypeIfNotExists)
            {
                string aTypeName = bHoMObject.TypeName();
                if (string.IsNullOrEmpty(aTypeName))
                    aTypeName = bHoMObject.Name;

                if(!string.IsNullOrEmpty(aTypeName))
                {
                    if (aElementTypeList.Count > 0 && !(aElementTypeList.First() is FamilySymbol))
                    {
                        // Duplicate Type for object which is not Family Symbol
                        aElementType = aElementTypeList.First().Duplicate(aTypeName);
                    }
                    else
                    {
                        // Duplicate Type for object which is Family Symbol

                        Family aFamily = bHoMObject.Family(document);
                        if(aFamily == null)
                        {
                            // Load and Duplicate Type from not existsing Family

                            string aCategoryName = builtInCategory.CategoryName(document);
                            if (string.IsNullOrEmpty(aCategoryName))
                                aCategoryName = bHoMObject.CategoryName();

                            if (familyLibrary != null)
                            {
                                string aFamilyName = bHoMObject.FamilyName();
                                if(!string.IsNullOrEmpty(aFamilyName ))
                                {
                                    aElementType = familyLibrary.LoadFamilySymbol(document, aCategoryName, aFamilyName);
                                    aElementType.Name = aTypeName;
                                }
                            }
                        }
                        else
                        {
                            // Duplicate from existing family

                            ISet<ElementId> aElementIdSet = aFamily.GetFamilySymbolIds();
                            if(aElementIdSet != null && aElementIdSet.Count > 0)
                            {
                                aElementType = document.GetElement( aElementIdSet.First()) as ElementType;
                                if(aElementType != null)
                                    aElementType = aElementType.Duplicate(aTypeName);
                            }
                        }
                    }
                }


            }

            return aElementType;
        }

        /***************************************************/
    }
}
