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

using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.Engine.Adapters.Revit;
using BH.Engine.Structure;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Structure.Elements;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static Wall ToRevitWall(this oM.Structure.Elements.Panel panel, Document document, PushSettings pushSettings = null)
        {
            //TODO: if no CustomData, set the levels & base+top offsets manually?

            if (panel == null || document == null)
                return null;

            Wall aWall = pushSettings.FindRefObject<Wall>(document, panel.BHoM_Guid);
            if (aWall != null)
                return aWall;

            pushSettings.DefaultIfNull();

            object aCustomDataValue = null;

            List<Curve> aCurves = panel.ExternalEdgeCurves().Select(c => c.ToRevitCurve(pushSettings)).ToList();
            if (panel.Openings.Count != 0) panel.OpeningInPanelWarning();

            Level aLevel = null;

            aCustomDataValue = panel.CustomDataValue("Base Constraint");
            if (aCustomDataValue != null && aCustomDataValue is int)
            {
                ElementId aElementId = new ElementId((int)aCustomDataValue);
                aLevel = document.GetElement(aElementId) as Level;
            }

            if (aLevel == null)
                aLevel = Query.BottomLevel(panel.Outline(), document, pushSettings.ConvertUnits);

            WallType aWallType = panel.Property.ToRevitWallType(document, pushSettings);

            if (aWallType == null)
            {
                aWallType = Query.ElementType(panel, document, BuiltInCategory.OST_Walls) as WallType;

                if (aWallType == null)
                {
                    BH.Engine.Reflection.Compute.RecordError(string.Format("Wall type has not been found for given BHoM panel property. BHoM Guid: {0}", panel.BHoM_Guid));
                    return null;
                }
            }

            aWall = Wall.Create(document, aCurves, aWallType.Id, aLevel.Id, true);

            aWall.CheckIfNullPush(panel);
            if (aWall == null)
                return null;

            if (pushSettings.CopyCustomData)
            {
                BuiltInParameter[] paramsToIgnore = new BuiltInParameter[]
                {
                        BuiltInParameter.ELEM_FAMILY_PARAM,
                        BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
                        BuiltInParameter.ALL_MODEL_IMAGE,
                        BuiltInParameter.WALL_KEY_REF_PARAM,
                        BuiltInParameter.ELEM_TYPE_PARAM,
                    //BuiltInParameter.WALL_BASE_CONSTRAINT,
                    //BuiltInParameter.WALL_BASE_OFFSET,
                    //BuiltInParameter.WALL_HEIGHT_TYPE,
                    //BuiltInParameter.WALL_TOP_OFFSET
                };
                Modify.SetParameters(aWall, panel, paramsToIgnore, pushSettings.ConvertUnits);
            }

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(panel, aWall);

            return aWall;
        }
    }
}
