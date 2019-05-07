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
using BH.oM.Adapters.Revit.Elements;
using BHP = BH.oM.Physical.Materials;
using BHC = BH.oM.Physical.Constructions;
using BH.oM.Geometry;
using BH.oM.Geometry.CoordinateSystem;
using BH.oM.Environment.Elements;
using System.Collections.Generic;
using BH.oM.Environment.Fragments;
using BH.oM.Structure.Elements;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Element ToRevit(this oM.Architecture.Elements.Grid grid, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitGrid(grid, document, pushSettings);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Architecture.Elements.Level level, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitLevel(level, document, pushSettings);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Architecture.Elements.Wall wall, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitWall(wall, document, pushSettings);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Architecture.Elements.Floor floor, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitFloor(floor, document, pushSettings);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Architecture.Elements.Roof roof, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitRoofBase(roof, document, pushSettings);
        }

        /***************************************************/

        public static Element ToRevit(this ModelInstance modelInstance, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitElement(modelInstance, document, pushSettings);
        }

        /***************************************************/

        public static Element ToRevit(this DraftingInstance draftingInstance, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitElement(draftingInstance, document, pushSettings);
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

        public static Element ToRevit(this oM.Environment.Elements.Panel panel, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitElement(panel, document, pushSettings);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Structure.Elements.Panel panel, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitHostObject(panel, document, pushSettings);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Structure.SurfaceProperties.ISurfaceProperty surfaceProperty, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitFloorType(surfaceProperty, document, pushSettings);
        }

        /***************************************************/

        public static Element ToRevit(this FramingElement framingElement, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitFamilyInstance(framingElement, document, pushSettings);
        }

        /***************************************************/

        public static Autodesk.Revit.DB.Plane ToRevit(this Cartesian coordinateSystem, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitPlane(coordinateSystem, pushSettings);
        }

        /***************************************************/

        public static Curve ToRevit(this ICurve curve, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitCurve(curve, pushSettings);
        }

        /***************************************************/

        public static CurveArray ToRevit(this PolyCurve polyCurve, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitCurveArray(polyCurve, pushSettings);
        }

        /***************************************************/

        public static XYZ ToRevit(this oM.Geometry.Point point, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitXYZ(point, pushSettings);
        }

        /***************************************************/

        public static Autodesk.Revit.DB.Family ToRevit(this oM.Adapters.Revit.Elements.Family family, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitFamily(family, document, pushSettings);
        }

        /***************************************************/

        public static CompoundStructureLayer ToRevit(this BHC.Construction constructionLayer, Document document, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            return ToRevitCompoundStructureLayer(constructionLayer, document, pushSettings);
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