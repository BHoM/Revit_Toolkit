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
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the material that covers largest area of a given facade opening represented by FamilyInstance." +
                     "\nLargest area is meant to be glazing or spandrel, but may give inconsistent results for elements with very low glazing to frame area ratio.")]
        [Input("opening", "Facade opening to query for dominating material.")]
        [Output("material", "Material that covers largest area of the input FamilyInstance representing a facade opening.")]
        public static Material FacadeOpeningMaterial(this FamilyInstance opening)
        {
            List<Solid> solids = opening?.Solids(new Options());
            if (solids == null)
                return null;

            Face maxAreaFace = null;
            foreach (Solid solid in solids)
            {
                foreach (Face face in solid.Faces)
                {
                    if (maxAreaFace == null || maxAreaFace.Area < face.Area)
                        maxAreaFace = face;
                }
            }

            return maxAreaFace != null ? opening.Document.GetElement(maxAreaFace.MaterialElementId) as Material : null;
        }

        /***************************************************/
    }
}

