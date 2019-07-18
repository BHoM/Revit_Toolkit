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

using BH.oM.Adapters.Revit.Settings;


namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Internal Methods                          ****/
        /***************************************************/

        internal static HostObjAttributes ToRevitHostObjAttributes(this oM.Physical.Constructions.IConstruction construction, Document document, PushSettings pushSettings = null)
        {
            if (construction == null || document == null)
                return null;

            HostObjAttributes aElementType = pushSettings.FindRefObject<HostObjAttributes>(document, construction.BHoM_Guid);
            if (aElementType != null)
                return aElementType;

            pushSettings.DefaultIfNull();

            List<BuiltInCategory> aBuiltInCategoryList = null;
            BuiltInCategory aBuiltInCategory = Query.BuiltInCategory(construction, document);
            if (aBuiltInCategory == BuiltInCategory.INVALID)
                aBuiltInCategoryList = new List<BuiltInCategory>() { BuiltInCategory.OST_Walls, BuiltInCategory.OST_Floors, BuiltInCategory.OST_Roofs };
            else
                aBuiltInCategoryList = new List<BuiltInCategory>() { aBuiltInCategory };

            if (aBuiltInCategoryList == null || aBuiltInCategoryList.Count == 0)
                return null;

            aElementType = construction.ElementType(document, aBuiltInCategoryList, pushSettings.FamilyLoadSettings, true) as HostObjAttributes;

            aElementType.CheckIfNullPush(construction);
            if (aElementType == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aElementType, construction, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(construction, aElementType);

            return aElementType;
        }

        /***************************************************/
        }
    }