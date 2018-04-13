using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using BH.Adapter;
using System.Collections;
using BH.oM.Base;
using BH.oM.DataManipulation.Queries;
using BH.Adapter.Socket;

namespace BH.Adapter.Revit
{
    public class RevitAdapter : BHoMAdapter
    {

        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        public static BHoMAdapter InternalAdapter { get; set; } = null;

        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        public RevitAdapter(int pushPort = 14128, int pullPort = 14129, double maxMinutesToWait = 10)
        {
            m_linkIn = new SocketLink_Tcp(pushPort);
            m_linkOut = new SocketLink_Tcp(pullPort);
            m_linkOut.DataObservers += M_linkOut_DataObservers;

            m_waitEvent = new ManualResetEvent(false);
            m_returnPackage = new List<object>();

            m_waitTime = maxMinutesToWait;
        }

        private void M_linkOut_DataObservers(oM.Socket.DataPackage package)
        {
            //Store the return data
            m_returnPackage = package.Data;

            //Set the wait event to allow methods to continue
            m_waitEvent.Set();
        }

        /***************************************************/
        /**** Public methods                            ****/
        /***************************************************/

        public override List<IObject> Push(IEnumerable<IObject> objects, string tag = "", Dictionary<string, object> config = null)
        {
            //If internal adapter is loaded call it directly
            if (InternalAdapter != null)
                return InternalAdapter.Push(objects, tag, config);

            //Reset the wait event
            m_waitEvent.Reset();

            if (!CheckConnection())
                return new List<IObject>();

            config = config == null ? new Dictionary<string, object>() : null;

            //Send data through the socket link
            m_linkIn.SendData(new List<object>() { PackageType.Push, objects.ToList(), config }, tag);

            //Wait until the return message has been recieved
            m_waitEvent.WaitOne(TimeSpan.FromMinutes(m_waitTime));

            //Grab the return objects from the latest package
            List<IObject> returnObjs = m_returnPackage.Cast<IObject>().ToList();

            //Clear the return list
            m_returnPackage.Clear();

            //Return the package
            return returnObjs;

        }

        /***************************************************/

        public override IEnumerable<object> Pull(IQuery query, Dictionary<string, object> config = null)
        {
            //If internal adapter is loaded call it directly
            if (InternalAdapter != null)
                return InternalAdapter.Pull(query, config);

            //Reset the wait event
            m_waitEvent.Reset();


            if (!CheckConnection())
                return new List<object>();

            config = config == null ? new Dictionary<string, object>() : null;

            if (!(query is FilterQuery))
                return new List<object>();

            //Send data through the socket link
            m_linkIn.SendData(new List<object>() { PackageType.Pull, query as FilterQuery, config });

            //Wait until the return message has been recieved
            m_waitEvent.WaitOne(TimeSpan.FromMinutes(m_waitTime));

            //Grab the return objects from the latest package
            List<object> returnObjs = new List<object>(m_returnPackage);

            //Clear the return list
            m_returnPackage.Clear();

            //Return the package
            return returnObjs;

        }

        /***************************************************/

        public override int Delete(FilterQuery filter, Dictionary<string, object> config = null)
        {
            //If internal adapter is loaded call it directly
            if (InternalAdapter != null)
                return InternalAdapter.Delete(filter, config);

            throw new NotImplementedException();
        }

        /***************************************************/

        public override int UpdateProperty(FilterQuery filter, string property, object newValue, Dictionary<string, object> config = null)
        {
            //If internal adapter is loaded call it directly
            if (InternalAdapter != null)
                return InternalAdapter.UpdateProperty(filter, property, newValue, config);

            throw new NotImplementedException();
        }

        /***************************************************/

        public override bool Execute(string command, Dictionary<string, object> parameters = null, Dictionary<string, object> config = null)
        {
            //If internal adapter is loaded call it directly
            if (InternalAdapter != null)
                return InternalAdapter.Execute(command, parameters, config);

            throw new NotImplementedException();
        }


        /***************************************************/
        /**** Private  Fields                           ****/
        /***************************************************/

        private SocketLink_Tcp m_linkIn;
        private SocketLink_Tcp m_linkOut;

        private ManualResetEvent m_waitEvent;
        private List<object> m_returnPackage;
        private double m_waitTime;

        /***************************************************/

        private bool CheckConnection()
        {
            m_linkIn.SendData(new List<object> { PackageType.ConnectionCheck });

            bool returned = m_waitEvent.WaitOne(TimeSpan.FromSeconds(5));
            m_waitEvent.Reset();
            return returned;
        }

        /***************************************************/

        protected override bool Create<T>(IEnumerable<T> objects, bool replaceAll = false)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<BH.oM.Base.IBHoMObject> Read(Type type, IList ids)
        {
            throw new NotImplementedException();
        }
    }
}
