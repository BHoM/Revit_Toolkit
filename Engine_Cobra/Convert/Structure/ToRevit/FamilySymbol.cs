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

            return Query.ElementType(framingElementProperty, document, BuiltInCategory.OST_StructuralColumns, pushSettings.FamilyLibrary) as FamilySymbol;
            
            //framingElementProperty.ElementType()

            //List<FamilySymbol> aFamilySymbolList = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_StructuralColumns).Cast<FamilySymbol>().ToList();
            //if (aFamilySymbolList == null || aFamilySymbolList.Count < 1)
            //    return null;

            //FamilySymbol aFamilySymbol = null;

            //aFamilySymbol = aFamilySymbolList.Find(x => x.Name == framingElementProperty.Name);

            //if (aFamilySymbol != null)
            //    return aFamilySymbol;

            //return null;
        }

        /***************************************************/

        private static FamilySymbol ToRevitFamilySymbol_Framing(this oM.Structure.Properties.IFramingElementProperty framingElementProperty, Document document, PushSettings pushSettings = null)
        {
            if (framingElementProperty == null || document == null)
                return null;

            pushSettings = pushSettings.DefaultIfNull();

            return Query.ElementType(framingElementProperty, document, BuiltInCategory.OST_StructuralFraming, pushSettings.FamilyLibrary) as FamilySymbol;

            //List<FamilySymbol> aFamilySymbolList = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_StructuralFraming).Cast<FamilySymbol>().ToList();
            //if (aFamilySymbolList == null || aFamilySymbolList.Count < 1)
            //    return null;

            //FamilySymbol aFamilySymbol = null;

            //aFamilySymbol = aFamilySymbolList.Find(x => x.Name == framingElementProperty.Name);
            //if (aFamilySymbol != null)
            //    return aFamilySymbol;

            //return null;
        }

        /***************************************************/
    }
}