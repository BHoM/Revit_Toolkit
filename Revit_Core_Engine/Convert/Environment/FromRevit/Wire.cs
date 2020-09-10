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
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.Electrical;
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Geometry.ShapeProfiles;
using BH.oM.MEP.Elements;
using BH.oM.Reflection.Attributes;
using BH.oM.Structure.Elements;
using BH.oM.Structure.MaterialFragments;
using BH.oM.Structure.SectionProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Convert a Revit wire into a BHoM wire.")]
        [Input("Autodesk.Revit.DB.Electrical.Wire", "Revit family instance.")]
        [Input("BH.oM.Adapters.Revit.Settings.RevitSettings", "Revit settings.")]
        [Input("Dictionary<string, List<IBHoMObject>>", "Referenced objects.")]
        [Output("BH.oM.MEP.Elements.Wire", "BHoM Wire.")]
        public static BH.oM.MEP.Elements.Wire WireFromRevit(this Autodesk.Revit.DB.Electrical.Wire revitWire, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            settings = settings.DefaultIfNull();
            Options options = new Options();
            options.IncludeNonVisibleObjects = false;

            // Wire
            BH.oM.MEP.Elements.Wire bhomWire = new BH.oM.MEP.Elements.Wire();

            WireSegment wireSegment = new WireSegment();
            
            // Start and end points
            LocationCurve locationCurve = revitWire.Location as LocationCurve;
            Curve curve = locationCurve.Curve;
            wireSegment.StartNode = new BH.oM.MEP.Elements.Node { Position = curve.GetEndPoint(0).PointFromRevit() }; // Start point
            wireSegment.EndNode = new BH.oM.MEP.Elements.Node { Position = curve.GetEndPoint(1).PointFromRevit() }; // End point

            //IProfile profile = revitWire.wir.ProfileFromRevit(revitWire, settings, refObjects);

            //// Duct section property
            //bhomPipe.SectionProperty = revitWire.PipeSectionProperty(settings, refObjects);

            bhomWire.WireSegments.Add(wireSegment);

            return bhomWire;
        }

        /***************************************************/
    }
}