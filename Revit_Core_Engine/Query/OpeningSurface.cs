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
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static BH.oM.Geometry.ISurface OpeningSurface(this FamilyInstance familyInstance, RevitSettings settings)
        {
            if (familyInstance == null || !(familyInstance.Location is LocationPoint))
                return null;

            HostObject host = familyInstance.Host as HostObject;
            if (host == null)
                return null;

            Document doc = familyInstance.Document;
            Dictionary<BH.oM.Geometry.PlanarSurface, List<BH.oM.Geometry.PlanarSurface>> result = new Dictionary<BH.oM.Geometry.PlanarSurface, List<BH.oM.Geometry.PlanarSurface>>();

            List<Plane> planes = host.IPanelPlanes();
            if (planes.Count == 0)
                return null;

            List<ElementId> inserts = host.FindInserts(true, true, true, true).Where(x => x.IntegerValue != familyInstance.Id.IntegerValue).ToList();

            Transaction t = new Transaction(doc);
            FailureHandlingOptions failureHandlingOptions = t.GetFailureHandlingOptions().SetClearAfterRollback(true);
            t.Start("Temp Delete Inserts");

            doc.Delete(inserts);
            doc.Regenerate();

            List<Solid> solidsWithOpening = host.Solids(new Options()).Select(x => SolidUtils.Clone(x)).ToList();

            // Rollback and restart of the transaction is needed because otherwise the object, to which familyInstance is pointing can become invalidated.
            t.RollBack(failureHandlingOptions);
            t.Start();

            inserts.Add(familyInstance.Id);
            doc.Delete(inserts);
            doc.Regenerate();
            
            List<Solid> fullSolids = host.Solids(new Options()).SelectMany(x => SolidUtils.SplitVolumes(x)).ToList();
            if (host is Wall)
            {
                fullSolids = fullSolids.Select(x => BooleanOperationsUtils.CutWithHalfSpace(x, planes[0])).ToList();
                planes[0] = Plane.CreateByNormalAndOrigin(-planes[0].Normal, planes[0].Origin);
            }

            List<CurveLoop> loops = new List<CurveLoop>();
            foreach (Solid s in fullSolids)
            {
                foreach (Solid s2 in solidsWithOpening)
                {
                    BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(s, s2, BooleanOperationsType.Difference);
                }

                foreach (Face f in s.Faces)
                {
                    PlanarFace pf = f as PlanarFace;
                    if (pf == null)
                        continue;

                    if (planes.Any(x => Math.Abs(1 - pf.FaceNormal.DotProduct(x.Normal)) <= settings.DistanceTolerance && Math.Abs((pf.Origin - x.Origin).DotProduct(x.Normal)) <= settings.AngleTolerance))
                        loops.AddRange(pf.GetEdgesAsCurveLoops());
                }
            }

            t.RollBack(failureHandlingOptions);

            if (loops.Count == 0)
                return null;
            else if (loops.Count == 1)
                return new BH.oM.Geometry.PlanarSurface(loops[0].FromRevit(), null);
            else
                return new BH.oM.Geometry.PolySurface { Surfaces = new List<oM.Geometry.ISurface>(loops.Select(x => new BH.oM.Geometry.PlanarSurface(x.FromRevit(), null))) };
        }

        /***************************************************/
    }
}