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

using Autodesk.Revit.DB;

using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Options Options(ViewDetailLevel detailLevel, bool includeNonVisible = false, bool computeReferences = false)
        {
            Options options = new Options();
            options.DetailLevel = detailLevel;
            options.IncludeNonVisibleObjects = includeNonVisible;
            options.ComputeReferences = computeReferences;
            return options;
        }

        /***************************************************/

        public static Options Options(View view, bool includeNonVisible = false, bool computeReferences = false)
        {
            if (view == null)
                return null;

            Options options = new Options();
            options.View = view;
            options.DetailLevel = view.DetailLevel;
            options.IncludeNonVisibleObjects = includeNonVisible;
            options.ComputeReferences = computeReferences;
            return options;
        }

        /***************************************************/
    }
}

