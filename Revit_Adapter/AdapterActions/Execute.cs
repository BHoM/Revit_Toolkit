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

using BH.oM.Adapter;
using BH.oM.Adapters.Revit;
using BH.oM.Base;
using System;
using System.Collections.Generic;

namespace BH.Adapter.Revit
{
    public partial class RevitAdapter : BHoMAdapter
    {
        /***************************************************/
        /****    BHoM side of Revit_Adapter Execute     ****/
        /***************************************************/

        /***************************************************/
        /**** IAdapter Interface                        ****/
        /***************************************************/

        public override Output<List<object>, bool> Execute(IExecuteCommand command, ActionConfig actionConfig = null)
        {
            Output<List<object>, bool> output = new Output<List<object>, bool>() { Item1 = null, Item2 = false };
            if (command == null)
            {
                BH.Engine.Base.Compute.RecordError("Command could not be executed because provided command is null.");
                return output;
            }

            //Initialize Revit config
            RevitExecutionConfig executionConfig = actionConfig as RevitExecutionConfig;

            //If internal adapter is loaded call it directly
            if (InternalAdapter != null)
            {
                InternalAdapter.RevitSettings = RevitSettings;
                return InternalAdapter.Execute(command, executionConfig);
            }

            if (!this.IsValid())
            {
                BH.Engine.Base.Compute.RecordError("Revit Adapter is not valid. Please check if it has been set up correctly and activated.");
                return output;
            }

            //Reset the wait event
            m_WaitEvent.Reset();

            if (!CheckConnection())
                return output;

            //Send data through the socket link
            m_LinkIn.SendData(new List<object>() { PackageType.Execute, command, executionConfig, RevitSettings });

            //Wait until the return message has been received
            if (!m_WaitEvent.WaitOne(TimeSpan.FromMinutes(m_WaitTime)))
                Engine.Base.Compute.RecordError("The connection with Revit timed out. If working on a big model, try to increase the max wait time");

            //Grab the return objects from the latest package
            List<object> returnObjs = new List<object>(m_ReturnPackage);

            //Raise returned events
            RaiseEvents();

            if (returnObjs.Count == 1)
            {
                if (returnObjs[0] is string Output<List<object>, bool> result)
					return result;
                else if (returnObjs[0] is string errorMessage)
				{
					Engine.Base.Compute.RecordError(errorMessage);
					return output;
				}
            }

			Engine.Base.Compute.RecordError("Execution failed due to an internal error.");
            return output;
        }

        /***************************************************/
    }
}





