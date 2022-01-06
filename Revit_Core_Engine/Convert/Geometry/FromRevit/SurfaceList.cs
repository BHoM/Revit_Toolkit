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
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a list of Revit Faces to a list of BH.oM.Geometry.ISurfaces.")]
        [Input("faces", "List of Revit Faces to be converted.")]
        [Output("surfaces", "List of BH.oM.Geometry.ISurfaces resulting from converting the input list of Revit Faces.")]
        public static List<oM.Geometry.ISurface> FromRevit(this List<Face> faces)
        {
            if (faces == null)
                return null;

            return faces.Select(f => f.IFromRevit()).ToList();
        }

        /***************************************************/

        [Description("Converts a Revit FaceArray to a list of BH.oM.Geometry.ISurfaces.")]
        [Input("faceArray", "Revit FaceArray to be converted.")]
        [Output("surfaces", "List of BH.oM.Geometry.ISurfaces resulting from converting the Revit FaceArray.")]
        public static List<oM.Geometry.ISurface> FromRevit(this FaceArray faceArray)
        {
            if (faceArray == null)
                return null;

            List<oM.Geometry.ISurface> result = new List<oM.Geometry.ISurface>();
            foreach (Face face in faceArray)
            {        
                result.Add(face.IFromRevit());
            }

            return result;
        }

        /***************************************************/
    }
}


