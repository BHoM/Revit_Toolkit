﻿using System.Collections.Generic;
using System.Linq;

using BH.oM.Adapters.Revit;
using BH.Engine.Adapters.Revit;

using Autodesk.Revit.DB;

namespace BH.UI.Cobra.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static FamilySymbol LoadFamilySymbol(this FamilyLibrary familyLibrary, Document document, string categoryName, string familyName, string typeName = null)
        {
            if (familyLibrary == null && document == null)
                return null;

            List<string> aPathList = BH.Engine.Adapters.Revit.Query.GetPaths(familyLibrary, categoryName, familyName, typeName);
            if (aPathList != null && aPathList.Count > 0)
            {
                return LoadFamilySymbol(document, aPathList.First(), typeName);
            }

            return null;
        }

        /***************************************************/

        public static FamilySymbol LoadFamilySymbol(this Document document, string path, string typeName = null)
        {
            if (string.IsNullOrEmpty(path) || document == null || !System.IO.File.Exists(path))
                return null;

            string aTypeName = typeName;
            if(string.IsNullOrEmpty(aTypeName))
            {
                //Look for first available type name if type name not provided

                RevitFilePreview aRevitFilePreview = Create.RevitFilePreview(path);
                if (aRevitFilePreview == null)
                    return null;

                List<string> aTypeNameList = BH.Engine.Adapters.Revit.Query.TypeNames(aRevitFilePreview);
                if (aTypeNameList == null || aTypeNameList.Count < 1)
                    return null;

                aTypeName = aTypeNameList.First();
            }

            if (string.IsNullOrEmpty(aTypeName))
                return null;

            FamilySymbol aFamilySymbol = null;
            if(document.LoadFamilySymbol(path, aTypeName, out aFamilySymbol))
            {
                if (!aFamilySymbol.IsActive)
                    aFamilySymbol.Activate();
            }

            return aFamilySymbol;
        }

        /***************************************************/
    }
}
