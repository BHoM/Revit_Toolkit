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
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Physical.Elements;
using System;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****            Interface methods              ****/
        /***************************************************/

        public static FamilyInstance IToRevitFamilyInstance(this IFramingElement framingElement, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return ToRevitFamilyInstance(framingElement as dynamic, document, settings, refObjects);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static FamilyInstance ToRevitFamilyInstance(this Column framingElement, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (framingElement == null || document == null)
                return null;

            FamilyInstance familyInstance = refObjects.GetValue<FamilyInstance>(document, framingElement.BHoM_Guid);
            if (familyInstance != null)
                return familyInstance;

            settings = settings.DefaultIfNull();

            if (framingElement.Location == null)
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Revit element could not be created because the driving curve of a BHoM object is null. BHoM_Guid: {0}", framingElement.BHoM_Guid));
                return null;
            }

            if (framingElement.Location as BH.oM.Geometry.Line == null)
            {
                BH.Engine.Reflection.Compute.RecordError(string.Format("Revit does only support line-based columns. Try pushing your element as a beam instead. BHoM_Guid: {0}", framingElement.BHoM_Guid));
                return null;
            }

            if (((BH.oM.Geometry.Line)framingElement.Location).Start.Z >= ((BH.oM.Geometry.Line)framingElement.Location).End.Z)
            {
                BH.Engine.Reflection.Compute.RecordError(string.Format("Start point of Revit columns need to have lower elevation than the end point. Have a look at flipping your location curves. BHoM_Guid: {0}", framingElement.BHoM_Guid));
                return null;
            }

            Level level = document.LevelBelow(framingElement.Location, settings);
            if (level == null)
                return null;

            Line columnLine = framingElement.Location.IToRevit() as Line;

            FamilySymbol familySymbol = framingElement.Property.ToRevitElementType(document, framingElement.BuiltInCategories(document), settings, refObjects);
            if (familySymbol == null)
                familySymbol = framingElement.ElementType(document, settings) as FamilySymbol;

            if (familySymbol == null)
            {
                Compute.ElementTypeNotFoundWarning(framingElement);
                return null;
            }

            FamilyPlacementType familyPlacementType = familySymbol.Family.FamilyPlacementType;
            if (familyPlacementType != FamilyPlacementType.CurveBased && familyPlacementType != FamilyPlacementType.CurveBasedDetail && familyPlacementType != FamilyPlacementType.CurveDrivenStructural && familyPlacementType != FamilyPlacementType.TwoLevelsBased)
            {
                Compute.InvalidFamilyPlacementTypeError(framingElement, familySymbol);
                return null;
            }

            familyInstance = document.Create.NewFamilyInstance(columnLine, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.Column);
            document.Regenerate();

            familyInstance.CheckIfNullPush(framingElement);
            if (familyInstance == null)
                return null;

            oM.Physical.FramingProperties.ConstantFramingProperty barProperty = framingElement.Property as oM.Physical.FramingProperties.ConstantFramingProperty;
            if (barProperty != null)
            {
                //TODO: if the material does not get assigned an error should be thrown?
                if (barProperty.Material != null)
                {
                    Autodesk.Revit.DB.Material material = document.GetElement(new ElementId(BH.Engine.Adapters.Revit.Query.ElementId(barProperty.Material))) as Autodesk.Revit.DB.Material;
                    if (material != null)
                    {
                        Parameter param = familyInstance.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                        if (param != null && param.HasValue && !param.IsReadOnly)
                            familyInstance.StructuralMaterialId = material.Id;
                        else
                            BH.Engine.Reflection.Compute.RecordWarning(string.Format("The BHoM material has been correctly converted, but the property could not be assigned to the Revit element. ElementId: {0}", familyInstance.Id));
                    }
                }
            }

            // Make sure the top is above base, otherwise Revit will complain for no reason.
            familyInstance.get_Parameter((BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM)).Set(-1e+3);
            familyInstance.get_Parameter((BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM)).Set(1e+3);

            familyInstance.CopyParameters(framingElement, settings);
            familyInstance.SetLocation(framingElement, settings);
            
            refObjects.AddOrReplace(framingElement, familyInstance);
            return familyInstance;
        }

        /***************************************************/

        public static FamilyInstance ToRevitFamilyInstance(this Pile framingElement, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            BH.Engine.Reflection.Compute.RecordError(string.Format("Push of pile foundations is not supported in current version of BHoM. BHoM element Guid: {0}", framingElement.BHoM_Guid));
            return null;
        }

        /***************************************************/

        public static FamilyInstance ToRevitFamilyInstance(this IFramingElement framingElement, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (framingElement == null || document == null)
                return null;

            FamilyInstance familyInstance = refObjects.GetValue<FamilyInstance>(document, framingElement.BHoM_Guid);
            if (familyInstance != null)
                return familyInstance;

            settings = settings.DefaultIfNull();

            if (framingElement.Location == null)
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Revit element could not be created because the driving curve of a BHoM object is null. BHoM_Guid: {0}", framingElement.BHoM_Guid));
                return null;
            }

            if (!framingElement.Location.IIsPlanar())
            {
                BH.Engine.Reflection.Compute.RecordError(string.Format("Revit framing does only support planar curves, element could not be created. BHoM_Guid: {0}", framingElement.BHoM_Guid));
                return null;
            }

            Curve revitCurve = framingElement.Location.IToRevit();
            if (revitCurve == null)
            {
                BH.Engine.Reflection.Compute.RecordError(string.Format("Revit element could not be created because of curve conversion issues. BHoM_Guid: {0}", framingElement.BHoM_Guid));
                return null;
            }

            Level level = document.LevelBelow(framingElement.Location, settings);

            FamilySymbol familySymbol = framingElement.Property.ToRevitElementType(document, framingElement.BuiltInCategories(document), settings, refObjects);
            if (familySymbol == null)
                familySymbol = framingElement.ElementType(document, settings) as FamilySymbol;

            if (familySymbol == null)
            {
                Compute.ElementTypeNotFoundWarning(framingElement);
                return null;
            }

            FamilyPlacementType familyPlacementType = familySymbol.Family.FamilyPlacementType;
            if (familyPlacementType != FamilyPlacementType.CurveBased && familyPlacementType != FamilyPlacementType.CurveBasedDetail && familyPlacementType != FamilyPlacementType.CurveDrivenStructural && familyPlacementType != FamilyPlacementType.TwoLevelsBased)
            {
                Compute.InvalidFamilyPlacementTypeError(framingElement, familySymbol);
                return null;
            }

            if (framingElement is Beam)
                familyInstance = document.Create.NewFamilyInstance(revitCurve, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.Beam);
            else if (framingElement is Bracing || framingElement is Cable)
                familyInstance = document.Create.NewFamilyInstance(revitCurve, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.Brace);
            else
                familyInstance = document.Create.NewFamilyInstance(revitCurve, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.UnknownFraming);

            document.Regenerate();

            familyInstance.CheckIfNullPush(framingElement);
            if (familyInstance == null)
                return null;

            oM.Physical.FramingProperties.ConstantFramingProperty barProperty = framingElement.Property as oM.Physical.FramingProperties.ConstantFramingProperty;
            if (barProperty != null)
            {
                //TODO: if the material does not get assigned an error should be thrown?
                if (barProperty.Material != null)
                {
                    Material material = document.GetElement(new ElementId(BH.Engine.Adapters.Revit.Query.ElementId(barProperty.Material))) as Material;
                    if (material != null)
                    {
                        Parameter param = familyInstance.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                        if (param != null && param.HasValue && !param.IsReadOnly)
                            familyInstance.StructuralMaterialId = material.Id;
                        else
                            BH.Engine.Reflection.Compute.RecordWarning(string.Format("The BHoM material has been correctly converted, but the property could not be assigned to the Revit element. ElementId: {0}", familyInstance.Id));
                    }
                }
            }

            //Set the insertion point to centroid. 
            Parameter zJustification = familyInstance.get_Parameter(BuiltInParameter.Z_JUSTIFICATION);
            if (zJustification != null && !zJustification.IsReadOnly)
                zJustification.Set((int)Autodesk.Revit.DB.Structure.ZJustification.Origin);

            familyInstance.CopyParameters(framingElement, settings);
            familyInstance.SetLocation(framingElement, settings);

            if (familyInstance.StructuralMaterialType != StructuralMaterialType.Concrete && familyInstance.StructuralMaterialType != StructuralMaterialType.PrecastConcrete)
            {
                StructuralFramingUtils.DisallowJoinAtEnd(familyInstance, 0);
                StructuralFramingUtils.DisallowJoinAtEnd(familyInstance, 1);
            }

            refObjects.AddOrReplace(framingElement, familyInstance);
            return familyInstance;
        }
        
        /***************************************************/
    }
}

