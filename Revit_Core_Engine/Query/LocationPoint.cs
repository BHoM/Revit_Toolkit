/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using System.ComponentModel;
using BH.oM.Base.Attributes;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Get the location point of a Revit element.")]
        [Input("elem", "A generic Element instance.")]
        [Input("useCurveMidPoint", "If True, get the middle point of the element's LocationCurve if its location isn't of type LocationPoint.")]
        [Output("xyz", "The location point of a Revit element.")]
        public static XYZ LocationPoint(this Element elem, bool useCurveMidPoint = false)
        {
            var eLoc = elem.Location;

            if (eLoc is LocationCurve)
            {
                if (useCurveMidPoint)
                {
                    return ((LocationCurve)eLoc).Curve.Evaluate(0.5, true);
                }
                else
                {
                    return null;
                }
            }
            else
                return ((LocationPoint)eLoc).Point;
        }

        /***************************************************/
    }
}




