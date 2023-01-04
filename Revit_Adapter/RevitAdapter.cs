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

using BH.Adapter.Socket;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using BH.oM.Base.Debugging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace BH.Adapter.Revit
{
    public partial class RevitAdapter : BHoMAdapter
    {   
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        public static IInternalRevitAdapter InternalAdapter { get; set; } = null;
        public RevitSettings RevitSettings { get; set; } = null;


        /***************************************************/
        /****                Constructors               ****/
        /***************************************************/

        [Description("Adapter to connect to an open Revit file. Connection will be made to the active document (based on active view). Make sure to only have ONE Revit Window open, or connection will not work as intended.")]
        [Input("revitSettings", "General settings that are applicable to all actions performed by this adapter, e.g. connection settings, tolerances, parameter mapping settings etc.")]
        [Input("active", "Establish connection with Revit by setting to 'True'")]
        [Output("adapter", "Adapter to Revit")]
        public RevitAdapter(RevitSettings revitSettings = null, bool active = false)
        {
            if (!active)
                return;

            RevitSettings = revitSettings.DefaultIfNull();

            m_LinkIn = new SocketLink_Tcp(RevitSettings.ConnectionSettings.PushPort);
            m_LinkOut = new SocketLink_Tcp(RevitSettings.ConnectionSettings.PullPort);
            m_LinkOut.DataObservers += M_linkOut_DataObservers;

            m_WaitEvent = new ManualResetEvent(false);
            m_ReturnPackage = new List<object>();
            m_ReturnEvents = new List<Event>();

            m_WaitTime = RevitSettings.ConnectionSettings.MaxMinutesToWait;
        } 


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public bool IsValid()
        {
            if (m_LinkIn == null || m_LinkOut == null)
                return false;

            return true;
        }


        /***************************************************/
        /****              Private  Fields              ****/
        /***************************************************/

        private SocketLink_Tcp m_LinkIn;
        private SocketLink_Tcp m_LinkOut;

        private ManualResetEvent m_WaitEvent;
        private List<object> m_ReturnPackage;
        private List<Event> m_ReturnEvents;
        private double m_WaitTime;


        /***************************************************/
        /****             Private  Methods              ****/
        /***************************************************/

        private void M_linkOut_DataObservers(oM.Adapters.Socket.DataPackage package)
        {
            //Store the return data
            m_ReturnPackage = package.Data;

            //Store the events
            m_ReturnEvents = package.Events;

            //Set the wait event to allow methods to continue
            m_WaitEvent.Set();
        }

        /***************************************************/

        private bool CheckConnection()
        {
            bool success = false;
            try
            {
                m_LinkIn.SendData(new List<object> { PackageType.ConnectionCheck });
                success = m_WaitEvent.WaitOne(TimeSpan.FromSeconds(5));
                RaiseEvents();
                m_WaitEvent.Reset();

                if (!success)
                    Engine.Base.Compute.RecordError("Failed to connect to Revit. Please check if the BHoM Revit Adapter plugin is activated on the same ports as this adapter (default ports: 14128 input and 14129 output)." +
                        "\nChecking and updating ports on the Revit side: navigate to the BHoM ribbon tab in Revit, click Activate (if not active) and then Update Ports." +
                        "\nChecking and updating ports on the BHoM side: extract the RevitSettings property of RevitAdapter object, then ConnectionSettings of RevitSettings, then extract/overwrite PushPort and PullPort properties." +
                        "\nPlease see the relevant Adapter/Setup Revit_Toolkit Wiki pages for more information.");
            }
            catch
            {
                Engine.Base.Compute.RecordError("There is an issue with the outgoing connection in the Revit Adapter. Please reset the Revit Adapter and try to re-run.");
            }
            
            return success;
        }

        /***************************************************/

        private void RaiseEvents()
        {
            if (m_ReturnEvents == null)
                return;

            m_ReturnEvents.ForEach(x => Engine.Base.Compute.RecordEvent(x));
            m_ReturnEvents.Clear();
        }

        /***************************************************/
    }
}



