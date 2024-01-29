/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using BH.oM.Physical.Elements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        //[Description("Converts BH.oM.Adapters.Revit.Elements.Assembly to a Revit AssemblyInstance." +
        //             "\nOnly the assembly instance itself is created, while any updates of name or parameters need to happen in a separate transaction, which is caused by Revit API limitations.")]
        //[Input("assembly", "BH.oM.Adapters.Revit.Elements.Assembly to be converted.")]
        //[Input("document", "Revit document, in which the output of the convert will be created.")]
        //[Input("settings", "Revit adapter settings to be used while performing the convert.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("assemblyInstance", "Revit AssemblyInstance resulting from converting the input BH.oM.Adapters.Revit.Elements.Assembly.")]
        public static ParameterFilterElement ToRevitParameterFilterElement(this oM.Adapters.Revit.Elements.ViewFilter filter, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            ParameterFilterElement revitFilter = refObjects.GetValue<ParameterFilterElement>(document, filter.BHoM_Guid);
            if (revitFilter != null)
                return revitFilter;

            //TODO: fill this in with the logic
            List<ElementId> categories = new List<ElementId>();
            revitFilter = ParameterFilterElement.Create(document, filter.Name, categories);

            revitFilter.CopyParameters(filter, settings);
            refObjects.AddOrReplace(filter, revitFilter);
            return revitFilter;
        }

        /***************************************************/
    }
}

