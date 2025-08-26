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
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Adapter.Core
{
    public partial class RevitListenerAdapter
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public Output<List<object>, bool> PullSelection(PullSelection command)
        {
            Output<List<object>, bool> output = new Output<List<object>, bool>() { Item1 = null, Item2 = false };

            UIDocument uidoc = this.UIDocument;
            if (uidoc == null)
            {
                TaskDialog.Show("BHoM","No connected document found.");
                return output;
            }                

            output.Item1 = new List<object>();
            output.Item2 = true;
            
            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();
            if (selectedIds.Count != 0)
                output.Item1.AddRange(Read(this.Document, Transform.Identity, selectedIds.ToList()).Cast<object>());

            return output;
        }

        /***************************************************/
    }
}