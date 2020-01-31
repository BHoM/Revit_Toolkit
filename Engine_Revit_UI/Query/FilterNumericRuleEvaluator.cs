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

using Autodesk.Revit.DB;
using System;
using BH.oM.Adapters.Revit.Generic;
using BH.oM.Adapters.Revit.Enums;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static FilterNumericRuleEvaluator FilterNumericRuleEvaluator(this NumberComparisonType numberComparisonType)
        {
            switch (numberComparisonType)
            {
                case NumberComparisonType.Equal:
                    return new FilterNumericEquals();
                case NumberComparisonType.Greater:
                    return new FilterNumericGreater();
                case NumberComparisonType.GreaterOrEqual:
                    return new FilterNumericGreaterOrEqual();
                case NumberComparisonType.Less:
                    return new FilterNumericLess();
                case NumberComparisonType.LessOrEqual:
                    return new FilterNumericLessOrEqual();
                case NumberComparisonType.NotEqual:
                    return new FilterNumericEquals();
                default:
                    return null;
            }
        }

        /***************************************************/
    }
}
