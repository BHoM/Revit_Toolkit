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

            //TODO: unify linkPath on identifiers with Id on host? how? HostFragment to carry link name?
            //TODO: make sure it works for linked hosts on push & pull
            //TODO: add SetLocation!
            //TODO: on Update remember about a check regarding inability to set new host
            //TODO: remember about Update, remember about Create instances on linked elements

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

            // Set dimensions based on parameter mapping.
            HashSet<string> parameterNames = settings.MappingSettings.ParameterNames(typeof(oM.Architecture.BuildersWork.Opening), nameof(oM.Architecture.BuildersWork.Opening.Depth));
            if (parameterNames == null || !familyInstance.SetParameters(parameterNames, opening.Depth))
                familyInstance.DimensionNotSetWarning("depth");

            if (opening.Profile is RectangleProfile)
            {
                parameterNames = settings.MappingSettings.ParameterNames(typeof(oM.Architecture.BuildersWork.Opening), $"{nameof(oM.Architecture.BuildersWork.Opening.Profile)}.{nameof(RectangleProfile.Width)}");
                if (parameterNames == null || !familyInstance.SetParameters(parameterNames, ((RectangleProfile)opening.Profile).Width))
                    familyInstance.DimensionNotSetWarning("width");

                parameterNames = settings.MappingSettings.ParameterNames(typeof(oM.Architecture.BuildersWork.Opening), $"{nameof(oM.Architecture.BuildersWork.Opening.Profile)}.{nameof(RectangleProfile.Height)}");
                if (parameterNames == null || !familyInstance.SetParameters(parameterNames, ((RectangleProfile)opening.Profile).Height))
                    familyInstance.DimensionNotSetWarning("height");
            }
            else if (opening.Profile is CircleProfile)
            {
                parameterNames = settings.MappingSettings.ParameterNames(typeof(oM.Architecture.BuildersWork.Opening), $"{nameof(oM.Architecture.BuildersWork.Opening.Profile)}.{nameof(CircleProfile.Diameter)}");
                if (parameterNames == null || !familyInstance.SetParameters(parameterNames, ((CircleProfile)opening.Profile).Diameter))
                    familyInstance.DimensionNotSetWarning("diameter");
            }

            familyInstance.CopyParameters(opening, settings);

            refObjects.AddOrReplace(opening, familyInstance);
            return familyInstance;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static void DimensionNotSetWarning(this Element element, string dimension)
        {
            BH.Engine.Reflection.Compute.RecordWarning($"The family instance has been created, but its {dimension} could not be set. ElementId: {element.Id}");
        }

        /***************************************************/
    }
}
