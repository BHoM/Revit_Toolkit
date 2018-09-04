using Autodesk.Revit.DB;
using BH.oM.Geometry;
using BH.oM.Revit;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Internal Methods                          ****/
        /***************************************************/

        internal static Element ToRevit(this GenericObject GenericObject, Document document, PushSettings pushSettings = null)
        {
            if (GenericObject == null || document == null)
                return null;

            pushSettings.DefaultIfNull();

            BuiltInCategory aBuiltInCategory = GenericObject.BuiltInCategory(document);
            if (aBuiltInCategory == BuiltInCategory.INVALID)
                return null;

            ElementType aElementType = GenericObject.ElementType(document, aBuiltInCategory, pushSettings.FamilyLibrary);

            if(aElementType != null)
            {
                if(aElementType is FamilySymbol)
                {
                    FamilySymbol aFamilySymbol = (FamilySymbol)aElementType;
                    Family aFamily = aFamilySymbol.Family;

                    IGeometry aIGeometry = GenericObject.Location;

                    switch (aFamily.FamilyPlacementType)
                    {
                        case FamilyPlacementType.ViewBased:
                            break;
                        case FamilyPlacementType.OneLevelBased:
                            if(aIGeometry is oM.Geometry.Point)
                            {
                                XYZ aXYZ = ToRevit((oM.Geometry.Point)aIGeometry);
                                return document.Create.NewFamilyInstance(aXYZ, aFamilySymbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                            }
                            break;
                    }
                }
            }

            return null;
        }

        /***************************************************/
    }
}