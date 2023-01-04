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

        [Description("Creates Revit Options based on the given properties.")]
        [Input("detailLevel", "Value of the DetailLevel property to be assigned to the created Options.")]
        [Input("includeNonVisible", "Value of the IncludeNonVisibleObjects property to be assigned to the created Options.")]
        [Input("computeReferences", "Value of the ComputeReferences property to be assigned to the created Options.")]
        [Output("options", "Revit Options created based on the input properties.")]
        public static Options Options(ViewDetailLevel detailLevel, bool includeNonVisible = false, bool computeReferences = false)
        {
            Options options = new Options();
            options.DetailLevel = detailLevel;
            options.IncludeNonVisibleObjects = includeNonVisible;
            options.ComputeReferences = computeReferences;
            return options;
        }

        /***************************************************/

        [Description("Creates Revit Options based the given view and properties.")]
        [Input("view", "Revit view, on which the created Options will be based.")]
        [Input("includeNonVisible", "Value of the IncludeNonVisibleObjects property to be assigned to the created Options.")]
        [Input("computeReferences", "Value of the ComputeReferences property to be assigned to the created Options.")]
        [Output("options", "Revit Options created based on the input view and properties.")]
        public static Options Options(View view, bool includeNonVisible = false, bool computeReferences = false)
        {
            if (view == null)
                return null;

            Options options = new Options();
            options.View = view;
            options.DetailLevel = view.DetailLevel;
            options.IncludeNonVisibleObjects = includeNonVisible;
            options.ComputeReferences = computeReferences;
            return options;
        }

        /***************************************************/
    }
}




