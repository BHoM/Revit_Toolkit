using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Internal Methods                          ****/
        /***************************************************/

        internal static Element ToRevit(this GenericObject genericObject, Document document, PushSettings pushSettings = null)
        {
            if (genericObject == null || document == null)
                return null;

            pushSettings = pushSettings.DefaultIfNull();

            Element aElement = null;

            BuiltInCategory aBuiltInCategory = genericObject.BuiltInCategory(document, pushSettings.FamilyLibrary);

            ElementType aElementType = genericObject.ElementType(document, aBuiltInCategory, pushSettings.FamilyLibrary);

            if (aElementType != null)
            {
                if (aElementType is FamilySymbol)
                {
                    FamilySymbol aFamilySymbol = (FamilySymbol)aElementType;
                    Family aFamily = aFamilySymbol.Family;

                    IGeometry aIGeometry = genericObject.Location;

                    switch (aFamily.FamilyPlacementType)
                    {
                        //TODO: Implement rest of the cases

                        case FamilyPlacementType.ViewBased:
                            break;
                        case FamilyPlacementType.CurveBased:
                            if (aIGeometry is ICurve)
                            {
                                Level aLevel = ((ICurve)aIGeometry).BottomLevel(document);
                                if (aLevel == null)
                                    break;

                                Curve aCurve = ToRevit((ICurve)aIGeometry, pushSettings);
                                aElement = document.Create.NewFamilyInstance(aCurve, aFamilySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                            }
                            break;
                        case FamilyPlacementType.OneLevelBased:
                            if (aIGeometry is oM.Geometry.Point)
                            {
                                XYZ aXYZ = ToRevit((oM.Geometry.Point)aIGeometry, pushSettings);
                                aElement = document.Create.NewFamilyInstance(aXYZ, aFamilySymbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                            }
                            break;
                    }
                }
            }

            if (aElement != null && pushSettings.CopyCustomData)
                Modify.SetParameters(aElement, genericObject, null, pushSettings.ConvertUnits);

            return aElement;
        }

        /***************************************************/

        private static Element ToRevit(this DraftingObject draftingObject, Document document, PushSettings pushSettings = null)
        {
            if (draftingObject == null || string.IsNullOrEmpty(draftingObject.ViewName) || document == null)
                return null;

            pushSettings = pushSettings.DefaultIfNull();

            List<ViewDrafting> aViewDraftingList = new FilteredElementCollector(document).OfClass(typeof(ViewDrafting)).Cast<ViewDrafting>().ToList();
            if(aViewDraftingList == null || aViewDraftingList.Count == 0)
                return null;

            ViewDrafting aViewDrafting = aViewDraftingList.Find(x => x.Name == draftingObject.ViewName);
            if (aViewDrafting == null)
                return null;

            Element aElement = null;

            BuiltInCategory aBuiltInCategory = draftingObject.BuiltInCategory(document, pushSettings.FamilyLibrary);

            ElementType aElementType = draftingObject.ElementType(document, aBuiltInCategory, pushSettings.FamilyLibrary);

            if (aElementType != null)
            {
                if (aElementType is FamilySymbol)
                {
                    FamilySymbol aFamilySymbol = (FamilySymbol)aElementType;
                    Family aFamily = aFamilySymbol.Family;

                    IGeometry aIGeometry = draftingObject.Location;

                    switch (aFamily.FamilyPlacementType)
                    {
                        //TODO: Implement rest of the cases

                        case FamilyPlacementType.ViewBased:
                            if (aIGeometry is oM.Geometry.Point)
                            {
                                XYZ aXYZ = ToRevit((oM.Geometry.Point)aIGeometry, pushSettings);
                                aElement = document.Create.NewFamilyInstance(aXYZ, aFamilySymbol, aViewDrafting);
                            }
                            break;
                        case FamilyPlacementType.OneLevelBased:
                            break;
                    }
                }
            }

            if (aElement != null && pushSettings.CopyCustomData)
                Modify.SetParameters(aElement, draftingObject, null, pushSettings.ConvertUnits);

            return aElement;
        }

        /***************************************************/
    }
}