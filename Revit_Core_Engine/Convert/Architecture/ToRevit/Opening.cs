﻿/*
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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Spatial.ShapeProfiles;
using System;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static FamilyInstance ToRevitFamilyInstance(this oM.Architecture.BuildersWork.Opening opening, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (opening == null || document == null)
                return null;

            FamilyInstance familyInstance = refObjects.GetValue<FamilyInstance>(document, opening.BHoM_Guid);
            if (familyInstance != null)
                return familyInstance;

            settings = settings.DefaultIfNull();

            if (opening.CoordinateSystem == null)
            {
                BH.Engine.Reflection.Compute.RecordError($"Revit element could not be created because the coordinate system of the opening is null. BHoM_Guid: {opening.BHoM_Guid}");
                return null;
            }

            FamilySymbol familySymbol = opening.ElementType(document, settings) as FamilySymbol;
            if (familySymbol == null)
            {
                Compute.ElementTypeNotFoundWarning(opening);
                return null;
            }

            //TODO: unify linkPath on identifiers with Id on host? how? HostFragment and Identifiers to carry link NAME! NOT PATH NOR ID
            //TODO: when above sorted, SetHost needs update
            //TODO: make sure it works for linked hosts on push & pull - check if transforms OK
            //TODO: Version LINKPATH removal, replaced with LinkDocument! also prop versioning in V_TK


            Element host = opening.HostElement(document, settings, true);
            if (host == null)
                BH.Engine.Reflection.Compute.RecordWarning("Host could not be found for the opening.");

            BH.oM.Geometry.CoordinateSystem.Cartesian cs = opening.CoordinateSystem;
            XYZ basisX = new XYZ(cs.X.X, cs.X.Y, cs.X.Z);
            XYZ basisY = new XYZ(cs.Y.X, cs.Y.Y, cs.Y.Z);
            XYZ basisZ = new XYZ(cs.Z.X, cs.Z.Y, cs.Z.Z);
            Transform orientation = Transform.Identity;
            orientation.set_Basis(0, basisX);
            orientation.set_Basis(1, basisY);
            orientation.set_Basis(2, basisZ);

            familyInstance = BH.Revit.Engine.Core.Create.FamilyInstance(document, familySymbol, opening.CoordinateSystem.Origin.ToRevit(), orientation, host, settings);
            
            document.Regenerate();

            familyInstance.CheckIfNullPush(opening);
            if (familyInstance == null)
                return null;

            // Set dimensions using a dedicated method.
            familyInstance.SetDimensions(opening, settings);

            familyInstance.CopyParameters(opening, settings);

            refObjects.AddOrReplace(opening, familyInstance);
            return familyInstance;
        }

        /***************************************************/
    }
}
