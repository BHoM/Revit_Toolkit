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

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public Methods               ****/
        /***************************************************/

        [Description("Converts the element type of the source Revit spatial element to RevitTypeFragment and attaches it to the target BHoM object." +
                     "\nPrimary source of the type information is Space Type parameter of the source Revit element.")]
        [Input("source", "Revit spatial element to get the element type from.")]
        [Input("target", "Target BHoM to set the Revit type fragment to.")]
        [Input("settings", "Revit adapter settings to be used while performing the operation.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        public static void CopySpatialElementTypeToFragment(this SpatialElement source, BHoMObject target, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();
            ElementId spaceTypeId = source.get_Parameter(BuiltInParameter.ROOM_SPACE_TYPE_PARAM)?.AsElementId();
            if (spaceTypeId != ElementId.InvalidElementId)
            {
                HVACLoadType type = source.Document.GetElement(spaceTypeId) as HVACLoadType;
                target.Fragments.Add(type.TypeFragmentFromRevit(settings, refObjects));
            }
            else
                source.CopyTypeToFragment(target, settings, refObjects);
        }

        /***************************************************/
    }
}


