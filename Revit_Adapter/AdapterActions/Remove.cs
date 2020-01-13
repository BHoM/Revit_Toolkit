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

using BH.oM.Adapter;
using BH.Adapter.Socket;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Reflection.Debugging;
using BH.oM.Adapters.Revit.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;
using BH.oM.Adapters.Revit;

namespace BH.Adapter.Revit
{
    public partial class RevitAdapter : BHoMAdapter
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public override int Remove(IRequest request, ActionConfig actionConfig = null)
        {
            //Initialize Revit config
            RevitConfig revitConfig = actionConfig as RevitConfig;

            //If internal adapter is loaded call it directly
            if (InternalAdapter != null)
            {
                InternalAdapter.RevitSettings = RevitSettings;
                return InternalAdapter.Remove(request, revitConfig);
            }

            throw new NotImplementedException();
        }

        /***************************************************/
    }
}
