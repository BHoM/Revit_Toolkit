/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****            Interface methods              ****/
        /***************************************************/

        public static List<CurtainGrid> ICurtainGrids(this HostObject hostObject)
        {
            return CurtainGrids(hostObject as dynamic);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static List<CurtainGrid> CurtainGrids(this Wall wall)
        {
            List<CurtainGrid> result = new List<CurtainGrid>();
            if (wall.CurtainGrid != null)
                result.Add(wall.CurtainGrid);

            return result;
        }

        /***************************************************/

        public static List<CurtainGrid> CurtainGrids(this ExtrusionRoof roof)
        {
            List<CurtainGrid> result = new List<CurtainGrid>();
            if (roof.CurtainGrids != null)
            {
                foreach (CurtainGrid cg in roof.CurtainGrids)
                {
                    result.Add(cg);
                }
            }

            return result;
        }

        /***************************************************/

        public static List<CurtainGrid> CurtainGrids(this FootPrintRoof roof)
        {
            List<CurtainGrid> result = new List<CurtainGrid>();
            if (roof.CurtainGrids != null)
            {
                foreach (CurtainGrid cg in roof.CurtainGrids)
                {
                    result.Add(cg);
                }
            }

            return result;
        }

        /***************************************************/

        public static List<CurtainGrid> CurtainGrids(this CurtainSystem curtainSystem)
        {
            List<CurtainGrid> result = new List<CurtainGrid>();
            if (curtainSystem.CurtainGrids != null)
            {
                foreach (CurtainGrid cg in curtainSystem.CurtainGrids)
                {
                    result.Add(cg);
                }
            }

            return result;
        }


        /***************************************************/
        /****             Fallback methods              ****/
        /***************************************************/

        private static List<CurtainGrid> CurtainGrids(this HostObject hostObject)
        {
            return new List<CurtainGrid>();
        }

        /***************************************************/
    }
}
