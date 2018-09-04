using System.Collections.Generic;
using System.Linq;

using BH.oM.Revit;

using Autodesk.Revit.DB;

namespace BH.UI.Cobra.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static FamilySymbol LoadFamilySymbol(this FamilyLibrary familyLibrary, Document document, string categoryName, string familyName, string typeName)
        {
            if (familyLibrary == null && document == null)
                return null;

            List<string> aPathList = BH.Engine.Revit.Query.GetPaths(familyLibrary, categoryName, familyName, typeName);
            if (aPathList != null && aPathList.Count > 0)
            {
                return LoadFamilySymbol(document, aPathList.First(), typeName);
            }

            return null;
        }

        /***************************************************/

        public static FamilySymbol LoadFamilySymbol(this Document document, string path, string typeName)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(typeName) || document == null)
                return null;

            FamilySymbol aFamilySymbol = null;
            document.LoadFamilySymbol(path, typeName, out aFamilySymbol);
            return aFamilySymbol;
        }

        /***************************************************/
    }
}
