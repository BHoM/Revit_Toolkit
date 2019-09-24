/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using BH.Engine.Geometry;
using BH.oM.Base;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BH.oM.Geometry.ICurve AdjustedLocation(this IFramingElement framingElement)
        {
            if (framingElement.Location == null || framingElement.Location.ILength() < BH.oM.Geometry.Tolerance.Distance)
            {
                //BH.Engine.Reflection.Compute.RecordWarning(string.Format("The framing element has zero length or no curve assigned. BHoM Guid: {0}", framingElement.BHoM_Guid));
                return framingElement.Location;
            }

            //TODO: CROSS SECTION ROTATION NOT FROM HERE, FROM PROPERTY!
            if (!framingElement.CustomData.ContainsKey("yz Justification") || !framingElement.CustomData.ContainsKey("Cross-Section Rotation"))
                return framingElement.Location;

            Line locationLine = framingElement.Location as Line;
            if (locationLine == null)
            {
                //BH.Engine.Reflection.Compute.RecordWarning(string.Format("Offset/justification of nonlinear bars is currently not supported. Revit justification and offset has been ignored. BHoM Guid: {0}", framingElement.BHoM_Guid));
                return framingElement.Location;
            }

            BH.oM.Physical.FramingProperties.ConstantFramingProperty property = framingElement.Property as BH.oM.Physical.FramingProperties.ConstantFramingProperty;
            if (property == null || property.Profile == null)
            {
                //BH.Engine.Reflection.Compute.RecordWarning(string.Format("The framing element does not have a profile. BHoM Guid: {0}", framingElement.BHoM_Guid));
                return framingElement.Location;
            }

            List<PolyCurve> outlines = BH.Engine.Geometry.Compute.IJoin(property.Profile.Edges.ToList());
            PolyCurve profileCurve = outlines.FirstOrDefault();
            if (outlines.Count != 1 || profileCurve == null || !profileCurve.IIsClosed())
            {
                //BH.Engine.Reflection.Compute.RecordWarning(string.Format("Framing element's profile is not supported or incorrect. BHoM Guid: {0}", framingElement.BHoM_Guid));
                return framingElement.Location;
            }
            
            BoundingBox bbox = profileCurve.Bounds();
            double dY = (bbox.Max.X - bbox.Min.X) * 0.5;
            double dZ = (bbox.Max.Y - bbox.Min.Y) * 0.5;
            Point centre = (bbox.Min + bbox.Max) * 0.5;

            double rotation = (double)framingElement.CustomData["Cross-Section Rotation"];
            Vector xDir = locationLine.Direction();
            Vector yDir = 1 - Math.Abs(xDir.DotProduct(Vector.ZAxis)) < BH.oM.Geometry.Tolerance.Angle ? Vector.XAxis.CrossProduct(xDir) : Vector.ZAxis.CrossProduct(xDir).Normalise().Rotate(-rotation, xDir);
            Vector zDir = yDir.CrossProduct(xDir);

            int yzJustification = (int)framingElement.CustomData["yz Justification"];
            if (yzJustification == 0)
            {
                if (!framingElement.CustomData.ContainsKey("y Justification") || !framingElement.CustomData.ContainsKey("z Justification"))
                {
                    //BH.Engine.Reflection.Compute.RecordWarning(string.Format("The framing element does not have justification properties. BHoM Guid: {0}", framingElement.BHoM_Guid));
                    return framingElement.Location;
                }

                int yJustification = (int)framingElement.CustomData["y Justification"];
                int zJustification = (int)framingElement.CustomData["z Justification"];

                double yAdjustment = 0;
                if (yJustification == 0)
                    yAdjustment = centre.Y + dY;
                else if (yJustification == 1)
                    yAdjustment = centre.Y;
                else if (yJustification == 3)
                    yAdjustment = centre.Y - dY;

                double zAdjustment = 0;
                if (zJustification == 0)
                    zAdjustment = centre.Z - dZ;
                else if (zJustification == 1)
                    zAdjustment = centre.Z;
                else if (zJustification == 3)
                    zAdjustment = centre.Z + dZ;

                return locationLine.Translate(yDir * yAdjustment + zDir * zAdjustment);
            }
            else
            {
                if (!framingElement.CustomData.ContainsKey("Start y Justification") || !framingElement.CustomData.ContainsKey("Start z Justification") || !framingElement.CustomData.ContainsKey("End y Justification") || !framingElement.CustomData.ContainsKey("End z Justification"))
                {
                    //BH.Engine.Reflection.Compute.RecordWarning(string.Format("The framing element does not have justification properties. BHoM Guid: {0}", framingElement.BHoM_Guid));
                    return framingElement.Location;
                }

                int startYJustification = (int)framingElement.CustomData["Start y Justification"];
                int startZJustification = (int)framingElement.CustomData["Start z Justification"];
                int endYJustification = (int)framingElement.CustomData["End y Justification"];
                int endZJustification = (int)framingElement.CustomData["End z Justification"];

                double startYAdjustment = 0;
                if (startYJustification == 0)
                    startYAdjustment = centre.Y + dY;
                else if (startYJustification == 1)
                    startYAdjustment = centre.Y;
                else if (startYJustification == 3)
                    startYAdjustment = centre.Y - dY;

                double startZAdjustment = 0;
                if (startZJustification == 0)
                    startZAdjustment = centre.Z - dZ;
                else if (startZJustification == 1)
                    startZAdjustment = centre.Z;
                else if (startZJustification == 3)
                    startZAdjustment = centre.Z + dZ;

                double endYAdjustment = 0;
                if (endYJustification == 0)
                    endYAdjustment = centre.Y + dY;
                else if (endYJustification == 1)
                    endYAdjustment = centre.Y;
                else if (endYJustification == 3)
                    endYAdjustment = centre.Y - dY;

                double endZAdjustment = 0;
                if (endZJustification == 0)
                    endZAdjustment = centre.Z - dZ;
                else if (endZJustification == 1)
                    endZAdjustment = centre.Z;
                else if (endZJustification == 3)
                    endZAdjustment = centre.Z + dZ;

                return new Line { Start = locationLine.Start.Translate(yDir * startYAdjustment + zDir * startZAdjustment), End = locationLine.End.Translate(yDir * endYAdjustment + zDir * endZAdjustment) };
            }
        }

        /***************************************************/
    }
}