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

        public static List<oM.Physical.Elements.Wall> WallsFromRevit(this Wall wall, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<oM.Physical.Elements.Wall> walls = refObjects.GetValues<oM.Physical.Elements.Wall>(wall.Id);
            if (walls != null && walls.Count != 0)
                return walls;

            if (wall.StackedWallOwnerId != null && wall.StackedWallOwnerId != ElementId.InvalidElementId)
                return null;
            
            HostObjAttributes hostObjAttributes = wall.Document.GetElement(wall.GetTypeId()) as HostObjAttributes;
            oM.Physical.Constructions.Construction construction = hostObjAttributes.ConstructionFromRevit(settings, refObjects);
            string materialGrade = wall.MaterialGrade(settings);
            construction = construction.UpdateMaterialProperties(hostObjAttributes, materialGrade, settings, refObjects);

            Dictionary<PlanarSurface, List<oM.Physical.Elements.IOpening>> dictionary = wall.PlanarSurfaceDictionary(true, settings);
            if (dictionary == null)
                return null;

            walls = new List<oM.Physical.Elements.Wall>();
            foreach (KeyValuePair<PlanarSurface, List<oM.Physical.Elements.IOpening>> kvp in dictionary)
            {
                oM.Physical.Elements.Wall bHoMWall = BH.Engine.Physical.Create.Wall(kvp.Key, construction);
                CurtainGrid curtainGrid = wall.CurtainGrid;
                if (curtainGrid != null)
                {
                    foreach (ElementId elementID in curtainGrid.GetPanelIds())
                    {
                        Panel panel = wall.Document.GetElement(elementID) as Panel;
                        if (panel == null)
                            continue;
                    }
                }

                if (kvp.Value != null)
                    bHoMWall.Openings = kvp.Value;

                bHoMWall.Name = wall.FamilyTypeFullName();

                //BEnv origin context fragment
                OriginContextFragment originContext = wall.OriginContext(settings);

                //Set identifiers, parameters & custom data
                bHoMWall.SetIdentifiers(wall);
                bHoMWall.SetCustomData(wall, settings.ParameterSettings);
                bHoMWall.SetParameters(wall, settings.ParameterSettings);

                refObjects.AddOrReplace(wall.Id, bHoMWall);
                walls.Add(bHoMWall);
            }

            return walls;
        }

        /***************************************************/
    }
}
