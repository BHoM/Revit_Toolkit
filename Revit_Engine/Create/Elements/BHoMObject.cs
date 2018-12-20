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

using BH.oM.Base;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates BHoMObject by given Revit ElementId (Element.Id). Allows to pull parameters from any element from Revit")]
        [Input("elementId", "Integer value for Revit ElementId")]
        [Output("BHoMObject")]
        public static BHoMObject BHoMObject(int elementId)
        {
            BHoMObject aBHoMObject = new BHoMObject()
            {

            };

            aBHoMObject.CustomData.Add(Convert.ElementId, elementId);

            return aBHoMObject;
        }

        /***************************************************/

        [Description("Creates BHoMObject by given Revit UniqueID (Element.UniqueId). Allows to pull parameters from any element from Revit")]
        [Input("elementId", "Integer value for Revit ElementId")]
        [Output("BHoMObject")]
        public static BHoMObject BHoMObject(string uniqueId)
        {
            BHoMObject aBHoMObject = new BHoMObject()
            {

            };

            aBHoMObject.CustomData.Add(Convert.AdapterId, uniqueId);

            return aBHoMObject;
        }

        /***************************************************/
    }
}

