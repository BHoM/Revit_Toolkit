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
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts a string value from a given Revit Parameter.")]
        [Input("parameter", "Revit Parameter to extract the string value from.")]
        [Output("value", "String value extracted from the input Revit Parameter.")]
        public static string StringValue(this Parameter parameter)
        {
            if (parameter != null && parameter.HasValue)
            {
                switch (parameter.StorageType)
                {
                    case (StorageType.Double):
                        return parameter.AsValueString();
                    case (StorageType.Integer):
                        {
                            if (parameter.IsBooleanParameter() || parameter.Definition.ParameterType() == null)
                                return parameter.AsValueString();
                            else
                                return parameter.AsInteger().ToString();
                        }
                    case (StorageType.String):
                        return parameter.AsString();
                    case (StorageType.ElementId):
                        return parameter.AsValueString();
                }
            }

            return null;
        }

        /***************************************************/
    }
}





