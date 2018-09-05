using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit;
using BH.oM.Structure.Elements;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static FamilyInstance ToRevitStructuralFraming(this FramingElement framingElement, Document document, PushSettings pushSettings = null)
        {
            if (framingElement == null || document == null)
                return null;

            pushSettings.DefaultIfNull();

            object aCustomDataValue = null;

            Curve aCurve = framingElement.LocationCurve.ToRevit(pushSettings);
            Level aLevel = null;

            aCustomDataValue = framingElement.ICustomData("Reference Level");
            if (aCustomDataValue != null && aCustomDataValue is int)
            {
                ElementId aElementId = new ElementId((int)aCustomDataValue);
                aLevel = document.GetElement(aElementId) as Level;
            }

            if (aLevel == null)
                aLevel = Query.BottomLevel(framingElement.LocationCurve, document);

            FamilySymbol aFamilySymbol = framingElement.Property.ToRevitFramingSymbol(document, pushSettings);

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
                    aFamilySymbol = aFamilySymbolList.Find(x => x.Name == framingElement.Name);

                if (aFamilySymbol == null)
                {
                    BH.Engine.Reflection.Compute.RecordError(string.Format("Family symbol has not been found for given BHoM framing property. BHoM Guid: {0}", framingElement.BHoM_Guid));
                    return null;
                }
            }

            FamilyInstance aFamilyInstance;
            switch (framingElement.StructuralUsage)
            {
                case StructuralUsage1D.Beam:
                    aFamilyInstance = document.Create.NewFamilyInstance(aCurve, aFamilySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                    break;
                case StructuralUsage1D.Brace:
                    aFamilyInstance = document.Create.NewFamilyInstance(aCurve, aFamilySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.Brace);
                    break;
                case StructuralUsage1D.Cable:
                    aFamilyInstance = document.Create.NewFamilyInstance(aCurve, aFamilySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.Brace);
                    break;
                default:
                    aFamilyInstance = document.Create.NewFamilyInstance(aCurve, aFamilySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.UnknownFraming);
                    break;
            }

            aFamilyInstance.CheckIfNullPush(framingElement);

            if (aFamilyInstance != null && pushSettings.CopyCustomData)
            {
                BuiltInParameter[] paramsToIgnore = new BuiltInParameter[]
                {
                    BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION,
                    BuiltInParameter.STRUCTURAL_BEAM_END1_ELEVATION,
                    BuiltInParameter.ELEM_FAMILY_PARAM,
                    BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
                    BuiltInParameter.ALL_MODEL_IMAGE,
                    BuiltInParameter.ELEM_TYPE_PARAM
                };
                Modify.SetParameters(aFamilyInstance, framingElement, paramsToIgnore, pushSettings.ConvertUnits);
            }

            return aFamilyInstance;
        }

        /***************************************************/
    }
}
