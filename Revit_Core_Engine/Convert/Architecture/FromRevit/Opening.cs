/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Spatial.ShapeProfiles;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit FamilyInstance to BH.oM.Architecture.BuildersWork.Opening.")]
        [Input("instance", "Revit FamilyInstance to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("opening", "BH.oM.Architecture.BuildersWork.Opening resulting from converting the input Revit FamilyInstance.")]
        public static oM.Architecture.BuildersWork.Opening OpeningFromRevit(this FamilyInstance instance, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (instance == null)
            {
                BH.Engine.Base.Compute.RecordError($"The convert from Revit element of type {typeof(FamilyInstance).Name} to {typeof(oM.Architecture.BuildersWork.Opening).Name} failed because the input element is null.");
                return null;
            }

            settings = settings.DefaultIfNull();

            oM.Architecture.BuildersWork.Opening opening = refObjects.GetValue<oM.Architecture.BuildersWork.Opening>(instance.Id);
            if (opening != null)
                return opening;
            
            opening = new oM.Architecture.BuildersWork.Opening { Name = instance.Name };

            //Set coordinate system
            opening.CoordinateSystem = instance.CoordinateSystem();

            IProfile profile = null;
            Parameter diameterParam = instance.LookupParameter(settings.MappingSettings, typeof(oM.Architecture.BuildersWork.Opening), $"{nameof(oM.Architecture.BuildersWork.Opening.Profile)}.{nameof(CircleProfile.Diameter)}");
            if (diameterParam != null)
            {
                double diameter = diameterParam.AsDouble().ToSI(diameterParam.Definition.GetDataType());
                profile = BH.Engine.Spatial.Create.CircleProfile(diameter);
            }

            if (profile == null)
            {
                Parameter widthParam = instance.LookupParameter(settings.MappingSettings, typeof(oM.Architecture.BuildersWork.Opening), $"{nameof(oM.Architecture.BuildersWork.Opening.Profile)}.{nameof(RectangleProfile.Width)}");
                Parameter heightParam = instance.LookupParameter(settings.MappingSettings, typeof(oM.Architecture.BuildersWork.Opening), $"{nameof(oM.Architecture.BuildersWork.Opening.Profile)}.{nameof(RectangleProfile.Height)}");
                if (widthParam != null && heightParam != null)
                {
                    double width = widthParam.AsDouble().ToSI(widthParam.Definition.GetDataType());
                    double height = heightParam.AsDouble().ToSI(heightParam.Definition.GetDataType());
                    profile = BH.Engine.Spatial.Create.RectangleProfile(height, width);
                }
            }

            if (profile == null)
                BH.Engine.Base.Compute.RecordWarning($"The profile of the opening could not be extracted from the Revit element because the parameters correspondent to their dimensions could not be found. Revit ElementId: {instance.Id}"
                    + "\nTo link the specific parameter values with opening height/width/diameter, add relevant ParameterMap to RevitSettings.MappingSettings.");

            opening.Profile = profile;

            Parameter depthParam = instance.LookupParameter(settings.MappingSettings, typeof(oM.Architecture.BuildersWork.Opening), nameof(BH.oM.Architecture.BuildersWork.Opening.Depth));
            if (depthParam != null)
            {
                double depth = depthParam.AsDouble().ToSI(depthParam.Definition.GetDataType());
                opening.Depth = depth;
            }
            else
                BH.Engine.Base.Compute.RecordWarning($"The depth of the opening could not be extracted from the Revit element because the correspondent parameter could not be found. Revit ElementId: {instance.Id}"
                    + "\nTo link the specific parameter values with opening depth, add relevant ParameterMap to RevitSettings.MappingSettings.");

            // Revit element type proxy
            RevitTypeFragment typeFragment = null;
            ElementType type = instance.Document.GetElement(instance.GetTypeId()) as ElementType;
            if (type != null)
                typeFragment = type.TypeFragmentFromRevit(settings, refObjects);

            // Set the type fragment
            if (typeFragment != null)
                opening.Fragments.Add(typeFragment);

            //Set identifiers, parameters & custom data
            opening.SetIdentifiers(instance);
            opening.CopyParameters(instance, settings.MappingSettings);
            opening.SetProperties(instance, settings.MappingSettings);

            refObjects.AddOrReplace(instance.Id, opening);
            return opening;
        }

        /***************************************************/
    }
}


