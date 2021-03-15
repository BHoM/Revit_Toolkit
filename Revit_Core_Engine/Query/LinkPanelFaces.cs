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
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static List<Face> ILinkPanelFaces(this HostObject hostObject, RevitSettings settings)
        {
            return LinkPanelFaces(hostObject as dynamic, settings);
        }

        /***************************************************/

        public static List<Face> LinkPanelFaces(this Wall wall, RevitSettings settings)
        {
            Line line = (wall?.Location as LocationCurve)?.Curve as Line;
            if (line == null)
            {
                BH.Engine.Reflection.Compute.RecordError($"Querying panel surfaces from links for Revit curved walls is currently not supported.");
                return null;
            }

            XYZ normal = line.Direction.CrossProduct(XYZ.BasisZ);
            Plane p = Plane.CreateByNormalAndOrigin(normal, line.Origin);

            List<Solid> solids = wall.Solids(new Options());
            List<Solid> halfSolids = solids.Select(x => BooleanOperationsUtils.CutWithHalfSpace(x, p)).ToList();

            List<Face> result = new List<Face>();
            foreach (Solid s in halfSolids)
            {
                foreach (Face f in s.Faces)
                {
                    PlanarFace pf = f as PlanarFace;
                    if (pf == null)
                        continue;

                    if (1 - Math.Abs(pf.FaceNormal.DotProduct(normal)) > settings.AngleTolerance)
                        continue;

                    if (Math.Abs(normal.DotProduct(pf.Origin - p.Origin)) > settings.DistanceTolerance)
                        continue;

                    result.Add(pf);
                }
            }

            return result;
        }

        /***************************************************/

        public static List<Face> LinkPanelFaces(this Floor floor, RevitSettings settings)
        {
            List<Face> result = new List<Face>();
            foreach (Reference reference in HostObjectUtils.GetTopFaces(floor))
            {
                PlanarFace pf = floor.GetGeometryObjectFromReference(reference) as PlanarFace;
                if (pf != null)
                    result.Add(pf);
            }

            return result;
        }

        /***************************************************/

        public static List<Face> LinkPanelFaces(this RoofBase roof, RevitSettings settings)
        {
            List<Face> result = new List<Face>();
            foreach (Reference reference in HostObjectUtils.GetTopFaces(roof))
            {
                PlanarFace pf = roof.GetGeometryObjectFromReference(reference) as PlanarFace;
                if (pf != null)
                    result.Add(pf);
            }

            return result;
        }

        /***************************************************/

        public static List<Face> LinkPanelFaces(this Ceiling ceiling, RevitSettings settings)
        {
            List<Face> result = new List<Face>();
            foreach (Reference reference in HostObjectUtils.GetBottomFaces(ceiling))
            {
                PlanarFace pf = ceiling.GetGeometryObjectFromReference(reference) as PlanarFace;
                if (pf != null)
                    result.Add(pf);
            }

            return result;
        }


        /***************************************************/
        /****             Fallback methods              ****/
        /***************************************************/

        private static List<Face> LinkPanelFaces(this HostObject hostObject, RevitSettings settings)
        {
            BH.Engine.Reflection.Compute.RecordError($"Querying panel surfaces from links for Revit elements of type {hostObject.GetType().Name} is currently not supported.");
            return new List<Face>();
        }

        /***************************************************/
    }
}


