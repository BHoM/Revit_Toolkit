using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using BH.UI.Revit.Adapter;

namespace BH.UI.Revit
{
    public class PullEvent : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            lock (RevitListener.Listener.m_packageLock)
            {
                try
                {
                    //Get instance of listener
                    RevitListener listener = RevitListener.Listener;

                    //Get the revit adapter
                    RevitInternalAdapter adapter = listener.GetAdapter(app.ActiveUIDocument.Document);

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

        public string GetName()
        {
            return "Pull event";
        }
    }
}
