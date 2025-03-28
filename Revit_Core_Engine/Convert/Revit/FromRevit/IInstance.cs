/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit CurveElement to a BHoM IInstance, either ModelInstance or DraftingInstance, depending on whether the Revit element is view specific.")]
        [Input("curveElement", "Revit CurveElement to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("instance", "BHoM object resulting from converting the given Revit CurveElement.")]
        public static IInstance InstanceFromRevit(this CurveElement curveElement, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            IInstance instance = refObjects.GetValue<ModelInstance>(curveElement.Id);
            if (instance != null)
                return instance;

            InstanceProperties instanceProperties = (curveElement.LineStyle as GraphicsStyle).InstancePropertiesFromRevit(settings, refObjects) as InstanceProperties;

            if (curveElement.ViewSpecific)
            {
                View view = curveElement.Document.GetElement(curveElement.OwnerViewId) as View;
                if (view == null)
                    return null;

                instance = BH.Engine.Adapters.Revit.Create.DraftingInstance(instanceProperties, view.Name, curveElement.GeometryCurve.IFromRevit());
            }
            else
                instance = BH.Engine.Adapters.Revit.Create.ModelInstance(instanceProperties, curveElement.GeometryCurve.IFromRevit());

            instance.Name = curveElement.Name;

            //Set identifiers, parameters & custom data
            instance.SetIdentifiers(curveElement);
            instance.CopyParameters(curveElement, settings.MappingSettings);
            instance.SetProperties(curveElement, settings.MappingSettings);

            refObjects.AddOrReplace(curveElement.Id, instance);
            return instance;
        }

        /***************************************************/
    }
}






