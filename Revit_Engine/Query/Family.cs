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

using BH.Adapter.Revit;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Generic;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns BHoM family wrapper based on RevitFilePreview and given Revit family type names.")]
        [Input("revitFilePreview", "RevitFilePreview to be queried.")]
        [Input("familyTypeNames", "Revit family type names sought for.")]
        [Output("family")]
        public static Family Family(this RevitFilePreview revitFilePreview, IEnumerable<string> familyTypeNames)
        {
            if (revitFilePreview == null)
                return null;

            string familyName = revitFilePreview.FamilyName();
            if (string.IsNullOrEmpty(familyName))
                return null;

            string categoryName = revitFilePreview.CategoryName();

            List<oM.Adapters.Revit.Properties.InstanceProperties> instanceProperties = new List<oM.Adapters.Revit.Properties.InstanceProperties>();

            List<string> familyTypeNameList = revitFilePreview.FamilyTypeNames();
            if (familyTypeNameList != null)
            {
                foreach (string familyTypeName in familyTypeNameList)
                {
                    if (familyTypeNames != null && !familyTypeNames.Contains(familyTypeName))
                        continue;

                    oM.Adapters.Revit.Properties.InstanceProperties instanceProps = Create.InstanceProperties(familyName, familyTypeName);
                    instanceProps.CustomData[RevitAdapter.CategoryName] = categoryName;
                    instanceProperties.Add(instanceProps);
                }
            }

            Family family = new Family()
            {
                PropertiesList = instanceProperties
            };

            family.CustomData[RevitAdapter.FamilyName] = familyName;
            family.CustomData[RevitAdapter.CategoryName] = categoryName;
            family.CustomData[RevitAdapter.FamilyPlacementTypeName] = revitFilePreview.CustomDataValue(RevitAdapter.FamilyPlacementTypeName);

            return family;
        }

        /***************************************************/

        [Description("Returns BHoM family wrapper based on RevitFilePreview.")]
        [Input("revitFilePreview", "RevitFilePreview to be queried.")]
        [Output("family")]
        public static Family Family(this RevitFilePreview revitFilePreview)
        {
            if (revitFilePreview == null)
                return null;

            return Family(revitFilePreview, null);
        }

        /***************************************************/
    }
}


