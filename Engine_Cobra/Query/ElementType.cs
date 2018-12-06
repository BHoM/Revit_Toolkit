using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using System;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        static public ElementType ElementType(this IBHoMObject bHoMObject, IEnumerable<ElementType> elementTypes, bool exactMatch = true)
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

            if (!string.IsNullOrEmpty(aFamilyName) && !exactMatch)
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

        static public ElementType ElementType(this IBHoMObject bHoMObject, Document document, IEnumerable<BuiltInCategory> builtInCategories, FamilyLoadSettings familyLoadSettings = null, bool DuplicateTypeIfNotExists = true)
        {
            if (bHoMObject == null || document == null || builtInCategories == null || builtInCategories.Count() == 0)
                return null;

            //Find Existing ElementType in Document
            foreach (BuiltInCategory builtInCategory in builtInCategories)
            {
                List<ElementType> aElementTypeList;
                if (builtInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
                    aElementTypeList = new FilteredElementCollector(document).OfClass(typeof(ElementType)).Cast<ElementType>().ToList();
                else
                    aElementTypeList = new FilteredElementCollector(document).OfClass(typeof(ElementType)).OfCategory(builtInCategory).Cast<ElementType>().ToList();
       
                ElementType aElementType = bHoMObject.ElementType(aElementTypeList, true);
                if (aElementType != null)
                {
                    if(aElementType is FamilySymbol)
                    {
                        FamilySymbol aFamilySymbol = (FamilySymbol)aElementType;

                        if (!aFamilySymbol.IsActive)
                            aFamilySymbol.Activate();
                    }

                    return aElementType;
                }  
            }

            //Find ElementType in FamilyLibrary
            if(familyLoadSettings != null)
            {
                foreach (BuiltInCategory builtInCategory in builtInCategories)
                {
                    if (builtInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
                        continue;

                    string aCategoryName = builtInCategory.CategoryName(document);
                    if (string.IsNullOrEmpty(aCategoryName))
                        aCategoryName = bHoMObject.CategoryName();

                    string aFamilyTypeName = bHoMObject.FamilyTypeName();
                    if (string.IsNullOrEmpty(aFamilyTypeName))
                        aFamilyTypeName = bHoMObject.Name;

                    string aFamilyName = bHoMObject.FamilyName();
                    FamilySymbol aFamilySymbol = familyLoadSettings.LoadFamilySymbol(document, aCategoryName, aFamilyName, aFamilyTypeName);
                    if (aFamilySymbol != null)
                    {
                        if (!aFamilySymbol.IsActive)
                            aFamilySymbol.Activate();

                        return aFamilySymbol;
                    }
                }
            }

            //Duplicate if not exists
            if(DuplicateTypeIfNotExists)
            {
                foreach (BuiltInCategory builtInCategory in builtInCategories)
                {
                    if (builtInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
                        continue;

                    string aTypeName = bHoMObject.FamilyTypeName();
                    if (string.IsNullOrEmpty(aTypeName))
                        aTypeName = bHoMObject.Name;

                    if (!string.IsNullOrEmpty(aTypeName))
                    {
                        List <ElementType> aElementTypeList = new FilteredElementCollector(document).OfClass(typeof(ElementType)).OfCategory(builtInCategory).Cast<ElementType>().ToList();

                        if (aElementTypeList.Count > 0 && !(aElementTypeList.First() is FamilySymbol))
                        {
                            // Duplicate Type for object which is not Family Symbol
                            ElementType aElementType = aElementTypeList.First().Duplicate(aTypeName);
                            return aElementType;
                        }
                        else
                        {
                            // Duplicate Type for object which is Family Symbol

                            Family aFamily = bHoMObject.Family(document);
                            if (aFamily == null && builtInCategory != Autodesk.Revit.DB.BuiltInCategory.INVALID)
                            {
                                // Load and Duplicate Type from not existsing Family

                                string aCategoryName = builtInCategory.CategoryName(document);
                                if (string.IsNullOrEmpty(aCategoryName))
                                    aCategoryName = bHoMObject.CategoryName();

                                if (familyLoadSettings != null)
                                {
                                    string aFamilyName = bHoMObject.FamilyName();
                                    if (!string.IsNullOrEmpty(aFamilyName))
                                    {
                                        FamilySymbol aFamilySymbol = familyLoadSettings.LoadFamilySymbol(document, aCategoryName, aFamilyName);
                                        if (aFamilySymbol != null)
                                        {
                                            if (!aFamilySymbol.IsActive)
                                                aFamilySymbol.Activate();

                                            aFamilySymbol.Name = aTypeName;
                                            return aFamilySymbol;
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

                                        return aFamilySymbol.Duplicate(aTypeName);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        /***************************************************/

        static public ElementType ElementType(this IBHoMObject bHoMObject, Document document, BuiltInCategory builtInCategory, FamilyLoadSettings familyLoadSettings = null, bool DuplicateTypeIfNotExists = true)
        {
            return ElementType(bHoMObject, document, new BuiltInCategory[] { builtInCategory }, familyLoadSettings, DuplicateTypeIfNotExists);
        }

        /***************************************************/
    }
}
