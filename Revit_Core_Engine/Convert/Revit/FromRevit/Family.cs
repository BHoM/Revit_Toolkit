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
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit Family to BH.oM.Adapters.Revit.Elements.Family.")]
        [Input("revitFamily", "Revit Family to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("family", "BH.oM.Adapters.Revit.Elements.Family resulting from converting the input Revit Family.")]
        public static oM.Adapters.Revit.Elements.Family FamilyFromRevit(this Family revitFamily, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            oM.Adapters.Revit.Elements.Family family = refObjects.GetValue<oM.Adapters.Revit.Elements.Family>(revitFamily.Id);
            if (family != null)
                return family;

            family = new oM.Adapters.Revit.Elements.Family();
            family.Name = revitFamily.Name;

            IEnumerable<ElementId> elementIDs = revitFamily.GetFamilySymbolIds();
            if (elementIDs != null)
            {
                foreach (ElementId elementID in elementIDs)
                {
                    if (elementID == null || elementID == ElementId.InvalidElementId)
                        continue;

                    ElementType elementType = revitFamily.Document.GetElement(elementID) as ElementType;
                    if (elementType == null)
                        continue;

                    InstanceProperties instanceProperties = elementType.InstancePropertiesFromRevit(settings, refObjects);
                    if (instanceProperties == null)
                        continue;

                    family.PropertiesList.Add(instanceProperties);
                }
            }

            //Set identifiers, parameters & custom data
            family.SetIdentifiers(revitFamily);
            family.CopyParameters(revitFamily, settings.MappingSettings);
            family.SetProperties(revitFamily, settings.MappingSettings);

            refObjects.AddOrReplace(revitFamily.Id, family);
            return family;
        }

        /***************************************************/
    }
}


