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

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        static public oM.Common.Properties.CompoundLayer CompoundLayer(this CompoundStructureLayer compoundStructureLayer, Document Document, BuiltInCategory builtInCategory = Autodesk.Revit.DB.BuiltInCategory.INVALID, PullSettings pullSettings = null)
        {
            if (compoundStructureLayer == null)
                return null;

            oM.Physical.Materials.Material aMaterial = Convert.ToBHoMMaterial(compoundStructureLayer, Document, builtInCategory, pullSettings);
            if (aMaterial == null)
                return null;

            double aThickness = compoundStructureLayer.Width;
            if (pullSettings.ConvertUnits)
                aThickness = UnitUtils.ConvertFromInternalUnits(aThickness, DisplayUnitType.DUT_METERS);

            oM.Common.Properties.CompoundLayer aCompoundLayer = new oM.Common.Properties.CompoundLayer()
            {
                //Material = aMaterial,
                Thickness = aThickness
            };

            return aCompoundLayer;
        }

        /***************************************************/

    }
}