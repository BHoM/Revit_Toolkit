using System;

using Autodesk.Revit.DB;

using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Structure.Elements;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static FamilyInstance ToRevitStructuralColumn(this FramingElement framingElement, Document document, PushSettings pushSettings = null)
        {
            if (framingElement == null || document == null)
                return null;

            pushSettings = pushSettings.DefaultIfNull();

            object aCustomDataValue = null;

            Curve aCurve = framingElement.LocationCurve.ToRevitCurve(pushSettings);
            Level aLevel = null;
            
            aCustomDataValue = framingElement.CustomDataValue("Base Level");
            if (aCustomDataValue != null && aCustomDataValue is int)
            {
                ElementId aElementId = new ElementId((int)aCustomDataValue);
                aLevel = document.GetElement(aElementId) as Level;
            }

            if (aLevel == null)
                aLevel = Query.BottomLevel(framingElement.LocationCurve, document);

            FamilySymbol aFamilySymbol = framingElement.Property.ToRevitFamilySymbol_Column(document, pushSettings);

            if (aFamilySymbol == null)
            {
                aFamilySymbol = Query.ElementType(framingElement, document, BuiltInCategory.OST_StructuralColumns, pushSettings.FamilyLibrary) as FamilySymbol;

                //List<FamilySymbol> aFamilySymbolList = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_StructuralColumns).Cast<FamilySymbol>().ToList();

                //aCustomDataValue = framingElement.ICustomData("Type");
                //if (aCustomDataValue != null && aCustomDataValue is int)
                //{
                //    ElementId aElementId = new ElementId((int)aCustomDataValue);
                //    aFamilySymbol = aFamilySymbolList.Find(x => x.Id == aElementId);
                //}

                //if (aFamilySymbol == null)
                //    aFamilySymbol = aFamilySymbolList.Find(x => x.Name == framingElement.Name);

                if (aFamilySymbol == null)
                {
                    BH.Engine.Reflection.Compute.RecordError(string.Format("Family symbol has not been found for given BHoM framing property. BHoM Guid: {0}", framingElement.BHoM_Guid));
                    return null;
                }
            }

            FamilyInstance aFamilyInstance = document.Create.NewFamilyInstance(aCurve, aFamilySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.Column);

            aFamilyInstance.CheckIfNullPush(framingElement);

            if (aFamilyInstance != null)
            {
                oM.Structure.Properties.ConstantFramingElementProperty barProperty = framingElement.Property as oM.Structure.Properties.ConstantFramingElementProperty;
                if (barProperty != null)
                {
                    double orientationAngle = (Math.PI * 0.5 - barProperty.OrientationAngle) % (2 * Math.PI);
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
    }
}
