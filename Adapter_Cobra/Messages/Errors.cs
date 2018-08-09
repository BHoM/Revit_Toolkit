using BH.oM.Base;
using System;

namespace BH.UI.Cobra.Adapter
{
    public partial class CobraAdapter
    {
        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static void NullObjectCreateError(Type type)
        {
            BH.Engine.Reflection.Compute.RecordError(string.Format("Revit object could not be created. BHoM {0} is null", type.Name));
        }

        /***************************************************/

        private static void NullDocumentCreateError()
        {
            BH.Engine.Reflection.Compute.RecordError(string.Format("Document is null. Objects could not be created"));
        }

        /***************************************************/

        private static void NullObjectsCreateError()
        {
            BH.Engine.Reflection.Compute.RecordError(string.Format("Objects are null."));
        }

        /***************************************************/

        private static void ObjectNotCreatedCreateError(IBHoMObject iBHoMObject)
        {
            BH.Engine.Reflection.Compute.RecordError(string.Format("Revit object could not be created. BHoM object Guid: {0}", iBHoMObject.BHoM_Guid));
        }

        /***************************************************/
    }
}