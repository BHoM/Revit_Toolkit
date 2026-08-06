/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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

using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using BH.oM.Spatial.Layouts;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        [Description("Builds an ExplicitLayout of pile positions in the oriented cap coordinate system.")]
        public static ExplicitLayout PileFoundationLayout(this PileFoundation pileFoundation, Polyline orientedOutline, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            Polyline outline = pileFoundation.PileCap.PadFoundationOutline();
            (Vector translation, double rotation) = outline.TransformToOriginInXY();
            if (translation == null || double.IsNaN(rotation))
                return null;

            List<Point> points = new List<Point>();
            foreach (Pile pile in pileFoundation.Piles)
            {
                Line line = pile.Location as Line;
                if (line == null)
                    return null;

                Point basePt = line.Start.Z < line.End.Z ? line.Start : line.End;
                Point local = basePt.Translate(translation).Rotate(new Point(), Vector.ZAxis, rotation);
                points.Add(new Point { X = local.X, Y = local.Y, Z = 0 });
            }

            return new ExplicitLayout(points);
        }
    }
}
