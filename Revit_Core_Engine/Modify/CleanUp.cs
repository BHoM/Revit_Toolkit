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
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Cleans up given solids, which includes:" +
                     "\n- removal of the ones with extremely small volume" +
                     "\n- Boolean union of the remaining ones" +
                     "\n- splitting disjoint volumes")]
        [Input("solids", "Solids to clean up.")]
        [Output("clean", "Cleaned up solids.")]
        public static List<Solid> CleanUp(this List<Solid> solids)
        {
            solids = solids.Where(x => x.Volume > 1e-6).ToList();

            if (solids.Count == 0)
                return new List<Solid>();
            if (solids.Count == 1)
                return SolidUtils.SplitVolumes(solids[0]).ToList();
            else
            {
                Solid result = solids[0];
                foreach (Solid solid in solids.Skip(1))
                {
                    try
                    {
                        result = BooleanOperationsUtils.ExecuteBooleanOperation(result, solid, BooleanOperationsType.Union);
                    }
                    catch (Exception ex)
                    {
                        BH.Engine.Base.Compute.RecordWarning($"Boolean union operation failed during CleanUp. Error: {ex.Message}. The operation will continue with the remaining solids.");
                    }
                }

                return SolidUtils.SplitVolumes(result).ToList();
            }
        }

        /***************************************************/
    }
}
