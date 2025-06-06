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
using Autodesk.Revit.UI;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapter;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base;
using BH.oM.Data.Requests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Adapter.Core
{
    public partial class RevitListenerAdapter
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public Output<List<object>, bool> Execute(BH.oM.Adapter.Commands.CustomCommand command, ActionConfig actionConfig = null)
        {
            return m_RevitCustomCommands[command.Command](command.Parameters, actionConfig);
        }

        /***************************************************/

        public void RegisterRevitCustomCommands()
        {
            m_RevitCustomCommands.Add("Isolate", Isolate);
            m_RevitCustomCommands.Add("Select", Select);
            m_RevitCustomCommands.Add("DirectPush", DirectPush);
            m_RevitCustomCommands.Add("DirectPull", DirectPull);
        }

        /***************************************************/

        /***************************************************/
        /****              Helper methods               ****/
        /***************************************************/

        private bool TryGetElementId(Dictionary<string, object> input, out List<ElementId> ids)
        {
            ids = null;

            if (!input.TryGetValue("BHoMObjects", out object objects))
            {
                BH.Engine.Base.Compute.RecordError("BHoMObjects is not found, make sure command inputs are valid.");
                return false;
            }

            if (objects == null)
            {
                BH.Engine.Base.Compute.RecordError("Input object list is null.");
                return false;
            }

            List<BHoMObject> bHoMObjects = (objects as IEnumerable<object>).OfType<BHoMObject>().ToList();

            if (bHoMObjects.Count == 0)
            {
                BH.Engine.Base.Compute.RecordError("No valid BHoMObjects provided.");
                return false;
            }

            var elementIds = bHoMObjects
                .Select(b => (b?.Fragments?.FirstOrDefault(f => f is RevitIdentifiers) as RevitIdentifiers)?.ElementId)
                .Where(id => id.HasValue)
                .Select(id => new ElementId(id.Value))
                .ToList();

            if (elementIds.Count == 0)
            {
                BH.Engine.Base.Compute.RecordError("No valid ElementIds found.");
                return false;
            }

            ids = elementIds;
            return true;
        }
    }
}
