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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
                        RevitParameter bHoMParameter = parameter.ParameterFromRevit(typeParameterLinks, true);
                        if (bHoMParameter != null)
                            parameters.Add(bHoMParameter);
                    }
                }

                parameterLinks = parameterMap.ParameterLinks.Where(x => !(x is ElementTypeParameterLink));
            }

            IEnumerable elementParams = element.ParametersMap;
            if (((Autodesk.Revit.DB.ParameterMap)elementParams).IsEmpty)
                elementParams = element.Parameters;

            foreach (Parameter parameter in elementParams)
            {
                parameters.Add(parameter.ParameterFromRevit(parameterLinks, false));
            }

            bHoMObject.Fragments.Add(new RevitPulledParameters(parameters));
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
            
            Type type = bHoMObject.GetType();
            BH.oM.Adapters.Revit.Parameters.ParameterMap parameterMap = settings?.ParameterSettings?.ParameterMap(type);

            IEnumerable<PropertyInfo> propertyInfos = type.MapPropertyInfos();

            if (propertyInfos != null)
            {
                propertyInfos = propertyInfos.Where(x => x.PropertyType != typeof(double)).Union(propertyInfos.Where(x => x.PropertyType == typeof(double)));
                foreach (PropertyInfo pInfo in propertyInfos)
                {
                    object value = pInfo.GetValue(bHoMObject);

                    HashSet<string> parameterNames = settings.ParameterSettings.ParameterNames(type, pInfo.Name, false);
                    if (parameterNames != null && element.SetParameters(parameterNames, value))
                        continue;

                    if (elementType == null)
                        continue;

                    parameterNames = settings.ParameterSettings.ParameterNames(type, pInfo.Name, true);
                    if (parameterNames != null)
                        element.SetParameters(parameterNames, value);
                }
            }

            // Sort the parameters so that doubles get assigned last, to make sure all reference levels etc. go first.
            foreach (RevitParameter param in fragment.Parameters.Where(x => !(x.Value is double)).Union(fragment.Parameters.Where(x => x.Value is double)))
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

            element.Document.Regenerate();
        }

        /***************************************************/

        public static void CopyParameters(this Element elementFrom, Element elementTo, IEnumerable<BuiltInParameter> toSkip = null)
        {
            List<Parameter> fromParams = elementFrom.GetOrderedParameters().ToList<Parameter>();
            List<Parameter> toParams = elementTo.GetOrderedParameters().ToList<Parameter>();

            foreach (Parameter paramFrom in fromParams)
            {
                Parameter paramTo = toParams.FirstOrDefault(x => x.Id.IntegerValue == paramFrom.Id.IntegerValue);
                if (paramTo == null || paramTo.IsReadOnly || (toSkip != null && toSkip.Any(x => (int)x == paramTo.Id.IntegerValue)))
                    continue;

                switch (paramFrom.StorageType)
                {
                    case StorageType.Double:
                        paramTo.Set(paramFrom.AsDouble());
                        break;
                    case StorageType.ElementId:
                        paramTo.Set(paramFrom.AsElementId());
                        break;
                    case StorageType.Integer:
                        paramTo.Set(paramFrom.AsInteger());
                        break;
                    case StorageType.String:
                        paramTo.Set(paramFrom.AsString());
                        break;
                    case StorageType.None:
                        paramTo.Set(paramFrom.AsValueString());
                        break;
                }
            }
        }
    }
}

