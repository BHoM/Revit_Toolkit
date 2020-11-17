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

            // Identify parameters
            double flowRate = duct.FlowRate;

            object ductStart = duct.StartPoint;
            object ductEnd = duct.EndPoint;
            //double elementThickness = duct.SectionProperty.SectionProfile.ElementProfile.
            //double liningThickness = 

            // Settings
            settings = settings.DefaultIfNull();



            // Need to learn more about ductType
            DuctType ductType = null;
            ShapeType bhomDuctShape = duct.SectionProperty.SectionProfile.ElementProfile.Shape;
            ConnectorProfileType revitDuctShape = ConnectorProfileType.Invalid;

            //switch (bhomDuctShape)
            //{
            //    case ShapeType.Box:
            //        return ConnectorProfileType.Rectangular;
            //}

            if (bhomDuctShape == ShapeType.Box)
                revitDuctShape = revitDuct.DuctShape(settings);

            // Set Level
            double bottomElevation = Math.Min(duct.StartPoint.Z, duct.EndPoint.Z).FromSI(UnitType.UT_Length);
            double topElevation = Math.Max(duct.StartPoint.Z, duct.EndPoint.Z).FromSI(UnitType.UT_Length);

            Level level = document.LevelBelow(bottomElevation, settings);
            if (level == null)
                return null;

            // Set Connectors
            Connector startConnector = revitDuct.ConnectorManager.Lookup(0);
            Connector endConnector = revitDuct.ConnectorManager.Lookup(1);

            startConnector.Origin = duct.StartPoint.ToRevit();
            endConnector.Origin = duct.EndPoint.ToRevit();

            // Create Revit Duct
            revitDuct = Duct.Create(document, ductType.Id, level.Id, startConnector, endConnector);

            // Copy parameters from BHoM object to Revit element
            revitDuct.CopyParameters(duct, settings);

            // Update top and bottom offset constraints for sloping or general offsets
            //Level bottomLevel = document.GetElement(revitDuct.LookupParameterElementId(BuiltInParameter.))


            refObjects.AddOrReplace(duct, revitDuct);
            return revitDuct;
        }

        /***************************************************/
    }
}
