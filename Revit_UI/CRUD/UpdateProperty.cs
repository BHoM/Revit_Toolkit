/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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
using BH.UI.Revit.Adapter;
using System;
using System.Collections.Generic;

namespace BH.UI.Revit
{
    public class UpdatePropertyEvent : IExternalEventHandler
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public void Execute(UIApplication app)
        {
            lock (RevitListener.Listener.m_packageLock)
            {
                try
                {
                    //Clear the event log
                    BH.Engine.Reflection.Compute.ClearCurrentEvents();

                    //Get instance of listener
                    RevitListener listener = RevitListener.Listener;

                    //Get the revit adapter
                    RevitUIAdapter adapter = listener.GetAdapter(app.ActiveUIDocument.Document);

                    //Push the data
                    int result = adapter.UpdateProperty(listener.LatestRequest as BH.oM.Data.Requests.FilterRequest, listener.LatestKeyValuePair.Key, listener.LatestKeyValuePair.Value, listener.LatestConfig);

                    //Clear the lastest package list
                    listener.LatestKeyValuePair = new KeyValuePair<string, object>();
                    listener.LatestConfig = null;
                    listener.LatestTag = "";

                    //Return the pushed objects
                    listener.ReturnData(new List<object> { result });
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
            return "Update Property event";
        }

        /***************************************************/
    }
}