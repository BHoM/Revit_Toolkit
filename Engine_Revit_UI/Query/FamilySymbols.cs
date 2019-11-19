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

using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;


namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        public static IEnumerable<FamilySymbol> FamilySymbols(this oM.Adapters.Revit.Generic.RevitFilePreview revitFilePreview, Document document)
        {
            if (document == null || revitFilePreview == null)
                return null;

            IEnumerable<Family> aFamilies = new FilteredElementCollector(document).OfClass(typeof(Family)).Cast<Family>();
            if (aFamilies == null)
                return null;

            string aCategoryName = BH.Engine.Adapters.Revit.Query.CategoryName(revitFilePreview);

            string aName = System.IO.Path.GetFileNameWithoutExtension(revitFilePreview.Path);
            if (string.IsNullOrEmpty(aName))
                return null;

            foreach (Family aFamily in aFamilies)
            {
                if (aFamily.IsInPlace || !aFamily.IsUserCreated || aFamily.FamilyCategory == null)
                    continue;

                if (!string.IsNullOrEmpty(aCategoryName) && !aFamily.FamilyCategory.Name.Equals(aCategoryName))
                    continue;

                if(aFamily.Name == aName)
                {
                    IEnumerable<ElementId> aElementIds = aFamily.GetFamilySymbolIds();
                    if (aElementIds == null)
                        return null;

                    List<FamilySymbol> aResult = new List<FamilySymbol>();
                    foreach (ElementId aElementId in aElementIds)
                        aResult.Add(document.GetElement(aElementId) as FamilySymbol);

                    return aResult;
                }
            }

            return null;

        }

        /***************************************************/
    }
}