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
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns the transform from the global coordinates system to the view's local one.")]
        [Input("view", "An existing view in the model.")]
        [Output("transform", "the transform from the global coordinates system to the view's local one.")]
        public static Transform ViewTransform(this View view)
        {
            if (view is ViewPlan)
            {
                double angle = XYZ.BasisX.AngleOnPlaneTo(view.RightDirection, XYZ.BasisZ);
                return Transform.CreateRotation(XYZ.BasisZ, angle);
            }
            else if (view is ViewSection || view is View3D)
            {
                double angleInXY = XYZ.BasisX.AngleOnPlaneTo(view.RightDirection, XYZ.BasisZ);
                Transform rotationInXY = Transform.CreateRotation(XYZ.BasisZ, angleInXY);

                double angleInYZ = XYZ.BasisY.AngleOnPlaneTo(rotationInXY.Inverse.OfVector(view.UpDirection), XYZ.BasisX);
                Transform rotationInYZ = Transform.CreateRotation(XYZ.BasisX, angleInYZ);

                return rotationInXY * rotationInYZ;
            }
            else
            {
                return null;
            }
        }

        /***************************************************/
    }
}

