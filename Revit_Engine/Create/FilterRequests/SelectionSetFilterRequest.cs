/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using System.ComponentModel;

using BH.oM.Data.Requests;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using BH.Engine.Adapters.Revit;

namespace BH.Engine.Revit
{
    public static partial class Create
    {
        [Description("Creates FilterRequest which filters all elements by given Revit Selection Set Name.")]
        [Input("slectionSetName", "Revit Slection Set Name")]
        [Output("FilterRequest")]
        public static FilterRequest SelectionSetFilterRequest(string slectionSetName)
        {
            FilterRequest aFilterRequest = new FilterRequest();
            aFilterRequest.Type = typeof(BHoMObject);
            aFilterRequest.Equalities[Convert.FilterRequest.RequestType] = RequestType.SelectionSet;
            aFilterRequest.Equalities[Convert.FilterRequest.SelectionSetName] = slectionSetName;
            return aFilterRequest;
        }
    }
}
