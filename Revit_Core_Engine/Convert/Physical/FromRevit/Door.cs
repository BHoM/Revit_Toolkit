/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Physical.Elements;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit FamilyInstance to BH.oM.Physical.Elements.Door.")]
        [Input("familyInstance", "Revit FamilyInstance to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("door", "BH.oM.Physical.Elements.Door resulting from converting the input Revit FamilyInstance.")]
        public static Door DoorFromRevit(this FamilyInstance familyInstance, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return familyInstance.DoorFromRevit(null as HostObject, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts a Revit FamilyInstance to BH.oM.Physical.Elements.Door.")]
        [Input("familyInstance", "Revit FamilyInstance to be converted.")]
        [Input("host", "Revit Element hosting the FamilyInstance to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("door", "BH.oM.Physical.Elements.Door resulting from converting the input Revit FamilyInstance.")]
        public static Door DoorFromRevit(this FamilyInstance familyInstance, HostObject host = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (familyInstance == null)
                return null;

            settings = settings.DefaultIfNull();

            string refId = familyInstance.Id.ReferenceIdentifier(host);
            Door door = refObjects.GetValue<Door>(refId);
            if (door != null)
                return door;

            BH.oM.Geometry.ISurface location = familyInstance.OpeningSurface(host, settings);
            if (location == null)
            {
                if (host == null)
                    BH.Engine.Base.Compute.RecordWarning(String.Format("Location of the door could not be retrieved from the model (possibly it has zero area or lies on a non-planar face). A door object without location has been returned. Revit ElementId: {0}", familyInstance.Id.IntegerValue));
                else
                {
                    BH.Engine.Base.Compute.RecordWarning(String.Format("Location of the door could not be retrieved from the model (possibly it has zero area or lies on a non-planar face), the opening has been skipped. Revit ElementId: {0}", familyInstance.Id.IntegerValue));
                    return null;
                }
            }

            door = new Door { Location = location, Name = familyInstance.FamilyTypeFullName() };
            FamilySymbol familySymbol = familyInstance.Document.GetElement(familyInstance.GetTypeId()) as FamilySymbol;
            door.Construction = familySymbol?.ConstructionFromRevit(settings, refObjects);

            //Set identifiers, parameters & custom data
            door.SetIdentifiers(familyInstance);
            door.CopyParameters(familyInstance, settings.MappingSettings);
            door.SetProperties(familyInstance, settings.MappingSettings);

            refObjects.AddOrReplace(refId, door);
            return door;
        }

        /***************************************************/

        [Description("Converts a Revit FamilyInstance to BH.oM.Physical.Elements.Door using the provided location.")]
        [Input("familyInstance", "Revit FamilyInstance to be converted.")]
        [Input("location", "Location to be applied to the returned object (the location is not queried from the object itself.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("door", "BH.oM.Physical.Elements.Door resulting from converting the input Revit FamilyInstance and applying the provided location.")]
        public static Door DoorFromRevit(this FamilyInstance familyInstance, BH.oM.Geometry.ISurface location = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (familyInstance == null || location == null)
                return null;

            settings = settings.DefaultIfNull();

            Door door = new Door { Location = location, Name = familyInstance.FamilyTypeFullName() };
            FamilySymbol familySymbol = familyInstance.Document.GetElement(familyInstance.GetTypeId()) as FamilySymbol;
            door.Construction = familySymbol?.ConstructionFromRevit(settings, refObjects);

            //Set identifiers, parameters & custom data
            door.SetIdentifiers(familyInstance);
            door.CopyParameters(familyInstance, settings.MappingSettings);
            door.SetProperties(familyInstance, settings.MappingSettings);

            return door;
        }

        /***************************************************/
    }
}

