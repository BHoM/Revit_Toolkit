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

using BH.oM.Adapters.Revit.Requests;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates IRequest that filters elements from the given Revit link instance.")]
        [Input("linkInstance", "BHoMObject that represents the pulled Revit link instance (contains its ElementId in RevitIdentifiers fragment).")]
        [Output("request", "Created request.")]
        public static FilterByLink FilterByLink(IBHoMObject linkInstance)
        {
            int elementId = linkInstance.ElementId();
            if (elementId == -1)
            {
                BH.Engine.Base.Compute.RecordError($"Valid ElementId has not been found. BHoM Guid: {linkInstance.BHoM_Guid}");
                return null;
            }
            else
                return new FilterByLink { LinkName = elementId.ToString() };
        }

        /***************************************************/
    }
}





