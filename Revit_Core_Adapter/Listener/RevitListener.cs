/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BH.Adapter.Revit;
using BH.Adapter.Socket;
using BH.oM.Adapter;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Data.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace BH.Revit.Adapter.Core
{
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class RevitListener : IExternalApplication
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        public List<IObject> LatestPackage { get; set; }

        public IRequest LatestRequest { get; set; }

        public PullType LatestPullType { get; set; }

        public PushType LatestPushType { get; set; }

        public ActionConfig LatestConfig { get; set; } = null;

        public string LatestTag { get; set; }

        public UIControlledApplication UIControlledApplication = null;

        public KeyValuePair<string, object> LatestKeyValuePair { get; set; }

        public static RevitListener Listener { get; private set; } = null;

        public static RevitSettings AdapterSettings { get; set; } = null;


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public RevitListenerAdapter GetAdapter(Document document)
        {
            RevitListenerAdapter adapter;

            if (!m_Adapters.TryGetValue(document, out adapter))
            {
                adapter = new RevitListenerAdapter(UIControlledApplication, document);
                m_Adapters[document] = adapter;
            }

            if (AdapterSettings != null)
                adapter.RevitSettings = AdapterSettings;

            return adapter;
        }

        /***************************************************/

        public void ReturnData(IEnumerable<object> objs)
        {

            oM.Adapters.Socket.DataPackage package = new oM.Adapters.Socket.DataPackage
            {
                Data = objs.ToList(),
                Events = BH.Engine.Reflection.Query.CurrentEvents(),
                Tag = ""
            };

            m_LinkOut.SendData(package);
        }

        /***************************************************/

        public void SetPorts(int inputPort, int outputPort)
        {
            if (m_LinkIn != null)
                m_LinkIn.DataObservers -= M_linkIn_DataObservers;

            m_LinkIn = new SocketLink_Tcp(inputPort);
            m_LinkIn.DataObservers += M_linkIn_DataObservers;

            m_LinkOut = new SocketLink_Tcp(outputPort);

            ShowActiveButtonSet();
        }

        /***************************************************/

        public void Deactivate()
        {
            if (m_LinkIn != null)
                m_LinkIn.DataObservers -= M_linkIn_DataObservers;

            m_LinkIn = null;
            m_LinkOut = null;

            ShowInactiveButtonSet();
        }

        /***************************************************/

        public int InPort()
        {
            return m_LinkIn == null ? -1 : m_LinkIn.Port;
        }

        /***************************************************/

        public int OutPort()
        {
            return m_LinkOut == null ? -1 : m_LinkOut.Port;
        }


        /***************************************************/
        /****           Revit addin methods             ****/
        /***************************************************/

        public Result OnShutdown(UIControlledApplication application)
        {
            if (m_LinkIn != null)
                m_LinkIn.DataObservers -= M_linkIn_DataObservers;

            return Result.Succeeded;
        }

        /***************************************************/

        public Result OnStartup(UIControlledApplication uIControlledApplication)
        {
            // Load sensitive Dynamo assemblies prior to BHoM to prevent the former from crashing.
            BH.Revit.Engine.Core.Compute.LoadSensitiveDynamoAssemblies();

            UIControlledApplication = uIControlledApplication;

            //Make sure all BHoM assemblies and methods are loaded
            BH.Engine.Reflection.Compute.LoadAllAssemblies();

            //Add buttons to manage the adapter
            AddAdapterButtons(uIControlledApplication);

            //Add info buttons
            AddInfoButtons(uIControlledApplication);

            //Define static instance of the listener
            Listener = this;

            //Define push and pull events
            PushEvent pushEvent = new PushEvent();
            m_PushEvent = ExternalEvent.Create(pushEvent);

            PullEvent pullEvent = new PullEvent();
            m_PullEvent = ExternalEvent.Create(pullEvent);

            RemoveEvent removeEvent = new RemoveEvent();
            m_RemoveEvent = ExternalEvent.Create(removeEvent);

            //empty list for package holding
            LatestPackage = new List<IObject>();

            uIControlledApplication.ViewActivated += Application_ViewActivated;

            // Make sure the button visibility is OK after startup (Revit likes to mess it if the buttons are pinned to quick access toolbar).
            uIControlledApplication.Idling += InitializeButtonVisibility;

            return Result.Succeeded;
        }


        /***************************************************/
        /****           Data observer method            ****/
        /***************************************************/

        private void M_linkIn_DataObservers(oM.Adapters.Socket.DataPackage package)
        {
            BH.Engine.Reflection.Compute.ClearCurrentEvents();

            ExternalEvent eve = null;

            lock (m_PackageLock)
            {
                if (!CheckPackage(package))
                    return;

                PackageType packageType = (PackageType)package.Data[0];

                switch (packageType)
                {
                    case PackageType.ConnectionCheck:
                        {
                            ReturnData(new List<object>());
                            return;
                        }
                    case PackageType.Push:
                        {
                            //Set the event to raise
                            eve = m_PushEvent;

                            //Clear the previous package list
                            LatestPackage = new List<IObject>();

                            //Add all objects to the list
                            foreach (object obj in package.Data[1] as IEnumerable<object>)
                            {
                                if (obj is IObject)
                                    LatestPackage.Add(obj as IObject);
                            }

                            // Get push type
                            if (!(package.Data[2] is PushType) && (!(package.Data[2] is int) || (int)package.Data[2] < 0 || (int)package.Data[2] >= Enum.GetNames(typeof(PushType)).Length))
                                return;

                            LatestPushType = (PushType)package.Data[2];
                            LatestConfig = package.Data[3] as ActionConfig;
                            AdapterSettings = package.Data[4] as RevitSettings;
                            break;
                        }
                    case PackageType.Pull:
                        {
                            //Set the event to raise
                            eve = m_PullEvent;

                            //Clear the previous package list
                            LatestRequest = package.Data[1] as IRequest;

                            // Get pull type
                            if (!(package.Data[2] is PullType) && (!(package.Data[2] is int) || (int)package.Data[2] < 0 || (int)package.Data[2] >= Enum.GetNames(typeof(PullType)).Length))
                                return;

                            LatestPullType = (PullType)package.Data[2];
                            LatestConfig = package.Data[3] as ActionConfig;
                            AdapterSettings = package.Data[4] as RevitSettings;
                            break;
                        }
                    case PackageType.Remove:
                        {
                            //Set the event to raise
                            eve = m_RemoveEvent;

                            //Clear the previous package list
                            LatestRequest = package.Data[1] as IRequest;
                            LatestConfig = package.Data[2] as ActionConfig;
                            AdapterSettings = package.Data[3] as RevitSettings;
                            break;
                        }
                    case PackageType.UpdateTags:
                        {
                            //Set the event to raise
                            eve = m_UpdateTagsEvent;

                            var tuple = package.Data[1] as Tuple<IRequest, string, object>;
                            LatestRequest = tuple.Item1;
                            LatestKeyValuePair = new KeyValuePair<string, object>(tuple.Item2, tuple.Item3);
                            break;
                        }
                    default:
                        ReturnData(new List<string> { "Unrecognized package type" });
                        return;
                }

                LatestTag = package.Tag;
            }

            eve.Raise();
        }

        /***************************************************/

        private bool CheckPackage(oM.Adapters.Socket.DataPackage package)
        {
            if (package.Data.Count == 0)
            {
                ReturnData(new List<string> { "Can't handle empty package." });
                return false;
            }

            PackageType packageType;

            try
            {
                packageType = (PackageType)package.Data[0];
            }
            catch
            {
                ReturnData(new List<string> { "Unrecognized package type." });
                return false;
            }

            int packageSize = 0;
            switch (packageType)
            {
                case PackageType.ConnectionCheck:
                    packageSize = 1;
                    break;
                case PackageType.Pull:
                case PackageType.Push:
                    packageSize = 5;
                    break;
                case PackageType.Remove:
                    packageSize = 4;
                    break;
            }

            if (package.Data.Count != packageSize)
            {
                ReturnData(new List<string> { "Invalid package size for given package type." });
                return false;
            }

            return true;
        }


        /***************************************************/
        /****             Private methods               ****/
        /***************************************************/

        private void ShowActiveButtonSet()
        {
            m_ActivateButton.Visible = false;
            m_UpdatePortsButton.Visible = true;
            m_DeactivateButton.Visible = true;
        }

        /***************************************************/

        private void ShowInactiveButtonSet()
        {
            m_ActivateButton.Visible = true;
            m_UpdatePortsButton.Visible = false;
            m_DeactivateButton.Visible = false;
        }

        /***************************************************/

        private void AddAdapterButtons(UIControlledApplication uiControlledApp)
        {
            string tabName = "BHoM";
            string panelName = "Adapter";
            try
            {
                uiControlledApp.CreateRibbonTab(tabName);
            }
            catch
            {

            }

            RibbonPanel panel = uiControlledApp.GetRibbonPanels(tabName).FirstOrDefault(x => x.Name == panelName) as RibbonPanel;
            if (panel == null)
                panel = uiControlledApp.CreateRibbonPanel(tabName, panelName);

            m_ActivateButton = panel.AddItem(new PushButtonData("Activate", "Activate", System.Reflection.Assembly.GetExecutingAssembly().Location, typeof(RevitListener).Namespace + ".Activate")) as PushButton;
            m_ActivateButton.ToolTip = "Activate the BHoM Revit Adapter plugin to allow data exchange with Grasshopper, Dynamo or Excel. The port numbers on both sides must match (default ports: 14128 input and 14129 output).";
            m_ActivateButton.LongDescription = "Checking and updating ports on the Revit side: navigate to the BHoM ribbon tab in Revit, click Activate (if not active) and then Update Ports." +
                        "\nChecking and updating ports on the BHoM side: extract the RevitSettings property of RevitAdapter object, then ConnectionSettings of RevitSettings, then extract/overwrite PushPort and PullPort properties." +
                        "\nPlease see the relevant Adapter/Setup Revit_Toolkit Wiki pages for more information.";
            m_ActivateButton.LargeImage = new BitmapImage(new Uri(Path.Combine(m_ResourceFolder, "AdapterActivate32.png")));
            m_ActivateButton.Image = new BitmapImage(new Uri(Path.Combine(m_ResourceFolder, "AdapterActivate16.png")));
            m_ActivateButton.Enabled = true;
            m_ActivateButton.Visible = true;

            m_UpdatePortsButton = panel.AddItem(new PushButtonData("Update Ports", "Update\nPorts", System.Reflection.Assembly.GetExecutingAssembly().Location, typeof(RevitListener).Namespace + ".SetPorts")) as PushButton;
            m_UpdatePortsButton.ToolTip = "Update ports used by the BHoM Adapter Revit plugin to allow data exchange with Grasshopper, Dynamo or Excel. The port numbers on both sides must match (default ports: 14128 input and 14129 output).";
            m_UpdatePortsButton.LongDescription = "Checking and updating ports on the Revit side: navigate to the BHoM ribbon tab in Revit, click Activate (if not active) and then Update Ports." +
                        "\nChecking and updating ports on the BHoM side: extract the RevitSettings property of RevitAdapter object, then ConnectionSettings of RevitSettings, then extract/overwrite PushPort and PullPort properties." +
                        "\nPlease see the relevant Adapter/Setup Revit_Toolkit Wiki pages for more information.";
            m_UpdatePortsButton.LargeImage = new BitmapImage(new Uri(Path.Combine(m_ResourceFolder, "AdapterUpdate32.png")));
            m_UpdatePortsButton.Image = new BitmapImage(new Uri(Path.Combine(m_ResourceFolder, "AdapterUpdate16.png")));
            m_UpdatePortsButton.Enabled = true;
            m_UpdatePortsButton.Visible = false;

            m_DeactivateButton = panel.AddItem(new PushButtonData("Deactivate", "Deactivate", System.Reflection.Assembly.GetExecutingAssembly().Location, typeof(RevitListener).Namespace + ".Deactivate")) as PushButton;
            m_DeactivateButton.ToolTip = "Deactivate the BHoM Revit Adapter plugin to disallow data exchange with Grasshopper, Dynamo or Excel.";
            m_DeactivateButton.LargeImage = new BitmapImage(new Uri(Path.Combine(m_ResourceFolder, "AdapterDeactivate32.png")));
            m_DeactivateButton.Image = new BitmapImage(new Uri(Path.Combine(m_ResourceFolder, "AdapterDeactivate16.png")));
            m_DeactivateButton.Enabled = true;
            m_DeactivateButton.Visible = false;
            
            PushButton button = panel.AddItem(new PushButtonData("Adapter Wiki", "Adapter Wiki", System.Reflection.Assembly.GetExecutingAssembly().Location, typeof(RevitListener).Namespace + ".RevitToolkitWiki")) as PushButton;
            button.ToolTip = "Visit the BHoM Revit Toolkit (BHoM adapter for Revit) Wiki page.";
            button.LargeImage = new BitmapImage(new Uri(Path.Combine(m_ResourceFolder, "Info32.png")));
            button.Image = new BitmapImage(new Uri(Path.Combine(m_ResourceFolder, "Info16.png")));
            button.Enabled = true;
            button.Visible = true;
        }

        /***************************************************/

        private void AddInfoButtons(UIControlledApplication uiControlledApp)
        {
            string tabName = "BHoM";
            string panelName = "Info";
            try
            {
                uiControlledApp.CreateRibbonTab(tabName);
            }
            catch
            {

            }

            RibbonPanel panel = uiControlledApp.GetRibbonPanels(tabName).FirstOrDefault(x => x.Name == panelName) as RibbonPanel;
            if (panel == null)
                panel = uiControlledApp.CreateRibbonPanel(tabName, panelName);

            PushButtonData bHoMInfoButton = new PushButtonData("BHoM Wiki", "BHoM Wiki", System.Reflection.Assembly.GetExecutingAssembly().Location, typeof(RevitListener).Namespace + ".BHoMWiki");
            PushButtonData bHoMWebsiteButton = new PushButtonData("bhom.xyz", "bhom.xyz", System.Reflection.Assembly.GetExecutingAssembly().Location, typeof(RevitListener).Namespace + ".BHoMWebsite");

            List<RibbonButton> infoButtons = panel.AddStackedItems(bHoMInfoButton, bHoMWebsiteButton).Cast<RibbonButton>().ToList();
            infoButtons[0].ToolTip = "Visit the BHoM Wiki page.";
            infoButtons[0].Image = new BitmapImage(new Uri(Path.Combine(m_ResourceFolder, "Info16.png")));
            infoButtons[0].Enabled = true;
            infoButtons[0].Visible = true;
            infoButtons[1].ToolTip = "Visit the BHoM website.";
            infoButtons[1].Image = new BitmapImage(new Uri(Path.Combine(m_ResourceFolder, "BHoMWebsite16.png")));
            infoButtons[1].Enabled = true;
            infoButtons[1].Visible = true;
        }

        /***************************************************/

        private void Application_ViewActivated(object sender, Autodesk.Revit.UI.Events.ViewActivatedEventArgs e)
        {
            RevitAdapter.InternalAdapter = GetAdapter(e.Document);
        }

        /***************************************************/

        private void InitializeButtonVisibility(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {
            ShowInactiveButtonSet();
            UIControlledApplication.Idling -= InitializeButtonVisibility;
        }


        /***************************************************/
        /****             Private fields                ****/
        /***************************************************/

        private SocketLink_Tcp m_LinkIn;
        private SocketLink_Tcp m_LinkOut;
        private ExternalEvent m_PushEvent;
        private ExternalEvent m_PullEvent;
        private ExternalEvent m_RemoveEvent;
        private ExternalEvent m_UpdateTagsEvent;
        private Dictionary<Document, RevitListenerAdapter> m_Adapters = new Dictionary<Document, RevitListenerAdapter>();
        private PushButton m_ActivateButton;
        private PushButton m_UpdatePortsButton;
        private PushButton m_DeactivateButton;
        private const string m_ResourceFolder = "C:\\ProgramData\\BHoM\\Resources\\Revit";

        public object m_PackageLock = new object();

        /***************************************************/
    }
}
