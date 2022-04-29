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

using Autodesk.Revit.DB;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Get the insulation of element. Return null if element has no insulation.")]
        [Input("host", "Host element to get the insulation from.")]
        [Output("insulation", "Insulation of the host element.")]
        public static InsulationLiningBase Insulation(this Element host)
        {
            if (host == null)
            {
                BH.Engine.Base.Compute.RecordError("Insulation could not be returned because the type of host insulation is not supported.");
                return null;
            }

            Document doc = host.Document;
            ICollection<ElementId> insIds = InsulationLiningBase.GetInsulationIds(doc, host.Id);

            if (insIds.Count == 0)
            {
                return null;
            }
            else if (insIds.Count == 1)
            {
                ElementId insId = insIds.ToList()[0];
                return doc.GetElement(insId) as InsulationLiningBase;
            }
            else
            {
                BH.Engine.Base.Compute.RecordError("Insulation could not be returned because the type of host insulation is not supported.");
                return null;
            }
        }
    }

    /***************************************************/
}
