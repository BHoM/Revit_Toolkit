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
using BH.oM.Base;
using System;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****               Internal methods            ****/
        /***************************************************/

        internal static void NotConvertedError(this IBHoMObject obj)
        {
            string message = String.Format("BHoM object conversion to Revit failed.");

            if (obj != null)
                message += string.Format(" BHoM object type: {0}, BHoM Guid: {1}", obj.GetType(), obj.BHoM_Guid);

            BH.Engine.Reflection.Compute.RecordError(message);
        }

        /***************************************************/

        internal static void ConvertBeforePushError(this IBHoMObject iBHoMObject, Type typeToConvert)
        {
            BH.Engine.Reflection.Compute.RecordError(string.Format("{0} has to be converted to {1} before pushing. BHoM object Guid: {2}", iBHoMObject.GetType().Name, typeToConvert.Name, iBHoMObject.BHoM_Guid));
        }

        /***************************************************/

        internal static void NoPanelLocationError(this Element element)
        {
            BH.Engine.Reflection.Compute.RecordError(string.Format("Location of the Revit {0} could not be converted to BHoM, and therefore the output BHoM object will have empty location and no openings. Please note that BHoM panel conversion supports only planar faces. Revit ElementId: {1}", element.GetType().Name, element.Id.IntegerValue));
        }

        /***************************************************/

        internal static void InvalidFamilyPlacementTypeError(this IBHoMObject bHoMObject, ElementType elementType)
        {
            BH.Engine.Reflection.Compute.RecordError($"BHoM Object location does not match with the required placement type of Revit family. BHoM Guid: {bHoMObject.BHoM_Guid}, Revit ElementId: {elementType.Id.IntegerValue}");
        }

        /***************************************************/

        internal static void FamilyPlacementTypeDraftingError(this FamilySymbol familySymbol)
        {
            BH.Engine.Reflection.Compute.RecordError($"Revit family placement type named {familySymbol.Family.FamilyPlacementType} indicates that the family is a drafting family. Please use DraftingInstance instead of a ModelInstance in order to push it. Revit ElementId: {familySymbol.Id.IntegerValue}");
        }

        /***************************************************/

        internal static void FamilyPlacementTypeModelError(this FamilySymbol familySymbol)
        {
            BH.Engine.Reflection.Compute.RecordError($"Revit family placement type named {familySymbol.Family.FamilyPlacementType} indicates that the family is a model family. Please use ModelInstance instead of a DraftingInstance in order to push it. Revit ElementId: {familySymbol.Id.IntegerValue}");
        }

        /***************************************************/

        internal static void LinearOnlyError(this FamilySymbol familySymbol)
        {
            BH.Engine.Reflection.Compute.RecordError($"Revit family placement type named {familySymbol.Family.FamilyPlacementType} accepts only linear curves. Please use ModelInstance instead of a DraftingInstance in order to push it. Revit ElementId: {familySymbol.Id.IntegerValue}");
        }

        /***************************************************/

        internal static void InvalidRegionSurfaceError(this DraftingInstance draftingInstance)
        {
            BH.Engine.Reflection.Compute.RecordError($"Only DraftingInstances with locations as PlanarSurfaces or PolySurfaces consisting of PlanarSurfaces can be converted into a FilledRegion. BHoM_Guid: {draftingInstance.BHoM_Guid}");
        }

        /***************************************************/
        
        internal static void InvalidTwoLevelLocationError(this FamilySymbol familySymbol)
        {
            BH.Engine.Reflection.Compute.RecordError($"Location line of the two-level based element is upside-down, which is not allowed for given family placement type. ElementId: {familySymbol.Id.IntegerValue}");
        }

        /***************************************************/
    }
}
