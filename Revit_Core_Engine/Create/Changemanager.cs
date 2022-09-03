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

using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using BH.Engine.Geometry;
using BH.Engine.Spatial;
using BH.oM.Geometry;
using BH.oM.Base.Attributes;
using Line = BH.oM.Geometry.Line;
using Point = BH.oM.Geometry.Point;
using BH.oM.Adapters.Revit.Elements;
using System;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static void Start(this ChangeManager changeManager, List<Element> elements)
        {
            changeManager.StartState = changeManager.Initiate(elements);
        }

        public static void Start(this ChangeManager changeManager, Document document)
        {
            List<Element> elements = Query.GetTrackedElements(document);
            changeManager.StartState = changeManager.Initiate(elements);
        }

        public static void End(this ChangeManager changeManager, List<Element> elements)
        {
            changeManager.StartState = changeManager.Initiate(elements);
        }

        public static void Report(this ChangeManager changeManager, Document document)
        {
            changeManager.Report = changeManager.GenerateReport(document);

        }

        public static bool IsChanged(this ChangeManager changeManager)
        {
            return changeManager.Report.Additions.Count > 0
                || changeManager.Report.Deletions.Count > 0 
                || changeManager.Report.Modifications.Count >0;
        }

        public static Report GenerateReport(this ChangeManager changeManager, Document document)
        {
            Report report = new Report();
            List<int> added;
            List<int> deleted;
            List<int> modified;
            List<int> identical;

            Query.GenerateChangeReport(document,changeManager.StartState, changeManager.EndState, out added, out deleted, out modified, out identical);

            report.Additions = added;
            report.Deletions = deleted;
            report.Modifications = modified;
            report.Identical = identical;

            return report;
        }

        private static DocumentSnapshot Initiate(this ChangeManager changeManager, List<Element> elements)
        {
            DocumentSnapshot state = new DocumentSnapshot();

            foreach (Element element in elements)
            {
                //create elementState from GetElementState()
                ElementState elementState = new ElementState() { Id = element.Id.IntegerValue, Properties = Query.GetElementState(element)};
                state.Elements.Add(elementState);
            }

            return state;
        }


        /***************************************************/

    }
}
