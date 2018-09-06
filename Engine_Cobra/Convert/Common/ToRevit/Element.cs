using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Geometry;
using BH.oM.Adapters.Revit;
using BH.oM.Base;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Internal Methods                          ****/
        /***************************************************/

        internal static Element ToRevit(this BHoMObject bHoMObject, Document document, PushSettings pushSettings = null)
        {
            if (bHoMObject == null || document == null)
                return null;

            pushSettings = pushSettings.DefaultIfNull();

            Element aElement = null;

            switch (bHoMObject.BuiltInCategory(document, pushSettings.FamilyLibrary))
            {
                case BuiltInCategory.OST_Sheets:
                    aElement = ToRevitSheet(bHoMObject, document, pushSettings);
                    break;
                default:
                    break;
            }

            if (aElement != null && pushSettings.CopyCustomData)
                Modify.SetParameters(aElement, bHoMObject, null, pushSettings.ConvertUnits);

            return aElement;

        }


        /***************************************************/

        internal static Element ToRevit(this GenericObject genericObject, Document document, PushSettings pushSettings = null)
        {
            if (genericObject == null || document == null)
                return null;

            pushSettings = pushSettings.DefaultIfNull();

            Element aElement = null;


            BuiltInCategory aBuiltInCategory = genericObject.BuiltInCategory(document, pushSettings.FamilyLibrary);

            switch (aBuiltInCategory)
            {
                default:
                    aElement = ToRevit(genericObject, document, aBuiltInCategory, pushSettings);
                    break;
            }

            if (aElement != null && pushSettings.CopyCustomData)
                Modify.SetParameters(aElement, genericObject, null, pushSettings.ConvertUnits);

            return aElement;
        }

        /***************************************************/

        private static ViewSheet ToRevitSheet(this BHoMObject BHoMObject, Document document, PushSettings pushSettings = null)
        {
            //TODO: Implement Sheet Creation

            return null;
        }

        /***************************************************/

        private static Element ToRevit(this GenericObject genericObject, Document document, BuiltInCategory builtInCategory, PushSettings pushSettings = null)
        {
            Element aElement = null;

            ElementType aElementType = genericObject.ElementType(document, builtInCategory, pushSettings.FamilyLibrary);

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

            return aElement;
        }

        /***************************************************/
    }
}