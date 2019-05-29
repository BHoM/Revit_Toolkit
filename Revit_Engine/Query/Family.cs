/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using System.ComponentModel;

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Reflection.Attributes;
using BH.oM.Adapters.Revit.Generic;

using System.Collections.Generic;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns Family based on RevitFilePreview and sepcified Family Type Names")]
        [Input("revitFilePreview", "RevitFilePreview")]
        [Input("FamilyTypeNames", "Family Type Names.")]
        [Output("Family")]
        public static Family Family(this RevitFilePreview revitFilePreview, IEnumerable<string> FamilyTypeNames)
        {
            if (revitFilePreview == null)
                return null;

            string aFamilyName = revitFilePreview.FamilyName();
            if (string.IsNullOrEmpty(aFamilyName))
                return null;

            string aCategoryName = revitFilePreview.CategoryName();

            List<oM.Adapters.Revit.Properties.InstanceProperties> aInstancePropertiesList = new List<oM.Adapters.Revit.Properties.InstanceProperties>();

            List<string> aFamilyTypeNameList = revitFilePreview.FamilyTypeNames();
            if (aFamilyTypeNameList != null)
            {
                foreach (string aFamilyTypeName in aFamilyTypeNameList)
                {
                    if (FamilyTypeNames != null && !FamilyTypeNames.Contains(aFamilyTypeName))
                        continue;

                    oM.Adapters.Revit.Properties.InstanceProperties aInstanceProperties = Create.InstanceProperties(aFamilyName, aFamilyTypeName);
                    aInstanceProperties = aInstanceProperties.UpdateCustomDataValue(Convert.CategoryName, aCategoryName) as oM.Adapters.Revit.Properties.InstanceProperties;
                    aInstancePropertiesList.Add(aInstanceProperties);
                }
            }

            Family aFamily = new Family()
            {
                PropertiesList = aInstancePropertiesList
            };

            aFamily = aFamily.UpdateCustomDataValue(Convert.FamilyName, aFamilyName) as Family;
            aFamily = aFamily.UpdateCustomDataValue(Convert.CategoryName, aCategoryName) as Family;

            aFamily = aFamily.UpdateCustomDataValue(Convert.FamilyPlacementTypeName, revitFilePreview.CustomDataValue(Convert.FamilyPlacementTypeName)) as Family;

            return aFamily;
        }

        /***************************************************/

        [Description("Returns Family based on RevitFilePreview")]
        [Input("revitFilePreview", "RevitFilePreview")]
        [Output("Family")]
        public static Family Family(this RevitFilePreview revitFilePreview)
        {
            if (revitFilePreview == null)
                return null;

            return Family(revitFilePreview, null);
        }

        /***************************************************/
    }
}

