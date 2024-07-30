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
using System;
using System.Collections.Generic;

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
            double topOffset = viewRange.GetOffset(PlanViewPlane.TopClipPlane);
            double topZ = (topLevel == null)
                ? m_DefaultVerticalExtents
                : topLevel.ProjectElevation + topOffset;

            Level bottomLevel;
            double bottomOffset;
            if (view.ViewType == Autodesk.Revit.DB.ViewType.CeilingPlan)
            {
                bottomLevel = doc.GetElement(viewRange.GetLevelId(PlanViewPlane.CutPlane)) as Level;
                bottomOffset = viewRange.GetOffset(PlanViewPlane.CutPlane);
            }
            else
            {
                bottomLevel = doc.GetElement(viewRange.GetLevelId(PlanViewPlane.ViewDepthPlane)) as Level;
                bottomOffset = viewRange.GetOffset(PlanViewPlane.ViewDepthPlane);
            }

            double bottomZ = bottomLevel != null ? bottomLevel.ProjectElevation + bottomOffset : -m_DefaultVerticalExtents;
            return new Output<double, double> { Item1 = bottomZ, Item2 = topZ };
        }


        /***************************************************/
        /****              Private fields               ****/
        /***************************************************/

        private static double m_DefaultVerticalExtents = 1e+4;
        private static double m_DefaultHorizontalExtents = 1e+6;

        /***************************************************/
    }
}



