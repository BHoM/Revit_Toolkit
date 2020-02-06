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

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.Engine.Geometry;

using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static Element ToRevitElement(this ModelInstance modelInstance, Document document, PushSettings pushSettings = null)
        {
            Element element = pushSettings.FindRefObject<Element>(document, modelInstance.BHoM_Guid);
            if (element != null)
                return element;

            if(modelInstance.Properties == null)
            {
                Compute.ElementTypeNotFoundWarning(modelInstance);
                return null;
            }

            pushSettings.DefaultIfNull();

            BuiltInCategory builtInCategory = modelInstance.Properties.BuiltInCategory(document, pushSettings.FamilyLoadSettings);

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
                ElementType elementType = modelInstance.Properties.ElementType(document, builtInCategory, pushSettings.FamilyLoadSettings);
                if (elementType == null)
                {
                    Compute.ElementTypeNotFoundWarning(modelInstance);
                    return null;
                }

                if (elementType is FamilySymbol)
                {
                    FamilySymbol familySymbol = (FamilySymbol)elementType;
                    FamilyPlacementType familyPlacementType = familySymbol.Family.FamilyPlacementType;

                    IGeometry geometry = modelInstance.Location;

                    if (familyPlacementType == FamilyPlacementType.Invalid ||
                        familyPlacementType == FamilyPlacementType.Adaptive ||
                        (geometry is oM.Geometry.Point && (familyPlacementType == FamilyPlacementType.CurveBased || familyPlacementType == FamilyPlacementType.CurveBasedDetail || familyPlacementType == FamilyPlacementType.CurveDrivenStructural)) ||
                        (geometry is ICurve && (familyPlacementType == FamilyPlacementType.OneLevelBased || familyPlacementType == FamilyPlacementType.OneLevelBasedHosted)))
                    {
                        Compute.InvalidFamilyPlacementTypeWarning(modelInstance, elementType);
                        return null;
                    }

                    switch (familyPlacementType)
                    {
                        //TODO: Implement rest of the cases
                        case FamilyPlacementType.CurveBased:
                            element = ToRevitElement_CurveBased(modelInstance, familySymbol, pushSettings);
                            break;
                        case FamilyPlacementType.OneLevelBased:
                            element = ToRevitElement_OneLevelBased(modelInstance, familySymbol, pushSettings);
                            break;
                        case FamilyPlacementType.CurveDrivenStructural:
                            element = ToRevitElement_CurveDrivenStructural(modelInstance, familySymbol, pushSettings);
                            break;
                        case FamilyPlacementType.TwoLevelsBased:
                            element = ToRevitElement_TwoLevelsBased(modelInstance, familySymbol, pushSettings);
                            break;
                        default:
                            Compute.FamilyPlacementTypeNotSupportedWarning(modelInstance, elementType);
                            return null;
                    }
                }
                else if (elementType is WallType)
                    element = ToRevitElement_Wall(modelInstance, (WallType)elementType, pushSettings);
                else if (elementType is MEPCurveType)
                    element = ToRevitMEPElement(modelInstance, elementType as MEPCurveType, pushSettings);
            }

            element.CheckIfNullPush(modelInstance);
            if (element == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(element, modelInstance, null);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(modelInstance, element);

            return element;
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static Element ToRevitElement(this DraftingInstance draftingInstance, Document document, PushSettings pushSettings = null)
        {
            if (draftingInstance == null || string.IsNullOrEmpty(draftingInstance.ViewName) || document == null)
                return null;

            if (string.IsNullOrWhiteSpace(draftingInstance.ViewName))
                return null;

            Element element = pushSettings.FindRefObject<Element>(document, draftingInstance.BHoM_Guid);
            if (element != null)
                return element;

            if (draftingInstance.Properties == null)
            {
                Compute.NullObjectPropertiesWarining(draftingInstance);
                return null;
            }

            View view = Query.View(draftingInstance, document);

            if (view == null)
                return null;

            pushSettings = pushSettings.DefaultIfNull();

            BuiltInCategory builtInCategory = draftingInstance.Properties.BuiltInCategory(document, pushSettings.FamilyLoadSettings);

            ElementType elementType = draftingInstance.Properties.ElementType(document, builtInCategory, pushSettings.FamilyLoadSettings);

            if (elementType != null)
            {
                if (elementType is FamilySymbol)
                {
                    FamilySymbol familySymbol = (FamilySymbol)elementType;
                    Autodesk.Revit.DB.Family family = familySymbol.Family;

                    IGeometry geometry = draftingInstance.Location;

                    switch (family.FamilyPlacementType)
                    {
                        //TODO: Implement rest of the cases

                        case FamilyPlacementType.ViewBased:
                            if (geometry is oM.Geometry.Point)
                            {
                                XYZ xyz = ((oM.Geometry.Point)geometry).ToRevit();
                                element = document.Create.NewFamilyInstance(xyz, familySymbol, view);
                            }
                            break;
                        case FamilyPlacementType.CurveBasedDetail:
                            if (geometry is oM.Geometry.Line)
                            {
                                Autodesk.Revit.DB.Line revitLine = (geometry as oM.Geometry.Line).ToRevit();
                                element = document.Create.NewFamilyInstance(revitLine, familySymbol, view);
                            }
                            break;
                        case FamilyPlacementType.OneLevelBased:
                            break;
                    }
                }
                else if (elementType is FilledRegionType)
                {
                    FilledRegionType regionType = elementType as FilledRegionType;

                    PlanarSurface surface = draftingInstance.Location as PlanarSurface;
                    if (surface != null)
                    {
                        List<CurveLoop> loops = new List<CurveLoop>();
                        foreach (ICurve curve in surface.Edges())
                        {
                            loops.Add(curve.ToRevitCurveLoop());
                        }

                        if (loops.Count != 0)
                            element = FilledRegion.Create(document, regionType.Id, view.Id, loops);
                    }
                }
            }

            element.CheckIfNullPush(draftingInstance);
            if (element == null)
                return null;

            if (element != null && pushSettings.CopyCustomData)
                Modify.SetParameters(element, draftingInstance, null);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(draftingInstance, element);

            return element;
        }

        /***************************************************/

        private static Element ToRevitElement_CurveBased(this ModelInstance modelInstance, FamilySymbol familySymbol, PushSettings pushSettings = null)
        {
            if (familySymbol == null || modelInstance == null)
                return null;

            if (!(modelInstance.Location is ICurve))
            {
                Compute.InvalidFamilyPlacementTypeWarning(modelInstance, familySymbol);
                return null;
            }

            Document document = familySymbol.Document;

            ICurve curve = (ICurve)modelInstance.Location;

            Level level = curve.BottomLevel(familySymbol.Document);
            if (level == null)
                return null;

            Curve revitCurve = curve.IToRevit();
            if (revitCurve == null)
                return null;

            return document.Create.NewFamilyInstance(revitCurve, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
        }

        /***************************************************/

        private static Element ToRevitMEPElement(this ModelInstance modelInstance, MEPCurveType MEPType, PushSettings pushSettings = null)
        {
            if (MEPType == null || modelInstance == null)
                return null;

            if (!(modelInstance.Location is ICurve))
            {
                Compute.InvalidFamilyPlacementTypeWarning(modelInstance, MEPType);
                return null;
            }

            Document document = MEPType.Document;

            BH.oM.Geometry.Line line = modelInstance.Location as BH.oM.Geometry.Line;

            Level level = line.BottomLevel(document);
            if (level == null)
                return null;

            Autodesk.Revit.DB.Line revitLine = line.ToRevit();
            if (revitLine == null)
                return null;

            XYZ startPoint = revitLine.GetEndPoint(0);
            XYZ endPoint = revitLine.GetEndPoint(1);

            if (MEPType is Autodesk.Revit.DB.Electrical.CableTrayType)
                return Autodesk.Revit.DB.Electrical.CableTray.Create(document, MEPType.Id, startPoint, endPoint, level.Id);
            else if (MEPType is Autodesk.Revit.DB.Electrical.ConduitType)
                return Autodesk.Revit.DB.Electrical.Conduit.Create(document, MEPType.Id, startPoint, endPoint, level.Id);
            else if (MEPType is Autodesk.Revit.DB.Plumbing.PipeType)
            {
                Autodesk.Revit.DB.Plumbing.PipingSystemType pst = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Plumbing.PipingSystemType)).OfType<Autodesk.Revit.DB.Plumbing.PipingSystemType>().FirstOrDefault();
                return Autodesk.Revit.DB.Plumbing.Pipe.Create(document, pst.Id, MEPType.Id, level.Id, startPoint, endPoint);
            }
            else if (MEPType is Autodesk.Revit.DB.Mechanical.DuctType)
            {
                Autodesk.Revit.DB.Mechanical.MechanicalSystemType mst = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Mechanical.MechanicalSystemType)).OfType<Autodesk.Revit.DB.Mechanical.MechanicalSystemType>().FirstOrDefault();
                return Autodesk.Revit.DB.Mechanical.Duct.Create(document, mst.Id, MEPType.Id, level.Id, startPoint, endPoint);
            }
            else
                return null;
        }

        private static Element ToRevitElement_OneLevelBased(this ModelInstance modelInstance, FamilySymbol familySymbol, PushSettings pushSettings = null)
        {
            if (familySymbol == null || modelInstance == null)
                return null;

            if (!(modelInstance.Location is oM.Geometry.Point))
            {
                Compute.InvalidFamilyPlacementTypeWarning(modelInstance, familySymbol);
                return null;
            }

            Document document = familySymbol.Document;

            oM.Geometry.Point point = (oM.Geometry.Point)modelInstance.Location;

            Level level = point.BottomLevel(document);
            if (level == null)
                return null;

            XYZ xyz = ToRevit(point);
            return document.Create.NewFamilyInstance(xyz, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
        }

        /***************************************************/

        private static Element ToRevitElement_CurveDrivenStructural(this ModelInstance modelInstance, FamilySymbol familySymbol, PushSettings pushSettings = null)
        {
            if (familySymbol == null || modelInstance == null)
                return null;

            if (!(modelInstance.Location is ICurve))
            {
                Compute.InvalidFamilyPlacementTypeWarning(modelInstance, familySymbol);
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

            Level level = curve.BottomLevel(document);
            if (level == null)
                return null;

            Curve revitCurve = curve.IToRevit();
            return document.Create.NewFamilyInstance(revitCurve, familySymbol, level, structuralType);
        }

        /***************************************************/

        private static Element ToRevitElement_TwoLevelsBased(this ModelInstance modelInstance, FamilySymbol familySymbol, PushSettings pushSettings = null)
        {
            if (familySymbol == null || modelInstance == null)
                return null;

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

            if (modelInstance.Location is oM.Geometry.Point)
            {
                oM.Geometry.Point point = (oM.Geometry.Point)modelInstance.Location;

                Level level = point.BottomLevel(document);
                if (level == null)
                    return null;

                XYZ xyz = ToRevit(point);
                return document.Create.NewFamilyInstance(xyz, familySymbol, level, structuralType);
            }
            else if (modelInstance.Location is oM.Geometry.Line)
            {
                oM.Geometry.Line line = (oM.Geometry.Line)modelInstance.Location;
                if (line.Start.Z > line.End.Z)
                {
                    Compute.InvalidTwoLevelLocationWarning(modelInstance, familySymbol);
                    return null;
                }

                Level level = line.Start.BottomLevel(document);
                if (level == null)
                    return null;

                Curve curve = line.ToRevit();
                return document.Create.NewFamilyInstance(curve, familySymbol, level, structuralType);
            }
            else
            {
                Compute.InvalidFamilyPlacementTypeWarning(modelInstance, familySymbol);
                return null;
            }
        }

        /***************************************************/

        private static Element ToRevitElement_Wall(this ModelInstance modelInstance, WallType wallType, PushSettings pushSettings = null)
        {
            if (wallType == null || modelInstance == null)
                return null;

            if (!(modelInstance.Location is ICurve))
            {
                Compute.InvalidFamilyPlacementTypeWarning(modelInstance, wallType);
                return null;
            }

            Document document = wallType.Document;

            ICurve curve = (ICurve)modelInstance.Location;

            Level level = curve.BottomLevel(wallType.Document);
            if (level == null)
                return null;

            Curve revitCurve = curve.IToRevit();
            return Wall.Create(document, revitCurve, level.Id, false);
        }

        /***************************************************/
    }
}