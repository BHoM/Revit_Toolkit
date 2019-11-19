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

        public static List<PolyCurve> PolyCurves(this Autodesk.Revit.DB.Face face, Transform transform = null, PullSettings pullSettings = null)
        {
            if (face == null)
                return null;

            List<PolyCurve> aResult = new List<PolyCurve>();

            foreach (CurveLoop aCurveLoop in face.GetEdgesAsCurveLoops())
            {
                List<ICurve> aCurveList = new List<ICurve>();
                foreach (Curve aCurve in aCurveLoop)
                {
                    ICurve aICurve = null;

                    if (transform != null)
                        aICurve = Convert.ToBHoM(aCurve.CreateTransformed(transform), pullSettings);
                    else
                        aICurve = Convert.ToBHoM(aCurve, pullSettings);

                    aCurveList.Add(aICurve);
                }
                aResult.Add(Create.PolyCurve(aCurveList));
            }

            return aResult;
        }

        /***************************************************/

        public static List<PolyCurve> PolyCurves(this Element element, IEnumerable<Reference> references, PullSettings pullSettings = null)
        {
            List<PolyCurve> aPolyCurveList = new List<PolyCurve>();

            foreach (Reference aReference in references)
            {
                Autodesk.Revit.DB.Face aFace = element.GetGeometryObjectFromReference(aReference) as Autodesk.Revit.DB.Face;
                if (aFace == null)
                    continue;

                List<PolyCurve> aPolyCurveList_Temp = null;

                if (aFace is PlanarFace)
                    aPolyCurveList_Temp = ((PlanarFace)aFace).PolyCurves(null, pullSettings);
                else
                    aPolyCurveList_Temp = aFace.Triangulate().PolyCurves(pullSettings);

                if (aPolyCurveList_Temp == null || aPolyCurveList_Temp.Count == 0)
                    continue;

                aPolyCurveList.AddRange(aPolyCurveList_Temp);
            }

            return aPolyCurveList;
        }

        /***************************************************/

        internal static List<PolyCurve> PolyCurves(this Autodesk.Revit.DB.Mesh mesh, PullSettings pullSettings = null)
        {
            if (mesh == null)
                return null;

            List<PolyCurve> aResult = new List<PolyCurve>();
            for (int i=0; i < mesh.NumTriangles; i++)
            {
                PolyCurve aPolyCurve = mesh.get_Triangle(i).PolyCurve(pullSettings);
                if (aPolyCurve != null)
                    aResult.Add(aPolyCurve);
            }
            return aResult;
        }

        /***************************************************/
    }
}