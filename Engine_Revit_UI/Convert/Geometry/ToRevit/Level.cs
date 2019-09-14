/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Settings;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        internal static Level ToRevitLevel(this oM.Geometry.SettingOut.Level level, Document document, PushSettings pushSettings = null)
        {
            Level aLevel = pushSettings.FindRefObject<Level>(document, level.BHoM_Guid);
            if (aLevel != null)
                return aLevel;

            pushSettings.DefaultIfNull();

            ElementId aElementId = level.ElementId();

            if (aElementId != null && aElementId != ElementId.InvalidElementId)
                aLevel = document.GetElement(aElementId) as Level;

            if (aLevel == null)
                aLevel = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList().Find(x => x.Name == level.Name);

            if (aLevel == null)
            {
                double aElevation = level.Elevation;
                if (pushSettings.ConvertUnits)
                    aElevation = Convert.FromSI(aElevation, UnitType.UT_Length);

                aLevel = Level.Create(document, aElevation);
                aLevel.Name = level.Name;
            }

            aLevel.CheckIfNullPush(level);
            if (aLevel == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aLevel, level, new BuiltInParameter[] { BuiltInParameter.DATUM_TEXT, BuiltInParameter.LEVEL_ELEV }, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(level, aLevel);

            return aLevel;
        }

        /***************************************************/
    }
}