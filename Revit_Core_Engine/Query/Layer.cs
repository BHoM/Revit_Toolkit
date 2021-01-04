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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static oM.Physical.Constructions.Layer Layer(this CompoundStructureLayer compoundStructureLayer, HostObjAttributes owner, string materialGrade = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (compoundStructureLayer == null)
                return null;

            settings = settings.DefaultIfNull();

            oM.Physical.Constructions.Layer layer = new oM.Physical.Constructions.Layer();
            layer.Thickness = compoundStructureLayer.Width.ToSI(UnitType.UT_Length);

            Material revitMaterial = owner.Document.GetElement(compoundStructureLayer.MaterialId) as Material;
            if (revitMaterial == null)
                revitMaterial = owner.Category.Material;

            layer.Material = revitMaterial.MaterialFromRevit(materialGrade, settings, refObjects);
            return layer;
        }

        /***************************************************/
    }
}

