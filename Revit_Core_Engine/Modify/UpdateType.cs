﻿/*
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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Physical.Elements;
using System;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static bool UpdateType(this FamilyInstance element, IFramingElement bHoMObject, RevitSettings settings)
        {
            FamilySymbol familySymbol = bHoMObject.Property.ToRevitFamilySymbol_Framing(element.Document, settings);
            if (familySymbol == null)
                familySymbol = Query.ElementType(bHoMObject, element.Document, BuiltInCategory.OST_StructuralFraming, settings.FamilyLoadSettings) as FamilySymbol;

            if (familySymbol == null)
            {
                Compute.ElementTypeNotFoundWarning(bHoMObject);
                return false;
            }
            else
                return element.SetParameter(BuiltInParameter.ELEM_TYPE_PARAM, familySymbol.Id);
        }


        /***************************************************/
        /****              Fallback Methods             ****/
        /***************************************************/
        
        public static bool UpdateType(this Element element, IBHoMObject bHoMObject, RevitSettings settings)
        {
            BH.Engine.Reflection.Compute.RecordWarning(String.Format("Element type has not been updated based on the BHoM object due to the lacking convert method. Revit ElementId: {0} BHoM_Guid: {1}", element.Id, bHoMObject.BHoM_Guid));
            return false;
        }
        

        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        public static bool IUpdateType(this Element element, IBHoMObject bHoMObject, RevitSettings settings)
        {
            return UpdateType(element as dynamic, bHoMObject as dynamic, settings);
        }

        /***************************************************/
    }
}

