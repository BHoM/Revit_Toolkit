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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/


        [Description("Retrieves parameters that are attached to a BHoM object. If a parameter with given name exists in both collections of pulled parameters and the ones to push, the latter is returned.")]
        [Input("bHoMObject", "BHoMObject to which the parameters will be attached.")]
        [Output("bHoMObject")]
        public static List<RevitParameter> GetRevitParameters(this BHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            RevitPulledParameters pullFragment = bHoMObject.Fragments.FirstOrDefault(x => x is RevitPulledParameters) as RevitPulledParameters;
            RevitParametersToPush pushFragment = bHoMObject.Fragments.FirstOrDefault(x => x is RevitParametersToPush) as RevitParametersToPush;

            List<RevitParameter> result = new List<RevitParameter>();
            if (pullFragment?.Parameters != null)
                result.AddRange(pullFragment.Parameters);

            if (pushFragment?.Parameters != null)
            {
                bool mixed = false;
                foreach (RevitParameter param in pushFragment.Parameters)
                {
                    int index = result.FindIndex(x => x.Name == param.Name);
                    if (index == -1)
                        result.Add(param);
                    else
                    {
                        mixed = true;
                        result.RemoveAt(index);
                        result.Add(param);
                    }
                }

                if (mixed)
                    BH.Engine.Reflection.Compute.RecordNote("Some of the parameters were retrieved from collection of pulled ones, some from the ones meant to be pushed.");
            }

            return result;
        }

        /***************************************************/
    }
}

