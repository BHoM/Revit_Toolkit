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

using BH.oM.Adapters.Revit;
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

namespace BH.Adapter.Revit
{
    public partial class RevitAdapter : BHoMAdapter
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public override IEnumerable<object> Pull(IRequest request, PullType pullType = PullType.AdapterDefault, ActionConfig actionConfig = null)
        {
            //Initialize Revit config
            RevitConfig revitConfig = actionConfig as RevitConfig;

            //If internal adapter is loaded call it directly
            if (InternalAdapter != null)
            {
                InternalAdapter.RevitSettings = RevitSettings;
                return InternalAdapter.Pull(request, pullType, revitConfig);
            }

            //Reset the wait event
            m_WaitEvent.Reset();

            if (!CheckConnection())
                return new List<object>();

            //Send data through the socket link
            m_LinkIn.SendData(new List<object>() { PackageType.Pull, request, revitConfig, RevitSettings });

            //Wait until the return message has been recieved
            if (!m_WaitEvent.WaitOne(TimeSpan.FromMinutes(m_WaitTime)))
                Engine.Reflection.Compute.RecordError("The connection with Revit timed out. If working on a big model, try to increase the max wait time");

            //Grab the return objects from the latest package
            List<object> returnObjs = new List<object>(m_ReturnPackage);

            //Clear the return list
            m_ReturnPackage.Clear();

            //Raise returned events
            RaiseEvents();

            //Return the package
            return returnObjs;

        }

        /***************************************************/
    }
}
