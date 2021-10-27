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
using Autodesk.Revit.DB.Structure;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Add or update existing insulation of the duct or pipe with given inuslation type and thickness.")]
        [Input("host", "Host element to add the insulation on.")]
        [Input("insulationType", "Type of insulation to be added on host object.")]
        [Input("insulationThickness", "Thickness of insulation to be added on host object.")]
        [Input("doc", "Document to create a FamilyInstance in.")]
        [Output("insulation", "Insulation that has been added on the host.")]
        public static InsulationLiningBase AddInsulation(this MEPCurve host, FamilySymbol insulationType, double insulationThickness, Document doc)
        {
            //Check inputs
            if (insulationType == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Insulation could not be created. Insulation Type not found.");
                return null;
            }

            if (host == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Insulation could not be created. Host element not found.");
                return null;
            }

            ICollection<ElementId> existingIns = InsulationLiningBase.GetInsulationIds(doc, host.Id);

            InsulationLiningBase ins;
            if (host is Pipe)
            {
                ins = PipeInsulation.Create(doc, host.Id, insulationType.Id, insulationThickness);
            }
            else if (host is Duct)
            {
                ins = DuctInsulation.Create(doc, host.Id, insulationType.Id, insulationThickness);
            }
            else
            {
                BH.Engine.Reflection.Compute.RecordError("Insulation could not be created. Host element type does not support insulation adding functionality.");
                return null;
            }

            return ins;
        }
    }
}