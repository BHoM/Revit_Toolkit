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

        [Description("Creates IRequest that filters elements specific to (owned by) a given Revit view.")]
        [Input("view", "BHoMObject that represents the pulled Revit view (contains its ElementId in RevitIdentifiers).")]
        [Output("request", "Created request.")]
        public static FilterByViewSpecific FilterByViewSpecific(IBHoMObject view)
        {
            int elementId = view.ElementId();
            if (elementId == -1)
            {
                BH.Engine.Base.Compute.RecordError(String.Format("Valid ElementId has not been found. BHoM Guid: {0}", view.BHoM_Guid));
                return null;
            }
            else
                return new FilterByViewSpecific { ViewId = elementId };
        }

        /***************************************************/
    }
}


