/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

namespace BH.Adapter.Revit
{
    public partial class RevitAdapter : BHoMAdapter
    {   
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        public static InternalRevitAdapter InternalAdapter { get; set; } = null;
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

            gLinkIn = new SocketLink_Tcp(RevitSettings.ConnectionSettings.PushPort);
            gLinkOut = new SocketLink_Tcp(RevitSettings.ConnectionSettings.PullPort);
            gLinkOut.DataObservers += M_linkOut_DataObservers;

            gWaitEvent = new ManualResetEvent(false);
            gReturnPackage = new List<object>();
            gReturnEvents = new List<Event>();

            gWaitTime = RevitSettings.ConnectionSettings.MaxMinutesToWait;
        } 


        /***************************************************/
        /****              Public methods               ****/
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
            gWaitEvent.Reset();

            if (!CheckConnection())
                return new List<IObject>();

            config = config == null ? new Dictionary<string, object>() : null;

            //Send data through the socket link
            gLinkIn.SendData(new List<object>() { PackageType.Push, objects.ToList(), config, RevitSettings }, tag);

            //Wait until the return message has been recieved
            if (!gWaitEvent.WaitOne(TimeSpan.FromMinutes(gWaitTime)))
                BH.Engine.Reflection.Compute.RecordError("The connection with Revit timed out. If working on a big model, try to increase the max wait time");

            //Grab the return objects from the latest package
            List<IObject> returnObjs = gReturnPackage.Cast<IObject>().ToList();

            //Clear the return list
            gReturnPackage.Clear();

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
            gWaitEvent.Reset();

            if (!CheckConnection())
                return new List<object>();

            config = config == null ? new Dictionary<string, object>() : null;

            if (!(request is FilterRequest))
                return new List<object>();

            //Send data through the socket link
            gLinkIn.SendData(new List<object>() { PackageType.Pull, request as FilterRequest, config, RevitSettings });

            //Wait until the return message has been recieved
            if (!gWaitEvent.WaitOne(TimeSpan.FromMinutes(gWaitTime)))
                Engine.Reflection.Compute.RecordError("The connection with Revit timed out. If working on a big model, try to increase the max wait time");

            //Grab the return objects from the latest package
            List<object> returnObjs = new List<object>(gReturnPackage);

            //Clear the return list
            gReturnPackage.Clear();

            //Raise returned events
            RaiseEvents();

            //Return the package
            return returnObjs;

        }

        /***************************************************/

        public override int Delete(IRequest request, Dictionary<string, object> config = null)
        {
            //If internal adapter is loaded call it directly
            if (InternalAdapter != null)
            {
                InternalAdapter.RevitSettings = RevitSettings;
                return InternalAdapter.Delete(request, config);
            }

            throw new NotImplementedException();
        }

        /***************************************************/

        public bool IsValid()
        {
            if (gLinkIn == null || gLinkOut == null)
                return false;

            return true;
        }


        /***************************************************/
        /****              Private  Fields              ****/
        /***************************************************/

        private SocketLink_Tcp gLinkIn;
        private SocketLink_Tcp gLinkOut;

        private ManualResetEvent gWaitEvent;
        private List<object> gReturnPackage;
        private List<Event> gReturnEvents;
        private double gWaitTime;


        /***************************************************/
        /****             Private  Methods              ****/
        /***************************************************/

        private void M_linkOut_DataObservers(oM.Socket.DataPackage package)
        {
            //Store the return data
            gReturnPackage = package.Data;

            //Store the events
            gReturnEvents = package.Events;

            //Set the wait event to allow methods to continue
            gWaitEvent.Set();
        }

        /***************************************************/

        private bool CheckConnection()
        {
            gLinkIn.SendData(new List<object> { PackageType.ConnectionCheck });

            bool returned = gWaitEvent.WaitOne(TimeSpan.FromSeconds(5));
            RaiseEvents();
            gWaitEvent.Reset();

            if (!returned)
                Engine.Reflection.Compute.RecordError("Failed to connect to Revit");

            return returned;
        }

        /***************************************************/

        private void RaiseEvents()
        {
            if (gReturnEvents == null)
                return;

            Engine.Reflection.Query.CurrentEvents().AddRange(gReturnEvents);
            Engine.Reflection.Query.AllEvents().AddRange(gReturnEvents);

            gReturnEvents = new List<Event>();
        }


        /***************************************************/
        /****             Protected  Methods            ****/
        /***************************************************/

        protected override bool Create<T>(IEnumerable<T> objects)
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
