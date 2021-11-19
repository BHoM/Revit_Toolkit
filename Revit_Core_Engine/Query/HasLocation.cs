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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Physical.Constructions;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Checks whether the element has a valid Revit Location property of type LocationPoint or LocationCurve.")]
        [Input("element", "The element to validate its Location property.")]
        [Input("allowLocationPoint", "If true, the method will pass if the element's location is LocationPoint.")]
        [Input("allowLocationCurve", "If true, the method will pass if the element's location is LocationCurve.")]
        [Output("hasLocation", "True if the element's Location property is of one of the allowed types.")]
        public static bool HasLocation(this Element element, bool allowLocationPoint = true, bool allowLocationCurve = true)
        {
            if (element.Location != null)
            {
                if (element.Location is LocationPoint && !allowLocationPoint)
                    return false;

                if (element.Location is LocationCurve && !allowLocationCurve)
                    return false;
                
                return true;
            }

            return false;
        }
        
        /***************************************************/
    }
}

