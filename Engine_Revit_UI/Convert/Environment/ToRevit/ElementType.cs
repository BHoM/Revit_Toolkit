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

using BH.oM.Environment.Properties;
using BH.oM.Adapters.Revit.Settings;
using System;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        /*internal static ElementType ToRevitElementType(this ElementProperties buildingElementProperties, Document document, PushSettings pushSettings = null)
        {
            throw new System.NotImplementedException("The method to convert a BHoM Building Element Type into a Revit Element Type has not been fixed yet. Check Issue #247 for more info");
            if (buildingElementProperties == null || document == null)
                return null;

            ElementType aElementType = pushSettings.FindRefObject<ElementType>(document, buildingElementProperties.BHoM_Guid);
            if (aElementType != null)
                return aElementType;

            pushSettings.DefaultIfNull();

            List<BuiltInCategory> aBuiltInCategoryList = null;
            BuiltInCategory aBuiltInCategory = buildingElementProperties.BuildingElementType.BuiltInCategory();
            if (aBuiltInCategory == BuiltInCategory.INVALID)
                aBuiltInCategoryList = Enum.GetValues(typeof(oM.Environment.Elements.BuildingElementType)).Cast<oM.Environment.Elements.BuildingElementType>().ToList().ConvertAll(x => Query.BuiltInCategory(x));
            else
                aBuiltInCategoryList = new List<BuiltInCategory>() { aBuiltInCategory };

            if (aBuiltInCategoryList == null || aBuiltInCategoryList.Count == 0)
                return null;

            aElementType = buildingElementProperties.ElementType(document, aBuiltInCategoryList, pushSettings.FamilyLoadSettings, true);

            aElementType.CheckIfNullPush(buildingElementProperties);
            if (aElementType == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aElementType, buildingElementProperties, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(buildingElementProperties, aElementType);

            return aElementType;
        }*/

        /***************************************************/
    }
}