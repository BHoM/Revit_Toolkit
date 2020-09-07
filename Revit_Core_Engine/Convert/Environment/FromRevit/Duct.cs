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
        public static BH.oM.MEP.Elements.Duct DuctFromRevit(this Autodesk.Revit.DB.Mechanical.Duct duct, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();
            //Options options = new Options();
            //options.IncludeNonVisibleObjects = false;

            // Linear duct

            BH.oM.MEP.Elements.Duct bhomDuct = new BH.oM.MEP.Elements.Duct();

            // Duct start and end points
            LocationCurve locationCurve = duct.Location as LocationCurve;
            Curve curve = locationCurve.Curve;
            bhomDuct.StartNode.Position = curve.GetEndPoint(0).PointFromRevit(); // Start point
            bhomDuct.EndNode.Position = curve.GetEndPoint(1).PointFromRevit(); // End point

            // Duct orientation angle
            //bhomDuct.OrientationAngle = duct.OrientationAngle(settings);

            // Duct section property
            bhomDuct.SectionProperty = duct.DuctSectionProperty(settings, refObjects);
            
            //// Box profile
            //double boxHeight = revitDuct.Height.ToSI(UnitType.UT_HVAC_DuctSize);
            //double boxWidth = revitDuct.Width.ToSI(UnitType.UT_HVAC_DuctSize);
            //double boxThickness = 0.001519; // Dafault to 16 gauge to be changed later
            //double outerRadius = double.NaN;
            //double innerRadius = double.NaN;
            //List<ICurve> edges = revitDuct.Curves(options, settings, true).FromRevit();
            //BoxProfile boxProfile = new BoxProfile(boxHeight, boxWidth, boxThickness, outerRadius, innerRadius, edges);

            //// Lining
            ////DuctInsulation ductInsulation = DuctInsulation.get
            ////double liningHeight = revitDuct.
            //double liningWidth = double.NaN;
            //IProfile liningProfile = BH.Engine.Geometry.Create.RectangleProfile(liningHeight, liningWidth, 0);

            //// Insulation
            //double insulationHeight = double.NaN;
            //double insulationWidth = double.NaN;
            //IProfile insulationProfile = BH.Engine.Geometry.Create.RectangleProfile(insulationHeight, insulationWidth, 0);

            //// Section profile
            //SectionProfile sectionProfile = new SectionProfile(boxProfile, liningProfile, insulationProfile);

            //// Duct section property
            //double elementSolidArea = double.NaN;
            //double liningSolidArea = double.NaN;
            //double insulationSolidArea = double.NaN;
            //double elementVoidArea = double.NaN;
            //double liningVoidArea = double.NaN;
            //double insulationVoidArea = double.NaN;
            //double hydraulicDiameter = double.NaN;
            //double circularEquivalentDiameter = double.NaN;
            //Autodesk.Revit.DB.Mechanical.DuctType ductType = revitDuct.DuctType;
            ////DuctSectionProperty ductSectionProperty = BH.Revit.Engine.Core.Convert.DuctSectionPropertyFromRevit(ductType, settings, refObjects);

            //DuctSectionProperty ductSectionProperty = new DuctSectionProperty(sectionProfile, elementSolidArea, liningSolidArea, insulationSolidArea, elementVoidArea, liningVoidArea, insulationVoidArea, hydraulicDiameter, circularEquivalentDiameter);

            //bhomDuct.SectionProperty = ductSectionProperty;

            return bhomDuct;
        }

        /***************************************************/
    }
}