﻿/*
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

        [Description("Convert a Revit pipe into a BHoM pipe.")]
        [Input("Autodesk.Revit.DB.Plumbing.Pipe", "Revit family instance.")]
        [Input("BH.oM.Adapters.Revit.Settings.RevitSettings", "Revit settings.")]
        [Input("Dictionary<string, List<IBHoMObject>>", "Referenced objects.")]
        [Output("BH.oM.MEP.Elements.Pipe", "BHoM Pipe.")]
        public static BH.oM.MEP.Elements.Pipe PipeFromRevit(this Autodesk.Revit.DB.Plumbing.Pipe revitPipe, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            settings = settings.DefaultIfNull();
            Options options = new Options();
            options.IncludeNonVisibleObjects = false;

            // BHoM pipe
            BH.oM.MEP.Elements.Pipe bhomPipe = new BH.oM.MEP.Elements.Pipe();

            // Start and end points
            LocationCurve locationCurve = revitPipe.Location as LocationCurve;
            Curve curve = locationCurve.Curve;
            bhomPipe.StartNode = new BH.oM.MEP.Elements.Node { Position = curve.GetEndPoint(0).PointFromRevit() }; // Start point
            bhomPipe.EndNode = new BH.oM.MEP.Elements.Node { Position = curve.GetEndPoint(1).PointFromRevit() }; // End point

            IProfile profile = revitPipe.PipeType.ProfileFromRevit(revitPipe, settings, refObjects);

            // Duct section property
            bhomPipe.SectionProperty = revitPipe.PipeSectionProperty(settings, refObjects);

            return bhomPipe;
        }

        /***************************************************/
    }
}