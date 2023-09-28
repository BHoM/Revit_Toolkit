using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        public static List<Connector> ConnectorsOfType(this MEPCurve mepCurve, ConnectorType connectorType)
        {
            List<Connector> result = new List<Connector>();
            foreach (Connector connector in mepCurve.ConnectorManager.Connectors)
            {
                if (connector.ConnectorType == connectorType)
                    result.Add(connector);
            }

            return result;
        }
    }
}
