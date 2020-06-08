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
using BH.oM.Adapters.Revit.Settings;
using System;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static FamilySymbol ToRevitFamilySymbol_Column(this oM.Physical.FramingProperties.IFramingElementProperty framingElementProperty, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (framingElementProperty == null || document == null)
                return null;

            FamilySymbol familySymbol = refObjects.GetValue<FamilySymbol>(document, framingElementProperty.BHoM_Guid);
            if (familySymbol != null)
                return familySymbol;

            settings = settings.DefaultIfNull();

            familySymbol = framingElementProperty.ElementType(document, BuiltInCategory.OST_StructuralColumns, settings.FamilyLoadSettings) as FamilySymbol;

            familySymbol.CheckIfNullPush(framingElementProperty);
            if (familySymbol == null)
                return null;

            // Copy parameters from BHoM CustomData to Revit Element
            familySymbol.CopyParameters(framingElementProperty, settings);

            refObjects.AddOrReplace(framingElementProperty, familySymbol);
            return familySymbol;
        }

        /***************************************************/

        private static FamilySymbol ToRevitFamilySymbol_Framing(this oM.Physical.FramingProperties.IFramingElementProperty framingElementProperty, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (framingElementProperty == null || document == null)
                return null;

            FamilySymbol familySymbol = refObjects.GetValue<FamilySymbol>(document, framingElementProperty.BHoM_Guid);
            if (familySymbol != null)
                return familySymbol;

            settings = settings.DefaultIfNull();

            familySymbol = framingElementProperty.ElementType(document, BuiltInCategory.OST_StructuralFraming, settings.FamilyLoadSettings) as FamilySymbol;

            familySymbol.CheckIfNullPush(framingElementProperty);
            if (familySymbol == null)
                return null;

            // Copy parameters from BHoM CustomData to Revit Element
            familySymbol.CopyParameters(framingElementProperty, settings);

            refObjects.AddOrReplace(framingElementProperty, familySymbol);
            return familySymbol;
        }

        /***************************************************/
    }
}
