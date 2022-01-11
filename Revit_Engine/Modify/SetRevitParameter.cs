/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using BH.Engine.Base;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Attaches parameter to a BHoM object, which will be applied to a correspondent Revit element on Push.")]
        [Input("bHoMObject", "BHoMObject to which the parameter will be attached.")]
        [Input("name", "Name of the parameter to be attached.")]
        [Input("value", "Value of the parameter to be attached.")]
        [Output("bHoMObject")]
        public static IBHoMObject SetRevitParameter(this IBHoMObject bHoMObject, string paramName, object value)
        {
            if (bHoMObject == null)
                return null;

            List<RevitParameter> parameters = new List<RevitParameter> { new RevitParameter { Name = paramName, Value = value } };

            RevitParametersToPush existingFragment = bHoMObject.Fragments.FirstOrDefault(x => x is RevitParametersToPush) as RevitParametersToPush;
            if (existingFragment != null)
            {
                foreach (RevitParameter parameter in existingFragment.Parameters)
                {
                    if (parameter.Name != paramName)
                        parameters.Add(parameter);
                }
            }
            
            RevitParametersToPush fragment = new RevitParametersToPush { Parameters = parameters };

            IBHoMObject obj = bHoMObject.ShallowClone();
            obj.Fragments = new FragmentSet(bHoMObject.Fragments.Where(x => !(x is RevitParametersToPush)).ToList());
            obj.Fragments.Add(fragment);
            return obj;
        }
        
        /***************************************************/

        [Description("Attaches parameters to a BHoM object, which will be applied to a correspondent Revit element on Push.")]
        [Input("bHoMObject", "BHoMObject to which the parameters will be attached.")]
        [Input("names", "Names of the parameters to be attached.")]
        [Input("values", "Values of the parameters to be attached.")]
        [Output("bHoMObject")]
        public static IBHoMObject SetRevitParameters(this IBHoMObject bHoMObject, List<string> paramNames, List<object> values)
        {
            if (bHoMObject == null)
                return null;

            if (paramNames.Count != values.Count)
            {
                BH.Engine.Base.Compute.RecordError("Number of input names needs to be equal to the number of input values. Parameters have not been set.");
                return bHoMObject;
            }

            List<RevitParameter> parameters = paramNames.Zip(values, (x, y) => new RevitParameter { Name = x, Value = y }).ToList();

            RevitParametersToPush existingFragment = bHoMObject.Fragments.FirstOrDefault(x => x is RevitParametersToPush) as RevitParametersToPush;
            if (existingFragment != null)
            {
                foreach (RevitParameter parameter in existingFragment.Parameters)
                {
                    if (paramNames.All(x => parameter.Name != x))
                        parameters.Add(parameter);
                }
            }

            RevitParametersToPush fragment = new RevitParametersToPush { Parameters = parameters };

            IBHoMObject obj = bHoMObject.ShallowClone();
            obj.Fragments = new FragmentSet(bHoMObject.Fragments.Where(x => !(x is RevitParametersToPush)).ToList());
            obj.Fragments.Add(fragment);
            return obj;
        }

        /***************************************************/
    }
}



