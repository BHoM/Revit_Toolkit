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
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BH.oM.Geometry.ICurve AdjustedLocation(this BH.oM.Physical.Elements.IFramingElement framingElement)
        {
            //TODO: add the geometrical properties to RefObjects!

            Line locationLine = framingElement.Location as Line;
            if (locationLine == null || locationLine.Length() < BH.oM.Geometry.Tolerance.Distance)
            {
                //TODO: warning
                return framingElement.Location;
            }

            if (!framingElement.CustomData.ContainsKey("yz Justification") || !framingElement.CustomData.ContainsKey("Cross-Section Rotation"))
                return framingElement.Location;

            BH.oM.Physical.FramingProperties.ConstantFramingProperty property = framingElement.Property as BH.oM.Physical.FramingProperties.ConstantFramingProperty;
            if (framingElement.Property == null)
            {
                //TODO: warning
                return framingElement.Location;
            }

            BH.oM.Geometry.ShapeProfiles.IProfile profile = property.Profile;
            if (profile == null)
            {
                //TODO: warning
                return framingElement.Location;
            }

            List<PolyCurve> outlines = BH.Engine.Geometry.Compute.IJoin(profile.Edges.ToList());
            if (outlines.Count != 1)
            {
                //TODO: warning
                return framingElement.Location;
            }

            PolyCurve profileCurve = outlines.FirstOrDefault();
            if (profileCurve == null || !profileCurve.IIsClosed())
            {
                //TODO: warning
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
                    //TODO: raise warning
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
                //independent
                throw new NotImplementedException();
            }
        }

        /***************************************************/
    }
}