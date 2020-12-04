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

        // ToDo for method functionality from top to bottom: 
        // SectionProperty.ToRevitElementType to BuiltInCategory is unknown and currently set to OST_CableTrayRun

        public static Autodesk.Revit.DB.Electrical.CableTray ToRevitCableTray(this oM.MEP.System.CableTray cableTray, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            // Check valid pipe object
            if (cableTray == null)
                return null;

            // Construct Revit Cable Tray
            Autodesk.Revit.DB.Electrical.CableTray revitTray = refObjects.GetValue<Autodesk.Revit.DB.Electrical.CableTray>(document, cableTray.BHoM_Guid);
            if (revitTray != null)
                return revitTray;

            // Settings
            settings = settings.DefaultIfNull();

            // CableTray type
            Autodesk.Revit.DB.Electrical.CableTrayType trayType = cableTray.SectionProperty.ToRevitElementType(document, new List<BuiltInCategory> { BuiltInCategory.OST_CableTrayRun }, settings, refObjects) as Autodesk.Revit.DB.Electrical.CableTrayType;

            // End points
            XYZ start = cableTray.StartPoint.ToRevit();
            XYZ end = cableTray.EndPoint.ToRevit();

            // Level
            Level level = document.LevelBelow(Math.Min(start.Z, end.Z), settings);
            if (level == null)
                return null;

            revitTray = Autodesk.Revit.DB.Electrical.CableTray.Create(document, trayType.Id, start, end, level.Id);

            // Copy parameters from BHoM object to Revit element
            revitTray.CopyParameters(cableTray, settings);

            // Set Orientation angle
            double orientationAngle = cableTray.OrientationAngle;
            if (Math.Abs(orientationAngle) > settings.AngleTolerance)
            {
                ElementTransformUtils.RotateElement(document, revitTray.Id, Line.CreateBound(start, end), orientationAngle);
            }

            SectionProfile sectionProfile = cableTray.SectionProperty.SectionProfile;
            if (sectionProfile == null)
            {
                BH.Engine.Reflection.Compute.RecordError("CableTray creation requires a SectionProfile. \n No elements created.");
                return null;
            }

            CableTraySectionProperty cableTraySectionProperty = cableTray.SectionProperty; // ToDo: Look into tray properties 

            BoxProfile elementProfile = sectionProfile.ElementProfile as BoxProfile;
            if (elementProfile == null)
                return null;

            // Set Height
            double profileHeight = elementProfile.Height;
            revitTray.SetParameter(BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM, profileHeight);

            // Set Width
            double profileWidth = elementProfile.Width;
            revitTray.SetParameter(BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM, profileWidth);

            //// ToDo: Verify builtInParameter for correct value. This is not functioning as expected

            refObjects.AddOrReplace(cableTray, revitTray);
            return revitTray;
        }

        /***************************************************/
    }
}
