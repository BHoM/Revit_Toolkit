/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using System;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****             Public dictionary             ****/
        /***************************************************/

        public static readonly Dictionary<Type, Type[]> UnsupportedAPITypes = new Dictionary<Type, Type[]>
        {
            {
                typeof(CurveElement), new Type[]
                {
                    typeof(CurveByPoints),
                    typeof(DetailCurve),
                    typeof(ModelCurve),
                    typeof(SymbolicCurve),
                    typeof(AreaReinforcementCurve)
                }
            },
            {
                typeof(DetailCurve), new Type[]
                {
                    typeof(DetailArc),
                    typeof(DetailEllipse),
                    typeof(DetailLine),
                    typeof(DetailNurbSpline)
                }
            },
            {
                typeof(Element), new Type[]
                {
                    typeof(CombinableElement)
                }
            },
            {
                typeof(FamilyInstance), new Type[]
                {
                    typeof(AnnotationSymbol),
                    typeof(Mullion),
                    typeof(Panel)
                }
            },
            {
                typeof(FamilySymbol), new Type[]
                {
                    typeof(RoomTagType),
                    typeof(AnnotationSymbolType),
                    typeof(SpaceTagType),
                    typeof(TrussType),
                    typeof(AreaTagType)
                }
            },
            {
                typeof(HostedSweep), new Type[]
                {
                    typeof(Fascia),
                    typeof(Gutter),
                    typeof(SlabEdge)
                }
            },
            {
                typeof(ModelCurve), new Type[]
                {
                    typeof(ModelArc),
                    typeof(ModelEllipse),
                    typeof(ModelHermiteSpline),
                    typeof(ModelLine),
                    typeof(ModelNurbSpline)
                }
            },
            {
                typeof(SpatialElement), new Type[]
                {
                    typeof(Room),
                    typeof(Area),
                    typeof(Space)
                }
            },
            {
                typeof(SpatialElementTag), new Type[]
                {
                    typeof(AreaTag),
                    typeof(RoomTag),
                    typeof(SpaceTag)
                }
            }
        };

        /***************************************************/
    }
}

