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

        public static string CategoryName(this RevitFilePreview RevitFilePreview)
        {
            if (RevitFilePreview == null || RevitFilePreview.XDocument == null)
                return null;

            XDocument aXDocument = RevitFilePreview.XDocument;

            if (aXDocument != null && aXDocument.Root != null && aXDocument.Root.Attributes() != null)
            {
                List<XAttribute> aAttributeList = aXDocument.Root.Attributes().ToList();
                XAttribute aAttribute = aAttributeList.Find(x => x.Name.LocalName == "xmlns");
                if (aAttribute != null)
                {
                    XName aName = XName.Get("category", aAttribute.Value);
                    List<XElement> aXElementList = aXDocument.Root.Elements(aName).ToList();
                    if (aXElementList != null)
                    {
                        aName = XName.Get("scheme", aAttribute.Value);
                        foreach (XElement aXElement in aXElementList)
                        {
                            XElement aXElement_Scheme = aXElement.Element(aName);
                            if (aXElement_Scheme != null && aXElement_Scheme.Value == "adsk:revit:grouping")
                            {
                                aName = XName.Get("term", aAttribute.Value);
                                XElement aXElement_Term = aXElement.Element(aName);
                                if (aXElement_Term != null)
                                    return aXElement_Term.Value;
                            }
                        }
                    }
                }
            }
            return null;
        }

        /***************************************************/
    }
}
