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

            List<TypeMap> typeMaps = new List<TypeMap>();

            TypeMap typeMap = null;

            typeMap = Create.TypeMap(typeof(PanelContextFragment));
            typeMap = typeMap.AddMap("IsAir", new string[] { "IsAir", "BHE_IsAir", "SAM_BuildingElementAir" });
            typeMap = typeMap.AddMap("Colour", new string[] { "Colour", "BHE_Colour", "SAM_BuildingElementColour" });
            typeMap = typeMap.AddMap("IsGround", "SAM_BuildingElementGround");
            typeMaps.Add(typeMap);

            typeMap = Create.TypeMap(typeof(OriginContextFragment));
            typeMap = typeMap.AddMap("Description", "SAM_BuildingElementDescription");
            typeMaps.Add(typeMap);

            typeMap = Create.TypeMap(typeof(PanelAnalyticalFragment));
            typeMap = typeMap.AddMap("UValue", "SAM_UValue");
            typeMap = typeMap.AddMap("GValue", "SAM_gValue");
            typeMap = typeMap.AddMap("LTValue", "SAM_LtValue");
            typeMaps.Add(typeMap);

            typeMap = Create.TypeMap(typeof(BuildingAnalyticalFragment));
            typeMap = typeMap.AddMap("NorthAngle", "SAM_NorthAngle");
            typeMaps.Add(typeMap);

            typeMap = Create.TypeMap(typeof(SpaceContextFragment));
            typeMap = typeMap.AddMap("IsExternal", "SAM_ExternalZone");
            typeMaps.Add(typeMap);

            typeMap = Create.TypeMap(typeof(CircleProfile));
            typeMap = typeMap.AddMap("Diameter", new string[] { "BHE_Diameter", "Diameter", "d", "D", "OD" });
            typeMap = typeMap.AddMap("Radius", new string[] { "BHE_Radius", "Radius", "r", "R" });
            typeMaps.Add(typeMap);

            typeMap = Create.TypeMap(typeof(TubeProfile));
            typeMap = typeMap.AddMap("Diameter", new string[] { "BHE_Diameter", "Diameter", "d", "D", "OD" });
            typeMap = typeMap.AddMap("Thickness", new string[] { "Wall Nominal Thickness", "Wall Thickness", "t", "T" });
            typeMaps.Add(typeMap);

            typeMap = Create.TypeMap(typeof(FabricatedISectionProfile));
            typeMap = typeMap.AddMap("Height", new string[] { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" });
            typeMap = typeMap.AddMap("TopFlangeWidth", new string[] { "Top Flange Width", "bt", "bf_t", "bft", "b1", "b", "B", "Bt" });
            typeMap = typeMap.AddMap("BotFlangeWidth", new string[] { "Bottom Flange Width", "bb", "bf_b", "bfb", "b2", "b", "B", "Bb" });
            typeMap = typeMap.AddMap("WebThickness", new string[] { "Web Thickness", "Stem Width", "tw", "t", "T" });
            typeMap = typeMap.AddMap("TopFlangeThickness", new string[] { "Top Flange Thickness", "tft", "tf_t", "tf", "T", "t" });
            typeMap = typeMap.AddMap("BotFlangeThickness", new string[] { "Bottom Flange Thickness", "tfb", "tf_b", "tf", "T", "t" });
            typeMap = typeMap.AddMap("WeldSize", new string[] { "k", "Weld Size" });
            typeMaps.Add(typeMap);

            typeMap = Create.TypeMap(typeof(RectangleProfile));
            typeMap = typeMap.AddMap("Height", new string[] { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" });
            typeMap = typeMap.AddMap("Width", new string[] { "b", "BHE_Width", "Width", "w", "B", "W", "bf", "D" });
            typeMap = typeMap.AddMap("CornerRadius", new string[] { "Corner Radius", "r", "r1" });
            typeMaps.Add(typeMap);

            typeMap = Create.TypeMap(typeof(AngleProfile));
            typeMap = typeMap.AddMap("Height", new string[] { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" });
            typeMap = typeMap.AddMap("Width", new string[] { "b", "BHE_Width", "Width", "w", "B", "W", "bf", "D" });
            typeMap = typeMap.AddMap("WebThickness", new string[] { "Web Thickness", "Stem Width", "tw", "t", "T" });
            typeMap = typeMap.AddMap("FlangeThickness", new string[] { "Flange Thickness", "Slab Depth", "tf", "T", "t" });
            typeMap = typeMap.AddMap("RootRadius", new string[] { "Web Fillet", "Root Radius", "r", "r1", "tr", "kr", "R1", "R", "t" });
            typeMap = typeMap.AddMap("ToeRadius", new string[] { "Flange Fillet", "Toe Radius", "r2", "R2", "t" });
            typeMaps.Add(typeMap);

            typeMap = Create.TypeMap(typeof(BoxProfile));
            typeMap = typeMap.AddMap("Height", new string[] { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" });
            typeMap = typeMap.AddMap("Width", new string[] { "b", "BHE_Width", "Width", "w", "B", "W", "bf", "D" });
            typeMap = typeMap.AddMap("Thickness", new string[] { "Wall Nominal Thickness", "Wall Thickness", "t", "T" });
            typeMap = typeMap.AddMap("OuterRadius", new string[] { "Outer Fillet", "Outer Radius", "r2", "R2", "ro", "tr" });
            typeMap = typeMap.AddMap("InnerRadius", new string[] { "Inner Fillet", "Inner Radius", "r1", "R1", "ri", "t" });
            typeMaps.Add(typeMap);

            typeMap = Create.TypeMap(typeof(ChannelProfile));
            typeMap = typeMap.AddMap("Height", new string[] { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" });
            typeMap = typeMap.AddMap("FlangeWidth", new string[] { "b", "BHE_Width", "Width", "w", "B", "W", "bf", "D" });
            typeMap = typeMap.AddMap("WebThickness", new string[] { "Web Thickness", "Stem Width", "tw", "t", "T" });
            typeMap = typeMap.AddMap("FlangeThickness", new string[] { "Flange Thickness", "Slab Depth", "tf", "T", "t" });
            typeMap = typeMap.AddMap("RootRadius", new string[] { "Web Fillet", "Root Radius", "r", "r1", "tr", "kr", "R1", "R", "t" });
            typeMap = typeMap.AddMap("ToeRadius", new string[] { "Flange Fillet", "Toe Radius", "r2", "R2", "t" });
            typeMaps.Add(typeMap);

            typeMap = Create.TypeMap(typeof(ISectionProfile));
            typeMap = typeMap.AddMap("Height", new string[] { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" });
            typeMap = typeMap.AddMap("Width", new string[] { "b", "BHE_Width", "Width", "w", "B", "W", "bf", "D" });
            typeMap = typeMap.AddMap("WebThickness", new string[] { "Web Thickness", "Stem Width", "tw", "t", "T" });
            typeMap = typeMap.AddMap("FlangeThickness", new string[] { "Flange Thickness", "Slab Depth", "tf", "T", "t" });
            typeMap = typeMap.AddMap("RootRadius", new string[] { "Web Fillet", "Root Radius", "r", "r1", "tr", "kr", "R1", "R", "t" });
            typeMap = typeMap.AddMap("ToeRadius", new string[] { "Flange Fillet", "Toe Radius", "r2", "R2", "t" });
            typeMaps.Add(typeMap);

            typeMap = Create.TypeMap(typeof(TSectionProfile));
            typeMap = typeMap.AddMap("Height", new string[] { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" });
            typeMap = typeMap.AddMap("Width", new string[] { "b", "BHE_Width", "Width", "w", "B", "W", "bf", "D" });
            typeMap = typeMap.AddMap("WebThickness", new string[] { "Web Thickness", "Stem Width", "tw", "t", "T" });
            typeMap = typeMap.AddMap("FlangeThickness", new string[] { "Flange Thickness", "Slab Depth", "tf", "T", "t" });
            typeMap = typeMap.AddMap("RootRadius", new string[] { "Web Fillet", "Root Radius", "r", "r1", "tr", "kr", "R1", "R", "t" });
            typeMap = typeMap.AddMap("ToeRadius", new string[] { "Flange Fillet", "Toe Radius", "r2", "R2", "t" });
            typeMaps.Add(typeMap);

            typeMap = Create.TypeMap(typeof(ZSectionProfile));
            typeMap = typeMap.AddMap("Height", new string[] { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" });
            typeMap = typeMap.AddMap("FlangeWidth", new string[] { "b", "BHE_Width", "Width", "w", "B", "W", "bf", "D" });
            typeMap = typeMap.AddMap("WebThickness", new string[] { "Web Thickness", "Stem Width", "tw", "t", "T" });
            typeMap = typeMap.AddMap("FlangeThickness", new string[] { "Flange Thickness", "Slab Depth", "tf", "T", "t" });
            typeMap = typeMap.AddMap("RootRadius", new string[] { "Web Fillet", "Root Radius", "r", "r1", "tr", "kr", "R1", "R", "t" });
            typeMap = typeMap.AddMap("ToeRadius", new string[] { "Flange Fillet", "Toe Radius", "r2", "R2", "t" });
            typeMaps.Add(typeMap);

            return Create.MapSettings(typeMaps);
        }

        /***************************************************/
    }
}

