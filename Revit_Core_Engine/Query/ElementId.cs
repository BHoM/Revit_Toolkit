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
using BH.oM.Base;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        public static ElementId ElementId(this IBHoMObject bHoMObject)
        {
            int id = BH.Engine.Adapters.Revit.Query.ElementId(bHoMObject);
            if (id == -1)
                return null;
            else
                return new ElementId(id);
        }

        /***************************************************/

        public static ElementId ElementId(this string originatingElementDescription)
        {
            if (string.IsNullOrEmpty(originatingElementDescription))
                return null;

            int startIndex = originatingElementDescription.LastIndexOf("[");
            if (startIndex == -1)
                return null;

            int endIndex = originatingElementDescription.IndexOf("]", startIndex);
            if (endIndex == -1)
                return null;

            string elementID = originatingElementDescription.Substring(startIndex + 1, endIndex - startIndex - 1);

            int id;
            if (!int.TryParse(elementID, out id))
                return null;

            return new ElementId(id);
        }

        /***************************************************/
    }
}
