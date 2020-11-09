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

using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.MEP.System;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Convert a Revit wire into a BHoM wire segment.")]
        [Input("revitWire", "Revit wire to be converted.")]
        [Input("settings", "Revit adapter settings.")]
        [Input("refObjects", "A collection of objects processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("wireSegment", "BHoM wire segment converted from a Revit wire.")]
        public static BH.oM.MEP.System.WireSegment WireFromRevit(this Autodesk.Revit.DB.Electrical.Wire revitWire, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            // Reuse a BHoM duct from refObjects it it has been converted before
            BH.oM.MEP.System.WireSegment bhomWire = refObjects.GetValue<BH.oM.MEP.System.WireSegment>(revitWire.Id);
            if (bhomWire != null)
                return bhomWire;
            
            LocationCurve locationCurve = revitWire.Location as LocationCurve;
            Curve curve = locationCurve.Curve;
            BH.oM.Geometry.Point startPoint = new BH.oM.Geometry.Point { X = curve.GetEndPoint(0).X, Y = curve.GetEndPoint(0).Y, Z = curve.GetEndPoint(0).Z };
            BH.oM.Geometry.Point endPoint = new BH.oM.Geometry.Point { X = curve.GetEndPoint(1).X, Y = curve.GetEndPoint(1).Y, Z = curve.GetEndPoint(1).Z };
            BH.oM.Geometry.Line line = BH.Engine.Geometry.Create.Line(startPoint, endPoint); // BHoM line

            // Wire
            bhomWire = BH.Engine.MEP.Create.WireSegment(line);

            //Set identifiers, parameters & custom data
            bhomWire.SetIdentifiers(revitWire);
            bhomWire.CopyParameters(revitWire, settings.ParameterSettings);
            bhomWire.SetProperties(revitWire, settings.ParameterSettings);

            refObjects.AddOrReplace(revitWire.Id, bhomWire);

            return bhomWire;
        }

        /***************************************************/
    }
}