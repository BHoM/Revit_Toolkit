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

using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Retrieves value of a parameter attached to a BHoM object. If a parameter with given name exists in both collections of pulled parameters and the ones to push, the latter is returned.")]
        [Input("bHoMObject", "BHoMObject to which the parameters will be attached.")]
        [Input("parameterName", "Name of the parameter to be sought for.")]
        [Output("value")]
        public static object GetRevitParameterValue(this IBHoMObject bHoMObject, string parameterName)
        {
            if (bHoMObject == null)
                return null;
            
            RevitParametersToPush pushFragment = bHoMObject.Fragments?.FirstOrDefault(x => x is RevitParametersToPush) as RevitParametersToPush;
            if (pushFragment?.Parameters != null)
            {
                RevitParameter param = pushFragment.Parameters.FirstOrDefault(x => x.Name == parameterName);
                if (param != null)
                    return param.Value;
            }

            RevitPulledParameters pullFragment = bHoMObject.Fragments?.FirstOrDefault(x => x is RevitPulledParameters) as RevitPulledParameters;
            if (pullFragment?.Parameters != null)
            {
                RevitParameter param = pullFragment.Parameters.FirstOrDefault(x => x.Name == parameterName);
                if (param != null)
                    return param.Value;
            }

            RevitIdentifiers identifierFragment = bHoMObject.Fragments?.FirstOrDefault(x => x is RevitIdentifiers) as RevitIdentifiers;
            if (identifierFragment != null)
            {
                string paramName = string.Concat(parameterName.Where(c => !char.IsWhiteSpace(c)));
                if (Reflection.Query.PropertyNames(identifierFragment).Contains(paramName))
                    return Reflection.Query.PropertyValue(identifierFragment, paramName);
            }

            return null;
        }

        /***************************************************/
    }
}

