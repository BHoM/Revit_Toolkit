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
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates a Revit CategorySet based on the given collection of Categories.")]
        [Input("document", "Revit document, in which the new CategorySet will be created.")]
        [Input("categories", "Collection of Revit Categories to be wrapped into a CategorySet.")]
        [Output("categorySet", "Revit CategorySet created based on the input collection of Categories.")]
        public static CategorySet CategorySet(Document document, IEnumerable<Category> categories)
        {
            CategorySet result = document.Application.Create.NewCategorySet();
            
            foreach (Category category in categories)
            {
                if (category.AllowsBoundParameters)
                    result.Insert(category);
            }

            return result;
        }

        /***************************************************/
    }
}



