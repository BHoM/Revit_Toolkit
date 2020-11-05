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

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Generic;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using BH.Engine;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using BH.Engine.Environment;
using BH.oM.Environment.Fragments;
using System.Net.NetworkInformation;
using BH.Engine.Physical;
using System.Runtime.Remoting.Messaging;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns the in Revit defining location line of an IFramingElement, based on its z Justification Revit parameter.")]
        [Input("element", "The IFramingElement to query the in Revit defining location line of.")]
        [Output("curve", "The in Revit geometry defining location line of the element.")]
        public static ICurve RevitDefiningLine(this IFramingElement element)
        {
            string zJustification = element.GetRevitParameterValue("z Justification").ToString();

            ICurve definingLine = null;

            if (zJustification == "Top")
            {
                definingLine = element.TopCentreline();
                return definingLine;
            }
            else if (zJustification == "Bottom")
            {
                definingLine = element.BottomCentreline();
                return definingLine;
            }
            else if (zJustification == "Origin" || zJustification =="Center")
            {
                return element.Location;
            }
            else
            {
                Engine.Reflection.Compute.RecordError("The Revit parameter z Justification is not defined for the element, will return element location line.");
                return element.Location;
            }
        }
    }
}