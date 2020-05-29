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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Generic;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static void SetCustomData(this IBHoMObject bHoMObject, Element element, ParameterSettings settings = null)
        {
            if (bHoMObject == null || element == null)
                return;

            oM.Adapters.Revit.Generic.ParameterMap parameterMap = settings?.ParameterMap(bHoMObject.GetType());
            IEnumerable<IParameterLink> parameterLinks = null;
            if (parameterMap != null)
            {
                Element elementType = element.Document.GetElement(element.GetTypeId());
                IEnumerable<IParameterLink> typeParameterLinks = parameterMap.ParameterLinks.Where(x => x is ElementTypeParameterLink);
                if (elementType != null && typeParameterLinks.Count() != 0)
                {
                    foreach (Parameter parameter in elementType.ParametersMap)
                    {
                        bHoMObject.SetCustomData(parameter, typeParameterLinks);
                    }
                }

                parameterLinks = parameterMap.ParameterLinks.Where(x => !(x is ElementTypeParameterLink));
            }

            foreach (Parameter parameter in element.ParametersMap)
            {
                bHoMObject.SetCustomData(parameter, parameterLinks);
            }
        }
        
        /***************************************************/

        public static void SetCustomData(this IBHoMObject bHoMObject, Parameter parameter, IEnumerable<IParameterLink> parameterLinks = null)
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

            IParameterLink parameterLink = parameterLinks.ParameterLink(parameter);
            if (parameterLink != null)
                name = parameterLink.PropertyName;

            bHoMObject.CustomData[name] = value;
        }

        /***************************************************/
    }
}
