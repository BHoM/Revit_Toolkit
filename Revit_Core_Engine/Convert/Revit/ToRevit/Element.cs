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
            Document doc = familySymbol.Document;
            if (AdaptiveComponentInstanceUtils.IsAdaptiveFamilySymbol(familySymbol))
            {
                List<IGeometry> pts = (modelInstance.Location as CompositeGeometry)?.Elements;
                if (pts == null || !pts.All(x => x is BH.oM.Geometry.Point))
                {
                    BH.Engine.Reflection.Compute.RecordError($"A family could not be created based on the given ModelInstance because its family type is adaptive, but location was not a collection of points. BHoM_Guid: {modelInstance.BHoM_Guid}");
                    return null;
                }

                return Create.AdaptiveComponent(doc, familySymbol, pts.Select(x => ((BH.oM.Geometry.Point)x).ToRevit()).ToList(), settings);
            }

            if (modelInstance.Location is BH.oM.Geometry.Point) 
                return Create.FamilyInstance(doc, familySymbol, ((BH.oM.Geometry.Point)modelInstance.Location).ToRevit(), modelInstance.Orientation.ToRevit(), doc.GetElement(new ElementId(modelInstance.HostId)), settings);
            else if (modelInstance.Location is ICurve)
                return Create.FamilyInstance(doc, familySymbol, ((ICurve)modelInstance.Location).IToRevit(), doc.GetElement(new ElementId(modelInstance.HostId)), settings);
            else
            {
                BH.Engine.Reflection.Compute.RecordError($"A family could not be created based on the given ModelInstance because its location was neither a point nor a curve. BHoM_Guid: {modelInstance.BHoM_Guid}");
                return null;
            }
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
            if (draftingInstance?.Location == null)
                return null;

            if (draftingInstance.Location is ICurve)
                return Create.FamilyInstance(familySymbol.Document, familySymbol, ((ICurve)draftingInstance.Location).IToRevit(), view, settings);
            else if (draftingInstance.Location is BH.oM.Geometry.Point)
                return Create.FamilyInstance(familySymbol.Document, familySymbol, ((BH.oM.Geometry.Point)draftingInstance.Location).ToRevit(), draftingInstance.Orientation.ToRevit(), view, settings);
            else
                return null;
        }


        /***************************************************/
        /****              Fallback Methods             ****/
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
    }
}
