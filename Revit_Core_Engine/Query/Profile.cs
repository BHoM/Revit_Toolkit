/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.oM.Spatial.ShapeProfiles;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Geometry;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Extract a BHoM duct profile from a Revit duct.")]
        [Input("duct", "Revit duct to extract relevant profile information from.")]
        [Input("settings", "Revit adapter settings.")]
        [Output("profile", "BHoM duct profile extracted from a Revit duct.")]
        public static IProfile Profile(this Autodesk.Revit.DB.Mechanical.Duct duct, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            // Is the duct circular, rectangular or oval?
            // Get the duct shape, which is either circular, rectangular, oval or null
            Autodesk.Revit.DB.ConnectorProfileType ductShape = BH.Revit.Engine.Core.Query.Shape(duct, settings); //duct.DuctType.Shape;
            switch (ductShape)
            {
                case Autodesk.Revit.DB.ConnectorProfileType.Round:
                    // Create a circular profile
                    // Diameter
                    double diameter = duct.Diameter.ToSI(SpecTypeId.DuctSize);

                    // Thickness / gauge of the duct sheet
                    double thickness = 0.001519; // Dafault to 16 gauge, to be changed later
                    
                    return BH.Engine.Spatial.Create.TubeProfile(diameter, thickness);
                case Autodesk.Revit.DB.ConnectorProfileType.Rectangular:
                    // Create a rectangular box profile
                    double boxHeight = duct.Height.ToSI(SpecTypeId.DuctSize);
                    double boxWidth = duct.Width.ToSI(SpecTypeId.DuctSize);
                    double boxThickness = 0.001519; // Dafault to 16 gauge, to be changed later
                    double outerRadius = 0;
                    double innerRadius = 0;

                    return BH.Engine.Spatial.Create.BoxProfile(boxHeight, boxWidth, boxThickness, outerRadius, innerRadius);
                case Autodesk.Revit.DB.ConnectorProfileType.Oval:
                    // Create an oval profile
                    // There is currently no section profile for an oval duct in BHoM_Engine. This part will be implemented once the relevant section profile becomes available.
                    BH.Engine.Reflection.Compute.RecordError("Unable to create a profile for an oval duct at this time because there is currently no section profile for an oval duct. Element ID: " + duct.Id);
                    return null;
                default:
                    return null;
            }
        }
        
        [Description("Extract a BHoM cable tray profile from a Revit cable tray.")]
        [Input("cableTray", "Revit cable tray to extract relevant profile information from.")]
        [Input("settings", "Revit adapter settings.")]
        [Output("profile", "BHoM cable tray profile extracted from a Revit cable tray.")]
        public static IProfile Profile(this Autodesk.Revit.DB.Electrical.CableTray cableTray, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            // Cable Trays are always rectangular
            double boxHeight = cableTray.Height.ToSI(SpecTypeId.CableTraySize);
            double boxWidth = cableTray.Width.ToSI(SpecTypeId.CableTraySize);
            double boxThickness = 0.001519;
            double outerRadius = 0;
            double innerRadius = 0;

            return BH.Engine.Spatial.Create.BoxProfile(boxHeight, boxWidth, boxThickness, outerRadius, innerRadius);
        }

        /***************************************************/

        [Description("Extract a BHoM pipe profile from a Revit pipe.")]
        [Input("pipe", "Revit pipe to extract relevant profile information from.")]
        [Input("settings", "Revit adapter settings.")]
        [Output("profile", "BHoM pipe profile extracted from a Revit pipe.")]
        public static IProfile Profile(this Autodesk.Revit.DB.Plumbing.Pipe pipe, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            List<ICurve> edges = new List<ICurve>();

            double diameter = pipe.Diameter.ToSI(SpecTypeId.PipeSize);

            // Thickness
            double outsideDiameter = pipe.LookupParameterDouble(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);
            double insideDiameter = pipe.LookupParameterDouble(BuiltInParameter.RBS_PIPE_INNER_DIAM_PARAM);
            double thickness = (outsideDiameter - insideDiameter) / 2;

            return BH.Engine.Spatial.Create.TubeProfile(diameter, thickness);
        }

        /***************************************************/

        [Description("Extract a BHoM tube profile from a Revit wire. A tube profile is used in this case to facilitate the extraction of the wire diameter because a dedicated BHoM profile for a wire is not available.")]
        [Input("wire", "Revit wire to extract relevant profile information from.")]
        [Input("settings", "Revit adapter settings.")]
        [Output("profile", "BHoM tube profile extracted from a Revit wire.")]
        public static IProfile Profile(this Autodesk.Revit.DB.Electrical.Wire wire, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            List<ICurve> edges = new List<ICurve>();

            double diameter = wire.Diameter.ToSI(SpecTypeId.WireDiameter);

            double thickness = 0;

            return BH.Engine.Spatial.Create.TubeProfile(diameter, thickness);
        }

        /***************************************************/
    }
}

