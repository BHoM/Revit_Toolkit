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
using BH.oM.Geometry.ShapeProfiles;
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

        [Description("Convert a Revit pipe into a BHoM pipe.")]
        [Input("revitPipe", "Revit pipe to be converted.")]
        [Input("settings", "Revit settings.")]
        [Input("refObjects", "Referenced objects.")]
        [Output("Pipe From Revit", "BHoM Pipe converted from Revit.")]
        public static BH.oM.MEP.Elements.Pipe PipeFromRevit(this Autodesk.Revit.DB.Plumbing.Pipe revitPipe, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            // Reuse a BHoM duct from refObjects it it has been converted before
            BH.oM.MEP.Elements.Pipe bhomPipe = refObjects.GetValue<BH.oM.MEP.Elements.Pipe>(revitPipe.Id);
            if (bhomPipe != null)
                return bhomPipe;

            // BHoM pipe
            bhomPipe = new BH.oM.MEP.Elements.Pipe();

            // Start and end points
            LocationCurve locationCurve = revitPipe.Location as LocationCurve;
            Curve curve = locationCurve.Curve;
            bhomPipe.StartNode = new BH.oM.MEP.Elements.Node { Position = curve.GetEndPoint(0).PointFromRevit() }; // Start point
            bhomPipe.EndNode = new BH.oM.MEP.Elements.Node { Position = curve.GetEndPoint(1).PointFromRevit() }; // End point

            // Duct section property
            bhomPipe.SectionProperty = revitPipe.PipeSectionProperty(settings, refObjects);

            //Set identifiers, parameters & custom data
            bhomPipe.SetIdentifiers(revitPipe);
            bhomPipe.CopyParameters(revitPipe, settings.ParameterSettings);
            bhomPipe.SetProperties(revitPipe, settings.ParameterSettings);

            refObjects.AddOrReplace(revitPipe.Id, bhomPipe);

            return bhomPipe;
        }

        /***************************************************/
    }
}