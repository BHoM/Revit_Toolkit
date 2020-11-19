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
            //TODO: in the future you could look for the existing connectors and check if any of them overlaps with start/end of this duct - if so, use it in Duct.Create.
            // hacky/heavy way of getting all connectors in the link below - however, i would rather filter the connecting elements out by type/bounding box first for performance reasons
            // https://thebuildingcoder.typepad.com/blog/2010/06/retrieve-mep-elements-and-connectors.html
            MechanicalSystemType mst = new FilteredElementCollector(document).OfClass(typeof(MechanicalSystemType)).OfType<MechanicalSystemType>().FirstOrDefault();
            revitDuct = Duct.Create(document, mst.Id, ductType.Id, level.Id, start, end);


            // Copy parameters from BHoM object to Revit element
            revitDuct.CopyParameters(duct, settings);

            //TODO: copy relevant properties to parameters
            double flowRate = duct.FlowRate;

            //double elementThickness = duct.SectionProperty.SectionProfile.ElementProfile.
            //double liningThickness = 


            // Update top and bottom offset constraints for sloping or general offsets
            //Level bottomLevel = document.GetElement(revitDuct.LookupParameterElementId(BuiltInParameter.))


            refObjects.AddOrReplace(duct, revitDuct);
            return revitDuct;
        }

        /***************************************************/
    }
}
