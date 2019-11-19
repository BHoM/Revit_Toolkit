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

using BH.oM.Base;

namespace BH.UI.Revit.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        public static Element SetTags(this Element element, IBHoMObject bHoMObject, string tagsParameterName)
        {
            if (bHoMObject == null || element == null || string.IsNullOrEmpty(tagsParameterName))
                return null;

            Parameter aParameter = element.LookupParameter(tagsParameterName);
            if (aParameter == null || aParameter.IsReadOnly || aParameter.StorageType != StorageType.String)
                return null;

            string aValue_New = null;
            if(bHoMObject.Tags != null)
                aValue_New = string.Join("\n", bHoMObject.Tags);

            string aValue_Old = aParameter.AsString();

            if (aValue_New != aValue_Old)
                aParameter.Set(aValue_New);


            return element;
        }

        /***************************************************/

        public static IBHoMObject SetTags(this IBHoMObject bHoMObject, Element element, string tagsParameterName)
        {
            if (bHoMObject == null || element == null || string.IsNullOrEmpty(tagsParameterName))
                return null;

            Parameter aParameter = element.LookupParameter(tagsParameterName);
            if (aParameter == null || aParameter.StorageType != StorageType.String)
                return null;

            string aValueString = aParameter.AsString();

            IBHoMObject aIBHoMObject = bHoMObject.GetShallowClone();

            if (string.IsNullOrEmpty(aValueString) && (bHoMObject.Tags == null || bHoMObject.Tags.Count == 0))
                return aIBHoMObject;

            if (aIBHoMObject.Tags == null)
                aIBHoMObject.Tags = new System.Collections.Generic.HashSet<string>();

            string[] aValues = aValueString.Split('\n');

            foreach (string aValue in aValues)
                aIBHoMObject.Tags.Add(aValue);


            return aIBHoMObject;
        }

        /***************************************************/
    }
}