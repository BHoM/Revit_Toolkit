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

        [Description("Creates a Revit ReferencePlane based on the given origin and orientation.")]
        [Input("document", "Revit document, in which the new ReferencePlane will be created.")]
        [Input("point", "Origin of the created ReferencePlane.")]
        [Input("orientation", "Orientation of the created ReferencePlane.")]
        [Input("name", "Name of the created ReferencePlane.")]
        [Output("referencePlane", "Revit ReferencePlane created based on the input origin and orientation.")]
        public static ReferencePlane ReferencePlane(Document document, XYZ point, Transform orientation, string name = "")
        {
            if (point == null || orientation == null)
                return null;

            XYZ dir1 = orientation.BasisZ.CrossProduct(XYZ.BasisZ);
            XYZ dir2 = orientation.BasisZ.CrossProduct(dir1);
            ReferencePlane rp = document.Create.NewReferencePlane(point, point + dir1, dir2, document.ActiveView);
            if (rp.Normal.DotProduct(orientation.BasisZ) < 0)
                rp.Flip();

            if (!string.IsNullOrWhiteSpace(name))
                rp.Name = name;

            return rp;
        }

        /***************************************************/
    }
}




