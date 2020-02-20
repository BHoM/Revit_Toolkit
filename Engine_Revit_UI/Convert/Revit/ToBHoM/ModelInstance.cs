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
using BH.oM.Base;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Adapters.Revit.Elements.ModelInstance ToBHoMModelInstance(this CurveElement curveElement, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            oM.Adapters.Revit.Elements.ModelInstance modelInstance = refObjects.GetValue<oM.Adapters.Revit.Elements.ModelInstance>(curveElement.Id);
            if (modelInstance != null)
                return modelInstance;

            oM.Adapters.Revit.Properties.InstanceProperties instanceProperties = (curveElement.LineStyle as GraphicsStyle).ToBHoMInstanceProperties(settings, refObjects) as oM.Adapters.Revit.Properties.InstanceProperties;
            modelInstance = BH.Engine.Adapters.Revit.Create.ModelInstance(instanceProperties, curveElement.GeometryCurve.IToBHoM());

            modelInstance.Name = curveElement.Name;

            //Set identifiers & custom data
            modelInstance.SetIdentifiers(curveElement);
            modelInstance.SetCustomData(curveElement);

            modelInstance.UpdateValues(settings, curveElement);

            refObjects.AddOrReplace(curveElement.Id, modelInstance);
            return modelInstance;
        }

        /***************************************************/
    }
}

