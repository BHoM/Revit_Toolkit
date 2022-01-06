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

using Autodesk.Revit.DB.Structure.StructuralSections;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static StructuralSectionShape SectionShape(this string familyName)
        {
            foreach (KeyValuePair<StructuralSectionShape, string[]> shapeNameEndingSet in ProfileShapeNameEndings)
            {
                if (shapeNameEndingSet.Value.Any(x => familyName.EndsWith(x)))
                    return shapeNameEndingSet.Key;
            }

            return StructuralSectionShape.NotDefined;
        }

        /***************************************************/

        private static readonly Dictionary<StructuralSectionShape, string[]> ProfileShapeNameEndings = new Dictionary<StructuralSectionShape, string[]>()
        {
            {
                StructuralSectionShape.ConcreteRectangle, new string[]
                {
                    "_Precast-RectangularColumnWithCL",
                    "_Precast-SquareColumn",
                    "_Precast-SquareColumnWithCL",
                    "_ConcreteRectangular-Precast",
                    "_ConcreteRectangular-PrecastWithCL",
                    "_Precast-RectangularColumn",
                    "_Precast-RectangularBeam",
                    "_ConcreteSquareWithCL",
                    "_ConcreteSquare",
                    "_ConcreteRectangularWithCL",
                    "_ConcreteRectangular",
                    "_Concrete-RectangularBeam"
                }
            },
            {
                StructuralSectionShape.RectangleParameterized, new string[]
                {
                    "_TimberWithCL",
                    "_DimensionLumberWithCL",
                    "_DimensionLumber",
                    "_Glulam(2)WithCL",
                    "_Glulam(2)",
                    "_Glulam(1)WithCL",
                    "_Glulam(1)",
                    "_ParallelStrandLumber",
                    "_ParallelStrandLumberWithCL",
                    "_LaminatedVeneerLumber",
                    "BHm_StructuralFraming_Timber",
                    "BHm_StructuralColumns_Timber"
                }
            },
            {
                StructuralSectionShape.RectangularBar, new string[]
                {
                    "_Plate"
                }
            },
            {
                StructuralSectionShape.IWideFlange, new string[]
                {
                    "_W-WideFlange-Column",
                    "_M-MiscellaneousWideFlange-Column",
                    "_H-WideFlange-Column",
                    "_H-WideFlangeBeams",
                    "_UKC-UKColumns-Column",
                    "_UKC-UKColumns",
                    "_UC-UniversalColumns-Column",
                    "_UC-UniversalColumns"
                }
            },
            {
                StructuralSectionShape.IParallelFlange, new string[]
                {
                    //"_RSJ-RolledSteelJoists-Column",
                    //"_RSJ-RolledSteelJoists",
                    "_HP-BearingPile-Column",
                    "_IPE-Column",
                    "_IPE-Beams",
                    //"_IPN-Column",
                    //"_IPN-Beams",
                    "_UKBP-UKBearingPiles-Column",
                    "_UKBP-UKBearingPiles",
                    "_UKB-UKBeams-Column",
                    "_UKB-UKBeams",
                    //"_ASB-Beams",
                    "_UBP-UniversalBearingPile-Column",
                    "_UBP-UniversalBearingPile",
                    "_UB-UniversalBeams-Column",
                    "_UB-UniversalBeams"
                }
            },
            {
                StructuralSectionShape.RoundBar, new string[]
                {
                    "_RoundBar"
                }
            },
            {
                StructuralSectionShape.ConcreteRound, new string[]
                {
                    "_ConcreteRoundWithCL",
                    "_ConcreteRound"
                }
            },
            {
                StructuralSectionShape.IWelded, new string[]
                {
                    "_WRF-WeldedReducedFlange-Column",
                    "_WWF-WeldedWideFlange-Column",
                    "_Plate-Column",
                    "_WeldedReducedFlange",
                    "_WeldedWideFlange",
                    "_PlateGirder",

                }
            },
            {
                StructuralSectionShape.LAngle, new string[]
                {
                    "_UKA-UKAngles-Column",
                    "_UKA-UKAngles",
                    "_L-EqualLegAngles-Column",
                    "_L-EqualLegAngles",
                    "_L-UnequalLegAngles-Column",
                    "_L-UnequalLegAngles",
                    "_L-Angles"
                }
            },
            {
                StructuralSectionShape.CProfile, new string[]
                {
                    //"_U-Channels",
                    //"_C-Channels"
                }
            },
            {
                StructuralSectionShape.CParallelFlange, new string[]
                {
                    "_U-ParallelFlangeChannels",
                    "_PFC-ParallelFlangeChannels-Column",
                    "_PFC-ParallelFlangeChannels",
                    "_UKPFC-ParallelFlangeChannels-Column",
                    "_UKPFC-ParallelFlangeChannels"
                }
            },
            {
                StructuralSectionShape.ConcreteT, new string[]
                {
                    //"_Precast-SingleTee"
                }
            },
            {
                StructuralSectionShape.StructuralTees, new string[]
                {
                    //"_T-Tees"
                }
            },
            {
                StructuralSectionShape.ISplitParallelFlange, new string[]
                {
                    "_MIPE-TeesfromIPE",
                    "_MH-TeesfromH-Beams",
                    "_T-TeesfromUniversalBeams-Column",
                    "_T-TeesfromUniversalBeams",
                    "_T-TeesfromUniversalColumns-Column",
                    "_T-TeesfromUniversalColumns",
                    "_UKT-UKTeesSplitfromUKB-Column",
                    "_UKT-UKTeesSplitfromUKB",
                    "_UKT-UKTeesSplitfromUKC-Column",
                    "_UKT-UKTeesSplitfromUKC"
                }
            },
            {
                StructuralSectionShape.RectangleHSS, new string[]
                {
                    "_RectangularandSquareHollowSections-Column",
                    "_RectangularandSquareHollowSections",
                    "_RHS-RectangularHollowSections-Column",
                    "_RHS-RectangularHollowSections",
                    "_RHS-RectangularHollowSections-Column(Cold)",
                    "_RHS-RectangularHollowSections(Cold)",
                    "_SHS-SquareHollowSections-Column",
                    "_SHS-SquareHollowSections",
                    "_SHS-SquareHollowSections-Column(Cold)",
                    "_SHS-SquareHollowSections(Cold)",
                    "_RectangularHollowSections-Column",
                    "_RectangularHollowSections",
                    "_SquareHollowSections-Column",
                    "_SquareHollowSections"
                }
            },
            {
                StructuralSectionShape.RoundHSS, new string[]
                {
                    "_CircularHollowSections",
                    "_CircularHollowSections-Column",
                    "_CHS-CircularHollowSections(Cold)",
                    "_CHS-CircularHollowSections-Column(Cold)",
                    "_CHS-CircularHollowSections",
                    "_CHS-CircularHollowSections-Column",
                    "_CircularHollowSections",
                    "_Pipe-Column"
                }
            }
        };

        /***************************************************/
    }
}


