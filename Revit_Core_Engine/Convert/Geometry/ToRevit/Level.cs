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
using BH.oM.Base.Attributes;
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

        [Description("Converts BH.oM.Spatial.SettingOut.Level to a Revit Level.")]
        [Input("level", "BH.oM.Spatial.SettingOut.Level to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("level", "Revit Level resulting from converting the input BH.oM.Spatial.SettingOut.Level.")]
        public static Level ToRevitLevel(this oM.Spatial.SettingOut.Level level, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            Level revitLevel = refObjects.GetValue<Level>(document, level.BHoM_Guid);
            if (revitLevel != null)
                return revitLevel;

            settings = settings.DefaultIfNull();

            List<Level> existingLevels = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
            if (existingLevels.Any(x => x.Name == level.Name))
            {
                BH.Engine.Base.Compute.RecordError($"Level named {level.Name} could not be created because a level with the same name already exists in the model. BHoM_Guid: {level.BHoM_Guid}");
                return null;
            }

            double elevation = level.Elevation.FromSI(SpecTypeId.Length);
            if (existingLevels.Any(x => Math.Abs(x.ProjectElevation - elevation) < settings.DistanceTolerance))
            {
                BH.Engine.Base.Compute.RecordError($"Level with elevation {level.Elevation} could not be created because a level with the same elevation already exists in the model. BHoM_Guid: {level.BHoM_Guid}");
                return null;
            }

            revitLevel = Level.Create(document, elevation);
            revitLevel.CheckIfNullPush(level);
            if (revitLevel == null)
                return null;

            revitLevel.Name = level.Name;

            // Copy parameters from BHoM object to Revit element
            revitLevel.CopyParameters(level, settings);

            refObjects.AddOrReplace(level, revitLevel);
            return revitLevel;
        }

        /***************************************************/
    }
}



