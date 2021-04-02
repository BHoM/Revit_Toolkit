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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static ModelInstance ModelInstanceFromRevit(this FamilyInstance adaptiveComponent, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            ModelInstance modelInstance = refObjects.GetValue<ModelInstance>(adaptiveComponent.Id);
            if (modelInstance != null)
                return modelInstance;
            
            ElementType elementType = adaptiveComponent.Document.GetElement(adaptiveComponent.GetTypeId()) as ElementType;
            InstanceProperties instanceProperties = elementType.InstancePropertiesFromRevit(settings, refObjects) as InstanceProperties;

            IEnumerable<BH.oM.Geometry.Point> pts = AdaptiveComponentInstanceUtils.GetInstancePointElementRefIds(adaptiveComponent).Select(x => ((ReferencePoint)adaptiveComponent.Document.GetElement(x)).Position.PointFromRevit());
            modelInstance = new ModelInstance { Properties = instanceProperties, Location = new CompositeGeometry { Elements = new List<IGeometry>(pts) } };
            modelInstance.Name = adaptiveComponent.Name;

            //Set identifiers, parameters & custom data
            modelInstance.SetIdentifiers(adaptiveComponent);
            modelInstance.CopyParameters(adaptiveComponent, settings.ParameterSettings);
            modelInstance.SetProperties(adaptiveComponent, settings.ParameterSettings);

            refObjects.AddOrReplace(adaptiveComponent.Id, modelInstance);
            return modelInstance;
        }

        /***************************************************/
    }
}


