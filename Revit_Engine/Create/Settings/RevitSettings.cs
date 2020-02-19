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

using System.ComponentModel;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates Revit Settings class which contols behaviour of Revit Adapter")]
        [Input("connectionSettings", "Connection Settings for Revit Adapter")]
        [Input("familyLoadSettings", "FamilyLoad Settings for Revit Adapter")]
        [Input("mapSettings", "Map Settings for Revit Adapter")]
        [Input("tagsParameterName", "Name of the parameter to which the tags will be assigned")]
        [Input("distanceTolerance", "Distance tolerance to be used by the adapter")]
        [Input("angleTolerance", "Angle tolerance to be used by the adapter")]
        [Output("RevitSettings")]
        public static RevitSettings RevitSettings(ConnectionSettings connectionSettings = null, FamilyLoadSettings familyLoadSettings = null, MapSettings mapSettings = null, string tagsParameterName = null, double distanceTolerance = BH.oM.Geometry.Tolerance.Distance, double angleTolerance = BH.oM.Geometry.Tolerance.Angle)
        {
            RevitSettings settings = new RevitSettings();

            if (connectionSettings != null)
                settings.ConnectionSettings = connectionSettings;

            if (familyLoadSettings != null)
                settings.FamilyLoadSettings = familyLoadSettings;

            if (mapSettings != null)
                settings.MapSettings = mapSettings;

            if (!string.IsNullOrWhiteSpace(tagsParameterName))
                settings.TagsParameterName = tagsParameterName;

            if (!double.IsNaN(distanceTolerance))
                settings.DistanceTolerance = distanceTolerance;

            if (!double.IsNaN(distanceTolerance))
                settings.AngleTolerance = angleTolerance;

            return settings;
        }

        /***************************************************/
    }
}

