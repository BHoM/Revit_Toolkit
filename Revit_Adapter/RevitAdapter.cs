/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Reflection.Debugging;
using BH.oM.Adapters.Revit.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;
using BH.oM.Adapter;

namespace BH.Adapter.Revit
{
    public partial class RevitAdapter : BHoMAdapter
    {   
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        public static IInternalRevitAdapter InternalAdapter { get; set; } = null;
        public RevitSettings RevitSettings { get; set; } = new RevitSettings();


        /***************************************************/
        /****                Constructors               ****/
        /***************************************************/

        [Description("Adapter to connect to an open Revit file. Connection will be made to the active document (based on active view). Make sure to only have ONE Revit Window open, or connection will not work as intended.")]
        [Input("revitSettings", "Connect RevitSettings to control what is being connected")]
        [Input("active", "Establish connection with Revit by setting to 'True'")]
        [Output("adapter", "Adapter to Revit")]
        public RevitAdapter(RevitSettings revitSettings = null, bool active = false)
        {
            if (!active)
                return;

            if (revitSettings != null)
                RevitSettings = revitSettings;

            if (RevitSettings.ConnectionSettings == null)
                RevitSettings.ConnectionSettings = new ConnectionSettings();

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

        private void M_linkOut_DataObservers(oM.Socket.DataPackage package)
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
            m_LinkIn.SendData(new List<object> { PackageType.ConnectionCheck });

            bool returned = m_WaitEvent.WaitOne(TimeSpan.FromSeconds(5));
            RaiseEvents();
            m_WaitEvent.Reset();

            if (!returned)
                Engine.Reflection.Compute.RecordError("Failed to connect to Revit");

            return returned;
        }

        /***************************************************/

        private void RaiseEvents()
        {
            if (m_ReturnEvents == null)
                return;

            Engine.Reflection.Query.CurrentEvents().AddRange(m_ReturnEvents);
            Engine.Reflection.Query.AllEvents().AddRange(m_ReturnEvents);

            m_ReturnEvents = new List<Event>();
        }

        /***************************************************/
    }
}
