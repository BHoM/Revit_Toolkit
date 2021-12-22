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
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Converts BH.oM.Physical.Elements.Floor to a Revit Floor.")]
        [Input("floor", "BH.oM.Physical.Elements.Floor to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("floor", "Revit Floor resulting from converting the input BH.oM.Physical.Elements.Floor.")]
        public static Floor ToRevitFloor(this oM.Physical.Elements.Floor floor, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (floor == null || floor.Construction == null || document == null)
                return null;

            Floor revitFloor = refObjects.GetValue<Floor>(document, floor.BHoM_Guid);
            if (revitFloor != null)
                return revitFloor;

            PlanarSurface planarSurface = floor.Location as PlanarSurface;
            if (planarSurface == null)
                return null;

            settings = settings.DefaultIfNull();

            FloorType floorType = floor.ElementType(document, settings, refObjects);
            if (floorType == null)
            {
                Compute.ElementTypeNotFoundWarning(floor);
                return null;
            }

            double bottomElevation = floor.Location.IBounds().Min.Z;
            Level level = document.LevelBelow(bottomElevation.FromSI(SpecTypeId.Length), settings);

            oM.Geometry.Plane sketchPlane = new oM.Geometry.Plane { Origin = new BH.oM.Geometry.Point { Z = bottomElevation }, Normal = Vector.ZAxis };
            ICurve curve = planarSurface.ExternalBoundary.IProject(sketchPlane);
            CurveArray curveArray = Create.CurveArray(curve.IToRevitCurves());

            BH.oM.Geometry.Plane slabPlane = planarSurface.FitPlane();
            if (1 - Math.Abs(Vector.ZAxis.DotProduct(slabPlane.Normal)) <= settings.AngleTolerance)
            {
                if (floorType.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralFoundation)
                    revitFloor = document.Create.NewFoundationSlab(curveArray, floorType, level, true, XYZ.BasisZ);
                else
                    revitFloor = document.Create.NewFloor(curveArray, floorType, level, true);
            }
            else
            {
                Vector normal = slabPlane.Normal;
                if (normal.Z < 0)
                    normal = -slabPlane.Normal;

                double angle = normal.Angle(Vector.ZAxis);
                double tan = Math.Tan(angle);

                XYZ dir = normal.Project(oM.Geometry.Plane.XY).ToRevit().Normalize();
                BH.oM.Geometry.Line ln = slabPlane.PlaneIntersection(sketchPlane);
                XYZ start = ln.ClosestPoint(curveArray.get_Item(0).GetEndPoint(0).PointFromRevit(), true).ToRevit();
                Autodesk.Revit.DB.Line line = Autodesk.Revit.DB.Line.CreateBound(start, start + dir);

                revitFloor = document.Create.NewSlab(curveArray, level, line, -tan, true);
                revitFloor.SetParameter(BuiltInParameter.ELEM_TYPE_PARAM, floorType.Id);
            }

            revitFloor.CheckIfNullPush(floor);
            if (revitFloor == null)
                return null;

            document.Regenerate();

            if (planarSurface.InternalBoundaries != null)
            {
                foreach (ICurve hole in planarSurface.InternalBoundaries)
                {
                    document.Create.NewOpening(revitFloor, Create.CurveArray(hole.IProject(slabPlane).IToRevitCurves()), true);
                }
            }

            foreach (BH.oM.Physical.Elements.IOpening opening in floor.Openings)
            {
                PlanarSurface openingLocation = opening.Location as PlanarSurface;
                if (openingLocation == null)
                {
                    BH.Engine.Base.Compute.RecordWarning(String.Format("Conversion of a floor opening to Revit failed because its location is not a planar surface. Floor BHoM_Guid: {0}, Opening BHoM_Guid: {1}", floor.BHoM_Guid, opening.BHoM_Guid));
                    continue;
                }

                document.Create.NewOpening(revitFloor, Create.CurveArray(openingLocation.ExternalBoundary.IToRevitCurves()), true);

                if (!(opening is BH.oM.Physical.Elements.Void))
                    BH.Engine.Base.Compute.RecordWarning(String.Format("Revit allows only void openings in floors, therefore the BHoM opening of type {0} has been converted to a void opening. Floor BHoM_Guid: {1}, Opening BHoM_Guid: {2}", opening.GetType().Name, floor.BHoM_Guid, opening.BHoM_Guid));
            }

            double offset = revitFloor.LookupParameterDouble(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);

            // Copy parameters from BHoM object to Revit element
            revitFloor.CopyParameters(floor, settings);

            // Update the offset in case the level had been overwritten.
            if (revitFloor.LevelId.IntegerValue != level.Id.IntegerValue)
            {
                Level newLevel = document.GetElement(revitFloor.LevelId) as Level;
                offset += (level.ProjectElevation - newLevel.ProjectElevation).ToSI(SpecTypeId.Length);
            }

            revitFloor.SetParameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM, offset);

            refObjects.AddOrReplace(floor, revitFloor);
            return revitFloor;
        }

        /***************************************************/
    }
}


