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
        [Description("Creates FilterRequest which filters specified views")]
        [Input("revitViewType", "Revit View Type")]
        [Output("FilterRequest")]
        public static FilterRequest ViewFilterRequest(RevitViewType revitViewType)
        {
            FilterRequest aFilterRequest = new FilterRequest();
            aFilterRequest.Type = typeof(BHoMObject);
            aFilterRequest.Equalities[Convert.FilterRequest.RequestType] = RequestType.View;
            aFilterRequest.Equalities[Convert.FilterRequest.RevitViewType] = revitViewType;
            return aFilterRequest;
        }

        [Description("Creates FilterRequest which filters specified views")]
        [Input("viewTemplateName", "Revit View Template Name applied to the views to be pulled")]
        [Output("FilterRequest")]
        public static FilterRequest ViewFilterRequest(string viewTemplateName)
        {
            FilterRequest aFilterRequest = new FilterRequest();
            aFilterRequest.Type = typeof(BHoMObject);
            aFilterRequest.Equalities[Convert.FilterRequest.RequestType] = RequestType.View;
            aFilterRequest.Equalities[Convert.FilterRequest.ViewTemplateName] = viewTemplateName;
            return aFilterRequest;
        }
    }
}
