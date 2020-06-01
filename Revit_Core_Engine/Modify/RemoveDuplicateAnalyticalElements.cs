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
using Autodesk.Revit.DB.Analysis;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static List<ElementId> RemoveDuplicateAnalyticalElements(this IEnumerable<ElementId> ids, Document document)
        {
            List<ElementId> result = new List<ElementId>(ids);
            List<Element> elements = ids.Select(x => document.GetElement(x)).ToList();

            for (int i = result.Count - 1; i >= 0; i--)
            {
                Element element = elements[i];

                //if (element is EnergyAnalysisSpace)
                //{
                //    Element e = document.GetElement(((EnergyAnalysisSpace)element).CADObjectUniqueId);
                //    if (e != null)
                //    {
                //        int id = e.Id.IntegerValue;
                //        bool foo = elements.Any(x => x.Id.IntegerValue == id);

                //        if (foo == true)
                //        {
                //            //
                //        }
                //    }
                //}

                
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
