using System;
using System.Collections.Generic;
using System.Linq;

using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.oM.Structural.Elements;
using BH.Engine.Revit;
using BH.oM.Base;

using Autodesk.Revit.DB;


namespace BH.UI.Revit.Adapter
{
    public partial class CobraAdapter
    {
        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static void NullObjectCreateError(Type type)
        {
            Engine.Reflection.Compute.RecordError(string.Format("Revit object could not be created. BHoM {0} is null", type.Name));
        }

        private static void NullDocumentCreateError()
        {
            Engine.Reflection.Compute.RecordError(string.Format("Document is null. Objects could not be created"));
        }

        private static void NullObjectsCreateError()
        {
            Engine.Reflection.Compute.RecordError(string.Format("Objects are null."));
        }

        private static void ObjectNotCreatedCreateError(IBHoMObject iBHoMObject)
        {
            Engine.Reflection.Compute.RecordError(string.Format("Revit object could not be created. BHoM object Guid: {0}", iBHoMObject.BHoM_Guid));
        }

        /***************************************************/
    }
}