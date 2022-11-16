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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Physical.Elements;
using BH.oM.Physical.FramingProperties;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Adapters.Revit;
using System.Linq;
using BH.Engine.Geometry;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit FamilyInstance to BH.oM.Physical.Elements.Beam.")]
        [Input("familyInstance", "Revit FamilyInstance to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("beam", "BH.oM.Physical.Elements.Beam resulting from converting the input Revit FamilyInstance.")]
        public static MeshElement MeshElementFromRevit(this Element element, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            MeshElement meshElement = refObjects.GetValue<MeshElement>(element.Id);
            if (meshElement != null)
                return meshElement;

            Options meshOptions = Create.Options(ViewDetailLevel.Medium, true, true);

            meshElement = new MeshElement();

            List<oM.Geometry.Mesh> meshes = element.MeshedGeometry(meshOptions, settings);

            if (meshes != null && meshes.Any())
                meshElement.Mesh = meshes.Join(false);

            //Set identifiers, parameters & custom data
            meshElement.SetIdentifiers(element);
            meshElement.CopyParameters(element, settings.MappingSettings);
            meshElement.SetProperties(element, settings.MappingSettings);

            refObjects.AddOrReplace(element.Id, meshElement);
            return meshElement;
        }

        /***************************************************/


    }
}


