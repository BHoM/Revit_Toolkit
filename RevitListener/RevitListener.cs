using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using BH.Adapter.Socket;
using BH.oM.Base;
using BH.oM.DataManipulation.Queries;

namespace BH.Adapter.Revit
{
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class RevitListener : IExternalApplication
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/
        public List<IObject> LatestPackage { get; set; }

        /***************************************************/

        public IQuery LatestQuery { get; set; }

        /***************************************************/

        public Dictionary<string, object> LatestConfig { get; set; } = null;

        /***************************************************/
        public string LatestTag { get; set; }

        /***************************************************/
        public static RevitListener Listener { get; private set; } = null;


        /***************************************************/
        /**** Public methods                            ****/
        /***************************************************/

        public RevitAdapter GetAdapter(Document doc)
        {
            RevitAdapter adapter;

            if (!m_adapters.TryGetValue(doc, out adapter))
            {
                adapter = new RevitAdapter(doc);
                m_adapters[doc] = adapter;
            }
            return adapter;
        }

        /***************************************************/
        public void ReturnData(IEnumerable<object> objs)
        {
            m_linkOut.SendData(objs.ToList());
        }

        /***************************************************/
        /**** Revit addin methods                       ****/
        /***************************************************/

        public Result OnShutdown(UIControlledApplication application)
        {
            //Not sure if needed
            m_linkIn.DataObservers -= M_linkIn_DataObservers;

            return Result.Succeeded;
        }

        /***************************************************/

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel aRibbonPanel = application.CreateRibbonPanel("RevitListen");

            //Define static instance of the listener
            Listener = this;

            //Define push and pull events
            PushEvent pushEvent = new PushEvent();
            m_pushEvent = ExternalEvent.Create(pushEvent);

            PullEvent pullEvent = new PullEvent();
            m_pullEvent = ExternalEvent.Create(pullEvent);

            //empty list for package holding
            LatestPackage = new List<IObject>();

            //Socket link for listening and sending data
            m_linkIn = new SocketLink_Tcp(14128);
            m_linkIn.DataObservers += M_linkIn_DataObservers;

            m_linkOut = new SocketLink_Tcp(14129);



            return Result.Succeeded;
        }

        /***************************************************/

        private void M_linkIn_DataObservers(oM.Socket.DataPackage package)
        {
            ExternalEvent eve = null;

            lock (m_packageLock)
            {
                if (package.Data.Count < 3)
                    return;

                BH.Adapter.RevitLink.PackageType packageType = (BH.Adapter.RevitLink.PackageType)package.Data[0];

                switch (packageType)
                {
                    case RevitLink.PackageType.Push:
                        eve = m_pushEvent;  //Set the event to raise
                        LatestPackage = new List<IObject>();    //Clear the previous package list
                        //Add all objects to the list
                        foreach (object obj in package.Data[1] as IEnumerable<object>)
                        {
                            if (obj is IObject)
                                LatestPackage.Add(obj as IObject);
                        }
                        break;
                    case RevitLink.PackageType.Pull:
                        eve = m_pullEvent;
                        LatestQuery = package.Data[1] as IQuery;
                        break;
                    default:
                        return;
                }

                LatestTag = package.Tag;
                LatestConfig = package.Data[2] as Dictionary<string,object>;

            }

            eve.Raise();
        }

        /***************************************************/
        /**** Private feilds                            ****/
        /***************************************************/

        private SocketLink_Tcp m_linkIn;
        private SocketLink_Tcp m_linkOut;
        private ExternalEvent m_pushEvent;
        private ExternalEvent m_pullEvent;
        private Dictionary<Document, RevitAdapter> m_adapters = new Dictionary<Document, RevitAdapter>();

        public object m_packageLock = new object();
        /***************************************************/
    }
}
