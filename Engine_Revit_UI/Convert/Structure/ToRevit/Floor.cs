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

        internal static Floor ToRevitFloor(this oM.Structure.Elements.Panel panel, Document document, PushSettings pushSettings = null)
        {
            //TODO: if no CustomData, set the level & offset manually?

            if (panel == null || document == null)
                return null;

            Floor aFloor = pushSettings.FindRefObject<Floor>(document, panel.BHoM_Guid);
            if (aFloor != null)
                return aFloor;

            pushSettings.DefaultIfNull();

            object aCustomDataValue = null;

            CurveArray aCurves = new CurveArray();
            foreach (Curve c in panel.ExternalEdgeCurves().Select(c => c.ToRevitCurve(pushSettings)))
            {
                aCurves.Append(c);
            }
            if (panel.Openings.Count != 0) panel.OpeningInPanelWarning();

            Level aLevel = null;

            aCustomDataValue = panel.CustomDataValue("Level");
            if (aCustomDataValue != null && aCustomDataValue is int)
            {
                ElementId aElementId = new ElementId((int)aCustomDataValue);
                aLevel = document.GetElement(aElementId) as Level;
            }

            if (aLevel == null)
                aLevel = Query.BottomLevel(panel.Outline(), document, pushSettings.ConvertUnits);

            FloorType aFloorType = panel.Property.ToRevitFloorType(document, pushSettings);

            if (aFloorType == null)
            {
                aFloorType = Query.ElementType(panel, document, BuiltInCategory.OST_Floors) as FloorType;

                if (aFloorType == null)
                {
                    BH.Engine.Reflection.Compute.RecordError(string.Format("Floor type has not been found for given BHoM panel property. BHoM Guid: {0}", panel.BHoM_Guid));
                    return null;
                }
            }

            if (aFloorType.IsFoundationSlab)
                aFloor = document.Create.NewFoundationSlab(aCurves, aFloorType, aLevel, true, XYZ.BasisZ);
            else
                aFloor = document.Create.NewFloor(aCurves, aFloorType, aLevel, true);

            aFloor.CheckIfNullPush(panel);
            if (aFloorType == null)
                return null;

            if (pushSettings.CopyCustomData)
            {
                BuiltInParameter[] paramsToIgnore = new BuiltInParameter[]
                {
                        BuiltInParameter.ELEM_FAMILY_PARAM,
                        BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
                        BuiltInParameter.ALL_MODEL_IMAGE,
                        BuiltInParameter.ELEM_TYPE_PARAM
                };
                Modify.SetParameters(aFloor, panel, paramsToIgnore, pushSettings.ConvertUnits);
            }

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(panel, aFloor);

            return aFloor;
        }
    }
}
