/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System.Linq;
using System.Collections.Generic;

using BH.oM.Adapters.Revit.Generic;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;

using Autodesk.Revit.DB;

namespace BH.UI.Revit.Engine
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static FamilySymbol LoadFamilySymbol(this FamilyLoadSettings FamilyLoadSettings, Document document, string categoryName, string familyName, string familyTypeName = null)
        {
            if (FamilyLoadSettings == null || FamilyLoadSettings.FamilyLibrary == null || document == null)
                return null;

            FamilyLibrary aFamilyLibrary = FamilyLoadSettings.FamilyLibrary;

            IEnumerable<string> aPaths = BH.Engine.Adapters.Revit.Query.Paths(aFamilyLibrary, categoryName, familyName, familyTypeName);
            if (aPaths == null || aPaths.Count() == 0)
                return null;

            string aPath = aPaths.First();

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
