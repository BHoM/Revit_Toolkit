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
using BH.Adapter.Revit;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Geometry;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static IBHoMObject ObjectFromRevit(this Element element, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            IBHoMObject iBHoMObject = refObjects.GetValue<IBHoMObject>(element.Id);
            if (iBHoMObject != null)
                return iBHoMObject;

            IGeometry iGeometry = element.Location.IFromRevit();
            if (iGeometry != null)
            {
                ElementType elementType = element.Document.GetElement(element.GetTypeId()) as ElementType;
                if (elementType != null)
                {
                    InstanceProperties objectProperties = elementType.InstancePropertiesFromRevit(settings, refObjects) as InstanceProperties;
                    if (objectProperties != null)
                    {
                        if (element.ViewSpecific)
                            iBHoMObject = BH.Engine.Adapters.Revit.Create.DraftingInstance(objectProperties, element.Document.GetElement(element.OwnerViewId).Name, iGeometry as dynamic);
                        else
                            iBHoMObject = BH.Engine.Adapters.Revit.Create.ModelInstance(objectProperties, iGeometry as dynamic);
                    }
                }
            }

            if (iBHoMObject == null)
                iBHoMObject = new BHoMObject();

            iBHoMObject.Name = element.Name;
            iBHoMObject.SetIdentifiers(element);
            iBHoMObject.CopyParameters(element, settings.ParameterSettings);
            iBHoMObject.SetProperties(element, settings.ParameterSettings);

            refObjects.AddOrReplace(element.Id, iBHoMObject);

            return iBHoMObject;
        }

        /***************************************************/
    }
}

