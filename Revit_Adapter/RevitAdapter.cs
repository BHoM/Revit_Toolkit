/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

namespace BH.Adapter.Revit
{
    public partial class RevitAdapter : BHoMAdapter
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        public static InternalRevitAdapter InternalAdapter { get; set; } = null;
        public RevitSettings RevitSettings { get; set; } = new RevitSettings();


        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        public RevitAdapter(RevitSettings revitSettings = null, bool active = false)
        {
            if (!active)
                return;

            if (revitSettings != null)
                RevitSettings = revitSettings;

            if (RevitSettings.ConnectionSettings == null)
                RevitSettings.ConnectionSettings = new ConnectionSettings();

            m_linkIn = new SocketLink_Tcp(RevitSettings.ConnectionSettings.PushPort);
            m_linkOut = new SocketLink_Tcp(RevitSettings.ConnectionSettings.PullPort);
            m_linkOut.DataObservers += M_linkOut_DataObservers;

            m_waitEvent = new ManualResetEvent(false);
            m_returnPackage = new List<object>();
            m_returnEvents = new List<Event>();

            m_waitTime = RevitSettings.ConnectionSettings.MaxMinutesToWait;
        } 


        /***************************************************/
        /**** Public methods                            ****/
        /***************************************************/

        public override List<IObject> Push(IEnumerable<IObject> objects, string tag = "", Dictionary<string, object> config = null)
        {
            //If internal adapter is loaded call it directly
            if (InternalAdapter != null)
            {
                InternalAdapter.RevitSettings = RevitSettings;
                return InternalAdapter.Push(objects, tag, config);
            }
                

            //Reset the wait event
            m_waitEvent.Reset();

            if (!CheckConnection())
                return new List<IObject>();

            config = config == null ? new Dictionary<string, object>() : null;

            //Send data through the socket link
            m_linkIn.SendData(new List<object>() { PackageType.Push, objects.ToList(), config, RevitSettings }, tag);

            //Wait until the return message has been recieved
            if (!m_waitEvent.WaitOne(TimeSpan.FromMinutes(m_waitTime)))
                BH.Engine.Reflection.Compute.RecordError("The connection with Revit timed out. If working on a big model, try to increase the max wait time");

            //Grab the return objects from the latest package
            List<IObject> returnObjs = m_returnPackage.Cast<IObject>().ToList();

            //Clear the return list
            m_returnPackage.Clear();

            RaiseEvents();

            //Return the package
            return returnObjs;

        }

        /***************************************************/

        public override IEnumerable<object> Pull(IRequest request, Dictionary<string, object> config = null)
        {
            //If internal adapter is loaded call it directly
            if (InternalAdapter != null)
            {
                InternalAdapter.RevitSettings = RevitSettings;
                return InternalAdapter.Pull(request, config);
            }  

            //Reset the wait event
            m_waitEvent.Reset();


            if (!CheckConnection())
                return new List<object>();

            config = config == null ? new Dictionary<string, object>() : null;

            if (!(request is FilterRequest))
                return new List<object>();

            //Send data through the socket link
            m_linkIn.SendData(new List<object>() { PackageType.Pull, request as FilterRequest, config, RevitSettings });

            //Wait until the return message has been recieved
            if (!m_waitEvent.WaitOne(TimeSpan.FromMinutes(m_waitTime)))
                Engine.Reflection.Compute.RecordError("The connection with Revit timed out. If working on a big model, try to increase the max wait time");

            //Grab the return objects from the latest package
            List<object> returnObjs = new List<object>(m_returnPackage);

            //Clear the return list
            m_returnPackage.Clear();

            //Raise returned events
            RaiseEvents();

            //Return the package
            return returnObjs;

        }

        /***************************************************/

        public override int Delete(FilterRequest filter, Dictionary<string, object> config = null)
        {
            //If internal adapter is loaded call it directly
            if (InternalAdapter != null)
            {
                InternalAdapter.RevitSettings = RevitSettings;
                return InternalAdapter.Delete(filter, config);
            }

            throw new NotImplementedException();
        }

        /***************************************************/

        public override int UpdateProperty(FilterRequest filter, string property, object newValue, Dictionary<string, object> config = null)
        {
            //If internal adapter is loaded call it directly
            if (InternalAdapter != null)
            {
                InternalAdapter.RevitSettings = RevitSettings;
                return InternalAdapter.UpdateProperty(filter, property, newValue, config);
            }

            //Reset the wait event
            m_waitEvent.Reset();


            if (!CheckConnection())
                return 0;

            config = config == null ? new Dictionary<string, object>() : null;

            //Send data through the socket link
            m_linkIn.SendData(new List<object>() { PackageType.UpdateProperty, new Tuple<FilterRequest,string,object>(filter,property, newValue), config, RevitSettings });

            //Wait until the return message has been recieved
            if (!m_waitEvent.WaitOne(TimeSpan.FromMinutes(m_waitTime)))
                Engine.Reflection.Compute.RecordError("The connection with Revit timed out. If working on a big model, try to increase the max wait time");

            int returnValue = 0;
            //Grab the return objects from the latest package
            if (m_returnPackage.Count > 0 && m_returnPackage[0] is int)
                returnValue = (int)m_returnPackage[0];

            //Clear the return list
            m_returnPackage.Clear();

            //Raise returned events
            RaiseEvents();

            //Return the package
            return returnValue;
        }

        public bool IsValid()
        {
            if (m_linkIn == null || m_linkOut == null)
                return false;

            return true;
        }


        /***************************************************/
        /**** Private  Fields                           ****/
        /***************************************************/

        private SocketLink_Tcp m_linkIn;
        private SocketLink_Tcp m_linkOut;

        private ManualResetEvent m_waitEvent;
        private List<object> m_returnPackage;
        private List<Event> m_returnEvents;
        private double m_waitTime;


        /***************************************************/
        /**** Private  Methods                          ****/
        /***************************************************/

        private void M_linkOut_DataObservers(oM.Socket.DataPackage package)
        {
            //Store the return data
            m_returnPackage = package.Data;

            //Store the events
            m_returnEvents = package.Events;

            //Set the wait event to allow methods to continue
            m_waitEvent.Set();
        }

        /***************************************************/

        private bool CheckConnection()
        {
            m_linkIn.SendData(new List<object> { PackageType.ConnectionCheck });

            bool returned = m_waitEvent.WaitOne(TimeSpan.FromSeconds(5));
            RaiseEvents();
            m_waitEvent.Reset();

            if (!returned)
                Engine.Reflection.Compute.RecordError("Failed to connect to Revit");

            return returned;
        }

        /***************************************************/

        private void RaiseEvents()
        {
            if (m_returnEvents == null)
                return;

            Engine.Reflection.Query.CurrentEvents().AddRange(m_returnEvents);
            Engine.Reflection.Query.AllEvents().AddRange(m_returnEvents);

            m_returnEvents = new List<Event>();
        }


        /***************************************************/
        /**** Protected  Methods                        ****/
        /***************************************************/

        protected override bool Create<T>(IEnumerable<T> objects, bool replaceAll = false)
        {
            throw new NotImplementedException();
        }

        /***************************************************/

        protected override IEnumerable<IBHoMObject> Read(Type type, IList ids)
        {
            throw new NotImplementedException();
        }

        /***************************************************/
    }
}
