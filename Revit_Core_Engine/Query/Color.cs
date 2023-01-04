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
using System;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts colour of a given Revit element's face.")]
        [Input("face", "Revit element's face to extract the colour from.")]
        [Input("document", "Revit document, to which the face belongs.")]
        [Output("colour", "Colour of the input Revit element's face.")]
        public static System.Drawing.Color Color(this Face face, Document document)
        {
            Material material = document.GetElement(face.MaterialElementId) as Material;
            if (material != null)
                return System.Drawing.Color.FromArgb((int)(Math.Round((100.0 - material.Transparency) / 100 * 255)), material.Color.Red, material.Color.Green, material.Color.Blue);
            else
                return System.Drawing.Color.Black;
        }

        /***************************************************/

        [Description("Extracts colour of a given Revit curve.")]
        [Input("curve", "Revit curve to extract the colour from.")]
        [Input("document", "Revit document, to which the curve belongs.")]
        [Output("colour", "Colour of the input Revit curve.")]
        public static System.Drawing.Color Color(this Curve curve, Document document)
        {
            GraphicsStyle gs = document.GetElement(curve.GraphicsStyleId) as GraphicsStyle;
            if (gs != null && gs.GraphicsStyleCategory != null)
                return System.Drawing.Color.FromArgb(255, gs.GraphicsStyleCategory.LineColor.Red, gs.GraphicsStyleCategory.LineColor.Green, gs.GraphicsStyleCategory.LineColor.Blue);
            else
                return System.Drawing.Color.Black;
        }

        /***************************************************/
    }
}

