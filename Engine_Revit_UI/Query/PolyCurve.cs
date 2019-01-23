/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static PolyCurve PolyCurve(this MeshTriangle meshTriangle, PullSettings pullSettings = null)
        {
            if (meshTriangle == null)
                return null;

            oM.Geometry.Point aPoint_1 = meshTriangle.get_Vertex(0).ToBHoM(pullSettings);
            oM.Geometry.Point aPoint_2 = meshTriangle.get_Vertex(1).ToBHoM(pullSettings);
            oM.Geometry.Point aPoint_3 = meshTriangle.get_Vertex(2).ToBHoM(pullSettings);

            return Create.PolyCurve(new ICurve[] { Create.Line(aPoint_1, aPoint_2), Create.Line(aPoint_2, aPoint_3), Create.Line(aPoint_3, aPoint_1) });
        }

        /***************************************************/

    }
}