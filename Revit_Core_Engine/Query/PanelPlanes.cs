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
using System;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****            Interface methods              ****/
        /***************************************************/

        public static List<Plane> IPanelPlanes(this HostObject hostObject)
        {
            return PanelPlanes(hostObject as dynamic);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static List<Plane> PanelPlanes(this Wall wall)
        {
            Line line = (wall.Location as LocationCurve)?.Curve as Line;
            if (line == null)
                return new List<Plane>();

            return new List<Plane> { Plane.CreateByNormalAndOrigin(line.Direction.CrossProduct(XYZ.BasisZ), line.Origin) };
        }

        /***************************************************/

        public static List<Plane> PanelPlanes(this Floor floor)
        {
            List<Plane> result = new List<Plane>();
            foreach (Reference reference in HostObjectUtils.GetTopFaces(floor))
            {
                PlanarFace pf = floor.GetGeometryObjectFromReference(reference) as PlanarFace;
                if (pf != null)
                    result.Add(Plane.CreateByNormalAndOrigin(pf.FaceNormal, pf.Origin));
            }

            return result;
        }

        /***************************************************/

        public static List<Plane> PanelPlanes(this RoofBase roof)
        {
            List<Plane> result = new List<Plane>();
            foreach (Reference reference in HostObjectUtils.GetTopFaces(roof))
            {
                PlanarFace pf = roof.GetGeometryObjectFromReference(reference) as PlanarFace;
                if (pf != null)
                    result.Add(Plane.CreateByNormalAndOrigin(pf.FaceNormal, pf.Origin));
            }

            return result;
        }

        /***************************************************/

        public static List<Plane> PanelPlanes(this Ceiling ceiling)
        {
            List<Plane> result = new List<Plane>();
            foreach (Reference reference in HostObjectUtils.GetBottomFaces(ceiling))
            {
                PlanarFace pf = ceiling.GetGeometryObjectFromReference(reference) as PlanarFace;
                if (pf != null)
                    result.Add(Plane.CreateByNormalAndOrigin(pf.FaceNormal, pf.Origin));
            }

            return result;
        }
        

        /***************************************************/
        /****             Fallback methods              ****/
        /***************************************************/

        private static List<Plane> PanelPlanes(this HostObject hostObject)
        {
            BH.Engine.Reflection.Compute.RecordError(String.Format("Querying panel locations for Revit elements of type {0} is currently not supported.", hostObject.GetType().Name));
            return new List<Plane>();
        }

        /***************************************************/
    }
}

