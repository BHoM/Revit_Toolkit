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

using Autodesk.Revit.DB;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static string FamilyTypeFullName(this Element Element)
        {
            if (Element == null)
                return null;

            ElementType aElementType = null;
            if (Element is ElementType)
            {
                aElementType = (ElementType)Element;
                if (aElementType == null)
                    return null;
            }
            else if(Element is Family)
            {
                return null;
            }
            else
            {
                ElementId aElementId = Element.GetTypeId();
                if (aElementId == null || aElementId == Autodesk.Revit.DB.ElementId.InvalidElementId)
                    return null;

                aElementType = Element.Document.GetElement(aElementId) as ElementType;
                if (aElementType == null)
                    return null;
            }

            if (aElementType == null)
                return null;

            string aFamilyName = aElementType.FamilyName;
            string aFamilyTypeName = aElementType.Name;

            if (string.IsNullOrEmpty(aFamilyName) || string.IsNullOrEmpty(aFamilyTypeName))
                return null;

            return BH.Engine.Adapters.Revit.Query.FamilyTypeFullName(aFamilyName, aFamilyTypeName);
        }

        /***************************************************/
    }
}
