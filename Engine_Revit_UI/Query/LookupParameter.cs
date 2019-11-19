/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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
using System;
using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;


namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Parameter LookupParameter(this Element element, IEnumerable<string> parameterNames)
        {
            foreach (string s in parameterNames)
            {
                Parameter p= element.LookupParameter(s);
                if (p != null && p.HasValue) return p;
            }
            return null;
        }

        /***************************************************/

        public static Parameter LookupParameter(this Element element, MapSettings mapSettings, Type type, string name, bool hasValue = true)
        {
            if (element == null || mapSettings == null || type == null)
                return null;

            IEnumerable<string> aNames = BH.Engine.Adapters.Revit.Query.Names(mapSettings, type, name);
            if (aNames == null || aNames.Count() == 0)
                return null;

            Parameter aResult = null;
            foreach (string aName in aNames)
            {
                Parameter aParameter = element.LookupParameter(aName);
                if (aParameter == null)
                    continue;

                if (!hasValue && !aParameter.HasValue)
                    continue;

                if(aResult == null)
                    aResult = aParameter;

                if (aParameter.HasValue)
                    return aParameter;
            }

            return aResult;
        }

        /***************************************************/
    }
}