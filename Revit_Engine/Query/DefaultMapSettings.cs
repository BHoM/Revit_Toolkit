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

using System.Collections.Generic;

using BH.oM.Environment.Properties;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit.Elements;


namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static MapSettings DefaultMapSettings()
        {
            List<TypeMap> aTypeMapList = new List<TypeMap>();

            TypeMap aTypeMap = Create.TypeMap(typeof(BuildingElementContextProperties));
            aTypeMap.AddMap("IsAir", "SAM_BuildingElementAir");
            aTypeMap.AddMap("Colour", "SAM_BuildingElementColour");
            //aTypeMap.AddMap("IsGround", "SAM_BuildingElementGround");
            aTypeMapList.Add(aTypeMap);


            return Create.MapSettings(aTypeMapList);

        }

        /***************************************************/
    }
}
