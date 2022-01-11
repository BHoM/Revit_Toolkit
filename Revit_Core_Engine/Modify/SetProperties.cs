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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Copies parameter values from a Revit Element to properties of a BHoM object.")]
        [Input("iObject", "Target BHoM object to copy the parameter values to.")]
        [Input("element", "Source Revit element to copy the parameter values from.")]
        [Input("settings", "MappingSettings containing the information about the relationships between property names of BHoM types and parameter names of correspondent Revit elements.")]
        public static void SetProperties(this IObject iObject, Element element, MappingSettings settings = null)
        {
            if (iObject == null || settings == null || settings.ParameterMaps == null || settings.ParameterMaps.Count == 0 || element == null)
                return;

            Type type = iObject.GetType();

            IEnumerable<PropertyInfo> propertyInfos = type.MapPropertyInfos();
            if (propertyInfos == null || propertyInfos.Count() == 0)
                return;

            Element elementType = element.Document.GetElement(element.GetTypeId());

            foreach (PropertyInfo pInfo in propertyInfos)
            {
                HashSet<string> parameterNames = settings.ParameterNames(type, pInfo.Name, false);
                if (parameterNames != null && iObject.SetProperty(pInfo, element, parameterNames))
                    continue;

                if (elementType == null)
                    continue;

                parameterNames = settings.ParameterNames(type, pInfo.Name, true);
                if (parameterNames != null)
                    iObject.SetProperty(pInfo, elementType, parameterNames);
            }
        }
        
        /***************************************************/
    }
}


