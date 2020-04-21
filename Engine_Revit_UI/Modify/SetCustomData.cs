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

namespace BH.UI.Revit.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static void SetCustomData(this IBHoMObject bHoMObject, Element element, string namePrefix = null)
        {
            if (bHoMObject == null || element == null)
                return;

            foreach (Parameter parameter in element.ParametersMap)
            {
                bHoMObject.SetCustomData(parameter, namePrefix);
            }
        }

        /***************************************************/

        public static void SetCustomData(this IBHoMObject bHoMObject, Element element, BuiltInParameter builtInParameter)
        {
            if (bHoMObject == null || element == null)
                return;

            bHoMObject.SetCustomData(element.get_Parameter(builtInParameter));
        }

        /***************************************************/

        public static void SetCustomData(this IBHoMObject bHoMObject, Parameter parameter, string namePrefix = null)
        {
            if (bHoMObject == null || parameter == null)
                return;

            object value = null;
            switch (parameter.StorageType)
            {
                case StorageType.Double:
                    value = parameter.AsDouble().ToSI(parameter.Definition.UnitType);
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

            string name = parameter.Definition.Name;
            if (!string.IsNullOrEmpty(namePrefix))
                name = namePrefix + name;

            bHoMObject.CustomData[name] = value;
        }

        /***************************************************/
    }
}
