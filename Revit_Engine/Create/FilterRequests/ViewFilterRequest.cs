/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        [Description("Creates FilterRequest which filters specified views")]
        [Input("revitViewType", "Revit View Type")]
        [Output("FilterRequest")]
        public static FilterRequest ViewFilterRequest(RevitViewType revitViewType)
        {
            FilterRequest filterReqeust = new FilterRequest();
            filterReqeust.Type = typeof(BHoMObject);
            filterReqeust.Equalities[Convert.FilterRequest.RequestType] = RequestType.View;
            filterReqeust.Equalities[Convert.FilterRequest.RevitViewType] = revitViewType;
            return filterReqeust;
        }

        [Description("Creates FilterRequest which filters specified views")]
        [Input("viewTemplateName", "Revit View Template Name applied to the views to be pulled")]
        [Output("FilterRequest")]
        public static FilterRequest ViewFilterRequest(string viewTemplateName)
        {
            FilterRequest filterRequest = new FilterRequest();
            filterRequest.Type = typeof(BHoMObject);
            filterRequest.Equalities[Convert.FilterRequest.RequestType] = RequestType.View;
            filterRequest.Equalities[Convert.FilterRequest.ViewTemplateName] = viewTemplateName;
            return filterRequest;
        }
    }
}

