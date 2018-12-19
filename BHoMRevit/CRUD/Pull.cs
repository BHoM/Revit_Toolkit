using Autodesk.Revit.UI;
using BH.UI.Revit.Adapter;
using System;
using System.Collections.Generic;

namespace BH.UI.Revit
{
    public class PullEvent : IExternalEventHandler
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
                    BHoMRevitAdapter adapter = listener.GetAdapter(app.ActiveUIDocument.Document);

                    //Push the data
                    IEnumerable<object> objs = adapter.Pull(listener.LatestQuery, listener.LatestConfig);

                    //Clear the previous data
                    listener.LatestConfig = null;
                    listener.LatestTag = "";

                    //Return the pushed objects
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