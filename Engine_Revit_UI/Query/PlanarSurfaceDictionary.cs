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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Dictionary<PlanarSurface, List<oM.Physical.Elements.IOpening>> PlanarSurfaceDictionary(this HostObject hostObject, bool pullOpenings = true, RevitSettings settings = null)
        {
            List<PolyCurve> polycurves = hostObject.Profiles(settings);

            List<PolyCurve> outerPolycurves = null;
            try
            {
                outerPolycurves = polycurves.OuterPolyCurves();
            }
            catch
            {
                outerPolycurves = polycurves;
            }

            if (outerPolycurves == null)
                return null;

            List<PlanarSurface> nonPlanarSurfaces = new List<PlanarSurface>();
            List<PlanarSurface> planarSurfaces = new List<PlanarSurface>();
            foreach (PolyCurve polycurve in outerPolycurves)
            {
                List<PolyCurve> innerPolycurves = null;
                try
                {
                    innerPolycurves = BH.Engine.Adapters.Revit.Query.InnerPolyCurves(polycurve, polycurves);
                }
                catch
                {

                }

                List<ICurve> curves = new List<ICurve>();
                if (innerPolycurves != null && innerPolycurves.Count > 0)
                    curves = innerPolycurves.ConvertAll(x => (ICurve)x);

                //TODO: Create method in Geometry Engine shall be used however IsClosed method returns false for some of the PolyCurves pulled from Revit
                PlanarSurface planarSurface = new PlanarSurface(polycurve, curves);

                if (!BH.Engine.Geometry.Query.IIsPlanar(planarSurface.ExternalBoundary))
                {
                    BH.Engine.Reflection.Compute.RecordWarning("Invalid Geometry of ISurface. External Boundary of ISurface is not planar and Openings cannot be pulled.");
                    nonPlanarSurfaces.Add(planarSurface);
                }
                else
                {
                    planarSurfaces.Add(planarSurface);
                }
                    
            }

            if (planarSurfaces == null && nonPlanarSurfaces == null)
                return null;

            Dictionary<PlanarSurface, List<oM.Physical.Elements.IOpening>> result = new Dictionary<PlanarSurface, List<oM.Physical.Elements.IOpening>>();

            if(planarSurfaces != null && planarSurfaces.Count > 0)
            {
                foreach (PlanarSurface planarSurface in planarSurfaces)
                    result[planarSurface] = null;

                if(pullOpenings)
                {
                    IEnumerable<ElementId> hostedElementIDs = hostObject.FindInserts(false, false, false, false);
                    if (hostedElementIDs != null && hostedElementIDs.Count() > 0)
                    {
                        List<oM.Physical.Elements.IOpening> openings = new List<oM.Physical.Elements.IOpening>();
                        List<PolyCurve> internalPolycurves = new List<PolyCurve>(); 
                        foreach (ElementId id in hostedElementIDs)
                        {
                            FamilyInstance familyInstance = hostObject.Document.GetElement(id) as FamilyInstance;
                            if (familyInstance == null)
                                continue;

                            if (familyInstance.Category == null)
                                continue;

                            switch ((BuiltInCategory)(familyInstance.Category.Id.IntegerValue))
                            {
                                case Autodesk.Revit.DB.BuiltInCategory.OST_Windows:
                                    openings.Add(familyInstance.WindowFromRevit(settings));
                                    break;
                                case Autodesk.Revit.DB.BuiltInCategory.OST_Doors:
                                    openings.Add(familyInstance.DoorFromRevit(settings));
                                    break;
                                default:
                                    PolyCurve pcurve = familyInstance.PolyCurve(hostObject, settings);
                                    if (pcurve != null)
                                        internalPolycurves.Add(pcurve);
                                    break;
                            }
                        }

                        if (openings != null && openings.Count > 0)
                        {
                            openings.RemoveAll(x => x == null);
                            foreach (oM.Physical.Elements.IOpening opening in openings)
                            {
                                if (opening.Location == null)
                                    continue;

                                PlanarSurface planarSrf = planarSurfaces.Find(x => x.IsContaining(opening.Location as PlanarSurface));
                                if (planarSrf == null)
                                    continue;

                                List<oM.Physical.Elements.IOpening> tempOpenings = result[planarSrf];
                                if (tempOpenings == null)
                                {
                                    tempOpenings = new List<oM.Physical.Elements.IOpening>();
                                    result[planarSrf] = tempOpenings;
                                }

                                tempOpenings.Add(opening);
                            }
                        }

                        if (internalPolycurves != null && internalPolycurves.Count > 0)
                        {
                            internalPolycurves.RemoveAll(x => x == null);
                            foreach (PolyCurve pcurve in internalPolycurves)
                            {
                                PlanarSurface planarSrf = planarSurfaces.Find(x => x.IsContaining(BH.Engine.Geometry.Query.IControlPoints(pcurve)));
                                if (planarSrf == null)
                                    continue;

                                planarSrf.InternalBoundaries.Add(pcurve);
                            }
                        }
                    }
                }
            }

            foreach (PlanarSurface nonPlanarSurface in nonPlanarSurfaces)
                result[nonPlanarSurface] = null;

            return result;
        }

        /***************************************************/
    }
}
