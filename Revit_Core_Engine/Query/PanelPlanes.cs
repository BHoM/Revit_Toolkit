/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****            Interface methods              ****/
        /***************************************************/

        [Description("Extracts the BHoM-representative (centre, top or bottom, depending on element category) planes from a given Revit host object.")]
        [Input("hostObject", "Revit host object to extract the BHoM-representative planes from.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("planes", "BHoM-representative planes extracted from the input Revit host object.")]
        public static List<Plane> IPanelPlanes(this HostObject hostObject, RevitSettings settings)
        {
            return PanelPlanes(hostObject as dynamic, settings);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the BHoM-representative (centre) planes from a given Revit wall.")]
        [Input("wall", "Revit wall to extract the BHoM-representative planes from.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("planes", "BHoM-representative planes extracted from the input Revit wall.")]
        public static List<Plane> PanelPlanes(this Wall wall, RevitSettings settings)
        {
            Line line = (wall.Location as LocationCurve)?.Curve as Line;
            if (line == null)
                return new List<Plane>();

            return new List<Plane> { Plane.CreateByNormalAndOrigin(line.Direction.CrossProduct(XYZ.BasisZ), line.Origin) };
        }

        /***************************************************/

        [Description("Extracts the BHoM-representative (top) planes from a given Revit floor.")]
        [Input("floor", "Revit floor to extract the BHoM-representative planes from.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("planes", "BHoM-representative planes extracted from the input Revit floor.")]
        public static List<Plane> PanelPlanes(this Floor floor, RevitSettings settings)
        {
            settings = settings.DefaultIfNull();

            List<Plane> result = new List<Plane>();
            foreach (Reference reference in HostObjectUtils.GetTopFaces(floor))
            {
                PlanarFace pf = floor.GetGeometryObjectFromReference(reference) as PlanarFace;
                if (pf != null)
                {
                    Plane plane = Plane.CreateByNormalAndOrigin(pf.FaceNormal, pf.Origin);
                    if (result.All(x => !x.IsSimilar(plane, settings, true)))
                        result.Add(plane);
                }
                else
                    BH.Engine.Base.Compute.RecordWarning($"Some top faces of the floor are not planar, therefore they could not be queried. ElementId: {floor.Id.IntegerValue}");
            }

            return result;
        }

        /***************************************************/

        [Description("Extracts the BHoM-representative (top) planes from a given Revit roof.")]
        [Input("roof", "Revit floor to extract the BHoM-representative planes from.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("planes", "BHoM-representative planes extracted from the input Revit roof.")]
        public static List<Plane> PanelPlanes(this RoofBase roof, RevitSettings settings)
        {
            settings = settings.DefaultIfNull();

            List<Plane> result = new List<Plane>();
            foreach (Reference reference in HostObjectUtils.GetTopFaces(roof))
            {
                PlanarFace pf = roof.GetGeometryObjectFromReference(reference) as PlanarFace;
                if (pf != null)
                {
                    Plane plane = Plane.CreateByNormalAndOrigin(pf.FaceNormal, pf.Origin);
                    if (result.All(x => !x.IsSimilar(plane, settings, true)))
                        result.Add(plane);
                }
                else
                    BH.Engine.Base.Compute.RecordWarning($"Some top faces of the roof are not planar, therefore they could not be queried. ElementId: {roof.Id.IntegerValue}");
            }

            return result;
        }

        /***************************************************/

        [Description("Extracts the BHoM-representative (bottom) planes from a given Revit ceiling.")]
        [Input("ceiling", "Revit ceiling to extract the BHoM-representative planes from.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("planes", "BHoM-representative planes extracted from the input Revit ceiling.")]
        public static List<Plane> PanelPlanes(this Ceiling ceiling, RevitSettings settings)
        {
            settings = settings.DefaultIfNull();

            List<Plane> result = new List<Plane>();
            foreach (Reference reference in HostObjectUtils.GetBottomFaces(ceiling))
            {
                PlanarFace pf = ceiling.GetGeometryObjectFromReference(reference) as PlanarFace;
                if (pf != null)
                {
                    Plane plane = Plane.CreateByNormalAndOrigin(pf.FaceNormal, pf.Origin);
                    if (result.All(x => !x.IsSimilar(plane, settings, true)))
                        result.Add(plane);
                }
                else
                    BH.Engine.Base.Compute.RecordWarning($"Some top faces of the ceiling are not planar, therefore they could not be queried. ElementId: {ceiling.Id.IntegerValue}");
            }

            return result;
        }
        

        /***************************************************/
        /****             Fallback methods              ****/
        /***************************************************/

        private static List<Plane> PanelPlanes(this HostObject hostObject, RevitSettings settings)
        {
            BH.Engine.Base.Compute.RecordError(String.Format("Querying panel locations for Revit elements of type {0} is currently not supported.", hostObject.GetType().Name));
            return new List<Plane>();
        }

        /***************************************************/
    }
}



