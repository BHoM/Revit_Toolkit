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
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.UI.Revit.Engine;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitUIAdapter
    {
        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static Level Create(oM.Architecture.Elements.Level level, Document document, PushSettings pushSettings = null)
        {
            if (level == null)
            {
                NullObjectCreateError(typeof(oM.Architecture.Elements.Level));
                return null;
            }

            pushSettings = pushSettings.DefaultIfNull();

            if (pushSettings.AdapterMode == oM.Adapters.Revit.Enums.AdapterMode.Replace || pushSettings.AdapterMode == oM.Adapters.Revit.Enums.AdapterMode.Delete)
                Delete(level, document);

            if (pushSettings.AdapterMode == oM.Adapters.Revit.Enums.AdapterMode.Delete)
                return null;

            List<Element> aElementList = new FilteredElementCollector(document).OfClass(typeof(Level)).ToList();
            if (aElementList == null || aElementList.Count < 1)
                return level.ToRevit(document, pushSettings) as Level;

            Element aElement = aElementList.Find(x => x.Name == level.Name);
            if (aElement == null)
                return level.ToRevit(document, pushSettings) as Level;

            return null;
        }

        /***************************************************/
    }
}