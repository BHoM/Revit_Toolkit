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
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static Element ToRevitElement(this ModelInstance modelInstance, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (modelInstance == null || document == null)
                return null;

            Element element = refObjects.GetValue<Element>(document, modelInstance.BHoM_Guid);
            if (element != null)
                return element;

            if (modelInstance.Properties == null)
            {
                Compute.NullObjectPropertiesWarning(modelInstance);
                return null;
            }

            settings = settings.DefaultIfNull();

            BuiltInCategory builtInCategory = modelInstance.BuiltInCategory(document);

            if (modelInstance.Location is ISurface || modelInstance.Location is ISolid)
            {
                Solid brep = ToRevit(modelInstance.Location as dynamic);
                if (brep == null)
                {
                    Compute.GeometryConvertFailed(modelInstance);
                    return null;
                }

                DirectShape directShape = DirectShape.CreateElement(document, new ElementId((int)builtInCategory));
                directShape.AppendShape(new List<GeometryObject> { brep });
                element = directShape;
            }
            else
            {
                ElementType elementType = modelInstance.Properties.ElementType(document, new List<BuiltInCategory> { builtInCategory }, settings);
                element = modelInstance.IToRevitElement(elementType, settings);
            }
            
            if (element == null)
                return null;

            // Copy parameters from BHoM object to Revit element
            element.CopyParameters(modelInstance, settings);

            refObjects.AddOrReplace(modelInstance, element);
            return element;
        }

        /***************************************************/

        public static Element ToRevitElement(this DraftingInstance draftingInstance, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (draftingInstance == null || string.IsNullOrWhiteSpace(draftingInstance.ViewName) || document == null)
                return null;

            Element element = refObjects.GetValue<Element>(document, draftingInstance.BHoM_Guid);
            if (element != null)
                return element;

            if (draftingInstance.Properties == null)
            {
                Compute.NullObjectPropertiesWarning(draftingInstance);
                return null;
            }

            settings = settings.DefaultIfNull();

            View view = draftingInstance.View(document);
            if (view == null)
                return null;

            ElementType elementType = draftingInstance.Properties.ElementType(document, draftingInstance.BuiltInCategories(document), settings);
            element = draftingInstance.IToRevitElement(elementType, view, settings);
            
            if (element == null)
                return null;

            // Copy parameters from BHoM object to Revit element
            element.CopyParameters(draftingInstance, settings);

            refObjects.AddOrReplace(draftingInstance, element);
            return element;
        }


        /***************************************************/
        /****     Private Methods - Model Instances     ****/
        /***************************************************/

        private static Element IToRevitElement(this ModelInstance modelInstance, ElementType elementType, RevitSettings settings)
        {
            if (elementType == null)
            {
                Compute.ElementTypeNotFoundWarning(modelInstance);
                return null;
            }

            return ToRevitElement(modelInstance, elementType as dynamic, settings);
        }

        /***************************************************/

        private static Element ToRevitElement(this ModelInstance modelInstance, WallType wallType, RevitSettings settings)
        {
            if (wallType == null || modelInstance == null)
                return null;

            if (!(modelInstance.Location is ICurve))
            {
                Compute.InvalidFamilyPlacementTypeError(modelInstance, wallType);
                return null;
            }

            Document document = wallType.Document;

            ICurve curve = (ICurve)modelInstance.Location;

            Level level = document.LevelBelow(curve, settings);
            if (level == null)
                return null;

            Curve revitCurve = curve.IToRevit();
            return Wall.Create(document, revitCurve, level.Id, false);
        }

        /***************************************************/

        private static Element ToRevitElement(this ModelInstance modelInstance, MEPCurveType mEPType, RevitSettings settings)
        {
            if (mEPType == null || modelInstance == null)
                return null;

            if (!(modelInstance.Location is ICurve))
            {
                Compute.InvalidFamilyPlacementTypeError(modelInstance, mEPType);
                return null;
            }

            Document document = mEPType.Document;

            BH.oM.Geometry.Line line = modelInstance.Location as BH.oM.Geometry.Line;

            Level level = document.LevelBelow(line, settings);
            if (level == null)
                return null;

            Autodesk.Revit.DB.Line revitLine = line.ToRevit();
            if (revitLine == null)
                return null;

            XYZ startPoint = revitLine.GetEndPoint(0);
            XYZ endPoint = revitLine.GetEndPoint(1);

            if (mEPType is Autodesk.Revit.DB.Electrical.CableTrayType)
                return Autodesk.Revit.DB.Electrical.CableTray.Create(document, mEPType.Id, startPoint, endPoint, level.Id);
            else if (mEPType is Autodesk.Revit.DB.Electrical.ConduitType)
                return Autodesk.Revit.DB.Electrical.Conduit.Create(document, mEPType.Id, startPoint, endPoint, level.Id);
            else if (mEPType is Autodesk.Revit.DB.Plumbing.PipeType)
            {
                Autodesk.Revit.DB.Plumbing.PipingSystemType pst = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Plumbing.PipingSystemType)).OfType<Autodesk.Revit.DB.Plumbing.PipingSystemType>().FirstOrDefault();
                return Autodesk.Revit.DB.Plumbing.Pipe.Create(document, pst.Id, mEPType.Id, level.Id, startPoint, endPoint);
            }
            else if (mEPType is Autodesk.Revit.DB.Mechanical.DuctType)
            {
                Autodesk.Revit.DB.Mechanical.MechanicalSystemType mst = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Mechanical.MechanicalSystemType)).OfType<Autodesk.Revit.DB.Mechanical.MechanicalSystemType>().FirstOrDefault();
                return Autodesk.Revit.DB.Mechanical.Duct.Create(document, mst.Id, mEPType.Id, level.Id, startPoint, endPoint);
            }
            else
                return null;
        }

        /***************************************************/

        private static Element ToRevitElement(this ModelInstance modelInstance, FamilySymbol familySymbol, RevitSettings settings)
        {
            switch (familySymbol.Family.FamilyPlacementType)
            {
                case FamilyPlacementType.OneLevelBased:
                case FamilyPlacementType.OneLevelBasedHosted:
                    return modelInstance.ToRevitElement_OneLevelBased(familySymbol, settings);
                case FamilyPlacementType.TwoLevelsBased:
                    return modelInstance.ToRevitElement_TwoLevelsBased(familySymbol, settings);
                case FamilyPlacementType.WorkPlaneBased:
                    return modelInstance.ToRevitElement_WorkPlaneBased(familySymbol, settings);
                case FamilyPlacementType.CurveBased:
                    return modelInstance.ToRevitElement_CurveBased(familySymbol, settings);
                case FamilyPlacementType.CurveDrivenStructural:
                    return modelInstance.ToRevitElement_CurveDrivenStructural(familySymbol, settings);
                case FamilyPlacementType.ViewBased:
                case FamilyPlacementType.CurveBasedDetail:
                    Compute.FamilyPlacementTypeDraftingError(modelInstance, familySymbol);
                    return null;
                default:
                    Compute.FamilyPlacementTypeNotSupportedError(modelInstance, familySymbol);
                    return null;
            }
        }

        /***************************************************/

        private static Element ToRevitElement_CurveBased(this ModelInstance modelInstance, FamilySymbol familySymbol, RevitSettings settings)
        {
            if (familySymbol == null || modelInstance == null)
                return null;

            if (!(modelInstance.Location is ICurve))
            {
                Compute.InvalidFamilyPlacementTypeError(modelInstance, familySymbol);
                return null;
            }

            Document document = familySymbol.Document;

            ICurve curve = (ICurve)modelInstance.Location;

            Level level = familySymbol.Document.LevelBelow(curve, settings);
            if (level == null)
                return null;

            Curve revitCurve = curve.IToRevit();
            if (revitCurve == null)
                return null;

            FamilyInstance familyInstance = null;
            if (modelInstance.Host == -1)
                familyInstance = document.Create.NewFamilyInstance(revitCurve, familySymbol, level, StructuralType.NonStructural);
            else
            {
                if (revitCurve is Autodesk.Revit.DB.Line)
                {
                    Element host = document.GetElement(new ElementId(modelInstance.Host));
                    if (host != null)
                    {
                        Reference reference;
                        Autodesk.Revit.DB.Line location = host.ClosestLineOn((Autodesk.Revit.DB.Line)revitCurve, out reference);
                        if (location != null && reference != null)
                            familyInstance = document.Create.NewFamilyInstance(reference, location, familySymbol);
                    }
                }
                else
                {
                    Compute.InvalidFamilyPlacementTypeError(modelInstance, familySymbol);
                    return null;
                }
            }

            return familyInstance;
        }

        /***************************************************/

        private static Element ToRevitElement_OneLevelBased(this ModelInstance modelInstance, FamilySymbol familySymbol, RevitSettings settings)
        {
            if (familySymbol == null || modelInstance == null)
                return null;

            if (!(modelInstance.Location is oM.Geometry.Point))
            {
                Compute.InvalidFamilyPlacementTypeError(modelInstance, familySymbol);
                return null;
            }

            Document document = familySymbol.Document;

            oM.Geometry.Point point = (oM.Geometry.Point)modelInstance.Location;
            XYZ xyz = point.ToRevit();
            FamilyInstance familyInstance = null;
            if (modelInstance.Host == -1)
            {
                Level level = document.LevelBelow(point, settings);
                if (level == null)
                    return null;

                //bool projected = false;
                //XYZ xdir = XYZ.BasisX;
                //if (modelInstance.Orientation?.X != null)
                //{
                //    Vector x = modelInstance.Orientation.X;
                //    if (Math.Abs(x.Z) > settings.AngleTolerance)
                //        projected = true;

                //    Vector newX = new Vector { X = x.X, Y = x.Y };
                //    if (newX.Length() > settings.DistanceTolerance)
                //        xdir = newX.ToRevit();
                //}

                XYZ xdir = modelInstance.Orientation.ProjectedX(settings).ToRevit();
                
                familyInstance = document.Create.NewFamilyInstance(xyz, familySymbol, xdir, level, StructuralType.NonStructural);

                if (modelInstance.Orientation?.X != null && Math.Abs(modelInstance.Orientation.X.Z) > settings.AngleTolerance)
                    modelInstance.ProjectedOnXYWarning(familyInstance);

                //document.Regenerate();
                //XYZ location = ((LocationPoint)familyInstance.Location).Point;
                //if (Math.Abs(location.Z - xyz.Z) > settings.DistanceTolerance)
                //{
                //    ElementTransformUtils.MoveElement(document, familyInstance.Id, xyz - location);
                //    document.Regenerate();
                //}

                //Basis orientation = modelInstance.Orientation;
                //if (orientation != null && 1 - orientation.Z.DotProduct(Vector.ZAxis) > settings.AngleTolerance)
                //    modelInstance.ProjectedOnXYWarning(familyInstance);

                //orientation = orientation.ProjectOnXY(settings);
                //familyInstance.Orient(orientation, settings);
            }
            else
            {
                Element host = document.GetElement(new ElementId(modelInstance.Host));
                if (host != null)
                {
                    if (modelInstance.Orientation?.X == null)
                        familyInstance = document.Create.NewFamilyInstance(xyz, familySymbol, host, StructuralType.NonStructural);
                    else
                    {
                        XYZ refDir = modelInstance.Orientation.X.ToRevit();
                        familyInstance = document.Create.NewFamilyInstance(xyz, familySymbol, refDir, host, StructuralType.NonStructural);
                    }
                }
                else
                {
                    //TODO: host not found error - relevant to other methods, make em uniform!
                }
            }

            return familyInstance;
        }

        /***************************************************/

        private static Element ToRevitElement_CurveDrivenStructural(this ModelInstance modelInstance, FamilySymbol familySymbol, RevitSettings settings)
        {
            if (familySymbol == null || modelInstance == null)
                return null;

            if (!(modelInstance.Location is ICurve))
            {
                Compute.InvalidFamilyPlacementTypeError(modelInstance, familySymbol);
                return null;
            }

            StructuralType structuralType = StructuralType.NonStructural;

            BuiltInCategory builtInCategory = (BuiltInCategory)familySymbol.Category.Id.IntegerValue;
            switch (builtInCategory)
            {
                case BuiltInCategory.OST_StructuralColumns:
                    structuralType = StructuralType.Column;
                    break;
                case BuiltInCategory.OST_StructuralFraming:
                    structuralType = StructuralType.Beam;
                    break;
            }

            if (structuralType == StructuralType.NonStructural)
                return null;

            Document document = familySymbol.Document;

            ICurve curve = (ICurve)modelInstance.Location;

            Level level = document.LevelBelow(curve, settings);
            if (level == null)
                return null;

            Curve revitCurve = curve.IToRevit();
            return document.Create.NewFamilyInstance(revitCurve, familySymbol, level, structuralType);
        }

        /***************************************************/

        private static Element ToRevitElement_TwoLevelsBased(this ModelInstance modelInstance, FamilySymbol familySymbol, RevitSettings settings)
        {
            if (familySymbol == null || modelInstance == null)
                return null;
            
            BuiltInCategory builtInCategory = (BuiltInCategory)familySymbol.Category.Id.IntegerValue;

            StructuralType structuralType = StructuralType.NonStructural;
            if (typeof(BH.oM.Physical.Elements.Column).BuiltInCategories().Contains(builtInCategory))
                structuralType = StructuralType.Column;
            else if (typeof(BH.oM.Physical.Elements.Bracing).BuiltInCategories().Contains(builtInCategory))
                structuralType = StructuralType.Brace;
            else if (typeof(BH.oM.Physical.Elements.IFramingElement).BuiltInCategories().Contains(builtInCategory))
                structuralType = StructuralType.Beam;

            Document document = familySymbol.Document;

            if (modelInstance.Location is oM.Geometry.Point)
            {
                oM.Geometry.Point point = (oM.Geometry.Point)modelInstance.Location;

                Level level = document.LevelBelow(point, settings);
                if (level == null)
                    return null;

                XYZ xyz = point.ToRevit();

                FamilyInstance familyInstance = document.Create.NewFamilyInstance(xyz, familySymbol, level, structuralType);

                


                Vector x = modelInstance.Orientation?.X;
                if (x != null)
                {
                    if (Math.Abs(x.Z) > settings.AngleTolerance)
                        modelInstance.ProjectedOnXYWarning(familyInstance);

                    Vector xdir = modelInstance.Orientation.ProjectedX(settings);
                    double angle = Vector.XAxis.SignedAngle(xdir, Vector.ZAxis);
                    if (Math.Abs(angle) > settings.AngleTolerance)
                    {
                        Autodesk.Revit.DB.Line dir = Autodesk.Revit.DB.Line.CreateBound(xyz, xyz + XYZ.BasisZ);
                        ElementTransformUtils.RotateElement(document, familyInstance.Id, dir, angle);
                        document.Regenerate();
                    }
                }

                return familyInstance;

                //document.Regenerate();
                //XYZ location = ((LocationPoint)familyInstance.Location).Point;
                //if (Math.Abs(location.Z - xyz.Z) > settings.DistanceTolerance)
                //{
                //    ElementTransformUtils.MoveElement(document, familyInstance.Id, xyz - location);
                //    document.Regenerate();
                //}

                //Basis orientation = modelInstance.Orientation;
                //if (orientation != null && 1 - orientation.Z.DotProduct(Vector.ZAxis) > settings.AngleTolerance)
                //    modelInstance.ProjectedOnXYWarning(familyInstance);

                //orientation = orientation.ProjectOnXY(settings);
                //familyInstance.Orient(orientation, settings);
                //return familyInstance;
            }
            else if (modelInstance.Location is oM.Geometry.Line)
            {
                oM.Geometry.Line line = (oM.Geometry.Line)modelInstance.Location;
                if (line.Start.Z > line.End.Z)
                {
                    Compute.InvalidTwoLevelLocationWarning(modelInstance, familySymbol);
                    return null;
                }

                Level level = document.LevelBelow(line.Start, settings);
                if (level == null)
                    return null;

                Curve curve = line.ToRevit();
                return document.Create.NewFamilyInstance(curve, familySymbol, level, structuralType);
            }
            else
            {
                Compute.InvalidFamilyPlacementTypeError(modelInstance, familySymbol);
                return null;
            }
        }

        /***************************************************/

        private static Element ToRevitElement_WorkPlaneBased(this ModelInstance modelInstance, FamilySymbol familySymbol, RevitSettings settings)
        {
            if (familySymbol == null || modelInstance == null)
                return null;

            Document document = familySymbol.Document;

            FamilyInstance familyInstance = null;
            if (modelInstance.Host != -1)
            {
                Element host = document.GetElement(new ElementId(modelInstance.Host));
                if (host == null)
                    return null;

                if (modelInstance.Location is BH.oM.Geometry.Point)
                {
                    XYZ xyz = ((BH.oM.Geometry.Point)modelInstance.Location).ToRevit();
                    Reference reference;
                    XYZ location = host.ClosestPointOn(xyz, out reference);
                    if (location == null || reference == null)
                        return null;

                    XYZ refDir = XYZ.BasisX;
                    if (modelInstance.Orientation?.X != null)
                    {
                        Basis orientation = modelInstance.Orientation;
                        refDir = orientation.X.ToRevit();
                        if (orientation.Z.DotProduct(orientation.X.CrossProduct(orientation.Y)) < 0)
                            refDir = -refDir;
                    }

                    familyInstance = document.Create.NewFamilyInstance(reference, location, refDir, familySymbol);
                }
                else if (modelInstance.Location is BH.oM.Geometry.Line)
                {
                    Autodesk.Revit.DB.Line revitLine = ((BH.oM.Geometry.Line)modelInstance.Location).ToRevit();
                    Reference reference;
                    Autodesk.Revit.DB.Line location = host.ClosestLineOn(revitLine, out reference);
                    if (location == null || reference == null)
                        return null;

                    familyInstance = document.Create.NewFamilyInstance(reference, location, familySymbol);
                }
                else
                {
                    Compute.InvalidFamilyPlacementTypeError(modelInstance, familySymbol);
                    return null;
                }
            }
            else
            {
                if (!(modelInstance.Location is oM.Geometry.Point))
                {
                    Compute.InvalidFamilyPlacementTypeError(modelInstance, familySymbol);
                    return null;
                }
                
                oM.Geometry.Point point = (oM.Geometry.Point)modelInstance.Location;
                XYZ xyz = point.ToRevit();

                Basis orientation = modelInstance.Orientation;
                Reference reference;
                if (orientation != null && 1 - orientation.Z.DotProduct(Vector.ZAxis) > settings.AngleTolerance)
                {
                    //TODO: take it out to a separate Create method, do renaming
                    Transform transform = orientation.ToRevit();
                    XYZ dir1 = transform.BasisZ.CrossProduct(XYZ.BasisZ);
                    XYZ dir2 = transform.BasisZ.CrossProduct(dir1);
                    ReferencePlane rp = document.Create.NewReferencePlane(xyz, xyz + dir1, dir2, document.ActiveView);
                    if (rp.Normal.DotProduct(transform.BasisZ) < 0)
                        rp.Flip();

                    reference = rp.GetReference();
                }
                else
                    reference = document.LevelBelow(point, settings).GetPlaneReference();

                XYZ refDir = XYZ.BasisX;
                if (orientation?.X != null)
                {
                    refDir = orientation.X.ToRevit();
                    if (orientation.Z.DotProduct(orientation.X.CrossProduct(orientation.Y)) < 0)
                        refDir = -refDir;
                }

                familyInstance = document.Create.NewFamilyInstance(reference, xyz, refDir, familySymbol);
            }

            return familyInstance;
        }


        /***************************************************/
        /****   Private Methods - Drafting Instances    ****/
        /***************************************************/

        private static Element IToRevitElement(this DraftingInstance draftingInstance, ElementType elementType, View view, RevitSettings settings)
        {
            if (elementType == null)
            {
                Compute.ElementTypeNotFoundWarning(draftingInstance);
                return null;
            }

            return ToRevitElement(draftingInstance, elementType as dynamic, view, settings);
        }

        /***************************************************/

        private static Element ToRevitElement(this DraftingInstance draftingInstance, FilledRegionType regionType, View view, RevitSettings settings)
        {
            ISurface location = draftingInstance.Location as ISurface;

            List<PlanarSurface> surfaces = new List<PlanarSurface>();
            if (location is PlanarSurface)
                surfaces.Add((PlanarSurface)location);
            else if (location is PolySurface)
            {
                PolySurface polySurface = (PolySurface)location;
                if (polySurface.Surfaces.Any(x => !(x is PlanarSurface)))
                {
                    draftingInstance.InvalidRegionSurfaceError();
                    return null;
                }

                surfaces = polySurface.Surfaces.Cast<PlanarSurface>().ToList();
            }
            else
            {
                draftingInstance.InvalidRegionSurfaceError();
                return null;
            }

            List<CurveLoop> loops = new List<CurveLoop>();
            foreach (PlanarSurface surface in surfaces)
            {
                foreach (ICurve curve in surface.Edges())
                {
                    loops.Add(curve.ToRevitCurveLoop());
                }
            }

            if (loops.Count != 0)
                return FilledRegion.Create(view.Document, regionType.Id, view.Id, loops);

            return null;
        }

        /***************************************************/

        private static Element ToRevitElement(this DraftingInstance draftingInstance, FamilySymbol familySymbol, View view, RevitSettings settings)
        {
            IGeometry location = draftingInstance.Location;
            
            switch (familySymbol.Family.FamilyPlacementType)
            {
                case FamilyPlacementType.ViewBased:
                    if (location is oM.Geometry.Point)
                    {
                        XYZ xyz = ((oM.Geometry.Point)location).ToRevit();
                        FamilyInstance familyInstance = view.Document.Create.NewFamilyInstance(xyz, familySymbol, view);

                        //TODO: orient here, remember about relative CS of views/sections
                        //ElementTransformUtils.GetTransformFromViewToView
                        return familyInstance;
                    }
                    else
                    {
                        Compute.InvalidFamilyPlacementTypeError(draftingInstance, familySymbol);
                        return null;
                    }
                case FamilyPlacementType.CurveBasedDetail:
                    if (location is oM.Geometry.Line)
                    {
                        Autodesk.Revit.DB.Line revitLine = ((oM.Geometry.Line)location).ToRevit();
                        return view.Document.Create.NewFamilyInstance(revitLine, familySymbol, view);
                    }
                    else
                    {
                        Compute.InvalidFamilyPlacementTypeError(draftingInstance, familySymbol);
                        return null;
                    }
                default:
                    Compute.FamilyPlacementTypeModelError(draftingInstance, familySymbol);
                    return null;
            }
        }


        /***************************************************/
        /****              Fallback Method              ****/
        /***************************************************/

        private static Element ToRevitElement(this DraftingInstance draftingInstance, ElementType elementType, View view, RevitSettings settings)
        {
            return null;
        }

        /***************************************************/

        private static Element ToRevitElement(this ModelInstance modelInstance, ElementType elementType, RevitSettings settings)
        {
            return null;
        }


        /***************************************************/
        /****          Private Methods - Others         ****/
        /***************************************************/

        //private static void Orient(this FamilyInstance familyInstance, Basis orientation, RevitSettings settings)
        //{
        //    if (orientation == null)
        //        return;

        //    Document doc = familyInstance.Document;
        //    XYZ origin = ((LocationPoint)familyInstance.Location).Point;

        //    double angle;
        //    Autodesk.Revit.DB.Line dir;
        //    if (1 - Vector.ZAxis.DotProduct(orientation.Z) > settings.AngleTolerance)
        //    {
        //        Vector normal;
        //        if (1 - Math.Abs(Vector.ZAxis.DotProduct(orientation.Z)) > settings.AngleTolerance)
        //            normal = Vector.ZAxis.CrossProduct(orientation.Z);
        //        else
        //            normal = Vector.XAxis;

        //        angle = Vector.ZAxis.SignedAngle(orientation.Z, normal);
        //        if (Math.Abs(angle) > settings.AngleTolerance)
        //        {
        //            dir = Autodesk.Revit.DB.Line.CreateBound(origin, origin + normal.ToRevit());
        //            ElementTransformUtils.RotateElement(doc, familyInstance.Id, dir, angle);
        //            doc.Regenerate();
        //        }
        //    }

        //    angle = Vector.XAxis.SignedAngle(orientation.X, Vector.ZAxis);
        //    if (Math.Abs(angle) > settings.AngleTolerance)
        //    {
        //        dir = Autodesk.Revit.DB.Line.CreateBound(origin, origin + XYZ.BasisZ);
        //        ElementTransformUtils.RotateElement(doc, familyInstance.Id, dir, angle);
        //        doc.Regenerate();
        //    }
        //}

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
                
                if (ir.Distance < minDist)
                {
                    if (normal != null && 1 - normal.DotProduct(face.ComputeNormal(ir.UVPoint)) > settings.AngleTolerance)
                        continue;

                    minDist = ir.Distance;
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

        private static Autodesk.Revit.DB.Line ClosestLineOn(this Element element, Autodesk.Revit.DB.Line refLine, out Reference reference)
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

            if (element is FamilyInstance && !((FamilyInstance)element).HasModifiedGeometry())
            {
                Transform t = ((FamilyInstance)element).GetTotalTransform();
                start = t.Inverse.OfPoint(start);
                end = t.Inverse.OfPoint(end);
            }

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
                Transform t = ((FamilyInstance)element).GetTotalTransform();
                start = t.OfPoint(start);
                end = t.OfPoint(end);
            }

            return Autodesk.Revit.DB.Line.CreateBound(startOnFace, endOnFace);
        }

        /***************************************************/

        //private static Basis ProjectOnXY(this Basis basis, RevitSettings settings)
        //{
        //    if (1 - basis.Z.DotProduct(Vector.ZAxis) <= settings.AngleTolerance)
        //        return basis;

        //    Vector x = new Vector { X = basis.X.X, Y = basis.X.Y };
        //    Vector y = new Vector { X = basis.Y.X, Y = basis.Y.Y };
        //    if (x.Length() < settings.DistanceTolerance)
        //    {
        //        y = y.Normalise();
        //        x = y.CrossProduct(Vector.ZAxis);
        //    }
        //    else if (y.Length() < settings.DistanceTolerance)
        //    {
        //        x = x.Normalise();
        //        y = Vector.ZAxis.CrossProduct(x);
        //    }
        //    else
        //    {
        //        x = x.Normalise();
        //        y = y.Normalise();
        //    }

        //    return new Basis(x, y, Vector.ZAxis);
        //}

        /***************************************************/

        //private static XYZ ProjectedX(this ModelInstance modelInstance, RevitSettings settings, out bool projected)
        //{
        //    projected = false;
        //    XYZ xdir = XYZ.BasisX;
        //    if (modelInstance.Orientation?.X != null)
        //    {
        //        Vector x = modelInstance.Orientation.X;
        //        if (Math.Abs(x.Z) > settings.AngleTolerance)
        //            projected = true;

        //        Vector newX = new Vector { X = x.X, Y = x.Y };
        //        if (newX.Length() > settings.DistanceTolerance)
        //            xdir = newX.ToRevit();
        //    }

        //    return xdir;
        //}

        private static Vector ProjectedX(this Basis basis, RevitSettings settings)
        {
            Vector x = basis?.X;
            if (x == null)
                return Vector.XAxis;

            Vector result = new Vector { X = x.X, Y = x.Y };
            if (result.Length() < settings.DistanceTolerance)
                result = Vector.XAxis;

            return result;
        }
    }
}
