/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
#if REVIT2021 || REVIT2022 || REVIT2023 || REVIT2024
        [Description("Gets the BuiltInParameterGroup of a Revit parameter definition.")]
        [Input("def", "A Revit parameter definition to get BuiltInParameterGroup for.")]
        [Output("parameterGroup", "BuiltInParameterGroup of a Revit parameter definition.")]
        public static BuiltInParameterGroup GroupTypeId(this Definition def)
        {
            return def.ParameterGroup;
        }
#else
        [Description("Gets the GroupTypeId of a Revit parameter definition.")]
        [Input("def", "A Revit parameter definition to get GroupTypeId for.")]
        [Output("groupTypeId", "GroupTypeId of a Revit parameter definition.")]
        public static ForgeTypeId GroupTypeId(this Definition def)
        {
            return def.GetGroupTypeId();
        }
#endif

        /***************************************************/
    }
}

