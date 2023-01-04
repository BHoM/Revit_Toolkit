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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public Methods               ****/
        /***************************************************/

        [Description("Converts the element type of the source Revit element to RevitTypeFragment and attaches it to the target BHoM object.")]
        [Input("source", "Revit element to get the element type from.")]
        [Input("target", "Target BHoM to set the Revit type fragment to.")]
        [Input("settings", "Revit adapter settings to be used while performing the operation.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        public static void CopyTypeToFragment(this Element source, BHoMObject target, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            ElementType type = source.Document.GetElement(source.GetTypeId()) as ElementType;
            if (type != null)
                target.Fragments.Add(type.TypeFragmentFromRevit(settings, refObjects));
            else
                BH.Engine.Base.Compute.RecordWarning($"Revit element type could not be extracted from the Revit element, so it has been skipped on convert. Element id: {source.Id.IntegerValue}");
        }

        /***************************************************/
    }
}



