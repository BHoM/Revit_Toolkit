using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using BH.oM.Base;
using BH.UI.Revit.Adapter;

namespace BH.UI.Revit
{
    public class UpdatePropertyEvent : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            lock (RevitListener.Listener.m_packageLock)
            {
                try
                {
                    //Clear the event log
                    Engine.Reflection.Compute.ClearCurrentEvents();

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

        public string GetName()
        {
            return "Update Property event";
        }
    }
}
