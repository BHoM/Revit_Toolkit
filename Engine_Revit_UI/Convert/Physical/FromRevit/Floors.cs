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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Environment.Fragments;
using BH.oM.Geometry;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static List<oM.Physical.Elements.Floor> FloorsFromRevit(this Floor floor, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<oM.Physical.Elements.Floor> floors = refObjects.GetValues<oM.Physical.Elements.Floor>(floor.Id);
            if (floors != null && floors.Count != 0)
                return floors;
            
            HostObjAttributes hostObjAttributes = floor.Document.GetElement(floor.GetTypeId()) as HostObjAttributes;
            oM.Physical.Constructions.Construction construction = hostObjAttributes.ConstructionFromRevit(settings, refObjects);
            string materialGrade = floor.MaterialGrade(settings);
            construction = construction.UpdateMaterialProperties(hostObjAttributes, materialGrade, settings, refObjects);

            Dictionary<PlanarSurface, List<oM.Physical.Elements.IOpening>> dictionary = floor.PlanarSurfaceDictionary(true, settings);
            if (dictionary == null)
                return null;

            floors = new List<oM.Physical.Elements.Floor>();
            foreach (KeyValuePair<PlanarSurface, List<oM.Physical.Elements.IOpening>> kvp in dictionary)
            {
                oM.Physical.Elements.Floor bHoMFloor = BH.Engine.Physical.Create.Floor(kvp.Key, construction);

                if (kvp.Value != null)
                    bHoMFloor.Openings = kvp.Value;

                bHoMFloor.Name = floor.FamilyTypeFullName();

                //BEnv origin context fragment
                OriginContextFragment originContext = floor.OriginContext(settings);

                //Set identifiers, parameters & custom data
                bHoMFloor.SetIdentifiers(floor);
                bHoMFloor.SetCustomData(floor, settings.ParameterSettings);
                bHoMFloor.SetParameters(floor, settings.ParameterSettings);

                refObjects.AddOrReplace(floor.Id, bHoMFloor);
                floors.Add(bHoMFloor);
            }

            return floors;
        }

        /***************************************************/
    }
}
