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

using Autodesk.Revit.DB;

using BH.UI.Revit.Engine;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Common.Properties;


namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Internal Methods                          ****/
        /***************************************************/

        internal static ElementType ToRevitElementType(this Object2DProperties object2DProperties, Document document, PushSettings pushSettings = null)
        {
            if (object2DProperties == null || document == null)
                return null;

            ElementType aElementType = pushSettings.FindRefObject<ElementType>(document, object2DProperties.BHoM_Guid);
            if (aElementType != null)
                return aElementType;

            pushSettings.DefaultIfNull();

            List<BuiltInCategory> aBuiltInCategoryList = null;
            BuiltInCategory aBuiltInCategory = object2DProperties.BuiltInCategory(document);
            if (aBuiltInCategory == BuiltInCategory.INVALID)
                aBuiltInCategoryList = new List<BuiltInCategory>() { BuiltInCategory.OST_Walls, BuiltInCategory.OST_Floors, BuiltInCategory.OST_Roofs };
            else
                aBuiltInCategoryList = new List<BuiltInCategory>() { aBuiltInCategory };

            if (aBuiltInCategoryList == null || aBuiltInCategoryList.Count == 0)
                return null;

            aElementType = object2DProperties.ElementType(document, aBuiltInCategoryList, pushSettings.FamilyLoadSettings, true);

            aElementType.CheckIfNullPush(object2DProperties);
            if (aElementType == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aElementType, object2DProperties, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(object2DProperties, aElementType);

            return aElementType;
        }

        /***************************************************/
        }
    }