/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns the quality factor to be used by the meshing algorithm, depending on the Revit view detail level.")]
        [Input("viewDetailLevel", "Revit view detail level to find the meshing quality factor for.")]
        [Output("factor", "Meshing quality factor correspondent to the input Revit view detail level.")]
        public static double FaceTriangulationFactor(this ViewDetailLevel viewDetailLevel)
        {
            switch (viewDetailLevel)
            {
                case Autodesk.Revit.DB.ViewDetailLevel.Coarse:
                    return 0;
                case Autodesk.Revit.DB.ViewDetailLevel.Fine:
                    return 1;
                default:
                    return 0.5;
            }
        }

        /***************************************************/
    }
}
