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
using Autodesk.Revit.DB.Structure;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates a FamilyInstance with given type, location and host, in a given Revit document.")]
        [Input("document", "Document to create a FamilyInstance in.")]
        [Input("familySymbol", "Symbol (type) of family to be applied when creating the FamilyInstance.")]
        [Input("origin", "Location of the created FamilyInstance.")]
        [Input("orientation", "Orientation of the created FamilyInstance.")]
        [Input("host", "Host element of the created FamilyInstance. If null, a non-hosted element is created.")]
        [Input("settings", "Revit adapter settings to be used while creating the FamilyInstance.")]
        [Output("instance", "New FamilyInstance created in the input Revit document, based on the provided type, location and host information.")]
        public static FamilyInstance FamilyInstance(Document document, FamilySymbol familySymbol, XYZ origin, Transform orientation = null, Element host = null, RevitSettings settings = null)
        {
            if (document == null || familySymbol == null || origin == null)
                return null;

            settings = settings.DefaultIfNull();

            switch (familySymbol.Family.FamilyPlacementType)
            {
                case FamilyPlacementType.OneLevelBased:
                    if (host is MEPCurve)
                        return Create.FamilyInstance_OneLevelBased(document, familySymbol, origin, orientation, (MEPCurve)host, settings);
                    else
                        return Create.FamilyInstance_OneLevelBased(document, familySymbol, origin, orientation, host, settings);
                case FamilyPlacementType.OneLevelBasedHosted:
                    return Create.FamilyInstance_OneLevelBasedHosted(document, familySymbol, origin, orientation, host as dynamic, settings);
                case FamilyPlacementType.TwoLevelsBased:
                    return Create.FamilyInstance_TwoLevelBased(document, familySymbol, origin, orientation, host, settings);
                case FamilyPlacementType.WorkPlaneBased:
                    return Create.FamilyInstance_WorkPlaneBased(document, familySymbol, origin, orientation, host, settings);
                case FamilyPlacementType.ViewBased:
                case FamilyPlacementType.CurveBasedDetail:
                    familySymbol.FamilyPlacementTypeDraftingError();
                    return null;
                default:
                    BH.Engine.Reflection.Compute.RecordError($"Revit family placement type named {familySymbol.Family.FamilyPlacementType} does not support point locations. Revit ElementId: {familySymbol.Id.IntegerValue}");
                    return null;
            }
        }

        /***************************************************/

        [Description("Creates a FamilyInstance with given type, location and host, in a given Revit document.")]
        [Input("document", "Document to create a FamilyInstance in.")]
        [Input("familySymbol", "Symbol (type) of family to be applied when creating the FamilyInstance.")]
        [Input("curve", "Location of the created FamilyInstance.")]
        [Input("host", "Host element of the created FamilyInstance. If null, a non-hosted element is created.")]
        [Input("settings", "Revit adapter settings to be used while creating the FamilyInstance.")]
        [Output("instance", "New FamilyInstance created in the input Revit document, based on the provided type, location and host information.")]
        public static FamilyInstance FamilyInstance(Document document, FamilySymbol familySymbol, Curve curve, Element host = null, RevitSettings settings = null)
        {
            if (document == null || familySymbol == null || curve == null)
                return null;

            settings = settings.DefaultIfNull();

            switch (familySymbol.Family.FamilyPlacementType)
            {
                case FamilyPlacementType.TwoLevelsBased:
                    return Create.FamilyInstance_TwoLevelsBased(document, familySymbol, curve, host, settings);
                case FamilyPlacementType.WorkPlaneBased:
                    return Create.FamilyInstance_WorkPlaneBased(document, familySymbol, curve, host, settings);
                case FamilyPlacementType.CurveBased:
                    return Create.FamilyInstance_CurveBased(document, familySymbol, curve, host, settings);
                case FamilyPlacementType.CurveDrivenStructural:
                    return Create.FamilyInstance_CurveDrivenStructural(document, familySymbol, curve, host, settings);
                case FamilyPlacementType.ViewBased:
                case FamilyPlacementType.CurveBasedDetail:
                    familySymbol.FamilyPlacementTypeDraftingError();
                    return null;
                default:
                    BH.Engine.Reflection.Compute.RecordError($"Revit family placement type named {familySymbol.Family.FamilyPlacementType} does not support curve locations. Revit ElementId: {familySymbol.Id.IntegerValue}");
                    return null;
            }
        }

        /***************************************************/

        [Description("Creates a view-specific FamilyInstance with given type and location, in a given Revit document.")]
        [Input("document", "Document to create a FamilyInstance in.")]
        [Input("familySymbol", "Symbol (type) of family to be applied when creating the FamilyInstance.")]
        [Input("origin", "Location of the created FamilyInstance.")]
        [Input("orientation", "Orientation of the created FamilyInstance.")]
        [Input("view", "Revit view, in which the FamilyInstance will be created.")]
        [Input("settings", "Revit adapter settings to be used while creating the FamilyInstance.")]
        [Output("instance", "New FamilyInstance created in the input Revit view, based on the provided type and location information.")]
        public static FamilyInstance FamilyInstance(Document document, FamilySymbol familySymbol, XYZ origin, Transform orientation, View view, RevitSettings settings = null)
        {
            if (document == null || familySymbol == null || origin == null || view == null)
                return null;

            settings = settings.DefaultIfNull();

            if (familySymbol.Family.FamilyPlacementType == FamilyPlacementType.ViewBased)
                return Create.FamilyInstance_ViewBased(document, familySymbol, origin, orientation, view, settings);
            else
            {
                familySymbol.FamilyPlacementTypeModelError();
                return null;
            }
        }

        /***************************************************/

        [Description("Creates a view-specific FamilyInstance with given type and location, in a given Revit document.")]
        [Input("document", "Document to create a FamilyInstance in.")]
        [Input("familySymbol", "Symbol (type) of family to be applied when creating the FamilyInstance.")]
        [Input("curve", "Location of the created FamilyInstance.")]
        [Input("view", "Revit view, in which the FamilyInstance will be created.")]
        [Input("settings", "Revit adapter settings to be used while creating the FamilyInstance.")]
        [Output("instance", "New FamilyInstance created in the input Revit view, based on the provided type and location information.")]
        public static FamilyInstance FamilyInstance(Document document, FamilySymbol familySymbol, Curve curve, View view, RevitSettings settings = null)
        {
            if (document == null || familySymbol == null || curve == null || view == null)
                return null;

            settings = settings.DefaultIfNull();

            if (familySymbol.Family.FamilyPlacementType == FamilyPlacementType.CurveBasedDetail)
                return Create.FamilyInstance_CurveBasedDetail(document, familySymbol, curve, view, settings);
            else
            {
                familySymbol.FamilyPlacementTypeModelError();
                return null;
            }
        }


        /***************************************************/
        /****       Private Methods - Point Based       ****/
        /***************************************************/

        private static FamilyInstance FamilyInstance_OneLevelBased(Document document, FamilySymbol familySymbol, XYZ origin, Transform orientation, Element host, RevitSettings settings)
        {
            Level level = document.LevelBelow(origin, settings);
            if (level == null)
                return null;

            XYZ xdir = orientation.ProjectedX(settings);
            FamilyInstance familyInstance = document.Create.NewFamilyInstance(origin, familySymbol, xdir, level, StructuralType.NonStructural);
            if (familyInstance == null)
                return null;

            if (orientation?.BasisX != null && Math.Abs(orientation.BasisX.Z) > settings.AngleTolerance)
                familyInstance.ProjectedOnXYWarning();
            
            if (host != null)
                familyInstance.HostIgnoredWarning();

            return familyInstance;
        }

        /***************************************************/

        private static FamilyInstance FamilyInstance_OneLevelBased(Document document, FamilySymbol familySymbol, XYZ origin, Transform orientation, MEPCurve host, RevitSettings settings)
        {
            //Check inputs
            if (familySymbol == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Element could not be created. Family Symbol not found.");
                return null;
            }

            if (host == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Element could not be created. Host element not found.");
                return null;
            }

            LocationCurve location = host.Location as LocationCurve;
            Line line = location.Curve as Line;
            if (line == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Element could not be created. Host element is not linear.");
                return null;
            }
            
            if (line.Distance(origin) > settings.DistanceTolerance)
            {
                origin = line.Project(origin).XYZPoint;
                if (origin == null)
                {
                    BH.Engine.Reflection.Compute.RecordError("Element could not be created. Its location point could not be projected on the host MEPCurve.");
                    return null;
                }
                BH.Engine.Reflection.Compute.RecordWarning("The input location does not lie on the host MEPCurve, therefore it has been projected on the curve.");
            }
            
            //BreakCurve
            ElementId newElementId;
            if (host is Pipe)
            {
                newElementId = PlumbingUtils.BreakCurve(document, host.Id, origin);
            }
            else if (host is Duct)
            {
                newElementId = MechanicalUtils.BreakCurve(document, host.Id, origin);
            }
            else
            {
                BH.Engine.Reflection.Compute.RecordError("Element could not be created. Host element type does not support line breaking functionality.");
                return null;
            }

            //Find connectors
            Element newElement = document.GetElement(newElementId);
            MEPCurve newElementMEPCurve = newElement as MEPCurve;
            ConnectorSet hostConnSet = host.ConnectorManager.Connectors;
            ConnectorSet newElConnSet = newElementMEPCurve.ConnectorManager.Connectors;
            Connector conn1 = null;
            Connector conn2 = null;
            foreach (Connector hostConn in hostConnSet)
            {
                foreach (Connector newElConn in newElConnSet)
                {
                    if (hostConn.Origin.IsAlmostEqualTo(newElConn.Origin))
                    {
                        conn1 = hostConn;
                        conn2 = newElConn;
                        break;
                    }
                }
            }

            //Place family instance
            FamilyInstance familyInstance = document.Create.NewUnionFitting(conn1, conn2);
            if (!familyInstance.SetParameter(BuiltInParameter.ELEM_TYPE_PARAM, familySymbol.Id))
                BH.Engine.Reflection.Compute.RecordWarning("The input family type could not be assigned to the created instance, possibly because it is not applicable to the host element.");

            //Rotate
            XYZ x = orientation?.BasisZ;
            if (x != null)
            {
                Transform localOrientation = familyInstance.GetTotalTransform();

                if (Math.Abs(orientation.BasisZ.DotProduct(localOrientation.BasisX)) > settings.AngleTolerance)
                    familyInstance.NotPerpendicularOrientationWarning();

                double angle = localOrientation.BasisZ.AngleOnPlaneTo(orientation.BasisZ, localOrientation.BasisX);
                if (Math.Abs(angle) > settings.AngleTolerance)
                {
                    Line dir = Line.CreateBound(origin, origin + localOrientation.BasisX);
                    ElementTransformUtils.RotateElement(document, familyInstance.Id, dir, angle);
                    document.Regenerate();
                }    
            }

            return familyInstance;
        }

        /***************************************************/

        private static FamilyInstance FamilyInstance_OneLevelBasedHosted(Document document, FamilySymbol familySymbol, XYZ origin, Transform orientation, Element host, RevitSettings settings)
        {
            if (host == null)
            {
                BH.Engine.Reflection.Compute.RecordError($"Element could not be created: instantiation of families with placement type of {familySymbol.Family.FamilyPlacementType} requires a host. ElementId: {familySymbol.Id.IntegerValue}");
                return null;
            }

            FamilyInstance familyInstance;
            Level hostLevel = host.Document.GetElement(host.LevelId) as Level;
            if (hostLevel != null)
                familyInstance = document.Create.NewFamilyInstance(origin, familySymbol, host, hostLevel, StructuralType.NonStructural);
            else
                familyInstance = document.Create.NewFamilyInstance(origin, familySymbol, host, StructuralType.NonStructural);

            if (familyInstance == null)
                return null;

            document.Regenerate();
            
            if (orientation?.BasisX != null)
            {
                Transform localOrientation = familyInstance.GetTotalTransform();
                XYZ normal;
                if (familySymbol.Family.LookupParameterString(BuiltInParameter.FAMILY_HOSTING_BEHAVIOR) == "Wall")
                    normal = localOrientation.BasisY;
                else
                    normal = localOrientation.BasisZ;

                if (Math.Abs(normal.DotProduct(orientation.BasisX)) > settings.AngleTolerance)
                    BH.Engine.Reflection.Compute.RecordWarning($"The orientation used to create the family instance was not perpendicular to the face on which the instance was placed. The orientation out of plane has been ignored. ElementId: {familyInstance.Id.IntegerValue}");

                double angle = localOrientation.BasisX.AngleOnPlaneTo(orientation.BasisX, normal);
                if (Math.Abs(angle) > settings.AngleTolerance)
                {
                    Line dir = Line.CreateBound(origin, origin + normal);
                    ElementTransformUtils.RotateElement(document, familyInstance.Id, dir, angle);
                }
            }
            
            XYZ location = (familyInstance.Location as LocationPoint)?.Point;
            if (location != null && location.DistanceTo(origin) > settings.DistanceTolerance)
            {
                List<Solid> hostSolids = host.Solids(new Options());
                if (hostSolids == null && hostSolids.All(x => !origin.IsInside(x, settings.DistanceTolerance)))
                    BH.Engine.Reflection.Compute.RecordWarning($"The input location point for creation of a family instance was outside of the host solid, the point has been snapped to the host. ElementId: {familyInstance.Id.IntegerValue}");
            }

            return familyInstance;
        }

        /***************************************************/

        private static FamilyInstance FamilyInstance_TwoLevelBased(Document document, FamilySymbol familySymbol, XYZ origin, Transform orientation, Element host, RevitSettings settings)
        {
            Level level = document.LevelBelow(origin, settings);
            if (level == null)
                return null;

            BuiltInCategory builtInCategory = (BuiltInCategory)familySymbol.Category.Id.IntegerValue;
            StructuralType structuralType = builtInCategory.StructuralType();

            FamilyInstance familyInstance = document.Create.NewFamilyInstance(origin, familySymbol, level, structuralType);
            if (familyInstance == null)
                return null;

            familyInstance.TwoLevelBasedByPointWarning();

            XYZ x = orientation?.BasisX;
            if (x != null)
            {
                if (Math.Abs(x.Z) > settings.AngleTolerance)
                    familyInstance.ProjectedOnXYWarning();

                XYZ xdir = orientation.ProjectedX(settings);
                double angle = XYZ.BasisX.AngleOnPlaneTo(xdir, XYZ.BasisZ);
                if (Math.Abs(angle) > settings.AngleTolerance)
                {
                    Line dir = Line.CreateBound(origin, origin + XYZ.BasisZ);
                    ElementTransformUtils.RotateElement(document, familyInstance.Id, dir, angle);
                    document.Regenerate();
                }
            }

            if (host != null)
                familyInstance.HostIgnoredWarning();

            return familyInstance;
        }

        /***************************************************/

        private static FamilyInstance FamilyInstance_WorkPlaneBased(Document document, FamilySymbol familySymbol, XYZ origin, Transform orientation, Element host, RevitSettings settings)
        {
            if (host != null)
            {
                Reference reference;
                bool closest;
                XYZ location = host.ClosestPointOn(origin, out reference, out closest, orientation?.BasisZ, settings);
                if (location == null || reference == null)
                    return null;

                XYZ refDir = XYZ.BasisX;
                if (orientation?.BasisX != null)
                {
                    refDir = orientation.BasisX;
                    if (orientation.BasisZ.DotProduct(orientation.BasisX.CrossProduct(orientation.BasisY)) < 0)
                        refDir = -refDir;
                }

                FamilyInstance familyInstance = document.Create.NewFamilyInstance(reference, location, refDir, familySymbol);
                if (familyInstance == null)
                    return null;

                document.Regenerate();
                if (orientation?.BasisZ != null && 1 - Math.Abs(familyInstance.GetTotalTransform().BasisZ.DotProduct(orientation.BasisZ.Normalize())) > settings.AngleTolerance)
                    BH.Engine.Reflection.Compute.RecordWarning($"The orientation used to create the family instance was not perpendicular to the face on which the instance was placed. The orientation out of plane has been ignored. ElementId: {familyInstance.Id.IntegerValue}");

                if (!closest)
                    BH.Engine.Reflection.Compute.RecordWarning($"The location point of the created family instance has been snapped to the closest host element's face with normal matching the local Z of the BHoM object's orientation, skipping closer faces with non-matching normals. ElementId: {familyInstance.Id.IntegerValue}");
                else if (origin.DistanceTo(location) > settings.DistanceTolerance)
                    BH.Engine.Reflection.Compute.RecordWarning($"The location point of the created family instance has been snapped to the closest face of the host element. ElementId: {familyInstance.Id.IntegerValue}");

                return familyInstance;
            }
            else
            {
                Reference reference;
                if (orientation?.BasisZ != null && 1 - Math.Abs(orientation.BasisZ.DotProduct(XYZ.BasisZ)) > settings.AngleTolerance)
                {
                    ReferencePlane rp = Create.ReferencePlane(document, origin, orientation);
                    reference = rp?.GetReference();
                }
                else
                    reference = document.LevelBelow(origin, settings).GetPlaneReference();

                XYZ refDir = XYZ.BasisX;
                if (orientation?.BasisX != null)
                {
                    refDir = orientation.BasisX;
                    if (orientation.BasisZ.DotProduct(orientation.BasisX.CrossProduct(orientation.BasisY)) < 0)
                        refDir = -refDir;
                }

                FamilyInstance familyInstance = document.Create.NewFamilyInstance(reference, origin, refDir, familySymbol);
                if (familyInstance == null)
                    return null;

                if (document.GetElement(reference.ElementId) is ReferencePlane)
                    familyInstance.ReferencePlaneCreatedWarning();

                return familyInstance;
            }
        }
        

        /***************************************************/
        /****       Private Methods - Curve Based       ****/
        /***************************************************/

        private static FamilyInstance FamilyInstance_TwoLevelsBased(Document document, FamilySymbol familySymbol, Curve curve, Element host, RevitSettings settings)
        {
            Line line = curve as Line;
            if (line == null)
            {
                familySymbol.LinearOnlyError();
                return null;
            }

            if (line.GetEndPoint(0).Z > line.GetEndPoint(1).Z)
            {
                familySymbol.InvalidTwoLevelLocationError();
                return null;
            }

            Level level = document.LevelBelow(line.GetEndPoint(0), settings);
            if (level == null)
                return null;

            BuiltInCategory builtInCategory = (BuiltInCategory)familySymbol.Category.Id.IntegerValue;
            StructuralType structuralType = builtInCategory.StructuralType();

            FamilyInstance familyInstance = document.Create.NewFamilyInstance(line, familySymbol, level, structuralType);
            if (familyInstance == null)
                return null;

            if (host != null)
                familyInstance.HostIgnoredWarning();

            return familyInstance;
        }

        /***************************************************/

        private static FamilyInstance FamilyInstance_WorkPlaneBased(Document document, FamilySymbol familySymbol, Curve curve, Element host, RevitSettings settings)
        {
            Line line = curve as Line;
            if (line == null)
            {
                familySymbol.LinearOnlyError();
                return null;
            }

            Line location;
            Reference reference;
            if (host != null)
                location = host.ClosestLineOn(line, out reference);
            else
            {
                location = line;

                Level level = document.LevelBelow(line, settings);
                if (level == null)
                    return null;

                reference = level.GetPlaneReference();
            }

            if (location == null || reference == null)
                return null;

            FamilyInstance familyInstance = document.Create.NewFamilyInstance(reference, location, familySymbol);
            if (familyInstance == null)
                return null;

            if (host == null)
                familyInstance.CurveBasedNonHostedWarning();
            else
                familyInstance.CurveBasedHostedWarning();

            return familyInstance;
        }

        /***************************************************/

        private static FamilyInstance FamilyInstance_CurveBased(Document document, FamilySymbol familySymbol, Curve curve, Element host, RevitSettings settings)
        {
            Line line = curve as Line;
            if (line == null)
            {
                familySymbol.LinearOnlyError();
                return null;
            }

            Level level = document.LevelBelow(line, settings);
            if (level == null)
                return null;

            FamilyInstance familyInstance = null;
            if (host == null)
            {
                familyInstance = document.Create.NewFamilyInstance(line, familySymbol, level, StructuralType.NonStructural);
                if (familyInstance == null)
                    return null;

                familyInstance.CurveBasedNonHostedWarning();
            }
            else
            {
                Reference reference;
                Line location = host.ClosestLineOn(line, out reference);
                if (location != null && reference != null)
                    familyInstance = document.Create.NewFamilyInstance(reference, location, familySymbol);

                if (familyInstance != null && location.GetEndPoint(0).DistanceTo(line.GetEndPoint(0)) > settings.DistanceTolerance || location.GetEndPoint(1).DistanceTo(line.GetEndPoint(1)) > settings.DistanceTolerance)
                    BH.Engine.Reflection.Compute.RecordWarning($"The location line of the created family instance has been snapped to the closest face of the host element. ElementId: {familyInstance.Id.IntegerValue}");
            }

            return familyInstance;
        }

        /***************************************************/

        private static FamilyInstance FamilyInstance_CurveDrivenStructural(Document document, FamilySymbol familySymbol, Curve curve, Element host, RevitSettings settings)
        {
            Level level = document.LevelBelow(curve, settings);
            if (level == null)
                return null;

            BuiltInCategory builtInCategory = (BuiltInCategory)familySymbol.Category.Id.IntegerValue;
            StructuralType structuralType = builtInCategory.StructuralType();
            
            FamilyInstance familyInstance = document.Create.NewFamilyInstance(curve, familySymbol, level, structuralType);
            if (familyInstance == null)
                return null;

            if (host != null)
                familyInstance.HostIgnoredWarning();

            return familyInstance;
        }


        /***************************************************/
        /****       Private Methods - View Based        ****/
        /***************************************************/

        private static FamilyInstance FamilyInstance_ViewBased(Document document, FamilySymbol familySymbol, XYZ origin, Transform orientation, View view, RevitSettings settings)
        {
            FamilyInstance familyInstance = view.Document.Create.NewFamilyInstance(origin, familySymbol, view);
            if (familyInstance == null)
                return null;

            if (orientation?.BasisZ != null)
            {
                if (1 - Math.Abs(view.ViewDirection.DotProduct(orientation.BasisZ)) > settings.AngleTolerance)
                    familyInstance.ProjectedOnViewPlaneWarning();
                
                double angle;
                if (1 - Math.Abs(view.ViewDirection.DotProduct(orientation.BasisX)) > settings.AngleTolerance)
                {
                    XYZ xDir = orientation.BasisY.CrossProduct(view.ViewDirection);
                    angle = view.RightDirection.AngleOnPlaneTo(xDir, view.ViewDirection);
                }
                else
                {
                    XYZ yDir = view.ViewDirection.CrossProduct(orientation.BasisX);
                    angle = view.UpDirection.AngleOnPlaneTo(yDir, view.ViewDirection);
                }

                Line dir = Line.CreateBound(origin, origin + view.ViewDirection);
                ElementTransformUtils.RotateElement(view.Document, familyInstance.Id, dir, angle);
            }

            return familyInstance;
        }

        /***************************************************/

        private static FamilyInstance FamilyInstance_CurveBasedDetail(Document document, FamilySymbol familySymbol, Curve curve, View view, RevitSettings settings)
        {
            Line line = curve as Line;
            if (line == null)
            {
                familySymbol.LinearOnlyError();
                return null;
            }

            return view.Document.Create.NewFamilyInstance(line, familySymbol, view);
        }


        /***************************************************/
        /****          Private Methods - Others         ****/
        /***************************************************/

        private static XYZ ClosestPointOn(this Element element, XYZ refPoint, out Reference reference, out bool closest, XYZ normal = null, RevitSettings settings = null)
        {
            closest = true;
            settings = settings.DefaultIfNull();
            normal = normal?.Normalize();

            double minFactoredDist = double.MaxValue;
            double minDist = double.MaxValue;
            reference = null;

            Options opt = new Options();
            opt.ComputeReferences = true;
            List<Face> faces = element.Faces(opt);
            IntersectionResult ir;
            XYZ pointOnFace = null;

            foreach (Face face in faces)
            {
                ir = face.Project(refPoint);
                if (ir == null || ir.XYZPoint == null)
                    continue;

                if (ir.Distance < minDist)
                    minDist = ir.Distance;

                double factoredDist = ir.Distance;
                if (normal != null)
                {
                    double normalFactor = normal.DotProduct(face.ComputeNormal(ir.UVPoint));
                    if (normalFactor < settings.AngleTolerance)
                        continue;

                    factoredDist /= normalFactor;
                }

                if (factoredDist < minFactoredDist)
                {
                    minFactoredDist = factoredDist;
                    pointOnFace = ir.XYZPoint;
                    reference = face.Reference;
                }
            }

            if (pointOnFace == null || reference == null)
                return null;

            closest = !(refPoint.DistanceTo(pointOnFace) - minDist > settings.DistanceTolerance);

            if (element is FamilyInstance && !((FamilyInstance)element).HasModifiedGeometry())
            {
                string instRef = ((FamilyInstance)element).UniqueId + ":0:INSTANCE:" + reference.ConvertToStableRepresentation(element.Document);
                reference = Reference.ParseFromStableRepresentation(element.Document, instRef);
            }

            return pointOnFace;
        }

        /***************************************************/

        private static Line ClosestLineOn(this Element element, Line refLine, out Reference reference)
        {
            double minDist = double.MaxValue;
            reference = null;

            Options opt = new Options();
            opt.ComputeReferences = true;
            List<Face> faces = element.Faces(opt);
            IntersectionResult ir1, ir2;
            XYZ start = refLine.GetEndPoint(0);
            XYZ end = refLine.GetEndPoint(1);
            XYZ startOnFace = null;
            XYZ endOnFace = null;

            foreach (Face face in faces)
            {
                if (!(face is PlanarFace))
                    continue;

                ir1 = face.Project(start);
                ir2 = face.Project(end);
                if (ir1?.XYZPoint == null || ir2?.XYZPoint == null)
                    continue;

                double totDist = ir1.Distance + ir2.Distance;
                if (totDist < minDist)
                {
                    minDist = totDist;
                    startOnFace = ir1.XYZPoint;
                    endOnFace = ir2.XYZPoint;
                    reference = face.Reference;
                }
            }

            if (startOnFace == null || endOnFace == null || reference == null)
                return null;

            if (element is FamilyInstance && !((FamilyInstance)element).HasModifiedGeometry())
            {
                string instRef = ((FamilyInstance)element).UniqueId + ":0:INSTANCE:" + reference.ConvertToStableRepresentation(element.Document);
                reference = Reference.ParseFromStableRepresentation(element.Document, instRef);
            }

            return Line.CreateBound(startOnFace, endOnFace);
        }

        /***************************************************/

        private static XYZ ProjectedX(this Transform basis, RevitSettings settings)
        {
            XYZ x = basis?.BasisX;
            if (x == null)
                return XYZ.BasisX;

            XYZ result = new XYZ(x.X, x.Y, 0);
            if (result.GetLength() < settings.DistanceTolerance)
                result = basis.BasisY.CrossProduct(basis.BasisZ);

            return result;
        }

        /***************************************************/
    }
}


