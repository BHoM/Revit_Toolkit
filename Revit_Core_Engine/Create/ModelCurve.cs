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
using BH.Engine.Geometry;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates ModelCurve based on a given Curve.")]
        [Input("document", "Revit document, in which the new ModelCurve will be created.")]
        [Input("curve", "Curve, based on which ModelCurve will be created.")]
        [Output("modelCurve", "ModelCurve created based on the input curve.")]
        public static ModelCurve ModelCurve(Document document, Curve curve)
        {
            oM.Geometry.ICurve bhomCurve = curve.IFromRevit();
            Plane revitPlane;
            BH.oM.Geometry.Plane plane = bhomCurve.IFitPlane();
            if (plane == null)
            {
                XYZ point = curve.GetEndPoint(0);
                XYZ vector = (curve.GetEndPoint(1) - point).Normalize();
                revitPlane = Create.ArbitraryPlane(point, vector);
            }
            else
                revitPlane = plane.ToRevit();

            SketchPlane sketchPlane = Create.SketchPlane(document, revitPlane);
            return document.Create.NewModelCurve(curve, sketchPlane);
        }

        /***************************************************/
    }
}
