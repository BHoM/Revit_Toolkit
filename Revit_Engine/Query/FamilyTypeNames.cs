using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using BH.oM.Adapters.Revit.Generic;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets Revit Family Type names in FamilyLibrary for given Category and Family Name.")]
        [Input("familyLibrary", "FamilyLibrary")]
        [Input("categoryName", "Category Name")]
        [Input("familyName", "Family Name")]
        [Output("FamilyTypeNames")]
        public static List<string> FamilyTypeNames(this FamilyLibrary familyLibrary, string categoryName = null, string familyName = null)
        {
            if (familyLibrary == null)
                return null;

            List<string> aFamilyTypeNames = new List<string>();

            List<Dictionary<string, Dictionary<string, string>>> aDictionaryList_Category = new List<Dictionary<string, Dictionary<string, string>>>();

            if (string.IsNullOrEmpty(categoryName))
                aDictionaryList_Category = familyLibrary.Dictionary.Values.ToList();
            else
            {
                Dictionary<string, Dictionary<string, string>> aDictionary_Category = null;
                if (familyLibrary.Dictionary.TryGetValue(categoryName, out aDictionary_Category))
                    aDictionaryList_Category.Add(aDictionary_Category);
            }

            foreach (Dictionary<string, Dictionary<string, string>> aDictionary_Category in aDictionaryList_Category)
            {
                if (string.IsNullOrEmpty(familyName))
                {
                    aFamilyTypeNames.AddRange(aDictionary_Category.Keys);
                }
                else
                {
                    foreach (KeyValuePair<string, Dictionary<string, string>> aKeyValuePair_Category in aDictionary_Category)
                        if (aKeyValuePair_Category.Value.ContainsKey(familyName))
                            aFamilyTypeNames.Add(aKeyValuePair_Category.Key);
                }
            }

            return aFamilyTypeNames;
        }

        /***************************************************/

        [Description("Gets all Revit Family Type names in RevitFilePreview")]
        [Input("familyLibrary", "FamilyLibrary")]
        [Input("categoryName", "Category Name")]
        [Input("familyName", "Family Name")]
        [Output("FamilyTypeNames")]
        public static List<string> FamilyTypeNames(this RevitFilePreview RevitFilePreview)
        {
            if (RevitFilePreview == null || RevitFilePreview.XDocument == null)
                return null;

            XDocument aXDocument = RevitFilePreview.XDocument;

            if (aXDocument != null && aXDocument.Root != null && aXDocument.Root.Attributes() != null)
            {
                List<XAttribute> aAttributeList = aXDocument.Root.Attributes().ToList();
                XAttribute aFirstAttribute = aAttributeList.Find(x => x.Name.LocalName == "A");
                XAttribute aSecondAttribute = aAttributeList.Find(x => x.Name.LocalName == "xmlns");

                if (aFirstAttribute != null && aSecondAttribute != null)
                {
                    XName aName = XName.Get("family", aFirstAttribute.Value);
                    XElement aXElement = aXDocument.Root.Element(aName);
                    if (aXElement != null)
                    {
                        aName = XName.Get("part", aFirstAttribute.Value);
                        List<XElement> aXElementList = aXElement.Elements(aName).ToList();
                        if (aXElementList != null)
                        {
                            List<string> aResult = new List<string>();
                            aName = XName.Get("title", aSecondAttribute.Value);
                            foreach (XElement XElement_Type in aXElementList)
                            {
                                XElement aXElement_Title = XElement_Type.Element(aName);
                                if (aXElement_Title != null && !string.IsNullOrEmpty(aXElement_Title.Value))
                                    aResult.Add(aXElement_Title.Value);
                            }
                            return aResult;
                        }
                    }
                }
            }
            return null;
        }

        /***************************************************/
    }
}


