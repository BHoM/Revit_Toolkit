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
using Autodesk.Revit.UI;
using BH.oM.Adapter;
using BH.oM.Adapters.Revit;
using BH.oM.Data.Requests;
using System.Collections.Generic;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitUIAdapter
    {
        /***************************************************/
        /****      Revit side of Revit_Adapter Pull     ****/
        /***************************************************/

        public override IEnumerable<object> Pull(IRequest request, PullType pullType = PullType.AdapterDefault, ActionConfig actionConfig = null)
        {
            // Check the document
            UIDocument uiDocument = this.UIDocument;
            Document document = this.Document;
            if (document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be removed because Revit Document is null (possibly there is no open documents in Revit).");
                return new List<object>();
            }

            // Set config
            RevitPullConfig pullConfig = actionConfig as RevitPullConfig;
            if (pullConfig == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning("Revit Pull Config has not been specified. Default Revit Pull Config is used.");
                pullConfig = new RevitPullConfig();
            }

            // Read the objects based on the request
            return Read(request, pullConfig);
        }

        /***************************************************/
    }
}
