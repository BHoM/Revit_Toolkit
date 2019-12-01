/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using System;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****             Public dictionary             ****/
        /***************************************************/

        public static readonly Dictionary<Type, Type[]> UnsupportedAPITypes = new Dictionary<Type, Type[]>
        {
            {
                typeof(Autodesk.Revit.DB.CurveElement), new Type[]
                {
                    typeof(Autodesk.Revit.DB.CurveByPoints),
                    typeof(Autodesk.Revit.DB.DetailCurve),
                    typeof(Autodesk.Revit.DB.ModelCurve),
                    typeof(Autodesk.Revit.DB.SymbolicCurve),
                    typeof(Autodesk.Revit.DB.Structure.AreaReinforcementCurve)
                }
            },
            {
                typeof(Autodesk.Revit.DB.DetailCurve), new Type[]
                {
                    typeof(Autodesk.Revit.DB.DetailArc),
                    typeof(Autodesk.Revit.DB.DetailEllipse),
                    typeof(Autodesk.Revit.DB.DetailLine),
                    typeof(Autodesk.Revit.DB.DetailNurbSpline)
                }
            },
            {
                typeof(Autodesk.Revit.DB.Element), new Type[]
                {
                    typeof(Autodesk.Revit.DB.CombinableElement)
                }
            },
            {
                typeof(Autodesk.Revit.DB.FamilyInstance), new Type[]
                {
                    typeof(Autodesk.Revit.DB.AnnotationSymbol),
                    typeof(Autodesk.Revit.DB.Mullion),
                    typeof(Autodesk.Revit.DB.Panel)
                }
            },
            {
                typeof(Autodesk.Revit.DB.FamilySymbol), new Type[]
                {
                    typeof(Autodesk.Revit.DB.Architecture.RoomTagType),
                    typeof(Autodesk.Revit.DB.AnnotationSymbolType),
                    typeof(Autodesk.Revit.DB.Mechanical.SpaceTagType),
                    typeof(Autodesk.Revit.DB.Structure.TrussType),
                    typeof(Autodesk.Revit.DB.AreaTagType)
                }
            },
            {
                typeof(Autodesk.Revit.DB.HostedSweep), new Type[]
                {
                    typeof(Autodesk.Revit.DB.Architecture.Fascia),
                    typeof(Autodesk.Revit.DB.Architecture.Gutter),
                    typeof(Autodesk.Revit.DB.SlabEdge)
                }
            },
            {
                typeof(Autodesk.Revit.DB.ModelCurve), new Type[]
                {
                    typeof(Autodesk.Revit.DB.ModelArc),
                    typeof(Autodesk.Revit.DB.ModelEllipse),
                    typeof(Autodesk.Revit.DB.ModelHermiteSpline),
                    typeof(Autodesk.Revit.DB.ModelLine),
                    typeof(Autodesk.Revit.DB.ModelNurbSpline)
                }
            },
            {
                typeof(Autodesk.Revit.DB.SpatialElement), new Type[]
                {
                    typeof(Autodesk.Revit.DB.Architecture.Room),
                    typeof(Autodesk.Revit.DB.Area),
                    typeof(Autodesk.Revit.DB.Mechanical.Space)
                }
            },
            {
                typeof(Autodesk.Revit.DB.SpatialElementTag), new Type[]
                {
                    typeof(Autodesk.Revit.DB.AreaTag),
                    typeof(Autodesk.Revit.DB.Architecture.RoomTag),
                    typeof(Autodesk.Revit.DB.Mechanical.SpaceTag)
                }
            }
        };

        /***************************************************/
    }
}
