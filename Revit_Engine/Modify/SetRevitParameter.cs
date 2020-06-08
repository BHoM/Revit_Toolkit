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

using BH.oM.Base;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Reflection.Attributes;
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

        //[Description("Sets tag to BHoMObject.")]
        //[Input("bHoMObject", "BHoMObject to be modified.")]
        //[Input("tag", "Tag to be set.")]
        //[Output("bHoMObject")]
        public static IBHoMObject SetRevitParameters(this IBHoMObject bHoMObject, List<string> names, List<object> values)
        {
            if (bHoMObject == null)
                return null;

            if (names.Count != values.Count)
            {
                BH.Engine.Reflection.Compute.RecordError("Number of input names needs to be equal to the number of input values. Parameters have not been set.");
                return bHoMObject;
            }

            IBHoMObject obj = bHoMObject.GetShallowClone();
            RevitParametersToPush fragment = obj.Fragments.FirstOrDefault(x => x is RevitParametersToPush) as RevitParametersToPush;
            if (fragment == null)
            {
                fragment = new RevitParametersToPush();
                obj.Fragments.Add(fragment);
            }

            for (int i = 0; i < names.Count; i++)
            {
                RevitParameter param = new RevitParameter { Name = names[i], Value = values[i] };

                RevitParameter existingParam = fragment.Parameters.FirstOrDefault(x => x.Name == names[i]);
                if (existingParam != null)
                    fragment.Parameters[fragment.Parameters.IndexOf(existingParam)] = param;
                else
                    fragment.Parameters.Add(param);
            }

            return obj;
        }

        /***************************************************/
    }
}

