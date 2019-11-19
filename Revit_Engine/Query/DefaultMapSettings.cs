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

using BH.oM.Environment.Fragments;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit.Generic;
using BH.oM.Geometry.ShapeProfiles;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
  
        public static MapSettings DefaultMapSettings()
        {
            //TODO: To be moved to DataSets??

            List<TypeMap> aTypeMapList = new List<TypeMap>();

            TypeMap aTypeMap = null;

            aTypeMap = Create.TypeMap(typeof(PanelContextFragment));
            aTypeMap = aTypeMap.AddMap("IsAir", new string[] { "IsAir", "BHE_IsAir", "SAM_BuildingElementAir" });
            aTypeMap = aTypeMap.AddMap("Colour", new string[] { "Colour", "BHE_Colour", "SAM_BuildingElementColour" });
            aTypeMap = aTypeMap.AddMap("IsGround", "SAM_BuildingElementGround");
            aTypeMapList.Add(aTypeMap);

            aTypeMap = Create.TypeMap(typeof(OriginContextFragment));
            aTypeMap = aTypeMap.AddMap("Description", "SAM_BuildingElementDescription");
            aTypeMapList.Add(aTypeMap);

            aTypeMap = Create.TypeMap(typeof(PanelAnalyticalFragment));
            aTypeMap = aTypeMap.AddMap("UValue", "SAM_UValue");
            aTypeMap = aTypeMap.AddMap("GValue", "SAM_gValue");
            aTypeMap = aTypeMap.AddMap("LTValue", "SAM_LtValue");
            aTypeMapList.Add(aTypeMap);

            aTypeMap = Create.TypeMap(typeof(BuildingAnalyticalFragment));
            aTypeMap = aTypeMap.AddMap("NorthAngle", "SAM_NorthAngle");
            aTypeMapList.Add(aTypeMap);

            aTypeMap = Create.TypeMap(typeof(SpaceContextFragment));
            aTypeMap = aTypeMap.AddMap("IsExternal", "SAM_ExternalZone");
            aTypeMapList.Add(aTypeMap);

            /*aTypeMap = Create.TypeMap(typeof(ElementProperties));
            aTypeMap = aTypeMap.AddMap("BuildingElementType", "SAM_BuildingElementType");
            aTypeMapList.Add(aTypeMap);*/

            aTypeMap = Create.TypeMap(typeof(CircleProfile));
            aTypeMap = aTypeMap.AddMap("Diameter", new string[] { "BHE_Diameter", "Diameter", "d", "D", "OD" });
            aTypeMap = aTypeMap.AddMap("Radius", new string[] { "BHE_Radius", "Radius", "r", "R" });
            aTypeMapList.Add(aTypeMap);

            aTypeMap = Create.TypeMap(typeof(TubeProfile));
            aTypeMap = aTypeMap.AddMap("Diameter", new string[] { "BHE_Diameter", "Diameter", "d", "D", "OD" });
            aTypeMap = aTypeMap.AddMap("Thickness", new string[] { "Wall Nominal Thickness", "Wall Thickness", "t", "T" });
            aTypeMapList.Add(aTypeMap);

            aTypeMap = Create.TypeMap(typeof(FabricatedISectionProfile));
            aTypeMap = aTypeMap.AddMap("Height", new string[] { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" });
            aTypeMap = aTypeMap.AddMap("TopFlangeWidth", new string[] { "Top Flange Width", "bt", "bf_t", "bft", "b1", "b", "B", "Bt" });
            aTypeMap = aTypeMap.AddMap("BotFlangeWidth", new string[] { "Bottom Flange Width", "bb", "bf_b", "bfb", "b2", "b", "B", "Bb" });
            aTypeMap = aTypeMap.AddMap("WebThickness", new string[] { "Web Thickness", "Stem Width", "tw", "t", "T" });
            aTypeMap = aTypeMap.AddMap("TopFlangeThickness", new string[] { "Top Flange Thickness", "tft", "tf_t", "tf", "T", "t" });
            aTypeMap = aTypeMap.AddMap("BotFlangeThickness", new string[] { "Bottom Flange Thickness", "tfb", "tf_b", "tf", "T", "t" });
            aTypeMap = aTypeMap.AddMap("WeldSize", new string[] { "k", "Weld Size" });
            aTypeMapList.Add(aTypeMap);

            aTypeMap = Create.TypeMap(typeof(RectangleProfile));
            aTypeMap = aTypeMap.AddMap("Height", new string[] { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" });
            aTypeMap = aTypeMap.AddMap("Width", new string[] { "b", "BHE_Width", "Width", "w", "B", "W", "bf", "D" });
            aTypeMap = aTypeMap.AddMap("CornerRadius", new string[] { "Corner Radius", "r", "r1" });
            aTypeMapList.Add(aTypeMap);

            aTypeMap = Create.TypeMap(typeof(AngleProfile));
            aTypeMap = aTypeMap.AddMap("Height", new string[] { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" });
            aTypeMap = aTypeMap.AddMap("Width", new string[] { "b", "BHE_Width", "Width", "w", "B", "W", "bf", "D" });
            aTypeMap = aTypeMap.AddMap("WebThickness", new string[] { "Web Thickness", "Stem Width", "tw", "t", "T" });
            aTypeMap = aTypeMap.AddMap("FlangeThickness", new string[] { "Flange Thickness", "Slab Depth", "tf", "T", "t" });
            aTypeMap = aTypeMap.AddMap("RootRadius", new string[] { "Web Fillet", "Root Radius", "r", "r1", "tr", "kr", "R1", "R", "t" });
            aTypeMap = aTypeMap.AddMap("ToeRadius", new string[] { "Flange Fillet", "Toe Radius", "r2", "R2", "t" });
            aTypeMapList.Add(aTypeMap);

            aTypeMap = Create.TypeMap(typeof(BoxProfile));
            aTypeMap = aTypeMap.AddMap("Height", new string[] { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" });
            aTypeMap = aTypeMap.AddMap("Width", new string[] { "b", "BHE_Width", "Width", "w", "B", "W", "bf", "D" });
            aTypeMap = aTypeMap.AddMap("Thickness", new string[] { "Wall Nominal Thickness", "Wall Thickness", "t", "T" });
            aTypeMap = aTypeMap.AddMap("OuterRadius", new string[] { "Outer Fillet", "Outer Radius", "r2", "R2", "ro", "tr" });
            aTypeMap = aTypeMap.AddMap("InnerRadius", new string[] { "Inner Fillet", "Inner Radius", "r1", "R1", "ri", "t" });
            aTypeMapList.Add(aTypeMap);

            aTypeMap = Create.TypeMap(typeof(ChannelProfile));
            aTypeMap = aTypeMap.AddMap("Height", new string[] { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" });
            aTypeMap = aTypeMap.AddMap("FlangeWidth", new string[] { "b", "BHE_Width", "Width", "w", "B", "W", "bf", "D" });
            aTypeMap = aTypeMap.AddMap("WebThickness", new string[] { "Web Thickness", "Stem Width", "tw", "t", "T" });
            aTypeMap = aTypeMap.AddMap("FlangeThickness", new string[] { "Flange Thickness", "Slab Depth", "tf", "T", "t" });
            aTypeMap = aTypeMap.AddMap("RootRadius", new string[] { "Web Fillet", "Root Radius", "r", "r1", "tr", "kr", "R1", "R", "t" });
            aTypeMap = aTypeMap.AddMap("ToeRadius", new string[] { "Flange Fillet", "Toe Radius", "r2", "R2", "t" });
            aTypeMapList.Add(aTypeMap);

            aTypeMap = Create.TypeMap(typeof(ISectionProfile));
            aTypeMap = aTypeMap.AddMap("Height", new string[] { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" });
            aTypeMap = aTypeMap.AddMap("Width", new string[] { "b", "BHE_Width", "Width", "w", "B", "W", "bf", "D" });
            aTypeMap = aTypeMap.AddMap("WebThickness", new string[] { "Web Thickness", "Stem Width", "tw", "t", "T" });
            aTypeMap = aTypeMap.AddMap("FlangeThickness", new string[] { "Flange Thickness", "Slab Depth", "tf", "T", "t" });
            aTypeMap = aTypeMap.AddMap("RootRadius", new string[] { "Web Fillet", "Root Radius", "r", "r1", "tr", "kr", "R1", "R", "t" });
            aTypeMap = aTypeMap.AddMap("ToeRadius", new string[] { "Flange Fillet", "Toe Radius", "r2", "R2", "t" });
            aTypeMapList.Add(aTypeMap);

            aTypeMap = Create.TypeMap(typeof(TSectionProfile));
            aTypeMap = aTypeMap.AddMap("Height", new string[] { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" });
            aTypeMap = aTypeMap.AddMap("Width", new string[] { "b", "BHE_Width", "Width", "w", "B", "W", "bf", "D" });
            aTypeMap = aTypeMap.AddMap("WebThickness", new string[] { "Web Thickness", "Stem Width", "tw", "t", "T" });
            aTypeMap = aTypeMap.AddMap("FlangeThickness", new string[] { "Flange Thickness", "Slab Depth", "tf", "T", "t" });
            aTypeMap = aTypeMap.AddMap("RootRadius", new string[] { "Web Fillet", "Root Radius", "r", "r1", "tr", "kr", "R1", "R", "t" });
            aTypeMap = aTypeMap.AddMap("ToeRadius", new string[] { "Flange Fillet", "Toe Radius", "r2", "R2", "t" });
            aTypeMapList.Add(aTypeMap);

            aTypeMap = Create.TypeMap(typeof(ZSectionProfile));
            aTypeMap = aTypeMap.AddMap("Height", new string[] { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" });
            aTypeMap = aTypeMap.AddMap("FlangeWidth", new string[] { "b", "BHE_Width", "Width", "w", "B", "W", "bf", "D" });
            aTypeMap = aTypeMap.AddMap("WebThickness", new string[] { "Web Thickness", "Stem Width", "tw", "t", "T" });
            aTypeMap = aTypeMap.AddMap("FlangeThickness", new string[] { "Flange Thickness", "Slab Depth", "tf", "T", "t" });
            aTypeMap = aTypeMap.AddMap("RootRadius", new string[] { "Web Fillet", "Root Radius", "r", "r1", "tr", "kr", "R1", "R", "t" });
            aTypeMap = aTypeMap.AddMap("ToeRadius", new string[] { "Flange Fillet", "Toe Radius", "r2", "R2", "t" });
            aTypeMapList.Add(aTypeMap);

            return Create.MapSettings(aTypeMapList);

        }

        /***************************************************/
    }
}
