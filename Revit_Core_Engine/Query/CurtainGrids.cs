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

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****            Interface methods              ****/
        /***************************************************/

        [Description("Extracts the curtain grids from a given Revit host object.")]
        [Input("hostObject", "Revit host object to extract the curtain grids from.")]
        [Output("grids", "Curtain grids extracted from the input Revit host object.")]
        public static List<CurtainGrid> ICurtainGrids(this HostObject hostObject)
        {
            return CurtainGrids(hostObject as dynamic);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the curtain grids from a given Revit wall.")]
        [Input("wall", "Revit wall to extract the curtain grids from.")]
        [Output("grids", "Curtain grids extracted from the input Revit wall.")]
        public static List<CurtainGrid> CurtainGrids(this Wall wall)
        {
            List<CurtainGrid> result = new List<CurtainGrid>();
            if (wall.CurtainGrid != null)
                result.Add(wall.CurtainGrid);

            return result;
        }

        /***************************************************/

        [Description("Extracts the curtain grids from a given Revit roof.")]
        [Input("roof", "Revit roof to extract the curtain grids from.")]
        [Output("grids", "Curtain grids extracted from the input Revit roof.")]
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
        
        [Description("Extracts the curtain grids from a given Revit roof.")]
        [Input("roof", "Revit roof to extract the curtain grids from.")]
        [Output("grids", "Curtain grids extracted from the input Revit roof.")]
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

        [Description("Extracts the curtain grids from a given Revit curtain system.")]
        [Input("curtainSystem", "Revit curtain system to extract the curtain grids from.")]
        [Output("grids", "Curtain grids extracted from the input Revit curtain system.")]
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

        [Description("Extracts the curtain grids from a given Revit host object.")]
        [Input("hostObject", "Revit host object to extract the curtain grids from.")]
        [Output("grids", "Curtain grids extracted from the input Revit host object.")]
        private static List<CurtainGrid> CurtainGrids(this HostObject hostObject)
        {
            return new List<CurtainGrid>();
        }

        /***************************************************/
    }
}


