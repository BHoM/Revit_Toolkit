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
using BH.oM.Environment.Elements;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<Panel> UpdateBuildingElementTypeByCustomData(this List<Panel> elements)
        {
            //Temporary fix for Revit...
            foreach (Panel be in elements)
            {
                if (be.CustomData.ContainsKey("Type SAM_BuildingElementType"))
                {
                    PanelType bType = oM.Environment.Elements.PanelType.Undefined;
                    string type = be.CustomData["Type SAM_BuildingElementType"] as string;
                    type = type.ToLower();

                    if (type == "underground wall")
                        bType = oM.Environment.Elements.PanelType.UndergroundWall;
                    else if (type == "curtain wall")
                        bType = oM.Environment.Elements.PanelType.CurtainWall;
                    else if (type == "external wall")
                        bType = oM.Environment.Elements.PanelType.WallExternal;
                    else if (type == "internal wall")
                        bType = oM.Environment.Elements.PanelType.WallInternal;
                    else if (type == "no type")
                        bType = oM.Environment.Elements.PanelType.Undefined;
                    else if (type == "shade")
                        bType = oM.Environment.Elements.PanelType.Shade;
                    else if (type == "solar/pv panel")
                        bType = oM.Environment.Elements.PanelType.SolarPanel;
                    else if (type == "roof")
                        bType = oM.Environment.Elements.PanelType.Roof;
                    else if (type == "underground ceiling")
                        bType = oM.Environment.Elements.PanelType.UndergroundCeiling;
                    else if (type == "internal floor")
                        bType = oM.Environment.Elements.PanelType.FloorInternal;
                    else if (type == "exposed floor")
                        bType = oM.Environment.Elements.PanelType.FloorExposed;
                    else if (type == "slab on grade")
                        bType = oM.Environment.Elements.PanelType.SlabOnGrade;

                    be.Type = bType;
                }
            }

            return elements;
        }

        /***************************************************/

        public static Panel UpdateBuildingElementTypeByCustomData(this Panel element)
        {
            return (new List<Panel> { element }).UpdateBuildingElementTypeByCustomData()[0];
        }

        /***************************************************/
    }
}
