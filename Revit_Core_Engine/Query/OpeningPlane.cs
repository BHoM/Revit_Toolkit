/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Plane OpeningPlane(this FamilyInstance familyInstance, RevitSettings settings)
        {
            if (familyInstance == null || !(familyInstance.Location is LocationPoint))
                return null;

            HostObject host = familyInstance.Host as HostObject;
            if (host == null)
                return null;

            XYZ normal = familyInstance.GetTotalTransform().BasisY;
            Plane plane = null;
            if (host is Wall && Math.Abs(normal.DotProduct(XYZ.BasisZ)) <= settings.AngleTolerance)
            {
                Line line = (((Wall)host).Location as LocationCurve)?.Curve as Line;
                if (line == null)
                    return null;

                plane = Plane.CreateByNormalAndOrigin(normal, line.Origin);
            }
            else
            {
                if (normal.Z < 0)
                    normal = normal.Negate();

                XYZ pt = ((LocationPoint)familyInstance.Location).Point;
                foreach (Face f in host.Faces(new Options(), settings))
                {
                    PlanarFace pf = f as PlanarFace;
                    if (pf == null)
                        continue;

                    if (1 - pf.FaceNormal.DotProduct(normal) <= settings.AngleTolerance && pf.Project(pt) != null)
                    {
                        plane = Plane.CreateByNormalAndOrigin(normal, pf.Origin);
                        break;
                    }
                }
            }

            return plane;
        }

        /***************************************************/
    }
}