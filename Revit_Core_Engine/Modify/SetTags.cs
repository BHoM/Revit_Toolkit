/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using System;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the values stored in Tags property of the given BHoM object and sets them to a given parameter of Revit Element.")]
        [Input("element", "Target Revit Element to copy the tags to.")]
        [Input("bHoMObject", "Source BHoM object to copy the tags from.")]
        [Input("tagsParameterName", "Name of the parameter of the Revit element to set the tags to.")]
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

        [Description("Extracts the values stored in a given parameter of Revit Element and sets them to Tags property of the given BHoM object.")]
        [Input("bHoMObject", "Target BHoM object to copy the tags to.")]
        [Input("element", "Source Revit Element to copy the tags from.")]
        [Input("tagsParameterName", "Name of the parameter of the Revit element to extract the tags from.")]
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


