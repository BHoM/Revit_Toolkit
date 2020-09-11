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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Geometry.ShapeProfiles;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Geometry;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Convert a profile from a duct type.")]
        [Input("ductType", "Revit duct type to be converted to a profile.")]
        [Input("duct", "Revit duct for property extraction.")]
        [Input("settings", "Revit settings.")]
        [Input("refObjects", "Referenced objects.")]
        [Output("ProfileFromRevit", "BHoM duct section property converted from a duct type.")]
        public static IProfile ProfileFromRevit(this Autodesk.Revit.DB.Mechanical.DuctType ductType, Autodesk.Revit.DB.Mechanical.Duct duct, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<ICurve> edges = new List<ICurve>();

            // Is the duct circular, rectangular or oval?
            // Get the duct shape, which is either circular, rectangular, oval or null
            Autodesk.Revit.DB.ConnectorProfileType ductShape = ductType.Shape;
            switch (ductShape)
            {
                case Autodesk.Revit.DB.ConnectorProfileType.Round:
                    // Create a circular profile
                    // Diameter
                    double diameter = duct.Diameter.ToSI(UnitType.UT_HVAC_DuctSize);

                    // Thickness / gauge of the duct sheet
                    double thickness = 0.001519; // Dafault to 16 gauge, to be changed later

                    return new TubeProfile(diameter, thickness, edges);
                case Autodesk.Revit.DB.ConnectorProfileType.Rectangular:
                    // Create a rectangular box profile
                    double height = duct.Height.ToSI(UnitType.UT_HVAC_DuctSize);
                    double boxWidth = duct.Width.ToSI(UnitType.UT_HVAC_DuctSize);
                    double boxThickness = 0.001519; // Dafault to 16 gauge, to be changed later
                    double outerRadius = 0;
                    double innerRadius = 0;
                    return new BH.oM.Geometry.ShapeProfiles.BoxProfile(height, boxWidth, boxThickness, outerRadius, innerRadius, edges);
                case Autodesk.Revit.DB.ConnectorProfileType.Oval:
                    // Create an oval profile
                    // There is currently no section profile for an oval duct in BHoM_Engine. This part will be implemented once the relevant section profile becomes available.
                    BH.Engine.Reflection.Compute.RecordError("Unable to create a profile for an oval duct at this time because there is currently no section profile for an oval duct. Element ID: " + duct.Id);
                    return null;
                default:
                    return null;
            }
        }

        /***************************************************/

        [Description("Convert a profile from a pipe type.")]
        [Input("pipeType", "Revit pipe type to be converted to a profile.")]
        [Input("pipe", "Revit pipe for property extraction.")]
        [Input("settings", "Revit settings.")]
        [Input("refObjects", "Referenced objects.")]
        [Output("ProfileFromRevit", "BHoM pipe section property to be converted from a pipe type.")]
        public static IProfile ProfileFromRevit(this Autodesk.Revit.DB.Plumbing.PipeType pipeType, Autodesk.Revit.DB.Plumbing.Pipe pipe, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<ICurve> edges = new List<ICurve>();

            double diameter = pipe.Diameter.ToSI(UnitType.UT_PipeSize);

            // Thickness
            double outsideDiameter = pipe.LookupParameterDouble("Outside Diameter");
            double insideDiameter = pipe.LookupParameterDouble("Inside Diameter");
            double thickness = (outsideDiameter - insideDiameter) / 2;

            return new TubeProfile(diameter, thickness, edges);
        }

        /***************************************************/

        [Description("Convert a profile from a wire type.")]
        [Input("wireType", "Revit wire type to be converted to a profile.")]
        [Input("wire", "Revit wire for property extraction.")]
        [Input("settings", "Revit settings.")]
        [Input("refObjects", "Referenced objects.")]
        [Output("ProfileFromRevit", "BHoM wire section property to be converted from a wire type.")]
        public static IProfile ProfileFromRevit(this Autodesk.Revit.DB.Electrical.WireType wireType, Autodesk.Revit.DB.Electrical.Wire wire, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<ICurve> edges = new List<ICurve>();

            double diameter = wire.Diameter.ToSI(UnitType.UT_WireSize);

            double thickness = 0;
            
            return new TubeProfile(diameter, thickness, edges);
        }

        /***************************************************/
    }
}