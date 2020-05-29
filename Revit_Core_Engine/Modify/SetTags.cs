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
using BH.oM.Base;
using System;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static void SetTags(this Element element, IBHoMObject bHoMObject, string tagsParameterName)
        {
            if (bHoMObject == null || element == null || string.IsNullOrEmpty(tagsParameterName))
                return;

            Parameter parameter = element.LookupParameter(tagsParameterName);
            if (parameter == null || parameter.IsReadOnly || parameter.StorageType != StorageType.String)
                return;

            string newValue = null;
            if (bHoMObject.Tags != null && bHoMObject.Tags.Count != 0)
                newValue = string.Join("; ", bHoMObject.Tags);

            string oldValue = parameter.AsString();

            if (newValue != oldValue)
                parameter.Set(newValue);
        }

        /***************************************************/

        public static void SetTags(this IBHoMObject bHoMObject, Element element, string tagsParameterName)
        {
            if (bHoMObject == null || element == null || string.IsNullOrEmpty(tagsParameterName))
                return;

            Parameter parameter = element.LookupParameter(tagsParameterName);
            if (parameter == null || parameter.StorageType != StorageType.String)
                return;

            string tags = parameter.AsString();
            if (string.IsNullOrEmpty(tags))
                return;

            if (bHoMObject.Tags == null)
                bHoMObject.Tags = new System.Collections.Generic.HashSet<string>();

            string[] values = tags.Split(new string[] { "; " }, StringSplitOptions.None);

            foreach (string value in values)
                bHoMObject.Tags.Add(value);
        }

        /***************************************************/
    }
}
