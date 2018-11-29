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

        internal static Element ToRevitElement(this GenericObject genericObject, Document document, PushSettings pushSettings = null)
        {
            Element aElement = pushSettings.FindRefObject<Element>(document, genericObject.BHoM_Guid);
            if (aElement != null)
                return aElement;

            pushSettings.DefaultIfNull();

            BuiltInCategory aBuiltInCategory = genericObject.BuiltInCategory(document, pushSettings.FamilyLoadSettings);

            ElementType aElementType = genericObject.ElementType(document, aBuiltInCategory, pushSettings.FamilyLoadSettings);

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
                                Level aLevel = ((ICurve)aIGeometry).BottomLevel(document, pushSettings.ConvertUnits);
                                if (aLevel == null)
                                    break;

                                Curve aCurve = ToRevitCurve((ICurve)aIGeometry, pushSettings);
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

            aElement.CheckIfNullPush(genericObject);
            if (aElement == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aElement, genericObject, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(genericObject, aElement);

            return aElement;
        }

        /***************************************************/

        private static Element ToRevitElement(this DraftingObject draftingObject, Document document, PushSettings pushSettings = null)
        {
            if (draftingObject == null || string.IsNullOrEmpty(draftingObject.ViewName) || document == null)
                return null;

            Element aElement = pushSettings.FindRefObject<Element>(document, draftingObject.BHoM_Guid);
            if (aElement != null)
                return aElement;

            pushSettings = pushSettings.DefaultIfNull();

            List<ViewDrafting> aViewDraftingList = new FilteredElementCollector(document).OfClass(typeof(ViewDrafting)).Cast<ViewDrafting>().ToList();
            if(aViewDraftingList == null || aViewDraftingList.Count == 0)
                return null;

            ViewDrafting aViewDrafting = aViewDraftingList.Find(x => x.Name == draftingObject.ViewName);
            if (aViewDrafting == null)
                return null;

            BuiltInCategory aBuiltInCategory = draftingObject.BuiltInCategory(document, pushSettings.FamilyLoadSettings);

            ElementType aElementType = draftingObject.ElementType(document, aBuiltInCategory, pushSettings.FamilyLoadSettings);

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

            aElement.CheckIfNullPush(draftingObject);
            if (aElement == null)
                return null;

            if (aElement != null && pushSettings.CopyCustomData)
                Modify.SetParameters(aElement, draftingObject, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(draftingObject, aElement);

            return aElement;
        }

        /***************************************************/
    }
}