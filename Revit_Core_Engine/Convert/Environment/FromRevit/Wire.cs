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
using BH.oM.MEP.Elements;
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

        [Description("BHoM wire converted from a Revit wire.")]
        [Input("revitWire", "Revit wire to be converted.")]
        [Input("settings", "Revit settings.")]
        [Input("refObjects", "Referenced objects.")]
        [Output("Wire From Revit", "BHoM wire.")]
        public static BH.oM.MEP.Elements.Wire WireFromRevit(this Autodesk.Revit.DB.Electrical.Wire revitWire, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            // Reuse a BHoM duct from refObjects it it has been converted before
            BH.oM.MEP.Elements.Wire bhomWire = refObjects.GetValue<BH.oM.MEP.Elements.Wire>(revitWire.Id);
            if (bhomWire != null)
                return bhomWire;

            // Wire
            bhomWire = new BH.oM.MEP.Elements.Wire();

            WireSegment wireSegment = new WireSegment();
            
            // Start and end points
            LocationCurve locationCurve = revitWire.Location as LocationCurve;
            Curve curve = locationCurve.Curve;
            wireSegment.StartNode = new BH.oM.MEP.Elements.Node { Position = curve.GetEndPoint(0).PointFromRevit() }; // Start point
            wireSegment.EndNode = new BH.oM.MEP.Elements.Node { Position = curve.GetEndPoint(1).PointFromRevit() }; // End point

            bhomWire.WireSegments.Add(wireSegment);

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