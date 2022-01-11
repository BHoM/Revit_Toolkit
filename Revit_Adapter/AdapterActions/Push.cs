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

using BH.oM.Adapter;
using BH.oM.Adapters.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Adapter.Revit
{
    public partial class RevitAdapter : BHoMAdapter
    {
        /***************************************************/
        /****      BHoM side of Revit_Adapter Push      ****/
        /***************************************************/

        public override List<object> Push(IEnumerable<object> objects, string tag = "", PushType pushType = PushType.AdapterDefault, ActionConfig actionConfig = null)
        {
            //Initialize Revit config
            RevitPushConfig pushConfig = actionConfig as RevitPushConfig;

            //If internal adapter is loaded call it directly
            if (InternalAdapter != null)
            {
                InternalAdapter.RevitSettings = RevitSettings;
                return InternalAdapter.Push(objects, tag, pushType, pushConfig);
            }

            if (!this.IsValid())
            {
                BH.Engine.Base.Compute.RecordError("Revit Adapter is not valid. Please check if it has been set up correctly and activated.");
                return new List<object>();
            }

            //Reset the wait event
            m_WaitEvent.Reset();

            if (!CheckConnection())
                return new List<object>();

            //Send data through the socket link
            m_LinkIn.SendData(new List<object>() { PackageType.Push, objects.ToList(), pushType, pushConfig, RevitSettings }, tag);

            //Wait until the return message has been recieved
            if (!m_WaitEvent.WaitOne(TimeSpan.FromMinutes(m_WaitTime)))
                BH.Engine.Base.Compute.RecordError("The connection with Revit timed out. If working on a big model, try to increase the max wait time");

            //Grab the return objects from the latest package
            List<object> returnObjs = m_ReturnPackage.ToList();

            //Clear the return list
            m_ReturnPackage.Clear();

            RaiseEvents();

            //Check if the return package contains error message
            if (returnObjs.Count == 1 && returnObjs[0] is string)
            {
                Engine.Base.Compute.RecordError(returnObjs[0] as string);
                return new List<object>();
            }

            //Return the package
            return returnObjs;
        }

        /***************************************************/
    }
}


