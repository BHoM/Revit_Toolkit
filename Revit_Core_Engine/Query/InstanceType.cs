/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Looks for the instance type correspondent to a given Revit element type, e.g. Wall for WallType.")]
        [Input("elementType", "Revit element type to find an instance type for.")]
        [Output("instanceType", "Revit instance type correspondent to the input element type.")]
        public static Type InstanceType(this ElementType elementType)
        {
            Type type = elementType.GetType();
            if (InstanceTypes.ContainsKey(type))
                return InstanceTypes[type];
            else
                return null;
        }


        /***************************************************/
        /****            Private collection             ****/
        /***************************************************/

        private static Dictionary<Type, Type> InstanceTypes = new Dictionary<Type, Type>
        {
            { typeof(WallType), typeof(Wall) },
            { typeof(FloorType), typeof(Floor) },
            { typeof(RoofType), typeof(RoofBase) },
            { typeof(CeilingType), typeof(Ceiling) },
            { typeof(CurtainSystemType), typeof(CurtainSystem) },
            { typeof(PanelType), typeof(Panel) },
            { typeof(MullionType), typeof(Mullion) },
            { typeof(DuctType), typeof(Duct) },
            { typeof(FlexDuctType), typeof(FlexDuctType) },
            { typeof(DuctInsulationType), typeof(DuctInsulation) },
            { typeof(PipeType), typeof(Pipe) },
            { typeof(FlexPipeType), typeof(FlexPipe) },
            { typeof(PipeInsulationType), typeof(PipeInsulation) },
            { typeof(ConduitType), typeof(Conduit) },
            { typeof(CableTrayType), typeof(CableTray) },
            { typeof(GroupType), typeof(Group) },
        };
        
        /***************************************************/
    }
}



