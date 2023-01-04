/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
    public class PushEvent : IExternalEventHandler
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

                    // Do not attempt to push if no document is open.
                    if (app.ActiveUIDocument == null || app.ActiveUIDocument.Document == null)
                    {
                        BH.Engine.Base.Compute.RecordError("The adaper has successfully connected to Revit, but open document could not be found. Push aborted.");
                        objs = new List<object>();
                    }
                    else
                    {
                        //Get the revit adapter
                        RevitListenerAdapter adapter = listener.GetAdapter(app.ActiveUIDocument.Document);

                        //Push the data
                        objs = adapter.Push(listener.LatestPackage, listener.LatestTag, listener.LatestPushType, listener.LatestConfig);
                    }
                    
                    //Clear the lastest package list
                    listener.LatestPackage.Clear();
                    listener.LatestPushType = oM.Adapter.PushType.AdapterDefault;
                    listener.LatestConfig = null;
                    listener.LatestTag = "";

                    //Return the pushed objects
                    listener.ReturnData(objs);
                }
                catch (Exception e)
                {
                    RevitListener.Listener.ReturnData(new List<string> { "Failed to push. Exception from the adapter: " + e.Message });
                }
            }
        }

        /***************************************************/

        public string GetName()
        {
            return "Push event";
        }

        /***************************************************/
    }
}


