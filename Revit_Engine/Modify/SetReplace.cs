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

using System.ComponentModel;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Sets Replace property for GeneralSettings stored in RevitSettings.")]
        [Input("revitSettings", "RevitSettings")]
        [Input("replace", "Replace existing elements in the model for push method. Update parameters (CustomData) only if set to false.")]
        [Output("RevitSettings")]
        public static RevitSettings SetReplace(this RevitSettings revitSettings, bool replace)
        {
            if (revitSettings == null || revitSettings.GeneralSettings == null)
                return null;

            RevitSettings aRevitSettings = revitSettings.GetShallowClone() as RevitSettings;
            aRevitSettings.GeneralSettings = aRevitSettings.GeneralSettings.GetShallowClone() as GeneralSettings;
            aRevitSettings.GeneralSettings.Replace = replace;

            return aRevitSettings;
        }

        /***************************************************/
    }
}
