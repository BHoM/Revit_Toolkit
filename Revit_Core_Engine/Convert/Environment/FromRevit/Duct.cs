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
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Geometry.ShapeProfiles;
using BH.oM.MEP.SectionProperties;
using BH.oM.MEP.Elements;
using BH.oM.Reflection.Attributes;
using BH.oM.Structure.Elements;
using BH.oM.Structure.MaterialFragments;
using BH.oM.Structure.SectionProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Geometry;
using System.Linq;
using Autodesk.Revit.DB.Mechanical;

using System.Text;
using System.Threading.Tasks;

using BH.oM.MEP.MaterialFragments;
using BH.Engine.Spatial;
using BH.Engine.MEP;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Convert a Revit duct to a BHoM duct.")]
        [Input("Autodesk.Revit.DB.Mechanical.Duct", "Revit duct.")]
        [Input("BH.oM.Adapters.Revit.Settings.RevitSettings", "Revit settings.")]
        [Input("Dictionary<string, List<IBHoMObject>>", "Referenced objects.")]
        [Output("BH.oM.MEP.Elements.Duct", "BHoM duct.")]
        public static BH.oM.MEP.Elements.Duct DuctFromRevit(this Autodesk.Revit.DB.Mechanical.Duct revitDuct, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();
            Options options = new Options();
            options.IncludeNonVisibleObjects = false;

            // BHoM duct
            BH.oM.MEP.Elements.Duct bhomDuct = new BH.oM.MEP.Elements.Duct();

            // Start and end points
            LocationCurve locationCurve = revitDuct.Location as LocationCurve;
            Curve curve = locationCurve.Curve;
            bhomDuct.StartNode = new BH.oM.MEP.Elements.Node { Position = curve.GetEndPoint(0).PointFromRevit() }; // Start point
            bhomDuct.EndNode = new BH.oM.MEP.Elements.Node { Position = curve.GetEndPoint(1).PointFromRevit() }; // End point

            // Orientation angle
            bhomDuct.OrientationAngle = revitDuct.OrientationAngle(settings);

            // Duct section profile
            SectionProfile sectionProfile = revitDuct.DuctSectionProfile(settings, refObjects);

            // Duct section property
            // This is being constructed manually because BH.Engine.MEP.Create.DuctSectionProperty doesn't work with round ducts, as it attempts to calculate the circular equivalent of a round duct.
            // Solid Areas
            double elementSolidArea = sectionProfile.ElementProfile.Area();
            double liningSolidArea = sectionProfile.LiningProfile.Area() - sectionProfile.ElementProfile.Area();
            double insulationSolidArea = sectionProfile.InsulationProfile.Area();

            // Void Areas
            double elementVoidArea = sectionProfile.ElementProfile.VoidArea();
            double liningVoidArea = sectionProfile.LiningProfile.VoidArea();
            double insulationVoidArea = sectionProfile.InsulationProfile.VoidArea();

            // Get the duct shape, which is either circular, rectangular, oval or null
            Autodesk.Revit.DB.ConnectorProfileType ductShape = BH.Revit.Engine.Core.Query.DuctShape(revitDuct, settings);

            // Duct specific properties
            // Circular equivalent diameter
            double circularEquivalent = 0;
            // Is the duct rectangular?
            if (ductShape == Autodesk.Revit.DB.ConnectorProfileType.Rectangular)
            {
                circularEquivalent = sectionProfile.ElementProfile.ICircularEquivalentDiameter();
            }

            // Hydraulic diameter
            double hydraulicDiameter = revitDuct.LookupParameterDouble("Hydraulic Diameter");

            bhomDuct.SectionProperty = new BH.oM.MEP.SectionProperties.DuctSectionProperty(sectionProfile, elementSolidArea, liningSolidArea, insulationSolidArea, elementVoidArea, liningVoidArea, insulationVoidArea, hydraulicDiameter, circularEquivalent);

            return bhomDuct;
        }

        /***************************************************/
    }
}