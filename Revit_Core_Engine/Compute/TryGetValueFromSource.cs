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
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Revit.Parameters;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public Methods               ****/
        /***************************************************/

        [Description("Tries to extract a value from an object based on the instruction embedded in the provided " + nameof(ParameterValueSource) + ".")]
        [Input("obj", "Object to extract the value from.")]
        [Input("valueSource", "Object defining how to extract the value from the input object.")]
        [MultiOutput(0, "found", "True if value source exists in the input object (i.e. value could be extracted from the object), otherwise false.")]
        [MultiOutput(1, "value", "Value extracted from the input object based on the provided instruction.")]
        public static Output<bool, object> TryGetValueFromSource(this Element element, ParameterValueSource valueSource)
        {
            if (element == null)
            {
                BH.Engine.Base.Compute.RecordError("Could not extract value from a null element.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(valueSource?.ParameterName))
            {
                BH.Engine.Base.Compute.RecordError("Could not extract value based on an empty value source.");
                return null;
            }

            if (valueSource.FromType)
            {
                Element type = element.Document.GetElement(element.GetTypeId());
                if (type != null)
                    element = type;
                else
                {
                    BH.Engine.Base.Compute.RecordNote($"Element with id {element.Id} does not have a type, so type parameter cannot be queried.");
                    return new Output<bool, object> { Item1 = false, Item2 = null };
                }
            }

            Parameter param = element?.LookupParameter(valueSource.ParameterName);
            if (param == null)
            {
                BH.Engine.Base.Compute.RecordNote($"Element with id {element.Id} does not have a parameter named {valueSource.ParameterName}.");
                return new Output<bool, object> { Item1 = false, Item2 = null };
            }

            return new Output<bool, object> { Item1 = true, Item2 = param.ParameterValue() };
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static object ParameterValue(this Parameter parameter)
        {
            switch (parameter.StorageType)
            {
                case StorageType.Double:
                    return parameter.AsDouble().ToSI(parameter.Definition.GetDataType());
                case StorageType.ElementId:
                    return parameter.AsValueString();
                case StorageType.Integer:
                    if (parameter.Definition.ParameterType() == null)
                        return parameter.AsValueString();
                    else if (parameter.IsBooleanParameter())
                        return parameter.AsInteger() == 1;
                    else
                        return parameter.AsInteger();
                case StorageType.String:
                    return parameter.AsString();
                default:
                    return parameter.AsValueString();
            }
        }

        /***************************************************/
    }
}
