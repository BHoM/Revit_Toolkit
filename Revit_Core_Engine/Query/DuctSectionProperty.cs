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
using BH.Revit.Engine.Core;
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
using BH.oM.MEP.MaterialFragments;
using Autodesk.Revit.DB.Mechanical;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Convert to a BHoM duct section property fom Revit.")]
        [Input("Autodesk.Revit.DB.Mechanical.DuctType", "Revit duct type.")]
        [Input("BH.oM.Adapters.Revit.Settings.RevitSettings", "Revit settings.")]
        [Input("Dictionary<string, List<IBHoMObject>>", "Referenced objects.")]
        [Output("BH.oM.MEP.SectionProperties.DuctSectionProperty", "BHoM duct section property.")]
        public static BH.oM.MEP.SectionProperties.DuctSectionProperty DuctSectionProperty(this Autodesk.Revit.DB.Mechanical.Duct duct, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();
            Options options = new Options();
            options.IncludeNonVisibleObjects = false;
            IProfile profile = duct.DuctType.ProfileFromRevit(duct, settings, refObjects);
            double liningThickness = 5;// duct.LookupParameterDouble("Lining Thickness").ToSI(UnitType.UT_HVAC_DuctLiningThickness); // extract the lining thk from Duct element
            double insulationThickness = 6;// duct.LookupParameterDouble("Insulation Thickness").ToSI(UnitType.UT_HVAC_DuctInsulationThickness); // as above

            SectionProfile sectionProfile;
            if (profile is TubeProfile)
                sectionProfile = BH.Engine.MEP.Create.SectionProfile((TubeProfile)profile, liningThickness, insulationThickness);
            else if (profile is BoxProfile)
                sectionProfile = BH.Engine.MEP.Create.SectionProfile((BoxProfile)profile, liningThickness, insulationThickness);
            else
            {
                //raise error
                return null;
            }


            DuctSectionProperty result = BH.Engine.MEP.Create.DuctSectionProperty(sectionProfile);

            //[Description("Convert to a BHoM duct section property fom Revit.")]
            //[Input("Autodesk.Revit.DB.Mechanical.DuctType", "Revit duct type.")]
            //[Input("BH.oM.Adapters.Revit.Settings.RevitSettings", "Revit settings.")]
            //[Input("Dictionary<string, List<IBHoMObject>>", "Referenced objects.")]
            //[Output("BH.oM.MEP.SectionProperties.DuctSectionProperty", "BHoM duct section property.")]
            //public static BH.oM.MEP.SectionProperties.DuctSectionProperty DuctSectionProperty(this Autodesk.Revit.DB.Mechanical.Duct duct, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
            //{
            //    settings = settings.DefaultIfNull();
            //    Options options = new Options();
            //    options.IncludeNonVisibleObjects = false;
            //    IProfile profile = duct.DuctType.ProfileFromRevit(duct, settings, refObjects);
            //    double liningThickness = 5;// duct.LookupParameterDouble("Lining Thickness").ToSI(UnitType.UT_HVAC_DuctLiningThickness); // extract the lining thk from Duct element
            //    double insulationThickness = 6;// duct.LookupParameterDouble("Insulation Thickness").ToSI(UnitType.UT_HVAC_DuctInsulationThickness); // as above

            //    SectionProfile sectionProfile;
            //    if (profile is TubeProfile)
            //        sectionProfile = BH.Engine.MEP.Create.SectionProfile((TubeProfile)profile, liningThickness, insulationThickness);
            //    else if (profile is BoxProfile)
            //        sectionProfile = BH.Engine.MEP.Create.SectionProfile((BoxProfile)profile, liningThickness, insulationThickness);
            //    else
            //    {
            //        //raise error
            //        return null;
            //    }


            //    DuctSectionProperty result = BH.Engine.MEP.Create.DuctSectionProperty(sectionProfile);

































            //// Linear duct

            //BH.oM.MEP.Elements.Duct bhomDuct = new BH.oM.MEP.Elements.Duct();

            //// Duct start point
            //LocationCurve locationCurve = revitDuct.Location as LocationCurve;
            //Curve curve = locationCurve.Curve;
            //bhomDuct.StartNode.Position.X = curve.GetEndPoint(0).X;
            //bhomDuct.StartNode.Position.Y = curve.GetEndPoint(0).Y;
            //bhomDuct.StartNode.Position.Z = curve.GetEndPoint(0).Z;

            //// Duct end point
            //bhomDuct.EndNode.Position.X = curve.GetEndPoint(1).X;
            //bhomDuct.EndNode.Position.Y = curve.GetEndPoint(1).Y;
            //bhomDuct.EndNode.Position.Z = curve.GetEndPoint(1).Z;

            //// Box profile
            //double boxHeight = double.NaN;
            //double boxWidth = double.NaN;
            //double boxThickness = double.NaN;
            //double outerRadius = double.NaN;
            //double innerRadius = double.NaN;
            //List<ICurve> edges = duct.GetType().Curves(options, settings, true).FromRevit();
            //BoxProfile boxProfile = new BoxProfile(boxHeight, boxWidth, boxThickness, outerRadius, innerRadius, edges);

            //// Lining
            //double liningHeight = double.NaN;
            //double liningWidth = double.NaN;
            //IProfile liningProfile = BH.Engine.Geometry.Create.RectangleProfile(liningHeight, liningWidth, 0);

            //// Insulation
            //double insulationHeight = double.NaN;
            //double insulationWidth = double.NaN;
            //IProfile insulationProfile = BH.Engine.Geometry.Create.RectangleProfile(insulationHeight, insulationWidth, 0);

            //// Section profile
            //SectionProfile sectionProfile = new SectionProfile(boxProfile, liningProfile, insulationProfile);

            //IMEPMaterial ductMaterial = null;
            //IMEPMaterial insulationMaterial = null;
            //IMEPMaterial liningMaterial = null;
            //string name = ductType.GetType().Name;
            //DuctSectionProperty ductSectionProperty = BH.Engine.MEP.Create.DuctSectionProperty(sectionProfile, ductMaterial, insulationMaterial, liningMaterial, name);

            return result;
        }

        /***************************************************/
    }
}