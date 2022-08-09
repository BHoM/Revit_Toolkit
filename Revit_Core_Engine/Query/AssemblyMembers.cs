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

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns all member (children) objects of a given BHoM Assembly object. Nesting of assemblies is not allowed, so nested assemblies get squashed with a warning.")]
        [Input("assembly", "Assembly to be queried for its members.")]
        [Output("members", "All member (children) objects of the input BHoM Assembly object.")]
        public static List<IBHoMObject> AssemblyMembers(this Assembly assembly)
        {
            List<IBHoMObject> result = new List<IBHoMObject>();
            foreach (IBHoMObject member in assembly.MemberElements)
            {
                if (member is Assembly)
                {
                    BH.Engine.Base.Compute.RecordWarning("Nested assemblies are not allowed - they got squashed with their members being added into the top assembly.");
                    result.AddRange(AssemblyMembers((Assembly)member));
                }
                else
                    result.Add(member);
            }

            return result;
        }

        /***************************************************/
    }
}
