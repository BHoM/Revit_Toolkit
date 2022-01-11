/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using System.ComponentModel;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates an object that carries general settings that are applicable to all actions performed by the instance of Revit adapter.")]
        [InputFromProperty("connectionSettings")]
        [InputFromProperty("familyLoadSettings")]
        [InputFromProperty("mappingSettings")]
        [InputFromProperty("distanceTolerance")]
        [InputFromProperty("angleTolerance")]
        [Output("revitSettings")]
        public static RevitSettings RevitSettings(ConnectionSettings connectionSettings = null, FamilyLoadSettings familyLoadSettings = null, MappingSettings mappingSettings = null, double distanceTolerance = BH.oM.Geometry.Tolerance.Distance, double angleTolerance = BH.oM.Geometry.Tolerance.Angle)
        {
            RevitSettings settings = new RevitSettings();

            if (connectionSettings != null)
                settings.ConnectionSettings = connectionSettings;

            if (familyLoadSettings != null)
                settings.FamilyLoadSettings = familyLoadSettings;

            if (mappingSettings != null)
                settings.MappingSettings = mappingSettings;

            if (!double.IsNaN(distanceTolerance))
                settings.DistanceTolerance = distanceTolerance;

            if (!double.IsNaN(distanceTolerance))
                settings.AngleTolerance = angleTolerance;

            return settings;
        }

        /***************************************************/
    }
}



