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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Dimensional;
using BH.oM.Geometry;
using BH.oM.Facade.Elements;
using BH.Engine.Facade;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static oM.Facade.Elements.Opening FacadePanelAsOpening(this oM.Facade.Elements.Panel panel, string refId = "", Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (panel == null)
                return null;
;
            oM.Facade.Elements.Opening opening = refObjects.GetValue<oM.Facade.Elements.Opening>(refId);
            if (opening != null)
                return opening;

            opening = new oM.Facade.Elements.Opening { Name = panel.Name, Edges = panel.ExternalEdges, Fragments = panel.Fragments, OpeningConstruction = panel.Construction, Tags = panel.Tags, CustomData = panel.CustomData, Type = oM.Facade.Elements.OpeningType.Undefined };
            
            return opening;
        }

        /***************************************************/
    }
}
