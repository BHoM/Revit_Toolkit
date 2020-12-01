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

            return ToRevitElement(modelInstance, elementType as dynamic);
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
                familyInstance = document.Create.NewFamilyInstance(revitCurve, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            else
            {
                if (revitCurve is Autodesk.Revit.DB.Line)
                {
                    Element host = document.GetElement(new ElementId(modelInstance.Host));
                    //TODO: project on face and place using NewFamilyInstance(Face, Line, FamilySymbol)
                }
                else
                {
                    //TODO: error that needs to be a line
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

            Level level = document.LevelBelow(point, settings);
            if (level == null)
                return null;

            XYZ xyz = point.ToRevit();
            FamilyInstance familyInstance = null;
            if (modelInstance.Host == -1)
            {
                familyInstance = document.Create.NewFamilyInstance(xyz, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                document.Regenerate();
                familyInstance.Orient(modelInstance.Orientation, settings);
            }
            else
            {
                //TODO: find closest point, place oriented using NewFamilyInstance(XYZ, FamilySymbol, XYZ, Element, StructuralType)
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

            Autodesk.Revit.DB.Structure.StructuralType structuralType = Autodesk.Revit.DB.Structure.StructuralType.NonStructural;

            BuiltInCategory builtInCategory = (BuiltInCategory)familySymbol.Category.Id.IntegerValue;
            switch (builtInCategory)
            {
                case BuiltInCategory.OST_StructuralColumns:
                    structuralType = Autodesk.Revit.DB.Structure.StructuralType.Column;
                    break;
                case BuiltInCategory.OST_StructuralFraming:
                    structuralType = Autodesk.Revit.DB.Structure.StructuralType.Beam;
                    break;
            }

            if (structuralType == Autodesk.Revit.DB.Structure.StructuralType.NonStructural)
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

            Autodesk.Revit.DB.Structure.StructuralType structuralType;
            if (typeof(BH.oM.Physical.Elements.Column).BuiltInCategories().Contains(builtInCategory))
                structuralType = Autodesk.Revit.DB.Structure.StructuralType.Column;
            else if (typeof(BH.oM.Physical.Elements.Bracing).BuiltInCategories().Contains(builtInCategory))
                structuralType = Autodesk.Revit.DB.Structure.StructuralType.Brace;
            else if (typeof(BH.oM.Physical.Elements.IFramingElement).BuiltInCategories().Contains(builtInCategory))
                structuralType = Autodesk.Revit.DB.Structure.StructuralType.Beam;
            else
                return null;

            Document document = familySymbol.Document;

            if (modelInstance.Location is oM.Geometry.Point)
            {
                oM.Geometry.Point point = (oM.Geometry.Point)modelInstance.Location;

                Level level = document.LevelBelow(point, settings);
                if (level == null)
                    return null;

                XYZ xyz = point.ToRevit();
                FamilyInstance familyInstance = document.Create.NewFamilyInstance(xyz, familySymbol, level, structuralType);

                //TODO: add orientation here!
                return familyInstance;
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
                FamilyInstance familyInstance = document.Create.NewFamilyInstance(curve, familySymbol, level, structuralType);

                //TODO: add orientation here!
                return familyInstance;
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

            FamilyInstance familyInstance;
            Element host = document.GetElement(new ElementId(modelInstance.Host));
            if (host != null)
            {
                //TODO:
                // 2 scenarios needed:
                // - point-based, create transformed using NewFamilyInstance(Face, XYZ, XYZ, FamilySymbol)
                // - curve-based, NewFamilyInstance(Face, Line, FamilySymbol) - make sure its line

                double minDist = double.MaxValue;
                Reference reference = null;

                Options opt = new Options();
                opt.ComputeReferences = true;
                List<Autodesk.Revit.DB.Face> faces = host.Faces(opt);
                IntersectionResult ir;
                XYZ pointOnFace = null;

                if (host is FamilyInstance && !((FamilyInstance)host).HasModifiedGeometry())
                    location = ((FamilyInstance)host).GetTotalTransform().Inverse.OfPoint(location);

                foreach (Autodesk.Revit.DB.Face face in faces)
                {
                    ir = face.Project(location);
                    if (ir == null || ir.XYZPoint == null)
                        continue;

                    if (ir.Distance < minDist)
                    {
                        minDist = ir.Distance;
                        pointOnFace = ir.XYZPoint;
                        reference = face.Reference;
                    }
                }

                if (pointOnFace == null || reference == null)
                    return null;

                if (host is FamilyInstance && !((FamilyInstance)host).HasModifiedGeometry())
                    pointOnFace = ((FamilyInstance)host).GetTotalTransform().OfPoint(pointOnFace);

                familyInstance = document.Create.NewFamilyInstance(reference, pointOnFace, XYZ.BasisX, familySymbol);
            }
            else
            {
                if (!(modelInstance.Location is oM.Geometry.Point))
                {
                    Compute.InvalidFamilyPlacementTypeError(modelInstance, familySymbol);
                    return null;
                }
                
                oM.Geometry.Point point = (oM.Geometry.Point)modelInstance.Location;
                XYZ location = point.ToRevit();

                Level level = document.LevelBelow(point, settings);
                if (level == null)
                    return null;

                familyInstance = document.Create.NewFamilyInstance(location, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                document.Regenerate();
                familyInstance.Orient(modelInstance.Orientation, settings);
            }

            //document.Regenerate();
            //if (host != null)
            //    familyInstance.OrientHosted(modelInstance.Orientation, settings);
            //else
            //    familyInstance.Orient(modelInstance.Orientation, settings);

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
        /****       Private Methods - Orientation       ****/
        /***************************************************/

        private static void OrientHosted(this FamilyInstance familyInstance, Basis orientation, RevitSettings settings)
        {
            Document doc = familyInstance.Document;
            XYZ normal = familyInstance.GetTotalTransform().BasisZ;
            XYZ origin = ((LocationPoint)familyInstance.Location).Point;

            double angle = Vector.XAxis.SignedAngle(orientation.X, normal.VectorFromRevit());
            if (Math.Abs(angle) > settings.AngleTolerance)
            {
                Autodesk.Revit.DB.Line dir = Autodesk.Revit.DB.Line.CreateBound(origin, origin + normal);
                ElementTransformUtils.RotateElement(doc, familyInstance.Id, dir, angle);
                doc.Regenerate();
            }
        }

        /***************************************************/

        private static void Orient(this FamilyInstance familyInstance, Basis orientation, RevitSettings settings)
        {
            Document doc = familyInstance.Document;
            XYZ origin = ((LocationPoint)familyInstance.Location).Point;

            double angle;
            Autodesk.Revit.DB.Line dir;
            if (1 - Vector.ZAxis.DotProduct(orientation.Z) > settings.AngleTolerance)
            {
                Vector normal;
                if (1 - Math.Abs(Vector.ZAxis.DotProduct(orientation.Z)) > settings.AngleTolerance)
                    normal = Vector.ZAxis.CrossProduct(orientation.Z);
                else
                    normal = Vector.XAxis;

                angle = Vector.ZAxis.SignedAngle(orientation.Z, normal);
                if (Math.Abs(angle) > settings.AngleTolerance)
                {
                    dir = Autodesk.Revit.DB.Line.CreateBound(origin, origin + normal.ToRevit());
                    ElementTransformUtils.RotateElement(doc, familyInstance.Id, dir, angle);
                    doc.Regenerate();
                }
            }

            angle = Vector.XAxis.SignedAngle(orientation.X, Vector.ZAxis);
            if (Math.Abs(angle) > settings.AngleTolerance)
            {
                dir = Autodesk.Revit.DB.Line.CreateBound(origin, origin + XYZ.BasisZ);
                ElementTransformUtils.RotateElement(doc, familyInstance.Id, dir, angle);
                doc.Regenerate();
            }
        }

        /***************************************************/
    }
}
