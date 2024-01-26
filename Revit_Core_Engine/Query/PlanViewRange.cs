/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using BH.oM.Base;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Output<double, double> PlanViewRange(this ViewPlan view)
        {
            Document doc = view.Document;

            PlanViewRange viewRange = view.GetViewRange();
            Level topLevel = doc.GetElement(viewRange.GetLevelId(PlanViewPlane.TopClipPlane)) as Level;
            Level bottomLevel = doc.GetElement(viewRange.GetLevelId(PlanViewPlane.ViewDepthPlane)) as Level;

            double topOffset = viewRange.GetOffset(PlanViewPlane.TopClipPlane);
            double bottomOffset = viewRange.GetOffset(PlanViewPlane.ViewDepthPlane);

            double topZ = (topLevel == null)
                ? m_DefaultExtents
                : topLevel.ProjectElevation + topOffset;

            double bottomZ = (bottomLevel == null)
                ? -m_DefaultExtents
                : bottomLevel.ProjectElevation + bottomOffset;

            return new Output<double, double> { Item1 = bottomZ, Item2 = topZ };
        }

        private static double m_DefaultExtents = 1e+4;

        /***************************************************/
    }
}



