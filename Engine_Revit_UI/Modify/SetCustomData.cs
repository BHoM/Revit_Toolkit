/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
        /****              Public methods               ****/
        /***************************************************/
        
        public static IBHoMObject SetCustomData(this IBHoMObject bHoMObject, Element element, string namePrefix = null)
        {
            if (bHoMObject == null || element == null)
                return bHoMObject;

            IBHoMObject obj = bHoMObject.GetShallowClone() as IBHoMObject;

            foreach (Parameter parameter in element.ParametersMap)
                obj = SetCustomData(obj, parameter, namePrefix);

            return obj;
        }

        /***************************************************/
        
        public static IBHoMObject SetCustomData(this IBHoMObject bHoMObject, Element element, BuiltInParameter builtInParameter)
        {
            if (bHoMObject == null || element == null)
                return bHoMObject;

            IBHoMObject obj = bHoMObject.GetShallowClone() as IBHoMObject;

            obj = SetCustomData(obj, element.get_Parameter(builtInParameter));

            return obj;
        }

        /***************************************************/

        public static IBHoMObject SetCustomData(this IBHoMObject bHoMObject, Parameter parameter, string namePrefix = null)
        {
            if (bHoMObject == null || parameter == null)
                return bHoMObject;

            IBHoMObject obj = bHoMObject.GetShallowClone() as IBHoMObject;

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

            obj.CustomData[name] = value;

            return obj;
        }

        /***************************************************/

        public static IBHoMObject SetCustomData(this IBHoMObject bHoMObject, string customDataName, object value)
        {
            if (bHoMObject == null || string.IsNullOrEmpty(customDataName))
                return bHoMObject;

            IBHoMObject obj = bHoMObject.GetShallowClone() as IBHoMObject;

            if (obj.CustomData.ContainsKey(customDataName))
                obj.CustomData[customDataName] = value;
            else
                obj.CustomData.Add(customDataName, value);

            return obj;
        }

        /***************************************************/
    }
}