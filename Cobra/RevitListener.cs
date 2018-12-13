using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using BH.Adapter.Revit;
using BH.Adapter.Socket;
using BH.oM.Base;
using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Settings;
using BH.UI.Cobra.Adapter;


namespace BH.UI.Cobra
{
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class RevitListener : IExternalApplication
    {
        /***************************************************/
        /**** Public properties                         ****/
        /***************************************************/

        public List<IObject> LatestPackage { get; set; }

        /***************************************************/

        public IQuery LatestQuery { get; set; }

        /***************************************************/

        public Dictionary<string, object> LatestConfig { get; set; } = null;

        /***************************************************/

        public string LatestTag { get; set; }

        /***************************************************/

        public KeyValuePair<string, object> LatestKeyValuePair { get; set; }

        /***************************************************/

        public static RevitListener Listener { get; private set; } = null;

        /***************************************************/

        public static RevitSettings AdapterSettings { get; set; } = null;


        /***************************************************/
        /**** Public methods                            ****/
        /***************************************************/

        public CobraAdapter GetAdapter(Document document)
        {
            CobraAdapter adapter;

            if (!m_adapters.TryGetValue(document, out adapter))
            {
                adapter = new CobraAdapter(document);
                m_adapters[document] = adapter;
            }

            if (AdapterSettings != null)
                adapter.RevitSettings = AdapterSettings;

            return adapter;
        }

        /***************************************************/

        public void ReturnData(IEnumerable<object> objs)
        {

            oM.Socket.DataPackage package = new oM.Socket.DataPackage
            {
                Data = objs.ToList(),
                Events = BH.Engine.Reflection.Query.CurrentEvents(),
                Tag = ""
            };

            m_linkOut.SendData(package);
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
            //Make sure all BHoM assemblies are loaded
            string versionNumber = application.ControlledApplication.VersionNumber;
            string path = Environment.GetEnvironmentVariable("APPDATA") + @"\Autodesk\Revit\Addins\" + versionNumber +  @"\Cobra";
            BH.Engine.Reflection.Compute.LoadAllAssemblies(path);

            //Add button to set socket ports
            AddRibbonItems(application);

            //Define static instance of the listener
            Listener = this;

            //Define push and pull events
            PushEvent pushEvent = new PushEvent();
            m_pushEvent = ExternalEvent.Create(pushEvent);

            PullEvent pullEvent = new PullEvent();
            m_pullEvent = ExternalEvent.Create(pullEvent);

            UpdatePropertyEvent updatePropEvent = new UpdatePropertyEvent();
            m_updatePropertyEvent = ExternalEvent.Create(updatePropEvent);

            //empty list for package holding
            LatestPackage = new List<IObject>();

            //Socket link for listening and sending data
            m_linkIn = new SocketLink_Tcp(14128);
            m_linkIn.DataObservers += M_linkIn_DataObservers;

            m_linkOut = new SocketLink_Tcp(14129);

            application.ViewActivated += Application_ViewActivated;

            return Result.Succeeded;
        }

        private void Application_ViewActivated(object sender, Autodesk.Revit.UI.Events.ViewActivatedEventArgs e)
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
            BH.Engine.Reflection.Compute.ClearCurrentEvents();

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
                    case PackageType.UpdateProperty:
                        if (!CheckPackageSize(package)) return;
                        eve = m_updatePropertyEvent;
                        var tuple = package.Data[1] as Tuple<FilterQuery, string, object>;
                        LatestQuery = tuple.Item1;
                        LatestKeyValuePair = new KeyValuePair<string, object>(tuple.Item2, tuple.Item3);
                        break;
                    default:
                        ReturnData(new List<string> { "Unrecognized package type" });
                        return;
                }

                LatestTag = package.Tag;
                LatestConfig = package.Data[2] as Dictionary<string,object>;
                AdapterSettings = package.Data[3] as RevitSettings;
            }

            eve.Raise();
        }

        /***************************************************/

        private bool CheckPackageSize(oM.Socket.DataPackage package)
        {
            if (package.Data.Count < 4)
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
        private ExternalEvent m_updatePropertyEvent;
        private Dictionary<Document, CobraAdapter> m_adapters = new Dictionary<Document, CobraAdapter>();
        
        public object m_packageLock = new object();

        /***************************************************/
    }
}