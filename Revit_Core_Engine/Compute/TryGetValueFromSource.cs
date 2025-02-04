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
using BH.oM.Adapters.Revit;
using BH.oM.Base;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public Methods               ****/
        /***************************************************/

        //[Description("Extracts a value from an object based on the instruction embedded in the provided " + nameof(PropertyValueSource) + ".")]
        //[Input("obj", "Object to extract the value from.")]
        //[Input("valueSource", "Object defining how to extract the value from the input object.")]
        //[Input("errorIfNotFound", "If true, error will be raised in case the value could not be found, otherwise not.")]
        //[Output("value", "Value extracted from the input object based on the provided instruction.")]
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
                element = element.Document.GetElement(element.GetTypeId());

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
