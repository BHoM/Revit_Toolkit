/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Find all relevant convert methods from Revit to BHoM, which return a BHoM object or a collection of them, and take Revit Element, RevitSettings and refObjects (in this exact order).")]
        [Output("methods", "All relevant Revit => BHoM convert methods.")]
        public static Dictionary<Tuple<Type, Type>, MethodInfo> AllConvertMethods()
        {
            if (m_ConvertMethods == null)
            {
                m_ConvertMethods = new Dictionary<Tuple<Type, Type>, MethodInfo>();

                BindingFlags bindingBHoM = BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static;
                foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (t.IsInterface || !t.IsAbstract || t.Name != "Convert")
                        continue;

                    MethodInfo[] typeMethods = t.GetMethods(bindingBHoM);
                    foreach (MethodInfo mi in typeMethods.Where(x => x.Name.EndsWith("FromRevit")))
                    {
                        Type to = mi.ReturnType;
                        if (!typeof(IBHoMObject).IsAssignableFrom(to) && !typeof(IEnumerable<IBHoMObject>).IsAssignableFrom(to))
                            continue;

                        ParameterInfo[] parameters = mi.GetParameters();
                        if (parameters?.Length != 3)
                            continue;

                        Type from = parameters[0].ParameterType;

                        // Skip the fallback ObjectFromRevit method
                        if (to == typeof(IBHoMObject) && from == typeof(Element))
                            continue;

                        if (typeof(Element).IsAssignableFrom(from) && parameters[1].ParameterType == typeof(RevitSettings) && parameters[2].ParameterType == typeof(Dictionary<string, List<IBHoMObject>>))
                            m_ConvertMethods.Add(new Tuple<Type, Type>(from, to), mi);
                    }
                }
            }

            return m_ConvertMethods;
        }


        /***************************************************/
        /****              Private fields               ****/
        /***************************************************/

        private static Dictionary<Tuple<Type, Type>, MethodInfo> m_ConvertMethods = null;

        /***************************************************/
    }
}




