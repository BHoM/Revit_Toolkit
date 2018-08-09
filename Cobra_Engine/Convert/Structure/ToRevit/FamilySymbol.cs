using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit;
using System.Collections.Generic;
using System.Linq;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static FamilySymbol ToRevitFamilySymbol(this oM.Structural.Properties.IFramingElementProperty framingElementProperty, Document document, PushSettings pushSettings = null)
        {
            List<FamilySymbol> aFamilySymbolList = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_StructuralFraming).Cast<FamilySymbol>().ToList();
            if (aFamilySymbolList == null || aFamilySymbolList.Count < 1)
                return null;

            if (pushSettings == null)
                pushSettings = PushSettings.Default;

            FamilySymbol aFamilySymbol = null;

            aFamilySymbol = aFamilySymbolList.Find(x => x.Name == framingElementProperty.Name);
            if (aFamilySymbol != null)
                return aFamilySymbol;

            return null;
        }

        /***************************************************/
    }
}
