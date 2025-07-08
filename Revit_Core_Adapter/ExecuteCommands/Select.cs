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
using BH.oM.Adapter.Commands;
using BH.oM.Base;
using BH.Revit.Engine.Core;
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

        public Output<List<object>, bool> Select(Select command)
        {
            Output<List<object>, bool> output = new Output<List<object>, bool>() { Item1 = null, Item2 = false };

            if (command?.Targets == null)
            {
                BH.Engine.Base.Compute.RecordError("No selected objects found.");
                return output;
            }

            var elementIds = command.Targets.TryGetElementIds();

            if (elementIds == null || elementIds.Count == 0)
            {
                BH.Engine.Base.Compute.RecordError("ElementIds is invalid or empty.");
                return output;
            }

            UIDocument uidoc = this.UIDocument;

            if (uidoc == null)
            {
                BH.Engine.Base.Compute.RecordError("Revit UI is not available).");
                return output;
            }

            try
            {
                if (command.ShowObjects) 
                    uidoc.ShowElements(elementIds);
                uidoc.Selection.SetElementIds(elementIds);
            }
            catch (Exception ex)
            {
                BH.Engine.Base.Compute.RecordError($"Some elements cannot be selected or do not have views: {ex.Message}");
            }

            output.Item1 = elementIds.Cast<object>().ToList();
            output.Item2 = true;
            return output;
        }

        /***************************************************/
    }
}