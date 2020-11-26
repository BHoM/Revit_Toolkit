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

using BH.oM.Reflection.Attributes;
using System.ComponentModel;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using BH.Engine.Physical;
using BH.Engine.Geometry;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns the driving curve of the given IFramingElement adjusted based on the 'z Justification' parameter value pulled from Revit.  Important: neither z offset nor any y justification is taken into account.")]
        [Input("element", "The IFramingElement to query the in Revit defining location line of.")]
        [Output("curve", "The in Revit geometry defining location line of the element.")]
        public static ICurve ZJustifiedCurve(this IFramingElement element)
        {
            if (element == null)
                return null;

            string zJustification = element.GetRevitParameterValue("z Justification")?.ToString().ToLower();

            if (zJustification == "top")
                return element.TopCentreline();
            else if (zJustification == "bottom")
                return element.BottomCentreline();
            else if (zJustification == "center")
            {
                ICurve top = element.TopCentreline();
                ICurve bottom = element.BottomCentreline();
                Point start = (top.IStartPoint() + bottom.IStartPoint()) * 0.5;
                Point end = (top.IEndPoint() + bottom.IEndPoint()) * 0.5;
                return new Line {Start = start, End = end};
            }
            else if (zJustification == "origin")
                return element.Location;
            else
            {
                Engine.Reflection.Compute.RecordError("Error extracting z Justification Revit parameter, location curve of the BHoM object has been returned.");
                return element.Location;
            }
        }
    }
}