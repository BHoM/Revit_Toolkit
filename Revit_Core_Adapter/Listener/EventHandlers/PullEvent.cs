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

using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace BH.Revit.Adapter.Core
{
    public class PullEvent : IExternalEventHandler
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public void Execute(UIApplication app)
        {
            lock (RevitListener.Listener.m_PackageLock)
            {
                try
                {
                    //Clear the event log
                    BH.Engine.Base.Compute.ClearCurrentEvents();

                    //Get instance of listener
                    RevitListener listener = RevitListener.Listener;

                    IEnumerable<object> objs;

                    // Do not attempt to pull if no document is open.
                    if (app.ActiveUIDocument == null || app.ActiveUIDocument.Document == null)
                    {
                        BH.Engine.Base.Compute.RecordError("The adaper has successfully connected to Revit, but open document could not be found. Pull aborted.");
                        objs = new List<object>();
                    }
                    else
                    {
                        //Get the revit adapter
                        RevitListenerAdapter adapter = listener.GetAdapter(app.ActiveUIDocument.Document);

                        //Pull the data
                        objs = adapter.Pull(listener.LatestRequest, listener.LatestPullType, listener.LatestConfig);
                    }

                    //Clear the previous data
                    listener.LatestPullType = oM.Adapter.PullType.AdapterDefault;
                    listener.LatestConfig = null;
                    listener.LatestTag = "";

                    //Return the pulled objects
                    listener.ReturnData(objs);
                }
                catch(Exception e)
                {
                    RevitListener.Listener.ReturnData(new List<string> { "Failed to pull. Exception from the adapter: " + e.Message });
                }
            }
        }

        /***************************************************/

        public string GetName()
        {
            return "Pull event";
        }

        /***************************************************/
    }
}

