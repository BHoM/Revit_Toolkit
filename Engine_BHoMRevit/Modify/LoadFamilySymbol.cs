using System.Linq;
using System.Collections.Generic;

using BH.oM.Adapters.Revit.Generic;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;

using Autodesk.Revit.DB;

namespace BH.UI.Revit.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static FamilySymbol LoadFamilySymbol(this FamilyLoadSettings FamilyLoadSettings, Document document, string categoryName, string familyName, string familyTypeName = null)
        {
            if (FamilyLoadSettings == null || FamilyLoadSettings.FamilyLibrary == null || document == null)
                return null;

            FamilyLibrary aFamilyLibrary = FamilyLoadSettings.FamilyLibrary;

            List<string> aPathList = BH.Engine.Adapters.Revit.Query.Paths(aFamilyLibrary, categoryName, familyName, familyTypeName);
            if (aPathList == null || aPathList.Count == 0)
                return null;

            string aPath = aPathList.First();

            string aTypeName = familyTypeName;
            if (string.IsNullOrEmpty(aTypeName))
            {
                //Look for first available type name if type name not provided

                RevitFilePreview aRevitFilePreview = Create.RevitFilePreview(aPath);
                if (aRevitFilePreview == null)
                    return null;

                List<string> aTypeNameList = BH.Engine.Adapters.Revit.Query.FamilyTypeNames(aRevitFilePreview);
                if (aTypeNameList == null || aTypeNameList.Count < 1)
                    return null;

                aTypeName = aTypeNameList.First();
            }

            if (string.IsNullOrEmpty(aTypeName))
                return null;

            FamilySymbol aFamilySymbol = null;

            if (document.LoadFamilySymbol(aPath, aTypeName, new FamilyLoadOptions(FamilyLoadSettings), out aFamilySymbol))
            {
                if (!aFamilySymbol.IsActive)
                    aFamilySymbol.Activate();
            }

            return aFamilySymbol;
        }

        /***************************************************/

        private class FamilyLoadOptions : IFamilyLoadOptions
        {
            private bool pOverwriteParameterValues;
            private bool pOverwriteFamily;

            public FamilyLoadOptions(FamilyLoadSettings FamilyLoadSettings)
            {
                pOverwriteParameterValues = FamilyLoadSettings.OverwriteParameterValues;
                pOverwriteFamily = FamilyLoadSettings.OverwriteFamily;
            }

            public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
            {
                overwriteParameterValues = pOverwriteParameterValues;
                return pOverwriteFamily;
            }

            public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
            {
                overwriteParameterValues = pOverwriteParameterValues;
                if(pOverwriteFamily)
                {
                    source = FamilySource.Family;
                    return true;
                }
                else
                {
                    source = FamilySource.Project;
                    return false;
                }
            }
        }
    }
}
