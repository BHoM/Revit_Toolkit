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

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<oM.Geometry.PolyCurve> PolyCurves(this Face face, Transform transform = null, PullSettings pullSettings = null)
        {
            List<oM.Geometry.PolyCurve> aResult = new List<oM.Geometry.PolyCurve>();

            foreach(CurveLoop aCurveLoop in face.GetEdgesAsCurveLoops())
            {
                List<oM.Geometry.ICurve> aCurveList = new List<oM.Geometry.ICurve>();
                foreach (Curve aCurve in aCurveLoop)
                {
                    if (transform != null)
                        aCurveList.Add(Convert.ToBHoM(aCurve.CreateTransformed(transform), pullSettings));
                    else
                        aCurveList.Add(Convert.ToBHoM(aCurve, pullSettings));
                }
                aResult.Add(Create.PolyCurve(aCurveList));
            }

            return aResult;
        }

        /***************************************************/
    }
}