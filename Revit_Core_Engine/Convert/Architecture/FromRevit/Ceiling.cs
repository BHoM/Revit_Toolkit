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
using BH.oM.Geometry;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BH.Revit.Engine.Core.Objects;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit Ceiling to BH.oM.Architecture.Elements.Ceiling.")]
        [Input("ceiling", "Revit Ceiling to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("ceiling", "BH.oM.Architecture.Elements.Ceiling resulting from converting the input Revit Ceiling.")]
        public static oM.Architecture.Elements.Ceiling CeilingFromRevit(this Ceiling ceiling, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (ceiling == null)
                return null;

            settings = settings.DefaultIfNull();

            oM.Architecture.Elements.Ceiling bHoMCeiling = refObjects.GetValue<oM.Architecture.Elements.Ceiling>(ceiling.Id);
            if (bHoMCeiling != null)
                return bHoMCeiling;
            
            oM.Physical.Constructions.Construction construction = (ceiling.Document.GetElement(ceiling.GetTypeId()) as HostObjAttributes).ConstructionFromRevit(null, settings, refObjects);

            ISurface location = null;
            Dictionary<PlanarSurface, List<PlanarSurface>> surfaces;
            SurfaceCache cache = refObjects.GetValue<SurfaceCache>(ceiling.Id.SurfaceCacheKey());
            if (cache != null)
                surfaces = cache.Surfaces;
            else
                surfaces = ceiling.PanelSurfaces(null, settings);

            if (surfaces != null && surfaces.Count != 0)
            {
                List<ISurface> locations = new List<ISurface>();
                foreach (KeyValuePair<PlanarSurface, List<PlanarSurface>> kvp in surfaces)
                {
                    locations.Add(BH.Engine.Geometry.Create.PlanarSurface(kvp.Key.ExternalBoundary, kvp.Value.Select(x => x.ExternalBoundary).ToList()));
                }

                if (locations.Count == 1)
                    location = locations[0];
                else
                    location = new PolySurface { Surfaces = locations };
            }
            else
                ceiling.NoPanelLocationError();

            bHoMCeiling = new oM.Architecture.Elements.Ceiling { Surface = location, Construction = construction };
            bHoMCeiling.Name = ceiling.FamilyTypeFullName();

            // Add ceiling patterns
            List<BH.oM.Geometry.Line> ceilingPatterns = new List<BH.oM.Geometry.Line>();
            if (location is PlanarSurface)
                ceilingPatterns.AddRange(ceiling.CeilingPattern(settings, location as PlanarSurface));
            else if (location is PolySurface)
            {
                foreach (ISurface surface in ((PolySurface)location).Surfaces)
                {
                    if (surface is PlanarSurface)
                        ceilingPatterns.AddRange(ceiling.CeilingPattern(settings, surface as PlanarSurface));
                }
            }

            if (ceilingPatterns.Count != 0)
                bHoMCeiling.Tiles = BH.Engine.Architecture.Compute.CeilingTiles(bHoMCeiling, ceilingPatterns);

            if (surfaces.Values.Where(x => x != null).Sum(x => x.Count) != 0)
                BH.Engine.Base.Compute.RecordWarning("Currently ceiling openings are not taken into account when generating ceilings.");
            
            //Set identifiers, parameters & custom data
            bHoMCeiling.SetIdentifiers(ceiling);
            bHoMCeiling.CopyParameters(ceiling, settings.MappingSettings);
            bHoMCeiling.SetProperties(ceiling, settings.MappingSettings);

            refObjects.AddOrReplace(ceiling.Id, bHoMCeiling);
            return bHoMCeiling;
        }

        /***************************************************/
    }
}



