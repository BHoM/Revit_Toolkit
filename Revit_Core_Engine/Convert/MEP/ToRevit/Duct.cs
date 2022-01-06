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
using Autodesk.Revit.DB.Mechanical;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.MEP.System.SectionProperties;
using BH.oM.Reflection.Attributes;
using BH.oM.Spatial.ShapeProfiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Converts BH.oM.MEP.System.Duct to a Revit Duct.")]
        [Input("duct", "BH.oM.MEP.System.Duct to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("duct", "Revit Duct resulting from converting the input BH.oM.MEP.System.Duct.")]
        public static Duct ToRevitDuct(this oM.MEP.System.Duct duct, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (document == null)
                return null;

            // Check valid duct object
            if (duct == null)
                return null;

            // Construct Revit Duct
            Duct revitDuct = refObjects.GetValue<Duct>(document, duct.BHoM_Guid);
            if (revitDuct != null)
                return revitDuct;

            // Settings
            settings = settings.DefaultIfNull();

            // Duct type
            DuctType ductType = duct.SectionProperty.ToRevitElementType(document, new List<BuiltInCategory> { BuiltInCategory.OST_DuctSystem }, settings, refObjects) as DuctType;
            if (ductType == null)
            {
                BH.Engine.Reflection.Compute.RecordError("No valid family has been found in the Revit model. Duct creation requires the presence of the default Duct Family Type.");
                return null;
            }

            // End points
            XYZ start = duct.StartPoint.ToRevit();
            XYZ end = duct.EndPoint.ToRevit();

            // Level
            Level level = document.LevelBelow(Math.Min(start.Z, end.Z), settings);
            if (level == null)
                return null;

            // Default system used for now
            // TODO: in the future you could look for the existing connectors and check if any of them overlaps with start/end of this duct - if so, use it in Duct.Create.
            // hacky/heavy way of getting all connectors in the link below - however, i would rather filter the connecting elements out by type/bounding box first for performance reasons
            // https://thebuildingcoder.typepad.com/blog/2010/06/retrieve-mep-elements-and-connectors.html

            MechanicalSystemType mst = new FilteredElementCollector(document).OfClass(typeof(MechanicalSystemType)).OfType<MechanicalSystemType>().FirstOrDefault();

            if(mst == null)
            {
                BH.Engine.Reflection.Compute.RecordError("No valid MechanicalSystemType can be found in the Revit model. Creating a revit Duct requires a MechanicalSystemType.");
                return null;
            }

            BH.Engine.Reflection.Compute.RecordWarning("Duct creation will utilise the first available MechanicalSystemType from the Revit model.");

            SectionProfile sectionProfile = duct.SectionProperty?.SectionProfile;
            if (sectionProfile == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Duct creation requires a valid SectionProfile.");
                return null;
            }

            DuctSectionProperty ductSectionProperty = duct.SectionProperty;

            // Create Revit Duct
            revitDuct = Duct.Create(document, mst.Id, ductType.Id, level.Id, start, end);
            if (revitDuct == null)
            {
                BH.Engine.Reflection.Compute.RecordError("No Revit Duct has been created. Please check inputs prior to push attempt.");
                return null;
            }

            // Copy parameters from BHoM object to Revit element
            revitDuct.CopyParameters(duct, settings);

            double orientationAngle = duct.OrientationAngle;
            if (Math.Abs(orientationAngle) > settings.AngleTolerance)
            {
                ElementTransformUtils.RotateElement(document, revitDuct.Id, Line.CreateBound(start, end), orientationAngle);
            }

            double flowRate = duct.FlowRate;
            revitDuct.SetParameter(BuiltInParameter.RBS_DUCT_FLOW_PARAM, flowRate);

            double hydraulicDiameter = ductSectionProperty.HydraulicDiameter;
            revitDuct.SetParameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM, hydraulicDiameter);

            DuctLiningType dlt = null;
            if (sectionProfile.LiningProfile != null)
            {
                // Get first available ductLiningType from document
                dlt = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Mechanical.DuctLiningType)).FirstOrDefault() as Autodesk.Revit.DB.Mechanical.DuctLiningType;
                if (dlt == null)
                {
                    BH.Engine.Reflection.Compute.RecordError("Any duct lining type needs to be present in the Revit model in order to push ducts with lining.\n" +
                        "Duct has been created but no lining has been applied.");
                }
            }

            DuctInsulationType dit = null;
            if (sectionProfile.InsulationProfile != null)
            {
                dit = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Mechanical.DuctInsulationType)).FirstOrDefault() as Autodesk.Revit.DB.Mechanical.DuctInsulationType;
                if (dit == null)
                {
                    BH.Engine.Reflection.Compute.RecordError("Any duct insulation type needs to be present in the Revit model in order to push ducts with lining.\n" +
                        "Duct has been created but no insulation has been applied.");
                }
            }

            // Rectangular Duct 
            if (sectionProfile.ElementProfile is BoxProfile)
            {
                BoxProfile elementProfile = sectionProfile.ElementProfile as BoxProfile;

                // Set Height
                double profileHeight = elementProfile.Height;
                revitDuct.SetParameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM, profileHeight);

                // Set Width
                double profileWidth = elementProfile.Width;
                revitDuct.SetParameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM, profileWidth);

                // Set LiningProfile
                if (dlt != null)
                {
                    BoxProfile liningProfile = sectionProfile.LiningProfile as BoxProfile;
                    double liningThickness = liningProfile.Thickness;
                    // Create ductLining
                    Autodesk.Revit.DB.Mechanical.DuctLining dl = Autodesk.Revit.DB.Mechanical.DuctLining.Create(document, revitDuct.Id, dlt.Id, liningThickness);
                }

                // Set InsulationProfile
                if (dit != null)
                {
                    BoxProfile insulationProfile = sectionProfile.InsulationProfile as BoxProfile;
                    double insulationThickness = insulationProfile.Thickness;
                    // Create ductInsulation
                    Autodesk.Revit.DB.Mechanical.DuctInsulation di = Autodesk.Revit.DB.Mechanical.DuctInsulation.Create(document, revitDuct.Id, dit.Id, insulationThickness);
                }

                // Set EquivalentDiameter
                double circularEquivalentDiameter = ductSectionProperty.CircularEquivalentDiameter;
                revitDuct.SetParameter(BuiltInParameter.RBS_EQ_DIAMETER_PARAM, circularEquivalentDiameter);
            }
            else if (sectionProfile.ElementProfile is TubeProfile)
            {
                TubeProfile elementProfile = sectionProfile.ElementProfile as TubeProfile;

                double diameter = elementProfile.Diameter;
                revitDuct.SetParameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM, diameter);

                // Set LiningProfile
                if (dlt != null)
                {
                    TubeProfile liningProfile = sectionProfile.LiningProfile as TubeProfile;
                    double liningThickness = liningProfile.Thickness;
                    //Create ductLining
                    Autodesk.Revit.DB.Mechanical.DuctLining dl = Autodesk.Revit.DB.Mechanical.DuctLining.Create(document, revitDuct.Id, dlt.Id, liningThickness);
                }

                // Set InsulationProfile
                if (dit != null)
                {
                    TubeProfile insulationProfile = sectionProfile.InsulationProfile as TubeProfile;
                    double insulationThickness = insulationProfile.Thickness;
                    // Create ductInsulation
                    Autodesk.Revit.DB.Mechanical.DuctInsulation di = Autodesk.Revit.DB.Mechanical.DuctInsulation.Create(document, revitDuct.Id, dit.Id, insulationThickness);
                }
            }

            refObjects.AddOrReplace(duct, revitDuct);
            return revitDuct;
        }

        /***************************************************/
    }
}


