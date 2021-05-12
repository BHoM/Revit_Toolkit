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
using BH.oM.Revit.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static ParameterElement ToRevitParameterElement(this ParameterDefinition parameterDefinition, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (parameterDefinition == null)
                return null;

            ParameterElement parameterElement = refObjects.GetValue<ParameterElement>(document, parameterDefinition.BHoM_Guid);
            if (parameterElement != null)
                return parameterElement;

            settings = settings.DefaultIfNull();

            Definition definition = Create.Parameter(document, parameterDefinition.Name, parameterDefinition.ParameterType, parameterDefinition.ParameterGroup, parameterDefinition.Instance, parameterDefinition.Categories, parameterDefinition.Shared);

            if (definition is ExternalDefinition)
                parameterElement = SharedParameterElement.Lookup(document, ((ExternalDefinition)definition).GUID);
            else if (definition is InternalDefinition)
                parameterElement = document.GetElement(((InternalDefinition)definition).Id) as ParameterElement;
            
            refObjects.AddOrReplace(parameterDefinition, parameterElement);
            return parameterElement;
        }

        /***************************************************/
    }
}

