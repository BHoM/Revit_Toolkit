﻿/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using BH.oM.Base;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static BH.oM.Geometry.SettingOut.Level ToBHoMLevel(this Level revitLevel, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            oM.Geometry.SettingOut.Level level = pullSettings.FindRefObject<oM.Geometry.SettingOut.Level>(revitLevel.Id.IntegerValue);
            if (level != null)
                return level;

            level = BH.Engine.Geometry.Create.Level(revitLevel.ProjectElevation.ToSI(UnitType.UT_Length));
            level.Name = revitLevel.Name;

            level = Modify.SetIdentifiers(level, revitLevel) as oM.Geometry.SettingOut.Level;
            if (pullSettings.CopyCustomData)
                level = Modify.SetCustomData(level, revitLevel) as oM.Geometry.SettingOut.Level;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(level);

            return level;
        }

        /***************************************************/
    }
}