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

namespace BH.oM.Adapters.Revit.Enums
{
    /***************************************************/

    [Description("Enumerator allowing choosing to which discipline (and corresponding namespace) should Revit elements be converted on pull.")]
    public enum Discipline
    {
        [Description("Default discipline to be used.")]
        Undefined,
        [Description("Elements to be converted to types from BH.oM.Environment. If no suitable conversion exists, default discipline to be used.")]
        Environmental,
        [Description("Elements to be converted to types from BH.oM.Structure. If no suitable conversion exists, default discipline to be used.")]
        Structural,
        [Description("Elements to be converted to types from BH.oM.Architecture. If no suitable conversion exists, default discipline to be used.")]
        Architecture,
        [Description("Elements to be converted to types from BH.oM.Physical. If no suitable conversion exists, default discipline to be used.")]
        Physical,
    }

    /***************************************************/
}
