using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Generic;


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

            string aTypeName = bHoMObject.FamilyTypeName();
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

            if (!string.IsNullOrEmpty(aFamilyName))
            {
                foreach (ElementType aElementType in elementTypes)
                {
                    if (aElementType.FamilyName == aFamilyName)
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

                string aFamilyTypeName = bHoMObject.FamilyTypeName();
                if (string.IsNullOrEmpty(aFamilyTypeName))
                    aFamilyTypeName = bHoMObject.Name;

                string aFamilyName = bHoMObject.FamilyName();
                FamilySymbol aFamilySymbol = familyLibrary.LoadFamilySymbol(document, aCategoryName, aFamilyName, aFamilyTypeName);
                if(aFamilySymbol != null)
                {
                    if (!aFamilySymbol.IsActive)
                        aFamilySymbol.Activate();

                    aElementType = aFamilySymbol;
                }
            }

            //Duplicate if not exists
            if (aElementType == null && DuplicateTypeIfNotExists)
            {
                string aTypeName = bHoMObject.FamilyTypeName();
                if (string.IsNullOrEmpty(aTypeName))
                    aTypeName = bHoMObject.Name;

                if(!string.IsNullOrEmpty(aTypeName))
                {
                    if (aElementTypeList.Count > 0 && !(aElementTypeList.First() is FamilySymbol))
                    {
                        FamilySymbol aFamilySymbol = aElementTypeList.First() as FamilySymbol;

                        if (!aFamilySymbol.IsActive)
                            aFamilySymbol.Activate();

                        // Duplicate Type for object which is not Family Symbol
                        aElementType = aFamilySymbol.Duplicate(aTypeName);
                    }
                    else
                    {
                        // Duplicate Type for object which is Family Symbol

                        Family aFamily = bHoMObject.Family(document);
                        if (aFamily == null)
                        {
                            // Load and Duplicate Type from not existsing Family

                            string aCategoryName = builtInCategory.CategoryName(document);
                            if (string.IsNullOrEmpty(aCategoryName))
                                aCategoryName = bHoMObject.CategoryName();

                            if (familyLibrary != null)
                            {
                                string aFamilyName = bHoMObject.FamilyName();
                                if (!string.IsNullOrEmpty(aFamilyName))
                                {
                                    FamilySymbol aFamilySymbol = familyLibrary.LoadFamilySymbol(document, aCategoryName, aFamilyName);
                                    if(aFamilySymbol != null)
                                    {
                                        if (!aFamilySymbol.IsActive)
                                            aFamilySymbol.Activate();

                                        aElementType = aFamilySymbol;
                                        aElementType.Name = aTypeName;
                                    }
                                    
                                }
                            }
                        }
                        else
                        {
                            // Duplicate from existing family

                            ISet<ElementId> aElementIdSet = aFamily.GetFamilySymbolIds();
                            if (aElementIdSet != null && aElementIdSet.Count > 0)
                            {
                                FamilySymbol aFamilySymbol = document.GetElement(aElementIdSet.First()) as FamilySymbol;
                                if (aFamilySymbol != null)
                                {
                                    if (!aFamilySymbol.IsActive)
                                        aFamilySymbol.Activate();

                                    aElementType = aFamilySymbol;
                                    aElementType = aElementType.Duplicate(aTypeName);
                                }
                            }
                        }
                    }
                }
            }

            if(aElementType is FamilySymbol)
            {
                FamilySymbol aFamilySymbol = aElementType as FamilySymbol;
                if (!aFamilySymbol.IsActive)
                    aFamilySymbol.Activate();
            }

            return aElementType;
        }

        /***************************************************/
    }
}
