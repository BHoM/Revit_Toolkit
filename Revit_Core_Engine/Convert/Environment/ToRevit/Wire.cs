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
        // Verify view
        // SectionProperty.ToRevitElementType to BuiltInCategory is unknown and currently set to OST_ElectricalCircuit
        // Electrical Connectors are not functioning, no create method in RevitAPI. 
        //     > Attempted to use Create.ElectricalConnector, but unsuccessful as Wire Create only works with standard Connectors. Ref: https://www.revitapidocs.com/2018.1/72b0d6cd-c165-1d17-6b4b-048961a3fa16.htm
        // Help: Create new List<XYZ> to be used in Wire creation 
        // Wire.SectionProperty.SectionProfile is unused in RevitAPI. Not sure how we make the most of this data
        // Wire.Flowrate needs corresponding builtInParameter

        public static Autodesk.Revit.DB.Electrical.Wire ToRevitWire(this oM.MEP.System.WireSegment wire, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            // Check valid pipe object
            if (wire == null)
                return null;

            // Construct Revit Wire
            Autodesk.Revit.DB.Electrical.Wire revitWire = refObjects.GetValue<Autodesk.Revit.DB.Electrical.Wire>(document, wire.BHoM_Guid);
            if (revitWire != null)
                return revitWire;

            // Get Active User View
            // Todo: View flexibility
            Autodesk.Revit.DB.View view = document.ActiveView;
            if (view == null)
            {
                BH.Engine.Reflection.Compute.RecordError("You must have an active view set.");
            }

            // Settings
            settings = settings.DefaultIfNull();

            // Wire type
            Autodesk.Revit.DB.Electrical.WireType wireType = wire.SectionProperty.ToRevitElementType(document, new List<BuiltInCategory> { BuiltInCategory.OST_ElectricalCircuit }, settings, refObjects) as Autodesk.Revit.DB.Electrical.WireType;

            // Creation of new Wire Electrical Connector Elements
            // ToDo: Work on Electrical System types and assignments
            Autodesk.Revit.DB.Connector connectorStart = null;
            Autodesk.Revit.DB.Connector connectorEnd = null;


            // End points
            XYZ start = wire.StartPoint.ToRevit();
            XYZ end = wire.EndPoint.ToRevit();

            List<XYZ> verticies = new List<XYZ>(); // Help: Create verticies for use in Wire Creation. 

            // Level
            Level level = document.LevelBelow(Math.Min(start.Z, end.Z), settings);
            if (level == null)
                return null;

            // Default system used for now
            Autodesk.Revit.DB.Electrical.ElectricalSystem es = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Electrical.ElectricalSystem)).OfType<Autodesk.Revit.DB.Electrical.ElectricalSystem>().FirstOrDefault();

            revitWire = Autodesk.Revit.DB.Electrical.Wire.Create(document, wireType.Id, view.Id, Autodesk.Revit.DB.Electrical.WiringType.Arc, verticies, connectorStart, connectorEnd); // ToDo: Connector classes are nonfunctional. 

            // Copy parameters from BHoM object to Revit element
            revitWire.CopyParameters(wire, settings);

            SectionProfile sectionProfile = wire.SectionProperty.SectionProfile;
            if (sectionProfile == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Wire creation requires a SectionProfile. \n No elements created.");
                return null;
            }

            WireSectionProperty wireSectionProperty = wire.SectionProperty; //WireSectionProperty is not implemented in RevitAPI

            //// ToDo: Verify builtInParameter for correct value. This is not functioning as expected
            //double flowRate = wire.FlowRate;
            //revitWire.SetParameter(BuiltInParameter., flowRate); //Not being set correctly.

            refObjects.AddOrReplace(wire, revitWire);
            return revitWire;
        }

        /***************************************************/
    }
}
