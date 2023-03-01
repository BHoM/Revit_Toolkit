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

using Autodesk.Revit.DB;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        //TODO: proper description!
#if (REVIT2018 || REVIT2019 || REVIT2020)
        public static string UnitTypeIdentifier(this UnitType unitType)
#else
        public static string UnitTypeIdentifier(this ForgeTypeId unitType)
#endif
        {
            if (m_UnitTypesWithIdentifiers == null)
                CollectUnitTypes();

            if (m_UnitTypesWithIdentifiers.ContainsValue(unitType))
                //TODO: shouldn't be .Equals for ForgeTypeId?
                return m_UnitTypesWithIdentifiers.FirstOrDefault(x => x.Value == unitType).Key;
            else
                return null;
        }

        /***************************************************/
    }
}
