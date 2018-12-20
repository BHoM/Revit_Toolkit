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

using System.Collections.Generic;
using System.ComponentModel;

using BH.oM.Base;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Adds reference object to existsing reference Dictionary. Method will create new dictionary if refObjects is null")]
        [Input("refObjects", "Existing reference objects")]
        [Input("bHoMObject", "BHoM object to be added")]
        [Output("RefObjects")]
        public static Dictionary<int, List<IBHoMObject>> AddRefObject(this Dictionary<int, List<IBHoMObject>> refObjects, IBHoMObject bHoMObject)
        {
            if (bHoMObject == null && bHoMObject == null)
                return null;

            Dictionary<int, List<IBHoMObject>> aResult = null;
            if (refObjects == null)
                aResult = new Dictionary<int, List<IBHoMObject>>();
            else
                aResult = new Dictionary<int, List<IBHoMObject>>(refObjects);

            if (bHoMObject == null)
                return new Dictionary<int, List<IBHoMObject>>(refObjects);

            int aId = Query.ElementId(bHoMObject);

            List<IBHoMObject> aBHoMObjectList = null;
            if (aResult.TryGetValue(aId, out aBHoMObjectList))
            {
                if (aBHoMObjectList == null)
                    aBHoMObjectList = new List<IBHoMObject>();

                if (aBHoMObjectList.Find(x => x != null && x.BHoM_Guid == bHoMObject.BHoM_Guid) == null)
                    aBHoMObjectList.Add(bHoMObject);
            }
            else
            {
                aResult.Add(aId, new List<IBHoMObject>() { bHoMObject });
            }

            return aResult;
        }

        /***************************************************/
    }
}