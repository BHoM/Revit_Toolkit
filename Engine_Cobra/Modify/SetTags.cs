using Autodesk.Revit.DB;

using BH.oM.Base;

namespace BH.UI.Cobra.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static Element SetTags(this Element element, IBHoMObject bHoMObject, string tagsParameterName)
        {
            if (bHoMObject == null || element == null || string.IsNullOrEmpty(tagsParameterName))
                return null;

            Parameter aParameter = element.LookupParameter(tagsParameterName);
            if (aParameter == null || aParameter.IsReadOnly || aParameter.StorageType != StorageType.String)
                return null;

            string aValue_New = null;
            if(bHoMObject.Tags != null)
                aValue_New = string.Join("\n", bHoMObject.Tags);

            string aValue_Old = aParameter.AsString();

            if (aValue_New != aValue_Old)
                aParameter.Set(aValue_New);


            return element;
        }

        /***************************************************/

        public static IBHoMObject SetTags(this IBHoMObject bHoMObject, Element element, string tagsParameterName)
        {
            if (bHoMObject == null || element == null || string.IsNullOrEmpty(tagsParameterName))
                return null;

            Parameter aParameter = element.LookupParameter(tagsParameterName);
            if (aParameter == null || aParameter.StorageType != StorageType.String)
                return null;

            string aValueString = aParameter.AsString();

            IBHoMObject aIBHoMObject = bHoMObject.GetShallowClone();

            if (string.IsNullOrEmpty(aValueString) && (bHoMObject.Tags == null || bHoMObject.Tags.Count == 0))
                return aIBHoMObject;

            if (aIBHoMObject.Tags == null)
                aIBHoMObject.Tags = new System.Collections.Generic.HashSet<string>();

            string[] aValues = aValueString.Split('\n');

            foreach (string aValue in aValues)
                aIBHoMObject.Tags.Add(aValue);


            return aIBHoMObject;
        }

        /***************************************************/
    }
}