using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static FamilySymbol ToRevitFamilySymbol(this oM.Structural.Properties.IFramingElementProperty framingElementProperty, Document document, bool copyCustomData = true, bool convertUnits = true)
        {
            List<FamilySymbol> aFamilySymbolList = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_StructuralFraming).Cast<FamilySymbol>().ToList();
            if (aFamilySymbolList == null || aFamilySymbolList.Count < 1)
                return null;

            FamilySymbol aFamilySymbol = null;

            aFamilySymbol = aFamilySymbolList.Find(x => x.Name == framingElementProperty.Name);
            if (aFamilySymbol != null)
                return aFamilySymbol;

            return null;
        }

        /***************************************************/
    }
}
