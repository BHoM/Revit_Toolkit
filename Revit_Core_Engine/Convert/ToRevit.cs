/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System;
using System.Collections.Generic;
using BH.oM.Physical.Materials;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Element ToRevit(this oM.Geometry.SettingOut.Grid grid, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return grid.ToRevitGrid(document, settings, refObjects);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Geometry.SettingOut.Level level, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return level.ToRevitLevel(document, settings, refObjects);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Physical.Elements.Wall wall, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return wall.ToRevitWall(document, settings, refObjects);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Physical.Elements.Floor floor, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return floor.ToRevitFloor(document, settings, refObjects);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Physical.Elements.Roof roof, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return roof.ToRevitRoofBase(document, settings, refObjects);
        }

        /***************************************************/

        public static Element ToRevit(this ModelInstance modelInstance, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            switch (modelInstance.BuiltInCategory(document))
            {
                case BuiltInCategory.OST_Lines:
                    return modelInstance.ToCurveElement(document, settings, refObjects);
                default:
                    return modelInstance.ToRevitElement(document, settings, refObjects);
            }
        }

        /***************************************************/

        public static Element ToRevit(this DraftingInstance draftingInstance, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            switch (draftingInstance.BuiltInCategory(document))
            {
                case BuiltInCategory.OST_Lines:
                    return draftingInstance.ToCurveElement(document, settings, refObjects);
                default:
                    return draftingInstance.ToRevitElement(document, settings, refObjects);
            }
        }

        /***************************************************/

        public static Element ToRevit(this BH.oM.Physical.Materials.Material material, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return material.ToRevitMaterial(document, settings, refObjects);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Adapters.Revit.Elements.ViewPlan viewPlan, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return viewPlan.ToRevitViewPlan(document, settings, refObjects);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Adapters.Revit.Elements.Viewport viewport, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return viewport.ToRevitViewport(document, settings, refObjects);
        }

        /***************************************************/

        public static Element ToRevit(this Sheet sheet, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return sheet.ToRevitSheet(document, settings, refObjects);
        }

        /***************************************************/
  
        public static Element ToRevit(this BH.oM.Environment.Elements.Space space, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return space.ToRevitSpace(document, settings, refObjects);
        }

        /***************************************************/

        public static Element ToRevit(this BH.oM.Physical.Elements.IFramingElement framingElement, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return framingElement.IToRevitFamilyInstance(document, settings, refObjects);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Adapters.Revit.Elements.Family family, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return family.ToRevitFamily(document, settings, refObjects);
        }

        /***************************************************/

        public static Element ToRevit(this oM.Adapters.Revit.Properties.InstanceProperties instanceProperties, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return instanceProperties.ToRevitElementType(document, settings, refObjects);
        }

        /***************************************************/

        public static Element ToRevit(this BH.oM.MEP.System.Duct duct, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return duct.ToRevitDuct(document, settings, refObjects);
        }

        /***************************************************/

        public static Element ToRevit(this BH.oM.MEP.System.Pipe pipe, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return pipe.ToRevitPipe(document, settings, refObjects);
        }

        /***************************************************/

        public static Element ToRevit(this BH.oM.MEP.System.CableTray cableTray, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return cableTray.ToRevitCableTray(document, settings, refObjects);
        }

        /***************************************************/

        public static Element ToRevit(this BH.oM.Physical.Reinforcement.IReinforcingBar reinforcement, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return reinforcement.IToRevitRebar(document, settings, refObjects);
        }

        /***************************************************/
        /****             Disallowed Types              ****/
        /***************************************************/

        public static Element ToRevit(this oM.Structure.Elements.Bar bar, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            bar.ConvertBeforePushError(typeof(oM.Physical.Elements.IFramingElement));
            return null;
        }

        /***************************************************/

        public static Element ToRevit(this oM.Structure.Elements.Panel panel, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            panel.ConvertBeforePushError(typeof(oM.Physical.Elements.ISurface));
            return null;
        }

        /***************************************************/

        public static Element ToRevit(this oM.Environment.Elements.Panel panel, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            panel.ConvertBeforePushError(typeof(oM.Physical.Elements.ISurface));
            return null;
        }


        /***************************************************/
        /****             Fallback Methods              ****/
        /***************************************************/

        public static Element ToRevit(this IBHoMObject obj, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return null;
        }


        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        public static Element IToRevit(this IBHoMObject obj, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (obj == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning("Revit Element could not be created because BHoM object does not exist.");
                return null;
            }

            Element result = ToRevit(obj as dynamic, document, settings, refObjects);
            if (result == null)
                obj.NotConvertedError();

            return result;
        }

        /***************************************************/
    }
}

