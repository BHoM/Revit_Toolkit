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
using BH.oM.Adapters.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static RevitMaterialTakeoff MaterialTakeoff(this Element element)
        {
            Dictionary<string, double> takeoff = new Dictionary<string, double>();
            double totalVolume = 0;
            foreach (ElementId materialId in element.GetMaterialIds(false))
            {
                Material material = (Material)element.Document.GetElement(materialId);
                string name = material.MaterialClass + ": " + material.Name;
                double volume = element.GetMaterialVolume(materialId).ToSI(UnitType.UT_Volume);
                if (takeoff.ContainsKey(name))
                    takeoff[name] += volume;
                else
                    takeoff[name] = volume;

                totalVolume += volume;
            }

            if (takeoff.Count != 0)
                return new RevitMaterialTakeoff(totalVolume, new oM.Physical.Materials.MaterialComposition(takeoff.Keys.Select(x => new BH.oM.Physical.Materials.Material { Name = x }), takeoff.Values.Select(x => x / totalVolume)));
            else
                return null;
        }

        /***************************************************/
    }
}
