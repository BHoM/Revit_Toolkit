/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Duplicates given BHoMObject and removes its identity data (ElementId, AdapterId).")]
        [Input("bHoMObject", "BHoMObject")]
        [Output("BHoMObject")]
        public static IBHoMObject Duplicate(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            IBHoMObject obj = bHoMObject.GetShallowClone();

            obj.CustomData.Remove(Convert.ElementId);
            obj.CustomData.Remove(Convert.AdapterId);


            return obj;
        }

        /***************************************************/

        [Description("Duplicates FilterRequest.")]
        [Input("filterRequest", "FilterRequest")]
        [Output("FilterRequest")]
        public static FilterRequest Duplicate(this FilterRequest filterRequest)
        {
            if (filterRequest == null)
                return null;

            FilterRequest request = new FilterRequest();

            if (filterRequest.Equalities != null)
                request.Equalities = new Dictionary<string, object>(filterRequest.Equalities);
            else
                request.Equalities = new Dictionary<string, object>();

            request.Tag = filterRequest.Tag;

            request.Type = filterRequest.Type;


            return request;
        }

        /***************************************************/
    }
}
