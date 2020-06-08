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
using BH.oM.Environment.Elements;
using System;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Autodesk.Revit.DB.Mechanical.Space ToRevitSpace(this Space space, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (space == null)
                return null;

            Autodesk.Revit.DB.Mechanical.Space revitSpace = refObjects.GetValue<Autodesk.Revit.DB.Mechanical.Space>(document, space.BHoM_Guid);
            if (revitSpace != null)
                return revitSpace;

            settings = settings.DefaultIfNull();

            Level level = Query.BottomLevel(space.Location.Z, document);
            if (level == null)
                return null;

            UV uv = new UV(space.Location.X.FromSI(UnitType.UT_Length), space.Location.Y.FromSI(UnitType.UT_Length));

            revitSpace = document.Create.NewSpace(level, uv);

            revitSpace.CheckIfNullPush(space);
            if (revitSpace == null)
                return null;

            revitSpace.Name = space.Name;

            // Copy parameters from BHoM object to Revit element
            revitSpace.CopyParameters(space, settings);

            refObjects.AddOrReplace(space, revitSpace);
            return revitSpace;
        }

        /***************************************************/
    }
}
