/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using Autodesk.Revit.DB.Structure;

using BH.oM.Base;
using BH.oM.Physical.Elements;
using System;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static ElementFilter StructuralTypeFilter(this ElementFilter filter, Type bHoMType)
        {
            if (bHoMType == typeof(Column))
                return new LogicalOrFilter(filter, new ElementStructuralTypeFilter(StructuralType.Column));
            else if (bHoMType == typeof(Bracing))
                return new LogicalOrFilter(filter, new ElementStructuralTypeFilter(StructuralType.Brace));
            else if (bHoMType == typeof(Beam))
                return new LogicalAndFilter(new List<ElementFilter> { filter, new ElementStructuralTypeFilter(StructuralType.Brace, true), new ElementStructuralTypeFilter(StructuralType.Column, true), new ElementStructuralTypeFilter(StructuralType.Footing, true) });
            else
                return filter;
        }

        /***************************************************/
    }
}


