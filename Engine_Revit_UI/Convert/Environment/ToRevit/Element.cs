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

using BH.oM.Environment.Elements;
using BH.oM.Adapters.Revit.Settings;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Element ToRevitElement(this oM.Environment.Elements.Panel panel, Document document, PushSettings pushSettings = null)
        {
            if (panel == null)
                return null;

            switch(panel.Type)
            {
                case oM.Environment.Elements.PanelType.Wall:
                case oM.Environment.Elements.PanelType.WallExternal:
                case oM.Environment.Elements.PanelType.WallInternal:
                    return ToRevitElement_Wall(panel, document, pushSettings);
            }

            Compute.AnalyticalObjectConversionWarning(panel, typeof(oM.Environment.Elements.Panel));
            return null;
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static Wall ToRevitElement_Wall(this oM.Environment.Elements.Panel panel, Document document, PushSettings pushSettings = null)
        {
            if (panel == null || panel.ExternalEdges == null)
                return null;

            Wall aWall = pushSettings.FindRefObject<Wall>(document, panel.BHoM_Guid);
            if (aWall != null)
                return aWall;

            pushSettings.DefaultIfNull();

            WallType aWallType = null;
            if (aWallType == null)
            {
                string aFamilyTypeName = BH.Engine.Adapters.Revit.Query.FamilyTypeName(panel);
                if (!string.IsNullOrEmpty(aFamilyTypeName))
                {
                    List<WallType> aWallTypeList = new FilteredElementCollector(document).OfClass(typeof(WallType)).Cast<WallType>().ToList().FindAll(x => x.Name == aFamilyTypeName);
                    if (aWallTypeList != null || aWallTypeList.Count() != 0)
                        aWallType = aWallTypeList.First();
                }
            }

            if (aWallType == null)
            {
                string aFamilyTypeName = panel.Name;

                if (!string.IsNullOrEmpty(aFamilyTypeName))
                {
                    List<WallType> aWallTypeList = new FilteredElementCollector(document).OfClass(typeof(WallType)).Cast<WallType>().ToList().FindAll(x => x.Name == aFamilyTypeName);
                    if (aWallTypeList != null || aWallTypeList.Count() != 0)
                        aWallType = aWallTypeList.First();
                }
            }

            if (aWallType == null)
                return null;

            double aLowElevation = panel.LowElevation();
            if (double.IsNaN(aLowElevation))
                return null;

            Level aLevel = document.LowLevel(aLowElevation, true);
            if (aLevel == null)
                return null;

            aWall = Wall.Create(document, Convert.ToRevitCurveList(panel, pushSettings), aWallType.Id, aLevel.Id, false);

            Parameter aParameter = null;

            aParameter = aWall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE);
            if (aParameter != null)
                aParameter.Set(ElementId.InvalidElementId);
            aParameter = aWall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
            if (aParameter != null)
            {
                double aHeight = (panel.HighElevation() - aLowElevation).FromSI(UnitType.UT_Length);
                aParameter.Set(aHeight);
            }

            double aElevation_Level = aLevel.Elevation;
            if (pushSettings.ConvertUnits)
                aElevation_Level = aElevation_Level.ToSI(UnitType.UT_Length);

            if (System.Math.Abs(aLowElevation - aElevation_Level) > oM.Geometry.Tolerance.MacroDistance)
            {
                aParameter = aWall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET);
                if (aParameter != null)
                {
                    double aOffset = aLowElevation - aElevation_Level;
                    if (pushSettings.ConvertUnits)
                        aOffset = aOffset.FromSI(UnitType.UT_Length);

                    aParameter.Set(aOffset);
                }
            }

            aWall.CheckIfNullPush(panel);
            if (aWall == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aWall, panel, new BuiltInParameter[] { BuiltInParameter.WALL_BASE_CONSTRAINT, BuiltInParameter.WALL_BASE_OFFSET }, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(panel, aWall);

            return aWall;
        }

        /***************************************************/
    }
}