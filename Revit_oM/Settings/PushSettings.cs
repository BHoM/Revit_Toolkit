/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using System;
using System.Collections.Generic;

using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;

namespace BH.oM.Adapters.Revit.Settings
{
    public class PushSettings : BHoMObject
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        public bool CopyCustomData { get; set; } = true;
        public AdapterMode AdapterMode { get; set; } = AdapterMode.Replace;
        public FamilyLoadSettings FamilyLoadSettings { get; set; } = null;
        public MapSettings MapSettings { get; set; } = null;
        public Dictionary<Guid, List<int>> RefObjects = null;

        /***************************************************/

        public static PushSettings Default = new PushSettings();

        /***************************************************/
    }
}
