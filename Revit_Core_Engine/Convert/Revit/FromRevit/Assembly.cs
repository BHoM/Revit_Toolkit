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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit AssemblyInstance to BH.oM.Adapters.Revit.Elements.Assembly.")]
        [Input("assemblyInstance", "Revit AssemblyInstance to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("assembly", "BH.oM.Adapters.Revit.Elements.Assembly resulting from converting the input Revit AssemblyInstance.")]
        public static Assembly AssemblyFromRevit(this AssemblyInstance assemblyInstance, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            Assembly assembly = refObjects.GetValue<Assembly>(assemblyInstance.Id);
            if (assembly != null)
                return assembly;

            assembly = new Assembly { Name = assemblyInstance.AssemblyTypeName };
            foreach (ElementId memberId in assemblyInstance.GetMemberIds())
            {
                List<IBHoMObject> members;
                if (refObjects == null || !refObjects.TryGetValue(memberId.ToString(), out members))
                {
                    BH.Engine.Base.Compute.RecordError("Assembly instance could not be converted from Revit because not all of its members were converted prior to it." +
                                                       "\nPlease make sure all member elements of an assembly instance get converted to BHoM and cached in refObjects before conversion of the instance itself.");
                    return null;
                }

                assembly.MemberElements.AddRange(members);
            }

            //Set identifiers, parameters & custom data
            assembly.SetIdentifiers(assemblyInstance);
            assembly.CopyParameters(assemblyInstance, settings.MappingSettings);
            assembly.SetProperties(assemblyInstance, settings.MappingSettings);

            refObjects.AddOrReplace(assemblyInstance.Id, assembly);
            return assembly;
        }

        /***************************************************/
    }
}

