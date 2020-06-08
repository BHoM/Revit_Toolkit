/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static HostObjAttributes ToRevitHostObjAttributes(this oM.Physical.Constructions.IConstruction construction, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (construction == null || document == null)
                return null;

            HostObjAttributes elementType = refObjects.GetValue<HostObjAttributes>(document, construction.BHoM_Guid);
            if (elementType != null)
                return elementType;

            settings = settings.DefaultIfNull();

            List<BuiltInCategory> builtInCategoryList = null;
            BuiltInCategory buildInCategory = Query.BuiltInCategory(construction, document);
            if (buildInCategory == BuiltInCategory.INVALID)
                builtInCategoryList = new List<BuiltInCategory>() { BuiltInCategory.OST_Walls, BuiltInCategory.OST_Floors, BuiltInCategory.OST_Roofs };
            else
                builtInCategoryList = new List<BuiltInCategory>() { buildInCategory };

            if (builtInCategoryList == null || builtInCategoryList.Count == 0)
                return null;

            elementType = construction.ElementType(document, builtInCategoryList, settings.FamilyLoadSettings, true) as HostObjAttributes;

            elementType.CheckIfNullPush(construction);
            if (elementType == null)
                return null;

            // Copy parameters from BHoM CustomData to Revit Element
            elementType.CopyParameters(construction, settings);

            refObjects.AddOrReplace(construction, elementType);
            return elementType;
        }

        /***************************************************/
    }
}
