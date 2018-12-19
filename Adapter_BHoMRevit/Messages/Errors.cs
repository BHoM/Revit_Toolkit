﻿using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Base;
using System;

namespace BH.UI.Revit.Adapter
{
    public partial class BHoMRevitAdapter
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

        private static void ObjectNotMovedWarning(IBHoMObject iBHoMObject)
        {
            BH.Engine.Reflection.Compute.RecordWarning(string.Format("Revit object could not be moved. Revit element id: {0}, BHoM object Guid: {1}", Query.ElementId( iBHoMObject).ToString(),  iBHoMObject.BHoM_Guid));
        }

        /***************************************************/

        private static void DeletePinnedElementError(Element Element)
        {
            BH.Engine.Reflection.Compute.RecordError(string.Format("Could not delete pinned element. Element Id: {0}", Element.Id));
        }

        /***************************************************/

        private static void ConvertBeforePushError(IBHoMObject iBHoMObject, Type TypeToConvert)
        {
            BH.Engine.Reflection.Compute.RecordError(string.Format("{0} has to be converted to {1} before pushing. BHoM object Guid: {2}", iBHoMObject.GetType().Name, TypeToConvert.Name, iBHoMObject.BHoM_Guid));
        }

        /***************************************************/

        
    }
}
