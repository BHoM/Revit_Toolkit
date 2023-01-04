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
using Autodesk.Revit.DB.Analysis;
using BH.oM.Base.Attributes;
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

        [Description("Remove the items correspondent to the energy analysis elements that overlap with physical ones from the given collection of Revit ElementIds.")]
        [Input("ids", "Collection of Revit ElementIds to remove the energy analysis elements that overlap with physical ones from.")]
        [Input("document", "Revit Document correspondent to the processed collection of ElementIds.")]
        [Output("ids", "Input collection of Revit ElementIds without the items correspondent to the energy analysis elements that overlap with physical ones.")]
        public static List<ElementId> RemoveDuplicateAnalyticalElements(this IEnumerable<ElementId> ids, Document document)
        {
            List<ElementId> result = new List<ElementId>(ids);
            List<Element> elements = ids.Select(x => document.GetElement(x)).ToList();

            for (int i = result.Count - 1; i >= 0; i--)
            {
                Element element = elements[i];
                
                if (element is EnergyAnalysisOpening && elements.Any(x => x.UniqueId == ((EnergyAnalysisOpening)element).CADObjectUniqueId))
                {
                    result.RemoveAt(i);
                    continue;
                }
                else if (element is EnergyAnalysisSpace && elements.Any(x => x.UniqueId == ((EnergyAnalysisSpace)element).CADObjectUniqueId))
                {
                    result.RemoveAt(i);
                    continue;
                }
                else if (element is EnergyAnalysisSurface && elements.Any(x => x.UniqueId == ((EnergyAnalysisSurface)element).CADObjectUniqueId))
                {
                    result.RemoveAt(i);
                    continue;
                }
            }

            return result;
        }

        /***************************************************/
    }
}



