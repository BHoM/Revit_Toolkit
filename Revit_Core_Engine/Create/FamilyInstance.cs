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
using Autodesk.Revit.DB.Structure;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using System;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static FamilyInstance FamilyInstance(Document document, FamilySymbol familySymbol, XYZ origin, Transform orientation = null, Element host = null, RevitSettings settings = null)
        {
            if (document == null || familySymbol == null || origin == null)
                return null;

            settings = settings.DefaultIfNull();

            switch (familySymbol.Family.FamilyPlacementType)
            {
                case FamilyPlacementType.OneLevelBased:
                    return Create.FamilyInstance_OneLevelBased(document, familySymbol, origin, orientation, host, settings);
                case FamilyPlacementType.OneLevelBasedHosted:
                    return Create.FamilyInstance_OneLevelBasedHosted(document, familySymbol, origin, orientation, host, settings);
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

            if (orientation?.BasisX != null && Math.Abs(orientation.BasisX.Z) > settings.AngleTolerance)
                familyInstance.ProjectedOnXYWarning();

            if (host != null)
                familyInstance.HostIgnoredWarning();

            return familyInstance;
        }

        /***************************************************/

        private static FamilyInstance FamilyInstance_OneLevelBasedHosted(Document document, FamilySymbol familySymbol, XYZ origin, Transform orientation, Element host, RevitSettings settings)
        {
            if (orientation?.BasisX == null)
                return document.Create.NewFamilyInstance(origin, familySymbol, host, StructuralType.NonStructural);
            else
            {
                FamilyInstance familyInstance = document.Create.NewFamilyInstance(origin, familySymbol, orientation.BasisX, host, StructuralType.NonStructural);

                if (1 - Math.Abs(familyInstance.GetTotalTransform().BasisY.DotProduct(orientation.BasisY.Normalize())) > settings.AngleTolerance)
                    BH.Engine.Reflection.Compute.RecordWarning($"The orientation used to create the family instance was not perpendicular to the face on which the instance was placed. The orientation out of plane has been ignored. ElementId: {familyInstance.Id.IntegerValue}");
                
                return familyInstance;
            }
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
                XYZ location = host.ClosestPointOn(origin, out reference);
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

                if (orientation?.BasisZ != null && 1 - Math.Abs(familyInstance.GetTotalTransform().BasisZ.DotProduct(orientation.BasisZ.Normalize())) > settings.AngleTolerance)
                    BH.Engine.Reflection.Compute.RecordWarning($"The orientation used to create the family instance was not perpendicular to the face on which the instance was placed. The orientation out of plane has been ignored. ElementId: {familyInstance.Id.IntegerValue}");
                
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
                familyInstance.CurveBasedNonHostedWarning();
            }
            else
            {
                Reference reference;
                Line location = host.ClosestLineOn((Line)line, out reference);
                if (location != null && reference != null)
                    familyInstance = document.Create.NewFamilyInstance(reference, location, familySymbol);
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

        private static XYZ ClosestPointOn(this Element element, XYZ refPoint, out Reference reference, XYZ normal = null, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();
            normal = normal?.Normalize();

            double minDist = double.MaxValue;
            reference = null;

            Options opt = new Options();
            opt.ComputeReferences = true;
            List<Autodesk.Revit.DB.Face> faces = element.Faces(opt);
            IntersectionResult ir;
            XYZ pointOnFace = null;

            foreach (Autodesk.Revit.DB.Face face in faces)
            {
                ir = face.Project(refPoint);
                if (ir == null || ir.XYZPoint == null)
                    continue;

                double dist = ir.Distance;
                if (normal != null)
                {
                    double normalFactor = normal.DotProduct(face.ComputeNormal(ir.UVPoint));
                    if (normalFactor < settings.AngleTolerance)
                        continue;

                    dist /= normalFactor;
                }

                if (dist < minDist)
                {
                    minDist = dist;
                    pointOnFace = ir.XYZPoint;
                    reference = face.Reference;
                }
            }

            if (pointOnFace == null || reference == null)
                return null;

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
            List<Autodesk.Revit.DB.Face> faces = element.Faces(opt);
            IntersectionResult ir1, ir2;
            XYZ start = refLine.GetEndPoint(0);
            XYZ end = refLine.GetEndPoint(1);
            XYZ startOnFace = null;
            XYZ endOnFace = null;

            foreach (Autodesk.Revit.DB.Face face in faces)
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

