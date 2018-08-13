using Autodesk.Revit.DB;
using BH.oM.Structural.Elements;
using System.Collections.Generic;
using System.Linq;
using BH.oM.Revit;
using BH.Engine.Revit;
using System;

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

            switch (framingElement.StructuralUsage)
            {
                case StructuralUsage1D.Column:
                    return framingElement.ToRevitStructuralColumn(document, pushSettings);
                case StructuralUsage1D.Beam:
                case StructuralUsage1D.Brace:
                case StructuralUsage1D.Cable:
                    return framingElement.ToRevitStructuralFraming(document, pushSettings);
                case StructuralUsage1D.Pile:
                    BH.Engine.Reflection.Compute.RecordError(string.Format("Push of pile foundations is not supported in current version of BHoM. BHoM element Guid: {0}", framingElement.BHoM_Guid));
                    return null;
                default:
                    BH.Engine.Reflection.Compute.RecordWarning(string.Format("Structural usage type is not set. An attempt to create a structural framing element is being made. BHoM element Guid: {0}", framingElement.BHoM_Guid));
                    return framingElement.ToRevitStructuralFraming(document, pushSettings);
            }
        }

        /***************************************************/

        public static FamilyInstance ToRevitStructuralColumn(this FramingElement framingElement, Document document, PushSettings pushSettings = null)
        {
            if (framingElement == null || document == null)
                return null;

            pushSettings.DefaultIfNull();

            object aCustomDataValue = null;

            Curve aCurve = framingElement.LocationCurve.ToRevit(pushSettings);
            Level aLevel = null;
            
            //TODO: Rotation yo!
            aCustomDataValue = framingElement.ICustomData("Base Level");
            if (aCustomDataValue != null && aCustomDataValue is int)
            {
                ElementId aElementId = new ElementId((int)aCustomDataValue);
                aLevel = document.GetElement(aElementId) as Level;
            }

            if (aLevel == null)
                aLevel = Query.BottomLevel(framingElement.LocationCurve, document);

            FamilySymbol aFamilySymbol = framingElement.Property.ToRevitColumnSymbol(document, pushSettings);

            if (aFamilySymbol == null)
            {
                List<FamilySymbol> aFamilySymbolList = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_StructuralColumns).Cast<FamilySymbol>().ToList();

                aCustomDataValue = framingElement.ICustomData("Type");
                if (aCustomDataValue != null && aCustomDataValue is int)
                {
                    ElementId aElementId = new ElementId((int)aCustomDataValue);
                    aFamilySymbol = aFamilySymbolList.Find(x => x.Id == aElementId);
                }

                if (aFamilySymbol == null)
                    aFamilySymbolList.Find(x => x.Name == framingElement.Name);

                if (aFamilySymbol == null)
                {
                    BH.Engine.Reflection.Compute.RecordError(string.Format("Family symbol has not been found for given BHoM framing property. BHoM Guid: {0}", framingElement.BHoM_Guid));
                    return null;
                }
            }

            FamilyInstance aFamilyInstance = document.Create.NewFamilyInstance(aCurve, aFamilySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.Column);

            if (aFamilyInstance != null)
            {
                oM.Structural.Properties.ConstantFramingElementProperty barProperty = framingElement.Property as oM.Structural.Properties.ConstantFramingElementProperty;
                if (barProperty != null)
                {
                    double orientationAngle = Math.PI * 0.5 - barProperty.OrientationAngle;
                    Parameter rotation = aFamilyInstance.LookupParameter("Cross-Section Rotation");
                    rotation.Set(orientationAngle);
                }

                if (pushSettings.CopyCustomData)
                {
                    BuiltInParameter[] paramsToIgnore = new BuiltInParameter[]
                    {
                    //BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM,
                    //BuiltInParameter.FAMILY_BASE_LEVEL_PARAM,
                    BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM,
                    BuiltInParameter.SCHEDULE_BASE_LEVEL_OFFSET_PARAM,
                    BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM,
                    BuiltInParameter.ELEM_FAMILY_PARAM,
                    BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
                    BuiltInParameter.ALL_MODEL_IMAGE,
                    //BuiltInParameter.SCHEDULE_LEVEL_PARAM,
                    //BuiltInParameter.FAMILY_TOP_LEVEL_PARAM,
                    //BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM,
                    BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM,
                    BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM,
                    BuiltInParameter.ELEM_TYPE_PARAM
                    };
                    Modify.SetParameters(aFamilyInstance, framingElement, paramsToIgnore, pushSettings.ConvertUnits);
                }
            }
            
            return aFamilyInstance;
        }

        /***************************************************/

        public static FamilyInstance ToRevitStructuralFraming(this FramingElement framingElement, Document document, PushSettings pushSettings = null)
        {
            //TODO: remember about setting structural usage parameter!

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
                    aFamilySymbolList.Find(x => x.Name == framingElement.Name);
                
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
                Modify.SetParameters(aFamilyInstance, framingElement, new BuiltInParameter[] { BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION, BuiltInParameter.STRUCTURAL_BEAM_END1_ELEVATION }, pushSettings.ConvertUnits);

            return aFamilyInstance;
        }

        /***************************************************/
    }
}