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

            if (Enum.IsDefined(typeof(BuiltInParameter), bip) && m_BuiltInParameters.ContainsKey((bip, bic)))
            {
                return m_BuiltInParameters[(bip, bic)];
            }


            ElementType elementType;

            if (element is ElementType)
            {
                elementType = element as ElementType;
            }
            else
            {
                elementType = doc.GetElement(element.GetTypeId()) as ElementType;
            }

            Parameter typeParameter = elementType.get_Parameter(bip);
            if (elementType != null && typeParameter != null && typeParameter.StorageType != StorageType.None)
            {
                m_BuiltInParameters.Add((bip, bic), false);
                return false;
            }

            Parameter instanceParameter = element.get_Parameter(bip);
            if (instanceParameter != null && instanceParameter.StorageType != StorageType.None)
            {
                m_BuiltInParameters.Add((bip, bic), true);
                return true;
            }

            BH.Engine.Base.Compute.RecordError($"Parameter {parameter.Definition.Name} is undetermined");
            return false;
        }

        /***************************************************/
        /****              Private fields               ****/
        /***************************************************/

        private static readonly Dictionary<(BuiltInParameter,BuiltInCategory), bool> m_BuiltInParameters = new Dictionary<(BuiltInParameter, BuiltInCategory), bool>();

        /***************************************************/
    }
}



