/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Converts BH.oM.Physical.Elements.Wall to a Revit Wall.")]
        [Input("wall", "BH.oM.Physical.Elements.Wall to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("wall", "Revit Wall resulting from converting the input BH.oM.Physical.Elements.Wall.")]
        public static Wall ToRevitWall(this oM.Physical.Elements.Wall wall, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (wall == null || wall.Location == null || document == null)
                return null;

            PlanarSurface planarSurface = wall.Location as PlanarSurface;
            if (planarSurface == null)
                return null;

            Wall revitWall = refObjects.GetValue<Wall>(document, wall.BHoM_Guid);
            if (revitWall != null)
                return revitWall;

            settings = settings.DefaultIfNull();

            WallType wallType = wall.ElementType(document, settings);
            if (wallType == null)
            {
                Compute.ElementTypeNotFoundWarning(wall);
                return null;
            }

            BoundingBox bbox = wall.Location.IBounds();
            if (bbox == null)
                return null;

            double bottomElevation = bbox.Min.Z.FromSI(SpecTypeId.Length);
            double topElevation = bbox.Max.Z.FromSI(SpecTypeId.Length);

            Level level = document.LevelBelow(bottomElevation, settings);
            if (level == null)
                return null;

            revitWall = Wall.Create(document, planarSurface.ExternalBoundary.IToRevitCurves(), wallType.Id, level.Id, false);
            revitWall.CheckIfNullPush(wall);
            if (revitWall == null)
                return null;

            // Copy parameters from BHoM object to Revit element
            revitWall.CopyParameters(wall, settings);

            // Update top and bottom offset constraints
            Level bottomLevel = document.GetElement(revitWall.LookupParameterElementId(BuiltInParameter.WALL_BASE_CONSTRAINT)) as Level;
            revitWall.SetParameter(BuiltInParameter.WALL_BASE_OFFSET, bottomElevation - bottomLevel.ProjectElevation, false);

            Level topLevel = document.GetElement(revitWall.LookupParameterElementId(BuiltInParameter.WALL_HEIGHT_TYPE)) as Level;
            if (topLevel != null)
                revitWall.SetParameter(BuiltInParameter.WALL_TOP_OFFSET, topElevation - topLevel.ProjectElevation, false);
            else
                revitWall.SetParameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM, topElevation - bottomElevation, false);

            refObjects.AddOrReplace(wall, revitWall);
            return revitWall;
        }

        /***************************************************/
    }
}


