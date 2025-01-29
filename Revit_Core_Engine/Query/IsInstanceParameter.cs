/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Determines if a given parameter is an instance parameter for the specified element.")]
        [Input("parameter", "parameter under investigate")]
        [Input("element", "element associated with the parameter")]
        [Output("bool", "true/false")]
        public static bool IsInstanceParameter(this Parameter parameter, Element element)
        {
            Document doc = parameter.Element.Document;
            Binding binding = doc.ParameterBindings.get_Item(parameter.Definition);

            //for project parameters
            if (binding != null && binding is InstanceBinding)
            {
                return true;
            }

            if (binding != null && binding is TypeBinding)
            {
                return false;
            }

            BuiltInParameter bip = (BuiltInParameter)parameter.Id.IntegerValue;
            BuiltInCategory bic = (BuiltInCategory)element.Category.Id.IntegerValue;

            ElementType elementType;
            Element elementInstance;
            Parameter typeParameter, instanceParameter = null;

            //check if element is ElementType or ElementInstance
            if (element is ElementType)
            {
                elementType = element as ElementType;
                typeParameter = elementType.get_Parameter(bip);
            }
            else
            {
                elementType = doc.GetElement(element.GetTypeId()) as ElementType;
                elementInstance = element;
                typeParameter = elementType.get_Parameter(bip);
                instanceParameter = elementInstance.get_Parameter(bip);
            }

            //return cached value if available
            if (Enum.IsDefined(typeof(BuiltInParameter), bip) && m_CachedBuiltInParameters.ContainsKey((bip, bic)))
                return m_CachedBuiltInParameters[(bip, bic)];

            if (!Enum.IsDefined(typeof(BuiltInParameter), bip) && m_CachedUserDefinedParameters.ContainsKey((parameter.Id, elementType.Id)))
                return m_CachedUserDefinedParameters[(parameter.Id, elementType.Id)];

            //check TypeParameter and InstanceParameter
            if (typeParameter != null && typeParameter.StorageType != StorageType.None)
            {
                if (Enum.IsDefined(typeof(BuiltInParameter), bip))
                    m_CachedBuiltInParameters.Add((bip, bic), false);
                else
                    m_CachedUserDefinedParameters.Add((parameter.Id, elementType.Id), false);
                return false;
            }

            if (instanceParameter != null && instanceParameter.StorageType != StorageType.None)
            {
                if (Enum.IsDefined(typeof(BuiltInParameter), bip))
                    m_CachedBuiltInParameters.Add((bip, bic), true);
                else
                    m_CachedUserDefinedParameters.Add((parameter.Id, elementType.Id), true);
                return true;
            }

            BH.Engine.Base.Compute.RecordError($"Parameter {parameter.Definition.Name} is undetermined");
            return false;
        }

        /***************************************************/
        /****              Private fields               ****/
        /***************************************************/

        private static Dictionary<(BuiltInParameter, BuiltInCategory), bool> m_CachedBuiltInParameters = new Dictionary<(BuiltInParameter, BuiltInCategory), bool>();

        private static Dictionary<(ElementId, ElementId), bool> m_CachedUserDefinedParameters = new Dictionary<(ElementId, ElementId), bool>();

        /***************************************************/
    }
}



