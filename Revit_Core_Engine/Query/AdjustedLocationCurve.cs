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
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using BH.oM.Base;
using System;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        [Description("Finds the location curve that should be assigned to the Revit FamilyInstance representing a framing element in order to make this instance's centroid overlap with the centreline of a given BHoM framing element, taken all offsets and justifications into account.")]
        [Input("framingElement", "BHoM framing element to align the Revit framing to.")]
        [Input("familyInstance", "Revit FamilyInstance to align to the BHoM framing element.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("curve", "Location curve for the input Revit FamilyInstance that aligns its centreline to the input BHoM framing element.")]
        public static ICurve AdjustedLocationCurveFraming(this IFramingElement framingElement, FamilyInstance familyInstance, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            ICurve curve = framingElement?.Location;
            if (curve == null || (!(curve is NurbsCurve) && curve.ILength() <= settings.DistanceTolerance))
            {
                framingElement.FramingCurveNotFoundWarning();
                return null;
            }

            BH.oM.Geometry.Line line = curve as BH.oM.Geometry.Line;
            if (line == null)
            {
                familyInstance.NonLinearFramingOffsetWarning();
                return curve;
            }

            Output<Vector, Vector> offsets = familyInstance.FramingOffsetVectors();
            Vector startOffset = -offsets.Item1;
            Vector endOffset = -offsets.Item2;

            double startOffsetLength = startOffset.Length();
            double endOffsetLength = endOffset.Length();
            if (startOffsetLength > settings.DistanceTolerance || endOffsetLength > settings.DistanceTolerance)
            {
                if ((startOffset - endOffset).Length() > settings.DistanceTolerance)
                {
                    BH.Engine.Base.Compute.RecordError(String.Format("Adjusted location curve of a Revit framing element could not be found because it has non-uniform offsets at ends. Revit ElementId: {0}", familyInstance.Id));
                    return null;
                }

                Transform transform = familyInstance.GetTotalTransform();
                Vector yOffsetStart = new Vector { X = transform.BasisY.X * startOffset.Y, Y = transform.BasisY.Y * startOffset.Y, Z = transform.BasisY.Z * startOffset.Y };
                Vector zOffsetStart = new Vector { X = transform.BasisZ.X * startOffset.Z, Y = transform.BasisZ.Y * startOffset.Z, Z = transform.BasisZ.Z * startOffset.Z };
                Vector yOffsetEnd = new Vector { X = transform.BasisY.X * endOffset.Y, Y = transform.BasisY.Y * endOffset.Y, Z = transform.BasisY.Z * endOffset.Y };
                Vector zOffsetEnd = new Vector { X = transform.BasisZ.X * endOffset.Z, Y = transform.BasisZ.Y * endOffset.Z, Z = transform.BasisZ.Z * endOffset.Z };
                line = new BH.oM.Geometry.Line { Start = line.Start.Translate(yOffsetStart + zOffsetStart), End = line.End.Translate(yOffsetEnd + zOffsetEnd) };
            }

            Vector dir = line.Direction();
            double startExtension = familyInstance.LookupParameterDouble(BuiltInParameter.START_EXTENSION);
            if (!double.IsNaN(startExtension))
                line.Start = line.Start + dir * startExtension;

            double endExtension = familyInstance.LookupParameterDouble(BuiltInParameter.END_EXTENSION);
            if (!double.IsNaN(endExtension))
                line.End = line.End - dir * endExtension;

            return line;
        }

        /***************************************************/
    }
}
