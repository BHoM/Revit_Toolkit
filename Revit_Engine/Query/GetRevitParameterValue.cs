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

using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;

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
                if (BH.Engine.Reflection.Query.PropertyNames(identifierFragment).Contains(paramName))
                    return Base.Query.PropertyValue(identifierFragment, paramName);
            }

            Dictionary <string, object> bHoMPropDic = BH.Engine.Reflection.Query.PropertyDictionary(bHoMObject);
            foreach (KeyValuePair<string, object> bHoMPropEntry in bHoMPropDic)
            {
                IBHoMObject bHoMProp = bHoMPropEntry.Value as IBHoMObject;
                if (bHoMProp != null)
                {
                    RevitPulledParameters typePullFragment = bHoMProp.Fragments?.FirstOrDefault(x => x is RevitPulledParameters) as RevitPulledParameters;
                    if (typePullFragment?.Parameters != null)
                    {
                        RevitParameter param = typePullFragment.Parameters.FirstOrDefault(x => x.Name == parameterName);
                        if (param != null)
                        {
                            Engine.Base.Compute.RecordWarning("The value for parameter " + parameterName + " for the object with BHoM_Guid " + bHoMObject.BHoM_Guid + " has been retrieved from its property " + bHoMPropEntry.Key + ".");
                            return param.Value;
                        }
                    }
                }
            }

            return null;
        }

        /***************************************************/
    }
}




