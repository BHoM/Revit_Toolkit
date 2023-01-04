/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates an object that contols family loading behaviour when family is not loaded in project.")]
        [InputFromProperty("familyLibrary")]
        [InputFromProperty("overwriteFamily")]
        [InputFromProperty("overwriteParameterValues")]
        [Output("familyLoadSettings")]
        public static FamilyLoadSettings FamilyLoadSettings(FamilyLibrary familyLibrary = null, bool overwriteFamily = true, bool overwriteParameterValues = true)
        {
            FamilyLoadSettings familyLoadSettings = new FamilyLoadSettings()
            {
                OverwriteFamily = overwriteFamily,
                OverwriteParameterValues = overwriteParameterValues
            };

            if (familyLibrary != null)
                familyLoadSettings.FamilyLibrary = familyLibrary;

            return familyLoadSettings;
        }

        /***************************************************/
    }
}




