using BH.oM.Revit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace BH.Engine.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

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
