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

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit.Elements;
using BHP = BH.oM.Physical.Materials;
using BHC = BH.oM.Physical.Constructions;
using BH.oM.Geometry;
using BH.oM.Geometry.CoordinateSystem;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Element ToRevit(this oM.Geometry.SettingOut.Grid grid, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitGrid(grid, document, pushSettings);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Geometry.SettingOut.Level level, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitLevel(level, document, pushSettings);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Physical.Elements.Wall wall, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitWall(wall, document, pushSettings);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Physical.Elements.Floor floor, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitFloor(floor, document, pushSettings);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Physical.Elements.Roof roof, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitRoofBase(roof, document, pushSettings);
        }

        /***************************************************/

        public static Element ToRevit(this ModelInstance modelInstance, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            switch (modelInstance.BuiltInCategory(document))
            {
                case BuiltInCategory.OST_Lines:
                    return ToCurveElement(modelInstance, document, pushSettings);
                default:
                    return ToRevitElement(modelInstance, document, pushSettings);
            }
        }

        /***************************************************/

        public static Element ToRevit(this DraftingInstance draftingInstance, Document document, PushSettings pushSettings = null)
        {
            if (draftingInstance == null)
                return null;

            pushSettings = pushSettings.DefaultIfNull();

            switch (draftingInstance.BuiltInCategory(document))
            {
                case BuiltInCategory.OST_Lines:
                    return ToCurveElement(draftingInstance, document, pushSettings);
                default:
                    return ToRevitElement(draftingInstance, document, pushSettings);
            }
        }

        /***************************************************/

        public static Element ToRevit(this BHP.Material material, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitMaterial(material, document, pushSettings);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Adapters.Revit.Elements.ViewPlan viewPlan, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitViewPlan(viewPlan, document, pushSettings);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Adapters.Revit.Elements.Viewport viewport, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitViewport(viewport, document, pushSettings);
        }

        /***************************************************/

        public static Element ToRevit(this Sheet sheet, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitViewSheet(sheet, document, pushSettings);
        }

        /***************************************************/
  
        public static Element ToRevit(this BH.oM.Environment.Elements.Space space, Document document, PushSettings pushSettings = null)
        {
            //dynamic calls needs full namespace on output to method

            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitSpace(space, document, pushSettings);
        }

        /***************************************************/

        public static Element ToRevit(this BH.oM.Physical.Elements.IFramingElement framingElement, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitFamilyInstance(framingElement, document, pushSettings);
        }

        /***************************************************/

        public static Autodesk.Revit.DB.Family ToRevit(this oM.Adapters.Revit.Elements.Family family, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitFamily(family, document, pushSettings);
        }

        /***************************************************/

        public static ElementType ToRevit(this oM.Adapters.Revit.Properties.InstanceProperties instanceProperties, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitElementType(instanceProperties, document, pushSettings);
        }

        /***************************************************/
    }
}
