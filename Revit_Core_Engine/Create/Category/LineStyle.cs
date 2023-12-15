/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates a new Line Style in a given Revit document. If a style with the same name already exists it will have its color and weight overridden.")]
        [Input("doc", "Revit document to add the Line Style.")]
        [Input("lineStyleName", "Name of the new Line Style.")]
        [Input("lineStyleColor", "Color of the new Line Style.")]
        [Input("lineStyleWeight", "Optional, the weight of the new Line Style.")]
        [Output("lineStyle", "Newly added or overridden Line Style.")]
        public static Category LineStyle(this Document doc, string lineStyleName, Color lineStyleColor, int lineStyleWeight = 1)
        {
            //checking if name of desired line styles exists
            Category result = null;
            Category linesCat = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines);
            CategoryNameMap subCategories = linesCat.SubCategories;
            foreach (Category sub in subCategories)
            {
                if (sub.Name == lineStyleName)
                {
                    result = sub;
                    break;
                }
            }

            //if styles don't exist in model then create them
            if (result == null)
            {
                result = doc.Settings.Categories.NewSubcategory(linesCat, lineStyleName);
                result.LineColor = lineStyleColor;
                result.SetLineWeight(lineStyleWeight, GraphicsStyleType.Projection);
            }
            //else modify its current attributs to match inputs
            else
            {
                result.LineColor = lineStyleColor;
                result.SetLineWeight(lineStyleWeight, GraphicsStyleType.Projection);
            }

            return result;
        }

        /***************************************************/
    }
}
