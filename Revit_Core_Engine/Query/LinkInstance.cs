/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static RevitLinkInstance LinkInstance(this Document linkDocument)
        {
            Document mainDoc = linkDocument.HostDocument();
            if (linkDocument == mainDoc)
            {
                BH.Engine.Reflection.Compute.RecordWarning($"The document under path {linkDocument.PathName} is a host document.");
                return null;
            }

            RevitLinkInstance linkInstance = new FilteredElementCollector(mainDoc).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().FirstOrDefault(x => x.GetLinkDocument().PathName == linkDocument.PathName);
            if (linkInstance == null)
                BH.Engine.Reflection.Compute.RecordError($"The link pointing to path {linkDocument.PathName} could not be found in active Revit document.");

            return linkInstance;
        }

        /***************************************************/
    }
}

