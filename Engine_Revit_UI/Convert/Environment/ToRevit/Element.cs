/*
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

            Wall wall = pushSettings.FindRefObject<Wall>(document, panel.BHoM_Guid);
            if (wall != null)
                return wall;

            pushSettings.DefaultIfNull();

            WallType wallType = null;
            if (wallType == null)
            {
                string familyTypeName = BH.Engine.Adapters.Revit.Query.FamilyTypeName(panel);
                if (!string.IsNullOrEmpty(familyTypeName))
                {
                    List<WallType> wallTypeList = new FilteredElementCollector(document).OfClass(typeof(WallType)).Cast<WallType>().ToList().FindAll(x => x.Name == familyTypeName);
                    if (wallTypeList != null || wallTypeList.Count() != 0)
                        wallType = wallTypeList.First();
                }
            }

            if (wallType == null)
            {
                string familyTypeName = panel.Name;

                if (!string.IsNullOrEmpty(familyTypeName))
                {
                    List<WallType> wallTypeList = new FilteredElementCollector(document).OfClass(typeof(WallType)).Cast<WallType>().ToList().FindAll(x => x.Name == familyTypeName);
                    if (wallTypeList != null || wallTypeList.Count() != 0)
                        wallType = wallTypeList.First();
                }
            }

            if (wallType == null)
                return null;

            double lowElevation = panel.LowElevation();
            if (double.IsNaN(lowElevation))
                return null;

            Level level = document.LowLevel(lowElevation);
            if (level == null)
                return null;

            wall = Wall.Create(document, Convert.ToRevitCurveList(panel), wallType.Id, level.Id, false);

            Parameter parameter = null;

            parameter = wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE);
            if (parameter != null)
                parameter.Set(ElementId.InvalidElementId);
            parameter = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
            if (parameter != null)
            {
                double height = (panel.HighElevation() - lowElevation).FromSI(UnitType.UT_Length);
                parameter.Set(height);
            }

            double levelElevation = level.Elevation.ToSI(UnitType.UT_Length);

            if (System.Math.Abs(lowElevation - levelElevation) > oM.Geometry.Tolerance.MacroDistance)
            {
                parameter = wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET);
                if (parameter != null)
                {
                    double offset = (lowElevation - levelElevation).FromSI(UnitType.UT_Length);
                    offset = offset.FromSI(UnitType.UT_Length);

                    parameter.Set(offset);
                }
            }

            wall.CheckIfNullPush(panel);
            if (wall == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(wall, panel, new BuiltInParameter[] { BuiltInParameter.WALL_BASE_CONSTRAINT, BuiltInParameter.WALL_BASE_OFFSET });

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(panel, wall);

            return wall;
        }

        /***************************************************/
    }
}