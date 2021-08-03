/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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

using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static void CheckFamilyMapUsage(this Type bHoMType, Document document, ParameterSettings settings)
        {
            List<string> familyNames = settings?.MappedFamilyNames(bHoMType);
            if (familyNames == null || familyNames.Count == 0)
                return;

            List<string> familiesInDoc = new FilteredElementCollector(document).OfClass(typeof(Family)).Select(x => x.Name).ToList();
            List<string> unusedMaps = new List<string>();
            foreach (string familyName in familyNames)
            {
                if (!familiesInDoc.Contains(familyName))
                    unusedMaps.Add(familyName);
            }

            if (unusedMaps.Count != 0)
            {
                string warning = $"Some of the family names declared in the RevitSettings.ParameterSettings.FamilyMaps are not present in the Revit document.\nMissing names: {string.Join(", ", unusedMaps)}.";
                BH.Engine.Reflection.Compute.RecordWarning(warning);
            }
        }

        /***************************************************/
    }
}


