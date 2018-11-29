using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static FamilySymbol ToRevitFamilySymbol_Column(this oM.Structure.Properties.IFramingElementProperty framingElementProperty, Document document, PushSettings pushSettings = null)
        {
            if (framingElementProperty == null || document == null)
                return null;

            FamilySymbol aFamilySymbol= pushSettings.FindRefObject<FamilySymbol>(document, framingElementProperty.BHoM_Guid);
            if (aFamilySymbol != null)
                return aFamilySymbol;

            pushSettings = pushSettings.DefaultIfNull();

            aFamilySymbol = Query.ElementType(framingElementProperty, document, BuiltInCategory.OST_StructuralColumns, pushSettings.FamilyLoadSettings) as FamilySymbol;

            aFamilySymbol.CheckIfNullPush(framingElementProperty);
            if (aFamilySymbol == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aFamilySymbol, framingElementProperty, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(framingElementProperty, aFamilySymbol);

            return aFamilySymbol;
        }

        /***************************************************/

        private static FamilySymbol ToRevitFamilySymbol_Framing(this oM.Structure.Properties.IFramingElementProperty framingElementProperty, Document document, PushSettings pushSettings = null)
        {
            if (framingElementProperty == null || document == null)
                return null;

            FamilySymbol aFamilySymbol = pushSettings.FindRefObject<FamilySymbol>(document, framingElementProperty.BHoM_Guid);
            if (aFamilySymbol != null)
                return aFamilySymbol;

            pushSettings = pushSettings.DefaultIfNull();

            aFamilySymbol = Query.ElementType(framingElementProperty, document, BuiltInCategory.OST_StructuralFraming, pushSettings.FamilyLoadSettings) as FamilySymbol;

            aFamilySymbol.CheckIfNullPush(framingElementProperty);
            if (aFamilySymbol == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aFamilySymbol, framingElementProperty, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(framingElementProperty, aFamilySymbol);

            return aFamilySymbol;
        }

        /***************************************************/
    }
}