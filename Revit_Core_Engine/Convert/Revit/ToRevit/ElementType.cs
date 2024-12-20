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
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts BH.oM.Adapters.Revit.Properties.InstanceProperties to a Revit ElementType.")]
        [Input("instanceProperties", "BH.oM.Adapters.Revit.Properties.InstanceProperties to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("elementType", "Revit ElementType resulting from converting the input BH.oM.Adapters.Revit.Properties.InstanceProperties.")]
        public static ElementType ToRevitElementType(this InstanceProperties instanceProperties, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            ElementType elementType = refObjects.GetValue<ElementType>(document, instanceProperties.BHoM_Guid);
            if (elementType != null)
                return elementType;

            settings = settings.DefaultIfNull();

            elementType = instanceProperties.ElementType(document, new List<BuiltInCategory> { instanceProperties.BuiltInCategory() }, settings);
            if (elementType == null)
                return null;

            // Copy parameters from BHoM object to Revit element
            elementType.CopyParameters(instanceProperties, settings);

            refObjects.AddOrReplace(instanceProperties, elementType);
            return elementType;
        }

        /***************************************************/

        [Description("Converts BH.oM.Adapters.Revit.ClonedType to a Revit ElementType.")]
        [Input("clonedType", "BH.oM.Adapters.Revit.ClonedType to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("elementType", "Revit ElementType resulting from converting the input BH.oM.Adapters.Revit.ClonedType.")]
        public static ElementType ToRevitElementType(this ClonedType clonedType, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            ElementType elementType = refObjects.GetValue<ElementType>(document, clonedType.BHoM_Guid);
            if (elementType != null)
                return elementType;

            settings = settings.DefaultIfNull();

            Element source = document.GetElement(new ElementId(clonedType.SourceTypeId));
            if (source == null)
            {
                BH.Engine.Base.Compute.RecordError($"The element with ElementId {clonedType.SourceTypeId} does not exist in the active Revit document.");
                return null;
            }

            ElementType sourceType = source as ElementType;
            if (sourceType == null)
            {
                BH.Engine.Base.Compute.RecordError($"The element with ElementId {clonedType.SourceTypeId} exists in the active Revit document, but it is not an element type.");
                return null;
            }

            elementType = sourceType.Duplicate(clonedType.Name);

            // Copy parameters from BHoM object to Revit element
            elementType.CopyParameters(clonedType, settings);

            refObjects.AddOrReplace(clonedType, elementType);
            return elementType;
        }

        /***************************************************/
    }
}





