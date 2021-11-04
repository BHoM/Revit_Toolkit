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
        [Input("insulationType", "Type of insulation to be added on the host object.")]
        [Input("insulationThickness", "Thickness of insulation to be added on the host object in millimeters.")]
        [Output("insulation", "Insulation that has been added on the host.")]
        public static InsulationLiningBase AddInsulation(this Element host, ElementType insulationType, double insulationThickness)
        {
            Document doc = host.Document;
            //Check inputs
            if (insulationType == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Insulation could not be created. Insulation type not found.");
                return null;
            }

            if (host == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Insulation could not be created. Host element not found.");
                return null;
            }

            InsulationLiningBase ins;
            ICollection<ElementId> insIds = InsulationLiningBase.GetInsulationIds(doc, host.Id);
            insulationThickness = insulationThickness / 304.8;
            BuiltInCategory hostCategory = (BuiltInCategory)host.Category.Id.IntegerValue;
            if (insIds.Count == 0)
            {
                if (host is Pipe || hostCategory is BuiltInCategory.OST_PipeFitting || hostCategory is BuiltInCategory.OST_PipeAccessory || hostCategory is BuiltInCategory.OST_FlexPipeCurves)
                {
                    ins = PipeInsulation.Create(doc, host.Id, insulationType.Id, insulationThickness);
                }
                else if (host is Duct || hostCategory is BuiltInCategory.OST_DuctFitting || hostCategory is BuiltInCategory.OST_DuctAccessory || hostCategory is BuiltInCategory.OST_FlexDuctCurves)
                {
                    ins = DuctInsulation.Create(doc, host.Id, insulationType.Id, insulationThickness);
                }
                else
                {
                    BH.Engine.Reflection.Compute.RecordError("Insulation could not be created. Host element type does not support insulation adding functionality.");
                    return null;
                }
            }
            else if (insIds.Count == 1)
            {
                ElementId insId = insIds.ToList()[0];
                ins = doc.GetElement(insId) as InsulationLiningBase;
                if (ins.GetTypeId() != insulationType.Id)
                {
                    ins.ChangeTypeId(insulationType.Id);
                }
                if (ins.Thickness != insulationThickness)
                {
                    ins.Thickness = insulationThickness;
                }
            }
            else
            {
                BH.Engine.Reflection.Compute.RecordError("Insulation could not be changed. Not supported type of host insulation.");
                return null;
            }

            return ins;
        }
    }

    /***************************************************/
}