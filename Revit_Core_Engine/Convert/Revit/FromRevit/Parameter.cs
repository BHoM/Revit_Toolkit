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
using BH.oM.Adapters.Revit.Mapping;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit Parameter to BH.oM.Adapters.Revit.Parameters.RevitParameter.")]
        [Input("parameter", "Revit Parameter to be converted.")]
        [Input("parameterLinks", "A collection of names of RevitParameters and sets of their correspondent Revit parameter names to be used on name mapping.")]
        [Input("onlyLinked", "If true, there needs to be a valid, relevant parameter link in parameterLinks in order for convert to succeed.")]
        [Output("parameter", "BH.oM.Adapters.Revit.Parameters.RevitParameter resulting from converting the input Revit Parameter.")]
        public static RevitParameter ParameterFromRevit(this Parameter parameter, IEnumerable<IParameterLink> parameterLinks = null, bool onlyLinked = false)
        {
            if (parameter == null)
                return null;

            string name = parameter.Definition.Name;

            IParameterLink parameterLink = parameterLinks.ParameterLink(parameter);
            if (parameterLink != null)
                name = parameterLink.PropertyName;
            else if (onlyLinked)
                return null;

            object value = null;
            switch (parameter.StorageType)
            {
                case StorageType.Double:
                    value = parameter.AsDouble().ToSI(parameter.Definition.GetSpecTypeId());
                    break;
                case StorageType.ElementId:
                    ElementId elementID = parameter.AsElementId();
                    if (elementID != null)
                        value = elementID.IntegerValue;
                    break;
                case StorageType.Integer:
                    if (parameter.Definition.ParameterType == ParameterType.YesNo)
                        value = parameter.AsInteger() == 1;
                    else if (parameter.Definition.ParameterType == ParameterType.Invalid)
                        value = parameter.AsValueString();
                    else
                        value = parameter.AsInteger();
                    break;
                case StorageType.String:
                    value = parameter.AsString();
                    break;
                case StorageType.None:
                    value = parameter.AsValueString();
                    break;
            }

            return new RevitParameter { Name = name, Value = value };
        }

        /***************************************************/
    }
}


