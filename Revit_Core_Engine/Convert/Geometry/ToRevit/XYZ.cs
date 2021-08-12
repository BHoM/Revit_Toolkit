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
using BH.oM.Adapters.Revit.Settings;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static XYZ ToRevit(this oM.Geometry.Point point)
        {
            return new XYZ(point.X.FromSI(SpecTypeId.Length), point.Y.FromSI(SpecTypeId.Length), point.Z.FromSI(SpecTypeId.Length));
        }

        /***************************************************/

        public static XYZ ToRevit(this oM.Geometry.Vector vector)
        {
            return new XYZ(vector.X.FromSI(SpecTypeId.Length), vector.Y.FromSI(SpecTypeId.Length), vector.Z.FromSI(SpecTypeId.Length));
        }

        /***************************************************/
    }
}

