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
                    CobraAdapter adapter = listener.GetAdapter(app.ActiveUIDocument.Document);

                    //Push the data
                    int result = adapter.UpdateProperty(listener.LatestQuery as BH.oM.DataManipulation.Queries.FilterQuery, listener.LatestKeyValuePair.Key, listener.LatestKeyValuePair.Value, listener.LatestConfig);

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