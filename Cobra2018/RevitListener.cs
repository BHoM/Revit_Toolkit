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
using BH.UI.Revit.Adapter;
using BH.Adapter.Revit;

namespace BH.UI.Revit
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

        public RevitInternalAdapter GetAdapter(Document doc)
        {
            RevitInternalAdapter adapter;

            if (!m_adapters.TryGetValue(doc, out adapter))
            {
                adapter = new RevitInternalAdapter(doc);
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
        public void SetPorts(int inputPort, int outputPort)
        {
            //Not sure if needed
            m_linkIn.DataObservers -= M_linkIn_DataObservers;

            m_linkIn = new SocketLink_Tcp(inputPort);
            m_linkIn.DataObservers += M_linkIn_DataObservers;

            m_linkOut = new SocketLink_Tcp(outputPort);
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
            AddRibbonItems(application);

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

            application.ControlledApplication.DocumentOpened += ControlledApplication_DocumentOpened;

            application.ControlledApplication.DocumentCreated += ControlledApplication_DocumentCreated;

            return Result.Succeeded;
        }

        /***************************************************/

        private void ControlledApplication_DocumentCreated(object sender, Autodesk.Revit.DB.Events.DocumentCreatedEventArgs e)
        {
            RevitAdapter.InternalAdapter = GetAdapter(e.Document);
        }

        /***************************************************/

        private void ControlledApplication_DocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs e)
        {
            RevitAdapter.InternalAdapter = GetAdapter(e.Document);
        }

        /***************************************************/

        private void AddRibbonItems(UIControlledApplication application)
        {
            RibbonPanel aRibbonPanel = application.CreateRibbonPanel("RevitListen");

            PushButton aPushButton = aRibbonPanel.AddItem(new PushButtonData("Update Ports", "Update Ports", System.Reflection.Assembly.GetExecutingAssembly().Location, typeof(RevitListener).Namespace + ".UpdatePorts")) as PushButton;
            //aPushButton.LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(TestResource.Test.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            aPushButton.ToolTip = "Update the ports that revit is listening on for information from external softwares sending BHoM information";
        }

        /***************************************************/
        /**** Data observer method                      ****/
        /***************************************************/

        private void M_linkIn_DataObservers(oM.Socket.DataPackage package)
        {
            ExternalEvent eve = null;

            lock (m_packageLock)
            {
                if (package.Data.Count < 1)
                {
                    ReturnData(new List<string> { "Cant handle empty package" });
                    return;
                }

                PackageType packageType = PackageType.ConnectionCheck;

                try
                {
                    packageType = (PackageType)package.Data[0];
                }
                catch
                {
                    ReturnData(new List<string> { "Unrecognized package type" });
                    return;
                }

                switch (packageType)
                {
                    case PackageType.ConnectionCheck:
                        ReturnData(new List<object>());
                        return;
                    case PackageType.Push:
                        if (!CheckPackageSize(package)) return;
                        eve = m_pushEvent;  //Set the event to raise
                        LatestPackage = new List<IObject>();    //Clear the previous package list
                        //Add all objects to the list
                        foreach (object obj in package.Data[1] as IEnumerable<object>)
                        {
                            if (obj is IObject)
                                LatestPackage.Add(obj as IObject);
                        }
                        break;
                    case PackageType.Pull:
                        if (!CheckPackageSize(package)) return;
                        eve = m_pullEvent;
                        LatestQuery = package.Data[1] as IQuery;
                        break;
                    default:
                        ReturnData(new List<string> { "Unrecognized package type" });
                        return;
                }

                LatestTag = package.Tag;
                LatestConfig = package.Data[2] as Dictionary<string,object>;

            }

            eve.Raise();
        }

        /***************************************************/

        private bool CheckPackageSize(oM.Socket.DataPackage package)
        {
            if (package.Data.Count < 3)
            {
                ReturnData(new List<string> { "Invalid Package" });
                return false;
            }
            return true;
        }

        /***************************************************/
        /**** Private feilds                            ****/
        /***************************************************/

        private SocketLink_Tcp m_linkIn;
        private SocketLink_Tcp m_linkOut;
        private ExternalEvent m_pushEvent;
        private ExternalEvent m_pullEvent;
        private Dictionary<Document, RevitInternalAdapter> m_adapters = new Dictionary<Document, RevitInternalAdapter>();


        public object m_packageLock = new object();
        /***************************************************/
    }
}
