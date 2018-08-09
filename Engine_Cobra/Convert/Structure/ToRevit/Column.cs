using Autodesk.Revit.DB;
using BH.oM.Structural.Elements;
using System.Collections.Generic;
using System.Linq;
using BH.oM.Revit;
using BH.Engine.Revit;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static FamilyInstance ToRevit(this FramingElement framingElement, Document document, PushSettings pushSettings = null)
        {
            pushSettings.DefaultIfNull();

            return framingElement.ToRevitColumn(document, pushSettings);
        }

        /***************************************************/

        public static FamilyInstance ToRevitColumn(this FramingElement framingElement, Document document, PushSettings pushSettings = null)
        {
            if (framingElement == null || document == null)
                return null;

            pushSettings.DefaultIfNull();

            object aCustomDataValue = null;

            Curve aCurve = framingElement.LocationCurve.ToRevit();
            Level aLevel = null;

            aCustomDataValue = framingElement.ICustomData("Reference Level");
            if (aCustomDataValue != null && aCustomDataValue is int)
            {
                ElementId aElementId = new ElementId((int)aCustomDataValue);
                aLevel = document.GetElement(aElementId) as Level;
            }

            if (aLevel == null)
                aLevel = Query.BottomLevel(framingElement.LocationCurve, document);

            FamilySymbol aFamilySymbol = ToRevitFamilySymbol(framingElement.Property, document, pushSettings);

            if (aFamilySymbol == null)
            {
                List<FamilySymbol> aFamilySymbolList = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_StructuralFraming).Cast<FamilySymbol>().ToList();

                aCustomDataValue = framingElement.ICustomData("Type");
                if (aCustomDataValue != null && aCustomDataValue is int)
                {
                    ElementId aElementId = new ElementId((int)aCustomDataValue);
                    aFamilySymbol = aFamilySymbolList.Find(x => x.Id == aElementId);
                }

                if (aFamilySymbol == null)
                    aFamilySymbolList.Find(x => x.Name == framingElement.Name);

                if (aFamilySymbol == null)
                    aFamilySymbol = aFamilySymbolList.First();
            }

            FamilyInstance aFamilyInstance = document.Create.NewFamilyInstance(aCurve, aFamilySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aFamilyInstance, framingElement, new BuiltInParameter[] { BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION, BuiltInParameter.STRUCTURAL_BEAM_END1_ELEVATION }, pushSettings.ConvertUnits);

            return aFamilyInstance;
        }

        /***************************************************/
    }
}