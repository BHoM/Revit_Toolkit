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
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static XYZ ToRevitXYZ(this oM.Geometry.Point point, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            if (pushSettings.ConvertUnits)
                return new XYZ(UnitUtils.ConvertToInternalUnits(point.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(point.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(point.Z, DisplayUnitType.DUT_METERS));
            else
                return new XYZ(point.X, point.Y, point.Z);
        }

        /***************************************************/
    }
}