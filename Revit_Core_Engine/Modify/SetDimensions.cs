/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using BH.oM.Spatial.ShapeProfiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Sets the dimensions of a given Revit FamilyInstance based on a given BHoM builders work Opening.")]
        [Input("familyInstance", "Revit FamilyInstance to be modified.")]
        [Input("opening", "BHoM builders work Opening acting as a source of information about the new dimensions.")]
        [Input("settings", "Revit adapter settings to be used while performing the operation.")]
        [Output("success", "True if dimensions of the input Revit FamilyInstance has been successfully set.")]
        public static bool SetDimensions(this FamilyInstance familyInstance, BH.oM.Architecture.BuildersWork.Opening opening, RevitSettings settings)
        {
            if (familyInstance == null || opening == null)
                return false;

            bool success = false;
            settings = settings.DefaultIfNull();

            // Set dimensions based on parameter mapping.
            HashSet<string> parameterNames = settings.MappingSettings.ParameterNames(typeof(oM.Architecture.BuildersWork.Opening), nameof(oM.Architecture.BuildersWork.Opening.Depth));
            if (parameterNames == null || !familyInstance.SetParameters(parameterNames, opening.Depth))
                familyInstance.DimensionNotSetWarning("depth");
            else
                success = true;

            if (opening.Profile is RectangleProfile)
            {
                parameterNames = settings.MappingSettings.ParameterNames(typeof(oM.Architecture.BuildersWork.Opening), $"{nameof(oM.Architecture.BuildersWork.Opening.Profile)}.{nameof(RectangleProfile.Width)}");
                if (parameterNames == null || !familyInstance.SetParameters(parameterNames, ((RectangleProfile)opening.Profile).Width))
                    familyInstance.DimensionNotSetWarning("width");
                else
                    success = true;

                parameterNames = settings.MappingSettings.ParameterNames(typeof(oM.Architecture.BuildersWork.Opening), $"{nameof(oM.Architecture.BuildersWork.Opening.Profile)}.{nameof(RectangleProfile.Height)}");
                if (parameterNames == null || !familyInstance.SetParameters(parameterNames, ((RectangleProfile)opening.Profile).Height))
                    familyInstance.DimensionNotSetWarning("height");
                else
                    success = true;
            }
            else if (opening.Profile is CircleProfile)
            {
                parameterNames = settings.MappingSettings.ParameterNames(typeof(oM.Architecture.BuildersWork.Opening), $"{nameof(oM.Architecture.BuildersWork.Opening.Profile)}.{nameof(CircleProfile.Diameter)}");
                if (parameterNames == null || !familyInstance.SetParameters(parameterNames, ((CircleProfile)opening.Profile).Diameter))
                    familyInstance.DimensionNotSetWarning("diameter");
                else
                    success = true;
            }

            return success;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static void DimensionNotSetWarning(this Element element, string dimension)
        {
            BH.Engine.Base.Compute.RecordWarning($"The family instance has been created, but its {dimension} could not be set. ElementId: {element.Id}");
        }

        /***************************************************/
    }
}


