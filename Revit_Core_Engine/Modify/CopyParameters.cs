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
using BH.oM.Adapters.Revit.Parameters;
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

        public static void CopyParameters(this IBHoMObject bHoMObject, Element element, ParameterSettings settings = null)
        {
            if (bHoMObject == null || element == null)
                return;

            List<RevitParameter> parameters = new List<RevitParameter>();
            oM.Adapters.Revit.Parameters.ParameterMap parameterMap = settings?.ParameterMap(bHoMObject.GetType());
            IEnumerable<IParameterLink> parameterLinks = null;
            if (parameterMap != null)
            {
                Element elementType = element.Document.GetElement(element.GetTypeId());
                IEnumerable<IParameterLink> typeParameterLinks = parameterMap.ParameterLinks.Where(x => x is ElementTypeParameterLink);
                if (elementType != null && typeParameterLinks.Count() != 0)
                {
                    foreach (Parameter parameter in elementType.ParametersMap)
                    {
                        parameters.Add(parameter.ParameterFromRevit(typeParameterLinks));
                    }
                }

                parameterLinks = parameterMap.ParameterLinks.Where(x => !(x is ElementTypeParameterLink));
            }

            foreach (Parameter parameter in element.ParametersMap)
            {
                parameters.Add(parameter.ParameterFromRevit(parameterLinks));
            }

            bHoMObject.Fragments.Add(new RevitPulledParameters(parameters));
        }

        /***************************************************/

        public static void CopyParameters(this Element element, IBHoMObject bHoMObject, IEnumerable<BuiltInParameter> builtInParametersIgnore = null)
        {
            if (bHoMObject == null || element == null)
                return;

            foreach (KeyValuePair<string, object> kvp in bHoMObject.CustomData)
            {
                IList<Parameter> parameters = element.GetParameters(kvp.Key);
                if (parameters == null || parameters.Count == 0)
                    continue;

                foreach (Parameter parameter in parameters)
                {
                    if (parameter == null || parameter.IsReadOnly)
                        continue;

                    if (builtInParametersIgnore != null && parameter.Id.IntegerValue < 0 && builtInParametersIgnore.Contains((BuiltInParameter)parameter.Id.IntegerValue))
                        continue;

                    SetParameter(parameter, kvp.Value, element.Document);
                }
            }
        }

        /***************************************************/

        public static void CopyParameters(this Element element, IBHoMObject bHoMObject, RevitSettings settings = null)
        {
            if (bHoMObject == null || element == null)
                return;

            settings = settings.DefaultIfNull();

            RevitParametersToPush fragment = bHoMObject.Fragments.FirstOrDefault(x => x is RevitParametersToPush) as RevitParametersToPush;
            if (fragment == null)
                return;

            ElementType elementType = element.Document.GetElement(element.GetTypeId()) as ElementType;
            BH.oM.Adapters.Revit.Parameters.ParameterMap parameterMap = settings?.ParameterSettings?.ParameterMap(bHoMObject.GetType());
            {
                foreach (RevitParameter param in fragment.Parameters)
                {
                    IEnumerable<IParameterLink> parameterLinks = parameterMap.ParameterLinks(param.Name);
                    if (parameterLinks != null)
                    {
                        foreach (IParameterLink parameterLink in parameterLinks)
                        {
                            if (parameterLink is ElementParameterLink)
                                element.SetParameters(parameterLink.ParameterNames, param.Value);
                            else if (elementType != null)
                                elementType.SetParameters(parameterLink.ParameterNames, param.Value);
                        }
                    }
                    else
                        element.SetParameters(param.Name, param.Value);
                }
            }


            //foreach (KeyValuePair<string, object> kvp in bHoMObject.CustomData)
            //{
            //    IList<Parameter> parameters = element.GetParameters(kvp.Key);
            //    if (parameters == null || parameters.Count == 0)
            //        continue;

            //    foreach (Parameter parameter in parameters)
            //    {
            //        if (parameter == null || parameter.IsReadOnly)
            //            continue;

            //        if (builtInParametersIgnore != null && parameter.Id.IntegerValue < 0 && builtInParametersIgnore.Contains((BuiltInParameter)parameter.Id.IntegerValue))
            //            continue;

            //        SetParameter(parameter, kvp.Value, element.Document);
            //    }
            //}
        }

        /***************************************************/
    }
}
