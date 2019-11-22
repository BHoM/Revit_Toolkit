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

            IBHoMObject aBHoMObject = bHoMObject.GetShallowClone() as IBHoMObject;

            foreach (Parameter aParameter in element.ParametersMap)
                aBHoMObject = SetCustomData(aBHoMObject, aParameter, namePrefix);

            return aBHoMObject;
        }

        /***************************************************/
        
        public static IBHoMObject SetCustomData(this IBHoMObject bHoMObject, Element element, BuiltInParameter builtInParameter)
        {
            if (bHoMObject == null || element == null)
                return bHoMObject;

            IBHoMObject aBHoMObject = bHoMObject.GetShallowClone() as IBHoMObject;

            aBHoMObject = SetCustomData(aBHoMObject, element.get_Parameter(builtInParameter));
                

            return aBHoMObject;
        }

        /***************************************************/

        public static IBHoMObject SetCustomData(this IBHoMObject bHoMObject, Parameter parameter, string namePrefix = null)
        {
            if (bHoMObject == null || parameter == null)
                return bHoMObject;

            IBHoMObject aBHoMObject = bHoMObject.GetShallowClone() as IBHoMObject;

            object aValue = null;
            switch (parameter.StorageType)
            {
                case StorageType.Double:
                    aValue = parameter.AsDouble();
                    break;
                case StorageType.ElementId:
                    ElementId aElementId = parameter.AsElementId();
                    if (aElementId != null)
                        aValue = aElementId.IntegerValue;
                    break;
                case StorageType.Integer:
                    if (parameter.Definition.ParameterType == ParameterType.YesNo)
                        aValue = parameter.AsInteger() == 1;
                    else
                        aValue = parameter.AsInteger();
                    break;
                case StorageType.String:
                    aValue = parameter.AsString();
                    break;
                case StorageType.None:
                    aValue = parameter.AsValueString();
                    break;
            }

            string aName = parameter.Definition.Name;
            if (!string.IsNullOrEmpty(namePrefix))
                aName = namePrefix + aName;

            aBHoMObject.CustomData[aName] = aValue;

            return aBHoMObject;
        }

        /***************************************************/

        public static IBHoMObject SetCustomData(this IBHoMObject bHoMObject, string customDataName, object value)
        {
            if (bHoMObject == null || string.IsNullOrEmpty(customDataName))
                return bHoMObject;

            IBHoMObject aBHoMObject = bHoMObject.GetShallowClone() as IBHoMObject;

            if (aBHoMObject.CustomData.ContainsKey(customDataName))
                aBHoMObject.CustomData[customDataName] = value;
            else
                aBHoMObject.CustomData.Add(customDataName, value);

            return aBHoMObject;
        }

        /***************************************************/
    }
}