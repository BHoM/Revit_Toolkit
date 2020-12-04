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
using Autodesk.Revit.DB.Mechanical;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Spatial.ShapeProfiles;
using System;
using System.Collections.Generic;
using System.Linq;
using BH.oM.MEP.System.SectionProperties;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Duct ToRevitDuct(this oM.MEP.System.Duct duct, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
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
            revitDuct = Duct.Create(document, mst.Id, ductType.Id, level.Id, start, end);

            // Copy parameters from BHoM object to Revit element
            revitDuct.CopyParameters(duct, settings);

            double orientationAngle = duct.OrientationAngle;
            if (Math.Abs(orientationAngle) > settings.AngleTolerance)
            {
                ElementTransformUtils.RotateElement(document, revitDuct.Id, Line.CreateBound(start, end), orientationAngle);
            }

            SectionProfile sectionProfile = duct.SectionProperty.SectionProfile;
            if (sectionProfile == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Duct creation requires a SectionProfile. \n No elements created.");
                return null;
            }

            DuctSectionProperty ductSectionProperty = duct.SectionProperty;

            //ToDo: Verify builtInParameter for correct value. This is not functioning as expected
            double flowRate = duct.FlowRate;
            revitDuct.SetParameter(BuiltInParameter.RBS_DUCT_FLOW_PARAM, flowRate); //Not being set correctly.

            double hydraulicDiameter = ductSectionProperty.HydraulicDiameter;
            revitDuct.SetParameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM, hydraulicDiameter);

            // Rectangular Duct 
            if (ductType.Shape == ConnectorProfileType.Rectangular)
            {
                BoxProfile elementProfile = sectionProfile.ElementProfile as BoxProfile;
                if (elementProfile == null)
                    return null;

                // Set Height
                double profileHeight = elementProfile.Height;
                revitDuct.SetParameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM, profileHeight);

                // Set Width
                double profileWidth = elementProfile.Width;
                revitDuct.SetParameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM, profileWidth);

                // Set LiningProfile
                BoxProfile liningProfile = sectionProfile.LiningProfile as BoxProfile;
                if (liningProfile != null)
                {
                    double liningThickness = liningProfile.Thickness;

                    // Get first available ductLiningType from document
                    Autodesk.Revit.DB.Mechanical.DuctLiningType dlt = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Mechanical.DuctLiningType)).FirstOrDefault() as Autodesk.Revit.DB.Mechanical.DuctLiningType;
                    if (dlt == null)
                    {
                        BH.Engine.Reflection.Compute.RecordError("You must first create a duct with lining in your Revit model.");
                        return null;
                    }
                    // Create ductLining
                    Autodesk.Revit.DB.Mechanical.DuctLining dl = Autodesk.Revit.DB.Mechanical.DuctLining.Create(document, revitDuct.Id, dlt.Id, liningThickness);
                }

                // Set InsulationProfile
                BoxProfile insulationProfile = sectionProfile.InsulationProfile as BoxProfile;
                if (insulationProfile != null)
                {
                    double insulationThickness = insulationProfile.Thickness;

                    // Get first available ductLiningType from document
                    Autodesk.Revit.DB.Mechanical.DuctInsulationType dit = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Mechanical.DuctInsulationType)).FirstOrDefault() as Autodesk.Revit.DB.Mechanical.DuctInsulationType;
                    if (dit == null)
                    {
                        BH.Engine.Reflection.Compute.RecordError("You must first create a duct with insulation in your Revit model.");
                        return null;
                    }
                    // Create ductInsulation
                    Autodesk.Revit.DB.Mechanical.DuctInsulation di = Autodesk.Revit.DB.Mechanical.DuctInsulation.Create(document, revitDuct.Id, dit.Id, insulationThickness);
                }

                // Set EquivalentDiameter
                double circularEquivalentDiameter = ductSectionProperty.CircularEquivalentDiameter;
                revitDuct.SetParameter(BuiltInParameter.RBS_EQ_DIAMETER_PARAM, circularEquivalentDiameter);           
                //Parameters not found in RevitAPI for Rectangular Ducts, needs further investigation. Not critical for current workflows. 
                //double profileThickness = elementProfile.Thickness;
                //double profileOuterRadius = elementProfile.OuterRadius;
                //double profileInnerRadius = elementProfile.InnerRadius;
            }
            else if (ductType.Shape == ConnectorProfileType.Round)
            {
                TubeProfile elementProfile = sectionProfile.ElementProfile as TubeProfile;
                if (elementProfile == null)
                    return null;

                double diameter = elementProfile.Diameter;
                revitDuct.SetParameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM, diameter);

                // Set LiningProfile
                TubeProfile liningProfile = sectionProfile.LiningProfile as TubeProfile;
                if (liningProfile != null)
                {
                    double liningThickness = liningProfile.Thickness;

                    //Get first available ductLiningType from document
                    Autodesk.Revit.DB.Mechanical.DuctLiningType dlt = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Mechanical.DuctLiningType)).FirstOrDefault() as Autodesk.Revit.DB.Mechanical.DuctLiningType;
                    if (dlt == null)
                    {
                        BH.Engine.Reflection.Compute.RecordError("You must first create a duct with lining in your Revit model.");
                        return null;
                    }
                    //Create ductLining
                    Autodesk.Revit.DB.Mechanical.DuctLining dl = Autodesk.Revit.DB.Mechanical.DuctLining.Create(document, revitDuct.Id, dlt.Id, liningThickness);
                }

                // Set InsulationProfile
                TubeProfile insulationProfile = sectionProfile.InsulationProfile as TubeProfile;
                if (insulationProfile != null)
                {
                    double insulationThickness = insulationProfile.Thickness;

                    // Get first available ductLiningType from document
                    Autodesk.Revit.DB.Mechanical.DuctInsulationType dit = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Mechanical.DuctInsulationType)).FirstOrDefault() as Autodesk.Revit.DB.Mechanical.DuctInsulationType;
                    if (dit == null)
                    {
                        BH.Engine.Reflection.Compute.RecordError("You must first create a duct with insulation in your Revit model.");
                        return null;
                    }
                    // Create ductInsulation
                    Autodesk.Revit.DB.Mechanical.DuctInsulation di = Autodesk.Revit.DB.Mechanical.DuctInsulation.Create(document, revitDuct.Id, dit.Id, insulationThickness);
                }
                //Parameters not found in RevitAPI for Round Ducts, needs further investigation. Not critical for current workflows. 
                //double profileThickness = elementProfile.Thickness;
                //Oval Ducts are currently unsupported in the BHoM. No ShapeProfile exists. 
            }
            else
            {
                BH.Engine.Reflection.Compute.RecordError("No objects created. Only Box or TubeProfiles are supported.");
                return null;
            }

            refObjects.AddOrReplace(duct, revitDuct);
            return revitDuct;
        }

        /***************************************************/
    }
}
