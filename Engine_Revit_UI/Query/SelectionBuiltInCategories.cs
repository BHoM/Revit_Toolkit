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

using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static List<BuiltInCategory> SelectionBuiltInCategories(this UIDocument uIDocument)
        {
            if (uIDocument == null)
                return null;

            Selection aSelection = uIDocument.Selection;
            if (aSelection == null)
                return null;

            Document aDocument = uIDocument.Document;
            if (aDocument == null)
                return null;

            List<BuiltInCategory> aResult = new List<BuiltInCategory>();
            foreach(ElementId aElementId in aSelection.GetElementIds())
            {
                Element aElement = aDocument.GetElement(aElementId);
                if(aElement != null && aElement.Category != null)
                {
                    BuiltInCategory aBuiltInCategory = (BuiltInCategory)aElement.Category.Id.IntegerValue;
                    if (!aResult.Contains(aBuiltInCategory))
                        aResult.Add(aBuiltInCategory);
                }
            }
            return aResult;
        }

        /***************************************************/
    }
}